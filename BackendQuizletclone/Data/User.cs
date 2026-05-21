using System;
using System.Collections.Generic;

namespace BackendQuizletclone.Data;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "Learner";

    public DateTime? CreatedAt { get; set; }


    public virtual ICollection<StudyProgress> StudyProgresses { get; set; } = new List<StudyProgress>();

    public virtual ICollection<StudySet> StudySets { get; set; } = new List<StudySet>();
}
