using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Helpers
{
    public class MessangerServicesHelper
    {
        public static T GetService<T>() =>
       Current.GetService<T>() ?? throw new InvalidOperationException($"Сервис {typeof(T)} не найден.");

        public static IServiceProvider Current =>
            Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Сервисы MAUI недоступны (приложение ещё не инициализировано).");
    }

}

