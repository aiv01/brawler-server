using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;

namespace BrawlerServer.Server
{
    public class CommandHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public CommanderCmds Command { get; private set; }
        public string FullArguments { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            Command = (CommanderCmds)packet.Reader.ReadByte();
            FullArguments = packet.Reader.ReadString();
            string[] Arguments = FullArguments.Split(' ');
            
            Logs.Log($"[{packet.Server.Time}] Received Command {Command} from {packet.RemoteEp}.");

            if (Command == CommanderCmds.ForceArena)
            {
                if (packet.Server.mode == Server.ServerMode.Lobby)
                {
                    packet.Server.CheckPlayersReady(1, true);
                }
            }
            else if (Command == CommanderCmds.ForceLobby)
            {
                if (packet.Server.mode == Server.ServerMode.Battle)
                {
                    packet.Server.MovePlayersToLobby();
                }
            }
            else if (Command == CommanderCmds.Kick)
            {
                Client client = packet.Server.GetClientFromName(Arguments[0]);
                if (client == null)
                {
                    throw new Exception($"Client with name {Arguments[0]} was not found");
                }
                packet.Server.QueueRemoveClient(client);
            }
            else if (Command == CommanderCmds.SetHealth)
            {
                Client client = packet.Server.GetClientFromName(Arguments[0]);
                if (client == null)
                {
                    throw new Exception($"Client with name {Arguments[0]} was not found");
                }
                int health;
                int.TryParse(Arguments[1], out health);
                client.SetHealth(health);
            }
            else if (Command == CommanderCmds.SetFury)
            {
                Client client = packet.Server.GetClientFromName(Arguments[0]);
                if (client == null)
                {
                    throw new Exception($"Client with name {Arguments[0]} was not found");
                }
                int fury;
                int.TryParse(Arguments[1], out fury);
                client.SetFury(fury);
            }
        }
    }
}