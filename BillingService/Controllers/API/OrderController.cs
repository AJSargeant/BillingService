using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BillingModel;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers.API
{
    [Produces("application/json")]
    [Route("api/Order")]
    [Authorize(Roles = "Customer, Staff, Admin")]
    public class OrderController : Controller
    {
        private BillingContext db;

        public OrderController(BillingContext context)
        {
            db = context;
        }

        // POST: api/Order
        [HttpPost]
        [Route("SaveOrder")]
        public IActionResult SaveOrder([FromBody]Order Order)
        {
            try
            {
                if (Order == null || Order.Products == null)
                    return new StatusCodeResult(400);
                if (Order.Products.Count == 0)
                    return new StatusCodeResult(400);
                foreach (BillingProduct bp in Order.Products)
                {
                    db.Products.Add(bp);
                }
                db.Orders.Add(Order);
                db.SaveChanges();
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
