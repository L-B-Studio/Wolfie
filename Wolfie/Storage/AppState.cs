using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Storage
{
    public static class AppState
    {
        private const string LastPageKey = "LastPage";

        // Сохраняем имя текущей страницы
        public static void SaveLastPage(string pageName)
        {
            Preferences.Default.Set(LastPageKey, pageName);
        }

        // Получаем имя последней страницы
        public static string GetLastPage()
        {
            return Preferences.Default.Get(LastPageKey, "MainPage"); // дефолт
        }
    }
}
