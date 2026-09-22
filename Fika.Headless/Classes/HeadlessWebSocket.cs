using System;
using System.Text.Json;
using System.Threading.Tasks;
using BepInEx.Logging;
using Fika.Core.Networking.Http;
using Fika.Core.Networking.Websocket;
using Fika.Core.Networking.Websocket.Headless;
using SPTushonka.Common.Http;

namespace Fika.Headless.Classes;

public class HeadlessWebSocket
{
    private static readonly ManualLogSource _logger = Logger.CreateLogSource("Fika.HeadlessWebSocket");

    // The server writes the nested EFT structs in camelCase. Their interop fields are PascalCase and lost the Newtonsoft names.
    internal static readonly JsonSerializerOptions RequestOptions = new(FikaJson.Options) { PropertyNameCaseInsensitive = true };

    public string Host { get; set; }
    public string Url { get; set; }
    public string SessionId { get; set; }
    public bool Connected
    {
        get
        {
            return _webSocket.IsOpen;
        }
    }

    private readonly FikaWebSocket _webSocket;
    private int _attempts = 1;
    private bool _closing;

    public HeadlessWebSocket()
    {
        Host = RequestHandler.Host.Replace("http", "ws");
        SessionId = RequestHandler.SessionId;
        Url = $"{Host}/fika/headless/client";

        _webSocket = new FikaWebSocket(Url, SessionId, TimeSpan.FromMinutes(1));
        _webSocket.Opened += WebSocket_OnOpen;
        _webSocket.Errored += WebSocket_OnError;
        _webSocket.Closed += WebSocket_OnClose;
        _webSocket.MessageReceived += data => MainThread.Post(() => WebSocket_OnMessage(data));
    }

    public void Connect()
    {
        _logger.LogInfo($"Attempting to connect to {Url}...");
        _closing = false;
        _webSocket.Connect();
        _attempts++;
    }

    public void Close()
    {
        _closing = true;
        _webSocket.Close();
    }

    private void WebSocket_OnOpen()
    {
        _logger.LogMessage("Connected to HeadlessWebSocket");
        _attempts = 1;
    }

    private void WebSocket_OnMessage(string data)
    {
#if DEBUG
        _logger.LogInfo("Received message");
#endif

        if (string.IsNullOrEmpty(data))
        {
            _logger.LogWarning("WebSocket_OnMessage:: Data was null");
            return;
        }

        using var document = JsonDocument.Parse(data);
        if (!document.RootElement.TryGetProperty("Type", out var typeElement))
        {
            _logger.LogWarning("WebSocket_OnMessage:: There was no type in the data");
            return;
        }

        var type = typeElement.ValueKind == JsonValueKind.Number
            ? (EFikaHeadlessWSMessageType)typeElement.GetInt32()
            : Enum.Parse<EFikaHeadlessWSMessageType>(typeElement.GetString());

        switch (type)
        {
            case EFikaHeadlessWSMessageType.HeadlessStartRaid:
                var startRaid = JsonSerializer.Deserialize<StartRaid>(data, RequestOptions);
                FikaHeadlessPlugin.Instance.OnFikaStartRaid(startRaid.StartHeadlessRequest);
                break;
            case EFikaHeadlessWSMessageType.ShutdownClient:
                Application.Quit();
                break;
            case EFikaHeadlessWSMessageType.KeepAlive:
            case EFikaHeadlessWSMessageType.RequesterJoinRaid:
                break;
        }
    }

    private void WebSocket_OnError(string message)
    {
        _logger.LogInfo($"HeadlessWebSocket error: {message}");
    }

    private void WebSocket_OnClose()
    {
        if (!_closing)
        {
            MainThread.Post(() => _ = RetryConnect());
        }
    }

    private async Task RetryConnect()
    {
        if (_attempts > 15)
        {
            _logger.LogError("Took more than 15 attempts to connect to the websocket, quitting...");
            Application.Quit();
            return;
        }
        _logger.LogWarning($"Websocket connection lost, retrying... Attempt {_attempts}/15");

        await Task.Delay(5000);
        Connect();
    }
}
