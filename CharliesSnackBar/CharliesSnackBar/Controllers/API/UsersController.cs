using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharliesSnackBar.Data;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CharliesSnackBar.Controllers.API
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(string type, string query=null)
        {
            if (type.Equals("email")&&query!=null)
            {
                var customerQuery = _db.Users.Where(x => x.Email.ToLower().Contains(query.ToLower()));

                return Ok(customerQuery.ToList());
            }
            return Ok();
        }      
    }
}
