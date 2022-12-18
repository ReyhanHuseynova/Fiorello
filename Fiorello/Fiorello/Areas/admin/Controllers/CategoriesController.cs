using Fiorello.DAL;
using Fiorello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Areas.admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _db.Categories.ToListAsync();

            return View(categories);
        }
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            if (dbCategory.IsDeactive == true)
            {
                dbCategory.IsDeactive = false;
            }
            else
            {
                dbCategory.IsDeactive = true;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async  Task<IActionResult> Create(Category category)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

            bool isExist = await _db.Categories.AnyAsync(x => x.Name == category.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This category is already exist");
                return View();

            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            return View(dbCategory);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id,Category category)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            bool isExist = await _db.Categories.AnyAsync(x => x.Name == category.Name && x.Id == id);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This category is already exist");
                return View();

            }
            dbCategory.Name = category.Name;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }



    }
}
