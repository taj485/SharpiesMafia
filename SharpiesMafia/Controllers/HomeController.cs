using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharpiesMafia.Models;

namespace SharpiesMafia.Controllers
{
    public class HomeController : Controller
    {
        private readonly MafiaContext _context;
        public HomeController (MafiaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost ("CreateUser")]
        public async Task CreateUser (string userName)
        {
            var user = new User() { name = userName };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
