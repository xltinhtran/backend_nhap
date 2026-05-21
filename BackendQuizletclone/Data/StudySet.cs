using System;
using System.Collections.Generic;

namespace BackendQuizletclone.Data;

public partial class StudySet
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsPublic { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();

    public virtual User? User { get; set; } 
}
