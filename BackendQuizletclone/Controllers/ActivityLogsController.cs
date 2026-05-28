using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendQuizletclone.Data;
using BackendQuizletclone.Models; // Đảm bảo gọi Models vào

namespace BackendQuizletclone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyProgressesController : ControllerBase
    {
        private readonly QuizletCloneDbContext _context;

        public StudyProgressesController(QuizletCloneDbContext context)
        {
            _context = context;
        }

        // Tạo 1 Class nhỏ để hứng dữ liệu từ Postman/Frontend gửi lên
        public class ReviewDto
        {
            public int UserId { get; set; }
            public int FlashcardId { get; set; }
            public int Grade { get; set; } // Điểm: 1 (Lại), 2 (Khó), 3 (Tốt), 4 (Dễ)

        }

        // API Chấm điểm và tính ngày ôn tiếp theo
        // Cách gọi: POST /api/StudyProgresses/review
        [HttpPost("review")]
        public async Task<IActionResult> ReviewCard([FromBody] ReviewDto request)
        {
            // 1. Tìm xem người này đã học từ này bao giờ chưa?
            var progress = await _context.StudyProgresses
                .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.FlashcardId == request.FlashcardId);

            // 2. Nếu chưa học bao giờ thì tạo mới lịch sử cho họ
            if (progress == null)
            {
                progress = new StudyProgress
                {
                    UserId = request.UserId,
                    FlashcardId = request.FlashcardId,
                    EaseFactor = 2.5, // Mặc định độ dễ là 2.5
                    Interval = 0,
                    Repetitions = 0
                };
                _context.StudyProgresses.Add(progress);
            }

            // 3. THUẬT TOÁN SM-2: Tính toán lại các chỉ số
            if (request.Grade >= 3) // Nếu chọn Tốt (3) hoặc Dễ (4)
            {
                if (progress.Repetitions == 0) progress.Interval = 1;
                else if (progress.Repetitions == 1) progress.Interval = 6;
                else progress.Interval = (int)Math.Round((double)progress.Interval.Value * (double)progress.EaseFactor.Value);

                progress.Repetitions++;
            }
            else // Nếu chọn Lại (1) hoặc Khó (2)
            {
                progress.Repetitions = 0;
                progress.Interval = 1;
            }

            // Tính lại độ dễ (Ease Factor) - Công thức gốc chuẩn SM-2
            progress.EaseFactor = (double)progress.EaseFactor.Value + (0.1 - (5 - request.Grade) * (0.08 + (5 - request.Grade) * 0.02));
            if (progress.EaseFactor < 1.3) progress.EaseFactor = 1.3; // Không bao giờ rớt dưới 1.3

            // Tính ngày ôn tiếp theo
            progress.NextReviewDate = DateTime.UtcNow.AddDays((double)progress.Interval.Value);
            progress.LastReviewedAt = DateTime.UtcNow;

            // 4. Lưu vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã lưu kết quả học tập!",
                nextReviewDate = progress.NextReviewDate,
                interval = progress.Interval
            });

        }
    }
} 