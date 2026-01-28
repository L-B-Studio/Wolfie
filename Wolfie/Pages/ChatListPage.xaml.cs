using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Wolfie.Models;
using Wolfie.Servers;
using Plugin.LocalNotification;
using Wolfie.Services;
using SQLitePCL;
using Wolfie.Auth;

namespace Wolfie.Pages;

public partial class ChatListPage : ContentPage
{
    private WebSocketClientService? _ws;
    private readonly NotificationService _not; // ИНИЦИАЛИЗИРОВАТЬ!
    private readonly AuthState _authstate;
    private List<ChatItemModel> _allChats = new();
    private ObservableCollection<MessageItemModel> _messages = new();
    private readonly HttpClientService _httpClient;
    private int _currentOffset = 0;
    private const int PageSize = 20;
    private string _currentChatUid = string.Empty; // Инициализировать
    private bool _isLoadingMessages = false;
    private bool _hasMoreMessages = true;

    public ChatListPage(HttpClientService httpClient, AuthState auth)
    {
        InitializeComponent();
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authstate = auth ?? throw new ArgumentNullException(nameof(auth));

        // КРИТИЧНО: Инициализировать NotificationService
        _not = new NotificationService();

        MessagesCollectionView.ItemsSource = _messages;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await LoadChats();

            // Проверка токена
            if (string.IsNullOrEmpty(_authstate.AccessToken))
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            // Создавать WebSocket только если еще не создан
            if (_ws == null)
            {
                _ws = new WebSocketClientService();
                _ws.OnMessageReceived += HandleWsMessage;
            }

            // Подключаться только если не подключен
            await _ws.ConnectAsync(_authstate.AccessToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OnAppearing: {ex.Message}");
            await DisplayAlertAsync("Помилка", "Не вдалося підключитися", "OK");
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            if (_ws != null)
            {
                await _ws.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OnDisappearing: {ex.Message}");
        }
    }

    private void HandleWsMessage(WsEnvelope envelope)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (envelope?.Op != "new_message" || envelope.Data == null)
                    return;

                // Проверка наличия данных
                if (!envelope.Data.ContainsKey("chat_uid") ||
                    !envelope.Data.ContainsKey("message_id") ||
                    !envelope.Data.ContainsKey("sender_id") ||
                    !envelope.Data.ContainsKey("message_text") ||
                    !envelope.Data.ContainsKey("created_at"))
                {
                    System.Diagnostics.Debug.WriteLine("[WARN] Incomplete message data");
                    return;
                }

                // Всегда добавляем в чат
                _messages.Add(new MessageItemModel
                {
                    ChatUid = envelope.Data["chat_uid"] ?? string.Empty,
                    MessageId = envelope.Data["message_id"] ?? string.Empty,
                    SenderId = envelope.Data["sender_id"] ?? string.Empty,
                    MessageText = envelope.Data["message_text"] ?? string.Empty,
                    CreatedAt = envelope.Data["created_at"] ?? string.Empty
                });

                // 🔔 Показываем уведомление ТОЛЬКО если приложение не активно
                if (!App.IsAppActive && _not != null)
                {
                    _not.Show(
                        envelope.Data["sender_id"] ?? "Нове повідомлення",
                        envelope.Data["message_text"] ?? string.Empty
                    );
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] HandleWsMessage: {ex.Message}");
        }
    }

    private async Task LoadChats()
    {
        ChatsLoadingIndicator.IsRunning = true;

        try
        {
            var chats = await _httpClient.GetChatsAsync();

            if (chats != null && chats.Count > 0)
            {
                _allChats = chats;
                ChatsCollectionView.ItemsSource = _allChats;
                EmptyState.IsVisible = false;
            }
            else
            {
                _allChats = new List<ChatItemModel>(); // Инициализировать пустой список
                ChatsCollectionView.ItemsSource = _allChats;
                EmptyState.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] LoadChats: {ex.Message}");
            await DisplayAlertAsync("Помилка", "Не вдалося завантажити чати", "OK");
        }
        finally
        {
            ChatsLoadingIndicator.IsRunning = false;
        }
    }

    private async void OnLoadMoreMessages(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentChatUid) || _isLoadingMessages || !_hasMoreMessages)
            return;

        System.Diagnostics.Debug.WriteLine($"[CHAT] Loading more messages, offset={_currentOffset}");
        await LoadMessages(_currentChatUid, isLoadMore: true);
    }

    private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selectedChat = e.CurrentSelection.FirstOrDefault() as ChatItemModel;
            if (selectedChat == null)
                return;

            // Если выбран тот же чат - не перезагружаем
            if (_currentChatUid == selectedChat.ChatId)
                return;

            HeaderTitleLabel.Text = selectedChat.ChatTitle;
            HeaderAvatarLabel.Text = selectedChat.ChatTitle.Length > 0
                ? selectedChat.ChatTitle[0].ToString()
                : "?";
            ChatHeader.IsVisible = true;
            InputArea.IsVisible = true;
            EmptyState.IsVisible = false;

            await LoadMessages(selectedChat.ChatId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OnChatSelected: {ex.Message}");
        }
    }

    private async Task LoadMessages(string chatUid, bool isLoadMore = false)
    {
        if (_isLoadingMessages) return;

        _isLoadingMessages = true;
        MsgLoadingIndicator.IsRunning = true;

        try
        {
            if (!isLoadMore)
            {
                _currentOffset = 0;
                _currentChatUid = chatUid;
                _messages.Clear();
                _hasMoreMessages = true;
                System.Diagnostics.Debug.WriteLine($"[CHAT] First load for chat {chatUid}");
            }

            System.Diagnostics.Debug.WriteLine($"[CHAT] Requesting messages: offset={_currentOffset}, limit={PageSize}");

            var newMessages = await _httpClient.GetMessagesAsync(chatUid, PageSize, _currentOffset);

            if (newMessages == null || newMessages.Count == 0)
            {
                _hasMoreMessages = false;
                System.Diagnostics.Debug.WriteLine("[CHAT] No more messages");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[CHAT] Received {newMessages.Count} messages");

            if (isLoadMore)
            {
                int addedCount = 0;
                foreach (var msg in newMessages)
                {
                    _messages.Add(msg);
                    addedCount++;
                }
                System.Diagnostics.Debug.WriteLine($"[CHAT] Added {addedCount} old messages to the end");
            }
            else
            {
                foreach (var msg in newMessages.AsEnumerable().Reverse())
                {
                    _messages.Add(msg);
                }
                System.Diagnostics.Debug.WriteLine($"[CHAT] Initial load: added {newMessages.Count} messages in reverse");
            }

            _currentOffset += newMessages.Count;

            if (newMessages.Count < PageSize)
            {
                _hasMoreMessages = false;
                System.Diagnostics.Debug.WriteLine("[CHAT] Reached end of messages");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CHAT] Error: {ex.Message}");
            await DisplayAlertAsync("Помилка", "Не вдалося завантажити повідомлення", "OK");
        }
        finally
        {
            _isLoadingMessages = false;
            MsgLoadingIndicator.IsRunning = false;
        }
    }

    private async void OnSendMessageClicked(object sender, EventArgs e)
    {
        try
        {
            var text = MessageEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(_currentChatUid))
                return;

            var newMessage = new MessageItemModel
            {
                MessageText = text,
                CreatedAt = DateTime.Now.ToString("dd HH:mm"),
                ChatUid = _currentChatUid,
                SenderId = "me" // Помечаем как свое сообщение
            };

            _messages.Insert(0, newMessage);
            MessageEntry.Text = string.Empty;

            await Task.Delay(50);
            MessagesCollectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);

            // Отправить на сервер
            if (_ws != null)
            {
                await _ws.SendMessageAsync(_currentChatUid, text);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OnSendMessageClicked: {ex.Message}");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var keyword = e.NewTextValue?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                ChatsCollectionView.ItemsSource = _allChats;
            }
            else
            {
                ChatsCollectionView.ItemsSource = _allChats
                    .Where(x => x.ChatTitle?.ToLower().Contains(keyword) ?? false)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OnSearchTextChanged: {ex.Message}");
        }
    }

    private async void ToggleMenu(object sender, EventArgs e)
    {
        Overlay.IsVisible = true;
        await BurgerMenu.TranslateToAsync(0, 0, 250, Easing.CubicOut);
    }

    private async void CloseMenu(object sender, EventArgs e)
    {
        await BurgerMenu.TranslateToAsync(-300, 0, 250, Easing.CubicIn);
        Overlay.IsVisible = false;
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        try
        {
            _authstate.AccessToken = null;
            SecureStorage.Remove("token_refresh");

            // Отключить WebSocket перед выходом
            if (_ws != null)
            {
                await _ws.DisposeAsync();
                _ws = null;
            }

            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] OnExitClicked: {ex.Message}");
        }
    }
}