using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using BrawlerServer.Utilities;
using System.Threading.Tasks;

namespace BrawlerServer.Server
{
    public class AuthHandlerJson
    {
        public string AuthToken;
    }

    public class AuthHandler : ICommandHandler
    {
        public Packet Packet { get; private set; }
        public AuthHandlerJson JsonData { get; private set; }
        public IPEndPoint EndPoint { get; private set; }
        public HttpResponseMessage response { get; private set; }
        public string responseString { get; private set; }

        public async void Init(Packet packet)
        {
            Packet = packet;

            JsonData = Utilities.Utilities.ParsePacketJson(packet, typeof(AuthHandlerJson));

            response = new HttpResponseMessage(HttpStatusCode.Continue);


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

            //TODO Connect to webService and check auth token
            Dictionary<string, string> requestValues = new Dictionary<string, string>();
            requestValues.Add("token", JsonData.AuthToken);
            requestValues.Add("ip", packet.RemoteEp.Address.ToString());
            FormUrlEncodedContent content = new FormUrlEncodedContent(requestValues);
            response = await packet.Server.HttpClient.PostAsync("http://taiga.aiv01.it/players/server-auth/", content);
            responseString = await response.Content.ReadAsStringAsync();
            Debug.Write(responseString);

            EndPoint = packet.RemoteEp;
            packet.Server.AddAuthedEndPoint(EndPoint);
            Logs.Log($"[{packet.Server.Time}] Player with remoteEp '{packet.RemoteEp}' successfully authed");
        }
    }
}