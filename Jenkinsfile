def buildVersion = ''

pipeline {
	agent none

	options { skipDefaultCheckout() }
	
	stages {
		stage("Build") {
			agent { label 'dotnetcore' }
			steps {
				deleteDir()
				checkout scm

				sh 'dotnet run --project \"build/Build.csproj\" -target "Export_Build_Version" -BuildVersionFilePath version.txt'
				
				script {
					echo "reading build version"
					buildVersion = readFile "${env.WORKSPACE}/version.txt"
					echo buildVersion
					currentBuild.displayName = buildVersion
				}
				
				sh 'dotnet run --project \"build/Build.csproj\" -target "Compile"'

				stash name: "solution", useDefaultExcludes: false
			}
		}
	stage("Test") {
			agent { label 'dotnetcore' }
			steps {
				deleteDir()
				unstash "solution"

				sh 'dotnet run --project \"build/Build.csproj\" -target "Test" -NoDeps'
			}
		}
	stage("Package & Upload") {
			agent { label 'dotnetcore' }

			environment { 
                AWS_ACCESS_KEY_ID = credentials('AWSAccessKey') 
				AWS_SECRET_ACCESS_KEY= credentials('AWSSecretKey') 
            }

			steps {
				deleteDir()
				unstash "solution"
				sh 'dotnet run --project \"build/Build.csproj\" -target "Package" -NoDeps'
				sh 'dotnet run --project \"build/Build.csproj\" -target "Upload" -NoDeps'
	
			}
		}
	stage("Deploy -> Dev") {
		agent { label 'dotnetcore' }

		environment { 
			AWS_ACCESS_KEY_ID = credentials('AWSAccessKey') 
			AWS_SECRET_ACCESS_KEY= credentials('AWSSecretKey') 
		}

		steps { 
			deploy "DevEnv", "dev", buildVersion, true 
			}

		}		
    }
}

void deploy(String awsAccountName, String environment, String versionToDeploy, Boolean cancelJobs = false) {
	
	deleteDir()
	unstash 'solution'
	echo 'deployment started'
			
	echo 'Running Nuke for deployment'
			
	sh "dotnet run --project \"build/Build.csproj\" -target \"Deploy\" -Account '${awsAccountName}' -Environment '${environment}' -VersionToDeploy '${versionToDeploy}'"
}


