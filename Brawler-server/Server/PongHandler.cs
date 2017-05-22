using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class PongHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public uint Id { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;
            
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            
            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = packet.Reader.ReadUInt32();

            Logs.Log($"[{packet.Server.Time}] Received pong packet from {Client} with pinged id {Id}");

            //Check if client that pinged is still in game
            if (Packet.Server.GetClientFromId(Id) == null)
            {
                throw new Exception($"[{packet.Server.Time}] Client that sent ping left the game.");
            }
            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, Packet.Server.GetClientFromId(Id).EndPoint);
            packetToSend.AddHeaderToData(false, Commands.ClientPinged);
            packetToSend.Writer.Write(Client.Id);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}