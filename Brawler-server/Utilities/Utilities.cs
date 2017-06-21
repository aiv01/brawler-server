using System;
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
        Dodge = 6,
        Taunt = 8,
        LightAttack = 10,
        HeavyAttack = 12,
        Hit = 14,
        SwapWeapon = 16,
        Command = 98,
        Ready = 117,
        NotReady = 119,
        Chat = 123,
        Auth = 125,

        // server -> client
        ClientJoined = 1,
        ClientLeft = 3,
        ClientMoved = 5,
        ClientDodged = 7,
        ClientTaunted = 9,
        ClientLightAttacked = 11,
        ClientHeavyAttacked = 13,
        ClientHitted = 15,
        ClientSwappedWeapon = 16,
        ClientCommanded = 99,
        EnterArena = 115,
        ExitArena = 116,
        ClientReady = 118,
        ClientNotReady = 120,
        ClientChatted = 124,
        ClientAuthed = 126,

        //Both Ways
        Ping = 121,
        Pong = 122,
        Ack = 127,

        //Others
        Empower = 100,
    }

    public enum CommanderCmds : byte
    {
        Ping = 0,
        ForceArena = 1,
        ForceLobby = 2,
        Kick = 3,
        SetHealth,
        SetFury,
        SendMessage
    }

    public static class Utilities
    {
        private static uint PacketId = 0;
        private static uint ClientId = 0;
        private static int RoomId = 0;

        // handlers per command (the array index is the command)
        private static readonly Dictionary<Commands, Type> Handlers = new Dictionary<Commands, Type> {
            { Commands.Join, typeof(JoinHandler) },
            { Commands.Leave, typeof(LeaveHandler) },
            { Commands.Move, typeof(MoveHandler) },
            { Commands.Ack, typeof(ACKHandler) },
            { Commands.Auth, typeof(AuthHandler) },
            { Commands.Chat, typeof(ChatHandler) },
            { Commands.Dodge, typeof(DodgeHandler) },
            { Commands.Taunt, typeof(TauntHandler) },
            { Commands.LightAttack, typeof(LightAttackHandler) },
            { Commands.HeavyAttack, typeof(HeavyAttackHandler) },
            { Commands.Ping, typeof(PingHandler) },
            { Commands.Pong, typeof(PongHandler) },
            { Commands.Ready, typeof(ReadyHandler) },
            { Commands.NotReady, typeof(NotReadyHandler) },
            { Commands.Command, typeof(CommandHandler) },
            { Commands.SwapWeapon, typeof(SwapWeaponHandler) },
            { Commands.Empower, typeof(EmpowerHandler) },

        };

        public static ICommandHandler GetHandler(Packet packet)
        {
            ICommandHandler result = null;
            Type commandHandlerType;
            if (Handlers.TryGetValue(packet.Command, out commandHandlerType))
            {
                result = (ICommandHandler)Activator.CreateInstance(commandHandlerType);
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

        public static int GetRoomId()
        {
            return RoomId++;
        }
    }
}
