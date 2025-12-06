using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Services
{
    public static class ThemeService
    {
        public static bool IsDarkTheme { get; private set; }

        public static void Init()
        {
            var theme = Preferences.Get("Theme", "Light");
            IsDarkTheme = theme == "Dark";
            Application.Current.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
        }

        public static void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            Application.Current.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
            Preferences.Set("Theme", IsDarkTheme ? "Dark" : "Light");
        }
    }

}
