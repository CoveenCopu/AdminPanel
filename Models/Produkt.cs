namespace AdminPanel.Models
{
    public class Produkt
    {
        public int Id { get; set; }
        public string Navn { get; set; }
        public decimal Pris { get; set; }
        public string? Info { get; set; }

        // Ny: antal på lager
        public int Beholdning { get; set; }
    }
}