using System;
using System.Net;
using BrawlerServer.Utilities;

namespace BrawlerServer.Server
{
    public class AuthHandlerJson
    {
        public string AuthToken;
    }

    public class AuthHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public AuthHandlerJson JsonData { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(AuthHandler));

            // first check if remoteEp is already in authed users
            if (packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' tried to authenticate but has already authenticated.");
            }

            //TODO Connect to webService and check auth token

            Logs.Log($"[{packet.Server.Time}] Received Auth token from '{packet.RemoteEp}'.");
            EndPoint = packet.RemoteEp;
            packet.Server.AddAuthedEndPoint(EndPoint);
            Logs.Log($"[{packet.Server.Time}] Player with remoteEp '{packet.RemoteEp}' joined the server");
        }
    }
}