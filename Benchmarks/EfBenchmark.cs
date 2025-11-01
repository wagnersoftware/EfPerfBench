using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using EfPerfBench.Data;
using Microsoft.EntityFrameworkCore;

namespace EfPerfBench.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class EfBenchmarks
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EfPerfBench;Trusted_Connection=True;MultipleActiveResultSets=true;";

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
            await SeedData.PopulateAsync(db, 50_000);
        }

        [Benchmark]
        public async Task Ef_Linq_NoTracking()
        {
            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .AsNoTracking()
                .Take(1000)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Ef_Linq_Tracking()
        {
            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .Take(1000)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Ef_EagerLoading()
        {
            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.Products)
                .AsNoTracking()
                .Take(500)
                .ToListAsync();
        }

        [Benchmark]
        public async Task Ef_RawSql()
        {
            using var db = new AppDbContext(_options);
            var data = await db.Customers
                .FromSqlRaw("SELECT TOP 1000 * FROM Customers")
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
