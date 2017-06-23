﻿using System;
using BrawlerServer.Utilities;
using System.Net;

namespace BrawlerServer.Server
{
    public enum EmpowerType
    {
        ThumbUp,
        ThumbDown,
        Applause,
        Criticism
    }

    public class EmpowerHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Client ClientToEmpower { get; private set; }
        public Json.EmpowerHandler JsonData { get; private set; }
        public uint Id { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;
            var jsonData = packet.Reader.ReadString();
            Logs.LogWarning($"{jsonData}, {packet}, {packet.PayloadOffset}");
            packet.Stream.Seek(packet.PayloadOffset, System.IO.SeekOrigin.Begin);
            var stringa = "";
            for (int i = 0; i < packet.PacketSize; i++)
            {
                stringa += packet.Data[i] + " ";
            }
            Logs.LogWarning($"{stringa}");
            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.EmpowerHandler));

            //Check if client is connected
            if (!packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"'{packet.RemoteEp}' sent an empower but player doesn't exist.");
            }
            IPEndPoint ClientEP = new IPEndPoint(IPAddress.Parse(JsonData.Ip), JsonData.Port);
            ClientToEmpower = packet.Server.GetClientFromEndPoint(ClientEP);
            EmpowerType Empower = (EmpowerType)JsonData.EmpowerType;
            // Error converting value "EmpowerType" to type 'BrawlerServer.Utilities.Json+EmpowerHandler'. Path '', line 1, position 13.

            if (Empower == EmpowerType.ThumbUp)
                ClientToEmpower.AddFury(20);
            else if (Empower == EmpowerType.ThumbDown)
                ClientToEmpower.AddFury(-20);
            else if (Empower == EmpowerType.Applause)
                ClientToEmpower.AddFury(10);
            else if (Empower == EmpowerType.Criticism)
                ClientToEmpower.AddFury(-10);

            Logs.LogWarning($"[{packet.Server.Time}] Received Empower packet from {packet.RemoteEp} with {Empower}");

            packet.Server.CheckPlayersReady();
        }
    }
}
