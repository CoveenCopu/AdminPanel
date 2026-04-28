using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminPanel.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Produkt)
                .ToList();
            return View(orders);
        }

        // GET: Create
        public IActionResult Create()
        {
            ViewBag.Produkter = _context.Produkter.ToList();
            return View();
        }

        // POST: Create
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
                if (antal[i] > tilgængelighed[produktId[i]])
                {
                    ModelState.AddModelError("", $"Ikke nok af produktet: {antal[i]} ønsket, maks {tilgængelighed[produktId[i]]}");
                    fejl = true;
                }
            }

            if (fejl)
            {
                ViewBag.Produkter = _context.Produkter.ToList();
                return View(order);
            }

            // Tilføj OrderItems (ingen låst pris)
            order.OrderItems = new List<OrderItem>();
            for (int i = 0; i < produktId.Length; i++)
            {
                if (antal[i] > 0)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProduktId = produktId[i],
                        Antal = antal[i],
                        Price = _context.Produkter.ToDictionary(p => p.Id, p => p.Pris)[produktId[i]]
                    });
                }
            }

            // Noter automatisk
            order.Noter = string.Join(", ", order.OrderItems.Select(oi =>
                $"{oi.Antal} x {oi.Produkt?.Navn ?? _context.Produkter.First(p => p.Id == oi.ProduktId).Navn}"));

            _context.Orders.Add(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
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

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Order order, int[] produktId, int[] antal)
        {
            var dbOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (dbOrder == null) return NotFound();

            // Opdater kundeinfo og datoer
            dbOrder.Kunde = order.Kunde;
            dbOrder.By = order.By;
            dbOrder.Adresse = order.Adresse;
            dbOrder.Telefonnummer = order.Telefonnummer;
            dbOrder.Opsætningsdato = order.Opsætningsdato;
            dbOrder.Afhetningsdato = order.Afhetningsdato;
            dbOrder.Transport = order.Transport;

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
                if (antal[i] > tilgængelighed[produktId[i]])
                {
                    ModelState.AddModelError("", $"Ikke nok af produktet: {antal[i]} ønsket, maks {tilgængelighed[produktId[i]]}");
                    fejl = true;
                }
            }

            if (fejl)
            {
                ViewBag.Produkter = _context.Produkter.ToList();
                return View(dbOrder);
            }

            // Opdater OrderItems (ingen låst pris)
            dbOrder.OrderItems.Clear();
            for (int i = 0; i < produktId.Length; i++)
            {
                if (antal[i] > 0)
                {
                    dbOrder.OrderItems.Add(new OrderItem
                    {
                        ProduktId = produktId[i],
                        Antal = antal[i]
                    });
                }
            }

            dbOrder.Noter = string.Join(", ", dbOrder.OrderItems.Select(oi =>
                $"{oi.Antal} x {oi.Produkt?.Navn ?? _context.Produkter.First(p => p.Id == oi.ProduktId).Navn}"));

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public IActionResult Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Produkt)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Delete
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

        // AJAX: tilgængelighed for periode
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