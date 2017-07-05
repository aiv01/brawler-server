using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class LightAttackHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client Client { get; private set; }
        public Json.LightAttackHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public uint Id { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public float Rx { get; private set; }
        public float Ry { get; private set; }
        public float Rz { get; private set; }
        public float Rw { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            //Check if server is in Battle
            if (Packet.Server.mode != Server.ServerMode.Battle)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a light attack but game hasn't started yet.");
            }

            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' sent a light attack but has never joined.");
            }
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            
            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Id = packet.Server.GetClientFromEndPoint(packet.RemoteEp).Id;
            X = packet.Reader.ReadSingle();
            Y = packet.Reader.ReadSingle();
            Z = packet.Reader.ReadSingle();
            Rx = packet.Reader.ReadSingle();
            Ry = packet.Reader.ReadSingle();
            Rz = packet.Reader.ReadSingle();
            Rw = packet.Reader.ReadSingle();

            Logs.Log($"[{packet.Server.Time}] Received light attack packet ({X},{Y},{Z},{Rx},{Ry},{Rz},{Rw}) from '{Client}'.");

            Packet packetToSend = new Packet(Packet.Server, 512, packet.Data, packet.RemoteEp);
            packetToSend.Broadcast = true;
            packetToSend.AddHeaderToData(false, Commands.ClientLightAttacked);
            packetToSend.Writer.Write(Id);
            packetToSend.Writer.Write(X);
            packetToSend.Writer.Write(Y);
            packetToSend.Writer.Write(Z);
            packetToSend.Writer.Write(Rx);
            packetToSend.Writer.Write(Ry);
            packetToSend.Writer.Write(Rz);
            packetToSend.Writer.Write(Rw);
            Packet.Server.SendPacket(packetToSend);

            JsonData = new Json.LightAttackHandler()
            {
                X = this.X,
                Y = this.Y,
                Z = this.Z,
                Rx = this.Rx,
                Ry = this.Ry,
                Rz = this.Rz,
                Rw = this.Rw,
            };
            JsonSerialized = JsonConvert.SerializeObject(JsonData);
        }
    }
}