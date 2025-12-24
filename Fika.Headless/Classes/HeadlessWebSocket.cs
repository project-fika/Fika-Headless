using System;
using System.Threading.Tasks;
using BepInEx.Logging;
using Diz.Utils;
using Fika.Core.Networking.Websocket;
using Fika.Core.Networking.Websocket.Headless;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using WebSocketSharp;

namespace Fika.Headless.Classes;

public class HeadlessWebSocket
{
    private static readonly ManualLogSource _logger = Logger.CreateLogSource("Fika.HeadlessWebSocket");

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

    private readonly WebSocket _webSocket;
    private int _attempts = 1;

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
        _logger.LogInfo($"Attempting to connect to {Url}...");
        _webSocket.Connect();
        _attempts++;
    }

    public void Close()
    {
        _webSocket.Close();
    }

    private void WebSocket_OnOpen(object sender, EventArgs e)
    {
        _logger.LogMessage("Connected to HeadlessWebSocket");
        _attempts = 1;
    }

    private void WebSocket_OnMessage(object sender, MessageEventArgs e)
    {
#if DEBUG
        _logger.LogInfo($"Received message"); 
#endif

        if (e == null)
        {
            _logger.LogWarning("WebSocket_OnMessage:: EventArgs was null");
            return;
        }

        if (string.IsNullOrEmpty(e.Data))
        {
            _logger.LogWarning("WebSocket_OnMessage:: Data was null");
            return;
        }

        var jsonObject = JObject.Parse(e.Data);

        if (!jsonObject.ContainsKey("Type"))
        {
            _logger.LogWarning("WebSocket_OnMessage:: There was no type in the data");
            return;
        }

        var type = (EFikaHeadlessWSMessageType)Enum.Parse(typeof(EFikaHeadlessWSMessageType), jsonObject.Value<string>("Type"));
        switch (type)
        {
            case EFikaHeadlessWSMessageType.HeadlessStartRaid:
                var data = JsonConvert.DeserializeObject<StartRaid>(e.Data);

                AsyncWorker.RunInMainTread(() => FikaHeadlessPlugin.Instance.OnFikaStartRaid(data.StartHeadlessRequest));
                break;
            case EFikaHeadlessWSMessageType.ShutdownClient:
                AsyncWorker.RunInMainTread(Application.Quit);
                break;
            case EFikaHeadlessWSMessageType.KeepAlive:
            case EFikaHeadlessWSMessageType.RequesterJoinRaid:
                break;
        }
    }

    private void WebSocket_OnError(object sender, ErrorEventArgs e)
    {
        _logger.LogInfo($"HeadlessWebSocket error: {e.Message}");
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
        if (_attempts > 15)
        {
            _logger.LogError("Took more than 15 attempts to connect to the websocket, quitting...");
            AsyncWorker.RunInMainTread(Application.Quit);
            return;
        }
        _logger.LogWarning($"Websocket connection lost, retrying... Attempt {_attempts}/15");

        await Task.Delay(5000);
        Connect();
    }
}
