using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillingService.Models
{
    public class Payment
    {
        public Orders.Order Order { get; set; }
        public BillingModel.Card Card { get; set; }
        [StringLength(3)]
        public string CVV { get; set; }
    }
}
