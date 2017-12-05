using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers.API
{
    [Produces("application/json")]
    [Route("api/Card")]
    [Authorize]
    public class CardController : Controller
    {

        [HttpGet]
        public ActionResult PayOrder(Models.Orders.Order order)
        {
            return View();
        }

    }
}
