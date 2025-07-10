using LuongVinhKhang.SachOnline.Data;
using LuongVinhKhang.SachOnline.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LuongVinhKhang.SachOnline.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOrderConfirmationEmail(User user, Order order, List<Cart> cartItems)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"Xác nhận đơn hàng #{order.OrderId} từ SachOnline",
                IsBodyHtml = true
            };
            mailMessage.To.Add(user.Email);

            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<h2>Xác nhận đơn hàng</h2>");
            bodyBuilder.AppendLine($"<p>Xin chào {user.HoTen},</p>");
            bodyBuilder.AppendLine("<p>Cảm ơn bạn đã đặt hàng tại SachOnline.</p>");
            bodyBuilder.AppendLine("<table border='1' style='width: 100%; border-collapse: collapse;'>");
            bodyBuilder.AppendLine("<tr><th>Tên sách</th><th>Số lượng</th><th>Giá</th><th>Thành tiền</th></tr>");
            foreach (var item in cartItems)
            {
                bodyBuilder.AppendLine("<tr>");
                bodyBuilder.AppendLine($"<td>{item.Product.Name}</td>");
                bodyBuilder.AppendLine($"<td>{item.Quantity}</td>");
                bodyBuilder.AppendLine($"<td>{item.Product.Price:N0} VNĐ</td>");
                bodyBuilder.AppendLine($"<td>{item.Product.Price * item.Quantity:N0} VNĐ</td>");
                bodyBuilder.AppendLine("</tr>");
            }
            bodyBuilder.AppendLine("</table>");
            bodyBuilder.AppendLine($"<p><strong>Tổng tiền:</strong> {order.TongTien:N0} VNĐ</p>");
            bodyBuilder.AppendLine("<p>Trân trọng cảm ơn quý khách!<br/>Mọi thắc mắc xin liên hệ: 0336863544</p>");

            mailMessage.Body = bodyBuilder.ToString();
            await client.SendMailAsync(mailMessage);
        }
    }
}