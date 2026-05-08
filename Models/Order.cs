using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models;

public class Order
{
    public int Id { get; set; }

    [Display(Name = "Kunde")]
    public string Customer { get; set; }

    [Display(Name = "By")]
    public string City { get; set; }

    [Display(Name = "Adresse")]
    public string Address { get; set; }

    [Display(Name = "Telefonnummer")]
    public string Number { get; set; }

    [Display(Name = "Opsætningsdato")]
    public DateTime? SetupDate { get; set; }

    [Display(Name = "Afhentningsdato")]
    public DateTime? PickupDate { get; set; }

    [Display(Name = "Transport")]
    public decimal? Transport { get; set; } // Decimal

    // Beregnet pris ud fra produktets aktuelle pris
    [Display(Name = "Pris")]
    public decimal Price => OrderItems?.Sum(oi => oi.TotalPrice) ?? 0;

    [Display(Name = "Noter")]
    public string? Notes { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}