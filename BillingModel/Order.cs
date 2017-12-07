using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BillingModel
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [Required]
        public string UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public List<BillingProduct> Products { get; set; }
    }
}
