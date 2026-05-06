namespace AdminPanel.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Produkt { get; set; }
        public DateTime Dato { get; set; }
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

        public decimal PrisPrStk
        {
            get => _prisPrStk;
            set
            {
                if (value < 0)
                    throw new ArgumentException("pris.pr.stk kan ikke være negativ");

                _prisPrStk = value;
            }
        }
        private decimal _prisPrStk { get; set; }

        public decimal TotalPris
        {
            get => _totalPris;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Totalpris kan ikke være negativ");

                _totalPris = value;
            }
        }
        private decimal _totalPris { get; set; }
        public string Noter { get; set; }
    }
}