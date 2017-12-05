using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BillingService.Models.Invoicing
{
    class Order
    { 
        public int OrderID { get; set; }
        public string UserID { get; set; }
        public DateTime TimeOfOrder { get; set; }
        public List<OrderProduct> Products { get; set; }
        [DataType(DataType.Currency)]
        public double Total { get; set; }
    }
}
