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
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

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

        [Authorize]
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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart cartObj)
        {
            cartObj.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cartObj.ApplicationUserId = claim.Value;

                var cartFromDb = await _db.ShoppingCart.Where(x => x.ApplicationUserId == cartObj.ApplicationUserId
                                 && x.MenuItemId == cartObj.MenuItemId).FirstOrDefaultAsync();

                if (cartFromDb == null)
                {
                    //this menu item does not exists
                    _db.ShoppingCart.Add(cartObj);
                }
                else
                {
                    //menu item exists in shopping cart for that user, so just update the count
                    cartFromDb.Count = cartFromDb.Count + cartObj.Count;
                }

                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart.Where(x => x.ApplicationUserId == cartObj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("CartCount", count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var menuItemFromDb = await _db.MenuItem.Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Where(x => x.Id == cartObj.MenuItemId)
                .FirstOrDefaultAsync();

                var cartObj1 = new ShoppingCart()
                {
                    MenuItem = menuItemFromDb,
                    MenuItemId = menuItemFromDb.Id
                };

                return View(cartObj1);
            }
        }

        
    }
}
