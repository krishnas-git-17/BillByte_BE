using BillByte.Models;

namespace Billbyte_BE.Data
{
    public static class DbSeeder
    {
        public static void SeedPlans(AppDbContext context)
        {
            if (context.Plans.Any())
                return;

            context.Plans.AddRange(
                new Plan
                {
                    Name = "FREE",
                    Price = 0,
                    MaxUsers = 1,
                    DurationInDays = 30,
                    IsActive = true
                },
                new Plan
                {
                    Name = "BASIC",
                    Price = 999,
                    MaxUsers = 3,
                    DurationInDays = 30,
                    IsActive = true
                },
                new Plan
                {
                    Name = "PRO",
                    Price = 1999,
                    MaxUsers = 10,
                    DurationInDays = 30,
                    IsActive = true
                }
            );

            context.SaveChanges();
        }
    }
}
