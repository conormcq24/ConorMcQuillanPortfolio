using System.Diagnostics;
using ConorMcQuillanPortfolio.Models;
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

    public IActionResult Devlogs(string appType = "", string technology = "", string sortBy = "date-desc", string page = "")
    {
        ViewData["Carousel"] = GetCarouselItems("Devlogs");


        var journalList = new JournalList();

        journalList.AddJournal(new JournalItem(
            "Building a Portfolio Website",
            "This journal covers my experience creating an ASP.NET Core MVC portfolio website from scratch.",
            "Web Application",
            new string[] { "C#", "ASP.NET Core", "Bootstrap", "MVC", "HTML/CSS" },
            new DateOnly(2025, 3, 6)
        ));

        journalList.AddJournal(new JournalItem(
            "Creating a Task Management API",
            "In this project, I built a RESTful API for managing tasks and projects.",
            "API",
            new string[] { "C#", ".NET Core", "Entity Framework", "SQL Server", "REST" },
            new DateOnly(2025, 2, 15)
        ));

        journalList.AddJournal(new JournalItem(
            "Game Development with Unity",
            "This journal documents my first steps in game development using Unity.",
            "Game",
            new string[] { "C#", "Unity", "3D Modeling", "Game Design" },
            new DateOnly(2025, 2, 15)
        ));

        var filteredJournals = new List<JournalItem>(journalList.journalList);

        // Apply filters
        if (!string.IsNullOrEmpty(appType))
        {
            filteredJournals = filteredJournals.Where(j => j.journalAppType == appType).ToList();
        }

        if (!string.IsNullOrEmpty(technology))
        {
            filteredJournals = filteredJournals.Where(j => j.journalTech.Contains(technology)).ToList();
        }

        switch (sortBy)
        {
            case "date-asc":
                filteredJournals = filteredJournals.OrderBy(j => j.journalDate).ToList();
                break;
            case "title-asc":
                filteredJournals = filteredJournals.OrderBy(j => j.journalTitle).ToList();
                break;
            case "title-desc":
                filteredJournals = filteredJournals.OrderByDescending(j => j.journalTitle).ToList();
                break;
            case "date-desc":
            default:
                filteredJournals = filteredJournals.OrderByDescending(j => j.journalDate).ToList();
                break;
        }

        // Create a new JournalList with the filtered results
        var resultList = new JournalList();
        foreach (var journal in filteredJournals)
        {
            resultList.AddJournal(journal);
        }

        if (!string.IsNullOrEmpty(page))
        {
            // Move the selected journal to the beginning of the list if it exists
            var selectedJournal = resultList.journalList.FirstOrDefault(j => j.journalTitle == page);
            if (selectedJournal != null)
            {
                // Remove and re-add to the beginning of the list
                resultList.journalList.Remove(selectedJournal);
                resultList.journalList.Insert(0, selectedJournal);
            }
        }

        // Store the current filter values in ViewData to maintain state in the form
        ViewData["AppType"] = appType;
        ViewData["Technology"] = technology;
        ViewData["SortBy"] = sortBy;

        return View(resultList);
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
