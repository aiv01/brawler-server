using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BrawlerServer.Utilities;

namespace BrawlerServer.Server
{
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

        private readonly Dictionary<IPEndPoint, Client> clients;

        public bool IsRunning { get; set; }
        public float Time { get; private set; }
        // does NOT count looptime
        public float DeltaTime { get; private set; }

        public Server(IPEndPoint bindEp, int bufferSize = 1024, int packetsPerLoop = 256)
        {
            packetsToSend = new List<Packet>();
            clients = new Dictionary<IPEndPoint, Client>();

            this.packetsPerLoop = packetsPerLoop;
            this.BindEp = bindEp;

            recvBuffer = new byte[bufferSize];
            recvStream = new MemoryStream(recvBuffer);
            recvReader = new BinaryReader(recvStream);
            recvWriter = new BinaryWriter(recvStream);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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

        #region ClientsManagement
        public void AddClient(Client client)
        {
            clients[client.EndPoint] = client;
            Logs.Log($"[{Time}] Added new Client: '{client}'.");
        }

        public void RemoveClient(Client client)
        {
            RemoveClient(client.EndPoint);
        }

        public void RemoveClient(IPEndPoint endPoint)
        {
            var removedClient = clients[endPoint];
            clients.Remove(endPoint);
            Logs.Log($"[{Time}] Removed Client: '{removedClient}'.");
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
            return clients.ContainsValue(client);
        }
        #endregion
    }
}