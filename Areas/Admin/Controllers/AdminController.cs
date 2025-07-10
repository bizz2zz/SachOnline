using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuongVinhKhang.SachOnline.Data;
using System.Threading.Tasks;

namespace LuongVinhKhang.SachOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly BookstoreContext _context;

        public AdminController(BookstoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Product.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.TrangThai == "Đã giao") // hoặc bất kỳ logic nào hợp lý
                .SumAsync(o => (decimal?)o.TongTien) ?? 0;
            var monthlyRevenue = await _context.Orders
                .Where(o => o.TrangThai == "Đã giao")
                .GroupBy(o => new { o.NgayDat.Year, o.NgayDat.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(x => x.TongTien)
                })
                .ToListAsync(); // Tách sớm khỏi database

            // Chuyển sang dạng chuỗi đẹp
            ViewBag.RevenueLabels = monthlyRevenue
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => $"{x.Month:00}/{x.Year}")
                .ToList();

            ViewBag.RevenueData = monthlyRevenue
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => x.Revenue)
                .ToList();

            return View();
        }

    }
}