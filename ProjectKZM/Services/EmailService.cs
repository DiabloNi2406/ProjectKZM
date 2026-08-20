using System.Net;
using System.Net.Mail;

namespace ProjectKZM.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task SendOtpAsync(
            string email,
            string otp)
        {
            string? smtpHost =
                _configuration["Email:SmtpHost"];

            string? smtpPortString =
                _configuration["Email:SmtpPort"];

            string? smtpUsername =
                _configuration["Email:SmtpUsername"];

            string? smtpPassword =
                _configuration["Email:SmtpPassword"];

            string? fromEmail =
                _configuration["Email:FromEmail"];


            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpUsername) ||
                string.IsNullOrWhiteSpace(smtpPassword) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new Exception(
                    "Cấu hình Email chưa đầy đủ.");
            }


            int smtpPort = 587;


            if (!string.IsNullOrWhiteSpace(
                smtpPortString))
            {
                int.TryParse(
                    smtpPortString,
                    out smtpPort);
            }


            using var message =
                new MailMessage();


            message.From =
                new MailAddress(
                    fromEmail,
                    "Yuuji Store");


            message.To.Add(
                new MailAddress(email));


            message.Subject =
                "Mã OTP đặt lại mật khẩu - Yuuji Store";


            message.Body =
$"""
Xin chào,

Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản Yuuji Store.

Mã OTP của bạn là:

{otp}

Mã OTP có hiệu lực trong 5 phút.

Nếu bạn không yêu cầu đặt lại mật khẩu,
hãy bỏ qua email này.

Trân trọng,
Yuuji Store
""";


            message.IsBodyHtml = false;


            using var smtp =
                new SmtpClient(
                    smtpHost,
                    smtpPort);


            smtp.EnableSsl = true;

            smtp.Credentials =
                new NetworkCredential(
                    smtpUsername,
                    smtpPassword);


            await smtp.SendMailAsync(
                message);
        }
    }
}