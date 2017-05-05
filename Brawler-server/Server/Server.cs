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
    public struct ReliablePacket
    {
        public Packet Packet { get; private set; }
        public long Time { get; private set; }

        public ReliablePacket(Packet packet)
        {
            this.Packet = packet;
            this.Time = packet.Server.Time;
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

        private Dictionary<long, ReliablePacket> ReliablePackets;
        public int MaxAckResponseTime { get; private set; }

        private readonly Dictionary<IPEndPoint, Client> authedEndPoints;

        public readonly HttpClient HttpClient;

        private readonly Dictionary<IPEndPoint, Client> clients;

        public bool IsRunning { get; set; }
        public long Time { get; private set; }
        // does NOT count looptime
        public long DeltaTime { get; private set; }

        public long MaxIdleTimeout { get; private set; }

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

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {Blocking = false};

            this.ReliablePackets = new Dictionary<long, ReliablePacket>();
            this.MaxAckResponseTime = 5000;

            this.MaxIdleTimeout = 10000;
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
                        packet = new Packet(this, size, recvBuffer, (IPEndPoint)remoteEp, recvStream, recvReader,
                            recvWriter);
                        if (clients.ContainsKey((IPEndPoint)remoteEp))
                        {
                            clients[(IPEndPoint)remoteEp].TimeLastPacketSent = this.Time;
                        }
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
                //Check if client didn't send any packet in MaxIdleTimeout seconds
                List<Client> clientsToRemove = new List<Client>();
                foreach (Client client in clients.Values)
                {
                    if (this.Time - client.TimeLastPacketSent > MaxIdleTimeout)
                    {
                        clientsToRemove.Add(client);
                    }
                }
                foreach (Client client in clientsToRemove)
                {
                    this.RemoveClient(client, "Kicked for Idle Timeout");
                }
                //Check if reliable packet has passed the time check limit
                Dictionary<long, ReliablePacket> reliablePacketsToRemove = new Dictionary<long, ReliablePacket>();
                foreach (KeyValuePair<long, ReliablePacket> reliablePacket in ReliablePackets)
                {
                    if (this.Time > reliablePacket.Value.Time + this.MaxAckResponseTime)
                    {
                        Logs.Log($"Reliable: {reliablePacket.Value.Time} {this.Time} {reliablePacket.Value.Time + this.MaxAckResponseTime}");
                        this.SendPacket(reliablePacket.Value.Packet);
                        reliablePacketsToRemove.Add(reliablePacket.Key, reliablePacket.Value);
                    }
                }
                foreach(KeyValuePair<long, ReliablePacket> reliablePacket in reliablePacketsToRemove)
                {
                    ReliablePackets.Remove(reliablePacket.Key);
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
                                    AddReliablePacket(packet);
                                }
                            }
                        }
                        Logs.Log($"Sent packet broadcast with command {packet.Command}. Packets in this block {packetsToSend.Count}");
                    }
                    else
                    {
                        socket.SendTo(packet.Data, 0, packet.PacketSize, SocketFlags.None, packet.RemoteEp);
                        Logs.Log($"Sent packet with command {packet.Command} to remoteEp {packet.RemoteEp}");
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
        public void AddReliablePacket(Packet packet)
        {
            ReliablePackets[packet.Id] = new ReliablePacket(packet);
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
            Logs.Log($"[{Time}] Added new Client: '{client}'.");

            byte[] data = new byte[512];

            // send a broadcast clientJoined packet
            Packet packetClientAdded = new Packet(this, data.Length, data, null);
            packetClientAdded.AddHeaderToData(true, Commands.ClientJoined);
            packetClientAdded.Broadcast = true;
            packetClientAdded.Writer.Write(jsonData);
            SendPacket(packetClientAdded);

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

        public void RemoveClient(Client client, string Reason = "Unkown")
        {
            RemoveClient(client.EndPoint, Reason);
        }

        public void RemoveClient(IPEndPoint endPoint, string Reason)
        {
            var removedClient = clients[endPoint];

            Json.ClientLeft jsonDataObject = new Json.ClientLeft { Reason = Reason, Id = removedClient.Id };
            string jsonData = JsonConvert.SerializeObject(jsonDataObject);

            clients.Remove(endPoint);
            Logs.Log($"[{Time}] Removed Client: '{removedClient}'.");

            byte[] data = new byte[512];

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