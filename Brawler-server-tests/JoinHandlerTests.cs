using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using BrawlerServer.Server;
using Newtonsoft.Json;
using NUnit;
using NUnit.Framework;

namespace BrawlerServer.Tests
{
    [TestFixture]
    public class JoinHandlerTests
    {
        [Test]
        public void TestJoinPacket()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server.Server(ep);

            CreateAndTestJoinPacket(server);
        }

        Packet CreateAndTestJoinPacket(Server.Server server)
        {
            var joinData = new byte[1024];

            var jsonDataObject = new JoinHandlerJson { Name = "TEST" };
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);

            var packet = new Packet(server, 1024, joinData, server.BindEp);
            packet.AddHeaderToData(17, true, 0);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(0));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(0));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as JoinHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));
            Assert.That(server.HasClient(packetHandler.Client), Is.EqualTo(true));

            return packet;
        }

        void TestJoinPacketBySocketSendPacket(Server.Server server)
        {
            server.ServerTick -= TestJoinPacketBySocketSendPacket;

            var packet = CreateAndTestJoinPacket(server);
            var client = ((JoinHandler) packet.PacketHandler).Client;

            server.RemoveClient(client);
            Assert.That(server.HasClient(client), Is.EqualTo(false));

            server.ServerPacketReceive += (s, p) =>
            {
                s.IsRunning = false;

                Assert.That(p, Is.Not.EqualTo(null));

                Assert.That(p.Id, Is.EqualTo(17));
                Assert.That(p.IsReliable, Is.EqualTo(true));
                Assert.That(p.Command, Is.EqualTo(0));
                Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                var packetHandler = p.PacketHandler as JoinHandler;
                client = ((JoinHandler)packet.PacketHandler).Client;

                Assert.That(packetHandler, Is.Not.EqualTo(null));
                Assert.That(Equals(packetHandler.Client, client), Is.EqualTo(true));

                Assert.That(s.HasClient(client), Is.EqualTo(true));
            };

            server.SendPacket(packet);
        }

        [Test]
        public void TestJoinPacketBySocket()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20237);
            var server = new Server.Server(ep);
            server.ServerTick += TestJoinPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
