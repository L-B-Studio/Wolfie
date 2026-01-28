using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Wolfie.Auth;
using Wolfie.Mappers;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.ChangePassword;
using Wolfie.Models.DTOObjects.Chats;
using Wolfie.Models.DTOObjects.ForgotPassword;
using Wolfie.Models.DTOObjects.GetAccess;
using Wolfie.Models.DTOObjects.Login;
using Wolfie.Models.DTOObjects.Message;
using Wolfie.Models.DTOObjects.Registration;

namespace Wolfie.Servers
{
    public class HttpClientService : IDisposable
    {
        private HttpClient _client;
        private readonly CancellationTokenSource _cts = new();
        private readonly AuthState _auth;
        private const int Port = 1234;
        private const string BaseUrl = "https://141.105.132.149";
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private X509Certificate2 _localPinnedCert;

        public HttpClientService(AuthState auth)
        {
            _auth = auth;
        }

        // ========================= INITIALIZE =========================

        public async Task InitializeAsync()
        {
            if (_client != null) return;

            await _initLock.WaitAsync();
            try
            {
                if (_client != null) return;

                // 1. Правильная загрузка сертификата из ресурсов MAUI
                if (_localPinnedCert == null)
                {
                    using var stream = await FileSystem.OpenAppPackageFileAsync("server.cer");
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    _localPinnedCert = new X509Certificate2(ms.ToArray());
                    Debug.WriteLine("✅ HttpClient pinned certificate loaded");
                }

                // 2. Настройка обработчика с колбэком валидации
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = ValidateServerCertificate
                };

                _client = new HttpClient(handler)
                {
                    BaseAddress = new Uri($"{BaseUrl}:{Port}/"),
                    Timeout = TimeSpan.FromSeconds(30)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ HttpClient init error: {ex.Message}");
            }
            finally
            {
                _initLock.Release();
            }
        }

        private bool ValidateServerCertificate(HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors)
        {
            // Если система сама подтвердила валидность - пропускаем
            if (errors == SslPolicyErrors.None) return true;

            // Если есть ошибки (например, самоподписанный сертификат), сверяем с нашим файлом
            if (cert != null && _localPinnedCert != null)
            {
                // Сравнение по отпечатку (Thumbprint)
                return cert.Thumbprint.Equals(_localPinnedCert.Thumbprint, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        // ========================= LOGIN =========================

        public async Task<LoginResponse?> SendLoginRequestAsync(string uri, LoginRequest dto)
        {
            return await PostAsync<LoginResponse>(uri, dto);
        }

        // ========================= REGISTER =========================

        public async Task<RegistrationResponse?> SendRegistrationRequestAsync(string uri, RegistrationRequest dto)
        {
            return await PostAsync<RegistrationResponse>(uri, dto);
        }

        // ========================= CHANGE PASSWORD =========================

        public async Task<string?> SendChangePassRequestAsync(string uri, ChangePasswordRequest dto)
        {
            return await PostAsync<string>(uri, dto);
        }

        // ========================= VERIFY EMAIL =========================

        public async Task<ForgotpassVerifyResponse?> SendForgotpassVerifyRequestAsync(string uri, ForgotpassVerifyRequest dto)
        {
            return await PostAsync<ForgotpassVerifyResponse>(uri, dto);
        }

        // ========================= FORGOT PASSWORD =========================

        public async Task<string?> SendForgotPassRequestAsync(string uri, ForgotPassRequest dto)
        {
            return await PostAsync<string>(uri, dto);
        }

        // ========================= REFRESH =========================

        public async Task<GetAccessTokenResponse?> GetNewAccessToken(string uri, GetAccessTokenRequest dto)
        {
            return await PostAsync<GetAccessTokenResponse>(uri, dto);
        }

        // ========================= CREATE CHATS =========================

        public async Task<CreateChatResponse?> CreateChatAsync(string uri, GetAccessTokenRequest dto)
        {
            return await PostAsync<CreateChatResponse>(uri, dto);
        }

        // ========================= GET CHATS =========================

        public async Task<List<ChatItemModel>> GetChatsAsync()
        {
            try
            {
                await InitializeAsync();

                using var request = new HttpRequestMessage(HttpMethod.Get, "chats");

                if (!string.IsNullOrEmpty(_auth.AccessToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AccessToken);

                var response = await SendWithAuthRetryAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[GetChats] Failed: {response.StatusCode}");
                    return new List<ChatItemModel>();
                }

                var json = await response.Content.ReadAsStringAsync();

                var chatDtos = JsonSerializer.Deserialize<List<GetChatResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (chatDtos == null) return new List<ChatItemModel>();

                return chatDtos.Select(dto => ChatMapper.ToChatModel(dto)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetChats] Error: {ex.Message}");
                return new List<ChatItemModel>();
            }
        }

        // ========================= GET MESSAGES =========================

        public async Task<List<MessageItemModel>> GetMessagesAsync(string chatUid, int limit = 20, int offset = 0)
        {
            try
            {
                await InitializeAsync();

                string uri = $"chats/{chatUid}/messages?limit={limit}&offset={offset}";
                Debug.WriteLine($"[GetMessages] Request: GET {uri}");

                using var request = new HttpRequestMessage(HttpMethod.Get, uri);

                if (!string.IsNullOrEmpty(_auth.AccessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
                }

                var response = await SendWithAuthRetryAsync(request);

                Debug.WriteLine($"[GetMessages] Response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[GetMessages] Failed: {response.StatusCode}");
                    return new List<MessageItemModel>();
                }

                var json = await response.Content.ReadAsStringAsync();

                var dtoList = JsonSerializer.Deserialize<List<GetMessagesResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dtoList == null || dtoList.Count == 0)
                {
                    Debug.WriteLine("[GetMessages] No messages found");
                    return new List<MessageItemModel>();
                }

                var messages = dtoList.Select(dto => MessageMapper.ToMessageModel(dto)).ToList();
                Debug.WriteLine($"[GetMessages] Loaded {messages.Count} messages");

                return messages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetMessages] Exception: {ex.Message}");
                return new List<MessageItemModel>();
            }
        }

        // ========================= CORE =========================

        private async Task<T?> PostAsync<T>(string uri, object body)
        {
            try
            {
                await InitializeAsync();

                var json = JsonSerializer.Serialize(body);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Content = content
                };

                var response = await SendWithAuthRetryAsync(request);

                var text = await response.Content.ReadAsStringAsync(_cts.Token);

                Debug.WriteLine($"[{uri}] Response: {text}");

                if (!response.IsSuccessStatusCode)
                    return default;

                return JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{uri}] Error: {ex.Message}");
                return default;
            }
        }

