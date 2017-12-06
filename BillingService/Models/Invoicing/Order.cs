using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BillingService.Models.Invoicing
{
    class Order
    { 
        public Order(BillingModel.Order Order)
        {
            OrderID = Order.OrderId;
            UserID = Order.UserID;
            TimeOfOrder = Order.OrderDate;
            Products = new List<OrderProduct>();
            Total = 0;

            foreach(BillingModel.BillingProduct bp in Order.Products)
            {
                Products.Add(new OrderProduct(bp));
                Total += (bp.Quantity * bp.Price);
            }
        }

        public int OrderID { get; set; }
        public string UserID { get; set; }
        public DateTime TimeOfOrder { get; set; }
        public List<OrderProduct> Products { get; set; }
        [DataType(DataType.Currency)]
        public double Total { get; set; }
    }
}
