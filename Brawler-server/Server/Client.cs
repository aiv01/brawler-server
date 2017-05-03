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
        public uint Id { get; private set; }
        public long TimeLastPacketSent { get; set; }

        public Client(uint id, IPEndPoint endPoint)
        {
            Id = id;
            EndPoint = endPoint;
        }

        public Client(IPEndPoint endPoint) : this(Utilities.Utilities.GetClientId(), endPoint) { }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return $"client:[name:'{Name}', endPoint:'{EndPoint}']";
        }

        protected bool Equals(Client other)
        {
            return Equals(EndPoint, other.EndPoint) && string.Equals(Name, other.Name) && Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Client;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EndPoint != null ? EndPoint.GetHashCode() : 0) * 397) ^
                    (Id.GetHashCode() * 397) ^
                    (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}
