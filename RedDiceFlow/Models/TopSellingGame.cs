namespace RedDiceFlow.Models
{
    public class TopSellingGame
    {
        public string Name { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public double TotalRevenue { get; set; }
    }
}
