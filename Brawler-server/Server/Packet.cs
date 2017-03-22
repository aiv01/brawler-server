using System;
using System.Net;

namespace BrawlerServer.Server
{
    public class Packet
    {
        public Server Server { get; set; }

        public byte[] Data { get; private set; }
        public IPEndPoint RemoteEp { get; private set; }
        // header
        public int Id { get; private set; }
        public float Time { get; private set; }
        public bool IsReliable { get; private set; }
        public int Command { get; private set; }
        public int PayloadIndex { get; private set; }
        public ICommandHandler PacketHandler { get; private set; }

        public Packet(Server server, int packetSize, byte[] buffer, IPEndPoint remoteEp)
        {
            Server = server;

            Data = new byte[packetSize];
            Buffer.BlockCopy(buffer, 0, Data, 0, packetSize);

            RemoteEp = remoteEp;

            ParseHeader();
        }

        private void ParseHeader()
        {
            var index = 0;
            Id = BitConverter.ToInt32(Data, index); index += sizeof(int);
            Time = BitConverter.ToSingle(Data, index); index += sizeof(float);

            // if 6th bit is set then this is a reliable packet, else it's not
            IsReliable = Utilities.Utilities.IsBitSet(Data[index], 7);

            // remove first two bits
            Command = 0x7f & BitConverter.ToInt32(Data, index); index += sizeof(int);
            // rest is payload
            PayloadIndex = index;

            PacketHandler = Utilities.Utilities.GetHandler(this);
            PacketHandler.Init(this);
        }
    }
}