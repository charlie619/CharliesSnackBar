using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models.MenuItemViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CharliesSnackBar.Controllers
{
    public class MenuItemsController : Controller
    {        
            private readonly ApplicationDbContext _db;
            private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemsController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category.ToList(),
                MenuItem = new Models.MenuItem()
            };
        }
    
        //Get : MenuItems
        public async Task<IActionResult> Index()
        {
            var menuitems = _db.MenuItem.Include(x => x.Category).Include(x => x.SubCategory);
            return View(await menuitems.ToListAsync());
        }
    }
}