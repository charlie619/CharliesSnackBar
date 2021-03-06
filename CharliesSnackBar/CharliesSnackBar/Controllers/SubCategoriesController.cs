﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models;
using CharliesSnackBar.Models.SubCategoryViewModels;
using CharliesSnackBar.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CharliesSnackBar.Controllers
{
    [Authorize(Roles = SD.AdminEndUser)]

    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }
        //Get Action
        public async Task<IActionResult> Index()
        {
            var subCategories = _db.SubCategory.Include(x => x.Category);
            return View(await subCategories.ToListAsync());
        }

        //Get Action for Create        
        public IActionResult Create()
        {
            var model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = new SubCategory(),
                SubCategoryList = _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToList()
            };
            return View(model);
        }

        //Post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subCategoryExists = _db.SubCategory
                    .Where(x => x.Name == model.SubCategory.Name).Count();

                var subCatAndCatExists = _db.SubCategory
                    .Where(x => x.Name == model.SubCategory.Name && x.CategoryId==model.SubCategory
                    .CategoryId).Count();

                if (subCategoryExists > 0 && model.IsNew)
                {
                    //error
                    StatusMessage = "Error : Sub Category Name already exists.";
                }
                else
                {
                    if(subCategoryExists == 0 && !model.IsNew)
                    {
                        //error
                        StatusMessage = "Error : Sub Category does not exists.";
                    }
                    else
                    {
                        if (subCatAndCatExists > 0)
                        {
                            //error
                            StatusMessage = "Error : Category and Sub Category combination exists.";
                        }
                        else
                        {
                            _db.Add(model.SubCategory);
                            await _db.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            var modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToList(),
                StatusMessage = StatusMessage
            };
            return View(modelVM);
        }

        //Get Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(x => x.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            var model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = subCategory,
                SubCategoryList = _db.SubCategory.Select(x => x.Name).Distinct().ToList()
            };
            return View(model);
        }

        //Post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subCategoryExists = _db.SubCategory
                    .Where(x => x.Name == model.SubCategory.Name).Count();

                var subCatAndCatExists = _db.SubCategory
                    .Where(x => x.Name == model.SubCategory.Name && x.CategoryId == model.SubCategory
                    .CategoryId).Count();

                if (subCategoryExists == 0)
                {
                    StatusMessage = "Error : Sub Category does not exist. You can not add a new subcategory here.";
                }
                else
                {
                    if (subCatAndCatExists > 0)
                    {
                        StatusMessage = "Error : Category and Sub Category combination exist.";
                    }
                    else
                    {
                        var subCatFromDb = _db.SubCategory.Find(id);
                        subCatFromDb.Name = model.SubCategory.Name;
                        subCatFromDb.CategoryId = model.SubCategory.CategoryId;
                        await _db.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }                    
                }
            }
            var modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategory.Select(x => x.Name).Distinct().ToList(),
                StatusMessage = StatusMessage
            };
            return View(modelVM);
        }

        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        //Get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        //POST Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(x => x.Id == id);
            _db.SubCategory.Remove(subCategory);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}