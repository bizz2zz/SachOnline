using LuongVinhKhang.SachOnline.Data;
using LuongVinhKhang.SachOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LuongVinhKhang.SachOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly BookstoreContext _context;

        public HomeController(BookstoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;
            var totalProducts = await _context.Product.CountAsync();
            var products = await _context.Product
                .Include(p => p.ChuDe)
                .Include(p => p.NhaXuatBan)
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var pagedProducts = new PagedList<Product>(products, page, pageSize, totalPages);

            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View(pagedProducts);
        }

        public async Task<IActionResult> BooksByChuDe(int id, int page = 1)
        {
            int pageSize = 6;
            var totalProducts = await _context.Product.CountAsync(p => p.ChuDeId == id);
            var products = await _context.Product
                .Include(p => p.ChuDe)
                .Include(p => p.NhaXuatBan)
                .Where(p => p.ChuDeId == id)
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var pagedProducts = new PagedList<Product>(products, page, pageSize, totalPages);

            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View("Index", pagedProducts);
        }

        public async Task<IActionResult> BooksByNhaXuatBan(int id, int page = 1)
        {
            int pageSize = 6;
            var totalProducts = await _context.Product.CountAsync(p => p.NhaXuatBanId == id);
            var products = await _context.Product
                .Include(p => p.ChuDe)
                .Include(p => p.NhaXuatBan)
                .Where(p => p.NhaXuatBanId == id)
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            var pagedProducts = new PagedList<Product>(products, page, pageSize, totalPages);

            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View("Index", pagedProducts);
        }

        public async Task<IActionResult> BookDetails(int id)
        {
            var product = await _context.Product
                .Include(p => p.ChuDe)
                .Include(p => p.NhaXuatBan)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View(product);
        }
    }
}