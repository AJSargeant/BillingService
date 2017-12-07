using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingModel
{
    public class BillingContext : DbContext
    {
        public BillingContext(DbContextOptions<BillingContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public BillingContext() : base(){ }
        public virtual DbSet<Card> Cards { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<BillingProduct> Products { get; set; }
        
    }
}
