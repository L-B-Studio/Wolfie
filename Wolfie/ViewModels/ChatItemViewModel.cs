using System.ComponentModel;
using System.IO;
using Microsoft.Maui.Controls;
using Wolfie.Models;

namespace Wolfie.ViewModels;

public class ChatItemViewModel : INotifyPropertyChanged
{
    private ChatItemModel _model;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ChatItemViewModel()
    {
        const string savedChatId = "saved_messages";
        _model = new ChatItemModel
        {
            ChatId = savedChatId,
            ChatTitle = "Saved Messages",
            ChatAvatarName = "default_user_icon.png", // local image
            LastMessage = string.Empty,
            LastActivityDate = DateTime.UtcNow,
            IsUnreaded = false,
            UnreadedMessageCount = 0
        };
        UpdateAvatar();
    }

    public ChatItemViewModel(ChatItemModel model)
    {
        _model = model;
        UpdateAvatar();
    }

    // ====== PROPERTIES ======

    public string ChatId => _model.ChatId;
    public string ChatTitle => _model.ChatTitle;
    public string LastMessage => _model.LastMessage;

    public string LastTime =>
        _model.LastActivityDate.Date == DateTime.Today
            ? _model.LastActivityDate.ToString("HH:mm")
            : _model.LastActivityDate.ToString("dd.MM");

    public bool IsUnread => _model.IsUnreaded;
    public int UnreadCount => _model.UnreadedMessageCount;
    public bool ShowUnread => IsUnread && UnreadCount > 0;
    public bool IsPinned => _model.IsPinned;

    public ImageSource Avatar { get; private set; }

    public void Update(ChatItemModel newModel)
    {
        _model = newModel;
        UpdateAvatar();
        NotifyAll();
    }

    private ImageSource GetAvatarImageSource()
    {
        if (_model.ChatAvatarBytes?.Length > 0)
            return ImageSource.FromStream(() => new MemoryStream(_model.ChatAvatarBytes));

        if (!string.IsNullOrEmpty(_model.ChatAvatarName))
        {
            // сначала пытаемся локальный файл
            try
            {
                return ImageSource.FromFile(_model.ChatAvatarName);
            }
            catch
            {
                // fallback на Embedded Resourceww
                try
                {
                    return ImageSource.FromResource($"Wolfie.Resources.Images.{_model.ChatAvatarName}");
                }
                catch
                {
                    // fallback на дефолт
                    return ImageSource.FromFile("default_user_icon.png");
                }
            }
        }

        return ImageSource.FromFile("default_user_icon.png");
    }

    private void UpdateAvatar()
    {
        Avatar = GetAvatarImageSource();
        PropertyChanged?.Invoke(this, new(nameof(Avatar)));
        //PropertyChanged?.Invoke(this, new(nameof(ChatAvatarSource))); // если биндинг ChatAvatarSource
    }



    public void UpdateFromModel(ChatItemModel chat)
    {
        _model.LastMessage = chat.LastMessage;
        _model.LastActivityDate = chat.LastActivityDate;
        _model.IsUnreaded = chat.IsUnreaded;
        _model.UnreadedMessageCount = chat.UnreadedMessageCount;

        // Update chat if needed
        NotifyAll();
    }



    private void NotifyAll()
    {
        PropertyChanged?.Invoke(this, new(nameof(ChatTitle)));
        PropertyChanged?.Invoke(this, new(nameof(LastMessage)));
        PropertyChanged?.Invoke(this, new(nameof(LastTime)));
        PropertyChanged?.Invoke(this, new(nameof(IsUnread)));
        PropertyChanged?.Invoke(this, new(nameof(UnreadCount)));
        PropertyChanged?.Invoke(this, new(nameof(ShowUnread)));
        PropertyChanged?.Invoke(this, new(nameof(IsPinned)));
        PropertyChanged?.Invoke(this, new(nameof(Avatar)));
    }
}
