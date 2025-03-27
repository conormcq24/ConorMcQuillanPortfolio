pipeline {
    agent any
    triggers {
        githubPush()
    }
    stages {
        stage('Detect PR Merge') {
            steps {
                script {
                    // Get target branch (the branch that received the merge)
                    def targetBranch = env.GIT_BRANCH ? env.GIT_BRANCH.tokenize('/').last() : "unknown"
                    echo "Target branch: ${targetBranch}"
                    
                    // Get commit message to extract source branch
                    def commitMsg = sh(script: 'git log -1 --pretty=%B', returnStdout: true).trim()
                    echo "Commit message: ${commitMsg}"
                    
                    // Extract source branch - simpler approach
                    def sourceBranch = "unknown"
                    
                    // Check if this is a merge commit from a PR
                    if (commitMsg.contains("Merge pull request") && commitMsg.contains("from ")) {
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
                }
            }
        }
        
        stage('Send Discord Notification') {
            when {
                expression { return env.TARGET_BRANCH == 'test' || env.TARGET_BRANCH == 'main' }
            }
            steps {
                script {
                    // Discord webhook URL
                    def webhookUrl = "https://discordapp.com/api/webhooks/1354215301033759092/_88-RaCajTnr8dA4GJUUqIpQVJnbF2t3n9nq-2VBHbl-ugZw3l7m4_UJ3tZY1fL7GNW1"
                    
                    // Create environment-specific message
                    def environment = env.TARGET_BRANCH == 'main' ? "production" : "test"
                    
                    // Set color based on environment (blue for test, red for production)
                    def color = env.TARGET_BRANCH == 'test' ? '5793266' : '15158332'
                    def timestamp = new Date().format("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", TimeZone.getTimeZone('UTC'))
                    
                    // Escape single quotes for shell command
                    def safeSourceBranch = env.SOURCE_BRANCH.replace("'", "\\'")
                    def safeTargetBranch = env.TARGET_BRANCH.replace("'", "\\'")
                    
                    def discordMessage = """
                    {
                        "username": "Jenkins",
                        "avatar_url": "https://www.jenkins.io/images/logos/jenkins/jenkins.png"
                        "embeds": [{
                            "title": "Build Process Started",
                            "description": "A pull request from **${safeSourceBranch}** branch to **${safeTargetBranch}** branch has started a build process for ${environment} environment",
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
                        curl -X POST "''' + webhookUrl + '''" \\
                        -H "Content-Type: application/json" \\
                        -d @discord_payload.json
                    '''
                    
                    echo "Discord notification sent for build in ${environment} environment"
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