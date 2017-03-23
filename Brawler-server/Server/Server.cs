using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BrawlerServer.Server
{
    public class Server
    {
        private readonly IPEndPoint bindEp;
        private readonly Socket socket;
        private readonly List<Packet> packetsToSend;
        private readonly byte[] recvBuffer;
        private readonly MemoryStream recvStream;
        private readonly BinaryReader recvReader;
        private readonly BinaryWriter recvWriter;

        private readonly int packetsPerLoop;

        private readonly Dictionary<IPEndPoint, Client> clients;

        public float Time { get; private set; }
        // does NOT count looptime
        public float DeltaTime { get; private set; }

        public Server(IPEndPoint bindEp, int bufferSize = 1024, int packetsPerLoop = 256)
        {
            packetsToSend = new List<Packet>();
            clients = new Dictionary<IPEndPoint, Client>();

            this.packetsPerLoop = packetsPerLoop;
            this.bindEp = bindEp;

            recvBuffer = new byte[bufferSize];
            recvStream = new MemoryStream(recvBuffer);
            recvReader = new BinaryReader(recvStream);
            recvWriter = new BinaryWriter(recvStream);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Bind()
        {
            socket.Bind(bindEp);
        }

        public void MainLoop(float loopTime = 1f / 10)
        {
            var msLoopTime = (int)(loopTime * 1000f);

            EndPoint remoteEp = new IPEndPoint(0, 0);

            var watch = Stopwatch.StartNew();
            while (true)
            {
                Time = watch.ElapsedMilliseconds;

                // first receive packets
                var packetIndex = 0;
                while (packetIndex < packetsPerLoop && socket.Available > 0)
                {
                    var size = socket.ReceiveFrom(recvBuffer, ref remoteEp);
                    try
                    {
                        var packet = new Packet(this, size, recvBuffer, (IPEndPoint)remoteEp, recvStream, recvReader, recvWriter);
                        packet.ParseHeaderFromData();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error while parsing packet from '{0}', with size of '{1}':\n{2}", remoteEp, size, e);
                        continue;
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

                DeltaTime = watch.ElapsedMilliseconds - Time;
                Thread.Sleep(Math.Max((int)(msLoopTime - DeltaTime), 0));
            }
        }

        public void SendPacket(Packet packet)
        {
            packetsToSend.Add(packet);
        }

        #region ClientsManagement
        public void AddClient(Client client)
        {
            clients[client.EndPoint] = client;
        }

        public void RemoveClient(Client client)
        {
            RemoveClient(client.EndPoint);
        }

        public void RemoveClient(IPEndPoint endPoint)
        {
            var removedClient = clients[endPoint];
            clients.Remove(endPoint);
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