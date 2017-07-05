using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class SwapWeaponHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public Json.SwapWeaponHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public uint Id { get; private set; }
        public byte WeaponId { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            //Check if server is in Battle
            if (Packet.Server.mode != Server.ServerMode.Battle)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a SwapWeapon but game hasn't started yet.");
            }

            //Check if client has joined
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a SwapWeapon but has never joined.");
            }
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = packet.Server.GetClientFromEndPoint(packet.RemoteEp).Id;
            WeaponId = packet.Reader.ReadByte();

            Logs.Log($"[{packet.Server.Time}] Received SwapWeapon packet ({WeaponId}) from {Client}.");

            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, packet.RemoteEp);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(false, Commands.ClientSwappedWeapon);
            packetToSend.Writer.Write(WeaponId);
            packetToSend.Writer.Write(Id);
            Packet.Server.SendPacket(packetToSend);

            JsonData = new Json.SwapWeaponHandler()
            {
                WeaponId = this.WeaponId,
            };
            JsonSerialized = JsonConvert.SerializeObject(JsonData);
        }
    }
}