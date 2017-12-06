using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BillingModel;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace BillingService.Controllers
{
    //[Authorize]
    public class CardController : Controller
    {
        public BillingContext db;

        public CardController(BillingContext context)
        {
            db = context;
        }
        
        [HttpPost]
        public ActionResult PayOrder([FromBody]Models.Orders.Order order)
        {
            List<Card> cards = db.Cards.Where(c => c.UserID == order.UserID).ToList();

            Models.UserOrder uo = new Models.UserOrder() { Order = order, Cards = cards };

            return View(uo);
        }
    }
}