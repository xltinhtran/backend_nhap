using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendQuizletclone.Data; // Gọi namespace chứa DbContext và Models của ông
using System.Linq;
using System.Threading.Tasks;

namespace BackendQuizletclone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudySetsController : ControllerBase
    {
        private readonly QuizletCloneDbContext _context;

        // Bơm DbContext vào đây để xài
        public StudySetsController(QuizletCloneDbContext context)
        {
            _context = context;
        }

        // 1. API Lấy toàn bộ danh sách bộ từ vựng (ĐÃ NÂNG CẤP ĐỂ BIẾT TỪ NÀO ĐÃ HỌC + ĐẾM SỐ NGƯỜI HỌC)
        // Cách gọi: GET /api/StudySets?userId=1
        [HttpGet]
        public async Task<IActionResult> GetAllStudySets([FromQuery] int userId = 0)
        {
            var studySets = await _context.StudySets
                .Select(set => new
                {
                    id = set.Id,
                    title = set.Title,
                    description = set.Description,
                    createdAt = set.CreatedAt,

                    // 🔥 BỔ SUNG: ĐẾM SỐ NGƯỜI HỌC DUY NHẤT TRONG BỘ THẺ NÀY
                    learnerCount = _context.StudyProgresses
                        .Where(p => p.Flashcard.StudySetId == set.Id)
                        .Select(p => p.UserId)
                        .Distinct()
                        .Count(),

                    // Quét từng thẻ trong bộ này và ghép điểm của User vào
                    flashcards = set.Flashcards.Select(card => new
                    {
                        id = card.Id,
                        term = card.Term,
                        definition = card.Definition,
                        example = card.Example,
                        imageUrl = card.ImageUrl,

                        isStarred = _context.StudyProgresses
                                      .Where(p => p.FlashcardId == card.Id && p.UserId == userId)
                                      .Select(p => p.IsStarred)
                                      .FirstOrDefault(),

                        // Móc sổ điểm (StudyProgresses) của riêng User này ra
                        repetitions = _context.StudyProgresses
                            .Where(p => p.FlashcardId == card.Id && p.UserId == userId)
                            .Select(p => (int?)p.Repetitions)
                            .FirstOrDefault() ?? 0,

                        interval = _context.StudyProgresses
                            .Where(p => p.FlashcardId == card.Id && p.UserId == userId)
                            .Select(p => (int?)p.Interval)
                            .FirstOrDefault() ?? 1,

                        easeFactor = _context.StudyProgresses
                            .Where(p => p.FlashcardId == card.Id && p.UserId == userId)
                            .Select(p => (double?)p.EaseFactor)
                            .FirstOrDefault() ?? 2.5,

                        nextReviewDate = _context.StudyProgresses
                            .Where(p => p.FlashcardId == card.Id && p.UserId == userId)
                            .Select(p => (System.DateTime?)p.NextReviewDate)
                            .FirstOrDefault()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(studySets);
        }
        // 2. API Tạo một bộ từ vựng mới
        // Khi gọi POST /api/StudySets, nó sẽ chạy vào đây
        [HttpPost]
        public async Task<IActionResult> CreateStudySet(StudySet newSet)
        {
            // Nhét dữ liệu mới vào CSDL
            _context.StudySets.Add(newSet);
            await _context.SaveChangesAsync(); // Lưu lại (Commit)

            return Ok(new { message = "Đã tạo bộ từ vựng thành công!", data = newSet });
        }

        // 3. Xóa TOÀN BỘ một bộ thẻ (Dành cho nút thùng rác bự)
        // Cách gọi: DELETE /api/StudySets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudySet(int id)
        {
            var studySet = await _context.StudySets.FindAsync(id);
            if (studySet == null)
            {
                return NotFound(new { message = "Không tìm thấy bộ thẻ!" });
            }

            _context.StudySets.Remove(studySet);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa sạch bộ thẻ và các từ vựng bên trong!" });
        }

        // 4. API tính % cho màn hình Dashboard
        // Cách gọi: GET /api/StudySets/dashboard/1
        [HttpGet("dashboard/{userId}")]
        public async Task<IActionResult> GetDashboardProgress(int userId)
        {
            var dashboardData = await _context.StudySets
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    TotalCards = s.Flashcards.Count,
                    // Đếm những thẻ ĐÃ CÓ trong bảng StudyProgresses của User này
                    LearnedCards = _context.StudyProgresses.Count(p => p.Flashcard.StudySetId == s.Id && p.UserId == userId)
                })
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.TotalCards,
                    s.LearnedCards,
                    // Toán học lớp 3: Tính phần trăm
                    ProgressPercent = s.TotalCards > 0 ? (s.LearnedCards * 100 / s.TotalCards) : 0
                })
                .ToListAsync();

            return Ok(dashboardData);
        }
      
    }
}