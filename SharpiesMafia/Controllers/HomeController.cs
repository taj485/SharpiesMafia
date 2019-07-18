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

        public IActionResult UsersToKill()
        {
            return PartialView("~/Views/Home/_UsersToKill.cshtml");
        }
        
        public IActionResult UsersToKillMafia()
        {
            return PartialView("~/Views/Home/_UsersToKillMafia.cshtml");
        }


        public IActionResult LoadNightScreen()
        {
            return PartialView("~/Views/Home/_NightTimePartial.cshtml");
        }

        public IActionResult MafiaScreen()
        {
            return PartialView("~/Views/Home/_MafiaScreenPartial.cshtml");
        }
        
        public IActionResult VillagerScreen()
        {
            return PartialView("~/Views/Home/_VillagerScreenPartial.cshtml");
        }

        public IActionResult LoadDayScreen()
        {
            return PartialView("~/Views/Home/_DayTimePartial.cshtml");
        }

        public IActionResult YouDiedScreen()
        {
            return PartialView("~/Views/Home/_DeathPartial.cshtml");
        }

        public IActionResult VillagerWinScreen()
        {
            return PartialView("~/Views/Home/_VillagerWinPartial.cshtml");
        }

        public IActionResult MafiaWinScreen()
        {
            return PartialView("~/Views/Home/_MafiaWinPartial.cshtml");
        }

        public IActionResult LoadVoteResultScreen()
        {
            return PartialView("~/Views/Home/_VoteResultScreenPartial.cshtml");
        }

        public IActionResult LoadVictimResultScreen()
        {
            return PartialView("~/Views/Home/_VictimResultPartial.cshtml");
        }

        public IActionResult LoadMafiaNightScreen()
        {
            return PartialView("~/Views/Home/_NightTimePartialMafiaSound.cshtml");
        }
        
    }
}
