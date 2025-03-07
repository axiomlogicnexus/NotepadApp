using System.Net.Http.Json;
using NotepadApp.Shared.Models;
using Blazored.LocalStorage;

namespace NotepadApp.Client.Services
{
    public class DocumentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public DocumentService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<List<NoteDto>> GetAllNotesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<NoteDto>>("api/notes") ?? new List<NoteDto>();
            }
            catch
            {
                // Fallback to local storage if API call fails
                return await _localStorage.GetItemAsync<List<NoteDto>>("notes") ?? new List<NoteDto>();
            }
        }

        public async Task<NoteDto> GetNoteAsync(string id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<NoteDto>($"api/notes/{id}");
            }
            catch
            {
                var notes = await _localStorage.GetItemAsync<List<NoteDto>>("notes") ?? new List<NoteDto>();
                return notes.FirstOrDefault(n => n.Id == id);
            }
        }

        public async Task SaveNoteAsync(NoteDto note)
        {
            try
            {
                if (string.IsNullOrEmpty(note.Id))
                {
                    await _httpClient.PostAsJsonAsync("api/notes", note);
                }
                else
                {
                    await _httpClient.PutAsJsonAsync($"api/notes/{note.Id}", note);
                }
            }
            catch
            {
                // Fallback to local storage
                var notes = await _localStorage.GetItemAsync<List<NoteDto>>("notes") ?? new List<NoteDto>();
                
                if (string.IsNullOrEmpty(note.Id))
                {
                    note.Id = Guid.NewGuid().ToString();
                    notes.Add(note);
                }
                else
                {
                    var existingNote = notes.FirstOrDefault(n => n.Id == note.Id);
                    if (existingNote != null)
                    {
                        notes.Remove(existingNote);
                        notes.Add(note);
                    }
                }
                
                await _localStorage.SetItemAsync("notes", notes);
            }
        }

        public async Task DeleteNoteAsync(string id)
        {
            try
            {
                await _httpClient.DeleteAsync($"api/notes/{id}");
            }
            catch
            {
                var notes = await _localStorage.GetItemAsync<List<NoteDto>>("notes") ?? new List<NoteDto>();
                var noteToRemove = notes.FirstOrDefault(n => n.Id == id);
                if (noteToRemove != null)
                {
                    notes.Remove(noteToRemove);
                    await _localStorage.SetItemAsync("notes", notes);
                }
            }
        }
    }
}
