namespace AdminPanel.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProduktId { get; set; }
        public Produkt Produkt { get; set; }

        public int Antal { get; set; }

        // Beregn pris dynamisk fra produkt
        public decimal TotalPris => Produkt?.Pris * Antal ?? 0;
    }
}