using LuongVinhKhang.SachOnline.Data;
using LuongVinhKhang.SachOnline.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LuongVinhKhang.SachOnline.Controllers
{
    public class AccountController : Controller
    {
        private readonly BookstoreContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(BookstoreContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Login()
        {
            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string taiKhoan, string matKhau)
        {

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TaiKhoan == taiKhoan && u.MatKhau == matKhau);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Tài khoản hoặc mật khẩu không đúng.";
                ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
                ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
                ViewBag.SliderImages = await _context.Slider.ToListAsync();
                ViewBag.SachBanNhieu = await _context.Product
                    .Where(p => p.SoLuongBan > 50)
                    .OrderByDescending(p => p.SoLuongBan)
                    .Take(5)
                    .ToListAsync();
                return View();
            }


            // Đăng nhập bằng Cookie Authentication
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.TaiKhoan),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("HoTen", user.HoTen ?? "")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);


            // Lưu TaiKhoan vào session 
            HttpContext.Session.SetString("TaiKhoan", taiKhoan);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }

            TempData["SuccessMessage"] = "Đăng nhập thành công!";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Register()
        {
            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string hoTen, string taiKhoan, string matKhau, string confirmMatKhau, string email, string diaChi, string dienThoai, string ngaySinh)
        {

            if (matKhau != confirmMatKhau)
            {
                ViewBag.ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.";
                ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
                ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
                ViewBag.SliderImages = await _context.Slider.ToListAsync();
                ViewBag.SachBanNhieu = await _context.Product
                    .Where(p => p.SoLuongBan > 50)
                    .OrderByDescending(p => p.SoLuongBan)
                    .Take(5)
                    .ToListAsync();
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.TaiKhoan == taiKhoan))
            {
                ViewBag.ErrorMessage = "Tài khoản đã tồn tại.";
                ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
                ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
                ViewBag.SliderImages = await _context.Slider.ToListAsync();
                ViewBag.SachBanNhieu = await _context.Product
                    .Where(p => p.SoLuongBan > 50)
                    .OrderByDescending(p => p.SoLuongBan)
                    .Take(5)
                    .ToListAsync();
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.ErrorMessage = "Email đã được sử dụng.";
                ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
                ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
                ViewBag.SliderImages = await _context.Slider.ToListAsync();
                ViewBag.SachBanNhieu = await _context.Product
                    .Where(p => p.SoLuongBan > 50)
                    .OrderByDescending(p => p.SoLuongBan)
                    .Take(5)
                    .ToListAsync();
                return View();
            }

            DateTime? birthDate = string.IsNullOrEmpty(ngaySinh) ? (DateTime?)null : DateTime.Parse(ngaySinh);

            var user = new User
            {
                HoTen = hoTen,
                TaiKhoan = taiKhoan,
                MatKhau = matKhau,
                Email = email,
                DiaChi = diaChi,
                DienThoai = dienThoai,
                NgaySinh = birthDate,
                Role = "customer" 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            // Đăng nhập tự động sau khi đăng ký
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.TaiKhoan),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);

            HttpContext.Session.SetString("TaiKhoan", taiKhoan);
            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}