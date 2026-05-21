using System;
using System.Collections.Generic;

namespace BackendQuizletclone.Data;

public partial class StudyProgress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int FlashcardId { get; set; }

    public double? EaseFactor { get; set; }

    public int? Interval { get; set; }

    public int? Repetitions { get; set; }

    public DateTime? NextReviewDate { get; set; }

    public DateTime? LastReviewedAt { get; set; }

    public virtual Flashcard Flashcard { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public bool IsStarred { get; set; } = false;

}
