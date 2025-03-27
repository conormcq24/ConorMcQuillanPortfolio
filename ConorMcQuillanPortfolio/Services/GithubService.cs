using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ConorMcQuillanPortfolio.Models;
using System.Security.AccessControl;

namespace ConorMcQuillanPortfolio.Services
{
    public class GithubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GithubService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _repoOwner = "conormcq24";
        private readonly string _repoName = "MyObsidianNotes";


        private JournalList _cachedJournalList = new JournalList();
        private bool _journalsLoaded = false;
        private bool _imagesDownloaded = false;

        public GithubService(ILogger<GithubService> logger, HttpClient httpClient, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _httpClient = httpClient;
            _webHostEnvironment = webHostEnvironment;

            var token = Environment.GetEnvironmentVariable("GITHUB_REPO_ACCESS");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("GitHub access token not found in environment variables.");
                throw new InvalidOperationException("GitHub access token not found in environment variables.");
            }

            _httpClient.BaseAddress = new Uri("https://api.github.com/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ProfessionalPortfolio", "1.0"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> VerifyAuthenticationAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"repos/{_repoOwner}/{_repoName}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully authenticated to repository {Owner}/{Repo}",
                        _repoOwner, _repoName);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to authenticate to repository. Status code: {StatusCode}",
                        response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication to GitHub repository failed");
                return false;
            }
        }

        private class GitHubContent
        {
            public string Name { get; set; } = "";
            public string Path { get; set; } = "";
            public string Type { get; set; } = "";
            public string Download_url { get; set; } = "";
        }

        public async Task<bool> DownloadImagesAsync()
        {
            if (_imagesDownloaded)
            {
                return true;
            }
            try
            {
                if (!await VerifyAuthenticationAsync())
                {
                    return false;
                }
                var response = await _httpClient.GetAsync($"repos/{_repoOwner}/{_repoName}/contents/Notes/images/Notes");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get images folder content. Status code: {StatusCode}",
                        response.StatusCode);
                    return false;
                }
                var contentJson = await response.Content.ReadAsStringAsync();
                var files = JsonSerializer.Deserialize<List<GitHubContent>>(contentJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (files == null || !files.Any())
                {
                    _logger.LogInformation("No files found in the images folder");
                    return true;
                }
                var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "ObsidianImages");
                if (Directory.Exists(imagesPath))
                {
                    var existingFiles = Directory.GetFiles(imagesPath);
                    foreach (var file in existingFiles)
                    {
                        try
                        {
                            System.IO.File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete file {FileName}", file);
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(imagesPath);
                }
                foreach (var file in files)
                {
                    if (file.Type != "file" || string.IsNullOrEmpty(file.Download_url))
                    {
                        continue;
                    }

                    // Use the injected _httpClient instead of creating a new one
                    var fileBytes = await _httpClient.GetByteArrayAsync(file.Download_url);
                    var filePath = Path.Combine(imagesPath, file.Name);
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
                    _logger.LogInformation("Downloaded file {FileName}", file.Name);
                }
                _logger.LogInformation("Successfully downloaded {Count} files to ObsidianImages folder",
                    files.Count(f => f.Type == "file"));

                _imagesDownloaded = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download images from GitHub");
                return false;
            }
        }

        public async Task<JournalList> PopulateJournalList()
        {
            if (_journalsLoaded)
            {
                return _cachedJournalList;
            }

            var journalList = new JournalList();
            try
            {
                // verify that we can authenticate
                if (!await VerifyAuthenticationAsync())
                {
                    _logger.LogWarning("Failed to authenticate when retrieving journal entries");
                    return journalList;
                }

                // requesting a response from github api
                var response = await _httpClient.GetAsync($"repos/{_repoOwner}/{_repoName}/contents/Notes/Programming Journal");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get Programming Journal folder content. Status code: {StatusCode}",
                        response.StatusCode);
                    return journalList;
                }

                // saves response content to a variable
                var contentJson = await response.Content.ReadAsStringAsync();

                // seperates json response from github api into seperate files represented by github content class 
                // defined higher in this file
                var files = JsonSerializer.Deserialize<List<GitHubContent>>(contentJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // checks if any files were included in the response
                // returns an empty list if not
                if (files == null || !files.Any())
                {
                    _logger.LogInformation("No files found in the Programming Journal folder");
                    return journalList;
                }
                foreach (var file in files)
                {
                    // Skip directories and non-markdown files
                    if (file.Type != "file" || !file.Name.EndsWith(".md") || string.IsNullOrEmpty(file.Download_url))
                    {
                        continue;
                    }

                    _logger.LogInformation("Processing file: {FileName}", file.Name);

                    // default information to be overwritten and inserted into JournalItem
                    string journalProperties = "";
                    string journalTitle = "";
                    string journalBody = "";
                    string journalAppType = "";
                    string journalTech = "";
                    string journalDate = "";

                    try
                    {
                        // remove file extension and then save file name to journalTitle
                        journalTitle = Path.GetFileNameWithoutExtension(file.Name);
                        using (HttpClient downloadClient = new HttpClient())
                        {
                            // pull all content from .md file and save to a string
                            string markdownContent = await downloadClient.GetStringAsync(file.Download_url);

                            // section designed to identify and seperate properties and body of markdown content
                            int firstDelimiterIndex = markdownContent.IndexOf("---");
                            if (firstDelimiterIndex >= 0)
                            {
                                int secondDelimiterIndex = markdownContent.IndexOf("---", firstDelimiterIndex + 3);
                                if (secondDelimiterIndex > firstDelimiterIndex)
                                {
                                    int propertiesStart = firstDelimiterIndex + 3; // Skip past the first "---"
                                    int propertiesLength = secondDelimiterIndex - propertiesStart;

                                    // split content into properties and body
                                    journalProperties = markdownContent.Substring(propertiesStart, propertiesLength).Trim();
                                    journalBody = markdownContent.Substring(secondDelimiterIndex + 3).Trim();
                                }
                            }

                            // find starting index of each of the properties
                            int appTypeIndex = journalProperties.IndexOf("application type:", StringComparison.OrdinalIgnoreCase);
                            int techTypeIndex = journalProperties.IndexOf("technologies:", StringComparison.OrdinalIgnoreCase);
                            int dateIndex = journalProperties.IndexOf("date:", StringComparison.OrdinalIgnoreCase);

                            // set journalAppType
                            if (appTypeIndex >= 0)
                            {
                                // 1. offset the start of our value by the character length of "application type:"
                                // 2. end = index of next new line if it exist
                                // 3. if end < 0, we did not find a new line and string hit the end, use end of file as index
                                // 4. if end > start, set journalAppType string using start and end index
                                int start = appTypeIndex + "application type:".Length;
                                int end = journalProperties.IndexOf('\n', start);
                                if (end < 0)
                                {
                                    end = journalProperties.Length;
                                }
                                if (end > start)
                                {
                                    journalAppType = journalProperties.Substring(start, end - start).Trim();
                                    _logger.LogInformation("Application Type: {appType}", journalAppType);
                                }
                                else
                                {
                                    journalAppType = "NOT FOUND";
                                }
                            }
                            else
                            {
                                journalAppType = "NOT FOUND";
                            }

                            if (techTypeIndex >= 0)
                            {
                                // 1. offset the start of our value by the character length of "technologies:"
                                // 2. end = index of next new line if it exist
                                // 3. if end < 0, we did not find a new line and string hit the end, use end of file as index
                                // 4. if end > start, set journalTechType string using start and end index
                                int start = techTypeIndex + "technologies:".Length;
                                int end = journalProperties.IndexOf('\n', start);
                                if (end > start)
                                {
                                    journalTech = journalProperties.Substring(start, end - start).Trim();
                                    _logger.LogInformation("Technologies: {techType}", journalTech);
                                }
                                else
                                {
                                    journalTech = "NOT FOUND";
                                }
                            }
                            else
                            {
                                journalTech = "NOT FOUND";
                            }

                            if (dateIndex >= 0)
                            {
                                // 1. offset the start of our value by the character length of "date:"
                                // 2. end = index of next new line if it exist
                                // 3. if end < 0, we did not find a new line and string hit the end, use end of file as index
                                // 4. if end > start, set journalDate string using start and end index
                                int start = dateIndex + "date:".Length;
                                int end = journalProperties.IndexOf('\n', start);
                                if (end > start)
                                {
                                    journalDate = journalProperties.Substring(start, end - start).Trim();
                                    _logger.LogInformation("Date: {techType}", journalDate);
                                }
                                else
                                {
                                    journalDate = "NOT FOUND";
                                }
                            }
                            else
                            {
                                journalDate = "NOT FOUND";
                            }

                            // ensure journal fields are valid before adding item to the list
                            if (ValidateJournalFields(journalTitle, journalBody, journalAppType, journalTech, journalDate, file.Name))
                            {
                                _logger.LogInformation("SUCCESFULL PARSE");
                                // Create a new JournalItem using the modified constructor
                                // that handles parsing technologies and date internally
                                JournalItem journalItem = new JournalItem(
                                    journalTitle,
                                    journalBody,
                                    journalAppType,
                                    journalTech,
                                    journalDate
                                );

                                // Add the item to our journal list
                                journalList.AddJournal(journalItem);
                                _logger.LogInformation("Added journal entry: {Title}", journalTitle);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error Processing File {FileName}", file.Name);
                        return journalList;
                    }
                }

                _cachedJournalList = journalList;
                _journalsLoaded = true;

                return journalList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to populate JournalList");
                return journalList;
            }
        }

        private bool ValidateJournalFields(string title, string body, string appType, string tech, string date, string fileName)
        {
            bool isValid = true;


            if (string.IsNullOrEmpty(title) || title == "NOT FOUND")
            {
                _logger.LogWarning("File has invalid title: {FileName}", fileName);
                isValid = false;
            }

            if (string.IsNullOrEmpty(body) || body == "NOT FOUND")
            {
                _logger.LogWarning("File has invalid body content: {FileName}", fileName);
                isValid = false;
            }

            if (string.IsNullOrEmpty(appType) || appType == "NOT FOUND")
            {
                _logger.LogWarning("File has invalid application type: {FileName}", fileName);
                isValid = false;
            }

            if (string.IsNullOrEmpty(tech) || tech == "NOT FOUND")
            {
                _logger.LogWarning("File has invalid technologies: {FileName}", fileName);
                isValid = false;
            }
            else
            {
                if (!tech.StartsWith("'\"") || !tech.EndsWith("\"'"))
                {
                    _logger.LogWarning("Technologies are not properly quoted: {FileName}", fileName);
                    isValid = false;
                }
            }


            if (string.IsNullOrEmpty(date) || date == "NOT FOUND")
            {
                _logger.LogWarning("File has invalid date: {FileName}", fileName);
                isValid = false;
            }

            return isValid;
        }
    }
}