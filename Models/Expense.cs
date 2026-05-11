using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class Expense
    {
        // Primær nøgle i databasen
        public int Id { get; set; }

        // Navn på produkt/udgift
        [Display(Name = "Produkt")]
        public string Product { get; set; }

        // Dato for udgiften
        [Display(Name = "Dato")]
        public DateTime Date { get; set; }

        // Antal produkter
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

        // Pris pr stk
        [Display(Name = "Pris pr. stk.")]
        public decimal PricePerPiece
        {
            get => _PricePerPiece;

            set
            {
                // Tjekker at pris ikke er negativ
                if (value < 0)
                    throw new ArgumentException("pris.pr.stk kan ikke være negativ");

                _PricePerPiece = value;
            }
        }

        // Privat backing field til PricePerPiece
        private decimal _PricePerPiece { get; set; }

        // Samlet pris
        [Display(Name = "Totale pris")]
        public decimal TotalPrice
        {
            get => _totalPrice;

            set
            {
                // Tjekker at totalpris ikke er negativ
                if (value < 0)
                    throw new ArgumentException("Totalpris kan ikke være negativ");

                _totalPrice = value;
            }
        }

        // Privat backing field til TotalPrice
        private decimal _totalPrice { get; set; }

        // Ekstra noter om udgiften
        [Display(Name = "Noter")]
        public string Notes { get; set; }
    }
}