using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendQuizletclone.Data;
// using BackendQuizletclone.Models; (Nếu code báo đỏ chữ Flashcard thì ông gõ thêm dòng này)

namespace BackendQuizletclone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlashcardsController : ControllerBase
    {
        private readonly QuizletCloneDbContext _context;

        public FlashcardsController(QuizletCloneDbContext context)
        {
            _context = context;
        }

        // 1. Lấy toàn bộ thẻ từ của một Bộ thẻ (VD: Lấy hết từ vựng của bộ ID số 1)
        // Cách gọi: GET /api/Flashcards/ByStudySet/1
        [HttpGet("ByStudySet/{studySetId}")]
        public async Task<IActionResult> GetFlashcardsBySet(int studySetId)
        {
            var cards = await _context.Flashcards
                                      .Where(f => f.StudySetId == studySetId)
                                      .ToListAsync();
            return Ok(cards);
        }

        // 2. Thêm một thẻ từ mới
        // Cách gọi: POST /api/Flashcards
        [HttpPost]
        public async Task<IActionResult> CreateFlashcard(Flashcard newCard)
        {
            _context.Flashcards.Add(newCard);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm từ vựng thành công!", data = newCard });
        }
        // 3. Xóa một thẻ từ vựng (Dành cho nút thùng rác nhỏ)
        // Cách gọi: DELETE /api/Flashcards/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlashcard(int id)
        {
            // Bước 1: Tìm xem thẻ này có tồn tại trong SQL không
            var card = await _context.Flashcards.FindAsync(id);
            if (card == null)
            {
                return NotFound(new { message = "Không tìm thấy thẻ này để xóa!" });
            }

            // Bước 2: Ra lệnh xóa và lưu lại thay đổi
            _context.Flashcards.Remove(card);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa thẻ vĩnh viễn khỏi SQL!" });
        }
        // Đã sửa Route để nhận thêm userId từ Javascript
        [HttpPost("toggle-star/{id}/{userId}")]
        public async Task<IActionResult> ToggleStar(int id, int userId)
        {
            try
            {
                // Tìm xem user này đã có sổ tiến độ (StudyProgress) cho thẻ này chưa
                var progress = await _context.StudyProgresses
                    .FirstOrDefaultAsync(p => p.FlashcardId == id && p.UserId == userId);

                if (progress == null)
                {
                    // Nếu chưa từng học mà ngứa tay bấm sao -> Tạo sổ mới cho nó
                    progress = new StudyProgress
                    {
                        FlashcardId = id,
                        UserId = userId,
                        IsStarred = true, // Lần đầu bấm chắc chắn là gắn sao
                        Repetitions = 0,
                        Interval = 1,
                        EaseFactor = 2.5,
                        NextReviewDate = DateTime.Now
                    };
                    _context.StudyProgresses.Add(progress);
                }
                else
                {
                    // Nếu có sổ rồi thì chỉ cần đảo trạng thái sao (Bật <-> Tắt)
                    progress.IsStarred = !progress.IsStarred;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật sao cá nhân thành công!",
                    isStarred = progress.IsStarred
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlashcard(int id, Flashcard updatedCard)
        {
            if (id != updatedCard.Id) return BadRequest("Lỗi ID không khớp!");

            var card = await _context.Flashcards.FindAsync(id);
            if (card == null) return NotFound("Không tìm thấy thẻ!");

            // Cập nhật thông tin
            card.Term = updatedCard.Term;
            card.Definition = updatedCard.Definition;
            card.ImageUrl = updatedCard.ImageUrl;
            card.Example = updatedCard.Example;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

    }
}