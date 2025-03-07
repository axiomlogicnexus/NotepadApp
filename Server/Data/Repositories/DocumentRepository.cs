using NotepadApp.Shared.Models;
using NotepadApp.Server.Models;
using Microsoft.EntityFrameworkCore;
using NotepadApp.Server.Services;
using NotepadApp.Server.Data;

namespace NotepadApp.Server.Data.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NoteDto>> GetAllNotesAsync()
        {
            return await _context.Notes
                .Include(n => n.Tags)
                .ThenInclude(nt => nt.Tag)
                .Select(n => new NoteDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    Tags = n.Tags.Select(t => t.Tag.Name).ToList(),
                    CreatedDate = n.CreatedDate,
                    LastModified = n.LastModified
                })
                .ToListAsync();
        }

        public async Task<NoteDto> GetNoteAsync(string id)
        {
            var note = await _context.Notes
                .Include(n => n.Tags)
                .ThenInclude(nt => nt.Tag)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
                return null;

            return new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Tags = note.Tags.Select(t => t.Tag.Name).ToList(),
                CreatedDate = note.CreatedDate,
                LastModified = note.LastModified
            };
        }

        public async Task SaveNoteAsync(NoteDto noteDto)
        {
            var note = await _context.Notes.FindAsync(noteDto.Id);

            if (note == null)
            {
                note = new Note
                {
                    Id = noteDto.Id,
                    Title = noteDto.Title,
                    Content = noteDto.Content,
                    CreatedDate = noteDto.CreatedDate,
                    LastModified = noteDto.LastModified
                };
                
                _context.Notes.Add(note);
            }
            else
            {
                note.Title = noteDto.Title;
                note.Content = noteDto.Content;
                note.LastModified = noteDto.LastModified;
                
                // Remove existing tags
                var existingTags = await _context.NoteTags
                    .Where(nt => nt.NoteId == note.Id)
                    .ToListAsync();
                
                _context.NoteTags.RemoveRange(existingTags);
            }

            // Add tags
            foreach (var tagName in noteDto.Tags)
            {
                var tag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name == tagName);
                
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    _context.Tags.Add(tag);
                }
                
                _context.NoteTags.Add(new NoteTag
                {
                    Note = note,
                    Tag = tag
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(string id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }
    }
}
