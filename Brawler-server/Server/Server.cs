using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using BrawlerServer.Utilities;
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

        public void UpdateTime()
        {
            this.Time = Packet.Server.Time;
            Logs.Log($"[{Packet.Server.Time}] Updated Reliable {Packet} time to {Time}.");
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
            Authentication
        }

        private static readonly Dictionary<RequestType, Type> Jsons = new Dictionary<RequestType, Type> {
            { RequestType.Authentication, typeof(Json.AuthPlayerPost) }
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
        }

        public static Type GetJsonType(RequestType requestType)
        {
            return Jsons[requestType];
        }
    }

    public class Server
    {
        public delegate void ServerTickHandler(Server server);
        public delegate void ServerPacketReceiveHandler(Server server, Packet packet);
        public event ServerTickHandler ServerTick;
        public event ServerPacketReceiveHandler ServerPacketReceive;

        public IPEndPoint BindEp { get; private set; }
        private readonly Socket socket;
        private readonly List<Packet> packetsToSend;
        private readonly byte[] recvBuffer;
        private readonly MemoryStream recvStream;
        private readonly BinaryReader recvReader;
        private readonly BinaryWriter recvWriter;

        private readonly int packetsPerLoop;

        private Dictionary<uint, ReliablePacket> ReliablePackets;
        public uint MaxAckResponseTime { get; private set; }

        private readonly Dictionary<IPEndPoint, Client> authedEndPoints;

        private readonly Dictionary<IPEndPoint, Client> clients;

        public bool IsRunning { get; set; }
        public uint Time { get; private set; }
        // does NOT count looptime
        public uint DeltaTime { get; private set; }

        public uint MaxIdleTimeout { get; private set; }

        public Server(IPEndPoint bindEp, int bufferSize = 512, int packetsPerLoop = 256)
        {
            packetsToSend = new List<Packet>();
            clients = new Dictionary<IPEndPoint, Client>();
            authedEndPoints = new Dictionary<IPEndPoint, Client>();
            HttpClient = new HttpClient();

            this.packetsPerLoop = packetsPerLoop;
            this.BindEp = bindEp;

            recvBuffer = new byte[bufferSize];
            recvStream = new MemoryStream(recvBuffer);
            recvReader = new BinaryReader(recvStream);
            recvWriter = new BinaryWriter(recvStream);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { Blocking = false };

            this.ReliablePackets = new Dictionary<uint, ReliablePacket>();
            this.MaxAckResponseTime = 5000;

            this.MaxIdleTimeout = 10000;
        }

        public void Bind()
        {
            if (!socket.IsBound)
            {
                socket.Bind(BindEp);
                BindEp = (IPEndPoint)socket.LocalEndPoint;
            }
        }

        public void MainLoop(float loopTime = 1f / 10)
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

                // first receive packets
                var packetIndex = 0;
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
                        Logs.Log($"[{Time}] Received {packet}.");
                    }
                    catch (Exception e)
                    {
                        Logs.LogError($"[{Time}] Error while parsing packet from '{remoteEp}', with size of {size}:");
                        foreach(string line in e.Message.Split('\n'))
                            Logs.LogError($"[{Time}] {line}");
                        continue;
                    }
                    finally
                    {
                        ServerPacketReceive?.Invoke(this, packet);
                    }

                    packetIndex++;
                }
                //Check if client didn't send any packet in MaxIdleTimeout seconds
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
                //Check if reliable packet has passed the time check limit
                foreach (KeyValuePair<uint, ReliablePacket> reliablePacket in ReliablePackets)
                {
                    if (this.Time > reliablePacket.Value.Time + this.MaxAckResponseTime)
                    {
                        Logs.LogWarning($"[{this.Time}] Queued Reliable {reliablePacket.Value.Packet} with time {Time}.");
                        this.SendPacket(reliablePacket.Value.Packet);
                    }
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
                            //    continue;
                            socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, pair.Key);
                            Logs.Log($"[{Time}] Sent broadcast {packet} to {pair.Value}.");
                        }
                    }
                    else
                    {
                        socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, packet.RemoteEp);
                        Logs.Log($"[{Time}] Sent {packet} to {packet.RemoteEp}");
                    }
                    if (packet.IsReliable)
                    {
                        if (HasReliablePacket(packet.Id))
                            ReliablePackets[packet.Id].UpdateTime();
                        else
                            AddReliablePacket(packet);
                    }

                    //remove Queued Clients
                    RemoveClients();
                }
                packetsToSend.Clear();

                CheckForResponse();
                CheckForResponseString();


                ServerTick?.Invoke(this);

                DeltaTime = (uint)watch.ElapsedMilliseconds - Time;
                Thread.Sleep(Math.Max((int)(msLoopTime - DeltaTime), 0));
            }
            socket.Close();
        }

        public void SendPacket(Packet packet)
        {
            packetsToSend.Add(packet);
        }

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

        #region AuthEndPoint

        public void AddAuthedEndPoint(IPEndPoint endPoint, Client client)
        {
            this.authedEndPoints.Add(endPoint, client);
        }

        public bool CheckAuthedEndPoint(IPEndPoint endPoint)
        {
            return this.authedEndPoints.ContainsKey(endPoint);
        }

        public Client GetClientFromAuthedEndPoint(IPEndPoint endPoint)
        {
            return this.authedEndPoints[endPoint];
        }

        #endregion

        #region ClientsManagement
        public void AddClient(Client client)
        {
            Json.ClientJoined jsonDataObject = new Json.ClientJoined { Name = client.Name, Id = client.Id };
            string jsonData = JsonConvert.SerializeObject(jsonDataObject);

            clients[client.EndPoint] = client;
            Logs.Log($"[{Time}] Added new {client}.");

            byte[] data = new byte[512];

            // send a broadcast clientJoined packet
            Packet packetClientAdded = new Packet(this, data.Length, data, null);
            packetClientAdded.AddHeaderToData(true, Commands.ClientJoined);
            packetClientAdded.Broadcast = true;
            packetClientAdded.Writer.Write(jsonData);
            SendPacket(packetClientAdded);
            
            //Send every client already joined to the new client joined
            foreach (var cl in clients.Values)
            {
                if (Equals(cl, client)) continue;

                byte[] welcomeData = new byte[512];
                Json.ClientJoined welcomeJsonDataObject = new Json.ClientJoined() { Name = client.Name, Id = cl.Id };
                string welcomeJsonData = JsonConvert.SerializeObject(welcomeJsonDataObject);

                Packet welcomePacket = new Packet(this, welcomeData.Length, welcomeData, client.EndPoint);
                welcomePacket.AddHeaderToData(true, Commands.ClientJoined);
                welcomePacket.Writer.Write(welcomeJsonData);
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
                clients.Remove(client.EndPoint);
            }
        }

        public Client GetClientFromEndPoint(IPEndPoint endPoint)
        {
            return clients[endPoint];
        }

        public Client GetClientFromId(uint id)
        {
            foreach(Client client in clients.Values)
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
    }
}