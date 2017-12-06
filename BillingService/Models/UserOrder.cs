using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingModel;

namespace BillingService.Models
{
    public class UserOrder
    {
        public Orders.Order Order { get; set; }
        public List<Card> Cards { get; set; }

    }
}
