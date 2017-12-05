using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillingService.Models.Orders
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public List<BillingProduct> Products { get; set; }
    }
}
