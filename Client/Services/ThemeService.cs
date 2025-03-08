using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace NotepadApp.Client.Services
{
    public class ThemeService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private const string ThemeKey = "app_theme";
        
        public string CurrentTheme { get; private set; } = "light-theme";
        
        private Action<string>? _themeChangedCallback;

        public ThemeService(ILocalStorageService localStorage, IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            // Try to load theme from local storage
            var savedTheme = await _localStorage.GetItemAsStringAsync(ThemeKey);
            
            if (!string.IsNullOrEmpty(savedTheme))
            {
                CurrentTheme = savedTheme;
            }
            
            // Apply the theme to the document
            await ApplyThemeAsync();
        }

        public async Task SetThemeAsync(string theme)
        {
            if (theme != "light-theme" && theme != "dark-theme")
            {
                throw new ArgumentException("Theme must be either 'light-theme' or 'dark-theme'", nameof(theme));
            }
            
            CurrentTheme = theme;
            await _localStorage.SetItemAsStringAsync(ThemeKey, theme);
            await ApplyThemeAsync();
            
            // Notify subscribers
            _themeChangedCallback?.Invoke(theme);
        }

        public async Task ToggleThemeAsync()
        {
            var newTheme = CurrentTheme == "light-theme" ? "dark-theme" : "light-theme";
            await SetThemeAsync(newTheme);
        }

        public bool IsDarkTheme()
        {
            return CurrentTheme == "dark-theme";
        }

        public void ThemeChanged(Action<string> callback)
        {
            _themeChangedCallback = callback;
        }

        private async Task ApplyThemeAsync()
        {
            // Apply theme class to document body
            await _jsRuntime.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", CurrentTheme);
        }
    }
}