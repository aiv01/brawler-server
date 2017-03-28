using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;
using System.Text;

namespace BrawlerServer.Server
{
    public class UpdateHandlerJson
    {
        public string Name;
        public float X;
        public float Y;
        public float Z;
        public float Rx;
        public float Ry;
        public float Rz;
    }

    public class UpdateHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public UpdateHandlerJson JsonData { get; private set; }
        public Client Client { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(Packet, typeof(UpdateHandlerJson));
            
            Logs.Log($"[{packet.Server.Time}] Received update packet from '{packet.RemoteEp}'.");

            Logs.Log($"[{packet.Server.Time}] Player ({JsonData.Name}) with remoteEp '{packet.RemoteEp}' updated his pos to x:{JsonData.X}, y:{JsonData.Y}, z:{JsonData.Z}, rX:{JsonData.Rx}, rY:{JsonData.Ry}, rZ:{JsonData.Rz}");

            byte[] JsonToSend = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(JsonData));
            Packet PacketToSend = new Packet(Packet.Server, 1024, JsonToSend, packet.RemoteEp);

            PacketToSend.Broadcast = true;
            PacketToSend.AddHeaderToData(Packet.Id + 1, true, 3);
            packet.Writer.Write(JsonToSend);
            packet.PacketSize = (int)packet.Stream.Position;

            Packet.Server.SendPacket(PacketToSend);
        }
    }
}