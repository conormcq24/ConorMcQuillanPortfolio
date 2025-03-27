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
                    // Check that the trigger branch is 'test'
                    def targetBranch = env.GIT_BRANCH ? env.GIT_BRANCH.tokenize('/').last() : "unknown"
                    echo "Build triggered for branch: ${targetBranch}"
                    
                    if (targetBranch != 'test') {
                        echo "This build was not triggered for the 'test' branch. Aborting."
                        currentBuild.result = 'ABORTED'
                        error("Build stopped: Not targeting test branch")
                        return
                    }
                    
                    // Set TARGET_BRANCH to 'test'
                    env.TARGET_BRANCH = 'test'
                    
                    // Now that we've confirmed we're on the test branch, get the commit message
                    def commitMsg = sh(script: 'git log -1 --pretty=%B', returnStdout: true).trim()
                    echo "Commit message: ${commitMsg}"
                    
                    // Check if this is a merge commit from a PR
                    if (!commitMsg.contains("Merge pull request")) {
                        echo "This is not a PR merge commit. Aborting."
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
                    withCredentials([string(credentialsId: 'discord-webhook-url', variable: 'DISCORD_WEBHOOK_URL')]) {
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
                            curl -X POST "${DISCORD_WEBHOOK_URL}" \\
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
                        dotnet publish --configuration Release --output ./publish
                    '''
                    
                    echo "Build completed successfully. Ready to create Docker image."
                }
            }
        }
        
        // You can add Docker image creation and deployment stages here
    }
    
    post {
        always {
            cleanWs()
        }
    }
}