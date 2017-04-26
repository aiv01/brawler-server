using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class MovedHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public byte MoveType { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Rx { get; private set; }
        public float Ry { get; private set; }
        public float Rz { get; private set; }
        public float Rw { get; private set; }
        public uint Id { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' sent an update but has never joined.");
            }
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);

            Logs.Log($"[{packet.Server.Time}] Received update packet from '{packet.RemoteEp}'.");

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            MoveType = packet.Reader.ReadByte();
            X = packet.Reader.ReadSingle();
            Y = packet.Reader.ReadSingle();
            Z = packet.Reader.ReadSingle();
            Rx = packet.Reader.ReadSingle();
            Ry = packet.Reader.ReadSingle();
            Rz = packet.Reader.ReadSingle();
            Rw = packet.Reader.ReadSingle();
            Id = packet.Server.GetClientFromEndPoint(packet.RemoteEp).Id;

            Packet packetToSend = new Packet(Packet.Server, 1024, packet.Data, packet.RemoteEp);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(false, Commands.ClientMoved);
            packetToSend.Writer.Write(Id);
            packetToSend.Writer.Write(MoveType);
            packetToSend.Writer.Write(X);
            packetToSend.Writer.Write(Y);
            packetToSend.Writer.Write(Z);
            packetToSend.Writer.Write(Rx);
            packetToSend.Writer.Write(Ry);
            packetToSend.Writer.Write(Rz);
            packetToSend.Writer.Write(Rw);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}