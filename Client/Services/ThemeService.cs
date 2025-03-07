using System;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace NotepadApp.Client.Services
{
  public class ThemeService
  {
    private readonly ILocalStorageService _localStorage;
    private bool _isDarkMode;
        
    // Add a property to get the current theme as a string
    public string CurrentTheme => _isDarkMode ? "dark" : "light";
        
    // Keep the existing IsDarkMode property for backward compatibility
    public bool IsDarkMode => _isDarkMode;
        
    // Change to a more generic event name
    public event EventHandler OnThemeChanged;
        
    public ThemeService(ILocalStorageService localStorage)
    {
      _localStorage = localStorage;
    }
        
    public async Task InitializeAsync()
    {
      _isDarkMode = await _localStorage.GetItemAsync<bool>("darkMode");
      OnThemeChanged?.Invoke(this, EventArgs.Empty);
    }
        
    // Rename to match my suggestion
    public async Task ToggleThemeAsync()
    {
      _isDarkMode = !_isDarkMode;
      await _localStorage.SetItemAsync("darkMode", _isDarkMode);
      OnThemeChanged?.Invoke(this, EventArgs.Empty);
    }
        
    // Add a method to set a specific theme
    public async Task SetThemeAsync(string theme)
    {
      bool isDark = theme.ToLower() == "dark";
      if (_isDarkMode != isDark)
      {
        _isDarkMode = isDark;
        await _localStorage.SetItemAsync("darkMode", _isDarkMode);
        OnThemeChanged?.Invoke(this, EventArgs.Empty);
      }
    }
  }
}