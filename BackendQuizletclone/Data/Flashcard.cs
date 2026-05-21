using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Đã có sẵn, quá ngon!

namespace BackendQuizletclone.Data;

public partial class Flashcard
{
    public int Id { get; set; }

    public string Term { get; set; } = null!;

    public string Definition { get; set; } = null!;

    public bool? IsStarred { get; set; }

    public int StudySetId { get; set; }

    public string? ImageUrl { get; set; }
   
    public string? Example { get; set; } 
    [JsonIgnore]
    public virtual ICollection<StudyProgress> StudyProgresses { get; set; } = new List<StudyProgress>();

    // 2. CHẶN VÒNG LẶP VỚI STUDYSET (Thủ phạm gây lỗi 500 hiện tại)
    [JsonIgnore]
    public virtual StudySet? StudySet { get; set; }
}