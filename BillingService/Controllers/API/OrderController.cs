using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BillingModel;

namespace BillingService.Controllers.API
{
    [Produces("application/json")]
    [Route("api/Order")]
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
        public void SaveOrder([FromBody]Order Order)
        {
            foreach(BillingProduct bp in Order.Products)
            {
                db.Products.Add(bp);
            }
            db.Orders.Add(Order);
            db.SaveChanges();
        }
    }
}
