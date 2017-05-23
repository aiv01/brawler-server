using System;
using System.IO;
using System.Net;
using BrawlerServer.Utilities;

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
        public Commands Command { get; private set; }
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

        public void AddHeaderToData(bool reliable, Commands command)
        {
            AddHeaderToData(Utilities.Utilities.GetPacketId(), reliable, command);
        }

        public void AddHeaderToData(uint id, bool reliable, Commands command)
        {
            Stream.Seek(0, SeekOrigin.Begin);

            Id = id;
            IsReliable = reliable;
            Command = command;
            Time = Server.Time / 1000f;

            Writer.Write(id);
            Writer.Write(Time);
            byte infoByte = (byte) command;
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
            Command = (Commands) Reader.ReadByte();
            if (IsReliable)
            {
                Command = (Commands) Utilities.Utilities.SetBitOnByte((byte) Command, 7, false);

                byte[] ackData = new byte[512];
                Packet ackPacket = new Packet(this.Server, ackData.Length, ackData, this.RemoteEp);
                ackPacket.AddHeaderToData(Utilities.Utilities.GetPacketId(), false, Commands.Ack);
                ackPacket.Stream.Seek(ackPacket.PayloadOffset, SeekOrigin.Begin);
                ackPacket.Writer.Write(this.Id);
                this.Server.SendPacket(ackPacket);
            }
            // rest is payload
            PayloadOffset = (int) Stream.Position;

            PacketHandler = Utilities.Utilities.GetHandler(this);
            if (initHandler && PacketHandler != null)
            {
                PacketHandler.Init(this);
            }
        }

        public override string ToString()
        {
            return $"packet:[Timestamp:'{Time}', Id:'{Id}', Bc:'{Broadcast}', ack:'{IsReliable}', command:'{Command}']";
        }
    }
}