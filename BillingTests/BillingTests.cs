using BillingModel;
using BillingService.Controllers;
using BillingService.Controllers.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BillingTests
{
    [TestClass]
    public class BillingTests
    {
        OrderController OrderController;
        CardController CardController;

        Mock<BillingContext> MockedContext;
        BillingContext db;

        #region data

        public Order SingleTestOrder()
        {
            return new Order
            {
                OrderId = 1,
                UserID = "Test-UserID-String-1",
                OrderDate = DateTime.Now.AddDays(-10),
                Products = new List<BillingProduct>()
                {
                    new BillingProduct
                    {
                        Ean = "6541",
                        Name = "Product-test-1",
                        Price = 5.79,
                        Quantity = 2
                    },
                    new BillingProduct
                    {
                        Ean = "48965",
                        Name = "Product-test-2",
                        Price = 3.14,
                        Quantity = 5
                    }
                }
            };
        }
        public Order UnauthSingleTestOrder()
        {
            return new Order
            {
                OrderId = 2,
                UserID = "Test-UserID-String-2",
                OrderDate = DateTime.Now.AddDays(-10),
                Products = new List<BillingProduct>()
                {
                    new BillingProduct
                    {
                        Ean = "6541",
                        Name = "Product-test-1",
                        Price = 5.79,
                        Quantity = 2
                    },
                    new BillingProduct
                    {
                        Ean = "48965",
                        Name = "Product-test-2",
                        Price = 3.14,
                        Quantity = 5
                    }
                }
            };
        }

        public Order EmptyOrder()
        {
            return new Order() { OrderId = 7 };
        }

        public Card NewCard()
        {
            return new Card
            {
                Active = true,
                CardNumber = "1234123412341234",
                ExpirationDate = "01/21",
                UserID = "Test-UserID-String-1",
                Type = "Customer"
            };
        }

        public Card CardFromForm()
        {
            return new Card
            {
                Active = true,
                CardNumber = "1234123412341234",
                ExpirationDate = "01/21",
                UserID = "Test-UserID-String-1"
            };
        }

        public BillingService.Models.Payment NewPayment()
        {
            return new BillingService.Models.Payment
            {
                Order = SingleTestOrder(),
                Card = NewCard(),
                CVV = "232"
            };
        }

        #endregion data

        #region Init

        [TestInitialize]
        public void Initialize()
        {
            //Prepare "database"
            MockedContext = new Mock<BillingContext>();
            MockedContext.Setup(c => c.Cards).Returns(new MockDbSet<Card>().Object);
            MockedContext.Setup(c => c.Orders).Returns(new MockDbSet<Order>().Object);
            MockedContext.Setup(c => c.Products).Returns(new MockDbSet<BillingProduct>().Object);

            db = MockedContext.Object;

            //Get security token
            var token = TokenGen.UserToken("Customer");
            var testClaims = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));

            //Set up OrderController
            OrderController = new OrderController(db);
            OrderController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = testClaims
                }
            };
            OrderController.ControllerContext.HttpContext.Request.Headers.Add("Authorization", "Bearer " + new JwtSecurityTokenHandler().WriteToken(token));

            //Set up Card Controller
            CardController = new CardController(db);
            CardController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = testClaims
                }
            };
            CardController.ControllerContext.HttpContext.Request.Headers.Add("Authorization", "Bearer " + new JwtSecurityTokenHandler().WriteToken(token));


        }

        #endregion

        #region Tests

        #region API

        [TestMethod]
        public void SendOrderSuccessful()
        {
            var response = (StatusCodeResult)OrderController.SaveOrder(SingleTestOrder());
            Assert.AreEqual(200, response.StatusCode);
            Assert.IsNotNull(db.Orders.Where(o => o.OrderId == 1));
        }

        [TestMethod]
        public void SendOrderNoData()
        {
            var response = (StatusCodeResult)OrderController.SaveOrder(EmptyOrder());
            Assert.AreEqual(400, response.StatusCode);
            Assert.AreEqual(0, db.Orders.Where(o => o.OrderId == 7).Count());
        }

        [TestMethod]
        public void SendOrderDbDown()
        {
            OrderController = new OrderController(null);

            var response = (StatusCodeResult)OrderController.SaveOrder(SingleTestOrder());
            Assert.AreEqual(500, response.StatusCode);
        }

        #endregion API

        #region Views

        #region PayOrder
        [TestMethod]
        public void PayOrderNull()
        {
            OrderController.SaveOrder(SingleTestOrder());

            var response = (StatusCodeResult)CardController.PayOrder(null);
            Assert.AreEqual(404, response.StatusCode);
        }

        [TestMethod]
        public void PayOrderNonexistent()
        {
            OrderController.SaveOrder(SingleTestOrder());

            var response = (StatusCodeResult)CardController.PayOrder(2);
            Assert.AreEqual(404, response.StatusCode);
        }

        [TestMethod]
        public void PayOrderDbDown()
        {
            OrderController = new OrderController(null);
            CardController = new CardController(null);
            OrderController.SaveOrder(SingleTestOrder());

            var response = (StatusCodeResult)CardController.PayOrder(2);
            Assert.AreEqual(500, response.StatusCode);
        }

        [TestMethod]
        public void PayOrderUnauth()
        {
            OrderController.SaveOrder(UnauthSingleTestOrder());

            var response = (StatusCodeResult)CardController.PayOrder(2);
            Assert.AreEqual(403, response.StatusCode);
        }

        [TestMethod]
        public void PayOrderSuccess()
        {
            OrderController.SaveOrder(SingleTestOrder());

            var response = (ViewResult)CardController.PayOrder(1);

            Assert.IsNotNull(response.Model);
            Assert.IsInstanceOfType(response.Model, typeof(BillingService.Models.UserOrder));
        }

        #endregion PayOrder

        #region Progress

        [TestMethod]
        public void ProgressNewCard()
        {
            BillingService.Models.UserOrder uo = new BillingService.Models.UserOrder { SelectedCardID = -1 };

            var response = (RedirectToActionResult)CardController.Progress(uo);

            Assert.AreEqual("AddCard", response.ActionName);
        }

        [TestMethod]
        public void ProgressExistingCard()
        {
            db.Cards.Add(NewCard());
            BillingService.Models.UserOrder uo = new BillingService.Models.UserOrder { SelectedCardID = 1 };

            var response = (RedirectToActionResult)CardController.Progress(uo);
            Assert.AreEqual("Finalise", response.ActionName);
        }

        [TestMethod]
        public void ProgressNull()
        {
            var response = (StatusCodeResult)CardController.Progress(null);

            Assert.AreEqual(response.StatusCode, 400);
        }

        #endregion Progress

        #region AddCard

        [TestMethod]
        public void AddCard()
        {
            var response = (RedirectToActionResult)CardController.AddCardPost(CardFromForm());

            Assert.AreEqual("PayOrder", response.ActionName);
            Assert.AreEqual(1,db.Cards.Where(c => c.UserID == "Test-UserID-String-1").Count());
        }

        [TestMethod]
        public void AddCardNull()
        {
            var response = (StatusCodeResult)CardController.AddCardPost(null);

            Assert.AreEqual(400, response.StatusCode);
        }

        [TestMethod]
        public void AddCardDbDown()
        {

            //Get security token
            var token = TokenGen.UserToken("Customer");
            var testClaims = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));

            //Set up OrderController
            CardController = new CardController(null);
            CardController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = testClaims
                }
            };
            CardController.ControllerContext.HttpContext.Request.Headers.Add("Authorization", "Bearer " + new JwtSecurityTokenHandler().WriteToken(token));

            var response = (StatusCodeResult)CardController.AddCardPost(CardFromForm());

            Assert.AreEqual(500, response.StatusCode);
        }

        #endregion AddCard

        #region Finalise

        [TestMethod]
        public void Finalise()
        {
            Order o = SingleTestOrder();
            OrderController.SaveOrder(o);
            CardController.AddCardPost(NewCard());
            CardController.PayOrder(o.OrderId);
            CardController.Progress(new BillingService.Models.UserOrder {SelectedCardID = 0});

            var response = (ViewResult)CardController.Finalise();

            Assert.IsNotNull(response.Model);
            Assert.IsInstanceOfType(response.Model, typeof(BillingService.Models.Payment));
        }

        [TestMethod]
        public void FinaliseDbDown()
        {
            CardController = new CardController(null);

            var response = (StatusCodeResult)CardController.Finalise();
            Assert.AreEqual(500, response.StatusCode);
        }

        [TestMethod]
        public void FinalisePostNull()
        {
            var response = (StatusCodeResult)CardController.FinalisePost(null).Result;

            Assert.AreEqual(400, response.StatusCode);
        }

        [TestMethod]
        public void FinalisePostDbDown()
        {
            CardController = new CardController(null);

            var response = (StatusCodeResult)CardController.FinalisePost(NewPayment()).Result;

            Assert.AreEqual(500, response.StatusCode);
        }

        [TestMethod]
        public void FinalisePost()
        {
            OrderController.SaveOrder(SingleTestOrder());

            var response = (RedirectResult)CardController.FinalisePost(NewPayment()).Result;

            Assert.AreEqual("http://localhost:54330/Products/Index", response.Url);
        }

        #endregion Finalise

        #endregion Views

        #endregion Tests
    }
}
