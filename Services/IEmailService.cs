using LuongVinhKhang.SachOnline.Models;

namespace LuongVinhKhang.SachOnline.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmail(User user, Order order, List<Cart> cartItems);
    }
}