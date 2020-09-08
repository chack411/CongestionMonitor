# Congestion Monitor

This repo is Congestion Monitor projects to detect congestion situations through web cameras in real time.

A few weeks ago, I realized that I have created a demo for my customer last year that the application is to detect faces and to show the number and the trend on the browser in realtime. Now, we are facing the COVID-19 situation, and the demand of congestion monitoring is getting higher. So, I decided to rebuild the demo application I created last year and to publish the repo as an Open Source.

One of the important things is that most people have a mask on the face right now, so the capability of mask detection is required such an application. Fortunately, the Azure Cognitive Services - Face API can detect a face with a (small) mask. Unfortunately, a large mask could result in no face to be detected and the recognition rate with a mask is not good. But, if needed, you can replace the face recognition to other AI services for the improvement of the recognition rate.

![Congestion Monitor Demo](Documentation/Images/cm_appsdemo.png)

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

## How to use source codes in this repo

This repo is setting up as Template repository. So, you can create a new GitHub repo from this repo at [Use this template](https://github.com/chack411/CongestionMonitor/generate).

![Use this Template](Documentation/Images/cm_gh_templateproject.png)

After that, clone your repo on GitHub to your local environment.

```sh
git clone https://github.com/(your repo name)/CongestionMonitor.git
```

## How to deploy with this repo manually

Let's deploy the Congestion Monitor applications using Azure CLI and ARM Template.

### Preparation

- If you haven't had an Azure subscription yet, you can create an Azure free account at [Create your Azure free account today](https://azure.microsoft.com/en-us/free/).
- If you haven't install the Azure CLI, please see [Install the Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) and setup the CLI with your Azure subscription.
- If you need more details about the login with the CLI, please see [Sign in with Azure CLI](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli?view=azure-cli-latest).

### Verify subscription

Run the command az account list -o table
```sh
az account list -o table

Name                     CloudName    SubscriptionId                        State    IsDefault
-----------------------  -----------  ------------------------------------  -------  -----------
My primary subscription  AzureCloud   xxxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx  Enabled  True
Another sub1             AzureCloud   xxxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx  Enabled  False
Another sub2             AzureCloud   xxxxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxx  Enabled  False
```

If you have more than subscription, make sure that subscription is set as default using the subscription name:

```sh
az account set -s 'My primary subscription'
```

### Generate a personal access token (PAT) for GitHub API

Before create a deployment, you need to generate a personal access token of GitHub that is used to access the GitHub API from Azure Static Web deployment with ARM Template.

Access to [Personal Access Tokens page](https://github.com/settings/tokens) in Developer settings at GitHub, and click `Generate new token` button. Then, input the token name in Note, and select `repo` and `workflow` at `Select scopes` area, and click `Generate token` button.

![Generate Access Token](Documentation/Images/cm_gh_pat.png)

So, you can see and copy the new token only once. Let's copy and keep the token for the next step.

![Generated Access Token](Documentation/Images/cm_gh_pat2.png)

### Modify parameters for your deployment

You need to modify `parameters.json` in `CongestionMonitor/ARMTemplate` with your own parameters.

```json
{
    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentParameters.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "cm_app_name": {
            "value": "(app resource prefix is here. Ex: mycmapptest)"
        },
        "sites_cm_repositoryUrl": {
            "value": "https://github.com/(your repo name)/CongestionMonitor"
        },
        "sites_cm_repositoryToken": {
            "value": "(personal access token here)"
        }
    }
}
```

### Create a resource group

```sh
az group create -g mycmapps-rg -l japaneast
```

### Create a deployment and start to deploy

OK, it's time to deploy with ARM template. Let's create a deployment at resource group from a local template file: `template.json`, using parameters from a local JSON file: `parameters.json`.

```sh
cd CongestionMonitor/ARMTemplate

az deployment group create \
  --name cm-arm-deployment \
  --resource-group mycmapps-rg \
  --template-file template.json --parameters @parameters.json
```


## CI/CD deployment using Azure DevOps

To be updated...

## Azure deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https://github.com/chack411/CongestionMonitor/blob/master/ARMTemplates/template.json)

## Build Status

|Project|Status|
|---:|:---|
|Azure Functions App|[![Build Status](https://dev.azure.com/akirain/Congestion%20Monitor/_apis/build/status/chack411.CongestionMonitorFunctionApp?branchName=master)](https://dev.azure.com/akirain/Congestion%20Monitor/_build/latest?definitionId=71&branchName=master)|
|Camera Console App|[![Build Status](https://dev.azure.com/akirain/Congestion%20Monitor/_apis/build/status/chack411.CongestionCameraConsoleApp?branchName=master)](https://dev.azure.com/akirain/Congestion%20Monitor/_build/latest?definitionId=72&branchName=master)|
|Static Vue App|![Static Web for Congestion Status - CI/CD](https://github.com/chack411/CongestionMonitor/workflows/Static%20Web%20for%20Congestion%20Status%20-%20CI/CD/badge.svg)|

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
