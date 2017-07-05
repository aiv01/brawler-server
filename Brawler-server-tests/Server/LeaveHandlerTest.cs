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
    public class LeaveHandlerTests
    {
        Packet CreateAndTestLeavePacket(Server server)
        {
            var LeaveData = new byte[1024];

            var jsonDataObject = new Json.LeaveHandler();
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);

            var packet = new Packet(server, 1024, LeaveData, server.BindEp);
            packet.AddHeaderToData(17, true, 0);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;
            //packet.Server.AddAuthedEndPoint(packet.RemoteEp, new Client(packet.RemoteEp));
            packet.ParseHeaderFromData();

            packet = new Packet(server, 1024, LeaveData, server.BindEp);
            packet.AddHeaderToData(17, true, Commands.Leave);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Leave));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(Commands.Leave));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as LeaveHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));
            Assert.That(server.QueuedClientsToRemove.Contains(packetHandler.Client), Is.EqualTo(true));

            return packet;
        }

        void TestLeavePacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestLeavePacketBySocketSendPacket;

            var packet = CreateAndTestLeavePacket(server);
            var client = ((LeaveHandler)packet.PacketHandler).Client;
            
            Assert.That(server.QueuedClientsToRemove.Contains(client), Is.EqualTo(true));

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command != Commands.Leave)
                    return;

                s.IsRunning = false;

                Assert.That(p, Is.Not.EqualTo(null));

                Assert.That(p.Id, Is.EqualTo(17));
                Assert.That(p.IsReliable, Is.EqualTo(true));
                Assert.That(p.Command, Is.EqualTo(Commands.Leave));
                Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                var packetHandler = p.PacketHandler as LeaveHandler;
                client = ((LeaveHandler)packet.PacketHandler).Client;

                Assert.That(packetHandler, Is.Not.EqualTo(null));

                Assert.That(s.HasClient(client), Is.EqualTo(false));
            };

            server.SendPacket(packet);
        }


        [Test]
        public void LeavePacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestLeavePacket(server);
        }

        [Test]
        public void LeavePacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestLeavePacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
           
        }
    }
}
