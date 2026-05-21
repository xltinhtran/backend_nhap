/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendQuizletclone.Data;
using BackendQuizletclone.Models;

namespace BackendQuizletclone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly QuizletCloneDbContext _context;

        public AccountsController(QuizletCloneDbContext context)
        {
            _context = context;
        }

        // 1. Class để nhận dữ liệu đăng nhập
        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        // 2. API Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // Tìm user trong DB có username và password khớp (Lưu ý: Thực tế phải mã hóa mật khẩu)
            // Tìm user trong DB có username và password khớp
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
            {
                return BadRequest(new { message = "Sai tài khoản hoặc mật khẩu rồi ông ơi!" });
            }

            // Đăng nhập thành công, trả về thông tin User (trừ password để bảo mật)
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role, // Thêm đúng dòng này nè ông
                message = "Đăng nhập thành công!"
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User registerUser)
        {
            // Kiểm tra xem trùng tên hoặc email không
            if (await _context.Users.AnyAsync(u => u.Username == registerUser.Username || u.Email == registerUser.Email))
            {
                return BadRequest(new { message = "Username hoặc Email đã tồn tại!" });
            }

            // Ép cứng Role là Learner cho an toàn
            registerUser.Role = "Learner";

            _context.Users.Add(registerUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }


    }
}*/