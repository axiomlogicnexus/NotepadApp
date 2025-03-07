namespace NotepadApp.Server.Models
{
    public class NoteTag
    {
        public string NoteId { get; set; }
        public int TagId { get; set; }
        
        public virtual Note Note { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
