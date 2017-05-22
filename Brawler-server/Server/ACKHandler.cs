using System;
using System.IO;
using BrawlerServer.Utilities;

namespace BrawlerServer.Server
{
    public class ACKHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            Packet.Stream.Seek(Packet.PayloadOffset, SeekOrigin.Begin);
            uint ackPacketId = packet.Reader.ReadUInt32();

            Logs.Log($"[{packet.Server.Time}] Received ack from '{packet.RemoteEp}' for packet '{ackPacketId}'.");

            // first check if packet id is already in reliable packets
            if (!packet.Server.HasReliablePacket(ackPacketId))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' tried to send ack for not-reliable / already acknowleged packet.");
            }

            packet.Server.AcknowledgeReliablePacket(ackPacketId);

        }
    }
}