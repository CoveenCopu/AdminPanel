using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class OrderItem
    {
        // Primær nøgle i databasen
        public int Id { get; set; }

        // Fremmednøgle til Order
        public int OrderId { get; set; }

        // Reference til Order objekt
        public Order Order { get; set; }

        // Fremmednøgle til Product
        [Display(Name = "Produkt")]
        public int ProductId { get; set; }

        // Reference til Product objekt
        public Product Product { get; set; }

        // Pris på produktet
        // Fastlåses når ordren oprettes
        [Display(Name = "Pris")]
        public decimal Price
        {
            get => _price;

            set
            {
                // Tjekker at pris ikke er negativ
                if (value < 0)
                    throw new ArgumentException("Pris kan ikke være negativ");

                _price = value;
            }
        }

        // Privat backing field til Price
        private decimal _price { get; set; }

        // Antal af produktet
        [Display(Name = "Antal")]
        public int Quantity
        {
            get => _quantity;

            set
            {
                // Tjekker at antal ikke er negativt
                if (value < 0)
                    throw new ArgumentException("Antal kan ikke være negativ");

                _quantity = value;
            }
        }

        // Privat backing field til Quantity
        private int _quantity { get; set; }

        // Beregner samlet pris automatisk
        // Pris * antal
        [Display(Name = "Samlet pris")]
        public decimal TotalPrice =>
            Price * (decimal)Quantity;
    }
}