using System;
using System.Collections.Generic;
using System.Text;

namespace BillingModel
{
    public class Card
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
    }
}
