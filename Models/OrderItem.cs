using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProduktId { get; set; }
        public Produkt Produkt { get; set; }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Pris kan ikke være negativ");

                _price = value;
            }
        }
        private decimal _price { get; set; }

        public int Antal
        {
            get => _antal;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Antal kan ikke være negativ");

                _antal = value;
            }
        }
        private int _antal { get; set; }

        // Beregn pris dynamisk fra produkt
        public decimal TotalPris => Price * (decimal)Antal;
    }
}