/**
 * ApiService JenkinsFile for the CI and CD pipelines of Microservices and Monolith services.
 * ~~ MANAGED BY DEVOPS ~~
 */

/**
 * By default the master branch of the library is loaded
 * Use the include directive below ONLY if you need to load a branch of the library
 * @Library('intellifloworkflow@IP-22228')
 */
import org.intelliflo.*

def changesetJson = new String()
def changeset = new Changeset()
def amazon = new Amazon()

def artifactoryCredentialsId = 'a3c63f46-4be7-48cc-869b-4239a869cbe8'
def artifactoryUri = 'https://artifactory.intelliflo.io/artifactory'
def ec2TemplateUrl = 'https://s3-eu-west-1.amazonaws.com/devops-aws/templates/ec2.subsys.vpc.microservice.template'
def codedeployTemplateUrl = 'https://s3-eu-west-1.amazonaws.com/devops-aws/templates/codedeploy.generic.template'
def gitCredentialsId = '1327a29c-d426-4f3d-b54a-339b5629c041'
def gitCredentialsSSH = 'jenkinsgithub'
def jiraCredentialsId = '32546070-393c-4c45-afcd-8e8f1de1757b'
def globals = env
def githubRepoName = "${env.JOB_NAME.split('/')[1]}"
def solutionName = "${env.JOB_NAME.split('/')[1].replace('Clone.', '')}"

def stageName
def semanticVersion
def packageVersion
def packageMd5
def stackName
def verboseLogging = false
def noSubsystemStages = ['Microservice.Scheduler']
def windowsNode = 'windows'

// ############################################################################
// DEBUG PURPOSES ONLY
// Use these bypass switches ONLY if testing changes further on in the pipeline
def bypassSubsystemStage = env.JOB_NAME.split('/')[1] in noSubsystemStages
def bypassSystemStage = false
// ############################################################################

