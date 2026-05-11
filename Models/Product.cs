using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class Product
    {
        // Primær nøgle i databasen
        public int Id { get; set; }

        // Produktets navn
        [Display(Name = "Navn")]
        public string Name { get; set; }

        // Produktets pris
        [Display(Name = "Pris")]
        public decimal Price { get; set; }

        // Ekstra information om produktet
        [Display(Name = "Info")]
        public string? Info { get; set; }

        // Antal produkter på lager
        [Display(Name = "Varebeholdning")]
        public int Inventory
        {
            get => _inventory;

            set
            {
                // Tjekker at lagerbeholdning ikke er negativ
                if (value < 0)
                    throw new ArgumentException("Beholdning kan ikke være negativ");

                _inventory = value;
            }
        }

        // Privat backing field til Inventory
        private int _inventory { get; set; }
    }
}