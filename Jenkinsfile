/**
 * ApiService JenkinsFile for the CI and CD pipelines of Microservices and Monolith services.
 * ~~ MANAGED BY DEVOPS ~~
 */

//@Library('intellifloworkflow@IP-36352')
import org.intelliflo.*

def pipelineRuntime = new PipelineRuntime()
def artifactoryCredentialsId = 'a3c63f46-4be7-48cc-869b-4239a869cbe8'
def artifactoryUri = 'https://artifactory.intelliflo.io/artifactory'
def gitCredentialsId = '1327a29c-d426-4f3d-b54a-339b5629c041'
def gitCredentialsSSH = 'jenkinsgithub'
def jiraCredentialsId = '32546070-393c-4c45-afcd-8e8f1de1757b'
def globals = env
def windowsNode = 'windows'
def linuxNode = 'linux'

pipeline {

    agent none

    environment {
        githubRepoName = "${env.JOB_NAME.split('/')[1]}"
    }

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
                    pipelineRuntime = preparePipeline {
                        repoName = globals.githubRepoName
                        prNumber = globals.CHANGE_ID
                        baseBranch = globals.CHANGE_TARGET
                        branchName = globals.BRANCH_NAME
                        buildNumber = globals.BUILD_NUMBER
                        abortOnFailure = true
                        configFile = "Jenkinsfile-config.yml"
                    }

                    validateReferences {
                        runtime = pipelineRuntime
                    }

                    buildCode {
                        runtime = pipelineRuntime
                    }

                    pipelineRuntime = unitTest {
                        runtime = pipelineRuntime
                    }

                    runResharperInspectCode {
                        runtime = pipelineRuntime
                    }

                    analyseBuild {
                        runtime = pipelineRuntime
                    }

                    createPackages {
                        runtime = pipelineRuntime
                    }

                    vulnerabilityScan {
                        runtime = pipelineRuntime
                    }

                    pipelineRuntime = publishPackages {
                        runtime = pipelineRuntime
                        credentialsId = artifactoryCredentialsId
                        uri = artifactoryUri
                    }
                }
            }
            post {
                always {
                    script {
                        pipelineRuntime = addTimings {
                            runtime = pipelineRuntime
                        }

                        publishToSplunk {
                            runtime = pipelineRuntime
                        }

                        archiveArtifacts allowEmptyArchive: true, artifacts: 'dist/*.*', caseSensitive: false, excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5'
                        if (pipelineRuntime.config.componentStage == null || pipelineRuntime.config.componentStage.deleteWorkspace) {
                            deleteWorkspace {
                                force = true
                            }
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
                    pipelineRuntime.currentStage = 'SubSystem'

                    prepareSubSystemStage {
                        runtime = pipelineRuntime
                        credentialsId = artifactoryCredentialsId
                        uri = artifactoryUri
                    }

                    if (!pipelineRuntime.config.subSystemStage.bypass) {
                        deployToSubSystem {
                            runtime = pipelineRuntime
                        }

                        pipelineRuntime = subSystemTest {
                            runtime = pipelineRuntime
                            delegate.artifactoryUri = artifactoryUri
                        }

                        analyseSubSystem {
                            runtime = pipelineRuntime
                            gitCredentials = gitCredentialsSSH
                        }
                    } else {
                        echo "[INFO] Bypassing ${pipelineRuntime.currentStage} Stage"
                    }

                    // Promote PR packages
                    promotePackages {
                        runtime = pipelineRuntime
                        credentialsId = artifactoryCredentialsId
                        uri = artifactoryUri
                    }

                    // Publish branch packages
                    pipelineRuntime = publishPackages {
                        runtime = pipelineRuntime
                        credentialsId = artifactoryCredentialsId
                        uri = artifactoryUri
                    }

                    if (pipelineRuntime.changeset.branch != null) {
                        addDeployLink {
                            packageName = pipelineRuntime.changeset.repoName
                            delegate.packageVersion = pipelineRuntime.packageVersion
                        }
                    }
                }
            }
            post {
                always {
                    script {
                        pipelineRuntime = addTimings {
                            runtime = pipelineRuntime
                        }

                        cleanUpSubSystem {
                            runtime = pipelineRuntime
                        }

                        if (!pipelineRuntime.config.subSystemStage.bypass) {
                            publishToSplunk {
                                runtime = pipelineRuntime
                            }
                        }

                        archiveArtifacts allowEmptyArchive: true, artifacts: 'dist/*.*', caseSensitive: false, excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5'
                        if (pipelineRuntime.config.subSystemStage.deleteWorkspace) {
                            deleteWorkspace {
                                force = true
                            }
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
                    pipelineRuntime.currentStage = 'System'

                    abortOlderBuilds {
                        logVerbose = pipelineRuntime.config.logVerbose
                    }

                    if (!pipelineRuntime.config.systemStage.bypass) {

                        def testingRequired = pauseForInput {
                            message = 'SIT testing required?'
                            okButtonText = 'Yes'
                            stageName = pipelineRuntime.currentStage
                            logVerbose = pipelineRuntime.config.logVerbose
                        }

                        if (testingRequired) {
                            deployToEnvironment {
                                runtime = pipelineRuntime
                                delegate.artifactoryUri = artifactoryUri
                                gitCredentials = gitCredentialsSSH
                            }
                        }

                        def sitTestingSuccessful = pauseForInput {
                            message = 'Manual SIT testing successful?'
                            okButtonText = 'Yes'
                            stageName = pipelineRuntime.currentStage
                            logVerbose = pipelineRuntime.config.logVerbose
                        }

                        if (sitTestingSuccessful == false) {
                            currentBuild.result = 'ABORTED'
                            error "SIT Testing unsuccessful"
                        }
                    } else {
                        echo "[INFO] Bypassing ${pipelineRuntime.currentStage} Stage"
                    }

                    node(windowsNode) {
                        unstashResourceFiles {
                            folder = 'pipeline'
                            stashName = 'ResourceFiles'
                        }

                        promotePackages {
                            runtime = pipelineRuntime
                            credentialsId = artifactoryCredentialsId
                            uri = artifactoryUri
                        }

                        deleteDir()
                    }
                }
            }
            post {
                always {
                    script {
                        pipelineRuntime = addTimings {
                            runtime = pipelineRuntime
                        }

                        if (!pipelineRuntime.config.systemStage.bypass) {

                            node(windowsNode) {
                                bat "if not exist dist\\NUL (mkdir dist)"

                                unstashResourceFiles {
                                    folder = 'pipeline'
                                    stashName = 'ResourceFiles'
                                }

                                publishToSplunk {
                                    runtime = pipelineRuntime
                                }

                                archiveArtifacts allowEmptyArchive: true, artifacts: 'dist/*.*', caseSensitive: false, excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5'
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
                    pipelineRuntime.currentStage = 'Production'

                    abortOlderBuilds {
                        logVerbose = pipelineRuntime.config.logVerbose
                    }

                    validateProductionStage {
                        runtime = pipelineRuntime
                    }

                    deployToProduction {
                        runtime = pipelineRuntime
                        delegate.artifactoryUri = artifactoryUri
                        gitCredentials = gitCredentialsSSH
                    }

                    def prdTestingSuccessful = pauseForInput {
                        withTimeout = {
                            time = pipelineRuntime.config.prdTestingSuccessfulTimeout
                            unit = 'HOURS'
                        }
                        message = 'Has manual PRD testing been successful?'
                        okButtonText = 'Yes'
                        stageName = pipelineRuntime.currentStage
                        logVerbose = pipelineRuntime.config.logVerbose
                    }

                    if (prdTestingSuccessful == false) {
                        currentBuild.result = 'ABORTED'
                        error "PRD Testing unsuccessful"
                    }

                    verifyPackages {
                        runtime = pipelineRuntime
                        uri = artifactoryUri
                    }

                    node(windowsNode) {

                        mergeChangeset {
                            runtime = pipelineRuntime
                            credentialsId = gitCredentialsId
                        }

                        unstashResourceFiles {
                            folder = 'pipeline'
                            stashName = 'ResourceFiles'
                        }

                        promotePackages {
                            runtime = pipelineRuntime
                            credentialsId = artifactoryCredentialsId
                            uri = artifactoryUri
                        }

                        updateJiraOnMerge {
                            runtime = pipelineRuntime
                            credentialsId = jiraCredentialsId
                        }

                        deleteDir()
                    }

                    vulnerabilityScan {
                        runtime = pipelineRuntime
                    }

                    cleanUpProduction {
                        runtime = pipelineRuntime
                        credentialsId = artifactoryCredentialsId
                        uri = artifactoryUri
                    }
                }
            }
            post {
                always {
                    script {
                        pipelineRuntime = addTimings {
                            runtime = pipelineRuntime
                        }

                        releaseProductionStage {
                            runtime = pipelineRuntime
                        }

                        node(windowsNode) {
                            bat "if not exist dist\\NUL (mkdir dist)"

                            unstashResourceFiles {
                                folder = 'pipeline'
                                stashName = 'ResourceFiles'
                            }

                            publishToSplunk {
                                runtime = pipelineRuntime
                            }

                            archiveArtifacts allowEmptyArchive: true, artifacts: 'dist/*.*', caseSensitive: false, excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5'
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
                reportBuildStatus {
                    runtime = pipelineRuntime
                }
            }
        }
    }
}
