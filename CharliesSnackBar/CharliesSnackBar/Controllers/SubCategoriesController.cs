using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
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
    }
}