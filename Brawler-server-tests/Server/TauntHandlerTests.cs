﻿using System;
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
    public class TauntHandlerTests
    {
        Packet CreateAndTestTauntPacket(Server server)
        {
            server.AddClient(new Client(server.BindEp));

            var UpdateData = new byte[1024];

            var packetId = Utilities.Utilities.GetPacketId();
            var packet = new Packet(server, 1024, UpdateData, server.BindEp);
            packet.AddHeaderToData(packetId, false, Commands.Taunt);
            packet.Writer.Write(102.5f);
            packet.Writer.Write(0f);
            packet.Writer.Write(25.25f);
            packet.Writer.Write(654f);
            packet.Writer.Write(177.7f);
            packet.Writer.Write(321f);
            packet.Writer.Write(6f);
            packet.PacketSize = (int)packet.Stream.Position;

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Taunt));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            var payloadOffset = packet.PayloadOffset;

            packet.ParseHeaderFromData();

            Assert.That(packet.Id, Is.EqualTo(packetId));
            Assert.That(packet.IsReliable, Is.EqualTo(false));
            Assert.That(packet.Command, Is.EqualTo(Commands.Taunt));
            Assert.That(packet.RemoteEp, Is.EqualTo(server.BindEp));
            Assert.That(packet.PayloadOffset, Is.EqualTo(payloadOffset));

            var packetHandler = packet.PacketHandler as TauntHandler;

            Assert.That(packetHandler, Is.Not.EqualTo(null));
            
            Assert.That(packetHandler.X, Is.EqualTo(102.5f));
            Assert.That(packetHandler.Y, Is.EqualTo(0f));
            Assert.That(packetHandler.Z, Is.EqualTo(25.25f));
            Assert.That(packetHandler.Rx, Is.EqualTo(654f));
            Assert.That(packetHandler.Ry, Is.EqualTo(177.7f));
            Assert.That(packetHandler.Rz, Is.EqualTo(321f));
            Assert.That(packetHandler.Rw, Is.EqualTo(6f));

            return packet;
        }

        void TestTauntPacketBySocketSendPacket(Server server)
        {
            server.ServerTick -= TestTauntPacketBySocketSendPacket;

            var packet = CreateAndTestTauntPacket(server);
            var client = ((TauntHandler)packet.PacketHandler).Client;

            server.ServerPacketReceive += (s, p) =>
            {
                if (p.Command == Commands.ClientTaunted)
                {
                    s.IsRunning = false;

                    Assert.That(p, Is.Not.EqualTo(null));

                    Assert.That(p.Id, Is.GreaterThan(packet.Id));
                    Assert.That(p.IsReliable, Is.EqualTo(false));
                    Assert.That(p.Command, Is.EqualTo(Commands.ClientTaunted));
                    Assert.That(p.RemoteEp, Is.EqualTo(server.BindEp));
                    Assert.That(p.PayloadOffset, Is.EqualTo(packet.PayloadOffset));

                    var packetHandler = p.PacketHandler as TauntHandler;

                    Assert.That(packetHandler, Is.EqualTo(null));
                    Assert.That(s.HasClient(client), Is.EqualTo(true));

                    p.Stream.Seek(p.PayloadOffset, SeekOrigin.Begin);
                    uint id = p.Reader.ReadUInt32();
                    Assert.That(p.Reader.ReadSingle(), Is.EqualTo(102.5f));
                    Assert.That(p.Reader.ReadSingle(), Is.EqualTo(0f));
                    Assert.That(p.Reader.ReadSingle(), Is.EqualTo(25.25f));
                    Assert.That(p.Reader.ReadSingle(), Is.EqualTo(654f));
                    Assert.That(p.Reader.ReadSingle(), Is.EqualTo(177.7f));
                    Assert.That(p.Reader.ReadSingle(), Is.EqualTo(321f));
                    Assert.That(p.Reader.ReadSingle(), Is.EqualTo(6f));
                }
            };

            server.SendPacket(packet);
        }


        [Test]
        public void TauntPacketTest()
        {
            var ep = new IPEndPoint(0, 0);
            var server = new Server(ep);

            CreateAndTestTauntPacket(server);
        }

        [Test]
        public void TauntPacketBySocketTest()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var server = new Server(ep);
            server.ServerTick += TestTauntPacketBySocketSendPacket;
            server.Bind();
            server.MainLoop();
        }
    }
}
