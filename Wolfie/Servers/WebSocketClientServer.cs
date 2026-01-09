using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace Wolfie.Servers
{

    public record WsEnvelope(
        string Op,
        Dictionary<string, string> Data,
        int? Seq
    )
    {

    };
    public class WebSocketClientServer : IAsyncDisposable
    {
        private readonly ClientWebSocket _ws = new();
        private CancellationTokenSource _cts = new();


        private const int _port = 1234;
        private const string _serverIp = "141.105.132.149";

        // Подключение к серверу
        public async Task ConnectAsync(string accessToken)
        {
            var serverCert = new X509Certificate2("server_certificate.cer");

            _ws.Options.RemoteCertificateValidationCallback =
                (sender, cert, chain, errors) =>
                {
                    if (cert == null) return false;

                    var remoteCert = new X509Certificate2(cert);
                    return remoteCert.RawData.SequenceEqual(serverCert.RawData);
                };

            _ws.Options.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            await _ws.ConnectAsync(new Uri($"wss://{_serverIp}:{_port}/ws"), CancellationToken.None);
            Console.WriteLine("WebSocket connected");

            // Запускаем постоянное прослушивание сервера
            _ = Task.Run(ReceiveLoop);
        }

        // Отправка сообщения на сервер
        public async Task SendMessageAsync(string chatUid, string text)
        {
            if (_ws.State != WebSocketState.Open) return;

            var envelope = new WsEnvelope(
                Op: "send_message",
                Data: new Dictionary<string, string>
                {
                    ["chat_uid"] = chatUid,
                    ["message_text"] = text
                },
                Seq: null
            );

            var json = JsonSerializer.Serialize(envelope);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _ws.SendAsync(
                bytes,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            Console.WriteLine($"Sent: {json}");
        }

        // Основной цикл прослушивания сервера
        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];

            try
            {
                while (!_cts.Token.IsCancellationRequested && _ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(buffer, _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("WS closed by server");
                        break;
                    }

                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(json);
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
        }

        // Обработка входящих сообщений
        private void HandleMessage(string json)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<WsEnvelope>(json);
                if (envelope == null) return;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"⬅[{envelope.Op}] seq={envelope.Seq}");
                Console.ResetColor();

                switch (envelope.Op)
                {
                    case "new_message":
                        Console.WriteLine($"chat={envelope.Data.GetValueOrDefault("chat_uid")}");
                        Console.WriteLine($"text={envelope.Data.GetValueOrDefault("message_text")}");
                        break;

                    case "error":
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"error: {envelope.Data.GetValueOrDefault("code")}");
                        Console.ResetColor();
                        break;

                    default:
                        Console.WriteLine("raw: " + json);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid WS message: " + ex.Message);
            }
        }

        // Закрытие WebSocket
        public async Task CloseAsync()
        {
            _cts.Cancel();

            if (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client closing",
                    CancellationToken.None
                );
            }
        }

        public async ValueTask DisposeAsync()
        {
            await CloseAsync();
            _ws.Dispose();
            _cts.Dispose();
        }
    }
}