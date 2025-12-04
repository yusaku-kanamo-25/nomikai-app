# Frontend Environment Configuration

## .env.local (Development)
Create this file in the `/frontend` directory for local development:

```
VITE_API_URL=https://nomikai-funcapp.azurewebsites.net
```

## .env.production
Create this file for production build:

```
VITE_API_URL=https://nomikai-funcapp.azurewebsites.net
```

## Using Environment Variables in Vue.js

Update your `script.js` to use environment variables instead of hardcoded URLs:

```javascript
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://nomikai-funcapp.azurewebsites.net';

// Then replace hardcoded URLs like:
// Before: 'https://nomikai-funcapp.azurewebsites.net/api/savenomikai'
// After:  `${API_BASE_URL}/api/savenomikai`
```

## Vite Configuration (vite.config.js)

Create a `vite.config.js` file in `/frontend`:

```javascript
import { defineConfig } from 'vite'

export default defineConfig({
  define: {
    __VITE_API_URL__: JSON.stringify(process.env.VITE_API_URL || 'https://nomikai-funcapp.azurewebsites.net')
  }
})
```

## Building for Azure Static Web Apps

```bash
cd frontend
npm install
npm run build:azure
```

Output will be in `dist/` directory.
