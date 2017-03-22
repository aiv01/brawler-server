using System;
using System.Collections.Generic;
using System.Text;
using BrawlerServer.Server;
using Newtonsoft.Json;

namespace BrawlerServer.Utilities
{
    public static class Utilities
    {
        // handlers per command (the array index is the command)
        private static readonly Dictionary<int, Type> Handlers = new Dictionary<int, Type> {
            { 0, typeof(JoinHandler) }
        };

        public static ICommandHandler GetHandler(Packet packet)
        {
            return (ICommandHandler)Activator.CreateInstance(Handlers[packet.Command]);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static dynamic ParsePacketJson(Packet packet, Type type)
        {
            var jsonData = Encoding.UTF8.GetString(packet.Data, packet.PayloadIndex,
                packet.Data.Length - packet.PayloadIndex);
            return JsonConvert.DeserializeObject(jsonData, type);
        }
    }
}
