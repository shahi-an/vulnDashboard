param location string
param environmentName string
param appInsightsConnectionString string
param sqlConnectionString string
param storageAccountName string
param serviceBusNamespace string
param tenantId string
param apiClientId string
param senderEmail string
param allowedOrigin string

var planName = 'asp-vulntrack-api-${environmentName}'
var webAppName = 'app-vulntrack-${environmentName}'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      healthCheckPath: '/health'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
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
          name: 'AzureAd__Audience'
          value: 'api://${apiClientId}'
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
          name: 'Graph__SenderEmail'
          value: senderEmail
        }
        {
          name: 'AllowedOrigins__0'
          value: allowedOrigin
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

output principalId string = webApp.identity.principalId
output webAppName string = webApp.name
output defaultHostName string = webApp.properties.defaultHostName
