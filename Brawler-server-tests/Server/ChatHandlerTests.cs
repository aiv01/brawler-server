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
    public class ChatHandlerTests
    {
        Packet CreateAndTestChatPacket(Server server)
        {
            server.AddClient(new Client(server.BindEp));

            var Data = new byte[1024];
            var Json = new Json.ChatHandler() { Text = "Chat Test" };
            var JsonString = JsonConvert.SerializeObject(Json);

            var packetId = Utilities.Utilities.GetPacketId();
            var packet = new Packet(server, 1024, Data, server.BindEp);
            packet.AddHeaderToData(packetId, false, Commands.Chat);
            packet.Writer.Write(JsonString);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Chat));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Chat));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as ChatHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));
            
            Assert.That(packetHandler.JsonData.Text, Is.EqualTo("Chat Test"));

            return packet;
        }

        void TestChatPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestChatPacketBySocketSendPacket;

            var packet = CreateAndTestChatPacket(server);

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command == Commands.Chat)
                {
                    s.IsRunning = false;

                    Assert.That(p, Is.Not.EqualTo(null));

                    Assert.That(p.Id, Is.EqualTo(packet.Id));
                    Assert.That(p.IsReliable, Is.EqualTo(false));
                    Assert.That(p.Command, Is.EqualTo(Commands.Chat));
                    Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                    Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                    var packetHandler = p.PacketHandler as ChatHandler;

                    Assert.That(packetHandler, Is.Not.EqualTo(null));
                    
                    
                    Assert.That(packetHandler.JsonData.Text, Is.EqualTo("Chat Test"));
                }
            };

            server.SendPacket(packet);
        }


        [Test]
        public void ChatPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestChatPacket(server);
        }

        [Test]
        public void ChatPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestChatPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
