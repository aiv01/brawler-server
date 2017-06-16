using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using BrawlerServer.Utilities;
using BrawlerServer.Gameplay;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BrawlerServer.Server
{
    public struct ReliablePacket
    {
        public Packet Packet { get; private set; }
        public uint Time { get; private set; }

        public ReliablePacket(Packet packet)
        {
            this.Packet = packet;
            this.Time = packet.Server.Time;
        }
    }

    public class AsyncRequest
    {
        public enum RequestMethod
        {
            GET,
            POST
        }

        public enum RequestType
        {
            Authentication,
            ServerInfoToServices,
            AddMatch,
            AddPlayerToMatch,
            EndMatch
        }

        private static readonly Dictionary<RequestType, Type> Jsons = new Dictionary<RequestType, Type> {
            { RequestType.Authentication, typeof(Json.AuthPlayerPost) },
            { RequestType.ServerInfoToServices, typeof(Json.InfoToServicesPost) }
            //{ RequestType.AddMatch, typeof(Json.AddMatchPost) },
            //{ RequestType.AddMatch, typeof(Json.AddPlayerToMatchPost) },
            //{ RequestType.AddMatch, typeof(Json.EndMatchPost) },
        };

        public Task<HttpResponseMessage> Response { get; private set; }
        public Task<string> ResponseString { get; private set; }
        public RequestType requestType;
        public bool requestedString;

        public IPEndPoint RemoteEp { get; private set; }

        public AsyncRequest(RequestMethod requestMethod, string uri, HttpClient HttpClient, IPEndPoint remoteEp, RequestType requestType, FormUrlEncodedContent content)
        {
            requestedString = false;
            this.requestType = requestType;
            RemoteEp = remoteEp;

            if (requestMethod == RequestMethod.GET)
            {
                Response = HttpClient.GetAsync(uri);
            }
            else if (requestMethod == RequestMethod.POST)
            {
                Response = HttpClient.PostAsync(uri, content);
            }
            Logs.Log($"[LongAgo] Sent {requestMethod} request to {uri} from {remoteEp} with request type {requestType}.");
        }

        public void ReadString()
        {
            this.ResponseString = Response.Result.Content.ReadAsStringAsync();
            requestedString = true;
        }

        public void CallHandler(object JsonData, Server server)
        {
            if (requestType == RequestType.Authentication)
            {
                AuthHandler.HandleResponse(JsonData as Json.AuthPlayerPost, RemoteEp, server);
            }
            if (requestType == RequestType.ServerInfoToServices)
            {
                server.HandleInfoToServicesResponse(JsonData as Json.InfoToServicesPost);
            }
        }

        public static Type GetJsonType(RequestType requestType)
        {
            return Jsons[requestType];
        }
    }

    public class Server
    {

        #region AttributesAndProperties
        public delegate void ServerTickHandler(Server server);
        public delegate void ServerPacketReceiveHandler(Server server, Packet packet);
        public event ServerTickHandler ServerTick;
        public event ServerPacketReceiveHandler ServerPacketReceive;

        public IPEndPoint BindEp { get; private set; }
        private readonly Socket Socket;
        private List<Socket> socketsToRead;
        private List<Socket> socketsToWrite;
        private readonly List<Packet> packetsToSend;
        private readonly byte[] recvBuffer;
        private readonly MemoryStream recvStream;
        private readonly BinaryReader recvReader;
        private readonly BinaryWriter recvWriter;

        private readonly int packetsPerLoop;

        private Dictionary<uint, ReliablePacket> ReliablePackets;
        public uint MaxAckResponseTime { get; private set; }
        public uint MaxAckSendTime { get; private set; }

        private readonly Dictionary<IPEndPoint, Client> clients;

        public bool IsRunning { get; set; }
        public uint Time { get; private set; }
        // does NOT count looptime
        public uint DeltaTime { get; private set; }

        public uint MaxIdleTimeout { get; private set; }

        public List<Arena> arenas { get; private set; }

        public List<Room> rooms { get; private set; }

        public ServerMode mode { get; private set; }
        #endregion

        public enum ServerMode
        {
            Lobby,
            Battle
        }

        public Server(IPEndPoint bindEp, int bufferSize = 1024, int packetsPerLoop = 256)
        {
            packetsToSend = new List<Packet>();
            clients = new Dictionary<IPEndPoint, Client>();
            HttpClient = new HttpClient();

            this.packetsPerLoop = packetsPerLoop;
            this.BindEp = bindEp;

            recvBuffer = new byte[bufferSize];
            recvStream = new MemoryStream(recvBuffer);
            recvReader = new BinaryReader(recvStream);
            recvWriter = new BinaryWriter(recvStream);

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { Blocking = false };

            socketsToRead = new List<Socket>();
            socketsToRead.Add(Socket);

            this.ReliablePackets = new Dictionary<uint, ReliablePacket>();
            this.MaxAckResponseTime = 5000;
            this.MaxAckSendTime = 30000;

            this.MaxIdleTimeout = 30000;

            arenas = new List<Arena>();
            Arena arena = new Arena();
            arena.AddSpawnPoint(0, 0.45f, 0);
            arena.AddSpawnPoint(3, 0.45f, -3);
            arena.AddSpawnPoint(3, 0.45f, 3);
            arena.AddSpawnPoint(-3, 0.45f, -3);
            arena.AddSpawnPoint(3, 0.45f, 3);
            arenas.Add(arena);

            rooms = new List<Room>();
            Room room = new Room(8);
            rooms.Add(room);

            mode = ServerMode.Lobby;
        }

        public void Bind()
        {
            if (!Socket.IsBound)
            {
                Socket.Bind(BindEp);
                BindEp = (IPEndPoint)Socket.LocalEndPoint;
                SendServerInfo();
            }
        }

        public void MainLoop(int loopTime = 120)
        {
            IsRunning = true;

            var msLoopTime = (int)(loopTime * 1000f);

            EndPoint remoteEp = new IPEndPoint(0, 0);

            var watch = Stopwatch.StartNew();
            while (IsRunning)
            {
                if (watch.ElapsedMilliseconds > UInt32.MaxValue)
                    watch.Restart();
                Time = (uint)watch.ElapsedMilliseconds;

                socketsToRead = new List<Socket>();
                socketsToRead.Add(Socket);
                socketsToWrite = new List<Socket>();
                socketsToWrite.Add(Socket);
                Socket.Select(socketsToRead, socketsToWrite, null, 1 / (loopTime * 1000 * 1000));

                // first receive packets
                var packetIndex = 0;
                socketsToRead.AddRange(socketsToWrite);
                foreach (Socket socket in socketsToRead)
                {
                    while (packetIndex < packetsPerLoop && socket.Available > 0)
                    {
                        var size = socket.ReceiveFrom(recvBuffer, ref remoteEp);
                        Logs.Log($"[{Time}] Received message from '{remoteEp}', size: {size}.");
                        Packet packet = null;
                        try
                        {
                            packet = new Packet(this, size, recvBuffer, (IPEndPoint)remoteEp, recvStream, recvReader,
                                recvWriter);
                            if (clients.ContainsKey((IPEndPoint)remoteEp))
                            {
                                clients[(IPEndPoint)remoteEp].TimeLastPacketSent = this.Time;
                            }
                            packet.ParseHeaderFromData();
                            Logs.Log($"[{Time}] Successfully parsed {packet}.");
                        }
                        catch (Exception e)
                        {
                            Logs.LogWarning($"[{Time}] Unable to parse packet from '{remoteEp}', with size of {size}:");
                            Logs.LogWarning($"[{Time}] {e.Message}");
                            continue;
                        }
                        finally
                        {
                            ServerPacketReceive?.Invoke(this, packet);
                        }

                        packetIndex++;
                    }
                    //Check if client didn't send any packet in MaxIdleTimeout seconds
                    CheckClientsTimeout();
                    //Check if reliable packet has passed the time check limit
                    List<uint> reliablePacketsToRemove = new List<uint>();
                    foreach (KeyValuePair<uint, ReliablePacket> reliablePacket in ReliablePackets)
                    {
                        if (this.Time > reliablePacket.Value.Packet.Time + this.MaxAckSendTime)
                        {
                            reliablePacketsToRemove.Add(reliablePacket.Key);
                        }

                        if (this.Time > reliablePacket.Value.Time + this.MaxAckResponseTime)
                        {
                            this.SendPacket(reliablePacket.Value.Packet);
                            reliablePacketsToRemove.Add(reliablePacket.Key);
                        }
                    }
                    foreach (uint reliablePacketId in reliablePacketsToRemove)
                    {
                        ReliablePackets.Remove(reliablePacketId);
                    }
                    // then send packets (do we need to send only a fixed number?)
                    foreach (var packet in packetsToSend)
                    {
                        if (packet.Broadcast)
                        {
                            foreach (var pair in clients)
                            {
                                //TODO Fails tests for Move, Taunt and Dodge ... Loses the last float value
                                //if (packet.RemoteEp == pair.Key)
                                //  continue;
                                socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, pair.Key);
                                Logs.Log($"[{Time}] Sent broadcast {packet} to {pair.Value}, clients for this broadcast: {clients.Count}.");
                            }
                        }
                        else
                        {
                            socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, packet.RemoteEp);
                            Logs.Log($"[{Time}] Sent {packet} to {packet.RemoteEp}");
                        }
                        if (packet.IsReliable)
                        {
                            AddReliablePacket(packet);
                        }
                    }
                    packetsToSend.Clear();

                    //remove Queued Clients
                    if (QueuedClientsToRemove.Count > 0)
                        RemoveClients();

                    CheckForResponse();
                    CheckForResponseString();
                }

                ServerTick?.Invoke(this);

                DeltaTime = (uint)watch.ElapsedMilliseconds - Time;
            }
            foreach (Socket socket in socketsToRead)
            {
                socket.Close();
            }
        }

        #region ServerInfoToServices
        public void SendServerInfo()
        {
            Dictionary<string, string> requestValues = new Dictionary<string, string>
            {
                { "port", this.BindEp.Port.ToString() }
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(requestValues);
            AddAsyncRequest(AsyncRequest.RequestMethod.POST, "http://taiga.aiv01.it/servers/register/", this.BindEp, AsyncRequest.RequestType.ServerInfoToServices, content);
        }

        public void HandleInfoToServicesResponse(Json.InfoToServicesPost JsonData)
        {
            if (!JsonData.server_register)
                Logs.LogWarning($"[{Time}] Server info to Service failed: {JsonData.info}");
            else
            {
                Logs.Log($"[{Time}] Server infos successfully sent");
            }
        }
        #endregion

        #region PacketManagement
        public void SendPacket(Packet packet)
        {
            packetsToSend.Add(packet);
        }

        public void SendPacketInstantly(Packet packet)
        {
            foreach (Socket socket in socketsToRead)
            {
                if (packet.Broadcast)
                {
                    foreach (var pair in clients)
                    {
                        if (packet.RemoteEp == pair.Key)
                            continue;
                        socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, pair.Key);
                        Logs.Log($"[{Time}] Instantly sent broadcast {packet} to {pair.Value}.");
                    }
                }
                else
                {
                    socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, packet.RemoteEp);
                    Logs.Log($"[{Time}] Instantly sent {packet} to {packet.RemoteEp}");
                }
                if (packet.IsReliable)
                {
                    AddReliablePacket(packet);
                }
            }
        }
        #endregion

        #region AsyncOperations

        public readonly HttpClient HttpClient;
        public List<AsyncRequest> requests = new List<AsyncRequest>();

        public void AddAsyncRequest(AsyncRequest.RequestMethod requestMethod, string uri, IPEndPoint remoteEp, AsyncRequest.RequestType requestType, FormUrlEncodedContent content)
        {
            AsyncRequest asyncRequest = new AsyncRequest(requestMethod, uri, HttpClient, remoteEp, requestType, content);
            requests.Add(asyncRequest);
        }

        public void CheckForResponse()
        {
            foreach (var request in requests)
            {
                if (request.Response.Status == TaskStatus.RanToCompletion && !request.requestedString)
                {
                    request.ReadString();
                    Logs.Log($"[{this.Time}] Got response for request type '{request.requestType}' for '{request.RemoteEp}'");
                }
            }
        }

        public void CheckForResponseString()
        {
            List<AsyncRequest> asyncRequestsToRemove = new List<AsyncRequest>();
            foreach (var request in requests)
            {
                if (request.ResponseString != null)
                {
                    if (request.ResponseString.Status == TaskStatus.RanToCompletion)
                    {
                        request.CallHandler(Json.Deserialize(request.ResponseString.Result, AsyncRequest.GetJsonType(request.requestType)), this);
                        asyncRequestsToRemove.Add(request);
                        Logs.Log($"[{this.Time}] Got string for request type '{request.requestType}' for '{request.RemoteEp}': '{request.ResponseString.Result})'.");
                    }
                }
            }
            foreach (var request in asyncRequestsToRemove)
            {
                requests.Remove(request);
            }
        }

        #endregion

        #region AckPackets
        public void AddReliablePacket(Packet packet)
        {
            ReliablePackets[packet.Id] = new ReliablePacket(packet);
            Logs.Log($"[{Time}] Added Reliable {packet}");
        }

        public bool HasReliablePacket(uint PacketId)
        {
            return ReliablePackets.ContainsKey(PacketId);
        }

        public void AcknowledgeReliablePacket(uint AckPacketId)
        {
            ReliablePackets.Remove(AckPacketId);
            Logs.Log($"[{Time}] Acknowledged Reliable Packet with Packet id '{AckPacketId}'");
        }
        #endregion

        #region ClientsManagement

        public Client GetClientFromName(string Name)
        {
            foreach (Client client in clients.Values)
            {
                if (client.Name == Name)
                    return client;
            }
            return null;
        }

        public void CheckClientsTimeout()
        {
            List<Client> clientsToRemove = new List<Client>();
            foreach (Client client in clients.Values)
            {
                if (this.Time - client.TimeLastPacketSent > MaxIdleTimeout)
                {
                    clientsToRemove.Add(client);
                    Logs.Log($"[{Time}] Queueing {client} removal, last packet sent at {client.TimeLastPacketSent}");
                }
            }
            foreach (Client client in clientsToRemove)
            {
                this.QueueRemoveClient(client, "Kicked for Idle Timeout");
            }
        }

        public void AddClient(Client client)
        {
            clients[client.EndPoint] = client;

            //Send every client already joined to the new client joined
            foreach (var cl in rooms[client.room].Clients)
            {
                if (Equals(cl, client)) continue;

                byte[] welcomeData = new byte[512];
                Json.ClientJoined jsonDataObject = new Json.ClientJoined
                {
                    Name = cl.Name,
                    Id = cl.Id,
                    IsReady = cl.isReady,
                };
                string JsonData = JsonConvert.SerializeObject(jsonDataObject);

                Packet welcomePacket = new Packet(this, welcomeData.Length, welcomeData, client.EndPoint);
                welcomePacket.AddHeaderToData(true, Commands.ClientJoined);
                welcomePacket.Writer.Write(JsonData);
                Logs.Log($"[{Time}] Sent {cl} client joined to {client}");
                SendPacket(welcomePacket);
            }

        }

        public List<Client> QueuedClientsToRemove = new List<Client>();

        public void QueueRemoveClient(Client client, string Reason = "Unknown")
        {
            QueueRemoveClient(client.EndPoint, Reason);
        }

        public void QueueRemoveClient(IPEndPoint endPoint, string Reason)
        {
            var removedClient = clients[endPoint];

            Json.ClientLeft jsonDataObject = new Json.ClientLeft { Reason = Reason, Id = removedClient.Id };
            string jsonData = JsonConvert.SerializeObject(jsonDataObject);

            byte[] data = new byte[512];

            Packet packetRemoveClient = new Packet(this, data.Length, data, null);
            packetRemoveClient.AddHeaderToData(true, Commands.ClientLeft);
            packetRemoveClient.Broadcast = true;
            packetRemoveClient.Writer.Write(jsonData);

            this.SendPacket(packetRemoveClient);

            QueuedClientsToRemove.Add(removedClient);

            Logs.Log($"[{Time}] Queued {removedClient} to remove for '{Reason}'.");
        }

        public void RemoveClients()
        {
            foreach (Client client in QueuedClientsToRemove)
            {
                this.SendChatMessage($"Removed {client}. Clients left in the server: {this.clients.Count}");
                clients.Remove(client.EndPoint);
                Logs.Log($"[{this.Time}] Removed {client}. Clients left in the server: {this.clients.Count}");
                if (this.mode == ServerMode.Battle)
                    CheckForWinner();
            }
            QueuedClientsToRemove.Clear();
        }

        public Client GetClientFromEndPoint(IPEndPoint endPoint)
        {
            return clients[endPoint];
        }

        public Client GetClientFromId(uint id)
        {
            foreach (Client client in clients.Values)
            {
                if (client.Id == id)
                {
                    return client;
                }
            }
            return null;
        }

        public bool HasClient(IPEndPoint endPoint)
        {
            return clients.ContainsKey(endPoint);
        }

        public bool HasClient(Client client)
        {
            return HasClient(client.EndPoint);
        }
        #endregion

        public void SendChatMessage(string text, string sender = "Er Server")
        {
            byte[] data = new byte[512];
            Json.ClientChatted JsonChatData = new Json.ClientChatted() { Text = text, Name = sender };
            Packet ClientChattedPacket = new Packet(this, data.Length, data, null);
            ClientChattedPacket.AddHeaderToData(false, Commands.ClientChatted);
            ClientChattedPacket.Broadcast = true;
            ClientChattedPacket.Writer.Write(JsonConvert.SerializeObject(JsonChatData));
            ClientChattedPacket.Server.SendPacket(ClientChattedPacket);
        }

        #region Gameplay
        public void CheckPlayersReady()
        {
            if (clients.Count <= 1) return;
            foreach (Client cl in clients.Values)
            {
                if (!cl.isReady)
                    return;
            }
            this.mode = ServerMode.Battle;
            MovePlayersToArena();
            SendChatMessage($"Match Started, Players count: {clients.Count}");
        }

        public void MovePlayersToArena()
        {
            foreach (Client cl in clients.Values)
            {
                Rotation rotation = new Rotation(0, 0, 0, 0);
                int spawnIndex = new Random().Next(0, this.arenas[0].spawnPoints.Count);
                cl.SetPosition(this.arenas[0].spawnPoints[spawnIndex]);
                cl.SetRotation(rotation);

                Json.EnterArena jsonDataObject = new Json.EnterArena
                {
                    Id = cl.Id,
                    X = cl.position.X,
                    Y = cl.position.Y,
                    Z = cl.position.Z,
                    Rx = rotation.Rx,
                    Ry = rotation.Ry,
                    Rz = rotation.Rz,
                    Rw = rotation.Rw
                };
                string jsonData = JsonConvert.SerializeObject(jsonDataObject);

                Logs.Log($"[{this.Time}] Sent {cl} to arena at {cl.position}.");

                byte[] data = new byte[512];

                // send a broadcast clientJoined packet
                Packet packetEnterArena = new Packet(this, data.Length, data, null);
                packetEnterArena.AddHeaderToData(true, Commands.EnterArena);
                packetEnterArena.Broadcast = true;
                packetEnterArena.Writer.Write(jsonData);
                this.SendPacket(packetEnterArena);
            }
        }

        public void MovePlayersToLobby()
        {
            foreach (Client cl in clients.Values)
            {
                Logs.Log($"[{this.Time}] Sent {cl} to lobby.");

                byte[] data = new byte[512];

                // send a broadcast clientJoined packet
                Packet packetEnterArena = new Packet(this, data.Length, data, null);
                packetEnterArena.AddHeaderToData(true, Commands.ExitArena);
                packetEnterArena.Broadcast = true;
                this.SendPacket(packetEnterArena);
            }
        }

        public void CheckForWinner()
        {
            if (clients.Count == 1)
            {
                this.mode = ServerMode.Lobby;
                foreach (Client cl in clients.Values)
                {
                    this.SendChatMessage($"{cl.Name} won the game");
                }
                MovePlayersToLobby();
            }
        }
        #endregion
    }
}