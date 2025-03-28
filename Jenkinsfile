pipeline {
    agent any
    triggers {
        githubPush()  // Only trigger for the test branch
    }
    environment {
        DOCKER_REGISTRY = 'localhost'
        TEST_DOMAIN = 'conor-mcquillan-test.com'
        TEST_CONTAINER_NAME = 'portfolio-test'
        TEST_PORT = '5001'
    }
    stages {
        stage('Detect PR Merge to Test') {
            steps {
                script {
                    // Ensure we're on the test branch
                    sh "git checkout test"
                    
                    // Get commit message to extract source branch
                    def commitMsg = sh(script: 'git log -1 --pretty=%B', returnStdout: true).trim()
                    echo "Commit message: ${commitMsg}"
                    
                    // Check if this is a merge commit from a PR
                    if (!commitMsg.contains("Merge pull request")) {
                        echo "This is not a PR merge commit. Skipping build process."
                        currentBuild.result = 'ABORTED'
                        error("Build stopped: Not a PR merge commit")
                        return
                    }
                    
                    // Extract source branch
                    def sourceBranch = "unknown"
                    
                    if (commitMsg.contains("from ")) {
                        // Get everything after "from "
                        def afterFrom = commitMsg.substring(commitMsg.indexOf("from ") + 5)
                        // Get the first word which should be the branch
                        def branchFullPath = afterFrom.split()[0]
                        // Get just the branch name (after last slash if present)
                        sourceBranch = branchFullPath.contains("/") ? 
                            branchFullPath.substring(branchFullPath.lastIndexOf("/") + 1) : 
                            branchFullPath
                    }
                    
                    echo "Detected source branch: ${sourceBranch}"
                    echo "Target branch: test"
                    
                    // Store for potential later use
                    env.SOURCE_BRANCH = sourceBranch
                }
            }
        }
        
        stage('Notify Discord of Test Build Process') {
            steps {
                script {
                    // Get the webhook URL from credentials
                    withCredentials([string(credentialsId: 'discord-build-status', variable: 'DISCORD_BUILD_STATUS')]) {
                        def timestamp = new Date().format("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", TimeZone.getTimeZone('UTC'))
                        
                        // Blue color for test environment
                        def color = '5793266'
                        
                        // Escape single quotes for shell command
                        def safeSourceBranch = env.SOURCE_BRANCH.replace("'", "\\'")
                        
                        def discordMessage = """
                        {
                            "username": "Jenkins",
                            "avatar_url": "https://www.jenkins.io/images/logos/jenkins/jenkins.png",
                            "embeds": [{
                                "title": "Test Build Process Started",
                                "description": "Branch **${safeSourceBranch}** merged into Branch **test** beginning build process for test environment",
                                "color": ${color},
                                "fields": [
                                    {
                                        "name": "Source Branch",
                                        "value": "${safeSourceBranch}"
                                    },
                                    {
                                        "name": "Target Branch",
                                        "value": "test"
                                    }
                                ],
                                "footer": {
                                    "text": "Jenkins CI/CD Notification"
                                },
                                "timestamp": "${timestamp}"
                            }]
                        }
                        """
                        
                        // Write the message to a file to avoid shell escaping issues
                        writeFile file: 'discord_payload.json', text: discordMessage
                        
                        // Send the webhook notification using the file
                        sh '''
                            curl -X POST "${DISCORD_BUILD_STATUS}" \\
                            -H "Content-Type: application/json" \\
                            -d @discord_payload.json
                        '''
                        
                        echo "Discord notification sent for test environment build"
                    }
                }
            }
        }
        
        stage('Build .NET MVC Test Project') {
            steps {
                script {
                    echo "Starting build process for TEST environment..."
                    
                    // Build the .NET project
                    echo "Building .NET MVC project"
                    sh '''
                        dotnet restore
                        dotnet clean
                        dotnet build --configuration Release
                        dotnet publish ./ConorMcQuillanPortfolio/ConorMcQuillanPortfolio.csproj --configuration Release --output ./publish
                    '''
                    
                    echo "Build completed successfully. Ready to create Docker image."
                }
            }
        }
        
        stage('Create and Deploy Docker Image'){
            steps {
                script{
                    echo "Creating Docker image for test environment"

                    withCredentials([
                        string(credentialsId: 'discord-inbox-url', variable: 'DISCORD_PORTFOLIO_INBOX_URL'),
                        string(credentialsId: 'github-repo-access', variable: 'GITHUB_REPO_ACCESS')
                    ]) {
                        sh """
                        # Build the Docker image
                        docker build -t ${DOCKER_REGISTRY}/portfolio:test ./publish
                        
                        # Stop any existing container
                        docker stop ${TEST_CONTAINER_NAME} || true
                        docker rm ${TEST_CONTAINER_NAME} || true
                        
                        # Run the new container
                        docker run -d --name ${TEST_CONTAINER_NAME} \\
                            -p ${TEST_PORT}:80 \\
                            -e ASPNETCORE_ENVIRONMENT=Staging \\
                            -e DISCORD_PORTFOLIO_INBOX_URL='${DISCORD_PORTFOLIO_INBOX_URL}' \\
                            -e GITHUB_REPO_ACCESS='${GITHUB_REPO_ACCESS}' \\
                            ${DOCKER_REGISTRY}/portfolio:test
                        """
                    }
                    
                    echo "Docker container for test environment is now running."

                }
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
    }
}