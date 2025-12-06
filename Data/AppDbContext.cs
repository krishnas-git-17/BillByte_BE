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

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<FoodType> FoodTypes { get; set; }
        public DbSet<BusinessUnitSetting> BusinessUnitSettings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MenuItems Table Mapping
            modelBuilder.Entity<MenuItem>()
                .ToTable("MenuItems")
                .HasKey(x => x.ItemId);

            // Food Type Mapping
            modelBuilder.Entity<FoodType>()
                .ToTable("FoodTypes")
                .HasKey(x => x.FoodTypeId);
        }
    }
}
