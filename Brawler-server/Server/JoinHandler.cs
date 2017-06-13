﻿using System;
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
            //check if client has authed
            if (!packet.Server.CheckAuthedEndPoint(packet.RemoteEp))
            {
                throw new Exception($"RemoteEp '{packet.RemoteEp}' tried to join but has not authenticated.");
            }
            // first check if user is already in joined users
            if (packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"{packet.Server.GetClientFromAuthedEndPoint(packet.RemoteEp)} tried to join but has already joined.");
            }
            // create client and add it to the server's clients
            Client = packet.Server.GetClientFromAuthedEndPoint(packet.RemoteEp);
            Logs.Log($"[{packet.Server.Time}] Received join message from {Client}.");

            //Check if client was already in
            Client alreadyIn = packet.Server.GetClientFromName(Client.Name);
            if (alreadyIn != null)
            {
                packet.Server.QueueRemoveClient(alreadyIn.EndPoint, "joined from another location");
            }
            packet.Server.AddClient(Client);

            Json.ClientJoined jsonDataObject = new Json.ClientJoined {
                Name = Client.Name,
                Id = Client.Id
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

            //Set last packet sent as this one
            Client.TimeLastPacketSent = packet.Server.Time;

            JsonSerialized = JsonConvert.SerializeObject(JsonData);
        }
    }
}