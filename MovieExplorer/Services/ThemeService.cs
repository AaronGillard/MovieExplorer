using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieExplorer.Services
{
    public static class ThemeService
    {
        private const string ThemePrefKey = "app_theme"; // Key for storing theme preference "Light" | "Dark" | "System"

        public static AppTheme GetSavedTheme()
        {
            var value = Preferences.Get(ThemePrefKey, "System");

            return value switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified // System default
            };
        }
    
        public static void ApplySavedTheme(Application app)
        {
            app.UserAppTheme = GetSavedTheme();
        }

        public static void setTheme(Application app, AppTheme theme)
        {
            var value = theme switch
            {
                AppTheme.Light => "Light",
                AppTheme.Dark => "Dark",
                _ => "System"
            }; 
            Preferences.Set(ThemePrefKey, value);
            app.UserAppTheme = theme;
        }

        public static void ToggleLightDark(Application app)
        {
            //If current theme is Light, switch to Dark, and vice versa. If System, switch to Light.
            var effective = app.UserAppTheme == AppTheme.Unspecified
                ? app.RequestedTheme
                : app.UserAppTheme;

            var next = effective == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
            setTheme(app, next);
        }
    }
}
