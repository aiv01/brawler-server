﻿using System;
using BrawlerServer.Utilities;

namespace BrawlerServer.Server
{
    class HitHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public Json.HitHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public uint Id { get; private set; }
        public float Damage { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            //Check if server is in Battle
            if (Packet.Server.mode != Server.ServerMode.Battle)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent an hit but game hasn't started yet.");
            }

            //Check if client has joined
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent an hit but has never joined.");
            }

            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);

            //Check if client is not dead
            if (Client.isDead)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent an hit but is dead.");
            }

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = Client.Id;
            Damage = packet.Reader.ReadSingle();

            Client.AddHealth(Damage);
            if (Client.isDead)
            {
                packet.Server.SendChatMessage($"{Client.Name} HP:{Client.health} died");
            }

            Logs.Log($"[{packet.Server.Time}] Received hit ({Damage}) from {Client}.");

            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, null);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(false, Commands.ClientHitted);
            packetToSend.Writer.Write(Id);
            packetToSend.Writer.Write(Client.health);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}