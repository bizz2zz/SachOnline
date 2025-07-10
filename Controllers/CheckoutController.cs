using LuongVinhKhang.SachOnline.Data;
using LuongVinhKhang.SachOnline.Models;
using LuongVinhKhang.SachOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LuongVinhKhang.SachOnline.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly BookstoreContext _context;
        private readonly ILogger<CheckoutController> _logger;
        private readonly IEmailService _emailService;

        public CheckoutController(BookstoreContext context, ILogger<CheckoutController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<IActionResult> Checkout()
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            _logger.LogInformation("Đang kiểm tra session TaiKhoan: {TaiKhoan}", taiKhoan ?? "null");
            if (string.IsNullOrEmpty(taiKhoan))
            {
                _logger.LogWarning("Session TaiKhoan không tồn tại, chuyển hướng đến Login.");
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TaiKhoan == taiKhoan);
            _logger.LogInformation("Tìm thấy user: {User}", user?.TaiKhoan ?? "null");
            if (user == null)
            {
                _logger.LogError("Không tìm thấy user với TaiKhoan: {TaiKhoan}", taiKhoan);
                return NotFound();
            }

            var cartItems = await _context.Cart
                .Include(c => c.Product)
                .Where(c => c.MaKH == user.MaKH)
                .ToListAsync();
            _logger.LogInformation("Số lượng sản phẩm trong giỏ hàng: {Count}", cartItems?.Count ?? 0);

            ViewBag.UserInfo = user;

            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string hoTen, string diaChi, string dienThoai, DateTime ngayGiao)
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            _logger.LogInformation("Đang xử lý PlaceOrder với TaiKhoan: {TaiKhoan}", taiKhoan ?? "null");
            if (string.IsNullOrEmpty(taiKhoan))
            {
                _logger.LogWarning("Session TaiKhoan không tồn tại, chuyển hướng đến Login.");
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TaiKhoan == taiKhoan);
            _logger.LogInformation("Tìm thấy user: {User}", user?.TaiKhoan ?? "null");
            if (user == null)
            {
                _logger.LogError("Không tìm thấy user với TaiKhoan: {TaiKhoan}", taiKhoan);
                return NotFound();
            }

            var cartItems = await _context.Cart
                .Include(c => c.Product)
                .Where(c => c.MaKH == user.MaKH)
                .ToListAsync();
            _logger.LogInformation("Số lượng sản phẩm trong giỏ hàng: {Count}", cartItems?.Count ?? 0);

            if (!cartItems.Any())
            {
                _logger.LogWarning("Giỏ hàng trống, chuyển hướng đến Cart.");
                return RedirectToAction("Cart", "Cart");
            }

            _logger.LogInformation("Dữ liệu form - HoTen: {HoTen}, DiaChi: {DiaChi}, DienThoai: {DienThoai}, NgayGiao: {NgayGiao}",
                hoTen, diaChi, dienThoai, ngayGiao);

            TempData["HoTen"] = hoTen;
            TempData["DiaChi"] = diaChi;
            TempData["DienThoai"] = dienThoai;
            TempData["NgayGiao"] = ngayGiao.ToString("yyyy-MM-ddTHH:mm:ss");
            _logger.LogInformation("Dữ liệu đã lưu vào TempData: HoTen={HoTen}, DiaChi={DiaChi}, DienThoai={DienThoai}, NgayGiao={NgayGiao}",
                hoTen, diaChi, dienThoai, ngayGiao);

            TempData.Keep();
            return RedirectToAction("PlaceOrder", "Order");
        }

        [HttpPost]
        public async Task<IActionResult> Notification(string hoTen, string diaChi, string dienThoai, DateTime ngayGiao)
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
            _logger.LogInformation("Đang xử lý Notification với TaiKhoan: {TaiKhoan}", taiKhoan ?? "null");
            if (string.IsNullOrEmpty(taiKhoan))
            {
                _logger.LogWarning("Session TaiKhoan không tồn tại, chuyển hướng đến Login.");
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TaiKhoan == taiKhoan);
            if (user == null)
            {
                _logger.LogError("Không tìm thấy user với TaiKhoan: {TaiKhoan}", taiKhoan);
                return NotFound();
            }

            var cartItems = await _context.Cart
                .Include(c => c.Product)
                .Where(c => c.MaKH == user.MaKH)
                .ToListAsync();
            if (!cartItems.Any())
            {
                _logger.LogWarning("Giỏ hàng trống, chuyển hướng đến Cart.");
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Cart", "Cart");
            }

            // Kiểm tra tồn kho
            //foreach (var item in cartItems)
            //{
            //    if (item.Product.Stock < item.Quantity)
            //    {
            //        _logger.LogWarning("Sản phẩm {ProductName} không đủ tồn kho: Yêu cầu={Quantity}, Tồn kho={Stock}", item.Product.Name, item.Quantity, item.Product.Stock);
            //        TempData["ErrorMessage"] = $"Sản phẩm {item.Product.Name} không đủ số lượng tồn kho.";
            //        return RedirectToAction("Cart", "Cart");
            //    }
            //}

            var tongTien = cartItems.Sum(c => c.Product.Price * c.Quantity);
            var order = new Order
            {
                MaKH = user.MaKH,
                HoTen = hoTen,
                DiaChi = diaChi,
                DienThoai = dienThoai,
                NgayDat = DateTime.Now,
                NgayGiao = ngayGiao,
                TongTien = tongTien,
                PaymentStatus = "Cash on Delivery",
                TrangThai = "Đang xử lý",
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    SoLuong = item.Quantity,
                    DonGia = item.Product.Price,
                    ThanhTien = item.Product.Price * item.Quantity
                });
            }

            try
            {
                await _emailService.SendOrderConfirmationEmail(user, order, cartItems);
                TempData["SuccessMessage"] = "Đặt hàng thành công! Kiểm tra email để xem chi tiết đơn hàng.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi email xác nhận đơn hàng thất bại.");
                TempData["ErrorMessage"] = "Đặt hàng thành công nhưng gửi email xác nhận thất bại.";
            }

            _context.Cart.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Notification", "Order");

        }
    }
}