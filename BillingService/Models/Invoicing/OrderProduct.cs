using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillingService.Models.Invoicing
{
    public class OrderProduct
    {
        public string Ean { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        [DataType(DataType.Currency)]
        public double Price { get; set; }
        public double TotalPrice { get; set; }
    }
}
