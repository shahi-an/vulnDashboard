param location string
param environmentName string

resource namespace 'Microsoft.ServiceBus/namespaces@2023-01-01-preview' = {
  name: 'sb-vulntrack-${environmentName}'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    // Shared access keys disabled; all clients must use managed identity.
    disableLocalAuth: true
  }
}

resource vulnerabilityEventsQueue 'Microsoft.ServiceBus/namespaces/queues@2023-01-01-preview' = {
  parent: namespace
  name: 'vulnerability-events'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'P14D'
  }
}

resource notificationsQueue 'Microsoft.ServiceBus/namespaces/queues@2023-01-01-preview' = {
  parent: namespace
  name: 'notifications'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'P7D'
  }
}

output namespaceName string = namespace.name
output fullyQualifiedNamespace string = '${namespace.name}.servicebus.windows.net'
output id string = namespace.id
