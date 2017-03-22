using System;

namespace BrawlerServer.Server
{
    public class UpdateHandlerJson
    {
        public float x;
        public float y;
        public float z;
        public float rX;
        public float rY;
        public float rZ;
    }

    public class UpdateHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public UpdateHandlerJson JsonData { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(UpdateHandlerJson));

            packet.Server.RemoveClient(packet.RemoteEp);
            Console.WriteLine("Client with remoteEp '{0}' updated (x: {1}, y: {2}, z: {3}, rX: {4}, rY: {5}, rZ: {6},)", packet.RemoteEp, JsonData.x, JsonData.y, JsonData.z, JsonData.rX, JsonData.rY, JsonData.rZ);
        }
    }
}