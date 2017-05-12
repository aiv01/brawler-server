using System.Net;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Diagnostics;

namespace BrawlerServer.Server.Tests
{
    [TestFixture]
    public class AuthHandlerTests
    {
        Packet CreateAndTestAuthPacket(Server server)
        {
            var authData = new byte[1024];

            var jsonDataObject = new Json.AuthHandler() { AuthToken = "e1cdcd42-0b98-4d12-82fd-53d0fb241ec6" };
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);

            var packet = new Packet(server, 1024, authData, server.BindEp);
            packet.AddHeaderToData(17, true, Commands.Auth);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Auth));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();
            
            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Auth));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as AuthHandler;

            packet.Server.AddAuthedEndPoint(packet.RemoteEp, new Client(packet.RemoteEp));

            Assert.That(packet.Server.CheckAuthedEndPoint(packet.RemoteEp), Is.EqualTo(true));

            return packet;
        }

        void TestAuthPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestAuthPacketBySocketSendPacket;

            var packet = CreateAndTestAuthPacket(server);

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command != Commands.Auth)
                    return;

                s.IsRunning = false;

                Assert.That(p, Is.Not.EqualTo(null));

                Assert.That(p.Id, Is.EqualTo(17));
                Assert.That(p.IsReliable, Is.EqualTo(true));
                Assert.That(p.Command, Is.EqualTo(Commands.Auth));
                Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                var packetHandler = p.PacketHandler as AuthHandler;

                Assert.That(packet.Server.CheckAuthedEndPoint(packet.RemoteEp), Is.EqualTo(true));

                Assert.That(packetHandler, Is.Not.EqualTo(null));
            };

            server.SendPacket(packet);
        }


        [Test]
        public void AuthPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestAuthPacket(server);
        }

        [Test]
        public void AuthPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestAuthPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
