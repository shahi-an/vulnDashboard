using '../main.bicep'

param environmentName = 'dev'
param location = 'eastus2'

// Get your object ID:  az ad signed-in-user show --query id --output tsv
param sqlAdminObjectId = '__REPLACE_WITH_YOUR_OBJECT_ID__'
param sqlAdminLogin    = '__REPLACE_WITH_YOUR_DISPLAY_NAME__'

// From your Entra ID tenant overview page.
param tenantId    = '__REPLACE_WITH_TENANT_ID__'

// Client ID of the VulnTrack.Api app registration in Entra.
param apiClientId = '__REPLACE_WITH_API_CLIENT_ID__'

// Licensed M365 mailbox address used as the From address for SLA reminder emails.
param senderEmail = '__REPLACE_WITH_SENDER_EMAIL__'
