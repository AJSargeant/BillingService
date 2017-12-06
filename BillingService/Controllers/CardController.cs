using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BillingModel;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers
{
    [Authorize]
    public class CardController : Controller
    {
        public BillingContext db;

        public CardController(BillingContext context)
        {
            db = context;
        }

        [HttpGet]
        public ActionResult PayOrder(Models.Orders.Order order)
        {
            if(User.FindFirst(ClaimTypes.NameIdentifier).Value == order.UserID)
            {
                return View(order);
            }
            return new StatusCodeResult(403);
        }




        [HttpPost]
        public ActionResult PayOrderPost()
        {
            return new StatusCodeResult(404);
        }
    }
}