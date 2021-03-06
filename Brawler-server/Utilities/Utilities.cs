﻿using System;
using System.Collections.Generic;
using System.Text;
using BrawlerServer.Server;
using Newtonsoft.Json;

namespace BrawlerServer.Utilities
{
    public enum Commands : byte
    {
        // client -> server
        Join = 0,
        Leave = 2,
        Move = 4,
        // server -> client
        ClientJoined = 1,
        ClientLeft = 3,
        ClientMoved = 5
    }

    public static class Utilities
    {
        private static uint PacketId = 0;
        private static uint ClientId = 0;

        // handlers per command (the array index is the command)
        private static readonly Dictionary<Commands, Type> Handlers = new Dictionary<Commands, Type> {
            { Commands.Join, typeof(JoinHandler) },
            //{ 1, typeof(KickHandler) },
            { Commands.Leave, typeof(LeaveHandler) },
            { Commands.Move, typeof(MovedHandler) },

        };

        public static ICommandHandler GetHandler(Packet packet)
        {
            ICommandHandler result = null;
            Type commandHandlerType;
            if (Handlers.TryGetValue(packet.Command, out commandHandlerType))
            {
                result = (ICommandHandler) Activator.CreateInstance(commandHandlerType);
            }
            return result;
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
