using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace WiFiWebAutoLogin {
    class CaptivePortalDetector {
        private static CaptivePortalDetector instance = null;
        private WebView webView;
        private Storage storage;

        private CaptivePortalDetector() {
            this.webView = null;
            this.storage = new Storage("credentials.dat");
        }

        public bool isSetup() {
            return this.webView != null;
        }

        public async Task setup(WebView webView) {
            this.webView = webView;

            this.webView.Navigate(new Uri(Conf.uri));
        }

        public WebView getWebView() {
            return this.webView;
        }

        public static async Task<CaptivePortalDetector> getInstance() {
            if (instance==null) {
                instance = new CaptivePortalDetector();
                await instance.storage.setup();
            }
            return instance;
        }

        public async void onLoad() {
            string[] args = new string[1];
            args[0] = "alert('TEST!');";
            await this.webView.InvokeScriptAsync("eval", args);
        }
    }
}
