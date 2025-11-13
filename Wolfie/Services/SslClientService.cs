using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Wolfie.Services
{
    public class SslClientService : IDisposable
    {
        private TcpClient? _client;
        private SslStream? _sslStream;
        private StreamWriter? _writer;
        private StreamReader? _reader;

        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private CancellationTokenSource _cts;

        private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);

        private readonly string _host = "213.231.4.165";
        private readonly int _port = 1234;

        private bool _isConnecting = false;

        private X509Certificate2 _localPinnedCert;   // ✅ локальный trusted cocert
        public bool IsConnected => _client?.Connected ?? false;

        public event Action<string> MessageReceived;
        public event Action Connected;
        public event Action Disconnected;

        public SslClientService() { _ = LoadPinnedCertificateAsync(); }

        // ✅ Загружаем server.cer из MAUI 
        private async Task LoadPinnedCertificateAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("server.cer");
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            _localPinnedCert = X509CertificateLoader.LoadCertificate(ms.ToArray());
        }



        public async Task EnsureConnectedAsync()
        {
            if (IsConnected || _isConnecting)
                return;

            _isConnecting = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    _client?.Close();
                    _client = new TcpClient();

                    await _client.ConnectAsync(_host, _port);

                    // ✅ SSL Stream + Certificate Validation
                    _sslStream = new SslStream(
                        _client.GetStream(),
                        false,
                        ValidateServerCertificate
                    );

                    await _sslStream.AuthenticateAsClientAsync(_host);

                    _writer = new StreamWriter(_sslStream, Encoding.UTF8) { AutoFlush = true };
                    _reader = new StreamReader(_sslStream, Encoding.UTF8);

                    Connected?.Invoke();
                    _ = Task.Run(() => ListenAsync(_cts.Token));

                    _isConnecting = false;
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reconnect after error: {ex}");
                    Disconnected?.Invoke();
                    await Task.Delay(_reconnectDelay);
                }
            }

            _isConnecting = false;
        }

        // ✅ НОРМАЛЬНАЯ ПРОВЕРКА (как в твоём первом коде)
        private bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            // 1️⃣ Если Windows доверяет — отлично
            if (errors == SslPolicyErrors.None)
                return true;

            // 2️⃣ Если сертификат есть — сравниваем с локальным
            if (certificate != null)
            {
                string serverHash = certificate.GetCertHashString();
                string localHash = _localPinnedCert.GetCertHashString();

                if (serverHash == localHash)
                {
                    Console.WriteLine("✅ Certificate matches pinned server.cer");
                    return true;
                }
            }

            Console.WriteLine("❌ Certificate rejected");
            return false;
        }


        private async Task ListenAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _reader != null)
                {
                    string message;

                    try
                    {
                        message = await _reader.ReadLineAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        Disconnected?.Invoke();
                        await TryReconnect(token);
                        break;
                    }

                    if (message == null)
                    {
                        Disconnected?.Invoke();
                        await TryReconnect(token);
                        break;
                    }

                    MessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Listen error: " + ex);
                Disconnected?.Invoke();
                await TryReconnect(token);
            }
        }

        private async Task TryReconnect(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            await Task.Delay(_reconnectDelay, token).ContinueWith(_ => { });
            await EnsureConnectedAsync();
        }

        public async Task SendAsync(string message)
        {
            await EnsureConnectedAsync();

            if (_writer == null)
                throw new InvalidOperationException("Not connected to server");

            await _sendLock.WaitAsync();
            try
            {
                await _writer.WriteLineAsync(message);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            _cts?.Cancel();

            try { await _sslStream?.ShutdownAsync(); } catch { }

            _writer?.Dispose();
            _reader?.Dispose();
            _sslStream?.Dispose();
            _client?.Close();

            _writer = null;
            _reader = null;
            _sslStream = null;
            _client = null;

            Disconnected?.Invoke();
        }

        public async void Dispose()
        {
            await DisconnectAsync();
            _cts?.Dispose();
            _sendLock?.Dispose();
        }
    }
}
