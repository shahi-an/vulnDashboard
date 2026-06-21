param location string
param environmentName string
param appInsightsConnectionString string
param sqlConnectionString string
param storageAccountName string
param serviceBusNamespace string
param tenantId string
param apiClientId string
param senderEmail string

var planName = 'asp-vulntrack-func-${environmentName}'
var funcAppName = 'func-vulntrack-${environmentName}'

resource functionPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp,linux'
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: funcAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      appSettings: [
        {
          // Identity-based connection for the WebJobs runtime storage (no shared key needed).
          name: 'AzureWebJobsStorage__accountName'
          value: storageAccountName
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          // Identity-based connection for the Service Bus trigger binding (Connection = "ServiceBusConnection").
          name: 'ServiceBusConnection__fullyQualifiedNamespace'
          value: '${serviceBusNamespace}.servicebus.windows.net'
        }
        {
          // Used by ServiceBusPublisher (Infrastructure) to construct the FQDN.
          name: 'ServiceBus__Namespace'
          value: serviceBusNamespace
        }
        {
          name: 'ServiceBus__VulnerabilityEventsQueue'
          value: 'vulnerability-events'
        }
        {
          name: 'ServiceBus__NotificationsQueue'
          value: 'notifications'
        }
        {
          name: 'AzureStorage__AccountName'
          value: storageAccountName
        }
        {
          name: 'AzureStorage__AttachmentsContainer'
          value: 'attachments'
        }
        {
          name: 'AzureAd__TenantId'
          value: tenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: apiClientId
        }
        {
          name: 'Graph__SenderEmail'
          value: senderEmail
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: sqlConnectionString
          type: 'SQLAzure'
        }
      ]
    }
  }
}

output principalId string = functionApp.identity.principalId
output functionAppName string = functionApp.name
