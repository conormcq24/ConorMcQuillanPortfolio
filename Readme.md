# Project Details

#### Local Environment Development: 

##### Requirements:
1. System Environment Variable DISCORD_PORTFOLIO_INBOX_URL
2. System Environment Variable GITHUB_REPO_ACCESS

both of thse can be provided by conor mcquillan upon request

##### Workflow:
1. clone repo
2. fetch and pull so that main and test are up to date
3. create a branch off of test 
4. when you are happy with changes merge to test
5. jenkins will automatically build and deploy your changes to the url
`www.conor-mcquillan-test.com`
6. if you like how it appears in test merge to main, jenkins will again build and deploy changes to the url
`www.conor-mcquillan.com`
