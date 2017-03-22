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
    public class CommandHandlerTests
    {
        [Test]
        public void JoinHandler()
        {
            var joinData = new byte[1024];

            var ep = new IPEndPoint(0, 0);
            var server = new Server.Server(ep);

            var jsonDataObject = new JoinHandlerJson { Name = "TEST" };
            var jsonData = JsonConvert.SerializeObject(jsonDataObject);
            
            var packet = new Packet(server, 1024, joinData, ep);
            packet.AddHeaderToData(17, true, 0);
            packet.Writer.Write(jsonData);
            packet.PacketSize = (int) packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(0));
            Assert.That(packet.RemoteEp, Is.EqualTo(ep));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(17));
            Assert.That(packet.IsReliable, Is.EqualTo(true));
            Assert.That(packet.Command, Is.EqualTo(0));
            Assert.That(packet.RemoteEp, Is.EqualTo(ep));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as JoinHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));
            Assert.That(server.HasClient(packetHandler.Client), Is.EqualTo(true));
        }
    }
}
