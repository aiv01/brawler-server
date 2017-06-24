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
                throw new Exception($"RemoteEp '{packet.RemoteEp}' tried to chat but has never joined.");
            }
            Client client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            string name = JsonData.Name != "" ? JsonData.Name : client.ToString();
            Logs.Log($"[{packet.Server.Time}] Received chat message from {name} with text '{JsonData.Text}'.");

            packet.Server.SendChatMessage(JsonData.Text, JsonData.Name != "" ? JsonData.Name : client.Name);
        }
    }
}