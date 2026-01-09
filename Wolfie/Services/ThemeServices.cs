using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Services
{
    public static class ThemeService
    {
        // Property to check if the current theme is dark
        public static bool IsDarkTheme { get; private set; }

        // Initialize theme based on saved preferences
        public static void Init()
        {
            var theme = Preferences.Get("Theme", "Light");
            IsDarkTheme = theme == "Dark";
            Application.Current.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
        }

        //  Toggle between dark and light themes
        public static void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            Application.Current.UserAppTheme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
            Preferences.Set("Theme", IsDarkTheme ? "Dark" : "Light");
        }
    }

}
