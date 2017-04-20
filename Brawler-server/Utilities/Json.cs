using System;
using Newtonsoft.Json;

namespace BrawlerServer.Utilities
{
    public static class Json
    {
        public class JoinHandler
        {

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

        public static dynamic Deserialize(string data, Type type)
        {
            return JsonConvert.DeserializeObject(data, type);
        }
    }
}
