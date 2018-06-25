using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CharliesSnackBar.Models;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models.HomeViewModel;
using Microsoft.EntityFrameworkCore;

namespace CharliesSnackBar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var IndexVM = new IndexViewModel()
            {
                MenuItem = await _db.MenuItem.Include(x => x.Category)
                .Include(x => x.SubCategory).ToListAsync(),

                Category = _db.Category.OrderBy(x => x.DsplayOrder),
                Coupons = _db.Coupons.Where(x =>x.IsActive == true).ToList()
            };
            return View(IndexVM);
        }      
        
        // GET : Details
        public async Task<IActionResult> Details(int id)
        {
            var menuItemFromDb = await _db.MenuItem.Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            var cartObj = new ShoppingCart()
            {
                MenuItem = menuItemFromDb,
                MenuItemId = menuItemFromDb.Id
            };

            return View(cartObj);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
