using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;

namespace BrawlerServer.Server
{
    public class JoinHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Json.JoinHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public Client Client { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.JoinHandler));

            //check if client has authed
            if (!packet.Server.CheckAuthedEndPoint(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' tried to join but has not authenticated.");
            }
            // first check if user is already in joined users
            if (packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"{packet.Server.GetClientFromAuthedEndPoint(packet.RemoteEp)} tried to join but has already joined.");
            }
            // create client and add it to the server's clients
            Client = packet.Server.GetClientFromAuthedEndPoint(packet.RemoteEp);
            Logs.Log($"[{packet.Server.Time}] Received join message from {Client}.");

            Client alreadyIn = packet.Server.GetClientFromName(Client.Name);
            if (alreadyIn != null)
            {
                packet.Server.QueueRemoveClient(alreadyIn.EndPoint, "joined from another location");
            }
            packet.Server.AddClient(Client);

            //Set last packet sent as this one
            Client.TimeLastPacketSent = packet.Server.Time;

            JsonSerialized = JsonConvert.SerializeObject(JsonData);
        }
    }
}