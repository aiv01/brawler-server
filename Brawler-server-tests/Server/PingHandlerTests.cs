using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using BrawlerServer.Server;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using NUnit;
using NUnit.Framework;

namespace BrawlerServer.Server.Tests
{
    [TestFixture]
    public class PingHandlerTests
    {
        Packet CreateAndTestPingPacket(Server server)
        {
            server.AddClient(new Client(server.BindEp));

            var UpdateData = new byte[1024];

            var packetId = Utilities.Utilities.GetPacketId();
            var packet = new Packet(server, 1024, UpdateData, server.BindEp);
            packet.AddHeaderToData(packetId, false, Commands.Ping);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Ping));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Ping));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as PingHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));

            return packet;
        }

        void TestPingPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestPingPacketBySocketSendPacket;

            var packet = CreateAndTestPingPacket(server);
            var client = ((PingHandler)packet.PacketHandler).Client;

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command == Commands.Ping)
                {
                    s.IsRunning = false;

                    Assert.That(p, Is.Not.EqualTo(null));

                    Assert.That(p.Id, Is.GreaterThan(packet.Id));
                    Assert.That(p.IsReliable, Is.EqualTo(false));
                    Assert.That(p.Command, Is.EqualTo(Commands.Ping));
                    Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                    Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                    var packetHandler = p.PacketHandler as PingHandler;

                    Assert.That(packetHandler, Is.Not.EqualTo(null));
                    Assert.That(s.HasClient(client), Is.EqualTo(true));

                    p.Stream.Seek(p.PayloadOffset, SeekOrigin.Begin);
                }
            };

            server.SendPacket(packet);
        }


        [Test]
        public void PingPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestPingPacket(server);
        }

        [Test]
        public void PingPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestPingPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
