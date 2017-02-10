using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
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
        private string ssid;

        private string currentFingerprint;
        private ActionSequence currentActionSequence;

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
            // GET FINGERPRINT
            this.currentFingerprint = await this.getFingerprint();
            string body = await this.getBody();

            this.currentActionSequence = this.storage.getLoginInfo().getActionSequence(this.currentFingerprint);
            if (this.currentActionSequence==null) {
                this.currentActionSequence = new ActionSequence();
                this.storage.getLoginInfo().addActionSequence(this.currentFingerprint, this.currentActionSequence);
                this.storage.saveData();
            }

            if (!body.Trim().Equals("connected")) {
                // Not Connected
                StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile file = await InstallationFolder.GetFileAsync(@"JavaScript\DeployListeners.js");
                string js = await FileIO.ReadTextAsync(file);
                await this.webView.InvokeScriptAsync("eval", new string[] { js });
                //await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML = " + (await this.EscapeJSONString(WebUtility.HtmlEncode(this.currentFingerprint))) });
            }
            else {
                // Connected
                
            }
        }

        private async Task<string> getFingerprint() {
            string uri = await this.webView.InvokeScriptAsync("eval", new string[] { "window.location.href;" });
            //string html = await this.webView.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            string title = await this.webView.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName(\"title\")[0].innerHTML;" });
            //html = Regex.Replace(html, @"\s+", "");
            //title = Regex.Replace(title, @"\s+", "");
            title = title.Trim();
            return this.ssid + "::" + uri + "::" + title;
        }

        private async Task<string> getBody() {
            return await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML;" });
        }

        public async void notify(string args) {
            if (this.currentActionSequence!=null) {
                this.currentActionSequence.add(args);
                this.storage.saveData();
            }
            //await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML = '" + args + "';" });
        }

        private void updateSSID() {
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
                    this.ssid = null;
                }
                else {
                    this.ssid = data;
                }
            }
            else {
                this.ssid = null;
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
            this.updateSSID();
            if (this.ssid != null) {
                // CONNECTED
                this.webView.Navigate(new Uri(Conf.uri));
            }
            else {
                // DISCONNECTED
                this.webView.NavigateToString("<br /><br /><br />NO CONNECTION");
            }
        }

        private async Task<string> EscapeJSONString(string unescaped) {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoginInformation));
            MemoryStream stream = new MemoryStream(); ;
            serializer.WriteObject(stream, unescaped);
            stream.Position = 0;
            return await (new StreamReader(stream)).ReadToEndAsync();
        }
    }
}
