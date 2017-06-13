using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class ReadyHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public Json.ReadyHandler JsonData { get; private set; }
        public uint Id { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.ReadyHandler));

            //Check if client is connected
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"'{packet.RemoteEp}' sent a Ready but has never joined.");
            }
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            
            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = Client.Id;

            Logs.Log($"[{packet.Server.Time}] Received Ready packet from {Client}");
            Client.IsReady(true);

            Json.ClientReady jsonToSend = new Json.ClientReady() {
                Id = Id,
                PrefabId = JsonData.PrefabId
            };
            string toSend = JsonConvert.SerializeObject(jsonToSend);
            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, packet.RemoteEp);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(true, Commands.ClientReady);
            packetToSend.Writer.Write(toSend);
            Packet.Server.SendPacket(packetToSend);

            packet.Server.CheckPlayersReady();
        }
    }
}