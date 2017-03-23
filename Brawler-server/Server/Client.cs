using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BrawlerServer.Server
{
    public class Client
    {
        public IPEndPoint EndPoint { get; private set; }
        public string Name { get; private set; }

        public Client(IPEndPoint endPoint, string name)
        {
            EndPoint = endPoint;
            Name = name;
        }

        public override string ToString()
        {
            return $"client:[name:'{Name}', endPoint:'{EndPoint}']";
        }
    }
}
