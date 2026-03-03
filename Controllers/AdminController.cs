using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MusicShop.Data;
using MusicShop.Models;

namespace MusicShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── 後台首頁 ───────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewBag.AlbumCount = await _context.Albums.CountAsync();
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.UserCount = await _context.Users.CountAsync();
            return View();
        }

        // ─── 專輯管理 ───────────────────────────────────────
        public async Task<IActionResult> Albums()
        {
            var albums = await _context.Albums
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(albums);
        }

        public IActionResult AlbumCreate()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumCreate(Album album)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
                return View(album);
            }
            album.CreatedAt = DateTime.Now;
            _context.Albums.Add(album);
            await _context.SaveChangesAsync();
            TempData["Success"] = "專輯新增成功！";
            return RedirectToAction("Albums");
        }

        public async Task<IActionResult> AlbumEdit(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null) return NotFound();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", album.CategoryId);
            return View(album);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumEdit(Album album)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", album.CategoryId);
                return View(album);
            }
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();
            TempData["Success"] = "專輯更新成功！";
            return RedirectToAction("Albums");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AlbumDelete(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album != null)
            {
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();
                TempData["Success"] = "專輯刪除成功！";
            }
            return RedirectToAction("Albums");
        }

        // ─── 分類管理 ───────────────────────────────────────
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Albums)
                .ToListAsync();
            return View(categories);
        }

        public IActionResult CategoryCreate() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "分類新增成功！";
            return RedirectToAction("Categories");
        }

        public async Task<IActionResult> CategoryEdit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "分類更新成功！";
            return RedirectToAction("Categories");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Albums)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                if (category.Albums.Any())
                {
                    TempData["Error"] = "此分類下還有專輯，無法刪除！";
                    return RedirectToAction("Categories");
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "分類刪除成功！";
            }
            return RedirectToAction("Categories");
        }

        // ─── 訂單管理 ───────────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Album)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "訂單狀態更新成功！";
            }
            return RedirectToAction("OrderDetail", new { id = orderId });
        }
    }
}