using '../main.bicep'

param environmentName = 'prod'
param location = 'eastus2'

param sqlAdminObjectId = '__REPLACE_WITH_PROD_OBJECT_ID__'
param sqlAdminLogin    = '__REPLACE_WITH_PROD_ADMIN_NAME__'
param tenantId         = '__REPLACE_WITH_TENANT_ID__'
param apiClientId      = '__REPLACE_WITH_API_CLIENT_ID__'
param senderEmail      = '__REPLACE_WITH_SENDER_EMAIL__'
