using System;
using System.ComponentModel;
using System.Windows.Input;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.ViewModels
{
    //public class MessageItemViewModel : INotifyPropertyChanged
    //{
    //    private readonly MessageItemModel _messageModel;
    //    private readonly ChatStorageService _chatStorage;
    //
    //    public event PropertyChangedEventHandler? PropertyChanged;
    //
    //    public MessageItemViewModel(MessageItemModel messageModel, ChatStorageService chatStorage)
    //    {
    //        _messageModel = messageModel;
    //        _chatStorage = chatStorage;
    //
    //        ReplyCommand = new Command(() => OnReply?.Invoke(this));
    //        EditCommand = new Command(() => OnEdit?.Invoke(this), () => CanEdit);
    //        DeleteCommand = new Command(ExecuteDelete);
    //
    //        // Подписка на обновления доставки/редактирования через сервис
    //        _chatStorage.MessageReceived += OnMessageReceived;
    //    }
    //
    //    public string MessageId => _messageModel.MessageId ?? string.Empty;
    //    public string MessageContent => _messageModel.Message ?? string.Empty;
    //    public string SenderId => _messageModel.Sender ?? string.Empty;
    //    public string ChatId => _messageModel.Getter ?? string.Empty;
    //    public bool IsDeleted => _messageModel.IsDeleted;
    //    public bool IsOutgoingMessage => SenderId == _chatStorage.CurrentUserId;
    //
    //    public bool CanEdit => IsOutgoingMessage && !_messageModel.IsDeleted;
    //
    //    public string Timestamp => _messageModel.Timestamp.ToString("HH:mm");
    //
    //    public ICommand ReplyCommand { get; }
    //    public ICommand EditCommand { get; }
    //    public ICommand DeleteCommand { get; }
    //
    //    // События для прокидывания на страницу
    //    public Action<MessageItemViewModel>? OnReply { get; set; }
    //    public Action<MessageItemViewModel>? OnEdit { get; set; }
    //
    //    private async void ExecuteDelete()
    //    {
    //        _messageModel.IsDeleted = true;
    //        OnPropertyChanged(nameof(IsDeleted));
    //        OnPropertyChanged(nameof(MessageContent));
    //        if (_chatStorage != null)
    //        {
    //            await _chatStorage.DeleteMessageAsync(_messageModel.MessageId);
    //        }
    //    }
    //
    //
    //    private void OnMessageReceived(MessageItemModel m)
    //    {
    //        if (m.MessageId == _messageModel.MessageId)
    //        {
    //            _messageModel.DeliveryStatus = m.DeliveryStatus;
    //            _messageModel.isReaded = m.isReaded;
    //            _messageModel.Message = m.Message; // если пришло редактирование
    //            OnPropertyChanged(nameof(MessageContent));
    //            OnPropertyChanged(nameof(DeliveryStatusIconSource));
    //        }
    //    }
    //
    //    public string DeliveryStatusIconSource
    //    {
    //        get
    //        {
    //            if (_messageModel.DeliveryStatus == 0) return "icon_pending.png";
    //            if (_messageModel.DeliveryStatus == 1) return "icon_sent.png";
    //            if (_messageModel.DeliveryStatus == 2 && !_messageModel.isReaded) return "icon_delivered.png";
    //            if (_messageModel.DeliveryStatus == 2 && _messageModel.isReaded) return "icon_read.png";
    //            return string.Empty;
    //        }
    //    }
    //
    //    protected void OnPropertyChanged(string propertyName)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //}
}
