using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models.OrderDetailsViewModel;
using CharliesSnackBar.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CharliesSnackBar.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET : Confirm (Place Order)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var OrderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = _db.OrderHeader.Where(x => x.Id == id && x.UserId == claim.Value).FirstOrDefault(),
                OrderDetails = _db.OrderDetails.Where(x => x.OrderId == id).ToList()
            };

            return View(OrderDetailsViewModel);
        }

        [Authorize]
        public IActionResult OrderHistory()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var OrderDetailsVM = new List<OrderDetailsViewModel>();

            var orderHeaderList = _db.OrderHeader.Where(x => x.UserId == claim.Value).OrderByDescending(x => x.OrderDate).ToList();

            foreach (var item in orderHeaderList)
            {
                var individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetails = _db.OrderDetails.Where(x => x.OrderId == item.Id).ToList();
                OrderDetailsVM.Add(individual);
            }
            return View(OrderDetailsVM);
        }

        [Authorize(Roles = SD.AdminEndUser)]
        public IActionResult ManageOrder()
        {
            var OrderDetailsVM = new List<OrderDetailsViewModel>();
            var orderHeaderList = _db.OrderHeader.Where(x => x.Status == SD.StatusSubmitted || x.Status == SD.StatusInProgress)
                .OrderByDescending(x => x.PickUp).ToList();

            foreach (var item in orderHeaderList)
            {
                var individual = new OrderDetailsViewModel();
                individual.OrderHeader = item;
                individual.OrderDetails = _db.OrderDetails.Where(x => x.OrderId == item.Id).ToList();
                OrderDetailsVM.Add(individual);
            }
            return View(OrderDetailsVM);
        }

        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderPrepare(int orderId)
        {
            var orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusInProgress;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder","Order");
        }


        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderReady(int orderId)
        {
            var orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusReady;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Order");
        }


        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderCancel(int orderId)
        {
            var orderHeader = _db.OrderHeader.Find(orderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();

            return RedirectToAction("ManageOrder", "Order");
        }

    }
}