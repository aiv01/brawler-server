using System.Net;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BrawlerServer.Server.Tests
{
    [TestFixture]
    public class CommandHandlerTests
    {
        Packet CreateAndTestCommandPacket(Server server)
        {
            var CommandData = new byte[1024];

            var jsonDataObject = new Json.CommandHandler();
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);

            var packet = new Packet(server, 1024, CommandData, server.BindEp);
            packet.AddHeaderToData(17, true, Commands.Command);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Command));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            //packet.Server.AddAuthedEndPoint(packet.RemoteEp, new Client(packet.RemoteEp));

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Command));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as CommandHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));

            return packet;
        }

        void TestCommandPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestCommandPacketBySocketSendPacket;

            var packet = CreateAndTestCommandPacket(server);

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command != Commands.Command)
                    return;

                s.IsRunning = false;

                Assert.That(p, Is.Not.EqualTo(null));

                Assert.That(p.Id, Is.EqualTo(17));
                Assert.That(p.IsReliable, Is.EqualTo(true));
                Assert.That(p.Command, Is.EqualTo(Commands.Command));
                Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                var packetHandler = p.PacketHandler as CommandHandler;

                Assert.That(packetHandler, Is.Not.EqualTo(null));
            };

            server.SendPacket(packet);
        }


        [Test]
        public void CommandPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestCommandPacket(server);
        }

        [Test]
        public void CommandPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestCommandPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
