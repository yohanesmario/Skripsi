using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;
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

            this.startNetworkProbing();
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
            //await this.webView.InvokeScriptAsync("eval", new string[] { js });
        }

        public async void notify(string args) {
            await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML = '" + args + "';" });
        }

        private string getSSID() {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            string data = "";
            if (connectionProfile != null) {
                IEnumerable<string> enumerable = connectionProfile.GetNetworkNames().AsEnumerable();
                foreach (string v in enumerable) {
                    if (data.Equals("")) {
                        data += v;
                    }
                    else {
                        data += " | " + v;
                    }
                }
                if (data.Equals("")) {
                    return null;
                }
                else {
                    return data;
                }
            }
            else {
                return null;
            }
        }

        private void startNetworkProbing() {
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkEventHandler);
            this.onNetworkChange();
        }

        private async void NetworkEventHandler(object sender, EventArgs e) {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                this.onNetworkChange();
            });
        }

        private void onNetworkChange() {
            string ssid = this.getSSID();
            if (ssid == null) {
                this.webView.NavigateToString("<br /><br /><br />NO CONNECTION");
            }
            else {
                this.webView.NavigateToString("<br /><br /><br />" + ssid);
            }
        }
    }
}
