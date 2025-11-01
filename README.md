# EF Core Performance Benchmark

This project is a **.NET 8 console application** that uses **Entity Framework Core** and **BenchmarkDotNet** to measure the performance of different database access patterns against **SQL Server LocalDB**.

---

## Tech Stack

- **.NET 8**
- **Entity Framework Core 8**
- **SQL Server LocalDB**
- **BenchmarkDotNet**

---

## Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/<your-username>/EfPerfBench.git
cd EfPerfBench
```

### 2. Restore dependencies
```bash
dotnet restore
```

### 3.Run the benchmark
```bash
dotnet run -c Release
```

### 4. View results
The benchmark results will be displayed in the console after the run completes.
You can also find detailed reports in the BenchmarkDotNet.Artifacts/results/
folder.




