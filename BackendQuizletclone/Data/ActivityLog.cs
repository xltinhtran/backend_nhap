using BackendQuizletclone.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizletCloneAPI.Models // Sửa namespace đúng với project của ông
{
    [Table("ActivityLogs")]
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActionText { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string BgColor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual User User { get; set; }
    }

    public class ActivityLogDto
    {
        public int UserId { get; set; }
        public string ActionText { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public string BgColor { get; set; }
    }
}