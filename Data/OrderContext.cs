#nullable disable

using Microsoft.EntityFrameworkCore;
using OTS_TEST.Models;

namespace OTS_TEST.Data
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Order { get; set; }
    }
}