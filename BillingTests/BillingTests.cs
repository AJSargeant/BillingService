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

namespace BillingTests
{
    [TestClass]
    public class BillingTests
    {
        OrderController OrderController;
        CardController CardController;

        Mock<BillingContext> MockedContext;

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
                        Ean = 6541,

                    }
                }
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

            //Get security token
            var token = TokenGen.UserToken("Customer");
            var testClaims = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));
            
            //Set up OrderController
            OrderController = new OrderController(MockedContext.Object);
            OrderController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = testClaims
                }
            };
            OrderController.ControllerContext.HttpContext.Request.Headers.Add("Authorization", "Bearer " + new JwtSecurityTokenHandler().WriteToken(token));

            //Set up Card Controller
            CardController = new CardController(MockedContext.Object);
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
        public void SendOrder()
        {

        }

        #endregion API

        #endregion Tests
    }
}
