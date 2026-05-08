using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Pris kan ikke være negativ");

                _price = value;
            }
        }
        private decimal _price { get; set; }

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

        // Beregn pris dynamisk fra produkt
        public decimal TotalPrice => Price * (decimal)Quantity;
    }
}