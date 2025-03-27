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
                    
                    // Try to extract source branch from merge commit message
                    // Typical format: "Merge pull request #X from source/branch"
                    if (commitMsg.contains("Merge pull request")) {
                        def matcher = commitMsg =~ /from\s+(\S+)/
                        if (matcher.find()) {
                            sourceBranch = matcher.group(1).tokenize('/').last()
                        }
                    }
                    
                    // Extract PR URL if possible
                    def prUrl = "Not available"
                    def prMatcher = commitMsg =~ /#(\d+)/
                    if (prMatcher.find()) {
                        def prNumber = prMatcher.group(1)
                        def repoUrl = sh(script: 'git config --get remote.origin.url', returnStdout: true).trim()
                        // Convert SSH URL to HTTPS if needed
                        repoUrl = repoUrl.replaceAll(/git@github.com:/, 'https://github.com/')
                        repoUrl = repoUrl.replaceAll(/\.git$/, '')
                        prUrl = "${repoUrl}/pull/${prNumber}"
                    }
                    
                    // Store the variables for later use
                    env.TARGET_BRANCH = targetBranch
                    env.SOURCE_BRANCH = sourceBranch
                    env.PR_URL = prUrl
                    
                    // Only proceed for monitored branches
                    if (targetBranch in ['test', 'main']) {
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
                expression { return env.TARGET_BRANCH in ['test', 'main'] }
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
                    
                    def discordMessage = """
                    {
                        "embeds": [{
                            "title": "Build Process Started",
                            "description": "A pull request from **${env.SOURCE_BRANCH}** branch to **${env.TARGET_BRANCH}** branch has started a build process for ${environment} environment",
                            "color": ${color},
                            "fields": [
                                {
                                    "name": "Source Branch",
                                    "value": "${env.SOURCE_BRANCH}"
                                },
                                {
                                    "name": "Target Branch",
                                    "value": "${env.TARGET_BRANCH}"
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
                    sh """
                        curl -X POST "${webhookUrl}" \\
                        -H "Content-Type: application/json" \\
                        -d '${discordMessage}'
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