
using LuongVinhKhang.SachOnline.Data;
using LuongVinhKhang.SachOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HuynhVanQuang.SachOnline.Controllers
{
    public class CartController : Controller
    {
        private readonly BookstoreContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(BookstoreContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Cart()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var taiKhoan = User.Identity.Name;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.TaiKhoan == taiKhoan);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            if (user != null)
            {
                ViewBag.CartCount = await _context.Cart
                    .Where(c => c.MaKH == user.MaKH)
                    .SumAsync(c => (int?)c.Quantity) ?? 0;
            }
            else
            {
                ViewBag.CartCount = 0;
            }
            var cartItems = await _context.Cart
                .Include(c => c.Product)
                    .ThenInclude(p => p.ChuDe)
                .Include(c => c.Product.NhaXuatBan)
                .Where(c => c.MaKH == user.MaKH)
                .ToListAsync();


            ViewBag.ChuDeList = await _context.ChuDe.ToListAsync();
            ViewBag.NhaXuatBanList = await _context.NhaXuatBan.ToListAsync();
            ViewBag.SliderImages = await _context.Slider.ToListAsync();
            ViewBag.SachBanNhieu = await _context.Product
                .Where(p => p.SoLuongBan > 50)
                .OrderByDescending(p => p.SoLuongBan)
                .Take(5)
                .ToListAsync();

            return View("Cart", cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var taiKhoan = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.TaiKhoan == taiKhoan);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Sản phẩm không tồn tại.");
            }

            var existingCartItem = await _context.Cart
                .FirstOrDefaultAsync(c => c.MaKH == user.MaKH && c.ProductId == productId);

            int currentCartQuantity = existingCartItem?.Quantity ?? 0;
            int newTotalQuantity = currentCartQuantity + quantity;

            if (newTotalQuantity > product.SoLuong)
            {
                TempData["ErrorMessage"] = $"Sách \"{product.Name}\" chỉ còn {product.SoLuong} sản phẩm trong kho. Xin vui lòng chọn sản phẩm khác!";
                return RedirectToAction("Cart");
            }

            if (existingCartItem != null)
            {
                existingCartItem.Quantity = newTotalQuantity;
            }
            else
            {
                var cartItem = new Cart
                {
                    MaKH = user.MaKH,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.Cart.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Cart");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCart(List<CartUpdateViewModel> cartItems)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            bool hasAdjustment = false;

            foreach (var item in cartItems)
            {
                var cartItem = await _context.Cart.FindAsync(item.CartId);

                if (cartItem != null && item.Quantity > 0)
                {
                    var product = await _context.Product.FindAsync(cartItem.ProductId);
                    if (product != null)
                    {
                        if (item.Quantity > product.SoLuong)
                        {
                            cartItem.Quantity = product.SoLuong;
                            hasAdjustment = true;
                        }
                        else
                        {
                            cartItem.Quantity = item.Quantity;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (hasAdjustment)
            {
                TempData["ErrorMessage"] = "Một số sản phẩm đã được điều chỉnh về số lượng tối đa hiện có trong kho.";
            }

            return RedirectToAction("Cart");
        }



        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _context.Cart.FindAsync(cartId);
            if (cartItem != null)
            {
                _context.Cart.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning($"RemoveFromCart: Không tìm thấy cartItem, CartId={cartId}");
            }
            return RedirectToAction("Cart");
        }
    }
}