pipeline {
    agent any
    triggers {
        githubPush()
    }
    parameters {
        string(name: 'BRANCH_NAME', defaultValue: '', description: 'Name of branch that had pull request completed')
        string(name: 'PR_TITLE', defaultValue: '', description: 'Title of the completed pull request')
        string(name: 'PR_URL', defaultValue: '', description: 'URL of the completed pull request')
        string(name: 'PR_AUTHOR', defaultValue: '', description: 'Author of the pull request')
    }
    stages {
        stage('Detect PR Closure') {
            steps {
                script {
                    // Alternative approach without using readJSON
                    // This will be triggered by GitHub webhook
                    // We'll use environment variables directly or parameters
                    
                    // If this is a manual build, use the parameters
                    if (params.BRANCH_NAME && params.PR_URL) {
                        echo "Manual trigger detected with parameters"
                        env.BRANCH_NAME = params.BRANCH_NAME
                        env.PR_TITLE = params.PR_TITLE ?: "Pull Request"
                        env.PR_URL = params.PR_URL
                        env.PR_AUTHOR = params.PR_AUTHOR ?: "Developer"
                    } else {
                        // For webhook triggers, we'd normally use the payload
                        // Since we can't use readJSON, we'll just check if specific env vars exist
                        // These would be provided by GitHub webhook plugin
                        
                        echo "Checking for GitHub webhook trigger"
                        
                        // In a real webhook situation, these would be populated by the GitHub plugin
                        // For now, we'll just check if we're on a monitored branch
                        if (env.GIT_BRANCH) {
                            def branch = env.GIT_BRANCH.tokenize('/').last()
                            env.BRANCH_NAME = branch
                            echo "Detected branch: ${env.BRANCH_NAME}"
                            
                            // Default values if we don't have webhook data
                            env.PR_TITLE = "PR to ${branch}"
                            env.PR_URL = env.GIT_URL ?: "https://github.com/conormcq24/ConorMcQuillanPortfolio"
                            env.PR_AUTHOR = env.GIT_COMMITTER_NAME ?: "Unknown"
                        } else {
                            echo "No branch information detected. Using manual parameters."
                            env.BRANCH_NAME = "main"
                            env.PR_TITLE = "Unknown PR"
                            env.PR_URL = "https://github.com/conormcq24/ConorMcQuillanPortfolio"
                            env.PR_AUTHOR = "Developer"
                        }
                    }
                    
                    echo "Processing for branch: ${env.BRANCH_NAME}"
                    
                    // Check if the branch is one we want to monitor
                    if (env.BRANCH_NAME == 'test' || env.BRANCH_NAME == 'main') {
                        echo "This is a monitored branch: ${env.BRANCH_NAME}"
                    } else {
                        echo "This is not a monitored branch. Skipping notification."
                        currentBuild.result = 'SUCCESS'
                        return
                    }
                }
            }
        }
        
        stage('Send Discord Notification') {
            when {
                expression { return env.BRANCH_NAME in ['test', 'main'] }
            }
            steps {
                script {
                    // Discord webhook URL
                    def webhookUrl = "https://discordapp.com/api/webhooks/1354215301033759092/_88-RaCajTnr8dA4GJUUqIpQVJnbF2t3n9nq-2VBHbl-ugZw3l7m4_UJ3tZY1fL7GNW1"
                    
                    // Create the discord message payload
                    def color = env.BRANCH_NAME == 'test' ? '5793266' : '15158332'
                    def timestamp = new Date().format("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", TimeZone.getTimeZone('UTC'))
                    
                    def discordMessage = """
                    {
                        "embeds": [{
                            "title": "Pull Request Completed",
                            "description": "A pull request has been successfully merged to the **${env.BRANCH_NAME}** branch",
                            "color": ${color},
                            "fields": [
                                {
                                    "name": "PR Title",
                                    "value": "${env.PR_TITLE}"
                                },
                                {
                                    "name": "Author",
                                    "value": "${env.PR_AUTHOR}"
                                },
                                {
                                    "name": "Pull Request URL",
                                    "value": "${env.PR_URL}"
                                }
                            ],
                            "footer": {
                                "text": "Jenkins CI/CD Notification"
                            },
                            "timestamp": "${timestamp}"
                        }]
                    }
                    """
                    
                    // Send the webhook notification
                    // Using triple quotes to avoid escaping issues
                    sh """
                        curl -X POST "${webhookUrl}" \\
                        -H "Content-Type: application/json" \\
                        -d '${discordMessage}'
                    """
                    
                    echo "Discord notification sent for branch: ${env.BRANCH_NAME}"
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