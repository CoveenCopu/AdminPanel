namespace AdminPanel.Models;

public class Order
{
    public int Id { get; set; }
    public string Kunde { get; set; }
    public string By { get; set; }
    public string Adresse { get; set; }
    public string Telefonnummer { get; set; }
    public DateTime? Opsætningsdato { get; set; }
    public DateTime? Afhetningsdato { get; set; }

    public decimal? Transport { get; set; } // Decimal

    // Beregnet pris ud fra produktets aktuelle pris
    public decimal Pris => OrderItems?.Sum(oi => oi.TotalPris) ?? 0;

    public string? Noter { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}