namespace AdminPanel.Models
{
    public class YearSummary
    {
        // Årstal for årsopgørelsen
        public int Year { get; set; }

        // Samlet omsætning fra ordrer
        // Inkluderer også transport
        public decimal Revenue { get; set; }

        // Samlede udgifter
        public decimal Expenses { get; set; }

        // Antal jobs/ordrer i året
        public int NumberOfJobs { get; set; }

        // Årets resultat
        // Omsætning - udgifter
        public decimal YearSummaries { get; set; }
    }
}