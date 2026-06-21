#!/bin/bash
# VulnTrack — Phase 1 GitHub Secrets Setup
# Run in bash/WSL. Requires: az CLI (logged in), gh CLI (authenticated).
# Idempotent: safe to re-run if interrupted.

set -euo pipefail

REPO="shahi-an/vulnDashboard"
TENANT_ID="374e6789-398d-46ce-a951-858a67cf0f00"
SUBSCRIPTION_ID="1481664a-d73e-4550-9ee5-6597cc378a8c"

echo "=== 1/6  Creating OIDC CI/CD app registration ==="
OIDC_APP_ID=$(az ad app create \
  --display-name "VulnTrack GitHub Actions CI/CD" \
  --query appId -o tsv)
echo "  OIDC App ID: $OIDC_APP_ID"

az ad sp create --id "$OIDC_APP_ID" --output none

SP_OBJ_ID=$(az ad sp show --id "$OIDC_APP_ID" --query id -o tsv)
az role assignment create \
  --role Contributor \
  --assignee-object-id "$SP_OBJ_ID" \
  --assignee-principal-type ServicePrincipal \
  --scope "/subscriptions/$SUBSCRIPTION_ID" \
  --output none
echo "  Contributor role assigned on subscription"

for ENV in production dev prod; do
  az ad app federated-credential create \
    --id "$OIDC_APP_ID" \
    --parameters "{
      \"name\": \"github-$ENV\",
      \"issuer\": \"https://token.actions.githubusercontent.com\",
      \"subject\": \"repo:$REPO:environment:$ENV\",
      \"audiences\": [\"api://AzureADTokenExchange\"]
    }" --output none
  echo "  Federated credential created: $ENV"
done

echo ""
echo "=== 2/6  Creating API app registration ==="
API_APP_ID=$(az ad app create \
  --display-name "VulnTrack.Api" \
  --sign-in-audience AzureADMyOrg \
  --query appId -o tsv)
API_OBJ_ID=$(az ad app show --id "$API_APP_ID" --query id -o tsv)
echo "  API App ID: $API_APP_ID"

az ad app update --id "$API_APP_ID" \
  --identifier-uris "api://$API_APP_ID" \
  --output none

SCOPE_ID=$(uuidgen)
az rest --method PATCH \
  --uri "https://graph.microsoft.com/v1.0/applications/$API_OBJ_ID" \
  --body "{
    \"api\": {
      \"requestedAccessTokenVersion\": 2,
      \"oauth2PermissionScopes\": [{
        \"id\": \"$SCOPE_ID\",
        \"adminConsentDescription\": \"Access VulnTrack API on behalf of the signed-in user.\",
        \"adminConsentDisplayName\": \"Access VulnTrack API\",
        \"isEnabled\": true,
        \"type\": \"User\",
        \"userConsentDescription\": \"Access VulnTrack API on your behalf.\",
        \"userConsentDisplayName\": \"Access VulnTrack API\",
        \"value\": \"access_as_user\"
      }]
    }
  }"
echo "  Scope 'access_as_user' exposed"

echo ""
echo "=== 3/6  Creating SPA app registration ==="
SPA_APP_ID=$(az ad app create \
  --display-name "VulnTrack Frontend" \
  --sign-in-audience AzureADMyOrg \
  --query appId -o tsv)
SPA_OBJ_ID=$(az ad app show --id "$SPA_APP_ID" --query id -o tsv)
echo "  SPA App ID: $SPA_APP_ID"

# Configure SPA platform (redirect URIs) + v2 token endpoint
az rest --method PATCH \
  --uri "https://graph.microsoft.com/v1.0/applications/$SPA_OBJ_ID" \
  --body "{
    \"api\": { \"requestedAccessTokenVersion\": 2 },
    \"spa\": { \"redirectUris\": [\"http://localhost:5173\"] }
  }"

# Grant SPA permission to access API scope
az rest --method PATCH \
  --uri "https://graph.microsoft.com/v1.0/applications/$SPA_OBJ_ID" \
  --body "{
    \"requiredResourceAccess\": [{
      \"resourceAppId\": \"$API_APP_ID\",
      \"resourceAccess\": [{
        \"id\": \"$SCOPE_ID\",
        \"type\": \"Scope\"
      }]
    }]
  }"
echo "  SPA configured with localhost redirect + API permission"

echo ""
echo "=== 4/6  Creating GitHub environments ==="
for ENV in production dev prod; do
  gh api "repos/$REPO/environments/$ENV" -X PUT --silent
  echo "  Environment created: $ENV"
done

echo ""
echo "=== 5/6  Setting GitHub secrets ==="
gh secret set AZURE_CLIENT_ID       --repo "$REPO" --body "$OIDC_APP_ID"
gh secret set AZURE_TENANT_ID       --repo "$REPO" --body "$TENANT_ID"
gh secret set AZURE_SUBSCRIPTION_ID --repo "$REPO" --body "$SUBSCRIPTION_ID"
gh secret set VITE_AZURE_CLIENT_ID  --repo "$REPO" --body "$SPA_APP_ID"
gh secret set VITE_API_SCOPE        --repo "$REPO" --body "api://$API_APP_ID/access_as_user"
echo "  5 secrets set"

echo ""
echo "=== 6/6  Summary ==="
echo ""
echo "  OIDC CI/CD App ID : $OIDC_APP_ID"
echo "  API App ID        : $API_APP_ID   ← needed for Bicep params"
echo "  SPA App ID        : $SPA_APP_ID"
echo ""
echo "  GitHub secrets set (5/10):"
echo "    AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID,"
echo "    VITE_AZURE_CLIENT_ID, VITE_API_SCOPE"
echo ""
echo "--- NEXT STEPS ---"
echo ""
echo "A) Get your Entra object ID for the SQL admin:"
echo "   az ad signed-in-user show --query id -o tsv"
echo ""
echo "B) Fill in infra/parameters/dev.bicepparam:"
echo "   sqlAdminObjectId = <output from A>"
echo "   sqlAdminLogin    = <your display name>"
echo "   tenantId         = $TENANT_ID"
echo "   apiClientId      = $API_APP_ID"
echo "   senderEmail      = <your M365 mailbox>"
echo ""
echo "C) Commit the updated bicepparam file, then trigger:"
echo "   gh workflow run deploy-infra.yml --repo $REPO -f environment=dev"
echo ""
echo "D) After infra deploys, run setup-github-secrets-phase2.sh"
echo "   (will be created for you after infra output is available)"
