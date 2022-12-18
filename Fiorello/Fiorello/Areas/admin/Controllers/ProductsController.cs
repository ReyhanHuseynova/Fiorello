using Fiorello.DAL;
using Fiorello.Helpers;
using Fiorello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Fiorello.Areas.admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _db.Products.Include(x =>x.Category).ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories= await _db.Categories.ToListAsync();
            return View();  
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, int catId)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (product.Photo == null)
            {
                ModelState.AddModelError("Photo", "Shekil sech");
                return View();
            }
            if (!product.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", "Shekil formati sech");
                return View();
            }
            if (product.Photo.IsOlder2Mb())
            {
                ModelState.AddModelError("Photo", "Max 2Mb");
                return View();
            }
            string folder = Path.Combine(_env.WebRootPath, "img");
            product.Image = await product.Photo.SaveImageAsync(folder);

            product.CategoryId= catId;
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product dbProduct = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(dbProduct);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product dbProduct = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            ViewBag.Categories = await _db.Categories.ToListAsync();
            if(!ModelState.IsValid)
            {
                return View(dbProduct);
            }

            if (product.Photo != null)
            {
                if (!product.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Shekil formati sech");
                    return View(dbProduct);
                }
                if (product.Photo.IsOlder2Mb())
                {
                    ModelState.AddModelError("Photo", "Max 2Mb");
                    return View(dbProduct);
                }
                string folder = Path.Combine(_env.WebRootPath, "img", "slider");
                string path = Path.Combine(folder, dbProduct.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                dbProduct.Image = await product.Photo.SaveImageAsync(folder);
            }


            dbProduct.Name = product.Name;
            dbProduct.Price = product.Price;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product dbProduct = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            if (dbProduct.IsDeactive == true)
            {
                dbProduct.IsDeactive = false;
            }
            else
            {
                dbProduct.IsDeactive = true;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return BadRequest();
            }

            return View(product);
        }
    }
}
