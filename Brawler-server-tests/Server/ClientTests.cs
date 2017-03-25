using NUnit.Framework;
using BrawlerServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BrawlerServer.Server.Tests
{
    [TestFixture]
    public class ClientTests
    {
        [Test]
        public void ClientEqualityTest()
        {
            var client = new Client(new IPEndPoint(0, 0), "name");
            var otherClient = new Client(new IPEndPoint(0, 1), "name");
            var otherClient2 = new Client(new IPEndPoint(0, 0), "nop");
            var equalClient = new Client(new IPEndPoint(0, 0), "name");

            Assert.That(client, Is.EqualTo(client));
            Assert.That(client, Is.EqualTo(equalClient));
            Assert.That(client, Is.Not.EqualTo(otherClient));
            Assert.That(client, Is.Not.EqualTo(otherClient2));
            Assert.That(client.GetHashCode(), Is.EqualTo(equalClient.GetHashCode()));
            Assert.That(client.GetHashCode(), Is.Not.EqualTo(otherClient.GetHashCode()));
            Assert.That(client.GetHashCode(), Is.Not.EqualTo(otherClient2.GetHashCode()));
            Assert.That(client, Is.Not.EqualTo(2));
        }
    }
}