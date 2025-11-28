# Azure Deployment Checklist

## Prerequisites
- [ ] Azure subscription
- [ ] Azure CLI installed
- [ ] Visual Studio or .NET CLI
- [ ] Azure Key Vault created

---

## STEP 1: Azure Key Vault Setup

### Create Key Vault (if not exists)
```bash
az keyvault create --name m3h-keyvault --resource-group your-resource-group --location eastus
```

### Store Secrets in Key Vault
```bash
# Store Database Connection String
az keyvault secret set --vault-name m3h-keyvault \
  --name DatabaseConnectionString \
  --value "Server=tcp:m3hkanamofunctiondb.database.windows.net,1433;Initial Catalog=m3h-kanamo-functionDB;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### Verify Secret
```bash
az keyvault secret show --vault-name m3h-keyvault --name DatabaseConnectionString
```

---

## STEP 2: Function App Configuration

### Enable Managed Identity
```bash
az functionapp identity assign --name m3h-beerkn-functionapp --resource-group your-resource-group
```

### Grant Key Vault Access to Function App
```bash
# Get the Principal ID of the managed identity
PRINCIPAL_ID=$(az functionapp identity show --name m3h-beerkn-functionapp --resource-group your-resource-group --query principalId -o tsv)

# Grant access to Key Vault
az keyvault set-policy --name m3h-keyvault \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

### Add Application Settings to Function App
```bash
az functionapp config appsettings set --name m3h-beerkn-functionapp \
  --resource-group your-resource-group \
  --settings "KeyVaultUri=https://m3h-keyvault.vault.azure.net/"
```

### Verify Settings
```bash
az functionapp config appsettings list --name m3h-beerkn-functionapp --resource-group your-resource-group
```

---

## STEP 3: Backend Deployment

### Option A: Deploy using Zip Deploy (Local)
```bash
cd backend/FunctionbeerAPI

# Build the project
dotnet build --configuration Release

# Create a deployment package
dotnet publish --configuration Release --output ./publish

# Zip the published files
cd publish
zip -r ../FunctionbeerAPI.zip .
cd ..

# Deploy using Azure CLI
az functionapp deployment source config-zip --resource-group your-resource-group \
  --name m3h-beerkn-functionapp \
  --src FunctionbeerAPI.zip
```

### Option B: Deploy using Visual Studio
1. Right-click project → Publish
2. Select existing Function App (m3h-beerkn-functionapp)
3. Click Publish

### Option C: Deploy using Azure DevOps or GitHub Actions
See `DEPLOYMENT_GITHUB_ACTIONS.md` for CI/CD setup

---

## STEP 4: Frontend Deployment

### Build Frontend
```bash
cd frontend
npm install
npm run build:azure
```

### Deploy to Azure Static Web Apps (if using)
```bash
az staticwebapp create \
  --name nomikai-app-frontend \
  --resource-group your-resource-group \
  --source ./dist \
  --build-folder dist \
  --api-location api
```

### Or Deploy to Azure Storage + CDN
```bash
# Create storage account (if not exists)
az storage account create --name nomikaistorage --resource-group your-resource-group --location eastus

# Upload files
az storage blob upload-batch \
  --account-name nomikaistorage \
  --destination '$web' \
  --source ./dist
```

---

## STEP 5: Testing

### Test Backend Functions
```bash
# Test Calculate endpoint
curl -X POST https://m3h-beerkn-functionapp.azurewebsites.net/api/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "totalAmount": 10000,
    "numberOfParticipants": 5,
    "eventID": 1,
    "participantID": 1
  }'

# Test Search endpoint
curl -X GET "https://m3h-beerkn-functionapp.azurewebsites.net/api/nomikai/search?eventdate=2025-11-28"
```

### Test Frontend
- Open `https://your-frontend-url`
- Test event registration
- Test event search
- Test payment flag updates

---

## STEP 6: Monitoring & Logging

### View Function App Logs
```bash
az functionapp log tail --name m3h-beerkn-functionapp --resource-group your-resource-group
```

### Check Application Insights
```bash
# View insights URL
az functionapp show --name m3h-beerkn-functionapp --resource-group your-resource-group --query appInsightsId
```

---

## Troubleshooting

### Connection String Not Found Error
- Verify Key Vault secret exists: `az keyvault secret show --vault-name m3h-keyvault --name DatabaseConnectionString`
- Verify Function App has managed identity enabled
- Verify access policy is set correctly

### 401 Unauthorized Errors
- Check CORS settings in Function App
- Verify API keys if using API Key authentication

### Database Connection Timeouts
- Check SQL Server firewall rules allow Azure services
- Verify connection string is correct
- Check if SQL Server is running

---

## Cleanup (When needed)
```bash
# Delete Function App
az functionapp delete --name m3h-beerkn-functionapp --resource-group your-resource-group

# Delete Key Vault
az keyvault delete --name m3h-keyvault --resource-group your-resource-group

# Delete resource group
az group delete --name your-resource-group
```

---

## Important Notes

✅ **Secrets are NOW retrieved from Azure Key Vault** - Connection string is no longer hardcoded  
✅ **Managed Identity used** - No need to store credentials in app settings  
✅ **CORS enabled** - Frontend can communicate with backend  
⚠️ **Replace `your-resource-group`** with your actual Azure resource group name  
⚠️ **Replace `your-username` and `your-password`** with your actual SQL credentials  
⚠️ **Keep `AZURE_KEY_VAULT_SETUP.md` and this file** for future reference  
