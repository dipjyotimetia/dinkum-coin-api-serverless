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

				sh 'dotnet run --project \"%WORKSPACE%/build/Build.csproj\" -target "Export_Build_Version" -BuildVersionFilePath \"%WORKSPACE%/version.txt\"'
				
				script {
					echo "reading build version"
					buildVersion = readFile "${env.WORKSPACE}/version.txt"
					echo buildVersion
					currentBuild.displayName = buildVersion
				}
				
				sh 'dotnet run --project \"%WORKSPACE%/Build/Build.csproj\" -target "Compile"'

				stash name: "solution", useDefaultExcludes: false
			}
		}
    }
}