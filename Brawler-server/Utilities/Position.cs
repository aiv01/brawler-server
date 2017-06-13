using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlerServer.Utilities
{
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

        public void SetPosition(float X, float Y, float Z)
        {
            this.X = X;
            this.X = Y;
            this.X = Z;
        }

        public override string ToString()
        {
            return String.Format($"pos[x: {this.X}, y: {this.Y}, z: {this.Z}]");
        }
    }

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

        public void SetRotation(float Rx, float Ry, float Rz, float Rw)
        {
            this.Rx = Rx;
            this.Ry = Ry;
            this.Rz = Rz;
            this.Rw = Rw;
        }
    }
}
