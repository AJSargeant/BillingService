using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BillingModel;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BillingService.Controllers
{
    [Authorize(Roles = "Customer,Staff,Admin")]
    public class CardController : Controller
    {
        public BillingContext db;
        private Models.Orders.Order _order;

        public CardController(BillingContext context)
        {
            db = context;
        }
        
        [HttpPost]
        public ActionResult PayOrder([FromBody]Models.Orders.Order order)
        {
            _order = order;
            List<SelectListItem> cardList = new List<SelectListItem>();
            List<Card> cards;

            if (User.IsInRole("Customer"))
            {
                cards = db.Cards.Where(c => c.UserID == order.UserID).ToList();
            }
            else
            {
                cards = db.Cards.Where(c => c.Type == "Staff").ToList();
            }

            if(User.IsInRole("Customer") || User.IsInRole("Admin"))
                cardList.Add(new SelectListItem { Value = "-1", Text = "Add New Payment Method..." });

            cardList.AddRange(cards.Select(c => new SelectListItem
            {
                Value = c.ID.ToString(),
                Text = "Card Ending " + c.CardNumber.Substring(11)
            }));

            Models.UserOrder uo = new Models.UserOrder() { Order = order, Cards = cardList.AsEnumerable() };

            return View(uo);
        }

        [HttpPost]
        public ActionResult Progress([FromBody]Models.UserOrder uo)
        {
            try
            {
                if (uo.SelectedCardID == -1)
                {
                    return RedirectToAction(nameof(AddCard));
                }
                else
                {
                    return RedirectToAction(nameof(Finalise), uo);
                }
            }
            catch
            {
                return new StatusCodeResult(500);
            }

        }

        public ActionResult AddCard()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCardPost([FromBody]Card c)
        {
            try
            {
                c.UserID = _order.UserID;
                if (User.IsInRole("Customer"))
                    c.Type = "Customer";
                else
                    c.Type = "Staff";
                c.Active = true;
                db.Add(c);
                db.SaveChanges();
                return RedirectToAction(nameof(PayOrder),_order);
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }

        public ActionResult Finalise(Models.UserOrder uo)
        {
            Models.Payment pay = new Models.Payment { Order = uo.Order, Card = db.Cards.Where(c => c.Active && c.ID == uo.SelectedCardID).First() }; 
            return new StatusCodeResult(404);
        }
    }
}