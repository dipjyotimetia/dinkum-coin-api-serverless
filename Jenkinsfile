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
	stage("Tests") {
			agent { label 'dotnetcore' }
			steps {
				deleteDir()
				unstash "solution"

				sh 'dotnet run --project \"build/Build.csproj\" -target "Test" -NoDeps'
			}
		}
	stage("Package & Upload") {
			agent { label 'dotnetcore' }
			steps {
				deleteDir()
				unstash "solution"
				sh 'dotnet run --project \"build/Build.csproj\" -target "Package" -NoDeps'
				sh 'dotnet run --project \"build/Build.csproj\" -target "Upload" -NoDeps'
	
			}
		}	
    }
}