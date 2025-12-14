using BillByte.Model;
using Microsoft.EntityFrameworkCore;

namespace Billbyte_BE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemImgs> menuItemImgs { get; set; }
        public DbSet<CompletedOrder> CompletedOrders { get; set; }
        public DbSet<CompletedOrderItem> CompletedOrderItems { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MenuItem>()
                .ToTable("MenuItems")
                .HasKey(x => x.MenuId);
            modelBuilder.Entity<MenuItemImgs>()
        .ToTable("MenuItemImgs")
        .HasKey(x => x.Id);

            modelBuilder.Entity<CompletedOrderItem>()
       .ToTable("CompletedOrderItems")
       .HasKey(x => x.Id);


            modelBuilder.Entity<CompletedOrder>()                    
                .ToTable("CompletedOrders")
                .HasKey(x => x.Id);
        }
    }
}
