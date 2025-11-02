namespace EfPerfBench.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public int Age { get; set; }
        public int Balance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string FaxNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
