using NUnit.Framework;
using BrawlerServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrawlerServer.Server.Tests
{
    [TestFixture]
    public class PacketTests
    {
        [Test]
        public void PacketAddHeaderToDataTest()
        {
            var data = new byte[1024];

            var server = new Server(new IPEndPoint(0, 0));

            var id = 0;
            var reliable = true;
            byte command = 124;
            var packet = new Packet(server, 1024, data, server.BindEp);
            packet.AddHeaderToData(id, reliable, command);

            Assert.That(packet.Id, Is.EqualTo(id));
            Assert.That(packet.IsReliable, Is.EqualTo(reliable));
            Assert.That(packet.Command, Is.EqualTo(command));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
        }

        [Test]
        public void PacketParseHeaderFromDataTest()
        {
            var data = new byte[1024];

            var server = new Server(new IPEndPoint(0, 0));

            var id = int.MaxValue;
            var reliable = false;
            byte command = 0;
            var packet = new Packet(server, 1024, data, server.BindEp);
            packet.AddHeaderToData(id, reliable, command);

            packet.ParseHeaderFromData(false);

            Assert.That(packet.Id, Is.EqualTo(id));
            Assert.That(packet.IsReliable, Is.EqualTo(reliable));
            Assert.That(packet.Command, Is.EqualTo(command));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
        }
    }
}