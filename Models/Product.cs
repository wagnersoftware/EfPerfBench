namespace EfPerfBench.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
