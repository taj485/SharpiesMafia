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

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult StartGameScreen()
        {
            return PartialView("~/Views/Home/_StartGamePartial.cshtml");
        }

        public IActionResult JoinGameScreen()
        {
            return PartialView("~/Views/Home/_JoinGamePartial.cshtml");
        }
    }
}
