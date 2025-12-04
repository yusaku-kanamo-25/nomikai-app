# GitHub Actions Deployment Setup

## Overview

This repository uses GitHub Actions for automated deployment:
- **Frontend**: Static Web App (already configured)
- **Backend**: Azure Functions App (newly added)

## Backend Function App Deployment

### Workflow File
`.github/workflows/azure-functions-deploy.yml`

### Features
- ✅ Triggers on push to `main` branch when backend files change
- ✅ Manual deployment via `workflow_dispatch`
- ✅ Builds .NET 8.0 Function App
- ✅ Deploys to Azure Functions using publish profile
- ✅ Provides deployment summary

### Setup Instructions

#### Step 1: Get Publish Profile from Azure

1. Go to Azure Portal
2. Navigate to your Function App: `nomikai-funcapp`
3. Click **Get publish profile** (in Overview or Deployment Center)
4. Download the `.PublishSettings` file
5. Open the file and copy its entire contents

#### Step 2: Add GitHub Secret

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secret:
   - **Name**: `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`
   - **Value**: Paste the entire contents of the publish profile

#### Step 3: Configure Environment Variables (Optional)

If using GitHub Environments for additional control:

1. Go to **Settings** → **Environments**
2. Create/Edit `production` environment
3. Add environment secrets or variables as needed

### Configuration

#### Environment Variables in Workflow

```yaml
env:
  AZURE_FUNCTIONAPP_NAME: 'nomikai-funcapp'
  AZURE_FUNCTIONAPP_PACKAGE_PATH: 'backend/FunctionbeerAPI'
  DOTNET_VERSION: '8.0.x'
```

Update these if your Function App name or path changes.

#### Trigger Configuration

The workflow triggers on:
1. **Push to main** - When backend files change
2. **Manual dispatch** - Via GitHub Actions UI

To change trigger paths:
```yaml
paths:
  - 'backend/FunctionbeerAPI/**'
  - '.github/workflows/azure-functions-deploy.yml'
```

### Deployment Process

1. **Checkout Code**: Gets latest code from repository
2. **Setup .NET**: Installs .NET 8.0 SDK
3. **Build**: Compiles the Function App in Release mode
4. **Deploy**: Publishes to Azure using the publish profile
5. **Summary**: Provides deployment details

### Azure Function App Configuration

Ensure these settings are configured in Azure Portal:

#### Application Settings Required
```
DatabaseConnectionString = "Your SQL connection string"
KeyVaultUri = "" (optional, leave empty to skip Key Vault)
```

#### Runtime Settings
- Runtime: .NET 8.0
- Platform: 64-bit
- Always On: Enabled (recommended for production)

### Testing the Workflow

#### Method 1: Push Changes
```bash
git add .
git commit -m "Update backend function"
git push origin main
```

#### Method 2: Manual Trigger
1. Go to **Actions** tab in GitHub
2. Select **Deploy Azure Functions App**
3. Click **Run workflow**
4. Select branch and run

### Monitoring Deployments

1. Go to **Actions** tab in GitHub
2. Click on the workflow run
3. View logs for each step
4. Check deployment summary at the end

### Troubleshooting

#### Deployment Fails

**Check Publish Profile**:
- Ensure secret `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` is correctly set
- Verify publish profile hasn't expired (regenerate if needed)

**Check Build Errors**:
- Review build logs in GitHub Actions
- Ensure all NuGet packages restore correctly
- Verify .NET version matches (8.0)

**Check Azure Configuration**:
- Verify Function App name is correct
- Ensure Application Settings are configured
- Check Function App is running

#### Deployment Succeeds but App Doesn't Work

**Check Application Settings**:
```bash
# Verify in Azure Portal or via CLI
az functionapp config appsettings list --name nomikai-funcapp --resource-group M3Harbor
```

**Check Function Logs**:
- Go to Function App in Azure Portal
- Navigate to Monitoring → Logs
- Review any runtime errors

**Test Connection String**:
- Ensure `DatabaseConnectionString` is valid
- Test database connectivity

### Frontend Deployment

The frontend Static Web App deployment is already configured in:
`.github/workflows/azure-static-web-apps-nice-stone-031ceb100.yml`

It deploys automatically when frontend files change.

### Best Practices

1. **Test Locally First**: Always test changes locally before pushing
2. **Use Pull Requests**: Create PRs for code review before merging to main
3. **Monitor Deployments**: Check GitHub Actions after each deployment
4. **Keep Secrets Secure**: Never commit secrets to the repository
5. **Update Documentation**: Keep this file updated with any changes

### Workflow Status Badge (Optional)

Add to README.md:
```markdown
[![Deploy Azure Functions](https://github.com/yusaku-kanamo-25/nomikai-app/actions/workflows/azure-functions-deploy.yml/badge.svg)](https://github.com/yusaku-kanamo-25/nomikai-app/actions/workflows/azure-functions-deploy.yml)
```

## Summary

✅ **Backend workflow created**: `.github/workflows/azure-functions-deploy.yml`
✅ **Deployment automated**: Deploys on push to main
✅ **Manual trigger available**: Can deploy on-demand
✅ **Environment variables supported**: Configure via Azure Portal

**Next Steps**:
1. Add `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` secret to GitHub
2. Push code to trigger first deployment
3. Verify deployment in Azure Portal
