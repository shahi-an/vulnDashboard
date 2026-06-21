using '../main.bicep'

param environmentName = 'dev'
param location = 'eastus'

// Get your object ID:  az ad signed-in-user show --query id --output tsv
param sqlAdminObjectId = '0e8d9b00-a59c-4d62-becf-ce673cf61510'
param sqlAdminLogin    = 'Shahi A'

// From your Entra ID tenant overview page.
param tenantId    = '374e6789-398d-46ce-a951-858a67cf0f00'

// Client ID of the VulnTrack.Api app registration in Entra.
param apiClientId = '2d500d63-e382-443e-9ac5-834d71b478fe'

// Licensed M365 mailbox address used as the From address for SLA reminder emails.
// Replace with a real M365 mailbox before enabling reminder emails.
param senderEmail = 'vulntrack-noreply@placeholder.onmicrosoft.com'
