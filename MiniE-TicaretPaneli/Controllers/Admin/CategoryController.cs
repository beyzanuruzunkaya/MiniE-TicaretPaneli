using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiniE_TicaretPaneli.Data;
using MiniE_TicaretPaneli.Models;
using MiniE_TicaretPaneli.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MiniE_TicaretPaneli.Controllers.Admin
{
    
    [Route("Admin/[controller]/[action]")]
    //[Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            ViewData["Title"] = "Kategori Yönetimi";
            var categories = await _context.Categories.Include(c => c.ParentCategory).ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> AddCategory()
        {
            ViewData["Title"] = "Yeni Kategori Ekle";
            var viewModel = new AddCategoryViewModel
            {
                MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"{key}: {error.ErrorMessage}");
                        }
                    }
                    viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
                    return View(viewModel);
                }
                var category = new Category
                {
                    Name = viewModel.Name,
                    ParentCategoryId = viewModel.ParentCategoryId,
                    Gender = viewModel.Gender,
                    Slug = viewModel.Slug
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kategori başarıyla eklendi!";
                return RedirectToAction(nameof(Categories));
            }
            viewModel.MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var viewModel = new AddCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId,
                Gender = category.Gender,
                Slug = category.Slug,
                CategoryLevelType = category.ParentCategoryId == null ? "MainGroup" : "ProductType",
                MainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync()
            };
            ViewBag.ParentCategories = _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.Id) { return NotFound(); }
            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) { return NotFound("Kategori bulunamadı."); }
            if (await _context.Products.AnyAsync(p => p.MainCategoryId == id || p.SubCategoryId == id) ||
                await _context.Categories.AnyAsync(c => c.ParentCategoryId == id))
            {
                return BadRequest("Bu kategoriye bağlı ürünler veya alt kategoriler bulunmaktadır. Silinemez!");
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetSubcategoriesByMainCategory(int mainCategoryId)
        {
            var subcategories = await _context.Categories
                                              .Where(c => c.ParentCategoryId == mainCategoryId)
                                              .Select(c => new { id = c.Id, name = c.Name, gender = c.Gender })
                                              .ToListAsync();
            return Json(subcategories);
        }

        [HttpGet]
        public async Task<JsonResult> GetMainCategories()
        {
            var mainCategories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.ParentCategoryId != null)
                .Select(c => new { id = c.Id, name = c.Name, parentName = c.ParentCategory != null ? c.ParentCategory.Name : "" })
                .ToListAsync();
            return Json(mainCategories);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllMainCategoriesWithParent()
        {
            var mainCategories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.ParentCategoryId != null)
                .Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    parentName = c.ParentCategory != null ? c.ParentCategory.Name : ""
                })
                .ToListAsync();
            return Json(mainCategories);
        }

        [HttpGet]
        public async Task<JsonResult> GetRootCategories()
        {
            var rootCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();
            return Json(rootCategories);
        }

        [HttpGet]
        public JsonResult GetSubCategories(int mainCategoryId)
        {
            var subCategories = _context.Categories
        .Where(c => c.ParentCategoryId == mainCategoryId)
        .Select(c => new
        {
            id = c.Id,
            name = c.Name
        }).ToList();

            return Json(subCategories);

          
        }
    }
} 