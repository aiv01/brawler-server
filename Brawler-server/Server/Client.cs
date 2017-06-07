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
        public uint TimeLastPacketSent { get; set; }

        public class Position
        {
            public float X;
            public float Y;
            public float Z;

            public Position(float X, float Y, float Z)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
            }
        }
        public Position position { get; private set; }
        public class Rotation
        {
            public float Rx;
            public float Ry;
            public float Rz;
            public float Rw;
            public Rotation(float Rx, float Ry, float Rz, float Rw)
            {
                this.Rx = Rx;
                this.Ry = Ry;
                this.Rz = Rz;
                this.Rw = Rw;
            }
        }
        public Rotation rotation { get; private set; }

        public int characterId { get; private set; }

        public Client(uint id, IPEndPoint endPoint)
        {
            Id = id;
            EndPoint = endPoint;

            this.position = new Position(0, 0, 0);
            this.rotation = new Rotation(0, 0, 0, 0);
        }

        public Client(IPEndPoint endPoint) : this(Utilities.Utilities.GetClientId(), endPoint) { }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return $"client:[name:'{Name}', endPoint:'{EndPoint}', id:'{Id}']";
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


        public void SetPosition(Position position)
        {
            SetPosition(position.X, position.Y, position.Z);
        }

        public void SetPosition(float x, float y, float z)
        {
            this.position.X = x;
            this.position.Y = y;
            this.position.Z = z;
        }


        public void SetRotation(Rotation rotation)
        {
            SetRotation(rotation.Rx, rotation.Ry, rotation.Rz, rotation.Rw);
        }

        public void SetRotation(float x, float y, float z, float w)
        {
            this.rotation.Rx = x;
            this.rotation.Ry = y;
            this.rotation.Rz = z;
            this.rotation.Rw = w;
        }


        public void SetCharacterId(int prefabId)
        {
            this.characterId = prefabId;
        }
    }
}
