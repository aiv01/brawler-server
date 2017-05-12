using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;

namespace BrawlerServer.Server
{
    public class ChatHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Json.ChatHandler JsonData { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.ChatHandler));
            
            // first check if user has joined
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' tried to chat but has never joined.");
            }
            Logs.Log($"[{packet.Server.Time}] Received chat message from '{packet.RemoteEp}' with text '{JsonData.Text}'.");
            
            byte[] data = new byte[512];
            string JsonChatData = JsonConvert.SerializeObject(JsonData);
            Packet ChatPacket = new Packet(packet.Server, data.Length, data, null);
            ChatPacket.AddHeaderToData(false, Commands.Chat);
            ChatPacket.Broadcast = true;
            ChatPacket.Writer.Write(JsonChatData);
            ChatPacket.Server.SendPacket(ChatPacket);
        }
    }
}