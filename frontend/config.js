// アプリケーション設定
// 環境間での切り替えを容易にする設定ファイル
const config = {
  // API Base URL - 環境変数またはデフォルト値から取得
  // Azure Functions App: nomikai-funcapp
  // 注意: Azure の新しい URL 形式を使用
  apiBaseUrl: window.ENV?.API_BASE_URL || 'https://nomikai-funcapp-g4hdgyachmhwfxbq.japaneast-01.azurewebsites.net',
  
  // 指定されたエンドポイントの完全な API URL を取得
  getApiUrl: function(endpoint) {
    return `${this.apiBaseUrl}${endpoint}`;
  }
};

// グローバルに設定を公開
window.appConfig = config;
