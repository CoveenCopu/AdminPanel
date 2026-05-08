namespace AdminPanel.Models;

public class Order
{
    public int Id { get; set; }
    public string Customer { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public string Number { get; set; }
    public DateTime? SetupDate { get; set; }
    public DateTime? PickupDate { get; set; }

    public decimal? Transport { get; set; } // Decimal

    // Beregnet pris ud fra produktets aktuelle pris
    public decimal Price => OrderItems?.Sum(oi => oi.TotalPrice) ?? 0;

    public string? Notes { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}