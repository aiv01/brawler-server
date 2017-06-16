using System;
using BrawlerServer.Utilities;
using Newtonsoft.Json;

namespace BrawlerServer.Server
{
    public class JoinHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Json.JoinHandler JsonData { get; private set; }
        public string JsonSerialized { get; private set; }
        public Client Client { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.JoinHandler));

            //Check if server is in lobby
            if (Packet.Server.mode != Server.ServerMode.Lobby)
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' tried to join but game has already started.");
            }
            //check if client is in the server
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' tried to join but has not entered.");
            }
            // create client and add it to the server's clients
            Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
            Logs.Log($"[{packet.Server.Time}] Received join message from {Client}.");

            //Check if Client has authed
            if (!Client.authed)
            {
                throw new Exception($"RemoteEp '{Client}' tried to join but has not authenticated.");
            }

            //Check if client was already in
            Client alreadyIn = packet.Server.GetClientFromName(Client.Name);
            if (alreadyIn != null)
            {
                packet.Server.QueueRemoveClient(alreadyIn.EndPoint, "joined from another location");
            }
            packet.Server.AddClient(Client);

            Room room = packet.Server.rooms[JsonData.MatchId];
            if (room == null)
            {
                throw new Exception($"room {JsonData.MatchId} does not exist");
            }

            string reason = "";
            bool canJoin = true;
            int playersInRoom = room.Clients.Count;
            if (playersInRoom == room.MaxPlayers)
            {
                canJoin = false;
                reason = "Room is full";
            }

            Json.ClientJoined jsonDataObject = new Json.ClientJoined {
                CanJoin = canJoin,
                Reason = reason,
                Name = Client.Name,
                Id = Client.Id,
                IsReady = Client.isReady,
            };
            string jsonData = JsonConvert.SerializeObject(jsonDataObject);

            Logs.Log($"[{packet.Server.Time}] Added new {Client}.");

            byte[] data = new byte[512];

            // send a broadcast clientJoined packet
            Packet packetClientAdded = new Packet(packet.Server, data.Length, data, null);
            packetClientAdded.AddHeaderToData(true, Commands.ClientJoined);
            packetClientAdded.Broadcast = true;
            packetClientAdded.Writer.Write(jsonData);
            packet.Server.SendPacket(packetClientAdded);

            Client.room = JsonData.MatchId;

            JsonSerialized = JsonConvert.SerializeObject(JsonData);
        }
    }
}