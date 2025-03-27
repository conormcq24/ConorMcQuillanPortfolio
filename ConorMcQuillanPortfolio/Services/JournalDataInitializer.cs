// Create this new class
using ConorMcQuillanPortfolio.Services;

public class JournalDataInitializer : IHostedService
{
    private readonly GithubService _githubService;
    private readonly ILogger<JournalDataInitializer> _logger;

    public JournalDataInitializer(GithubService githubService, ILogger<JournalDataInitializer> logger)
    {
        _githubService = githubService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing journal data at application startup");
        try
        {
            await _githubService.PopulateJournalList();
            await _githubService.DownloadImagesAsync();
            _logger.LogInformation("Journal data initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing journal data at startup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}