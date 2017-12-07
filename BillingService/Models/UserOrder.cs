using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BillingService.Models
{
    public class UserOrder
    {
        public Order Order { get; set; }
        public IEnumerable<SelectListItem> Cards { get; set; }
        public int SelectedCardID { get; set; }

    }
}
