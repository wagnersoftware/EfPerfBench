using EfPerfBench.Models;
using Microsoft.EntityFrameworkCore;

namespace EfPerfBench.Data
{
    public static class SeedData
    {
        public static async Task PopulateAsync(AppDbContext db, int count)
        {
            if (await db.Customers.AnyAsync()) return;

            Console.WriteLine($"Seeding {count} customers...");
            var rnd = new Random();
            var batch = new List<Customer>();

            for (int i = 0; i < count; i++)
            {
                batch.Add(new Customer
                {
                    Name = $"Customer {i}",
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            OrderDate = DateTime.UtcNow.AddDays(-rnd.Next(1000)),
                            Products = new List<Product>
                            {
                                new Product { Name = "Product A", Price = rnd.Next(10, 500) },
                                new Product { Name = "Product B", Price = rnd.Next(10, 500) }
                            }
                        }
                    }
                });

                // Batch insert for performance
                if (batch.Count >= 2000)
                {
                    await db.AddRangeAsync(batch);
                    await db.SaveChangesAsync();
                    batch.Clear();
                    Console.Write(".");
                }
            }

            if (batch.Count > 0)
            {
                await db.AddRangeAsync(batch);
                await db.SaveChangesAsync();
            }

            Console.WriteLine("\nSeed complete!");
        }
    }
}
