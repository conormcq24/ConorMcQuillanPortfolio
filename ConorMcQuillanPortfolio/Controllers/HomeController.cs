using System.Diagnostics;
using System.Security.AccessControl;
using ConorMcQuillanPortfolio.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ConorMcQuillanPortfolio.Models;
using ConorMcQuillanPortfolio.Services;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;


namespace ConorMcQuillanPortfolio.Controllers;

public class HomeController : Controller
{
    private JournalList _filteredList = new JournalList();


    private readonly ILogger<HomeController> _logger;
    private readonly GithubService _githubService;
    private readonly IWebHostEnvironment _environment;

    public HomeController(
        ILogger<HomeController> logger,
        GithubService githubService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _githubService = githubService;
        _environment = environment;
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

        var unfilteredList = await _githubService.PopulateJournalList();
        unfilteredList.journalList = unfilteredList.journalList.OrderByDescending(item => item.journalDate).ToList();

        // Get journal entries from GitHub repository instead of using dummy data
        unfilteredList = await _githubService.PopulateJournalList();
        

        // Create a copy of the list for _filteredlist
        _filteredList = new JournalList();
        foreach (var item in unfilteredList.journalList)
        {
            _filteredList.AddJournal(item);
        }
        _filteredList = trimList(_filteredList, appType, techType, orderType);

        // Get all tech types from unfilteredList
        List<string> allTechTypes = unfilteredList.GetUniqueTechnologies();
        allTechTypes.Insert(0, "All Tech Types");

        // Get all app types from unfilteredList
        List<string> allAppTypes = unfilteredList.GetUniqueAppTypes();
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
        ViewData["UnfilteredList"] = unfilteredList;
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
    [HttpPost]
    public async Task<IActionResult> SendEmail(string FirstName, string LastName, string Email, string PhoneNumber, string Message)
    {
        try
        {
            string discordWebhookUrl = Environment.GetEnvironmentVariable("DISCORD_HOOK");
            var payload = new
            {
                content = $"**Portfolio Contact:** {FirstName} {LastName}\n" +
                          $"**Email:** {Email}\n" +
                          $"**Phone:** {PhoneNumber}\n\n" +
                          $"**Message:**\n{Message}"
            };

            // Serialize the payload to JSON
            var jsonPayload = JsonConvert.SerializeObject(payload);

            // Send the message to Discord via HTTP POST
            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(discordWebhookUrl, content);

                // Ensure the response indicates success
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Message sent successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to send message: {response.ReasonPhrase}";
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending a a a message: " + ex.Message);
            TempData["ErrorMessage"] = "Failed to send message: " + ex.Message;
            return RedirectToAction("Index");
        }
    }
}
