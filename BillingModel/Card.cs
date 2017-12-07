using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BillingModel
{
    public class Card
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string UserID { get; set; }
        [Required]
        [StringLength(16)]
        [RegularExpression(@"\d{16}")]
        public string CardNumber { get; set; }
        [Required]
        [RegularExpression(@"((0[1-9])|(1[12]))\/((1[89])|([2-4]\d)|5[0-8])")]
        public string ExpirationDate { get; set; }
        [Required]
        public string Type { get; set; }
        public bool Active { get; set; }
    }
}
