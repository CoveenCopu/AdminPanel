namespace AdminPanel.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public DateTime Date { get; set; }
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
        public string Notes { get; set; }
    }
}