using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models;
using CharliesSnackBar.Models.OrderDetailsViewModel;
using CharliesSnackBar.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CharliesSnackBar.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public OrderDetailsCart detailCart { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET : Cart/Index
        public IActionResult Index()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new OrderHeader()
            };

            detailCart.OrderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _db.ShoppingCart.Where(x => x.ApplicationUserId == claim.Value);

            if (cart != null)
            {
                detailCart.listCart = cart.ToList();
            }

            foreach (var list in detailCart.listCart)
            {
                list.MenuItem = _db.MenuItem.FirstOrDefault(x => x.Id == list.MenuItemId);
                detailCart.OrderHeader.OrderTotal += (list.MenuItem.Price * list.Count);

                if (list.MenuItem.Description.Length > 100)
                {
                    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                }
            }

            detailCart.OrderHeader.PickUp = DateTime.Now;
            return View(detailCart);
        }

        // POST : Cart/Index
        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detailCart.listCart = _db.ShoppingCart.Where(x => x.ApplicationUserId == claim.Value).ToList();

            detailCart.OrderHeader.OrderDate = DateTime.Now;
            detailCart.OrderHeader.UserId = claim.Value;
            detailCart.OrderHeader.Status = SD.StatusSubmitted;
            var orderHeader = detailCart.OrderHeader;
            _db.OrderHeader.Add(orderHeader);
            _db.SaveChanges();

            foreach (var item in detailCart.listCart)
            {
                item.MenuItem = _db.MenuItem.FirstOrDefault(x => x.Id == item.MenuItemId);
                var orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = orderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                _db.OrderDetails.Add(orderDetails);
            }

            _db.ShoppingCart.RemoveRange(detailCart.listCart);
            _db.SaveChanges();
            HttpContext.Session.SetInt32("CartCount", 0);

            return RedirectToAction("Confirm", "Order", new { id = orderHeader.Id });
        }

        // Adding 1 count of an item in shopping cart
        public IActionResult Plus(int cartId)
        {
            var cart = _db.ShoppingCart.Where(x => x.Id == cartId).FirstOrDefault();
            cart.Count += 1;
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Reducing 1 count of an item in shopping cart
        public IActionResult Minus(int cartId)
        {
            var cart = _db.ShoppingCart.Where(x => x.Id == cartId).FirstOrDefault();
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                _db.SaveChanges();

                var cnt = _db.ShoppingCart.Where(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("CartCount", cnt);
            }
            else
            {
                cart.Count -= 1;
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}