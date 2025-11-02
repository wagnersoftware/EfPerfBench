using EfPerfBench.Models;

namespace EfPerfBench.Data
{
    public  class OrderWithCustomerDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
}
