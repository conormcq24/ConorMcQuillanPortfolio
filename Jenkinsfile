pipeline {
    agent any
    
    triggers {
        githubPush()
    }
    
    stages {
        stage('Check Webhook') {
            steps {
                script {
                    echo "Jenkins job triggered!"
                    echo "Environment Variables:"
                    sh 'env | grep -i github || echo "No GitHub environment variables found"'
                    
                    // Save all environment variables to a file for inspection
                    sh 'env > env_vars.txt'
                    
                    echo "Webhook received and processed."
                }
            }
        }
    }
}