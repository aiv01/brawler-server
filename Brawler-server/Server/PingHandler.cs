using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class PingHandler : ICommandHandler
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
                throw new Exception($"'{packet.RemoteEp}' sent a ping but has never joined.");
            }
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            
            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = packet.Server.GetClientFromEndPoint(packet.RemoteEp).Id;

            Logs.Log($"[{packet.Server.Time}] Received ping packet from {Client}");

            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, null);
            packetToSend.Broadcast = true;
            //ToDo Fix, should be Ping and not Client Pinged
            packetToSend.AddHeaderToData(false, Commands.ClientPinged);
            packetToSend.Writer.Write(Id);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}