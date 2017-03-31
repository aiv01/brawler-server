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
            var client = new Client(0, new IPEndPoint(0, 0));
            var otherClient = new Client(0, new IPEndPoint(0, 1));
            var otherClient2 = new Client(1, new IPEndPoint(0, 0));
            var equalClient = new Client(0, new IPEndPoint(0, 0));

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