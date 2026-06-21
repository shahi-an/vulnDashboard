param storageAccountName string
param serviceBusNamespaceName string
param apiPrincipalId string
param funcPrincipalId string

// Built-in Azure RBAC role definition IDs (immutable across all tenants/subscriptions).
var storageBlobDataContributor = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
var storageBlobDataOwner       = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
var storageQueueDataContributor = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '974c5e8b-45b9-4653-ba55-5f855dd0fb88')
var storageTableDataContributor = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3')
var serviceBusDataOwner        = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419')

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2023-01-01-preview' existing = {
  name: serviceBusNamespaceName
}

// ── API Web App ────────────────────────────────────────────────────────────────

resource apiBlobContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, apiPrincipalId, storageBlobDataContributor)
  properties: {
    roleDefinitionId: storageBlobDataContributor
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource apiServiceBusOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(serviceBusNamespace.id, apiPrincipalId, serviceBusDataOwner)
  properties: {
    roleDefinitionId: serviceBusDataOwner
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ── Function App ───────────────────────────────────────────────────────────────
// Storage Blob Data Owner is a superset of Contributor and is required by the
// Azure Functions WebJobs runtime for blob lease management when using identity-based
// connections (AzureWebJobsStorage__accountName).

resource funcBlobOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, funcPrincipalId, storageBlobDataOwner)
  properties: {
    roleDefinitionId: storageBlobDataOwner
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource funcQueueContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, funcPrincipalId, storageQueueDataContributor)
  properties: {
    roleDefinitionId: storageQueueDataContributor
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource funcTableContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, funcPrincipalId, storageTableDataContributor)
  properties: {
    roleDefinitionId: storageTableDataContributor
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource funcServiceBusOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(serviceBusNamespace.id, funcPrincipalId, serviceBusDataOwner)
  properties: {
    roleDefinitionId: serviceBusDataOwner
    principalId: funcPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ── Manual step required after deployment ─────────────────────────────────────
// SQL managed identity access cannot be granted via Bicep. After first deploy, run:
//
//   sqlcmd -S <serverFqdn> -d VulnTrack --authentication-method ActiveDirectoryDefault -Q "
//     CREATE USER [app-vulntrack-<env>] FROM EXTERNAL PROVIDER;
//     ALTER ROLE db_owner ADD MEMBER [app-vulntrack-<env>];
//     CREATE USER [func-vulntrack-<env>] FROM EXTERNAL PROVIDER;
//     ALTER ROLE db_owner ADD MEMBER [func-vulntrack-<env>];"
//
// Graph Mail.Send permission must also be granted via Entra portal admin consent.
