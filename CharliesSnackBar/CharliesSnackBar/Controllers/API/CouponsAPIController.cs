using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using CharliesSnackBar.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CharliesSnackBar.Controllers.API
{
    [Route("api/[controller]")]
    public class CouponsAPIController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponsAPIController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(double orderTotal, string couponCode = null)
        {
            //Return string will have :E for error and :s for success at the end
            var rtn = "";
            if (couponCode == null)
            {
                rtn = orderTotal + ":E";
                return Ok(rtn);
            }

            var couponFromDb = _db.Coupons.Where(x => x.Name == couponCode).FirstOrDefault();
            if (couponFromDb == null)
            {
                rtn = orderTotal + ":E";
                return Ok(rtn);
            }
            if (couponFromDb.MinimumAmount > orderTotal)
            {
                rtn = orderTotal + ":E";
                return Ok(rtn);
            }
            if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupons.ECouponType.Dollar)
            {
                orderTotal = orderTotal - couponFromDb.Discount;
                rtn = orderTotal + ":S";
                return Ok(rtn);
            }
            else
            {
                if (Convert.ToInt32(couponFromDb.CouponType) == (int)Coupons.ECouponType.Percent)
                {
                    orderTotal = orderTotal - (orderTotal * couponFromDb.Discount / 100);
                    rtn = orderTotal + ":S";
                    return Ok(rtn);
                }
                return Ok();
            }

        }
    }
}
