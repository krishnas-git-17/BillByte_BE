using BillByte.Model;
using Billbyte_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace Billbyte_BE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ❌ REMOVE legacy DbSets
        // public DbSet<MenuItem> MenuItems { get; set; }
        // public DbSet<MenuItemImgs> MenuItemImgs { get; set; }
        // public DbSet<CompletedOrder> CompletedOrders { get; set; }
        // public DbSet<CompletedOrderItem> CompletedOrderItems { get; set; }

        // ✅ ONLY NEW TABLE
        public DbSet<TablePreference> TablePreferences { get; set; }
        public DbSet<TableState> TableStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🚫 IGNORE legacy entities completely
            modelBuilder.Ignore<MenuItem>();
            modelBuilder.Ignore<MenuItemImgs>();
            modelBuilder.Ignore<CompletedOrder>();
            modelBuilder.Ignore<CompletedOrderItem>();
            //modelBuilder.Ignore<TableState>();

            modelBuilder.Entity<TablePreference>(e =>
            {
                e.ToTable("TablePreferences");
                e.HasKey(x => x.Id);
            });
            modelBuilder.Entity<TableState>(e =>
            {
                e.ToTable("TableStates");
                e.HasKey(x => x.Id);
            });


        }
    }
}
