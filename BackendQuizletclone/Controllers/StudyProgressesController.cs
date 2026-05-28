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
        // 2. API Lấy danh sách các từ cần ôn tập HÔM NAY của một bộ thẻ
        // Cách gọi: GET /api/StudyProgresses/due?userId=1&studySetId=1
        [HttpGet("due")]
        public async Task<IActionResult> GetDueFlashcards([FromQuery] int userId, [FromQuery] int studySetId)
        {
            var today = DateTime.UtcNow;

            var dueCards = await _context.Flashcards
                .Where(f => f.StudySetId == studySetId)
                .Where(f =>
                    // Điều kiện 1: Chưa học bao giờ
                    !_context.StudyProgresses.Any(p => p.FlashcardId == f.Id && p.UserId == userId)
                    ||
                    // Điều kiện 2: Đã đến hạn ôn tập (NextReviewDate nhỏ hơn hoặc bằng hôm nay)
                    _context.StudyProgresses.Any(p => p.FlashcardId == f.Id && p.UserId == userId && p.NextReviewDate <= today)
                )
                .ToListAsync();

            if (!dueCards.Any())
            {
                return Ok(new { message = "Tuyệt vời! Ông đã học hết các từ của hôm nay rồi.", data = dueCards });
            }

            return Ok(new { message = $"Ông có {dueCards.Count} từ cần ôn tập!", data = dueCards });
        }
        // 3. API Xóa sạch tiến độ (Reset) để học lại từ đầu
        // Cách gọi: DELETE /api/StudyProgresses/reset/1/1
        [HttpDelete("reset/{setId}/{userId}")]
        public async Task<IActionResult> ResetProgress(int setId, int userId)
        {
            // Dò tìm tất cả lịch sử học của ông trong bộ thẻ này
            var progressToReset = await _context.StudyProgresses
                .Where(p => p.UserId == userId && p.Flashcard.StudySetId == setId)
                .ToListAsync();

            if (progressToReset.Any())
            {
                // Xóa sạch sổ nợ
                _context.StudyProgresses.RemoveRange(progressToReset);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đã xóa sạch bộ nhớ bộ thẻ này, mời đại hiệp cày lại từ đầu!" });
        }
        [HttpGet("user/{userId}/stats")]
        public IActionResult GetUserProgress(int userId)
        {
            try
            {
                // 1. Từ Đã thuộc (Ôn tập thành công từ 3 lần trở lên)
                int mastered = _context.StudyProgresses
                    .Count(p => p.UserId == userId && p.Repetitions >= 3);

                // 2. Từ Đang học (Mới ôn tập được 1 hoặc 2 lần)
                int learning = _context.StudyProgresses
                    .Count(p => p.UserId == userId && p.Repetitions > 0 && p.Repetitions < 3);

                // 3. Từ Mới (Chưa ôn tập lần nào)
                int newWords = _context.StudyProgresses
                    .Count(p => p.UserId == userId && (p.Repetitions == 0 || p.Repetitions == null));

                // 4. Chuỗi ngày học (Hiện tại DB chưa có cột này nên gán cứng bằng 0)
                int streakCount = 0;

                return Ok(new
                {
                    masteredWords = mastered,
                    learningWords = learning,
                    newWords = newWords,
                    streak = streakCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Database: " + ex.Message });
            }
        }

    }

}
