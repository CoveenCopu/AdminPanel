namespace AdminPanel.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Info { get; set; }

        // Ny: antal på lager
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