using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using BrawlerServer.Utilities;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BrawlerServer.Server
{
    public class AuthHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public Json.AuthHandler JsonData { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public Client Client { get; private set; }

        public void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.AuthHandler));

            // check if remoteEp is already authed
            if (packet.Server.HasClient(packet.RemoteEp))
            {
                Client = packet.Server.GetClientFromEndPoint(packet.RemoteEp);
                if (Client.authed)
                {
                    throw new Exception($"[{packet.Server.Time}] {Client} tried to authenticate but has already authenticated.");
                }
            }
            
            Logs.Log($"[{packet.Server.Time}] Received Auth token '{JsonData.AuthToken}' from '{packet.RemoteEp}'.");

            Dictionary<string, string> requestValues = new Dictionary<string, string>
            {
                { "token", JsonData.AuthToken },
                { "ip", packet.RemoteEp.Address.ToString() }
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(requestValues);
            packet.Server.AddAsyncRequest(AsyncRequest.RequestMethod.POST, "http://taiga.aiv01.it/players/server-auth/", packet.RemoteEp, AsyncRequest.RequestType.Authentication, content);

        }

        public static void HandleResponse(Json.AuthPlayerPost JsonAuthPlayer, IPEndPoint remoteEp, Server server)
        {
            if (JsonAuthPlayer.auth_ok)
            {
                Client Client = new Client(remoteEp);
                Client.SetName(JsonAuthPlayer.nickname);
                Client.HasAuthed(true);
                Logs.Log($"[{server.Time}] {Client} successfully authed");

                Json.ClientAuthed JsonClientAuthed = new Json.ClientAuthed()
                {
                    Ip = remoteEp.Address.ToString(),
                    Port = remoteEp.Port.ToString()
                };
                string JsonClientAuthedData = JsonConvert.SerializeObject(JsonClientAuthed);
                byte[] data = new byte[512];
                Packet ClientAuthedPacket = new Packet(server, data.Length, data, remoteEp);
                ClientAuthedPacket.AddHeaderToData(true, Commands.ClientAuthed);
                ClientAuthedPacket.Writer.Write(JsonClientAuthedData);
                ClientAuthedPacket.Server.SendPacket(ClientAuthedPacket);
            }
            else
            {
                Logs.Log($"[{server.Time}] Client with remoteEp '{remoteEp}' failed to authenticate: ({JsonAuthPlayer.fields}: {JsonAuthPlayer.info})");
            }
        }
    }
}