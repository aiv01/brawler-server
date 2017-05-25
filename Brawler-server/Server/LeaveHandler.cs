using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;

namespace BrawlerServer.Server
{
    public class LeaveHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Json.LeaveHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public Client Client { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.LeaveHandler));

            // first check if user is not in joined users
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' tried to leave but has never joined.");
            }
            // create client and add it to the server's clients
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            Logs.Log($"[{packet.Server.Time}] Received leave message from {Client}.");
            packet.Server.QueueRemoveClient(packet.RemoteEp, "left the game");

            JsonSerialized = JsonConvert.SerializeObject(JsonData);
        }
    }
}