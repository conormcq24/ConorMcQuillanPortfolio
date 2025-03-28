FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY . .

# Make sure these environment variables are available at runtime
ENV ASPNETCORE_URLS=http://+:80
ENV DISCORD_PORTFOLIO_INBOX_URL=""
ENV GITHUB_REPO_ACCESS=""

# Expose port 80
EXPOSE 80

# Set the entrypoint to run your application
ENTRYPOINT ["dotnet", "ConorMcQuillanPortfolio.dll"]