using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models;

public class Order
{
    // Primær nøgle i databasen
    public int Id { get; set; }

    // Kundens navn
    [Display(Name = "Kunde")]
    public string Customer { get; set; }

    // Kundens by
    [Display(Name = "By")]
    public string City { get; set; }

    // Kundens adresse
    [Display(Name = "Adresse")]
    public string Address { get; set; }

    // Kundens telefonnummer
    [Display(Name = "Telefonnummer")]
    public string Number { get; set; }

    // Dato hvor ordren sættes op
    [Display(Name = "Opsætningsdato")]
    public DateTime? SetupDate { get; set; }

    // Dato hvor ordren afhentes igen
    [Display(Name = "Afhentningsdato")]
    public DateTime? PickupDate { get; set; }

    // Transportpris
    [Display(Name = "Transport")]
    public decimal? Transport { get; set; }

    // Beregner samlet pris automatisk
    // Summerer alle OrderItems totalpris
    [Display(Name = "Pris")]
    public decimal Price =>
        OrderItems?.Sum(oi => oi.TotalPrice) ?? 0;

    // Ekstra noter om ordren
    [Display(Name = "Noter")]
    public string? Notes { get; set; }

    // Liste med produkter tilknyttet ordren
    public List<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();
}