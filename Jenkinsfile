pipeline {
    agent any
    triggers {
        githubPush()
    }
    environment {
        DOCKER_REGISTRY = 'localhost'
        TEST_DOMAIN = 'conor-mcquillan-test.com'
        PROD_DOMAIN = 'conor-mcquillan.com'
        TEST_CONTAINER_NAME = 'portfolio-test'
        PROD_CONTAINER_NAME = 'portfolio-prod'
        TEST_PORT = '5001'
        PROD_PORT = '5002'
    }
    stages {
        stage('Detect PR Merge') {
            steps {
                script {
                    // Get target branch (the branch that received the merge)
                    def targetBranch = env.GIT_BRANCH ? env.GIT_BRANCH.tokenize('/').last() : "unknown"
                    echo "Target branch: ${targetBranch}"
                    
                    // Check if this is a target branch we want to process
                    if (!(targetBranch == 'test' || targetBranch == 'main')) {
                        echo "Target branch is not 'test' or 'main'. Skipping build process."
                        currentBuild.result = 'ABORTED'
                        error("Build stopped: Target branch ${targetBranch} is not monitored")
                        return
                    }
                    
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
                    echo "Detected target branch: ${targetBranch}"
                    
                    // Store for potential later use
                    env.SOURCE_BRANCH = sourceBranch
                    env.TARGET_BRANCH = targetBranch
                    
                    // Set which environment to build based on target branch
                    if (targetBranch == 'test') {
                        env.BUILD_TEST = 'true'
                        env.BUILD_PROD = 'false'
                    } else if (targetBranch == 'main') {
                        env.BUILD_TEST = 'false'
                        env.BUILD_PROD = 'true'
                    }
                }
            }
        }
        stage('Notify Discord of Build Process') {
            steps {
                script {
                    // Get the webhook URL from credentials
                    withCredentials([string(credentialsId: 'discord-webhook-url', variable: 'DISCORD_WEBHOOK_URL')]) {
                        def timestamp = new Date().format("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", TimeZone.getTimeZone('UTC'))
                        
                        // Determine environment name based on target branch
                        def environment = env.TARGET_BRANCH == 'main' ? "production" : "test"
                        def color = env.TARGET_BRANCH == 'test' ? '5793266' : '15158332'
                        
                        // Escape single quotes for shell command
                        def safeSourceBranch = env.SOURCE_BRANCH.replace("'", "\\'")
                        def safeTargetBranch = env.TARGET_BRANCH.replace("'", "\\'")
                        
                        def discordMessage = """
                        {
                            "username": "Jenkins",
                            "avatar_url": "https://www.jenkins.io/images/logos/jenkins/jenkins.png",
                            "embeds": [{
                                "title": "Build Process Started",
                                "description": "Branch **${safeSourceBranch}** merged into Branch **${safeTargetBranch}** beginning build process for ${environment} environment",
                                "color": ${color},
                                "fields": [
                                    {
                                        "name": "Source Branch",
                                        "value": "${safeSourceBranch}"
                                    },
                                    {
                                        "name": "Target Branch",
                                        "value": "${safeTargetBranch}"
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
                        
                        echo "Discord notification sent for build in ${environment} environment"
                    }
                }
            }
        }
        stage('Build .NET MVC Portfolio Project') {
            when {
                expression { return env.BUILD_TEST == 'true' } 
            }
            steps {
                script {
                    echo "Starting build process for TEST environment..."
                    
                    // Step 1: Checkout the correct branch
                    echo "Checking out test branch"
                    sh "git checkout test"
                    
                    // Step 2: Build the .NET project
                    echo "Building .NET MVC project"
                    sh '''
                        dotnet restore
                        dotnet clean
                        dotnet build --configuration Release
                        dotnet publish --configuration Release --output ./publish
                    '''
                    
                    // At this point we'll check if the build was successful
                    echo "Build completed successfully. Ready to create Docker image."
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