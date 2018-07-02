using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models;
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
                var individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = _db.OrderDetails.Where(x => x.OrderId == item.Id).ToList()
                };
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
                var individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = _db.OrderDetails.Where(x => x.OrderId == item.Id).ToList()
                };
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

        // GET : Order Pickup
        [Authorize(Roles = SD.AdminEndUser)]
        public IActionResult OrderPickup(string searchEmail=null,string searchPhone=null,string searchOrder=null)
        {
            var OrderDetailsVM = new List<OrderDetailsViewModel>();

            if (searchEmail != null || searchOrder != null || searchPhone != null)
            {
                //filtering the criteria
                var user = new ApplicationUser();
                var orderHeaderList = new List<OrderHeader>();

                if (searchOrder!=null)
                {
                    orderHeaderList = _db.OrderHeader.Where(x => x.Id == Convert.ToInt32(searchOrder)).ToList();
                }
                else
                {
                    if (searchEmail!=null)
                    {
                        user = _db.Users.Where(x => x.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefault();
                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            user = _db.Users.Where(x => x.PhoneNumber.ToLower().Contains(searchPhone.ToLower())).FirstOrDefault();
                        }
                    }
                }
                if (user!=null||orderHeaderList.Count>0)
                {
                    if (orderHeaderList.Count==0)
                    {
                        orderHeaderList = _db.OrderHeader.Where(x => x.UserId == user.Id).OrderByDescending(x => x.OrderDate).ToList();
                    }
                    foreach (var item in orderHeaderList)
                    {
                        var individual = new OrderDetailsViewModel
                        {
                            OrderHeader = item,
                            OrderDetails = _db.OrderDetails.Where(x => x.OrderId == item.Id).ToList()
                        };
                        OrderDetailsVM.Add(individual);
                    }
                }

            }
            else
            {
                var orderHeaderList = _db.OrderHeader.Where(x => x.Status == SD.StatusReady || x.Status == SD.StatusInProgress)
                .OrderByDescending(x => x.PickUp).ToList();

                foreach (var item in orderHeaderList)
                {
                    var individual = new OrderDetailsViewModel
                    {
                        OrderHeader = item,
                        OrderDetails = _db.OrderDetails.Where(x => x.OrderId == item.Id).ToList()
                    };
                    OrderDetailsVM.Add(individual);
                }
            }
            return View(OrderDetailsVM);
        }


    }
}