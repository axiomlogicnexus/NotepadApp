using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using Blazored.LocalStorage;
using NotepadApp.Shared.Models;
using Microsoft.Extensions.Logging;

namespace NotepadApp.Client.Services
{
    public class DocumentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<DocumentService> _logger;
        private const string NotesStorageKey = "notes";

        public event EventHandler<NoteDto> OnNoteUpdated;
        public event EventHandler<string> OnNoteDeleted;

        public DocumentService(HttpClient httpClient, ILocalStorageService localStorage, ILogger<DocumentService> logger = null)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _logger = logger;
        }

        public async Task<List<NoteDto>> GetAllNotesAsync()
        {
            try
            {
                var notes = await _httpClient.GetFromJsonAsync<List<NoteDto>>("api/notes");
                
                // Cache the notes in local storage
                if (notes != null)
                {
                    await _localStorage.SetItemAsync(NotesStorageKey, notes);
                }
                
                return notes ?? new List<NoteDto>();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to fetch notes from API, falling back to local storage");
                
                // Fallback to local storage if API call fails
                return await _localStorage.GetItemAsync<List<NoteDto>>(NotesStorageKey) ?? new List<NoteDto>();
            }
        }

        public async Task<NoteDto> GetNoteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Note ID cannot be null or empty", nameof(id));
            }

            try
            {
                var note = await _httpClient.GetFromJsonAsync<NoteDto>($"api/notes/{id}");
                return note;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to fetch note {NoteId} from API, falling back to local storage", id);
                
                var notes = await _localStorage.GetItemAsync<List<NoteDto>>(NotesStorageKey) ?? new List<NoteDto>();
                return notes.FirstOrDefault(n => n.Id == id);
            }
        }

        public async Task<NoteDto> SaveNoteAsync(NoteDto note)
        {
            if (note == null)
            {
                throw new ArgumentNullException(nameof(note), "Note cannot be null");
            }

            try
            {
                if (string.IsNullOrEmpty(note.Id))
                {
                    // Create new note
                    var response = await _httpClient.PostAsJsonAsync("api/notes", note);
                    if (response.IsSuccessStatusCode)
                    {
                        var savedNote = await response.Content.ReadFromJsonAsync<NoteDto>();
                        if (savedNote != null)
                        {
                            OnNoteUpdated?.Invoke(this, savedNote);
                            await UpdateLocalStorageCache(savedNote);
                            return savedNote;
                        }
                    }
                }
                else
                {
                    // Update existing note
                    var response = await _httpClient.PutAsJsonAsync($"api/notes/{note.Id}", note);
                    if (response.IsSuccessStatusCode)
                    {
                        OnNoteUpdated?.Invoke(this, note);
                        await UpdateLocalStorageCache(note);
                        return note;
                    }
                }
                
                // If we get here, something went wrong with the API call
                throw new HttpRequestException("Failed to save note to the server");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to save note to API, falling back to local storage");
                
                // Fallback to local storage
                return await SaveNoteToLocalStorageAsync(note);
            }
        }

        public async Task DeleteNoteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Note ID cannot be null or empty", nameof(id));
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"api/notes/{id}");
                if (response.IsSuccessStatusCode)
                {
                    OnNoteDeleted?.Invoke(this, id);
                    await RemoveNoteFromLocalStorageAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to delete note {NoteId} from API, falling back to local storage", id);
                
                await RemoveNoteFromLocalStorageAsync(id);
            }
        }

        // Helper method to search notes by title or content
        public async Task<List<NoteDto>> SearchNotesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllNotesAsync();
            }

            try
            {
                return await _httpClient.GetFromJsonAsync<List<NoteDto>>($"api/notes/search?term={Uri.EscapeDataString(searchTerm)}") ?? new List<NoteDto>();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to search notes from API, falling back to local storage");
                
                var notes = await _localStorage.GetItemAsync<List<NoteDto>>(NotesStorageKey) ?? new List<NoteDto>();
                searchTerm = searchTerm.ToLower();
                return notes.Where(n => 
                    n.Title.ToLower().Contains(searchTerm) || 
                    n.Content.ToLower().Contains(searchTerm) ||
                    n.Tags.Any(t => t.ToLower().Contains(searchTerm))
                ).ToList();
            }
        }

        #region Private Helper Methods

        private async Task<NoteDto> SaveNoteToLocalStorageAsync(NoteDto note)
        {
            var notes = await _localStorage.GetItemAsync<List<NoteDto>>(NotesStorageKey) ?? new List<NoteDto>();
            
            if (string.IsNullOrEmpty(note.Id))
            {
                note.Id = Guid.NewGuid().ToString();
                note.CreatedAt = DateTime.UtcNow;
                notes.Add(note);
            }
            else
            {
                var existingNote = notes.FirstOrDefault(n => n.Id == note.Id);
                if (existingNote != null)
                {
                    notes.Remove(existingNote);
                }
                note.UpdatedAt = DateTime.UtcNow;
                notes.Add(note);
            }
            
            await _localStorage.SetItemAsync(NotesStorageKey, notes);
            OnNoteUpdated?.Invoke(this, note);
            return note;
        }

        private async Task RemoveNoteFromLocalStorageAsync(string id)
        {
            var notes = await _localStorage.GetItemAsync<List<NoteDto>>(NotesStorageKey) ?? new List<NoteDto>();
            var noteToRemove = notes.FirstOrDefault(n => n.Id == id);
            
            if (noteToRemove != null)
            {
                notes.Remove(noteToRemove);
                await _localStorage.SetItemAsync(NotesStorageKey, notes);
                OnNoteDeleted?.Invoke(this, id);
            }
        }

        private async Task UpdateLocalStorageCache(NoteDto updatedNote)
        {
            var notes = await _localStorage.GetItemAsync<List<NoteDto>>(NotesStorageKey) ?? new List<NoteDto>();
            var existingNote = notes.FirstOrDefault(n => n.Id == updatedNote.Id);
            
            if (existingNote != null)
            {
                notes.Remove(existingNote);
            }
            
            notes.Add(updatedNote);
            await _localStorage.SetItemAsync(NotesStorageKey, notes);
        }

        #endregion
    }
}