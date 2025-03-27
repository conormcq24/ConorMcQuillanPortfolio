pipeline {
    agent any
    triggers {
        githubPush()
        // Add scheduled triggers for midnight and 5am
        cron('30 18 * * *')
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
        // This stage determines what caused the build process to start
        stage('Detect Trigger Type') {
            steps {
                script {
                    def isCronTrigger = currentBuild.getBuildCauses().toString().contains('hudson.triggers.TimerTrigger')
                    env.IS_SCHEDULED_RUN = isCronTrigger.toString()
                    
                    if (isCronTrigger) {
                        echo "This is a scheduled run (cron trigger)"
                    } else {
                        echo "This is a webhook trigger"
                    }
                }
            }
        }
        
        // This stage only runs if IS_SCHEDULED_RUN is false
        // It detects if the trigger is a merge to test or main and sends the appropriate Discord message
        stage('Notify Discord of Trigger Based Build') {
            when {
                expression { return env.IS_SCHEDULED_RUN == 'false' }
            }
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
                    
                    // Discord webhook URL
                    def webhookUrl = "https://discordapp.com/api/webhooks/1354215301033759092/_88-RaCajTnr8dA4GJUUqIpQVJnbF2t3n9nq-2VBHbl-ugZw3l7m4_UJ3tZY1fL7GNW1"
                    def timestamp = new Date().format("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", TimeZone.getTimeZone('UTC'))
                    
                    // For webhook triggers, create the PR merge message
                    def environment = targetBranch == 'main' ? "production" : "test"
                    def color = targetBranch == 'test' ? '5793266' : '15158332'
                    
                    // Escape single quotes for shell command
                    def safeSourceBranch = sourceBranch.replace("'", "\\'")
                    def safeTargetBranch = targetBranch.replace("'", "\\'")
                    
                    def discordMessage = """
                    {
                        "username": "Jenkins",
                        "avatar_url": "https://www.jenkins.io/images/logos/jenkins/jenkins.png",
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
        
        // This stage only runs if IS_SCHEDULED_RUN is true
        // It sends a Discord message stating both prod and test are going through a scheduled redeploy
        stage('Notify Discord of Schedule Based Build') {
            when {
                expression { return env.IS_SCHEDULED_RUN == 'true' }
            }
            steps {
                script {
                    // Discord webhook URL
                    def webhookUrl = "https://discordapp.com/api/webhooks/1354215301033759092/_88-RaCajTnr8dA4GJUUqIpQVJnbF2t3n9nq-2VBHbl-ugZw3l7m4_UJ3tZY1fL7GNW1"
                    def timestamp = new Date().format("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", TimeZone.getTimeZone('UTC'))
                    
                    // For scheduled runs, create the nightly refresh message
                    def discordMessage = """
                    {
                        "username": "Jenkins",
                        "avatar_url": "https://www.jenkins.io/images/logos/jenkins/jenkins.png",
                        "embeds": [{
                            "title": "Scheduled Build",
                            "description": "Performing a scheduled refresh for production and test environment",
                            "color": 3447003,
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
                    
                    echo "Discord notification sent for scheduled run"
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