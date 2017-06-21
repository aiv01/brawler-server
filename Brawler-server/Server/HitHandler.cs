using System;
using BrawlerServer.Utilities;

namespace BrawlerServer.Server
{
    class HitHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public Json.HitHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Rx { get; private set; }
        public float Ry { get; private set; }
        public float Rz { get; private set; }
        public float Rw { get; private set; }
        public uint Id { get; private set; }
        public float Damage { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            //Check if server is in Battle
            if (Packet.Server.mode != Server.ServerMode.Battle)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent an hit but game hasn't started yet.");
            }

            //Check if client has joined
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent an hit but has never joined.");
            }

            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);

            //Check if client is not dead
            if (Client.isDead)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent an hit but is dead.");
            }

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = Client.Id;
            X = packet.Reader.ReadSingle();
            Y = packet.Reader.ReadSingle();
            Z = packet.Reader.ReadSingle();
            Rx = packet.Reader.ReadSingle();
            Ry = packet.Reader.ReadSingle();
            Rz = packet.Reader.ReadSingle();
            Rw = packet.Reader.ReadSingle();
            Damage = packet.Reader.ReadSingle();

            Client.AddHealth(Damage);
            if (Client.isDead)
            {
                packet.Server.SendChatMessage($"{Client.Name} HP:{Client.health} died");
            }

            Logs.Log($"[{packet.Server.Time}] Received hit ({Damage}) from {Client}.");

            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, packet.RemoteEp);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(false, Commands.ClientHitted);
            packetToSend.Writer.Write(Id);
            packetToSend.Writer.Write(X);
            packetToSend.Writer.Write(Y);
            packetToSend.Writer.Write(Z);
            packetToSend.Writer.Write(Rx);
            packetToSend.Writer.Write(Ry);
            packetToSend.Writer.Write(Rz);
            packetToSend.Writer.Write(Rw);
            packetToSend.Writer.Write(Client.health);
            Packet.Server.SendPacket(packetToSend);
        }
    }
}
