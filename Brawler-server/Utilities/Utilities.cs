using System;
using System.Collections.Generic;
using System.Text;
using BrawlerServer.Server;
using Newtonsoft.Json;

namespace BrawlerServer.Utilities
{
    public enum Commands : byte
    {
        Join,
        ClientJoined,
        Leave,
        ClientLeft,
        Update,
        ClientMoved
    }

    public static class Utilities
    {
        private static uint PacketId = 0;
        private static uint ClientId = 0;

        // handlers per command (the array index is the command)
        private static readonly Dictionary<int, Type> Handlers = new Dictionary<int, Type> {
            { 0, typeof(JoinHandler) },
            //{ 1, typeof(KickHandler) },
            { 2, typeof(LeaveHandler) },
            { 3, typeof(UpdateHandler) },

        };

        public static ICommandHandler GetHandler(Packet packet)
        {
            return (ICommandHandler)Activator.CreateInstance(Handlers[packet.Command]);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static byte SetBitOnByte(byte b, int pos, bool value)
        {
            return value ? (byte)(b | (1 << pos)) : (byte)(b & ~(1 << pos));
        }

        public static dynamic ParsePacketJson(Packet packet, Type type)
        {
            var jsonData = packet.Reader.ReadString();
            return JsonConvert.DeserializeObject(jsonData, type);
        }

        public static string ComposePacketJson(object jsonObject)
        {
            return JsonConvert.SerializeObject(jsonObject);
        }

        public static dynamic GetCommandFromId(sbyte id)
        {
            return Handlers[id];
        }

        //TODO
        //public static dynamic GetIdFromCommand(Type type)
        //{
        //    return Handlers[type];
        //}

        public static uint GetPacketId()
        {
            return PacketId++;
        }

        public static uint GetClientId()
        {
            return ClientId++;
        }
    }
}
