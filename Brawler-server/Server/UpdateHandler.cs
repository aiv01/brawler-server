using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class UpdateHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Rx { get; private set; }
        public float Ry { get; private set; }
        public float Rz { get; private set; }
        public float Rw { get; private set; }
        public float Id { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' sent an update but has never joined.");
            }

            Logs.Log($"[{packet.Server.Time}] Received update packet from '{packet.RemoteEp}'.");

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            X = packet.Reader.ReadSingle();
            Y = packet.Reader.ReadSingle();
            Z = packet.Reader.ReadSingle();
            Rx = packet.Reader.ReadSingle();
            Ry = packet.Reader.ReadSingle();
            Rz = packet.Reader.ReadSingle();
            Rw = packet.Reader.ReadSingle();
            Id = packet.Server.GetClientFromEndPoint(packet.RemoteEp).Id;

            Packet PacketToSend = new Packet(Packet.Server, 1024, packet.Data, packet.RemoteEp);
            PacketToSend.Broadcast = true;
            PacketToSend.AddHeaderToData(Utilities.Utilities.GetPacketId(), false, (byte)Commands.CLIENT_MOVED);
            PacketToSend.Writer.Write(X);
            PacketToSend.Writer.Write(Y);
            PacketToSend.Writer.Write(Z);
            PacketToSend.Writer.Write(Rx);
            PacketToSend.Writer.Write(Ry);
            PacketToSend.Writer.Write(Rz);
            PacketToSend.Writer.Write(Rw);
            PacketToSend.Writer.Write(Id);
            Packet.Server.SendPacket(PacketToSend);
        }
    }
}