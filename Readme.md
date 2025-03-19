# Project Setup

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Required for local development on Windows/Mac)
- All docker commands should be run in the project root directory (where the Dockerfile is located)
- You will need a system environment variable called GITHUB_REPO_ACCESS that contains a git personal access 
  token to my obsidian notes project to pull journal entries

## Option 1: Development with Visual Studio

This approach doesn't require rebuilding a Docker image for every change.

1. Set up your GitHub token as an environment variable:
   - Open Windows search and type "environment variables"
   - Select "Edit the system environment variables"
   - Click "Environment Variables..."
   - Under "System variables", click "New..."
   - Variable name: `GITHUB_REPO_ACCESS`
   - Variable value: `your_actual_token_here`
   - Click OK on all dialogs

2. Select "portfolio-dev-launch" in the dropdown near the start button
3. Click the green Start button
4. The application will launch with HTTPS in your browser

## Option 2: Development with Docker and PowerShell

### 1. Build Docker Image
Build or rebuild the image after making changes to the project:

```
docker build -t portfolio-dev-image .
```

If you encounter this error:
```
ERROR: failed to solve: failed to read dockerfile: open Dockerfile: no such file or directory
```

You're likely in the solution directory instead of the project directory. Navigate to the project directory with:
```
cd ConorMcQuillanPortfolio
```
Verify you're in the correct directory (containing the Dockerfile) with `ls`, then run the build command again.

### 2. Create and Launch Container

For first-time setup:
```
docker run --name portfolio-dev-container -e GITHUB_REPO_ACCESS=your_github_token -p 8080:8080 -p 8081:8081 portfolio-dev-image
```

For subsequent launches, remove the existing container first:
```
docker rm portfolio-dev-container
docker run --name portfolio-dev-container -e GITHUB_REPO_ACCESS=your_github_token -p 8080:8080 -p 8081:8081 portfolio-dev-image
```

Replace `your_github_token` with a valid GitHub token that has access to the repository.

### 3. Managing the Container

You can start and stop the container using Docker Desktop. 
Access the web application by navigating to http://localhost:8080 in your browser, or via the links provided in Docker Desktop.