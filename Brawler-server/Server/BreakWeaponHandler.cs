using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class BreakWeaponHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public uint Id { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            //Check if server is in Battle
            if (Packet.Server.mode != Server.ServerMode.Battle)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a weapon break but game hasn't started yet.");
            }

            //Check if client has joined
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a weapon break but has never joined.");
            }

            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);

            //Check if client is not dead
            if (Client.isDead)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a weapon break but is dead.");
            }

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = packet.Reader.ReadUInt32();

            Logs.Log($"[{packet.Server.Time}] Received Weapon Break packet ({Id}) from {Client}.");

            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, null);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(false, Commands.ClientBrokeWeapon);
            packetToSend.Writer.Write(Id);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}