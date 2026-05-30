using System;

namespace RedDiceFlow.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public double TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemsCount { get; set; }
        public int DisplayNumber { get; set; }
    }
}
