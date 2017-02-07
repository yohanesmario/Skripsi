using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace WiFiWebAutoLogin {
    public class CaptivePortalDetector {
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
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(@"JavaScript\DeployListeners.js");
            string js = await FileIO.ReadTextAsync(file);
            await this.webView.InvokeScriptAsync("eval", new string[] { js });
        }

        public async void notify(string args) {
            await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML = '" + args + "';" });
        }
    }
}
