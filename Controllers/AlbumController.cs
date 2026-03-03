using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicShop.Data;

namespace MusicShop.Controllers
{
    public class AlbumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlbumController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Album
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _context.Albums
                .Include(a => a.Category)
                .AsQueryable();

            // 搜尋
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.Title.Contains(search) ||
                    a.Artist.Contains(search));
                ViewBag.Search = search;
            }

            // 分類篩選
            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId);
                ViewBag.CategoryId = categoryId;
            }

            // 傳分類清單給 View 做篩選選單
            ViewBag.Categories = await _context.Categories.ToListAsync();

            var albums = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
            return View(albums);
        }

        // GET: /Album/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null) return NotFound();

            return View(album);
        }
    }
}