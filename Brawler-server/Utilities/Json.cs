﻿using System;
using Newtonsoft.Json;

namespace BrawlerServer.Utilities
{
    public static class Json
    {
        public class JoinHandler
        {
            
        }

        public class LeaveHandler
        {
            
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

        public class AuthHandler
        {
            public string AuthToken;
        }

        public class ClientJoined
        {
            public uint Id;
            public string Name;
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

        public class AuthPlayerPost
        {
            public bool auth_ok;
            public string nickname;
            public string fields;
            public string info;
        }

        public class ChatHandler
        {
            public string Text;
        }

        public static dynamic Deserialize(string data, Type type)
        {
            return JsonConvert.DeserializeObject(data, type);
        }
    }
}
