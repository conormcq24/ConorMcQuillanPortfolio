pipeline {
    agent any
    triggers {
        githubPush()
    }
    stages {
        stage('Detect PR Merge') {
            steps {
                script {
                    // Get current branch (target branch of the merge)
                    def targetBranch = env.GIT_BRANCH ? env.GIT_BRANCH.tokenize('/').last() : "unknown"
                    echo "Target branch: ${targetBranch}"
                    
                    // Get source branch info from commit message (if available)
                    def sourceBranch = "unknown"
                    def commitMsg = sh(script: 'git log -1 --pretty=%B', returnStdout: true).trim()
                    echo "Commit message: ${commitMsg}"
                    
                    // Extract source branch from merge commit message without using regex Matcher
                    // Typical format: "Merge pull request #X from source/branch"
                    if (commitMsg.contains("Merge pull request") && commitMsg.contains("from ")) {
                        // Split by "from " and take the second part
                        def fromParts = commitMsg.split("from ")
                        if (fromParts.length > 1) {
                            // Take the branch name part and handle possible newlines
                            def branchPath = fromParts[1].trim().split("\\s")[0]
                            // Extract the branch name (last part after any slashes)
                            sourceBranch = branchPath.tokenize('/').last()
                        }
                    }
                    
                    // Extract PR URL if possible without using regex
                    def prUrl = "Not available"
                    if (commitMsg.contains("#")) {
                        def hashParts = commitMsg.split("#")
                        if (hashParts.length > 1) {
                            // Extract PR number, handling possible non-digit characters after it
                            def prNumStr = hashParts[1].trim()
                            def prNum = ""
                            for (int i = 0; i < prNumStr.length(); i++) {
                                if (prNumStr.charAt(i).isDigit()) {
                                    prNum += prNumStr.charAt(i)
                                } else {
                                    break
                                }
                            }
                            
                            if (prNum) {
                                def repoUrl = sh(script: 'git config --get remote.origin.url', returnStdout: true).trim()
                                // Convert SSH URL to HTTPS if needed
                                repoUrl = repoUrl.replace('git@github.com:', 'https://github.com/')
                                repoUrl = repoUrl.replace('.git', '')
                                prUrl = "${repoUrl}/pull/${prNum}"
                            }
                        }
                    }
                    
                    // Store the variables for later use
                    env.TARGET_BRANCH = targetBranch
                    env.SOURCE_BRANCH = sourceBranch
                    env.PR_URL = prUrl
                    
                    echo "Source branch: ${sourceBranch}"
                    echo "PR URL: ${prUrl}"
                    
                    // Only proceed for monitored branches
                    if (targetBranch == 'test' || targetBranch == 'main') {
                        echo "This is a monitored branch: ${targetBranch}"
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
                    def safePrUrl = env.PR_URL.replace("'", "\\'")
                    
                    def discordMessage = """
                    {
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
                                },
                                {
                                    "name": "Pull Request URL",
                                    "value": "${safePrUrl}"
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
                    sh """
                        curl -X POST "${webhookUrl}" \\
                        -H "Content-Type: application/json" \\
                        -d @discord_payload.json
                    """
                    
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