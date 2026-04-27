namespace AdminPanel.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Produkt { get; set; }
        public DateTime Dato { get; set; }
        public int Antal { get; set; }
        public decimal PrisPrStk { get; set; }
        public decimal TotalPris { get; set; }
        public string Noter { get; set; }
    }
}