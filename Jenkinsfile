pipeline {
    agent {
        docker {
            image 'lyrositor/csunity-ci:latest'
        }
    }
    environment {
        UNITY_CLOUD_API_KEY = credentials('UNITY_CLOUD_API_KEY')
    }
    stages {
        stage('Build') {
            steps {
                sh 'python3 /csunity-ci/build.py'
            }
        }
    }
}
