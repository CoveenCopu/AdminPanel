namespace AdminPanel.Models
{
    public class YearSummary
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }       // Sum af Orders (inkl. Transport)
        public decimal Expenses { get; set; }        // Sum af Expenses
        public int NumberOfJobs { get; set; }           // Antal Orders
        public decimal YearSummaries { get; set; }    // Omsætning - Udgifter
    }
}