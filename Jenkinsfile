pipeline {
    agent {
        docker {
            image 'csunity-ci:latest'
        }
    }
    environment {
        UNITY_CLOUD_API_KEY = credentials('UNITY_CLOUD_API_KEY')
    }
    stages {
        stage('Build') {
            steps {
                sh 'python3 build.py'
            }
        }
    }
}
