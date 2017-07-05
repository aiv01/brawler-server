using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class NotReadyHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public uint Id { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            //Check if client is connected
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"'{packet.RemoteEp}' sent a NotReady but has never joined.");
            }
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            
            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = Client.Id;

            Logs.Log($"[{packet.Server.Time}] Received NotReady packet from {Client}");
            Client.IsReady(false);

            Json.ClientNotReady jsonToSend = new Json.ClientNotReady() {
                Id = Id
            };
            string toSend = JsonConvert.SerializeObject(jsonToSend);
            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, packet.RemoteEp);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(true, Commands.ClientNotReady);
            packetToSend.Writer.Write(toSend);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}