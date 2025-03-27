pipeline {
	agent any
	triggers {
		githubPush()
	}
	parameters {
		string(name: 'BRANCH NAME', defaultValue: '', description: 'Name of branch that had pull request completed')
		string(name: 'PR_URL', defaultValue: '', description: 'Url of the completed pull request')
	}
	stages {
        stage('Receiving Pull Request') {
            steps {
                script {
                    // Parse the GitHub webhook payload to determine if this is a PR closure event
                    def payload = readJSON file: env.GITHUB_WEBHOOK_PAYLOAD ?: '{}'
                    
                    // Check if this is a pull request event and if the action is "closed" and if it was merged
                    if (payload.pull_request && payload.action == 'closed' && payload.pull_request.merged == true) {
                        // Extract PR information
                        env.PR_TITLE = payload.pull_request.title
                        env.PR_URL = payload.pull_request.html_url
                        env.PR_AUTHOR = payload.pull_request.user.login
                        env.BRANCH_NAME = payload.pull_request.base.ref
                        
                        echo "Pull request closed and merged to ${env.BRANCH_NAME}"
                        
                        // Check if the PR was merged to one of our monitored branches
                        if (env.BRANCH_NAME == 'test' || env.BRANCH_NAME == 'main') {
                            echo "This PR was merged to a monitored branch: ${env.BRANCH_NAME}"
                        } else {
                            echo "This PR was not merged to a monitored branch. Skipping notification."
                            currentBuild.result = 'SUCCESS'
                            return
                        }
                    } else {
                        echo "This is not a PR closure event. Skipping."
                        currentBuild.result = 'SUCCESS'
                        return
                    }
                }
            }
        }
        stage('Send Discord Message') {
            when {
                expression { return env.BRANCH_NAME in ['test', 'production'] }
            }
            steps {
                script {
                    // Determine which webhook to use based on the branch
                    def webhookUrl = "https://discordapp.com/api/webhooks/1354215301033759092/_88-RaCajTnr8dA4GJUUqIpQVJnbF2t3n9nq-2VBHbl-ugZw3l7m4_UJ3tZY1fL7GNW1"
                    
                    // Create the discord message payload
                    def discordMessage = """{
                        "embeds": [{
                            "title": "Pull Request Completed",
                            "description": "A pull request has been successfully merged to the **${env.BRANCH_NAME}** branch",
                            "color": ${env.BRANCH_NAME == 'test' ? '5793266' : '15158332'},
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
                            "timestamp": "${new Date().format("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", TimeZone.getTimeZone('UTC'))}"
                        }]
                    }"""
                    
                    // Send the webhook notification
                    sh """
                        curl -X POST "${webhookUrl}" \
                        -H "Content-Type: application/json" \
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