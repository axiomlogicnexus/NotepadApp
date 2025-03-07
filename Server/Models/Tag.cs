namespace NotepadApp.Server.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public virtual ICollection<NoteTag> Notes { get; set; } = new List<NoteTag>();
    }
}
