using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuongVinhKhang.SachOnline.Data;
using LuongVinhKhang.SachOnline.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace LuongVinhKhang.SachOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductAdminController : Controller
    {
        private readonly BookstoreContext _context;

        public ProductAdminController(BookstoreContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductAdmin
        public async Task<IActionResult> Index()
        {
            var products = await _context.Product
                .Include(p => p.ChuDe)
                .Include(p => p.NhaXuatBan)
                .ToListAsync();

            return View(products);
        }

        // GET: Admin/ProductAdmin/Create

        public async Task<IActionResult> Create()
        {
            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NxbList = await _context.NhaXuatBan.ToListAsync();
            return View();
        }


        // POST: Admin/ProductAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NxbList = await _context.NhaXuatBan.ToListAsync();
            return View(product);
        }



        // GET: Admin/ProductAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null) return NotFound();

            // BẮT BUỘC PHẢI CÓ HAI DÒNG NÀY
            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NxbList = await _context.NhaXuatBan.ToListAsync();

            return View(product);
        }



        // POST: Admin/ProductAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Debug lỗi ModelState
            ViewBag.DebugInfo = "Model không hợp lệ:";
            foreach (var item in ModelState)
            {
                foreach (var error in item.Value.Errors)
                {
                    ViewBag.DebugInfo += $"<br /> - {item.Key}: {error.ErrorMessage}";
                }
            }

            // Tải lại danh sách ChuDe và NhaXuatBan
            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NxbList = await _context.NhaXuatBan.ToListAsync();

            return View(product);
        }

        // GET: Admin/ProductAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Product
                .Include(p => p.ChuDe)
                .Include(p => p.NhaXuatBan)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Admin/ProductAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product != null)
            {
                _context.Product.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}



//[HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> Edit(int id, Product product, IFormFile ImageFile)
//{
//    if (id != product.Id) return NotFound();

//    if (ImageFile != null && ImageFile.Length > 0)
//    {
//        var fileName = Path.GetFileName(ImageFile.FileName);
//        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

//        using (var stream = new FileStream(filePath, FileMode.Create))
//        {
//            await ImageFile.CopyToAsync(stream);
//        }

//        product.Image = fileName;
//    }

//    if (ModelState.IsValid)
//    {
//        _context.Update(product);
//        await _context.SaveChangesAsync();
//        return RedirectToAction(nameof(Index));
//    }

//    ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
//    ViewBag.NxbList = await _context.NhaXuatBan.ToListAsync();
//    return View(product);
//}
