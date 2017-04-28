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
        public HttpResponseMessage Response { get; private set; }
        public string ResponseString { get; private set; }
        public Json.AuthPlayerPost JsonAuthPlayer { get; private set; }

        public async void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(Json.AuthHandler));

            Response = new HttpResponseMessage(HttpStatusCode.Continue);


            // check if remoteEp is already authed
            if (packet.Server.CheckAuthedEndPoint(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' tried to authenticate but has already authenticated.");
            }
            // check if remoteEp has already joined 
            if (packet.Server.HasClient(packet.RemoteEp))
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' tried to join but has never authenticated.");
            }

            Logs.Log($"[{packet.Server.Time}] Received Auth token from '{packet.RemoteEp}'.");
            
            Dictionary<string, string> requestValues = new Dictionary<string, string>
            {
                { "token", JsonData.AuthToken },
                { "ip", packet.RemoteEp.Address.ToString() }
            };
            FormUrlEncodedContent content = new FormUrlEncodedContent(requestValues);
            Response = await packet.Server.HttpClient.PostAsync("http://taiga.aiv01.it/players/server-auth/", content);
            ResponseString = await Response.Content.ReadAsStringAsync();

            JsonAuthPlayer = Json.Deserialize(ResponseString, typeof(Json.AuthPlayerPost));

            if (JsonAuthPlayer.auth_ok)
            {
                EndPoint = packet.RemoteEp;
                Client = new Client(EndPoint);
                Client.SetName(JsonAuthPlayer.nickname);
                packet.Server.AddAuthedEndPoint(EndPoint, Client);
                Logs.Log($"[{packet.Server.Time}] Player with remoteEp '{packet.RemoteEp}' successfully authed");

                Json.ClientAuthed JsonClientAuthed = new Json.ClientAuthed()
                {
                    Ip = packet.RemoteEp.Address.ToString(),
                    Port = packet.RemoteEp.Port.ToString()
                };
                string JsonClientAuthedData = JsonConvert.SerializeObject(JsonClientAuthed);
                
                byte[] data = new byte[512];
                Packet ClientAuthedPacket = new Packet(packet.Server, data.Length, data, EndPoint);
                ClientAuthedPacket.AddHeaderToData(true, Commands.ClientAuthed);
                packet.Writer.Write(JsonClientAuthedData);
                packet.Server.SendPacket(ClientAuthedPacket);
            }
            else
            {
                throw new Exception($"Client with remoteEp '{packet.RemoteEp}' failed to authenticate ({JsonAuthPlayer.fields}: {JsonAuthPlayer.info})");
            }
        }
    }
}