pipeline {
    agent {
        docker {
            image 'csunity-cli:latest'
        }
    }
    stages {
        stage('Build') {
            steps {
                sh 'python3 build.py'
            }
        }
    }
}