        private async Task<HttpResponseMessage> SendWithAuthRetryAsync(HttpRequestMessage request)
        {
            // Первая попытка
            if (!string.IsNullOrEmpty(_auth.AccessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AccessToken);

            var response = await _client.SendAsync(request, _cts.Token);

            // Если не 401 - возвращаем результат
            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                return response;

            Debug.WriteLine("🔁 Token expired — refreshing...");

            // Пытаемся обновить токен
            var refreshed = await TryRefreshTokenAsync();
            if (!refreshed)
            {
                Debug.WriteLine("❌ Refresh failed");
                return response;
            }

            Debug.WriteLine("✅ Token refreshed");

            // Создаём НОВЫЙ запрос с обновлённым токеном
            using var retryRequest = await CloneRequestAsync(request);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AccessToken);

            return await _client.SendAsync(retryRequest, _cts.Token);
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri);

            // Копируем контент
            if (original.Content != null)
            {
                var ms = new MemoryStream();
                await original.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                // Копируем заголовки контента
                foreach (var header in original.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Копируем заголовки запроса (кроме Authorization - его добавим отдельно)
            foreach (var header in original.Headers)
            {
                if (header.Key != "Authorization")
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        private async Task<bool> TryRefreshTokenAsync()
        {
            try
            {
                var refreshToken = await SecureStorage.GetAsync("token_refresh");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    Debug.WriteLine("No refresh token found");
                    return false;
                }

                var dto = new GetAccessTokenRequest
                {
                    token_refresh = refreshToken
                };

                var result = await GetNewAccessToken("auth/get_access_token/", dto);

                if (result == null || string.IsNullOrEmpty(result.token_access))
                {
                    Debug.WriteLine("Refresh token request failed");
                    return false;
                }

                // Обновляем токен в AuthState
                _auth.AccessToken = result.token_access;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TryRefreshToken error: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _client?.Dispose();
            _cts?.Dispose();
            _initLock?.Dispose();
        }
    }
}