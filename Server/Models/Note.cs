namespace NotepadApp.Server.Models
{
    public class Note
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "Untitled";
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<NoteTag> Tags { get; set; } = new List<NoteTag>();
    }
}
