using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using BrawlerServer.Server;
using Newtonsoft.Json;
using NUnit;
using NUnit.Framework;

namespace BrawlerServer.Server.Tests
{
    [TestFixture]
    public class UpdateHandlerTests
    {
        Packet CreateAndTestUpdatePacket(Server server)
        {
            var UpdateData = new byte[1024];

            var jsonDataObject = new UpdateHandlerJson { Name = "TEST" , X = 102.5f, Y = 0f, Z = 25.25f, Rx = 0f, Ry = 177.7f, Rz = 0f };
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);

            var packet = new Packet(server, 1024, UpdateData, server.BindEp);
            packet.AddHeaderToData(17, true, 3);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(3));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(3));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as UpdateHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));
            Assert.That(packetHandler.JsonData.X, Is.EqualTo(102.5f));
            Assert.That(packetHandler.JsonData.Y, Is.EqualTo(0f));
            Assert.That(packetHandler.JsonData.Z, Is.EqualTo(25.25f));
            Assert.That(packetHandler.JsonData.Rx, Is.EqualTo(0f));
            Assert.That(packetHandler.JsonData.Ry, Is.EqualTo(177.7f));
            Assert.That(packetHandler.JsonData.Rz, Is.EqualTo(0f));

            return packet;
        }

        void TestUpdatePacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestUpdatePacketBySocketSendPacket;

            var packet = CreateAndTestUpdatePacket(server);
            var client = ((UpdateHandler)packet.PacketHandler).Client;

            server.ServerPacketReceive += (s, p) =>
            {
                s.IsRunning = false;

                Assert.That(p, Is.Not.EqualTo(null));

                Assert.That(p.Id, Is.EqualTo(17));
                Assert.That(p.IsReliable, Is.EqualTo(true));
                Assert.That(p.Command, Is.EqualTo(3));
                Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                var packetHandler = p.PacketHandler as UpdateHandler;
                client = ((UpdateHandler)packet.PacketHandler).Client;

                Assert.That(packetHandler, Is.Not.EqualTo(null));
                Assert.That(Equals(packetHandler.Client, client), Is.EqualTo(true));

                Assert.That(packetHandler.JsonData.X, Is.EqualTo(102.5f));
                Assert.That(packetHandler.JsonData.Y, Is.EqualTo(0f));
                Assert.That(packetHandler.JsonData.Z, Is.EqualTo(25.25f));
                Assert.That(packetHandler.JsonData.Rx, Is.EqualTo(0f));
                Assert.That(packetHandler.JsonData.Ry, Is.EqualTo(177.7f));
                Assert.That(packetHandler.JsonData.Rz, Is.EqualTo(0f));
            };

            server.SendPacket(packet);
        }


        [Test]
        public void UpdatePacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestUpdatePacket(server);
        }

        [Test]
        public void UpdatePacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20237);
            var server = new Server(ep);
            server.ServerTick += TestUpdatePacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
