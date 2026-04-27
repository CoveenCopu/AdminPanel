namespace AdminPanel.Models
{
    public class YearSummary
    {
        public int År { get; set; }
        public decimal Omsætning { get; set; }       // Sum af Orders (inkl. Transport)
        public decimal Udgifter { get; set; }        // Sum af Expenses
        public int AntalJobs { get; set; }           // Antal Orders
        public decimal Årsopgørelse { get; set; }    // Omsætning - Udgifter
    }
}