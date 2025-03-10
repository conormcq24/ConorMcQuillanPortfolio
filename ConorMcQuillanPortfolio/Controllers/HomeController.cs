using System.Diagnostics;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProfessionalPortfolio.Models;

namespace ProfessionalPortfolio.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    // private readonly JournalService _journalService;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Carousel"] = GetCarouselItems("Index");
        return View();
    }

    public IActionResult Projects()
    {
        ViewData["Carousel"] = GetCarouselItems("Projects");
        return View();
    }

    public IActionResult Devlogs()
    {
        ViewData["Carousel"] = GetCarouselItems("Devlogs");
        return View();
    }

    public IActionResult Models()
    {
        ViewData["Carousel"] = GetCarouselItems("Models");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static CarouselList GetCarouselItems(string currentView)
    {
        var carouselList = new CarouselList
        {
            CurrentView = currentView,

            PageTitle = currentView switch
            {
                "Index" => "About Me",
                "Projects" => "Projects",
                "Devlogs" => "Development Logs",
                "Models" => "3D Models",
                _ => currentView 
            },

            Items = new List<CarouselItem>
            {
                new CarouselItem
                {
                    action = "Index",
                    description = "A breakdown of my various skills and work history",
                    btnText = "Learn More!",
                    isVisible = currentView != "Index",
                    isActive = false
                },
                new CarouselItem
                {
                    action = "Projects",
                    description = "A collection of personal projects I enjoyed making",
                    btnText = "See My Projects!",
                    isVisible = currentView != "Projects",
                    isActive = false
                },
                new CarouselItem
                {
                    action = "Devlogs",
                    description = "Read dev logs that outline my problem solving skills",
                    btnText = "Read My Journal!",
                    isVisible = currentView != "Devlogs",
                    isActive = false
                },
                new CarouselItem
                {
                    action = "Models",
                    description = "Models I've created and textured for mods and games",
                    btnText = "View My Art!",
                    isVisible = currentView != "Models",
                    isActive = false
                }
            }
        };

        carouselList.Items = carouselList.Items.Where(item => item.isVisible).ToList();
        if (carouselList.Items.Any())
        {
            carouselList.Items[0].isActive = true;
        }

        return carouselList;
    }
}
