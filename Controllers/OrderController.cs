using LuongVinhKhang.SachOnline.Data;
using LuongVinhKhang.SachOnline.Models;
using LuongVinhKhang.SachOnline.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuongVinhKhang.SachOnline.Controllers
{
    public class OrderController : Controller
    {
        private readonly BookstoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderController> _logger;
        private readonly IEmailService _emailService;

        public OrderController(BookstoreContext context, IConfiguration configuration, ILogger<OrderController> logger, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string hoTen, string diaChi, string dienThoai, DateTime ngayGiao)
        {
            var taiKhoan = HttpContext.Session.GetString("TaiKhoan");
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

            if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(diaChi) || string.IsNullOrEmpty(dienThoai))
            {
                _logger.LogWarning("Thiếu thông tin đặt hàng: HoTen={HoTen}, DiaChi={DiaChi}, DienThoai={DienThoai}", hoTen, diaChi, dienThoai);
                return BadRequest("Thiếu thông tin đặt hàng.");
            }

            var cartItems = await _context.Cart.Include(c => c.Product).Where(c => c.MaKH == user.MaKH).ToListAsync();
            if (!cartItems.Any())
            {
                _logger.LogWarning("Giỏ hàng trống, chuyển hướng đến Cart.");
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
                PaymentStatus = "Pending",
                TrangThai = "Đang xử lý"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var returnUrl = Url.Action("PaymentCallback", "Order", null, Request.Scheme);
            var onePayUrl = CreateOnePayPaymentUrl(order, returnUrl);
            return Redirect(onePayUrl);
        }

        public async Task<IActionResult> PaymentCallback()
        {
            var txnResponseCode = Request.Query["vpc_TxnResponseCode"];
            var orderId = Request.Query["vpc_MerchTxnRef"];

            if (txnResponseCode == "0" && int.TryParse(orderId, out int id))
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null)
                {
                    var cartItems = await _context.Cart.Include(c => c.Product).Where(c => c.MaKH == order.MaKH).ToListAsync();

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

                    order.PaymentStatus = "Completed";
                    await _context.SaveChangesAsync();

                    try
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.MaKH == order.MaKH);
                        if (user != null)
                        {
                            await _emailService.SendOrderConfirmationEmail(user, order, cartItems);
                            TempData["SuccessMessage"] = "Thanh toán thành công qua OnePay! Kiểm tra email.";
                        }
                        else
                        {
                            _logger.LogError("Không tìm thấy user với MaKH: {MaKH}", order.MaKH);
                            TempData["ErrorMessage"] = "Thanh toán thành công nhưng không tìm thấy thông tin người dùng để gửi email.";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Gửi email xác nhận đơn hàng thất bại.");
                        TempData["ErrorMessage"] = "Thanh toán thành công nhưng gửi email xác nhận thất bại.";
                    }

                    _context.Cart.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError("Không tìm thấy đơn hàng với ID: {OrderId}", id);
                    TempData["ErrorMessage"] = "Thanh toán thất bại: Không tìm thấy đơn hàng.";
                }
            }
            else
            {
                _logger.LogWarning("Thanh toán thất bại với mã phản hồi: {ResponseCode}", txnResponseCode);
                TempData["ErrorMessage"] = "Thanh toán thất bại hoặc bị hủy.";
            }

            return RedirectToAction("Notification");
        }

        private string CreateOnePayPaymentUrl(Order order, string returnUrl)
        {
            var secureSecret = _configuration["OnePay:SecureSecret"];
            var baseUrl = _configuration["OnePay:BaseUrl"] ?? "https://mtf.onepay.vn/vpcpay/vpcpay.op";
            var merchant = _configuration["OnePay:Merchant"];
            var accessCode = _configuration["OnePay:AccessCode"];

            var vpc_Params = new SortedList<string, string>
            {
                { "vpc_Version", "2" },
                { "vpc_Command", "pay" },
                { "vpc_AccessCode", accessCode },
                { "vpc_Merchant", merchant },
                { "vpc_Locale", "en" },
                { "vpc_ReturnURL", returnUrl },
                { "vpc_MerchTxnRef", order.OrderId.ToString() },
                { "vpc_OrderInfo", $"Order {order.OrderId}" },
                { "vpc_Amount", ((int)(order.TongTien * 100)).ToString() },
                { "vpc_Currency", "VND" },
                { "vpc_TicketNo", HttpContext.Connection.RemoteIpAddress?.ToString() }
            };

            var query = new StringBuilder();
            var hashData = new StringBuilder();
            foreach (var kvp in vpc_Params)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    query.Append($"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}&");
                    hashData.Append($"{kvp.Key}={kvp.Value}&");
                }
            }

            var rawHashData = hashData.ToString().TrimEnd('&');
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secureSecret));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHashData));
            var secureHash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

            return $"{baseUrl}?{query}vpc_SecureHash={secureHash}&vpc_SecureHashType=SHA256";
        }

        public IActionResult Notification()
        {
            return View();
        }
    }
}