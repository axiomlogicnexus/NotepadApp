using System;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace NotepadApp.Client.Services
{
    public class ThemeService
    {
        private readonly ILocalStorageService _localStorage;
        private bool _isDarkMode;

        public bool IsDarkMode => _isDarkMode;
        public event EventHandler ThemeChanged;

        public ThemeService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            _isDarkMode = await _localStorage.GetItemAsync<bool>("darkMode");
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            await _localStorage.SetItemAsync("darkMode", _isDarkMode);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
