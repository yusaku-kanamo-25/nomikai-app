// Configuration for the application
// This allows for easy switching between environments
const config = {
  // API Base URL - can be overridden by environment variable or setting
  apiBaseUrl: window.ENV?.API_BASE_URL || 'https://nice-stone-031ceb100.3.azurestaticapps.net',
  
  // Get the full API URL for a given endpoint
  getApiUrl: function(endpoint) {
    return `${this.apiBaseUrl}${endpoint}`;
  }
};

// Make config available globally
window.appConfig = config;
