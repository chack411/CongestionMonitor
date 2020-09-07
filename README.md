# Congestion Monitor

This repo is Congestion Monitor projects to detect congestion situations through web cameras in real time.

A few weeks ago, I realized that I have created a demo for my customer last year that the applicaton is to detect faces and to show the number and the trend on the browser in real time. Now, we are facing the COVID-19 situation, and the demand of congestion monitoring is getting higher. So, I decided to rebuild the demo application I created last year and to publish the repo as an open source.

The one of the importaint things is that most of people have a mask on the face right now, so a capability of mask detection is required shuch an application. Fortunately, the Azure Cognitive Services - Face API has a capability to detect a face with (small) mask. Unfortunately, large mask could result in no face to be detected and the recognition rate with mask is not good. But, if needed, you can replace the face recognition to another AI services for the improvement of the recognition rate.

## Architecture

![Congestion Monitor](Documentation/Images/cm_architecrue.png)

## Technology stack

* .NET Core 3.1
* Vue.js
* Azure Functions
* Azure SignalR Service
* Azure Cosmos DB
* Azure Cognitive Services
* Azure Static Web Apps
* Azure Storage
* Azure Application Insights
* Azure Resource Manager
* Azure DevOps
* GitHub Actions

## How to deploy with this repo

### Manual deployment

### Using Azure DevOps

## Build Status

Azure Functions App: [![Build Status](https://dev.azure.com/akirain/Congestion%20Monitor/_apis/build/status/chack411.CongestionMonitorFunctionApp?branchName=master)](https://dev.azure.com/akirain/Congestion%20Monitor/_build/latest?definitionId=71&branchName=master)

Camera Console App: [![Build Status](https://dev.azure.com/akirain/Congestion%20Monitor/_apis/build/status/chack411.CongestionCameraConsoleApp?branchName=master)](https://dev.azure.com/akirain/Congestion%20Monitor/_build/latest?definitionId=72&branchName=master)

Static Vue App: ![Static Web for Congestion Status - CI/CD](https://github.com/chack411/CongestionMonitor/workflows/Static%20Web%20for%20Congestion%20Status%20-%20CI/CD/badge.svg)

## Azure deployment
[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https://github.com/chack411/CongestionMonitor/blob/master/ARMTemplates/template.json)

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
