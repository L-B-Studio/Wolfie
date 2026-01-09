using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.ChangePassword;
using Wolfie.Models.DTOObjects.ForgotPassword;
using Wolfie.Models.DTOObjects.Login;
using Wolfie.Models.DTOObjects.Registration;

namespace Wolfie.Servers
{
    public class HttpClientService : IDisposable
    {
        private X509Certificate2 _localPinnedCert;
        private HttpClient _client;

        private const int _port = 1234;
        //private const string _serverIp = "192.168.168.118";
        private const string _serverIp = "141.105.132.149";

        // Initialize here to prevent NullReference on early Dispose
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        public bool IsConnected { get; private set; } = false;

        public event Action Connected;
        public event Action Disconnected;

        // Initialize the HttpClient with base address and certificate validation
        public async Task InitializeAsync()
        {

            var certBytes = await File.ReadAllBytesAsync("server_certificate.cer");
            _localPinnedCert = new X509Certificate2(certBytes);
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;
            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri($"https://{_serverIp}:{_port}")
            };
        }

        private bool ValidateServerCertificate(HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None) return true;
            return cert != null && _localPinnedCert != null && cert.Thumbprint == _localPinnedCert.Thumbprint;
        }

        public async Task<LoginResponse> SendLoginRequestAsync(string uri, LoginRequest loginRequest)
        {
            var json = JsonSerializer.Serialize(loginRequest);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            // ... setup code ...
            try
            {
                var response = await _client.PostAsync(uri, content, _cts.Token);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync(_cts.Token);
                System.Diagnostics.Debug.WriteLine ($"DEBUG: Raw JSON: {responseText}");
                return JsonSerializer.Deserialize<LoginResponse>(responseText);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                System.Diagnostics.Debug.WriteLine ("Login request error: " + ex.Message);

                // OPTIONAL: Rethrow the exception if you want the UI to see the specific error message
                // throw; 

                return null;
            }
        }

        public async Task<RegistrationResponse> SendRegistrationRequestAsync(string uri, RegistrationRequest registrationRequest)
        {
            var json = JsonSerializer.Serialize(registrationRequest);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            // ... setup code ...
            try
            {
                var response = await _client.PostAsync(uri, content, _cts.Token);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync(_cts.Token);
                System.Diagnostics.Debug.WriteLine($"DEBUG: Raw JSON: {responseText}");
                return JsonSerializer.Deserialize<RegistrationResponse>(responseText);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                System.Diagnostics.Debug.WriteLine("Registration request error: " + ex.Message);

                // OPTIONAL: Rethrow the exception if you want the UI to see the specific error message
                // throw; 

                return null;
            }
        }

        public async Task<string> SendForgotPassRequestAsync(string uri, ForgotPassRequest forgotPassRequest)
        {
            var json = JsonSerializer.Serialize(forgotPassRequest);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.PostAsync(uri, content, _cts.Token);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync(_cts.Token);
                // Handle the response as needed
                return response.EnsureSuccessStatusCode().ToString();
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine("Forgot password request error: " + ex.Message);
                return null;
            }
        }

        public async Task<ForgotpassVerifyResponse> SendForgotpassVerifyRequestAsync(string uri, ForgotpassVerifyRequest forgotpassVerifyRequest)
        {
            var json = JsonSerializer.Serialize(forgotpassVerifyRequest);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.PostAsync(uri, content, _cts.Token);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync(_cts.Token);
                return JsonSerializer.Deserialize<ForgotpassVerifyResponse>(responseText);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine("Forgot password verify request error: " + ex.Message);
                return null;
            }
        }

        public async Task<string> SendChangePassRequestAsync(string uri, ChangePasswordRequest changePassRequest)
        {
            var json = JsonSerializer.Serialize(changePassRequest);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.PostAsync(uri, content, _cts.Token);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync(_cts.Token);
                // Handle the response as needed
                return response.EnsureSuccessStatusCode().ToString();
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine("Forgot password request error: " + ex.Message);
                return null;
            }
        }

        // Dispose method to clean up resourcess
        public void Dispose()
        {
            _cts?.Cancel();
            _client?.Dispose();
            _cts?.Dispose();
            _connectionLock?.Dispose();
        }
    }
}