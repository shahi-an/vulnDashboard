targetScope = 'resourceGroup'

@description('Short environment name used in all resource names.')
@allowed(['dev', 'prod'])
param environmentName string = 'dev'

@description('Primary Azure region. Must support Azure Static Web Apps (eastus2, westus2, westeurope, etc.).')
param location string = 'eastus2'

@description('Object ID of the Entra ID user or group that will administer Azure SQL.')
param sqlAdminObjectId string

@description('Display name of the SQL administrator (any descriptive string).')
param sqlAdminLogin string

@description('Entra ID Tenant ID.')
param tenantId string

@description('Client ID of the VulnTrack.Api Entra app registration (exposes access_as_user scope).')
param apiClientId string

@description('M365 shared mailbox address used as the sender for SLA reminder emails.')
param senderEmail string

// ── Modules ───────────────────────────────────────────────────────────────────

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    environmentName: environmentName
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    environmentName: environmentName
  }
}

module servicebus 'modules/servicebus.bicep' = {
  name: 'servicebus'
  params: {
    location: location
    environmentName: environmentName
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    location: location
    environmentName: environmentName
    adminObjectId: sqlAdminObjectId
    adminLogin: sqlAdminLogin
  }
}

module staticWebApp 'modules/staticwebapp.bicep' = {
  name: 'staticwebapp'
  params: {
    location: location
    environmentName: environmentName
  }
}

module appservice 'modules/appservice.bicep' = {
  name: 'appservice'
  params: {
    location: location
    environmentName: environmentName
    appInsightsConnectionString: monitoring.outputs.connectionString
    sqlConnectionString: sql.outputs.connectionString
    storageAccountName: storage.outputs.accountName
    serviceBusNamespace: servicebus.outputs.namespaceName
    tenantId: tenantId
    apiClientId: apiClientId
    senderEmail: senderEmail
    // Derived from the Static Web App hostname so CORS is always correct.
    allowedOrigin: 'https://${staticWebApp.outputs.defaultHostname}'
  }
}

module functions 'modules/functions.bicep' = {
  name: 'functions'
  params: {
    location: location
    environmentName: environmentName
    appInsightsConnectionString: monitoring.outputs.connectionString
    sqlConnectionString: sql.outputs.connectionString
    storageAccountName: storage.outputs.accountName
    serviceBusNamespace: servicebus.outputs.namespaceName
    tenantId: tenantId
    apiClientId: apiClientId
    senderEmail: senderEmail
  }
}

module roleAssignments 'modules/roleassignments.bicep' = {
  name: 'roleassignments'
  params: {
    storageAccountName: storage.outputs.accountName
    serviceBusNamespaceName: servicebus.outputs.namespaceName
    apiPrincipalId: appservice.outputs.principalId
    funcPrincipalId: functions.outputs.principalId
  }
}

// ── Outputs ───────────────────────────────────────────────────────────────────

output apiUrl string = 'https://${appservice.outputs.defaultHostName}'
output frontendUrl string = 'https://${staticWebApp.outputs.defaultHostname}'
output apiAppServiceName string = appservice.outputs.webAppName
output functionAppName string = functions.outputs.functionAppName
output sqlServerFqdn string = sql.outputs.serverFqdn
output sqlDatabaseName string = sql.outputs.databaseName
output storageAccountName string = storage.outputs.accountName
output serviceBusNamespace string = servicebus.outputs.namespaceName
