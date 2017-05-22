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
    public class PongHandlerTests
    {
        Packet CreateAndTestPongPacket(Server server)
        {
            Client client = new Client(server.BindEp);
            server.AddClient(client);

            var UpdateData = new byte[1024];

            var packetId = Utilities.Utilities.GetPacketId();
            var packet = new Packet(server, 1024, UpdateData, server.BindEp);
            packet.AddHeaderToData(packetId, false, Commands.ClientPinged);
            packet.Writer.Write(client.Id);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.ClientPinged));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.ClientPinged));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as PongHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));

            return packet;
        }

        void TestPongPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestPongPacketBySocketSendPacket;

            var packet = CreateAndTestPongPacket(server);
            var client = ((PongHandler)packet.PacketHandler).Client;

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command == Commands.ClientPinged)
                {
                    s.IsRunning = false;

                    Assert.That(p, Is.Not.EqualTo(null));

                    Assert.That(p.Id, Is.GreaterThan(packet.Id));
                    Assert.That(p.IsReliable, Is.EqualTo(false));
                    Assert.That(p.Command, Is.EqualTo(Commands.ClientPinged));
                    Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                    Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                    var packetHandler = p.PacketHandler as PongHandler;

                    Assert.That(packetHandler, Is.Not.EqualTo(null));
                    Assert.That(s.HasClient(client), Is.EqualTo(true));

                    p.Stream.Seek(p.PayloadOffset, SeekOrigin.Begin);
                }
            };

            server.SendPacket(packet);
        }


        [Test]
        public void PongPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestPongPacket(server);
        }

        [Test]
        public void PongPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestPongPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
