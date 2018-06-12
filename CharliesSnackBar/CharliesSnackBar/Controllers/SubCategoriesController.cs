using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models;
using CharliesSnackBar.Models.SubCategoryViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CharliesSnackBar.Controllers
{
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
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
                SubCategoryList = _db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToList()
            };
            return View(model);
        }
    }
}