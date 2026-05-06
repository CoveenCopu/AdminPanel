namespace AdminPanel.Models
{
    public class Produkt
    {
        public int Id { get; set; }
        public string Navn { get; set; }
        public decimal Pris { get; set; }
        public string? Info { get; set; }

        // Ny: antal på lager
        public int Beholdning
        {
            get => _beholdning;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Beholdning kan ikke være negativ");

                _beholdning = value;
            }
        }
        private int _beholdning { get; set; }

    }
}