using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;

namespace BrawlerServer.Server
{
    public class CommandHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public CommanderCmds Command { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Command = (CommanderCmds)packet.Reader.ReadByte();
            
            Logs.Log($"[{packet.Server.Time}] Received Command {Command} from {packet.RemoteEp}.");

            if (Command == CommanderCmds.ForceArena)
            {
                if (packet.Server.mode == Server.ServerMode.Lobby)
                {
                    packet.Server.CheckPlayersReady(1, true);
                }
            }
        }
    }
}