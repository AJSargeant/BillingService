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
        
        public Order EmptyOrder()
        {
            return new Order() { OrderId = 7 };
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
            Assert.IsNull(db.Orders.(o => o.OrderId == 7));
        }

        #endregion API

        #endregion Tests
    }
}
