using System;
using System.IO;
using System.Net;

namespace BrawlerServer.Server
{
    public class Packet
    {
        public Server Server { get; private set; }

        public byte[] Data { get; private set; }
        public MemoryStream Stream { get; private set; }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }
        public IPEndPoint RemoteEp { get; private set; }
        public bool Broadcast { get; set; }
        // header
        public uint Id { get; private set; }
        public float Time { get; private set; }
        public bool IsReliable { get; private set; }
        public byte Command { get; private set; }
        public int PayloadOffset { get; private set; }
        public ICommandHandler PacketHandler { get; private set; }
        public int PacketSize { get; set; }

        public Packet(Server server, int packetSize, byte[] buffer, IPEndPoint remoteEp, MemoryStream stream,
            BinaryReader reader, BinaryWriter writer)
        {
            Server = server;

            PacketSize = packetSize;
            Data = buffer;
            Stream = stream;
            Reader = reader;
            Writer = writer;

            RemoteEp = remoteEp;
        }

        public Packet(Server server, int packetSize, byte[] buffer, IPEndPoint remoteEp)
        {
            Server = server;

            PacketSize = packetSize;
            Data = buffer;
            Stream = new MemoryStream(Data);
            Reader = new BinaryReader(Stream);
            Writer = new BinaryWriter(Stream);

            RemoteEp = remoteEp;
        }

        public void AddHeaderToData(bool reliable, byte command)
        {
            AddHeaderToData(Utilities.Utilities.GetPacketId(), reliable, command);
        }

        public void AddHeaderToData(uint id, bool reliable, byte command)
        {
            if (command > 127)
            {
                throw new Exception("Command can NOT be higher than 127.");
            }

            Stream.Seek(0, SeekOrigin.Begin);

            Id = id;
            IsReliable = reliable;
            Command = command;
            Time = Server.Time / 1000f;

            Writer.Write(id);
            Writer.Write(Time);
            byte infoByte = command;
            infoByte = Utilities.Utilities.SetBitOnByte(infoByte, 7, reliable);
            Writer.Write(infoByte);

            PayloadOffset = (int) Stream.Position;
        }

        public void ParseHeaderFromData(bool initHandler = true)
        {
            Stream.Seek(0, SeekOrigin.Begin);

            Id = Reader.ReadUInt32();
            Time = Reader.ReadSingle();

            // if 6th bit is set then this is a reliable packet, else it's not
            IsReliable = Utilities.Utilities.IsBitSet(Reader.ReadByte(), 7);
            Stream.Seek(-1, SeekOrigin.Current);

            // remove first two bits
            Command = Reader.ReadByte();
            if (IsReliable)
            {
                Command = Utilities.Utilities.SetBitOnByte(Command, 7, false);
            }
            // rest is payload
            PayloadOffset = (int) Stream.Position;

            PacketHandler = Utilities.Utilities.GetHandler(this);
            if (initHandler)
            {
                PacketHandler.Init(this);
            }
        }
    }
}