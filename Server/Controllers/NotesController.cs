using Microsoft.AspNetCore.Mvc;
using NotepadApp.Server.Services;
using NotepadApp.Shared.Models;

namespace NotepadApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;

        public NotesController(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<NoteDto>>> GetAll()
        {
            var notes = await _documentRepository.GetAllNotesAsync();
            return Ok(notes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDto>> Get(string id)
        {
            var note = await _documentRepository.GetNoteAsync(id);
            if (note == null)
                return NotFound();

            return Ok(note);
        }

        [HttpPost]
        public async Task<ActionResult<NoteDto>> Create(NoteDto note)
        {
            if (string.IsNullOrEmpty(note.Id))
                note.Id = Guid.NewGuid().ToString();

            note.CreatedDate = DateTime.UtcNow;
            note.LastModified = DateTime.UtcNow;

            await _documentRepository.SaveNoteAsync(note);
            return CreatedAtAction(nameof(Get), new { id = note.Id }, note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, NoteDto note)
        {
            if (id != note.Id)
                return BadRequest();

            var existingNote = await _documentRepository.GetNoteAsync(id);
            if (existingNote == null)
                return NotFound();

            note.CreatedDate = existingNote.CreatedDate;
            note.LastModified = DateTime.UtcNow;

            await _documentRepository.SaveNoteAsync(note);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var note = await _documentRepository.GetNoteAsync(id);
            if (note == null)
                return NotFound();

            await _documentRepository.DeleteNoteAsync(id);
            return NoContent();
        }
    }
}