pipeline {

    agent none

    options {
        timestamps()
        skipDefaultCheckout()
    }

    stages {
        stage('Master') {
            when {
                branch 'master'
            }
            steps {
                script {
                    triggerRepoScan {
                        credentialsId = gitCredentialsId
                    }
                }
            }
        }

        stage('Initialise') {
            agent none
            when {
                expression { env.BRANCH_NAME ==~ /^(IP-|PR-).*/ }
            }
            steps {
                script {
                    stashResourceFiles {
                        targetPath = 'org/intelliflo'
                        masterNode = 'master'
                        stashName = 'ResourceFiles'
                        resourcePath = "@libs/intellifloworkflow/resources"
                    }

                    abortOlderBuilds {
                        logVerbose = verboseLogging
                    }
                }
            }
        }

        stage('Component') {
            agent {
                label windowsNode
            }
            when {
                expression { env.BRANCH_NAME ==~ /^(IP-|PR-).*/ }
            }
            steps {
                bat 'set'

                script {
                    stageName = 'Component'

                    // Analyse and validate the changeset
                    validateChangeset {
                        repoName = githubRepoName
                        prNumber = globals.CHANGE_ID
                        baseBranch = globals.CHANGE_TARGET
                        branchName = globals.BRANCH_NAME
                        buildNumber = globals.BUILD_NUMBER
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                        abortOnFailure = true
                    }
                    changesetJson = (String)Consul.getStoreValue(ConsulKey.get(githubRepoName, globals.BRANCH_NAME, globals.CHANGE_ID, 'changeset'))
                    changeset = changeset.fromJson(changesetJson)

                    if (changeset.pullRequest != null) {
                        syncJiraLabel {
                            repoName = changeset.repoName
                            prNumber = changeset.prNumber
                            jiraTicket = changeset.jiraTicket
                            label = 'externalfeed'
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }
                    }

                    // Checkout the code and unstash supporting scripts
                    checkoutCode {
                        delegate.stageName = stageName
                    }

                    // Scripts required by the pipeline
                    unstashResourceFiles {
                        folder = 'pipeline'
                        stashName = 'ResourceFiles'
                    }

                    // Versioning
                    calculateVersion {
                        buildNumber = changeset.buildNumber
                        delegate.changeset = changeset
                        delegate.stageName = stageName
                        abortOnFailure = true
                    }

                    semanticVersion = Consul.getStoreValue(ConsulKey.get(githubRepoName, changeset.branchName, changeset.prNumber, 'existing.version'))
                    packageVersion = "${semanticVersion}.${changeset.buildNumber}"
                    if (changeset.pullRequest != null) {
                        currentBuild.displayName = "${githubRepoName}.Pr${changeset.prNumber}(${packageVersion})"
                    } else {
                        currentBuild.displayName = "${githubRepoName}(${packageVersion})"
                    }
                    stackName = amazon.getStackName(githubRepoName, packageVersion, false, false)

                    validateReferencePackageVersion {
                        repoName = githubRepoName
                        packagesConfigPath = "src/${githubRepoName}/packages.config"
                        ref = changeset.commitSha
                        referencePackages = ['IntelliFlo.Platform', 'Intelliflo.Platform.QueryLang']
                    }

                    startSonarQubeAnalysis {
                        repoName = githubRepoName
                        delegate.solutionName = solutionName
                        version = semanticVersion
                        branchName = changeset.originatingBranch
                        unitTestResults = "UnitTestResults"
                        coverageResults = "OpenCoverResults"
                        inspectCodeResults = "ResharperInspectCodeResults"
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    createVersionTargetsFile {
                        serviceName = solutionName
                        version = packageVersion
                        sha = changeset.commitSha
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    buildSolution {
                        solutionFile = "${solutionName}.sln"
                        configuration = 'Release'
                        targetFramework = 'v4.5.2'
                        includeSubsystemTests = true
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    scanWithWhiteSource {
                        serviceName = githubRepoName
                        libIncludePath = "src/${githubRepoName}/bin/**/*.dll"
                        semver = semanticVersion
                        version = packageVersion
                        jiraTicket = changeset.jiraTicket
                        delegate.stageName = stageName
                        isPr = changeset.pullRequest != null
                    }

                    runDependencyCheck {
                        repoName = "${solutionName}"
                        binariesLocation = "src\\${solutionName}\\bin\\Release"
                        delegate.stageName = stageName
                    }

                    stashSubSystemTests {
                        delegate.solutionName = solutionName
                        stashName = 'SubSystemTests'
                        delegate.stageName = stageName
                        logVerbose = verboseLogging
                    }

                    def unitTestResults = runUnitTests {
                        title = "Unit Tests"
                        withCoverage = true
                        include = "**/test/${solutionName}.Tests/bin/Release/**/${solutionName}.Tests.dll"
                        unitTestsResultsFilename = "UnitTestResults"
                        coverageInclude = solutionName
                        coverageResultsFilename = "OpenCoverResults"
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    runResharperInspectCode {
                        repoName = githubRepoName
                        delegate.solutionName = solutionName
                        resultsFile = "ResharperInspectCodeResults"
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    completeSonarQubeAnalysis {
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    analyseTestResults {
                        title = "Unit Tests"
                        testResults = unitTestResults
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    createNugetPackages {
                        createSubsysJsonFile = true
                        serviceName = changeset.serviceName
                        updateModConfigJsonFile = true
                        stack = stackName
                        version = packageVersion
                        artifactFolder = 'dist'
                        stashPackages = true
                        stashName = 'Packages'
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    findAndDeleteOldPackages {
                        credentialsId = artifactoryCredentialsId
                        packageName = "${changeset.repoName}.${semanticVersion}"
                        latestBuildNumber = changeset.buildNumber
                        url = artifactoryUri
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    if (changeset.pullRequest != null) {
                        publishPackages {
                            credentialsId = artifactoryCredentialsId
                            repo = 'nuget-snapshot'
                            version = packageVersion
                            include = "*.nupkg"
                            uri = artifactoryUri
                            properties = "github.pr.number=${changeset.prNumber} git.repo.name=${changeset.repoName} git.master.mergebase=${changeset.masterSha} jira.ticket=${changeset.jiraTicket}"
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        packageMd5 = getMd5Sum {
                            repoName = githubRepoName
                            version = packageVersion
                        }
                    }
                }
            }
            post {
                always {
                    script {
                        if (changeset.pullRequest != null) {
                            publishToSplunk {
                                stage = stageName
                                repoName = changeset.repoName
                                prNumber = changeset.prNumber
                                version = packageVersion
                                outputToFile = true
                                outputToLog = false
                                consulKey = changeset.consulBuildKey
                                logVerbose = verboseLogging
                                delegate.stageName = stageName
                            }
                        }
                        archive excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5', includes: 'dist/*.*'
                        deleteWorkspace {
                            force = true
                        }
                    }
                }
            }
        }

        stage('SubSystem') {
            agent {
                label windowsNode
            }
            when {
                expression { env.BRANCH_NAME ==~ /^(IP-|PR-).*/ }
            }
            steps {
                script {
                    stageName = 'SubSystem'
                    if (!bypassSubsystemStage) {

                        prepareSubSystemStage {
                            delegate.solutionName = solutionName
                            subsystemTestsStashName = 'SubSystemTests'
                            resourceFilesFolder = 'pipeline'
                            resourceFilesStashName = 'ResourceFiles'
                            artifactFolder = 'dist'
                            packagesStashName = 'Packages'
                            delegate.stageName = stageName
                        }

                        prepareCodeDeployPackages {
                            isMicroservice = changeset.isMicroservice
                            bucket = "codeartefacts"
                            filter = "${changeset.repoName}.*.nupkg"
                            artifactFolder = 'dist'
                            credentials = artifactoryCredentialsId
                            delegate.artifactoryUri = artifactoryUri
                            consulBuildKey = changeset.consulBuildKey
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        createEc2Stack {
                            stack = stackName
                            templateUrl = ec2TemplateUrl
                            consulKey = changeset.consulBuildKey
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        labelStackResources {
                            repoName = changeset.repoName
                            version = packageVersion
                            consulKey = changeset.consulBuildKey
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        deployToAws {
                            packageName = changeset.repoName
                            version = packageVersion
                            isMicroservice = changeset.isMicroservice
                            instance = "microservice"
                            serviceAction = "start"
                            templateUrl = codedeployTemplateUrl
                            consulKey = changeset.consulBuildKey
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        checkServiceHealth {
                            serverDns = Consul.getStoreValue("${changeset.consulBuildKey}/MicroserviceAddress")
                            maxAttempts = 12
                            sleepIncrement = 10
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        if (changeset.pullRequest != null) {
                            verifyPackageExists {
                                packageName = changeset.repoName
                                version = packageVersion
                                uri = artifactoryUri
                                repo = "nuget-snapshot"
                                logVerbose = verboseLogging
                                delegate.stageName = stageName
                            }
                        }

                        prepareSubSystemTestConfigFile {
                            delegate.solutionName = solutionName
                            configuration = 'Debug'
                            stack = stackName
                            consulKey = changeset.consulBuildKey
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        def subsystemTestResults = runUnitTests {
                            title = "SubSystem Tests"
                            withCoverage = false
                            include = "**/test/${solutionName}.SubSystemTests/bin/Debug/**/${solutionName}.SubSystemTests.dll"
                            unitTestsResultsFilename = "SubSystemTestResults"
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        analyseTestResults {
                            title = "SubSystem Tests"
                            testResults = subsystemTestResults
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        measureSubSystemCoverage {
                            repoName = changeset.repoName
                            delegate.solutionName = solutionName
                            serviceName = changeset.serviceName
                            consulKey = changeset.consulBuildKey
                            artifactFolder = 'dist'
                            warnIfProdUrlNotAvailable = true
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        validatePublicSwagger {
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                            consulKey = changeset.consulBuildKey
                            serviceName = changeset.serviceName
                            gitCredentials = gitCredentialsSSH
                            outputFolder = "${pwd()}\\dist"
                        }
                    } else {
                        echo "[DEBUG] Bypassing ${stageName} Stage"
                    }

                    findAndDeleteOldPackages {
                        credentialsId = artifactoryCredentialsId
                        packageName = "${changeset.repoName}.${semanticVersion}"
                        latestBuildNumber = changeset.buildNumber
                        url = artifactoryUri
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    if (changeset.pullRequest != null) {
                        promotePackage {
                            packageName = changeset.repoName
                            version = packageVersion
                            packageMasterSha = changeset.masterSha
                            sourceRepo = 'nuget-snapshot'
                            destinationRepo = 'nuget-ready4test'
                            credentialsId = artifactoryCredentialsId
                            url = artifactoryUri
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }
                    }

                    if (changeset.branch != null) {
                        publishPackages {
                            credentialsId = artifactoryCredentialsId
                            repo = 'nuget-dev-snapshot'
                            version = packageVersion
                            include = "*.nupkg"
                            uri = artifactoryUri
                            properties = "git.branch.name=${changeset.branchName} git.repo.name=${changeset.repoName} git.master.mergebase=${changeset.masterSha} jira.ticket=${changeset.jiraTicket}"
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        addDeployLink {
                            packageName = changeset.repoName
                            delegate.packageVersion = packageVersion
                        }
                    }
                }
            }
            post {
                always {
                    script {
                        if (!bypassSubsystemStage) {
                            addSubSystemLogsToArtifacts {
                                stack = stackName
                                consulKey = changeset.consulBuildKey
                                artifactFolder = 'dist'
                                logVerbose = verboseLogging
                                delegate.stageName = stageName
                            }

                            removeAwsStacks {
                                repoName = changeset.repoName
                                version = packageVersion
                                isMicroservice = changeset.isMicroservice
                                logVerbose = verboseLogging
                                delegate.stageName = stageName
                            }

                            deleteS3Package {
                                bucketName = 'codeartefacts'
                                name = changeset.repoName
                                version = packageVersion
                                logVerbose = verboseLogging
                                delegate.stageName = stageName
                            }

                            publishToSplunk {
                                stage = stageName
                                repoName = changeset.repoName
                                prNumber = changeset.prNumber
                                version = packageVersion
                                outputToFile = true
                                outputToLog = false
                                consulKey = changeset.consulBuildKey
                                logVerbose = verboseLogging
                                delegate.stageName = stageName
                            }
                        }
                        archive excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5', includes: 'dist/*.*'
                        deleteWorkspace {
                            force = true
                        }
                    }
                }
            }
        }

        stage('System') {
            agent none
            when {
                expression { env.BRANCH_NAME ==~ /^PR-.*/ }
            }
            steps {
                script {
                    stageName = 'System'
                    if (!bypassSystemStage) {
                        def inputResult = pauseForInput {
                            delegate.stageName = stageName
                            message = 'SIT testing required?'
                            okButtonText = 'Yes'
                            logVerbose = verboseLogging
                        }

                        if (inputResult) {
                            deployToEnvironment {
                                delegate.stageName = stageName
                                repoName = changeset.repoName
                                delegate.solutionName = solutionName
                                serviceName = changeset.serviceName
                                prNumber = changeset.prNumber
                                delegate.packageVersion = packageVersion
                                targetRepo = "nuget-ready4test"
                                delegate.artifactoryUri = artifactoryUri
                                packageMasterSha = changeset.masterSha
                                playbook = "api-service"
                                deploySlaveLabel = 'deploy'
                                deployScriptsBranchName = 'master'
                                gitCredentials = gitCredentialsSSH
                                logVerbose = verboseLogging
                                packageMd5Checksum = packageMd5
                            }
                        }

                        inputResult = pauseForInput {
                            delegate.stageName = stageName
                            message = 'Manual SIT testing successful?'
                            okButtonText = 'Yes'
                            logVerbose = verboseLogging
                        }

                        if (inputResult == false) {
                            currentBuild.result = 'ABORTED'
                            error "SIT Testing unsuccessful"
                        }
                    } else {
                        echo "[DEBUG] Bypassing ${stageName} Stage"
                    }

                    node(windowsNode) {
                        unstashResourceFiles {
                            folder = 'pipeline'
                            stashName = 'ResourceFiles'
                        }

                        findAndDeleteOldPackages {
                            credentialsId = artifactoryCredentialsId
                            packageName = "${changeset.repoName}.${semanticVersion}"
                            latestBuildNumber = changeset.buildNumber
                            url = artifactoryUri
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        promotePackage {
                            packageName = changeset.repoName
                            version = packageVersion
                            packageMasterSha = changeset.masterSha
                            sourceRepo = 'nuget-ready4test'
                            destinationRepo = 'nuget-ready4prd'
                            credentialsId = artifactoryCredentialsId
                            url = artifactoryUri
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        deleteDir()
                    }
                }
            }
            post {
                always {
                    script {
                        if (!bypassSystemStage) {
                            node(windowsNode) {
                                bat "if not exist dist\\NUL (mkdir dist)"

                                unstashResourceFiles {
                                    folder = 'pipeline'
                                    stashName = 'ResourceFiles'
                                }

                                publishToSplunk {
                                    stage = stageName
                                    repoName = changeset.repoName
                                    prNumber = changeset.prNumber
                                    version = packageVersion
                                    outputToFile = true
                                    outputToLog = false
                                    consulKey = changeset.consulBuildKey
                                    logVerbose = verboseLogging
                                    delegate.stageName = stageName
                                }

                                archive excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5', includes: 'dist/*.*'
                                deleteDir()
                            }
                        }
                    }
                }
            }
        }

        stage('Production') {
            agent none
            when {
                expression { env.BRANCH_NAME ==~ /^PR-.*/ }
            }
            steps {
                script {
                    stageName = 'Production'

                    validateCodeReviews {
                        repoName = githubRepoName
                        prNumber = changeset.prNumber
                        author = changeset.author
                        failBuild = false
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    validateJiraTicket {
                        delegate.changeset = changeset
                        failBuild = false
                        delegate.stageName = stageName
                        logVerbose = verboseLogging
                    }

                    deployToProduction {
                        delegate.stageName = stageName
                        repoName = changeset.repoName
                        delegate.solutionName = solutionName
                        serviceName = changeset.serviceName
                        prNumber = changeset.prNumber
                        delegate.packageVersion = packageVersion
                        targetRepo = "nuget-ready4prd"
                        delegate.artifactoryUri = artifactoryUri
                        packageMasterSha = changeset.masterSha
                        playbook = "api-service"
                        deploySlaveLabel = 'deploy'
                        deployScriptsBranchName = 'master'
                        gitCredentials = gitCredentialsSSH
                        logVerbose = verboseLogging
                        packageMd5Checksum = packageMd5
                    }

                    def inputResult = pauseForInput {
                        withTimeout = {
                            time = 3
                            unit = 'HOURS'
                        }
                        delegate.stageName = stageName
                        message = 'Has manual PRD testing been successful?'
                        okButtonText = 'Yes'
                        logVerbose = verboseLogging
                    }

                    if (inputResult == false) {
                        currentBuild.result = 'ABORTED'
                        error "PRD Testing unsuccessful"
                    }

                    verifyPackageExists {
                        packageName = changeset.repoName
                        version = packageVersion
                        uri = artifactoryUri
                        repo = 'nuget-ready4prd'
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    validateMasterSha {
                        repoName = changeset.repoName
                        packageMasterSha = changeset.masterSha
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    node(windowsNode) {
                        mergePullRequest {
                            repoName = changeset.repoName
                            prNumber = changeset.prNumber
                            masterSha = changeset.masterSha
                            sha = changeset.commitSha
                            consulKey = changeset.consulBaseKey
                            credentialsId = gitCredentialsId
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        updateMasterVersion {
                            repoName = changeset.repoName
                            version = semanticVersion
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        tagCommit {
                            repoName = changeset.repoName
                            version = semanticVersion
                            author = changeset.author
                            email = changeset.commitInfo.author.email
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        unstashResourceFiles {
                            folder = 'pipeline'
                            stashName = 'ResourceFiles'
                        }

                        promotePackage {
                            packageName = changeset.repoName
                            version = packageVersion
                            packageMasterSha = changeset.masterSha
                            sourceRepo = 'nuget-ready4prd'
                            destinationRepo = 'nuget-prd'
                            force = true
                            credentialsId = artifactoryCredentialsId
                            url = artifactoryUri
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        updateJiraOnMerge {
                            issueKey = changeset.jiraTicket
                            packageName = changeset.repoName
                            version = packageVersion
                            credentialsId = jiraCredentialsId
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        deleteDir()
                    }

                    node('linux') {
                        scanPackageWithWhiteSource {
                            cleanTestOrganization = true
                            serviceName = githubRepoName
                            libIncludePath = 'content/microservice/**/*.dll'
                            semver = semanticVersion
                            repoName = 'nuget-prd'
                            delegate.packageVersion = packageVersion
                            packageExtension = 'nupkg'
                            logVerbose = verboseLogging
                        }

                        deleteDir()
                    }

                    updateWatermarks {
                        repoName = changeset.repoName
                        consulBuildKey = changeset.consulBuildKey
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    cleanupConsul {
                        repoName = changeset.repoName
                        prNumber = changeset.prNumber
                        consulBuildKey = changeset.consulBuildKey
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    deleteGithubBranch {
                        repoName = changeset.repoName
                        branchName = changeset.originatingBranch
                        logVerbose = verboseLogging
                    }
                }
            }
            post {
                always {
                    script {
                        releaseProductionStage {
                            repoName = changeset.repoName
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        node(windowsNode) {
                            bat "if not exist dist\\NUL (mkdir dist)"

                            unstashResourceFiles {
                                folder = 'pipeline'
                                stashName = 'ResourceFiles'
                            }

                            publishToSplunk {
                                stage = stageName
                                repoName = changeset.repoName
                                prNumber = changeset.prNumber
                                version = packageVersion
                                outputToFile = true
                                outputToLog = false
                                consulKey = changeset.consulBuildKey
                                logVerbose = verboseLogging
                                delegate.stageName = stageName
                            }

                            archive excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5', includes: 'dist/*.*'
                            deleteDir()
                        }
                    }
                }
            }
        }
    }
    post {
        always {
            script {
                reportBuildStatusToSlack {
                    delegate.changesetJson = changesetJson
                }
            }
        }
    }
}
