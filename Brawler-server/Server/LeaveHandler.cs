using System;

namespace BrawlerServer.Server
{
    public class LeaveHandlerJson
    {
        public string Name;
    }

    public class LeaveHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public LeaveHandlerJson JsonData { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(LeaveHandlerJson));

            packet.Server.RemoveClient(packet.RemoteEp);
            Console.WriteLine("Client with remoteEp '{0}' left the server", packet.RemoteEp);
        }
    }
}