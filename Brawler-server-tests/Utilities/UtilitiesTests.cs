using NUnit.Framework;
using BrawlerServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlerServer.Utilities.Tests
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void SetBitOnByteTest()
        {
            byte b = 1;
            Assert.That(Utilities.SetBitOnByte(b, 0, false), Is.EqualTo(0));
            Assert.That(Utilities.SetBitOnByte(b, 1, true), Is.EqualTo(3));
            Assert.That(Utilities.SetBitOnByte(b, 7, true), Is.EqualTo(129));
        }

        [Test]
        public void IsBitSetTest()
        {
            byte b = 130;
            Assert.That(Utilities.IsBitSet(b, 0), Is.EqualTo(false));
            Assert.That(Utilities.IsBitSet(b, 1), Is.EqualTo(true));
            Assert.That(Utilities.IsBitSet(b, 2), Is.EqualTo(false));
            Assert.That(Utilities.IsBitSet(b, 7), Is.EqualTo(true));
        }
    }
}