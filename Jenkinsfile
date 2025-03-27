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
    }
}