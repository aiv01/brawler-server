using System;
using BrawlerServer.Utilities;

namespace BrawlerServer.Server
{
    public class JoinHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Json.JoinHandler JsonData { get; private set; }
        public Client Client { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.JoinHandler));

            //check if client has authed
            if (!packet.Server.CheckAuthedEndPoint(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' tried to join but has not authenticated.");
            }
            // first check if user is already in joined users
            if (packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' tried to join but has already joined.");
            }
            Logs.Log($"[{packet.Server.Time}] Received join message from '{packet.RemoteEp}'.");
            // create client and add it to the server's clients
            Client = packet.Server.GetClientFromAuthedEndPoint(packet.RemoteEp);
            packet.Server.AddClient(Client);
            Logs.Log($"[{packet.Server.Time}] Player with remoteEp '{packet.RemoteEp}' joined the server");
        }
    }
}