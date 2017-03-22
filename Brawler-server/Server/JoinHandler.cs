
using System;

namespace BrawlerServer.Server
{
    public class JoinHandlerJson
    {
        public string Name;
    }

    public class JoinHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public JoinHandlerJson JsonData { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(JoinHandlerJson));

            // first check if user is already in joined users
            if (packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception(string.Format("Client with remoteEp '{0}' tried to join but has already joined.", packet.RemoteEp));
            }
            // create client and add it to the server's clients
            var client = new Client(packet.RemoteEp, JsonData.Name);
            Console.WriteLine("Client with remoteEp '{0}' joined the server", packet.RemoteEp);
            packet.Server.AddClient(client);
        }
    }
}