using System;

namespace BrawlerServer.Server
{
    //TODO: Server To Client
    /*public class KickHandlerJson
    {
        public string Name;
    }

    public class KickHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public JoinHandlerJson JsonData { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(JoinHandlerJson));

            packet.Server.RemoveClient(packet.RemoteEp);
            Console.WriteLine("Client with remoteEp '{0}' was kicked from the server", packet.RemoteEp);
        }
    }*/
}