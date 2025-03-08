using System;
using System.Collections.Generic;

namespace NotepadApp.Shared.Models
{
  public class NoteDto
  {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;

    // Default constructor for serialization
    public NoteDto() { }

    // Constructor with parameters for convenience
    public NoteDto(string title, string content, List<string> tags = null)
    {
      Title = title;
      Content = content;
      Tags = tags ?? new List<string>();
      CreatedAt = DateTime.Now;
      LastModified = DateTime.Now;
    }

    // Clone method for creating copies
    public NoteDto Clone()
    {
      return new NoteDto
      {
        Id = this.Id,
        Title = this.Title,
        Content = this.Content,
        Tags = new List<string>(this.Tags),
        CreatedAt = this.CreatedAt,
        LastModified = this.LastModified
      };
    }
  }
}