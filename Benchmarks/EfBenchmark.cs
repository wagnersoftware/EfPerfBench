using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using EfPerfBench.Data;
using EfPerfBench.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace EfPerfBench.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class EfBenchmarks
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EfPerfBench;Trusted_Connection=True;MultipleActiveResultSets=true;";

        private int _customerCount = 50000;
        private string GetMethodName([CallerMemberName] string name = null) => name;

        public EfBenchmarks()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
        }

        [GlobalSetup]
        public async Task Setup()
        {
            using var db = new AppDbContext(_options);

            if(db.Customers.Any())
                return;

            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            await SeedData.PopulateAsync(db, 50_000);
        }

        [Benchmark]
        public async Task Get_Customers_NoTracking()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .TagWith(methodName)
                .AsNoTracking()
                .Take(_customerCount)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Get_Customers_Tracking()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .TagWith(methodName)
                .Take(_customerCount)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Count_Customers_With_IEunumerable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customersCount = db.Customers
                .TagWith(methodName)
                .ToList()
                .Count();
        }

        [Benchmark]
        public async Task Count_Customers_With_IQueryable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customersCount = await db.Customers
                .TagWith(methodName)
                .CountAsync();
        }

        [Benchmark]
        public async Task Customer_Exists_With_IEunumerable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customerExists = db.Customers
                .TagWith(methodName)
                .ToList().Any(x => x.Id == 25000);
        }

        [Benchmark]
        public async Task Customer_Exists_With_IQueryable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customerExists = await db.Customers
                .TagWith(methodName)
                .AnyAsync(x => x.Id == 25000);
        }

        [Benchmark]
        public async Task Customer_Update()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customers = await db.Customers.ToListAsync();

            foreach (var customer in customers)
            {
                customer.Name = "Updated Name";
            }

            await db.SaveChangesAsync();
        }

        [Benchmark]
        public async Task Customer_Execute_Update()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customers = await db.Customers
                .ExecuteUpdateAsync( x => x.
                    SetProperty(c => c.Name, "Updated Name"));
        }

        [Benchmark]
        public async Task Customer_Execute_Delete()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customerExists = await db.Customers
                .TagWith(methodName)
                .Where(x => x.Id == 25000)
                .ExecuteDeleteAsync();
        }

        [Benchmark]
        public async Task Ef_EagerLoading_ExplicitIncludes()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .TagWith(methodName)
                .Include(c => c.Orders)
                .ThenInclude(o => o.Products)
                .AsNoTracking()
                .Take(_customerCount)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Ef_EagerLoading_ImplicitIncludes()
        {
            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .TagWith(GetMethodName())
                .AsNoTracking()
                .Select(c => new
                {
                    Customer = c,
                    Orders = c.Orders.Select(o => new
                    {
                        Order = o,
                        Products = o.Products.Select(p => p)
                    }).ToList()
                })
                .Take(_customerCount)
                .ToListAsync();
        }
    }
}
