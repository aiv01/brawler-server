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
    public class AckHandlerTests
    {
        private UInt32 ackPacketId = 8;
        Packet CreateAndTestAckPacket(Server server)
        {
            var data = new byte[1024];

            var packet = new Packet(server, 1024, data, server.BindEp);

            var jsonDataObject = new JoinHandlerJson();
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);

            packet.AddHeaderToData(ackPacketId, true, Commands.Join);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;

            server.AddReliablePacket(packet);

            packet = new Packet(server, 1024, data, server.BindEp);
            packet.AddHeaderToData(17, false, Commands.Ack);
            packet.Writer.Write(ackPacketId);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Ack));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            packet.Stream.Seek(packet.PayloadOffset, SeekOrigin.Begin);
            Assert.That(packet.Reader.ReadUInt32(), Is.EqualTo(ackPacketId));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Ack));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));
            packet.Stream.Seek(packet.PayloadOffset, SeekOrigin.Begin);
            Assert.That(packet.Reader.ReadUInt32(), Is.EqualTo(ackPacketId));

            var packetHandler = packet.PacketHandler as ACKHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));

            return packet;
        }

        void TestAckPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestAckPacketBySocketSendPacket;

            var packet = CreateAndTestAckPacket(server);

            server.ServerPacketReceive += (s, p) =>
            {
                s.IsRunning = false;

                packet = new Packet(server, 1024, new byte[1024], server.BindEp);

                var jsonDataObject = new JoinHandlerJson();
                var jsonData = JsonConvert.SerializeObject(jsonDataObject);

                packet.AddHeaderToData(ackPacketId, true, Commands.Join);
                packet.Writer.Write(jsonData);
                packet.PacketSize = (int)packet.Stream.Position;

                server.AddReliablePacket(packet);

                Assert.That(s.HasReliablePacket(ackPacketId), Is.EqualTo(true));

                Assert.That(p, Is.Not.EqualTo(null));

                Assert.That(p.Id, Is.EqualTo(17));
                Assert.That(p.IsReliable, Is.EqualTo(false));
                Assert.That(p.Command, Is.EqualTo(Commands.Ack));
                Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));
                packet.Stream.Seek(packet.PayloadOffset, SeekOrigin.Begin);
                Assert.That(packet.Reader.ReadUInt32(), Is.Not.EqualTo(ackPacketId));

                p.ParseHeaderFromData();

                var packetHandler = p.PacketHandler as ACKHandler;

                Assert.That(s.HasReliablePacket(ackPacketId), Is.EqualTo(false));
                Assert.That(packetHandler, Is.Not.EqualTo(null));

            };

            server.SendPacket(packet);
        }


        [Test]
        public void AckPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestAckPacket(server);
        }

        [Test]
        public void AckPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestAckPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
