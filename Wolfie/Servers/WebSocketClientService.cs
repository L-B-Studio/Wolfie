using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace Wolfie.Servers;

public record WsEnvelope(string Op, Dictionary<string, string> Data, int? Seq);

public sealed class WebSocketClientService : IAsyncDisposable
{
    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;

    private X509Certificate2 _pinnedCert;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private readonly string _serverIp = "141.105.132.149";
    private readonly int _port = 1234;

    public bool IsConnected => _ws?.State == WebSocketState.Open;

    public event Action<WsEnvelope> OnMessageReceived;
    public event Action<string> OnError;
    public event Action OnConnected;
    public event Action OnDisconnected;

    // ---------------- INIT ----------------

    public WebSocketClientService()
    {
        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();
    }

    // ---------------- CERT LOAD ----------------

    private async Task LoadCertAsync()
    {
        if (_pinnedCert != null) return;

        using var stream = await FileSystem.OpenAppPackageFileAsync("server.cer");
        using var ms = new MemoryStream();

        await stream.CopyToAsync(ms);

        _pinnedCert = new X509Certificate2(ms.ToArray());

        Debug.WriteLine("✅ Pinned certificate loaded");
    }

    // ---------------- CONNECT ----------------

    public async Task ConnectAsync(string accessToken)
    {
        if (IsConnected) return;

        await LoadCertAsync();

        _ws.Dispose();
        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        _ws.Options.RemoteCertificateValidationCallback = ValidateCert;
        _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(25);

        _ws.Options.SetRequestHeader(
            "Authorization",
            $"Bearer {accessToken}"
        );

        try
        {
            await _ws.ConnectAsync(
                new Uri($"wss://{_serverIp}:{_port}/ws"),
                _cts.Token
            );

            OnConnected?.Invoke();

            _ = Task.Run(ReceiveLoop);
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Connect failed: {ex.Message}");
            throw;
        }
    }

    // ---------------- CERT VALIDATION ----------------

    private bool ValidateCert(
        object sender,
        X509Certificate cert,
        X509Chain chain,
        SslPolicyErrors errors)
    {
        if (cert == null) return false;

        var remote = new X509Certificate2(cert);

        return remote.RawData.SequenceEqual(_pinnedCert.RawData);
    }

    // ---------------- SEND ----------------

    public async Task SendMessageAsync(string chatUid, string text) { if (!IsConnected) return; await _sendLock.WaitAsync(); try { var envelope = new WsEnvelope("send_message", new Dictionary<string, string> { ["chat_uid"] = chatUid, ["message_text"] = text }, null); var json = JsonSerializer.Serialize(envelope); var bytes = Encoding.UTF8.GetBytes(json); await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token); Debug.WriteLine($"➡️ Sent: {json}"); } catch (Exception ex) { OnError?.Invoke($"Send error: {ex.Message}"); } finally { _sendLock.Release(); } }

    // ---------------- RECEIVE ----------------

    private async Task ReceiveLoop()
    {
        var buffer = new byte[8192];

        try
        {
            while (!_cts.IsCancellationRequested && IsConnected)
            {
                var result = await _ws.ReceiveAsync(buffer, _cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var msg = JsonSerializer.Deserialize<WsEnvelope>(json);

                if (msg != null)
                    OnMessageReceived?.Invoke(msg);
            }
        }
        catch (Exception ex)
        {
            if (!_cts.IsCancellationRequested)
            {
                OnError?.Invoke($"Receive error: {ex.Message}");
                await Reconnect();
            }
        }

        OnDisconnected?.Invoke();
    }

    // ---------------- RECONNECT ----------------

    private async Task Reconnect()
    {
        if (_cts.IsCancellationRequested) return;

        Debug.WriteLine("🔄 Reconnecting...");

        await Task.Delay(3000);

        try
        {
            // передай токен заново при необходимости
        }
        catch
        {
            await Reconnect();
        }
    }

    // ---------------- CLOSE ----------------

    public async Task CloseAsync()
    {
        _cts.Cancel();

        if (IsConnected)
        {
            await _ws.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "closing",
                CancellationToken.None
            );
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        _ws.Dispose();
        _cts.Dispose();
        _sendLock.Dispose();
        _pinnedCert?.Dispose();
    }
}
