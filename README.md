"# Nomikai App (飲み会アプリ)

A comprehensive event management application for tracking drinking party expenses and participants.

## 🏗️ Architecture

- **Frontend**: Static Web App (Vue.js)
- **Backend**: Azure Functions (.NET 8.0)
- **Database**: Azure SQL Database

## 📁 Project Structure

```
nomikai-app/
├── frontend/              # Vue.js Static Web App
│   ├── index.html
│   ├── script.js
│   ├── config.js
│   └── style.css
├── backend/
│   └── FunctionbeerAPI/   # .NET 8.0 Azure Functions
│       ├── Function1.cs
│       ├── FunctionbeerAPI.csproj
│       └── local.settings.json
└── .github/
    └── workflows/         # CI/CD Pipelines
        ├── azure-functions-deploy.yml
        └── azure-static-web-apps-nice-stone-031ceb100.yml
```

## 🚀 Deployment

### Automated CI/CD

Both frontend and backend deploy automatically via GitHub Actions:

- **Frontend**: Deploys on push to `main` (frontend changes)
- **Backend**: Deploys on push to `main` (backend changes)

### Setup Instructions

See [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) for complete setup instructions including:
- GitHub secrets configuration
- Azure publish profile setup
- Environment variable configuration

## ⚙️ Configuration

### Backend Configuration

See [ENV_CONFIG.md](./ENV_CONFIG.md) for detailed configuration options:
- Environment variables (primary)
- Azure Key Vault (optional)
- Local development setup

### Frontend Configuration

API endpoint configured in `frontend/config.js`:
- Environment variable: `window.ENV.API_BASE_URL`
- Fallback: Hardcoded URL

## 🛠️ Local Development

### Backend

1. Configure environment:
   ```bash
   cd backend/FunctionbeerAPI
   # Edit local.settings.json with your database connection string
   ```

2. Run locally:
   ```bash
   dotnet build
   func start
   ```

### Frontend

1. Update API URL in `frontend/config.js`

2. Serve locally:
   ```bash
   cd frontend
   # Use any static file server
   npx serve .
   ```

## 📚 Documentation

- [ENV_CONFIG.md](./ENV_CONFIG.md) - Environment configuration guide
- [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) - GitHub Actions deployment setup
- [CODE_REVIEW_SUMMARY.md](./CODE_REVIEW_SUMMARY.md) - Recent code changes summary

## 🔑 Key Features

- ✅ Event registration with participant tracking
- ✅ Automatic cost calculation per participant
- ✅ Payment status tracking
- ✅ Search by date, event name, or participant name
- ✅ Environment variable-based configuration
- ✅ Optional Azure Key Vault integration
- ✅ Automated CI/CD deployment

## 🌐 Endpoints

### Backend API
- Base URL: `https://nomikai-funcapp.azurewebsites.net`
- Functions:
  - `POST /api/savenomikai` - Save event
  - `GET /api/nomikai/search` - Search events
  - `POST /api/updatepaymentflags` - Update payment status

### Frontend
- URL: `https://nice-stone-031ceb100.3.azurestaticapps.net`

## 🤝 Contributing

1. Create a feature branch
2. Make changes and test locally
3. Submit a pull request
4. Wait for automated deployment after merge

## 📝 License

[Your License Here]
" 
