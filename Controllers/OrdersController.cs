using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AdminPanel.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Henter og viser alle ordrer inkl. order items og produkter
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();
            return View(orders);
        }

        // Viser formular til oprettelse af ordre og sender produktliste til view
        public IActionResult Create()
        {
            ViewBag.Produkter = _context.Products.ToList();
            return View();
        }

        // Opretter en ny ordre:
        // - Validerer datoer
        // - Tjekker lager i perioden
        // - Opretter OrderItems med fastlåst pris
        // - Genererer noter automatisk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order, int[] productId, int[] antal)
        {
            if (order.SetupDate == null)
                ModelState.AddModelError("Opsætningsdato", "Startdato kræves");
            if (order.PickupDate == null)
                ModelState.AddModelError("Afhetningsdato", "Slutdato kræves");

            if (!ModelState.IsValid)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(order);
            }

            // Beregn tilgængelighed i perioden
            var tilgængelighed = _context.Products.ToDictionary(p => p.Id, p => p.Inventory);
            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.SetupDate <= order.PickupDate && o.PickupDate >= order.SetupDate)
                .ToList();

            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    tilgængelighed[oi.ProductId] -= oi.Quantity;

            // Tjek antal
            bool fejl = false;

            for (int i = 0; i < productId.Length; i++)
            {
                var ant = (i < antal.Length && antal[i] > 0) ? antal[i] : 0;

                if (ant > tilgængelighed[productId[i]])
                {
                    ModelState.AddModelError("", $"Ikke nok af produktet: {ant} ønsket, maks {tilgængelighed[productId[i]]}");
                    fejl = true;
                }
            }

            if (fejl)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(order);
            }

            // Tilføj OrderItems (pris fastlåses ved oprettelse)
            var prices = _context.Products.ToDictionary(p => p.Id, p => p.Price);
            order.OrderItems = new List<OrderItem>();

            for (int i = 0; i < productId.Length; i++)
            {
                var ant = (i < antal.Length && antal[i] > 0) ? antal[i] : 0;

                if (ant > 0)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = productId[i],
                        Quantity = ant,
                        Price = prices[productId[i]]
                    });
                }
            }

            // Generer noter automatisk (fx "2 x Stol, 1 x Bord")
            order.Notes = string.Join(", ", order.OrderItems.Select(oi =>
                $"{oi.Quantity} x {oi.Product?.Name ?? _context.Products.First(p => p.Id == oi.ProductId).Name}"));

            _context.Orders.Add(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Henter en ordre og viser den til redigering
        public IActionResult Edit(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();
            ViewBag.Produkter = _context.Products.ToList();
            return View(order);
        }

        // Opdaterer en ordre:
        // - Opdaterer kundeinfo og datoer
        // - Tjekker lager uden at tælle denne ordre med
        // - Genskaber OrderItems med fastlåst pris
        // - Opdaterer noter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Order order, int[] productId, int[] quantity)
        {
            var dbOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (dbOrder == null) return NotFound();

            // Opdater basisinfo
            dbOrder.Customer = order.Customer;
            dbOrder.City = order.City;
            dbOrder.Address = order.Address;
            dbOrder.Number = order.Number;
            dbOrder.SetupDate = order.SetupDate;
            dbOrder.PickupDate = order.PickupDate;
            dbOrder.Transport = order.Transport ?? dbOrder.Transport;

            // Beregn tilgængelighed minus denne ordre
            var tilgængelighed = _context.Products.ToDictionary(p => p.Id, p => p.Inventory);
            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Id != id && o.SetupDate <= dbOrder.PickupDate && o.PickupDate >= dbOrder.SetupDate)
                .ToList();

            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    tilgængelighed[oi.ProductId] -= oi.Quantity;

            // Tjek antal
            bool fejl = false;

            for (int i = 0; i < productId.Length; i++)
            {
                var ant = (i < quantity.Length && quantity[i] > 0) ? quantity[i] : 0;

                if (ant > tilgængelighed[productId[i]])
                {
                    ModelState.AddModelError("", $"Ikke nok af produktet: {ant} ønsket, maks {tilgængelighed[productId[i]]}");
                    fejl = true;
                }
            }

            if (fejl)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(dbOrder);
            }

            // Opdater OrderItems (pris fastlåses igen ved redigering)
            var prices = _context.Products.ToDictionary(p => p.Id, p => p.Price);

            // Gem gamle items (inkl. deres pris)
            var eksisterendeItems = dbOrder.OrderItems.ToDictionary(oi => oi.ProductId);

            dbOrder.OrderItems.Clear();

            for (int i = 0; i < productId.Length; i++)
            {
                var ant = (i < quantity.Length && quantity[i] > 0) ? quantity[i] : 0;

                if (ant > 0)
                {
                    decimal price;

                    if (eksisterendeItems.ContainsKey(productId[i]))
                    {
                        // Brug gammel pris
                        price = eksisterendeItems[productId[i]].Price;
                    }
                    else
                    {
                        // Nyt produkt → brug ny pris
                        price = prices[productId[i]];
                    }

                    dbOrder.OrderItems.Add(new OrderItem
                    {
                        ProductId = productId[i],
                        Quantity = ant,
                        Price = price
                    });
                }
            }

            // Opdater noter
            dbOrder.Notes = string.Join(", ", dbOrder.OrderItems.Select(oi =>
                $"{oi.Quantity} x {oi.Product?.Name ?? _context.Products.First(p => p.Id == oi.ProductId).Name}"));

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Viser bekræftelse før sletning
        public IActionResult Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // Sletter en ordre
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            _context.Orders.Remove(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Returnerer lager-tilgængelighed i en periode (bruges via AJAX)
        [HttpGet]
        public IActionResult Availability(DateTime start, DateTime end)
        {
            var tilgængelighed = _context.Products.ToDictionary(p => p.Id, p => p.Inventory);

            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.SetupDate <= end && o.PickupDate >= start)
                .ToList();

            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    tilgængelighed[oi.ProductId] -= oi.Quantity;

            return Json(tilgængelighed);
        }
    }
}



