def buildVersion = ''

pipeline {
	agent any

	options { skipDefaultCheckout() }
	
	environment { 
		AWS_ACCESS_KEY_ID = credentials('AWSAccessKey') 
		AWS_SECRET_ACCESS_KEY= credentials('AWSSecretKey') 
	}

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

				stash name: "solution", useDefaultExcludes: false

			}
		}
		stage("Package & Upload") {
			agent { label 'dotnetcore' }

			steps {

				sh 'dotnet run --project \"build/Build.csproj\" -target "Package" -NoDeps'
				sh 'dotnet run --project \"build/Build.csproj\" -target "Upload" -NoDeps'
	
			}
		}
		stage("Deploy > Dev") {
			agent { label 'dotnetcore' }


			steps { 
				deleteDir()
				unstash "solution"
				deploy "DevEnv", "dev", buildVersion, true 
			}

		}

		stage("Performance Test") {
//			agent {         
//				docker {
//					image 'stu-p/gatling'
				//	args '-u jenkins'
//				} 
//			}
			agent { label 'dockerhost' }

			steps { 
				deleteDir()
				unstash "solution"

//sh "cp ${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/user-files/simulations/* /opt/gatling/user-files/simulations/"
//				sh "/opt/gatling/bin/gatling.sh -s DinkumCoinSimulation -rf ${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/results"

			//sh "docker run --user=jenkins --rm -i -v ${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/user-files:/opt/gatling/user-files -v ${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/results:/opt/gatling/results stu-p/gatling -s DinkumCoinSimulation"	
  
  //working
   // sh "docker run --user=jenkins --entrypoint=gatling.sh --rm -i -v ${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/user-files:/opt/gatling/user-files --mount type=bind,src=${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/results,dst=/opt/gatling/results stu-p/gatling -s DinkumCoinSimulation"	


			script {
				docker.image('stu-p/gatling').withRun("-i --user=jenkins --entrypoint=/bin/sh -v ${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/user-files:/opt/gatling/user-files -v ${env.WORKSPACE}/test/DinkumCoin.Api.PerformanceTests/results:/opt/gatling/results ") { c -> 
					sh '/opt/gatling/bin/gatling.sh -s DinkumCoinSimulation'
					sh 'pwd'
					sh 'ls -la'

				}
			}
			stash name: "solution", useDefaultExcludes: false

			}

		}

		stage("Promote > UAT") {
			agent { label 'dotnetcore' }

		steps { 
				deleteDir()
				unstash "solution"
				echo 'Not implemented'
			}
		}			
	}
	post {
		always {
			deleteDir()
			unstash "solution" 
			   
			step([$class: 'XUnitBuilder',
			thresholds: [[$class: 'FailedThreshold', unstableThreshold: '1']],
			tools: [[ $class: 'XUnitDotNetTestType', pattern: '**/TestResults.xml']]])

			gatlingArchive()
			// cobertura autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/*Cobertura.coverageresults', conditionalCoverageTargets: '70, 0, 0', failUnhealthy: false, failUnstable: false, lineCoverageTargets: '80, 0, 0', maxNumberOfBuilds: 0, methodCoverageTargets: '80, 0, 0', onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false
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


