pipeline {
	agent any
	stages {
		stage('Setup') {
			steps {
				echo 'Starting build...'
			}
		}
		stage('Test') {
			steps {
				echo 'Testing..'
				
				dir('BBBSTests') {
					sh 'dotnet add package coverlet.collector'
					sh 'dotnet add package coverlet.msbuild'
					sh "dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:ExcludeByFile='**/*DBModels/*.cs'"
				}
			}			
			post {
				success {
					archiveArtifacts 'BBBSTests/coverage.cobertura.xml'
					publishCoverage adapters: [istanbulCoberturaAdapter(path: 'BBBSTests/coverage.cobertura.xml', thresholds: [
						[failUnhealthy: true, thresholdTarget: 'Conditional', unhealthyThreshold: 80.0, unstableThreshold: 15.0]
					])], checksName: '', sourceFileResolver: sourceFiles('NEVER_STORE')
				}
			}
		}
		stage('Build') {
			steps {
				// The Dockerfile will build an image with a base ASP.NET image, and bundle, build, and publish the backend before running it.
				// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
				// Build docker images with no cache option (no clutter, full rebuild), using dockerfile located in 'BBBSBackend/', and set name of image to 'bbbsbackend'
				sh "docker build --no-cache -f BBBSBackend/Dockerfile -t bbbsbackend ."
			}
		}
		stage('Deploy') {
			steps {
				// Kill previous container
				catchError(buildResult: 'SUCCESS', stageResult: 'SUCCESS') {
					sh "docker kill \$(docker ps --format '{{.ID}} {{.Ports}}' | grep '0.0.0.0:3000->' | cut -d ' ' -f1)"	
				}
				// Run the image and remove container after completion, expose host port 80 -> container port 3000, detached (execution can continue), image name
				sh "docker run --rm -p 3000:3000 -d bbbsbackend"
			}
		}
		stage('Clean') {
			steps {
				// Purge all unused images from docker
				sh "docker image prune -a -f"
			}
		}
	}
}
