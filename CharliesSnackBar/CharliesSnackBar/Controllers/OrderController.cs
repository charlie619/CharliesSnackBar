using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
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

            return RedirectToAction("ManageOrder", "Order");
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
        public IActionResult OrderPickup(string searchEmail = null, string searchPhone = null, string searchOrder = null)
        {
            var OrderDetailsVM = new List<OrderDetailsViewModel>();

            if (searchEmail != null || searchOrder != null || searchPhone != null)
            {
                //filtering the criteria
                var user = new ApplicationUser();
                var orderHeaderList = new List<OrderHeader>();

                if (searchOrder != null)
                {
                    orderHeaderList = _db.OrderHeader.Where(x => x.Id == Convert.ToInt32(searchOrder)).ToList();
                }
                else
                {
                    if (searchEmail != null)
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
                if (user != null || orderHeaderList.Count > 0)
                {
                    if (orderHeaderList.Count == 0)
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

        [Authorize(Roles = SD.AdminEndUser)]
        public IActionResult OrderPickupDetails(int orderid)
        {
            var orderDetailsVM = new OrderDetailsViewModel
            {
                OrderHeader = _db.OrderHeader.Where(x => x.Id == orderid).FirstOrDefault()
            };
            orderDetailsVM.OrderHeader.ApplicationUser = _db.Users.Where(x => x.Id == orderDetailsVM.OrderHeader.UserId).FirstOrDefault();
            orderDetailsVM.OrderDetails = _db.OrderDetails.Where(x => x.OrderId == orderDetailsVM.OrderHeader.Id).ToList();

            return View(orderDetailsVM);
        }

        [HttpPost,ActionName("OrderPickupDetails")]
        [Authorize(Roles = SD.AdminEndUser)]
        public async Task<IActionResult> OrderPickupDetailsPost(int orderid)
        {
            var orderHeader = _db.OrderHeader.Find(orderid);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("OrderPickup", "Order");
        }

        //GET : Order Summary Export
        public IActionResult OrderSummaryExport()
        {
            return View();
        }

        //POST : Order Summary Export
        [HttpPost]
        public IActionResult OrderSummaryExport(OrderExportViewModel orderExportVM)
        {
            var OrderHeaderList = _db.OrderHeader.Where(o => o.OrderDate >= orderExportVM.startDate && o.OrderDate <= orderExportVM.endDate).ToList();
            var OrderDetailList = new List<OrderDetails>();
            var IndividualOrderList = new List<OrderDetails>();
            foreach (var OrderHeader in OrderHeaderList)
            {
                IndividualOrderList = _db.OrderDetails.Where(o => o.OrderId == OrderHeader.Id).ToList();

                foreach (var individualOrder in IndividualOrderList)
                {
                    OrderDetailList.Add(individualOrder);
                }
            }

            byte[] bytes = Encoding.ASCII.GetBytes(ConvertToString(OrderDetailList));
            return File(bytes, "application/text", "OrderDetail.csv");
        }

        public String ConvertToString<T>(IList<T> data)
        {

            var properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (T item in data)
            {
                var row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            table.Columns.Remove("OrderHeader");
            table.Columns.Remove("MenuItemId");
            table.Columns.Remove("MenuItem");
            table.Columns.Remove("Description");

            var sb = new StringBuilder();

            var columnNames = table.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));
            foreach (DataRow row in table.Rows)
            {
                var fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }
            return sb.ToString();
        }

    }
}
