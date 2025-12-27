using BillByte.Model;
using BillByte.Models;
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

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<TablePreference> TablePreferences { get; set; }
        public DbSet<TableState> TableStates { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemImgs> MenuItemImgs { get; set; }
        public DbSet<CompletedOrder> CompletedOrders { get; set; }
        public DbSet<CompletedOrderItem> CompletedOrderItems { get; set; }
        public DbSet<ActiveTableItem> ActiveTableItems { get; set; }
        public DbSet<KotSnapshot> KotSnapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Restaurant>(e =>
            {
                e.ToTable("Restaurants");
                e.HasKey(x => x.Id);
            });

            modelBuilder.Entity<TablePreference>(e =>
            {
                e.ToTable("TablePreferences");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.RestaurantId);
            });

            modelBuilder.Entity<TableState>(e =>
            {
                e.ToTable("TableStates");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.RestaurantId, x.TableId });
            });

            modelBuilder.Entity<MenuItem>(e =>
            {
                e.ToTable("MenuItems");
                e.HasKey(x => x.MenuId);

                e.Property(x => x.Price)
                    .HasColumnType("numeric(10,2)");

                e.HasIndex(x => new { x.RestaurantId, x.MenuId });
            });

            modelBuilder.Entity<MenuItemImgs>(e =>
            {
                e.ToTable("MenuItemImgs");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.RestaurantId, x.MenuId });
            });

            modelBuilder.Entity<CompletedOrder>(e =>
            {
                e.ToTable("CompletedOrders");
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.RestaurantId, x.CreatedDate });
            });

            modelBuilder.Entity<CompletedOrderItem>(e =>
            {
                e.ToTable("CompletedOrderItems");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.CompletedOrderId);
            });
            modelBuilder.Entity<ActiveTableItem>(e =>
            {
                e.ToTable("ActiveTableItems");

                e.HasKey(x => x.Id);

                e.Property(x => x.TableId)
                    .IsRequired()
                    .HasMaxLength(50);

                e.Property(x => x.ItemName)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(x => x.Price)
                    .HasColumnType("numeric(10,2)");

                e.Property(x => x.Qty)
                    .IsRequired();

                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                // 🔹 Fast lookups per restaurant + table
                e.HasIndex(x => new { x.RestaurantId, x.TableId });

                // 🔹 Prevent duplicate item rows per table
                e.HasIndex(x => new { x.RestaurantId, x.TableId, x.ItemId })
                    .IsUnique();
            });
            modelBuilder.Entity<KotSnapshot>(e =>
            {
                e.ToTable("KotSnapshots");

                e.HasKey(x => x.Id);

                e.Property(x => x.TableId)
                    .IsRequired()
                    .HasMaxLength(50);

                e.HasIndex(x => new { x.RestaurantId, x.TableId, x.ItemId })
                    .IsUnique();
            });



        }
    }
}
