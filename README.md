# Congestion Monitor

This repo is the Congestion Monitor project to detect congestion situations through web cameras in real-time.

In 2020, I realized that I have created a demo for my customer in 2019 that the application is to detect faces and to show the number and the trend on the browser in realtime. Now, we are facing the COVID-19 situation, and the demand for congestion monitoring is getting higher. So, I decided to rebuild the demo application that I've created and to publish the repo as an Open Source.

One of the important things is that most people have a mask on the face right now, so the capability of mask detection is required such an application. Fortunately, the Azure Cognitive Services - Face API can detect a face with a mask. We can use the recognition_04 and the detection_03 models (published in 2021) with Face API and the API detects faces who are wearing a face cover (surgical mask, N95 mask, cloth mask) and return "faceMask" and "noseAndMouthCovered" attributes. For more information about this, please see: [Specify a face recognition model - Face API](https://docs.microsoft.com/en-us/azure/cognitive-services/face/face-api-how-to-topics/specify-recognition-model).

![Congestion Monitor Demo](Documentation/Images/cm_appsdemo.png)

## Architecture

![Congestion Monitor](Documentation/Images/cm_architecture.png)

## Technology stack

* .NET 5
* Vue.js
* Azure Functions
* Azure SignalR Service
* Azure Cosmos DB
* Azure Cognitive Services
* Azure Static Web Apps
* Azure Application Insights
* Azure Resource Manager
* GitHub Actions
* (Azure DevOps)

## How to use source codes in this repo

This repo is setting up as a Template Repository. So, you can create a new GitHub repo from this repo at ["Use this template"](https://github.com/chack411/CongestionMonitor/generate).

![Use this Template](Documentation/Images/cm_gh_templateproject.png)

After that, clone your repo on GitHub to your local environment and start to develop your own Congestion Monitor!

```sh
git clone https://github.com/{yourGitHubAccountName}/{yourRepoName}.git
```

## Deploy and run with this repo using Azure CLI and GitHub Actions

1. [Prepare environments and deploy Azure resources using GitHub Actions](Documentation/deploy-arm-and-function-app.md)
1. [Build Congestion Camera Console App](Documentation/build-camera-console-app.md)
1. [Confrim Congestion Status on Static Web App](Documentation/comfirm-static-web-app.md)

## CI/CD deployment using Azure DevOps

If you are an Azure DevOps user, you can refer to Azure Pipeline YAML files in the `Pipelines` directory in this repo and can run pipelines for ARM deployment, Function App deployment, and building Console Camera app.

## Build Status

|Project|Status|
|---:|:---|
|ARM and Function App|[![ARM and Azure Functions deployment](https://github.com/chack411/CongestionMonitor/actions/workflows/arm-and-func-deploy.yml/badge.svg)](https://github.com/chack411/CongestionMonitor/actions/workflows/arm-and-func-deploy.yml)|

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
