using BackendQuizletclone.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
// Nhớ using Models và Data Context của ông nha

namespace BackendQuizletClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly QuizletCloneDbContext _context; // Đổi thành tên DbContext của ông

        public UsersController(QuizletCloneDbContext context)
        {
            _context = context;
        }

        // 1. API Lấy danh sách tất cả tài khoản
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Username, u.Email, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        // 2. API Xóa tài khoản
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy user!" });

            // Xóa luôn mấy cái tiến độ học của nó cho sạch Database
            var progresses = _context.StudyProgresses.Where(p => p.UserId == id);
            _context.StudyProgresses.RemoveRange(progresses);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa tài khoản thành công!" });
        }
    }
}