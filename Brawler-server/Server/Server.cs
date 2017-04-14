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

namespace BrawlerServer.Server
{
    public class ClientJoinJson
    {
        public uint Id;
        public string Name;
    }

    public class ClientLeftJson
    {
        public uint Id;
        public string Reason;
    }

    public class ClientAuthJson
    {
        public string Ip;
        public string Port;
    }

    public struct ReliablePacket
    {
        public Packet Packet { get; private set; }
        public float Time { get; private set; }

        public ReliablePacket(Packet packet)
        {
            this.Packet = packet;
            this.Time = packet.Server.Time + packet.Server.MaxAckResponseTime;
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
        public float MaxAckResponseTime { get; private set; }

        private readonly List<IPEndPoint> authedEndPoints;

        public readonly HttpClient HttpClient;

        private readonly Dictionary<IPEndPoint, Client> clients;

        public bool IsRunning { get; set; }
        public float Time { get; private set; }
        // does NOT count looptime
        public float DeltaTime { get; private set; }

        public Server(IPEndPoint bindEp, int bufferSize = 1024, int packetsPerLoop = 256)
        {
            packetsToSend = new List<Packet>();
            clients = new Dictionary<IPEndPoint, Client>();
            authedEndPoints = new List<IPEndPoint>();
            HttpClient = new HttpClient();

            this.packetsPerLoop = packetsPerLoop;
            this.BindEp = bindEp;

            recvBuffer = new byte[bufferSize];
            recvStream = new MemoryStream(recvBuffer);
            recvReader = new BinaryReader(recvStream);
            recvWriter = new BinaryWriter(recvStream);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {Blocking = false};

            this.ReliablePackets = new Dictionary<uint, ReliablePacket>();
            this.MaxAckResponseTime = 5f;
        }

        public void Bind()
        {
            if (!socket.IsBound)
            {
                socket.Bind(BindEp);
                BindEp = (IPEndPoint) socket.LocalEndPoint;
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
                Time = watch.ElapsedMilliseconds;

                // first receive packets
                var packetIndex = 0;
                while (packetIndex < packetsPerLoop && socket.Available > 0)
                {
                    var size = socket.ReceiveFrom(recvBuffer, ref remoteEp);
                    Logs.Log($"[{Time}] Received message from '{remoteEp}', size: '{size}'.");
                    Packet packet = null;
                    try
                    {
                        packet = new Packet(this, size, recvBuffer, (IPEndPoint) remoteEp, recvStream, recvReader,
                            recvWriter);
                        packet.ParseHeaderFromData();
                    }
                    catch (Exception e)
                    {
                        Logs.LogError($"Error while parsing packet from '{remoteEp}', with size of '{size}':\n{e}");
                        continue;
                    }
                    finally
                    {
                        ServerPacketReceive?.Invoke(this, packet);
                    }

                    packetIndex++;
                }
                //Check if reliable packet has passed the time check limit
                foreach (KeyValuePair<uint, ReliablePacket> reliablePacket in ReliablePackets)
                {
                    if (reliablePacket.Value.Time > this.Time)
                    {
                        this.SendPacket(reliablePacket.Value.Packet);
                        //Packet isn't removed here as it gets replaced when is sent, this may no longer work if we change the server behaviour
                    }
                }
                // then send packets (do we need to send only a fixed number?)
                foreach (var packet in packetsToSend)
                {
                    if (packet.Broadcast)
                    {
                        foreach (var pair in clients)
                        {
                            if (!pair.Key.Equals(packet.RemoteEp))
                            {
                                socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, pair.Key);
                                if (packet.IsReliable)
                                {
                                    ReliablePackets.Add(packet.Id, new ReliablePacket(packet));
                                }
                            }
                        }
                    }
                    else
                    {
                        socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, packet.RemoteEp);
                    }
                }
                packetsToSend.Clear();
                

                ServerTick?.Invoke(this);

                DeltaTime = watch.ElapsedMilliseconds - Time;
                Thread.Sleep(Math.Max((int)(msLoopTime - DeltaTime), 0));
            }
            socket.Close();
        }
        
        public void SendPacket(Packet packet)
        {
            packetsToSend.Add(packet);
        }

        #region AckPackets
        //For Tests purposes
        public void AddReliablePacket(Packet packet)
        {
            ReliablePackets.Add(packet.Id, new ReliablePacket(packet));
        }

        public bool HasReliablePacket(uint PacketId)
        {
            return ReliablePackets.ContainsKey(PacketId);
        }

        public void AcknowledgeReliablePacket(uint AckPacketId)
        {
            ReliablePackets.Remove(AckPacketId);
        }
        #endregion

        #region AuthEndPoint

        public void AddAuthedEndPoint(IPEndPoint endPoint)
        {
            this.authedEndPoints.Add(endPoint);
        }

        public bool CheckAuthedEndPoint(IPEndPoint endPoint)
        {
            return this.authedEndPoints.Contains(endPoint);
        }

        #endregion

        #region ClientsManagement
        public void AddClient(Client client)
        {
            ClientJoinJson jsonDataObject = new ClientJoinJson { Name = client.Name, Id = client.Id };
            string jsonData = JsonConvert.SerializeObject(jsonDataObject);

            clients[client.EndPoint] = client;
            Logs.Log($"[{Time}] Added new Client: '{client}'.");

            byte[] data = new byte[256];

            // send a broadcast clientJoined packet
            Packet packetClientAdded = new Packet(this, data.Length, data, null);
            packetClientAdded.AddHeaderToData(true, Commands.ClientJoined);
            packetClientAdded.Broadcast = true;
            packetClientAdded.Writer.Write(jsonData);
            SendPacket(packetClientAdded);

            foreach (var cl in clients.Values)
            {
                if (Equals(cl, client)) continue;
                byte[] welcomeData = new byte[256];
                ClientJoinJson welcomeJsonDataObject = new ClientJoinJson { Name = client.Name, Id = cl.Id };
                string welcomeJsonData = JsonConvert.SerializeObject(welcomeJsonDataObject);

                Packet welcomePacket = new Packet(this, data.Length, data, null);
                welcomePacket.AddHeaderToData(true, Commands.ClientJoined);
                welcomePacket.Writer.Write(welcomeJsonData);
                SendPacket(welcomePacket);
            }

        }

        public void RemoveClient(Client client, string Reason = "Unkown")
        {
            RemoveClient(client.EndPoint, Reason);
        }

        public void RemoveClient(IPEndPoint endPoint, string Reason)
        {
            var removedClient = clients[endPoint];

            ClientLeftJson jsonDataObject = new ClientLeftJson { Reason = Reason, Id = removedClient.Id };
            string jsonData = JsonConvert.SerializeObject(jsonDataObject);

            clients.Remove(endPoint);
            Logs.Log($"[{Time}] Removed Client: '{removedClient}'.");

            byte[] data = new byte[1024];

            Packet packetRemoveClient = new Packet(this, data.Length, data, null);
            packetRemoveClient.AddHeaderToData(true, Commands.ClientLeft);
            packetRemoveClient.Broadcast = true;
            packetRemoveClient.Writer.Write(jsonData);

            this.SendPacket(packetRemoveClient);
        }

        public Client GetClientFromEndPoint(IPEndPoint endPoint)
        {
            return clients[endPoint];
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