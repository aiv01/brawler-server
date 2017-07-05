using System.Net;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BrawlerServer.Server.Tests
{
    [TestFixture]
    public class JoinHandlerTests
    {
        Packet CreateAndTestJoinPacket(Server server)
        {
            var joinData = new byte[1024];

            var jsonDataObject = new Json.JoinHandler();
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);

            var packet = new Packet(server, 1024, joinData, server.BindEp);
            packet.AddHeaderToData(17, true, Commands.Join);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Join));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            //packet.Server.AddAuthedEndPoint(packet.RemoteEp, new Client(packet.RemoteEp));

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Join));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as JoinHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));
            Assert.That(server.HasClient(packetHandler.Client), Is.EqualTo(true));

            return packet;
        }

        void TestJoinPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestJoinPacketBySocketSendPacket;

            var packet = CreateAndTestJoinPacket(server);
            var client = ((JoinHandler)packet.PacketHandler).Client;

            server.QueueRemoveClient(client);
            Assert.That(server.QueuedClientsToRemove.Contains(client), Is.EqualTo(true));

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command != Commands.Join)
                    return;

                s.IsRunning = false;

                Assert.That(p, Is.Not.EqualTo(null));

                Assert.That(p.Id, Is.EqualTo(17));
                Assert.That(p.IsReliable, Is.EqualTo(true));
                Assert.That(p.Command, Is.EqualTo(Commands.Join));
                Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                var packetHandler = p.PacketHandler as JoinHandler;
                client = ((JoinHandler)packet.PacketHandler).Client;

                Assert.That(packetHandler, Is.Not.EqualTo(null));
                Assert.That(s.HasClient(client), Is.EqualTo(true));
            };

            server.SendPacket(packet);
        }


        [Test]
        public void JoinPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestJoinPacket(server);
        }

        [Test]
        public void JoinPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestJoinPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
