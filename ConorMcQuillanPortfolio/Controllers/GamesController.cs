using Microsoft.AspNetCore.Mvc;

namespace ConorMcQuillanPortfolio.Controllers
{
    public class GamesController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Conors Web Games";
            return View();
        }

        public IActionResult Snake()
        {
            return View();
        }
    }

}