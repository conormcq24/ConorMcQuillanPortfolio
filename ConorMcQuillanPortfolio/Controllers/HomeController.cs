using System.Diagnostics;
using System.Security.AccessControl;
using ConorMcQuillanPortfolio.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ConorMcQuillanPortfolio.Models;
using ConorMcQuillanPortfolio.Services;

namespace ConorMcQuillanPortfolio.Controllers;

public class HomeController : Controller
{

    private JournalList _unfilteredList = new JournalList();
    private JournalList _filteredList = new JournalList();


    private readonly ILogger<HomeController> _logger;
    private readonly GithubService _githubService;

    public HomeController(
        ILogger<HomeController> logger,
        GithubService githubService)
    {
        _logger = logger;
        _githubService = githubService;
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

    public async Task<IActionResult> TestGitHubAuth()
    {
        bool isAuthenticated = await _githubService.VerifyAuthenticationAsync();

        ViewData["IsAuthenticated"] = isAuthenticated;
        _logger.LogInformation("GitHub authentication test result: {Result}", isAuthenticated);

        return View();
    }

    public async Task<IActionResult> TestDownloadImages()
    {
        JournalList testJournalList = new JournalList();
        testJournalList = await _githubService.PopulateJournalList();
        bool success = await _githubService.DownloadImagesAsync();

        var result = new
        {
            Success = success,
            Message = success ? "Images successfully downloaded" : "Failed to download images",
            Timestamp = DateTime.Now
        };

        return Json(result);
    }

    public async Task<IActionResult> Devlogs(string appType = "", string techType = "", string orderType = "DateDown", string selectedJournal = "")
    {
        // Check for no filter and empty strings if occurs
        if (appType == "All App Types")
        {
            appType = "";
        }

        if (techType == "All Tech Types")
        {
            techType = "";
        }

        // Get journal entries from GitHub repository instead of using dummy data
        _unfilteredList = await _githubService.PopulateJournalList();

        // If no journals were found, provide fallback data
        if (_unfilteredList.journalList.Count == 0)
        {
            _logger.LogWarning("No journal entries found from GitHub, using fallback data");

            // Create fallback dummy list
            _unfilteredList.AddJournal(new JournalItem(
                "Building a Portfolio Website",
                "This journal covers my experience creating an ASP.NET Core MVC portfolio website from scratch.",
                "Web Application",
                "'\"C#, .NET CORE, VS Studio\"'",
                "3/19/2025"
            ));

            _unfilteredList.AddJournal(new JournalItem(
                "Creating a Task Management API",
                "In this project, I built a RESTful API for managing tasks and projects.",
                "API",
                "'\"C#, .NET CORE, VS Studio, API\"'",
                "3/18/2025"
            ));

            _unfilteredList.AddJournal(new JournalItem(
                "Game Development with Unity",
                "This journal documents my first steps in game development using Unity.",
                "Game",
                "'\"C#, Unity, Blender\"'",
                "3/19/2025"
            ));
        }

        // Create a copy of the list for _filteredlist
        _filteredList = new JournalList();
        foreach (var item in _unfilteredList.journalList)
        {
            _filteredList.AddJournal(item);
        }
        _filteredList = trimList(_filteredList, appType, techType, orderType);

        // Get all tech types from unfilteredList
        List<string> allTechTypes = _unfilteredList.GetUniqueTechnologies();
        allTechTypes.Insert(0, "All Tech Types");

        // Get all app types from unfilteredList
        List<string> allAppTypes = _unfilteredList.GetUniqueAppTypes();
        allAppTypes.Insert(0, "All App Types");

        // Get active journal
        JournalItem journalToDisplay;
        if (string.IsNullOrEmpty(selectedJournal) || !_filteredList.journalList.Any(j => j.journalTitle == selectedJournal))
        {
            // Default to the first item in the filtered list
            journalToDisplay = _filteredList.journalList.FirstOrDefault();
        }
        else
        {
            // Find the selected journal
            journalToDisplay = _filteredList.journalList.FirstOrDefault(j => j.journalTitle == selectedJournal);
        }

        ViewData["Carousel"] = GetCarouselItems("Devlogs");
        ViewData["UnfilteredList"] = _unfilteredList;
        ViewData["FilteredList"] = _filteredList;
        ViewData["AppType"] = appType;
        ViewData["TechType"] = techType;
        ViewData["OrderType"] = orderType;
        ViewData["AllAppTypes"] = allAppTypes;
        ViewData["AllTechTypes"] = allTechTypes;
        ViewData["SelectedJournal"] = journalToDisplay;

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

    private static JournalList trimList(JournalList filteredList, string appType, string techType, string orderType)
    {
        if (!string.IsNullOrEmpty(appType))
        {
            filteredList.journalList = filteredList.journalList.Where(item => item.journalAppType == appType).ToList();
        }
        if (!string.IsNullOrEmpty(techType))
        {
            filteredList.journalList = filteredList.journalList.Where(item => item.journalTech.Contains(techType)).ToList();
        }
        if (!string.IsNullOrEmpty(orderType))
        {
            switch (orderType)
            {
                case "A-Z":
                    filteredList.journalList = filteredList.journalList.OrderBy(item => item.journalTitle).ToList();
                    break;
                case "Z-A":
                    filteredList.journalList = filteredList.journalList.OrderByDescending(item => item.journalTitle).ToList();
                    break;
                case "DateUp":
                    filteredList.journalList = filteredList.journalList.OrderBy(item => item.journalDate).ToList();
                    break;
                case "DateDown":
                    filteredList.journalList = filteredList.journalList.OrderByDescending(item => item.journalDate).ToList();
                    break;
            }
        }

        return filteredList;
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
