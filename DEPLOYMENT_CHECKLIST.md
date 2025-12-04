# Deployment Checklist

## ✅ Pre-Deployment Setup

### 1. GitHub Actions Setup (Primary Deployment Method)

#### Backend Function App
- [ ] Get Azure Function App publish profile from Azure Portal
- [ ] Go to: Function App → **Get publish profile**
- [ ] Copy entire XML content
- [ ] Add GitHub secret: `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`
- [ ] Path: **Settings** → **Secrets and variables** → **Actions**

#### Frontend Static Web App (Already Configured)
- [ ] Verify secret exists: `AZURE_STATIC_WEB_APPS_API_TOKEN_NICE_STONE_031CEB100`
- [ ] No action needed if already set

### 2. Azure Function App Configuration

#### Application Settings (Environment Variables - Primary)
- [ ] Go to Function App: `nomikai-funcapp`
- [ ] Navigate to: **Configuration** → **Application settings**
- [ ] Add/Verify:
  ```
  DatabaseConnectionString = "Server=...;Database=...;User Id=...;Password=...;Encrypt=True;"
  KeyVaultUri = "" (leave empty - Key Vault is optional)
  ```
- [ ] Click **Save** and **Continue**

#### Optional: Azure Key Vault (if needed)
- [ ] Only configure if you want Key Vault as fallback
- [ ] See [Azure Key Vault Setup](#optional-azure-key-vault-setup) below

### 3. CORS Configuration
- [ ] Function App → **CORS** settings
- [ ] Add frontend URL: `https://nice-stone-031ceb100.3.azurestaticapps.net`
- [ ] Or add wildcard: `*` (for development)
- [ ] Remove `http://localhost:*` in production

---

## 🚀 Deployment Process

### Automated Deployment (Recommended)

#### Deploy Backend
1. Make changes to backend code
2. Commit and push to `main` branch:
   ```bash
   git add backend/
   git commit -m "Update backend function"
   git push origin main
   ```
3. GitHub Actions automatically deploys
4. Monitor at: https://github.com/yusaku-kanamo-25/nomikai-app/actions

#### Deploy Frontend
1. Make changes to frontend code
2. Commit and push to `main` branch:
   ```bash
   git add frontend/
   git commit -m "Update frontend"
   git push origin main
   ```
3. GitHub Actions automatically deploys

#### Manual Deployment Trigger
- [ ] Go to **Actions** tab in GitHub
- [ ] Select workflow: **Deploy Azure Functions App**
- [ ] Click **Run workflow**
- [ ] Select `main` branch → **Run workflow**

### Manual Deployment (Alternative)

#### Backend via Azure CLI
#### Backend via Azure CLI
```bash
cd backend/FunctionbeerAPI
dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish
cd publish
Compress-Archive -Path * -DestinationPath ../deploy.zip
cd ..
az functionapp deployment source config-zip --resource-group M3Harbor --name nomikai-funcapp --src deploy.zip
```

#### Backend via Visual Studio
1. Right-click project → **Publish**
2. Select: `nomikai-funcapp`
3. Click **Publish**

---

## ✅ Testing & Verification

### Backend Health Check
```bash
# Test base URL
curl https://nomikai-funcapp.azurewebsites.net

# Test search endpoint
curl "https://nomikai-funcapp.azurewebsites.net/api/nomikai/search?eventdate=2024-12-04"
```

### Frontend Health Check
```bash
# Test frontend
curl https://nice-stone-031ceb100.3.azurestaticapps.net
```

### Integration Test
- [ ] Open frontend in browser
- [ ] Register a new event with participants
- [ ] Verify event is saved
- [ ] Search for the event
- [ ] Update payment flags
- [ ] Verify updates persist

### Check Deployment Logs
- [ ] GitHub Actions: https://github.com/yusaku-kanamo-25/nomikai-app/actions
- [ ] Azure Function Logs: Portal → Function App → **Log stream**
- [ ] Static Web App Logs: Portal → Static Web App → **Logs**

---

## 🚨 Troubleshooting

### GitHub Actions Deployment Fails

**Issue: Publish profile secret not found**
```
Solution:
1. Regenerate publish profile from Azure Portal
2. Update GitHub secret: AZURE_FUNCTIONAPP_PUBLISH_PROFILE
3. Retry workflow
```

**Issue: Build fails**
```
Solution:
1. Check GitHub Actions logs for errors
2. Verify .NET 8.0 is specified
3. Check NuGet package restore issues
4. Test build locally: dotnet build
```

### Function App Errors

**Issue: 500 Internal Server Error**
```
Solution:
1. Check Application Settings in Azure
2. Verify DatabaseConnectionString is set
3. Check Function App logs for exceptions
4. Test connection string locally
```

**Issue: CORS errors from frontend**
```
Solution:
1. Function App → CORS settings
2. Add frontend URL or wildcard (*)
3. Save and restart Function App
```

**Issue: Database connection fails**
```
Solution:
1. Verify connection string format
2. Check SQL Server firewall allows Azure services
3. Test connection using Azure Data Studio
4. Check SQL credentials are correct
```

### Key Vault Issues (if using)

**Issue: Key Vault access denied**
```
Solution:
1. Enable Managed Identity on Function App
2. Grant Key Vault access policy
3. Verify KeyVaultUri setting
4. Or use environment variable instead (primary method)
```

---

## 📊 Monitoring

### GitHub Actions Monitoring
- [ ] Enable email notifications for failed workflows
- [ ] Review deployment history regularly
- [ ] Check workflow run times

### Azure Monitoring
- [ ] Enable Application Insights (recommended)
- [ ] Set up alerts for:
  - Failed requests
  - High response times
  - Exceptions
- [ ] Review metrics dashboard

### View Logs
```bash
# Function App logs
az functionapp log tail --name nomikai-funcapp --resource-group M3Harbor

# View Application Settings
az functionapp config appsettings list --name nomikai-funcapp --resource-group M3Harbor
```

---

## 📝 Optional: Azure Key Vault Setup

**Note**: Key Vault is optional. Environment variables are the primary configuration method.

### Only if you want Key Vault as fallback:

#### 1. Enable Managed Identity  
