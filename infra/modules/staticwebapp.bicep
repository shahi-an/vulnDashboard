param location string
param environmentName string

// Azure Static Web Apps is only available in specific regions.
// Supported: eastus2, westus2, centralus, westeurope, eastasia, eastus, australiaeast, japaneast, southeastasia, uksouth, northeurope.
// If your main location is not in this list, override this parameter separately.
param swaLocation string = location

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: 'swa-vulntrack-${environmentName}'
  location: swaLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    stagingEnvironmentPolicy: 'Disabled'
    allowConfigFileUpdates: true
    buildProperties: {
      // GitHub Actions deployment is managed by deploy-frontend.yml; suppress auto-workflow generation.
      skipGithubActionWorkflowGeneration: true
    }
  }
}

output defaultHostname string = staticWebApp.properties.defaultHostname
output id string = staticWebApp.id
