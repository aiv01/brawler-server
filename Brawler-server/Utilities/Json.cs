using System;
using Newtonsoft.Json;

namespace BrawlerServer.Utilities
{
    public static class Json
    {
        public class JoinHandler
        {
            public int MatchId;
        }

        public class LeaveHandler
        {
            
        }

        public class CommandHandler
        {
            public string command;
        }

        public class MoveHandler
        {
            public byte MoveType;
            public float X;
            public float Y;
            public float Z;
            public float Rx;
            public float Ry;
            public float Rz;
            public float Rw;
        }

        public class DodgeHandler
        {
            public float X;
            public float Y;
            public float Z;
            public float Rx;
            public float Ry;
            public float Rz;
            public float Rw;
        }

        public class TauntHandler
        {
            public float X;
            public float Y;
            public float Z;
            public float Rx;
            public float Ry;
            public float Rz;
            public float Rw;
            public byte TauntId;
        }

        public class LightAttackHandler
        {
            public float X;
            public float Y;
            public float Z;
            public float Rx;
            public float Ry;
            public float Rz;
            public float Rw;
        }

        public class HeavyAttackHandler
        {
            public float X;
            public float Y;
            public float Z;
            public float Rx;
            public float Ry;
            public float Rz;
            public float Rw;
        }

        public class AuthHandler
        {
            public string AuthToken;
        }

        public class ReadyHandler
        {
            public int PrefabId;
        }

        public class EnterArena
        {
            public uint Id;
            public float X;
            public float Y;
            public float Z;
            public float Rx;
            public float Ry;
            public float Rz;
            public float Rw;
        }

        public class ClientJoined
        {
            public bool CanJoin;
            public string Reason;
            public uint Id;
            public string Name;
            public bool IsReady;
        }

        public class ClientLeft
        {
            public uint Id;
            public string Reason;
        }

        public class ClientAuthed
        {
            public string Ip;
            public string Port;
        }

        public class ClientReady
        {
            public int PrefabId;
            public uint Id;
        }

        public class ClientNotReady
        {
            public int PrefabId;
            public uint Id;
        }

        public class ChatHandler
        {
            public string Text;
        }

        public class ClientChatted
        {
            public string Text;
            public string Name;
        }
        
        public class AuthPlayerPost
        {
            public bool auth_ok;
            public string nickname;
            public string fields;
            public string info;
        }

        public class InfoToServicesPost
        {
            public bool server_register;
            public string fields;
            public string info;
        }

        public static dynamic Deserialize(string data, Type type)
        {
            return JsonConvert.DeserializeObject(data, type);
        }
    }
}
