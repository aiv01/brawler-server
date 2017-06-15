using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;

namespace BrawlerServer.Server
{
    public class CommandHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Json.CommandHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public Client Client { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.CommandHandler));

            // create client and add it to the server's clients
            Client = packet.Server.GetClientFromAuthedEndPoint(packet.RemoteEp);
            Logs.Log($"[{packet.Server.Time}] Received Command {JsonData.command} from {packet.RemoteEp}.");
        }
    }
}