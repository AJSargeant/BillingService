﻿using Microsoft.AspNetCore.Mvc;
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
                {
                    try
                    {
                        _order = db.Orders.Single(o => o.OrderId == OrderID);
                    }
                    catch (InvalidOperationException)
                    {
                        _order = null;
                    }
                }
                if (_order == null)
                    return new StatusCodeResult(404);

                if (User.FindFirst(ClaimTypes.NameIdentifier).Value != _order.UserID && !User.IsInRole("Admin"))
                    return new StatusCodeResult(403);
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
                if (User.IsInRole("Customer"))
                {
                    if (ChosenCard == -1)
                    {
                        return RedirectToAction(nameof(AddCard));
                    }
                    else
                    {
                        return RedirectToAction(nameof(Finalise));
                    }
                }
                else
                {
                    if(ChosenCard == -1)
                    {
                        return RedirectToAction(nameof(AddCard));
                    }
                    return RedirectToAction(nameof(Finalise));
                }
            }
            catch
            {
                return new StatusCodeResult(400);
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
                try
                {
                    c.UserID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                }
                catch (NullReferenceException)
                {
                    return new StatusCodeResult(400);
                }
                if (User.IsInRole("Customer"))
                    c.Type = "Customer";
                else
                    c.Type = "Staff";
                c.Active = true;
                db.Cards.Add(c);
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
            try
            {
                Models.Payment pay = new Models.Payment { Order = _order, Card = db.Cards.Where(c => c.Active && c.ID == ChosenCard).First() };
                return View(pay);
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }

        [HttpPost]
        public async Task<ActionResult> FinalisePost([FromBody] Models.Payment Payment)
        {
            //INSERT TRANSACTION LOGIC HERE (THIS APP DOESN'T TAKE MONEY FROM REAL ACCOUNTS) 
            if (Payment == null)
                return new StatusCodeResult(400);
            try
            {
                await PostInvoice();
                await NotifyOrdering();
                foreach(BillingProduct bp in Payment.Order.Products)
                {
                    db.Products.Remove(bp);
                }
                db.Remove(Payment.Order);
                db.SaveChanges();
            }
            catch { return new StatusCodeResult(500); }
            
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
            catch { }
        }

        private async Task NotifyOrdering()
        {
            try
            {
                HttpClient client = new HttpClient();
                if (User.IsInRole("Customer"))
                    client.BaseAddress = new Uri("http://localhost:54997/api/CustomerOrdering/BillingComplete/" + _order.OrderId);
                else
                    client.BaseAddress = new Uri("http://localhost:50492/api/Staff/BillingComplete/" + _order.OrderId);


                await client.PostAsync(client.BaseAddress.ToString(), null);
            }
            catch { }
        }
    }
}