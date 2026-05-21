using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using BackendQuizletclone.Data; // Sửa lại đúng namespace của ông

namespace BackendQuizletclone.Services
{
    public class StudyReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        // Khoảng thời gian giữa các lần quét (Ví dụ: 1 tiếng quét 1 lần)
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        public StudyReminderWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<QuizletCloneDbContext>();

                    // 1. Quét tìm các User có thẻ đã đến hạn hoặc quá hạn ôn tập
                    var usersToRemind = await context.StudyProgresses
                        .Where(p => p.NextReviewDate <= DateTime.Now)
                        .Select(p => p.User)
                        .Distinct()
                        .ToListAsync(stoppingToken);

                    // 2. Duyệt qua từng người và gửi mail nhắc nhở
                    foreach (var user in usersToRemind)
                    {
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            await SendEmailAsync(user.Email, user.Username);
                        }
                    }
                }

                // Chờ 1 tiếng sau mới quét tiếp
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        // Hàm gửi Mail dùng SMTP của Gmail (Hoàn toàn miễn phí)
        private async Task SendEmailAsync(string toEmail, string username)
        {
            try
            {
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MailboxAddress("StudySet Notification", "your_gmail@gmail.com")); // Email của ông
                message.To.Add(new MailboxAddress(username, toEmail));
                message.Subject = "📚 [StudySet] Nhắc nhở: Bạn có từ vựng đến hạn ôn tập hôm nay!";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; padding: 20px; rounded-xl;'>
                        <h2 style='color: #4f46e5;'>Chào {username} ní ơi! 👋</h2>
                        <p style='font-size: 16px; color: #334155;'>Hệ thống Spaced Repetition phát hiện bạn đang có một số từ vựng đến hạn cần ôn tập để tránh bị quên lãng.</p>
                        <p style='font-size: 16px; color: #334155;'>Học tập đều đặn hàng ngày là chìa khóa để đạt trình độ cao hơn đó!</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='http://127.0.0.1:5500/index.html?login={username}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; font-weight: bold; border-radius: 8px;'>Vào Học Ngay Thôi</a>
                        </div>
                        <hr style='border: 0; border-top: 1px solid #e2e8f0;'>
                        <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Tin nhắn này được gửi tự động từ hệ thống Quizlet Clone của ông.</p>
                    </div>";

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("sandbox.smtp.mailtrap.io", 2525, MailKit.Security.SecureSocketOptions.StartTls);

                    // Dán cái Username và Password của Mailtrap vào đây (Nhớ giữ lại dấu ngoặc kép "")
                    await client.AuthenticateAsync("11c78982ef24a0", "0d08d529e4e64a");

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                Console.WriteLine($"[Email Sent] Đã gửi mail nhắc nhở thành công cho: {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] Không gửi được mail: {ex.Message}");
            }
        }
    }
}