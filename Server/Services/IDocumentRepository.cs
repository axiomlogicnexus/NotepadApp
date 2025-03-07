using NotepadApp.Shared.Models;

namespace NotepadApp.Server.Services
{
    public interface IDocumentRepository
    {
        Task<List<NoteDto>> GetAllNotesAsync();
        Task<NoteDto> GetNoteAsync(string id);
        Task SaveNoteAsync(NoteDto note);
        Task DeleteNoteAsync(string id);
    }
}
