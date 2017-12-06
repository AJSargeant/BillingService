using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BillingModel;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Newtonsoft.Json;

namespace BillingService.Controllers
{
    [Authorize(Roles = "Customer,Staff,Admin")]
    public class CardController : Controller
    {
        public BillingContext db;
        private Order _order;
        private int ChosenCard;

        public CardController(BillingContext context)
        {
            db = context;
        }

        [HttpGet]
        public ActionResult PayOrder(int? OrderID)
        {
            try
            {
                if (OrderID != null)
                    _order = db.Orders.Single(o => o.OrderId == OrderID);
                if (_order == null)
                    return new StatusCodeResult(404);

                List<SelectListItem> cardList = new List<SelectListItem>();
                List<Card> cards;

                if (User.IsInRole("Customer"))
                {
                    cards = db.Cards.Where(c => c.UserID == _order.UserID).ToList();
                }
                else
                {
                    cards = db.Cards.Where(c => c.Type == "Staff").ToList();
                }

                if (User.IsInRole("Customer") || User.IsInRole("Admin"))
                    cardList.Add(new SelectListItem { Value = "-1", Text = "Add New Payment Method..." });

                cardList.AddRange(cards.Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),
                    Text = "Card Ending " + c.CardNumber.Substring(11)
                }));

                Models.UserOrder uo = new Models.UserOrder() { Order = _order, Cards = cardList.AsEnumerable() };

                return View(uo);
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpPost]
        public ActionResult Progress([FromBody]Models.UserOrder uo)
        {
            try
            {
                ChosenCard = uo.SelectedCardID;
                if (ChosenCard == -1)
                {
                    return RedirectToAction(nameof(AddCard));
                }
                else
                {
                    return RedirectToAction(nameof(Finalise));
                }
            }
            catch
            {
                return new StatusCodeResult(500);
            }

        }

        [HttpGet]
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
                return RedirectToAction(nameof(PayOrder));
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public ActionResult Finalise()
        {
            Models.Payment pay = new Models.Payment { Order = _order, Card = db.Cards.Where(c => c.Active && c.ID == ChosenCard).First() };
            return View(pay);
        }

        [HttpPost]
        public async Task<ActionResult> FinalisePost([FromBody] Models.Payment Payment)
        {
            //INSERT TRANSACTION LOGIC HERE (THIS APP DOESN'T TAKE MONEY FROM REAL ACCOUNTS) 

            try
            {
                await PostInvoice();
                foreach(BillingProduct bp in _order.Products)
                {
                    db.Products.Remove(bp);
                }
                db.Remove(_order);
                db.SaveChanges();
            }
            catch { }
            
            return Redirect("http://localhost:54330/Products/Index");
        }

        private async Task PostInvoice()
        {
            try
            {
                HttpClient client = new HttpClient()
                {
                    BaseAddress = new Uri("http://localhost:54349/api/InvoiceStore/StoreOrder")
                };
                //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);


                Models.Invoicing.Order inv = new Models.Invoicing.Order(_order);

                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(inv));
                await client.PostAsync(client.BaseAddress.ToString(), httpContent);
            }
            catch { throw; }
        }
    }
}