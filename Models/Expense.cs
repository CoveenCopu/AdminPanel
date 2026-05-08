using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Display(Name = "Produkt")]
        public string Product { get; set; }

        [Display(Name = "Dato")]
        public DateTime Date { get; set; }

        [Display(Name = "Antal")]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Antal kan ikke være negativ");

                _quantity = value;
            }
        }
        private int _quantity { get; set; }

        [Display(Name = "Pris pr. stk.")]
        public decimal PricePerPiece
        {
            get => _PricePerPiece;
            set
            {
                if (value < 0)
                    throw new ArgumentException("pris.pr.stk kan ikke være negativ");

                _PricePerPiece = value;
            }
        }
        private decimal _PricePerPiece { get; set; }

        [Display(Name = "Totale pris")]
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Totalpris kan ikke være negativ");

                _totalPrice = value;
            }
        }
        private decimal _totalPrice { get; set; }

        [Display(Name = "Noter")]
        public string Notes { get; set; }
    }
}