using BepInEx.Logging;
using Diz.Utils;
using EFT;
using Fika.Core.Networking.Websocket;
using Fika.Core.Networking.Websocket.Headless;
using Fika.Headless;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using SPT.Common.Utils;
using System;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Fika.Core.Networking
{
    public class HeadlessWebSocket
    {
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Fika.HeadlessWebSocket");

        public string Host { get; set; }
        public string Url { get; set; }
        public string SessionId { get; set; }
        public bool Connected
        {
            get
            {
                return _webSocket.ReadyState == WebSocketState.Open;
            }
        }

        private WebSocket _webSocket;

        public HeadlessWebSocket()
        {
            Host = RequestHandler.Host.Replace("http", "ws");
            SessionId = RequestHandler.SessionId;
            Url = $"{Host}/fika/headless/client";

            _webSocket = new WebSocket(Url)
            {
                WaitTime = TimeSpan.FromMinutes(1),
                EmitOnPing = true
            };

            _webSocket.SetCredentials(SessionId, "", true);

            _webSocket.OnOpen += WebSocket_OnOpen;
            _webSocket.OnMessage += WebSocket_OnMessage;
            _webSocket.OnError += WebSocket_OnError;
            _webSocket.OnClose += WebSocket_OnClose;
        }

        public void Connect()
        {
            logger.LogInfo($"Attempting to connect to {Url}...");
            _webSocket.Connect();
        }

        public void Close()
        {
            _webSocket.Close();
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            logger.LogInfo("Connected to HeadlessWebSocket");
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
#if DEBUG
            logger.LogInfo($"Received message"); 
#endif

            if (e == null)
            {
                logger.LogWarning("WebSocket_OnMessage:: EventArgs was null");
                return;
            }

            if (string.IsNullOrEmpty(e.Data))
            {
                logger.LogWarning("WebSocket_OnMessage:: Data was null");
                return;
            }

            JObject jsonObject = JObject.Parse(e.Data);

            if (!jsonObject.ContainsKey("Type"))
            {
                logger.LogWarning("WebSocket_OnMessage:: There was no type in the data");
                return;
            }

            logger.LogInfo(JsonConvert.SerializeObject(e.Data));

            EFikaHeadlessWSMessageTypes type = (EFikaHeadlessWSMessageTypes)Enum.Parse(typeof(EFikaHeadlessWSMessageTypes), jsonObject.Value<string>("Type"));

            switch (type)
            {
                case EFikaHeadlessWSMessageTypes.HeadlessStartRaid:
                    StartRaid data = JsonConvert.DeserializeObject<StartRaid>(e.Data);
                    logger.LogInfo(JsonConvert.SerializeObject(data));

                    AsyncWorker.RunInMainTread(() =>
                    {
                        FikaHeadlessPlugin.Instance.OnFikaStartRaid(data.StartHeadlessRequest);
                    });
                    break;
            }
        }

        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            logger.LogInfo($"HeadlessWebSocket error: {e.Message}");
        }

        private void WebSocket_OnClose(object sender, CloseEventArgs closeEventArgs)
        {
            if (!closeEventArgs.WasClean)
            {
                Task.Run(RetryConnect);
            }
        }

        private async void RetryConnect()
        {
            logger.LogWarning($"Websocket connection lost, retrying...");

            await Task.Delay(5000);
            Connect();
        }
    }
}
