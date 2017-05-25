using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BrawlerServer.Server;
using BrawlerServer.Utilities;

namespace BrawlerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Logs.Level = Logs.DebugLevel.Full;

            Logs.Log($"Brawler Server started!\nHostname: {args[0]}\nPort: {args[1]}");

            var bindEp = new IPEndPoint(IPAddress.Parse(args[0]), Convert.ToInt32(args[1]));

            var server = new Server.Server(bindEp);
            server.Bind();
            server.MainLoop();
        }
    }
}