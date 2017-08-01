using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class MoveHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public Json.MoveHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public uint Id { get; private set; }
        public byte MoveType { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Rx { get; private set; }
        public float Ry { get; private set; }
        public float Rz { get; private set; }
        public float Rw { get; private set; }
        public float Health { get; private set; }
        public float Fury { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            //Check if server is in Battle
            if (Packet.Server.mode != Server.ServerMode.Battle)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a move but game hasn't started yet.");
            }

            //Check if client has joined
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a move but has never joined.");
            }

            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);

            //Check if client is not dead
            if (Client.isDead)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a move but is dead.");
            }

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = Client.Id;
            MoveType = packet.Reader.ReadByte();
            X = packet.Reader.ReadSingle();
            Y = packet.Reader.ReadSingle();
            Z = packet.Reader.ReadSingle();
            Rx = packet.Reader.ReadSingle();
            Ry = packet.Reader.ReadSingle();
            Rz = packet.Reader.ReadSingle();
            Rw = packet.Reader.ReadSingle();
            Health = Client.health;
            Fury = Client.fury;

            Logs.Log($"[{packet.Server.Time}] Received move packet ({MoveType},({X},{Y},{Z}),({Rx},{Ry},{Rz},{Rw}),HP:{Health},Fury:{Fury}) from {Client}.");

            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, null);
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
            packetToSend.Writer.Write(Health);
            packetToSend.Writer.Write(Fury);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}