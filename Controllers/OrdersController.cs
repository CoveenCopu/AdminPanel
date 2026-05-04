using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AdminPanel.Controllers
{
    [Authorize(Roles = "Bruger,Administrator")]
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
                .ThenInclude(oi => oi.Produkt)
                .ToList();
            return View(orders);
        }

        // Viser formular til oprettelse af ordre og sender produktliste til view
        public IActionResult Create()
        {
            ViewBag.Produkter = _context.Produkter.ToList();
            return View();
        }

        // Opretter en ny ordre:
        // - Validerer datoer
        // - Tjekker lager i perioden
        // - Opretter OrderItems med fastlåst pris
        // - Genererer noter automatisk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order, int[] produktId, int[] antal)
        {
            if (order.Opsætningsdato == null)
                ModelState.AddModelError("Opsætningsdato", "Startdato kræves");
            if (order.Afhetningsdato == null)
                ModelState.AddModelError("Afhetningsdato", "Slutdato kræves");

            if (!ModelState.IsValid)
            {
                ViewBag.Produkter = _context.Produkter.ToList();
                return View(order);
            }

            // Beregn tilgængelighed i perioden
            var tilgængelighed = _context.Produkter.ToDictionary(p => p.Id, p => p.Beholdning);
            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Opsætningsdato <= order.Afhetningsdato && o.Afhetningsdato >= order.Opsætningsdato)
                .ToList();

            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    tilgængelighed[oi.ProduktId] -= oi.Antal;

            // Tjek antal
            bool fejl = false;

            for (int i = 0; i < produktId.Length; i++)
            {
                var ant = (i < antal.Length && antal[i] > 0) ? antal[i] : 0;

                if (ant > tilgængelighed[produktId[i]])
                {
                    ModelState.AddModelError("", $"Ikke nok af produktet: {ant} ønsket, maks {tilgængelighed[produktId[i]]}");
                    fejl = true;
                }
            }

            if (fejl)
            {
                ViewBag.Produkter = _context.Produkter.ToList();
                return View(order);
            }

            // Tilføj OrderItems (pris fastlåses ved oprettelse)
            var priser = _context.Produkter.ToDictionary(p => p.Id, p => p.Pris);
            order.OrderItems = new List<OrderItem>();

            for (int i = 0; i < produktId.Length; i++)
            {
                var ant = (i < antal.Length && antal[i] > 0) ? antal[i] : 0;

                if (ant > 0)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProduktId = produktId[i],
                        Antal = ant,
                        Price = priser[produktId[i]]
                    });
                }
            }

            // Generer noter automatisk (fx "2 x Stol, 1 x Bord")
            order.Noter = string.Join(", ", order.OrderItems.Select(oi =>
                $"{oi.Antal} x {oi.Produkt?.Navn ?? _context.Produkter.First(p => p.Id == oi.ProduktId).Navn}"));

            _context.Orders.Add(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Henter en ordre og viser den til redigering
        public IActionResult Edit(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Produkt)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();
            ViewBag.Produkter = _context.Produkter.ToList();
            return View(order);
        }

        // Opdaterer en ordre:
        // - Opdaterer kundeinfo og datoer
        // - Tjekker lager uden at tælle denne ordre med
        // - Genskaber OrderItems med fastlåst pris
        // - Opdaterer noter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Order order, int[] produktId, int[] antal)
        {
            var dbOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (dbOrder == null) return NotFound();

            // Opdater basisinfo
            dbOrder.Kunde = order.Kunde;
            dbOrder.By = order.By;
            dbOrder.Adresse = order.Adresse;
            dbOrder.Telefonnummer = order.Telefonnummer;
            dbOrder.Opsætningsdato = order.Opsætningsdato;
            dbOrder.Afhetningsdato = order.Afhetningsdato;
            dbOrder.Transport = order.Transport ?? dbOrder.Transport;

            // Beregn tilgængelighed minus denne ordre
            var tilgængelighed = _context.Produkter.ToDictionary(p => p.Id, p => p.Beholdning);
            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Id != id && o.Opsætningsdato <= dbOrder.Afhetningsdato && o.Afhetningsdato >= dbOrder.Opsætningsdato)
                .ToList();

            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    tilgængelighed[oi.ProduktId] -= oi.Antal;

            // Tjek antal
            bool fejl = false;

            for (int i = 0; i < produktId.Length; i++)
            {
                var ant = (i < antal.Length && antal[i] > 0) ? antal[i] : 0;

                if (ant > tilgængelighed[produktId[i]])
                {
                    ModelState.AddModelError("", $"Ikke nok af produktet: {ant} ønsket, maks {tilgængelighed[produktId[i]]}");
                    fejl = true;
                }
            }

            if (fejl)
            {
                ViewBag.Produkter = _context.Produkter.ToList();
                return View(dbOrder);
            }

            // Opdater OrderItems (pris fastlåses igen ved redigering)
            var priser = _context.Produkter.ToDictionary(p => p.Id, p => p.Pris);

            // Gem gamle items (inkl. deres pris)
            var eksisterendeItems = dbOrder.OrderItems.ToDictionary(oi => oi.ProduktId);

            dbOrder.OrderItems.Clear();

            for (int i = 0; i < produktId.Length; i++)
            {
                var ant = (i < antal.Length && antal[i] > 0) ? antal[i] : 0;

                if (ant > 0)
                {
                    decimal price;

                    if (eksisterendeItems.ContainsKey(produktId[i]))
                    {
                        // Brug gammel pris
                        price = eksisterendeItems[produktId[i]].Price;
                    }
                    else
                    {
                        // Nyt produkt → brug ny pris
                        price = priser[produktId[i]];
                    }

                    dbOrder.OrderItems.Add(new OrderItem
                    {
                        ProduktId = produktId[i],
                        Antal = ant,
                        Price = price
                    });
                }
            }

            // Opdater noter
            dbOrder.Noter = string.Join(", ", dbOrder.OrderItems.Select(oi =>
                $"{oi.Antal} x {oi.Produkt?.Navn ?? _context.Produkter.First(p => p.Id == oi.ProduktId).Navn}"));

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Viser bekræftelse før sletning
        public IActionResult Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Produkt)
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
            var tilgængelighed = _context.Produkter.ToDictionary(p => p.Id, p => p.Beholdning);

            var ordersIPeriode = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Opsætningsdato <= end && o.Afhetningsdato >= start)
                .ToList();

            foreach (var o in ordersIPeriode)
                foreach (var oi in o.OrderItems)
                    tilgængelighed[oi.ProduktId] -= oi.Antal;

            return Json(tilgængelighed);
        }
    }
}



