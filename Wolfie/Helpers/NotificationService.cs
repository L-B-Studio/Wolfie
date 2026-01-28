using Plugin.LocalNotification;

namespace Wolfie.Services
{
    public  class NotificationService
    {
        public  void Show(string title, string message)
        {
            var request = new NotificationRequest
            {
                NotificationId = new Random().Next(1000, 9999),
                Title = title,
                Description = message,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddMilliseconds(100)
                }
            };

            LocalNotificationCenter.Current.Show(request);
        }
    }
}
