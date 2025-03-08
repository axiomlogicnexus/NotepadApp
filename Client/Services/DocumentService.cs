using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using NotepadApp.Shared.Models;

namespace NotepadApp.Client.Services
{
    public class DocumentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;
        private const string NotesStorageKey = "local_notes";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Events for note changes
        public event Action<NoteDto>? NoteUpdated;
        public event Action<string>? NoteDeleted;

        public DocumentService(HttpClient httpClient, ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        public async Task<List<NoteDto>> GetNotesAsync()
        {
            try
            {
                // Try to get from API first
                var apiNotes = await _httpClient.GetFromJsonAsync<List<NoteDto>>("api/notes");
                if (apiNotes != null)
                {
                    return apiNotes;
                }
            }
            catch
            {
                // If API fails, fall back to local storage
                Console.WriteLine("API call failed, falling back to local storage");
            }

            // Get from local storage
            var json = await _localStorage.GetItemAsStringAsync(NotesStorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<List<NoteDto>>(json, _jsonOptions) ?? new List<NoteDto>();
            }

            return new List<NoteDto>();
        }

        public async Task<List<NoteDto>> SearchNotesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetNotesAsync();
            }

            var allNotes = await GetNotesAsync();
            searchTerm = searchTerm.ToLower();

            return allNotes.FindAll(note => 
                note.Title.ToLower().Contains(searchTerm) || 
                note.Content.ToLower().Contains(searchTerm) ||
                note.Tags.Any(tag => tag.ToLower().Contains(searchTerm)));
        }

        public async Task<NoteDto?> GetNoteAsync(string id)
        {
            try
            {
                // Try to get from API first
                var apiNote = await _httpClient.GetFromJsonAsync<NoteDto>($"api/notes/{id}");
                if (apiNote != null)
                {
                    return apiNote;
                }
            }
            catch
            {
                // If API fails, fall back to local storage
                Console.WriteLine($"API call failed for note {id}, falling back to local storage");
            }

            // Get from local storage
            var notes = await GetNotesAsync();
            return notes.Find(n => n.Id == id);
        }

        public async Task<NoteDto> SaveNoteAsync(NoteDto note)
        {
            if (string.IsNullOrEmpty(note.Id))
            {
                note.Id = Guid.NewGuid().ToString();
                note.CreatedDate = DateTime.UtcNow;
            }

            note.LastModified = DateTime.UtcNow;

            try
            {
                // Try to save to API first
                var response = await _httpClient.PostAsJsonAsync("api/notes", note);
                if (response.IsSuccessStatusCode)
                {
                    var savedNote = await response.Content.ReadFromJsonAsync<NoteDto>();
                    if (savedNote != null)
                    {
                        OnNoteUpdated(savedNote);
                        return savedNote;
                    }
                }
            }
            catch
            {
                // If API fails, fall back to local storage
                Console.WriteLine("API save failed, falling back to local storage");
            }

            // Save to local storage
            var notes = await GetNotesAsync();
            var existingIndex = notes.FindIndex(n => n.Id == note.Id);
            
            if (existingIndex >= 0)
            {
                notes[existingIndex] = note;
            }
            else
            {
                notes.Add(note);
            }

            await _localStorage.SetItemAsStringAsync(NotesStorageKey, JsonSerializer.Serialize(notes, _jsonOptions));
            OnNoteUpdated(note);
            return note;
        }

        public async Task<bool> DeleteNoteAsync(string id)
        {
            try
            {
                // Try to delete from API first
                var response = await _httpClient.DeleteAsync($"api/notes/{id}");
                if (response.IsSuccessStatusCode)
                {
                    OnNoteDeleted(id);
                    return true;
                }
            }
            catch
            {
                // If API fails, fall back to local storage
                Console.WriteLine($"API delete failed for note {id}, falling back to local storage");
            }

            // Delete from local storage
            var notes = await GetNotesAsync();
            var existingIndex = notes.FindIndex(n => n.Id == id);
            
            if (existingIndex >= 0)
            {
                notes.RemoveAt(existingIndex);
                await _localStorage.SetItemAsStringAsync(NotesStorageKey, JsonSerializer.Serialize(notes, _jsonOptions));
                OnNoteDeleted(id);
                return true;
            }

            return false;
        }

        // Event handlers
        public void OnNoteUpdated(NoteDto note)
        {
            NoteUpdated?.Invoke(note);
        }

        public void OnNoteDeleted(string noteId)
        {
            NoteDeleted?.Invoke(noteId);
        }

        // Navigation
        public void NavigateToNote(string noteId)
        {
            _navigationManager.NavigateTo($"/note/{noteId}");
        }

        public void NavigateToNote(NoteDto note)
        {
            NavigateToNote(note.Id);
        }
    }
}