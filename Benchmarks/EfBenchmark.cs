using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using EfPerfBench.Data;
using EfPerfBench.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace EfPerfBench.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.Method)]
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
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            await SeedData.PopulateAsync(db, 5000);
        }

        [Benchmark]
        public async Task Get_Customers_NoTracking()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .TagWith(methodName)
                .AsNoTracking()
                .Include(c => c.Orders)
                .ThenInclude(o => o.Products)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Get_Customers_Tracking()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .TagWith(methodName)
                .Include(c => c.Orders)
                .ThenInclude(o => o.Products)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Count_Customers_With_IEunumerable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customersCount = db.Customers
                .TagWith(methodName)
                .ToList() // Materialize as IEnumerable
                .Count(); //Executed in RAM
        }

        [Benchmark(Baseline = true)]
        public async Task Count_Customers_With_IQueryable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customersCount = await db.Customers
                .TagWith(methodName)
                .CountAsync(); //Executed in Database
        }

        [Benchmark]
        public async Task Customer_Exists_With_IEunumerable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customerExists = db.Customers
                .TagWith(methodName)
                .ToList()
                .Any(x => x.Id == 25000); //Executed in RAM
        }

        [Benchmark(Baseline = true)]
        public async Task Customer_Exists_With_IQueryable()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var customerExists = await db.Customers
                .TagWith(methodName)
                .AnyAsync(x => x.Id == 25000); //Executed in Database
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
                .ExecuteUpdateAsync(x => x.
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
        public async Task Customer_Delete()
        {
            using var db = new AppDbContext(_options);
            var customers = await db.Customers.TagWith(GetMethodName())
                .ToListAsync();

            var customerToRemove = customers.FirstOrDefault(c => c.Id == 25001);
            if (customerToRemove != null)
            {
                db.Customers.Remove(customerToRemove);
            }

            await db.SaveChangesAsync();
        }

        [Benchmark]
        public async Task Pagination_Local()
        {
            using var db = new AppDbContext(_options);
            var page = 5;
            var pageSize = 20;

            IQueryable<Customer> customerQuery = db.Customers
                .AsNoTracking()
                .TagWith(GetMethodName())
                .Include(c => c.Orders);

            var customers = await customerQuery.ToListAsync();
            int count = customers.Count;

            List<OrderWithCustomerDto> result = customers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .SelectMany(c => c.Orders, (c, o) => new OrderWithCustomerDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    CustomerId = c.Id,
                    CustomerName = c.Name
                })
                .ToList();
        }

        [Benchmark]
        public async Task Pagination_Database()
        {
            using var db = new AppDbContext(_options);
            var page = 5;
            var pageSize = 20;

            IQueryable<OrderWithCustomerDto> customerQuery = db.Customers
                .TagWith(GetMethodName())
                .SelectMany(c => c.Orders, (c, o) => new OrderWithCustomerDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    CustomerId = c.Id,
                    CustomerName = c.Name
                });

            int count = await customerQuery.CountAsync();

            customerQuery = customerQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var result = await customerQuery.ToListAsync();
        }

        [Benchmark]
        public async Task Ef_EagerLoading_ExplicitIncludes()
        {
            var methodName = GetMethodName();

            using var db = new AppDbContext(_options);
            var data = await db.Orders
                .TagWith(methodName)
                .Include(o => o.Customer)
                .Where(o => o.CustomerId == 1000)
                .ToListAsync();
        }

        [Benchmark(Baseline =true)]
        public async Task Ef_EagerLoading_ImplicitIncludes()
        {
            using var db = new AppDbContext(_options);
            var data = await db.Orders
                .TagWith(GetMethodName())
                .Where(o => o.CustomerId == 1000)
                .Select(o => new
                {
                    CustomerName = o.Customer!.Name,
                    o.Id,
                    o.CustomerId,
                    o.OrderDate,

                }).ToListAsync();
        }
    }
}
