using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Navn")]
        public string Name { get; set; }

        [Display(Name = "Pris")]
        public decimal Price { get; set; }

        [Display(Name = "Info")]
        public string? Info { get; set; }

        // Ny: antal på lager
        [Display(Name = "Varebeholdning")]
        public int Inventory
        {
            get => _inventory;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Beholdning kan ikke være negativ");

                _inventory = value;
            }
        }

        private int _inventory { get; set; }
    }
}