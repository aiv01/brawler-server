using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BrawlerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Brawler Server started!\nHostname: {0}\nPort: {1}", args[0], args[1]);

            var bindEp = new IPEndPoint(IPAddress.Parse(args[0]), Convert.ToInt32(args[1]));

            var server = new Server.Server(bindEp);
            server.MainLoop();
        }
    }
}