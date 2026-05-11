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
        public IActionResult Create(Order order, int[] productId, int[] quantity)
        {
            // Tjekker om setup dato mangler
            if (order.SetupDate == null)
                ModelState.AddModelError("Opsætningsdato", "Startdato kræves");

            // Tjekker om pickup dato mangler
            if (order.PickupDate == null)
                ModelState.AddModelError("Afhetningsdato", "Slutdato kræves");

            // Hvis model ikke er valid
            if (!ModelState.IsValid)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(order);
            }

            // Opretter dictionary med lagerbeholdning
            var Availability =
                _context.Products.ToDictionary(p => p.Id, p => p.Inventory);

            // Finder alle ordrer i samme periode
            var ordersInPeriod = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o =>
                    o.SetupDate <= order.PickupDate &&
                    o.PickupDate >= order.SetupDate)
                .ToList();

            // Trækker allerede bookede produkter fra lager
            foreach (var o in ordersInPeriod)
                foreach (var oi in o.OrderItems)
                    Availability[oi.ProductId] -= oi.Quantity;

            // Fejl flag
            bool fejl = false;

            // Tjekker om ønsket antal overstiger lager
            for (int i = 0; i < productId.Length; i++)
            {
                var ant =
                    (i < quantity.Length && quantity[i] > 0)
                    ? quantity[i]
                    : 0;

                if (ant > Availability[productId[i]])
                {
                    ModelState.AddModelError(
                        "",
                        $"Ikke nok af produktet: {ant} ønsket, maks {Availability[productId[i]]}");

                    fejl = true;
                }
            }

            // Returnerer view igen hvis fejl
            if (fejl)
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
            for (int i = 0; i < productId.Length; i++)
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
                        ProductId = productId[i],

                        // Fastlåser antal
                        Quantity = ant,

                        // Fastlåser pris
                        Price = prices[productId[i]]
                    });
                }
            }

            // Genererer noter automatisk
            // Fx "2 x Stol, 1 x Bord"
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
        public IActionResult Edit(
            int id,
            Order order,
            int[] productId,
            int[] quantity)
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

            // Opretter dictionary med lagerbeholdning
            var Availability =
                _context.Products.ToDictionary(p => p.Id, p => p.Inventory);

            // Finder ordrer i samme periode
            // Ekskluderer denne ordre
            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o =>
                    o.Id != id &&
                    o.SetupDate <= dbOrder.PickupDate &&
                    o.PickupDate >= dbOrder.SetupDate)
                .ToList();

            // Trækker bookede produkter fra lager
            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    Availability[oi.ProductId] -= oi.Quantity;

            // Fejl flag
            bool fejl = false;

            // Tjekker lagerbeholdning
            for (int i = 0; i < productId.Length; i++)
            {
                var ant =
                    (i < quantity.Length && quantity[i] > 0)
                    ? quantity[i]
                    : 0;

                if (ant > Availability[productId[i]])
                {
                    ModelState.AddModelError(
                        "",
                        $"Ikke nok af produktet: {ant} ønsket, maks {Availability[productId[i]]}");

                    fejl = true;
                }
            }

            // Returnerer view igen hvis fejl
            if (fejl)
            {
                ViewBag.Produkter = _context.Products.ToList();
                return View(dbOrder);
            }

            // Henter produktpriser
            var prices =
                _context.Products.ToDictionary(p => p.Id, p => p.Price);

            // Gemmer gamle OrderItems
            var existingItems =
                dbOrder.OrderItems.ToDictionary(oi => oi.ProductId);

            // Fjerner gamle items
            dbOrder.OrderItems.Clear();

            // Opretter nye OrderItems
            for (int i = 0; i < productId.Length; i++)
            {
                var ant =
                    (i < quantity.Length && quantity[i] > 0)
                    ? quantity[i]
                    : 0;

                if (ant > 0)
                {
                    decimal price;

                    // Hvis produkt fandtes før
                    if (existingItems.ContainsKey(productId[i]))
                    {
                        // Behold gammel pris
                        price = existingItems[productId[i]].Price;
                    }
                    else
                    {
                        // Nyt produkt får ny pris
                        price = prices[productId[i]];
                    }

                    // Tilføjer item til ordren
                    dbOrder.OrderItems.Add(new OrderItem
                    {
                        ProductId = productId[i],
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
            // Opretter dictionary med lagerbeholdning
            var Availability =
                _context.Products.ToDictionary(p => p.Id, p => p.Inventory);

            // Finder ordrer i perioden
            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o =>
                    o.SetupDate <= end &&
                    o.PickupDate >= start)
                .ToList();

            // Trækker bookede produkter fra lager
            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    Availability[oi.ProductId] -= oi.Quantity;

            // Returnerer data som JSON
            return Json(Availability);
        }
    }
}