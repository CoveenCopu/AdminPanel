using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Controllers
{
    // Brugere og admins må bruge Orders controller
    [Authorize(Roles = "User,Admin")]
    public class OrdersController : Controller
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor som injicerer database context
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Beregner tilgængeligt lager i en given periode
        // Ekskluderer en specifik ordre hvis angivet (bruges ved Edit)
        private Dictionary<int, int> GetAvailability(DateTime start, DateTime end, int? excludeOrderId = null)
        {
            // Opretter dictionary med lagerbeholdning
            var availability =
                _context.Products.ToDictionary(p => p.Id, p => p.Inventory);

            // Finder ordrer i samme periode
            var ordersInPeriod = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o =>
                    o.Id != excludeOrderId &&
                    o.SetupDate <= end &&
                    o.PickupDate >= start)
                .ToList();

            // Trækker allerede bookede produkter fra lager
            foreach (var o in ordersInPeriod)
                foreach (var oi in o.OrderItems)
                    availability[oi.ProductId] -= oi.Quantity;

            return availability;
        }

        // Henter og viser alle ordrer
        // Inkluderer OrderItems og Products
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();

            return View(orders);
        }

        // Viser formular til oprettelse af ordre
        // Sender produktliste til view
        public IActionResult Create()
        {
            ViewBag.Produkter = _context.Products.ToList();
            return View();
        }

        // Opretter ny ordre
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order, Product[] products, int[] quantity)
        {
            // Tjekker om setup dato mangler
            if (order.SetupDate == null)
                ModelState.AddModelError("Opsætningsdato", "Startdato kræves");

            // Tjekker om pickup dato mangler
            if (order.PickupDate == null)
                ModelState.AddModelError("Afhentningsdato", "Slutdato kræves");

            // Hvis model ikke er valid
            if (!ModelState.IsValid)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(order);
            }

            // Henter tilgængeligt lager i perioden
            var availability = GetAvailability(
                order.SetupDate!.Value,
                order.PickupDate!.Value);

            // Fejl flag
            bool hasError = false;

            // Tjekker om ønsket antal overstiger lager
            for (int i = 0; i < products.Length; i++)
            {
                var ant =
                    (i < quantity.Length && quantity[i] > 0)
                    ? quantity[i]
                    : 0;

                if (ant > availability[products[i].Id])
                {
                    ModelState.AddModelError(
                        "",
                        $"Ikke nok af produktet: {products[i].Name}, maks {availability[products[i].Id]}");

                    hasError = true;
                }
            }

            // Returnerer view igen hvis fejl
            if (hasError)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(order);
            }

            // Henter produktpriser
            var prices =
                _context.Products.ToDictionary(p => p.Id, p => p.Price);

            // Opretter liste med OrderItems
            order.OrderItems = new List<OrderItem>();

            // Tilføjer OrderItems til ordren
            for (int i = 0; i < products.Length; i++)
            {
                var ant =
                    (i < quantity.Length && quantity[i] > 0)
                    ? quantity[i]
                    : 0;

                // Kun produkter med antal > 0 tilføjes
                if (ant > 0)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = products[i].Id,

                        // Fastlåser antal
                        Quantity = ant,

                        // Fastlåser pris
                        Price = prices[products[i].Id]
                    });
                }
            }

            // Genererer noter automatisk
            order.Notes = string.Join(
                ", ",
                order.OrderItems.Select(oi =>
                    $"{oi.Quantity} x {oi.Product?.Name ?? _context.Products.First(p => p.Id == oi.ProductId).Name}"));

            // Gemmer ordre i database
            _context.Orders.Add(order);

            // Gemmer ændringer i SQL database
            _context.SaveChanges();

            // Sender bruger tilbage til Index siden
            return RedirectToAction(nameof(Index));
        }

        // Henter ordre til redigering
        public IActionResult Edit(int id)
        {
            // Finder ordre inkl produkter
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            // Hvis ordre ikke findes
            if (order == null)
                return NotFound();

            // Sender produktliste til view
            ViewBag.Produkter = _context.Products.ToList();

            // Sender ordre til view
            return View(order);
        }

        // Opdaterer eksisterende ordre
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit( int id, Order order, Product[] products, int[] quantity)
        {
            // Finder eksisterende ordre i database
            var dbOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            // Hvis ordre ikke findes
            if (dbOrder == null)
                return NotFound();

            // Opdaterer kundeinformation
            dbOrder.Customer = order.Customer;
            dbOrder.City = order.City;
            dbOrder.Address = order.Address;
            dbOrder.Number = order.Number;
            dbOrder.SetupDate = order.SetupDate;
            dbOrder.PickupDate = order.PickupDate;

            // Opdaterer transport
            dbOrder.Transport =
                order.Transport ?? dbOrder.Transport;

            // Henter tilgængeligt lager i perioden
            // Ekskluderer denne ordre så dens egne produkter ikke tælles med
            var availability = GetAvailability(
                dbOrder.SetupDate!.Value,
                dbOrder.PickupDate!.Value,
                excludeOrderId: id);

            // Fejl flag
            bool hasError = false;

            // Tjekker lagerbeholdning
            for (int i = 0; i < products.Length; i++)
            {
                var ant =
                    (i < quantity.Length && quantity[i] > 0)
                    ? quantity[i]
                    : 0;

                if (ant > availability[products[i].Id])
                {
                    ModelState.AddModelError(
                        "",
                        $"Ikke nok af produktet: {products[i].Name}, maks {availability[products[i].Id]}");

                    hasError = true;
                }
            }

            // Returnerer view igen hvis fejl
            if (hasError)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(dbOrder);
            }

            // Henter produktpriser
            var prices = _context.Products.ToDictionary(p => p.Id, p => p.Price);

            // Gemmer gamle OrderItems
            var existingItems = dbOrder.OrderItems.ToDictionary(oi => oi.ProductId);

            // Fjerner gamle items
            dbOrder.OrderItems.Clear();

            // Opretter nye OrderItems
            for (int i = 0; i < products.Length; i++)
            {
                var ant =
                    (i < quantity.Length && quantity[i] > 0)
                    ? quantity[i]
                    : 0;

                if (ant > 0)
                {
                    decimal price;

                    // Hvis produkt fandtes før
                    if (existingItems.ContainsKey(products[i].Id))
                    {
                        // Behold gammel pris
                        price = existingItems[products[i].Id].Price;
                    }
                    else
                    {
                        // Nyt produkt får ny pris
                        price = prices[products[i].Id];
                    }

                    // Tilføjer item til ordren
                    dbOrder.OrderItems.Add(new OrderItem
                    {
                        ProductId = products[i].Id,
                        Quantity = ant,
                        Price = price
                    });
                }
            }

            // Opdaterer noter automatisk
            dbOrder.Notes = string.Join(
                ", ",
                dbOrder.OrderItems.Select(oi =>
                    $"{oi.Quantity} x {oi.Product?.Name ?? _context.Products.First(p => p.Id == oi.ProductId).Name}"));

            // Gemmer ændringer
            _context.SaveChanges();

            // Sender bruger tilbage til Index
            return RedirectToAction(nameof(Index));
        }

        // Viser bekræftelse før sletning
        public IActionResult Delete(int id)
        {
            // Finder ordre inkl produkter
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            // Hvis ordre ikke findes
            if (order == null)
                return NotFound();

            // Sender ordre til view
            return View(order);
        }

        // Sletter ordre
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // Finder ordre inkl items
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            // Hvis ordre ikke findes
            if (order == null)
                return NotFound();

            // Fjerner ordre fra database
            _context.Orders.Remove(order);

            // Gemmer ændringer
            _context.SaveChanges();

            // Sender bruger tilbage til Index
            return RedirectToAction(nameof(Index));
        }

        // Returnerer lager-tilgængelighed via AJAX
        [HttpGet]
        public IActionResult Availability(DateTime start, DateTime end)
        {
            // Henter tilgængeligt lager i perioden
            var availability = GetAvailability(start, end);

            // Returnerer data som JSON
            return Json(availability);
        }
    }
}