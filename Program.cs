using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Brawler_server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test Server Running!");

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(args[0]), Convert.ToInt32(args[1])));

            var buffer = new byte[1024];
            EndPoint remoteEp = new IPEndPoint(0, 0);
            while (true)
            {
                socket.ReceiveFrom(buffer, ref remoteEp);
                socket.SendTo(buffer, remoteEp);
                Thread.Sleep(100);
            }
        }
    }
}