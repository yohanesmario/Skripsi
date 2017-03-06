using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WiFiWebAutoLogin.Classes {
    public class CaptivePortalDetector {
        private static CaptivePortalDetector instance = null;
        private Storage storage;
        private string ssid;
        private Queue<Uri> uriQueue;
        
        private WebView webView;
        private TextBlock textBlock;

        private string currentFingerprint;
        private ActionSequence currentActionSequence;
        private ThreadPoolTimer timer;

        private CaptivePortalDetector() {
            this.webView = null;
            this.storage = new Storage("credentials.dat");
            this.uriQueue = new Queue<Uri>();
        }

        public bool isSetup() {
            return this.webView != null;
        }

        public void setup(WebView webView, TextBlock textBlock) {
            this.webView = webView;
            this.textBlock = textBlock;

            //this.startNetworkProbing();

            this.updateSSID();
            this.updateWebView();
        }

        public WebView getWebView() {
            return this.webView;
        }

        public void updateWebView() {
            if (this.ssid != null) {
                // CONNECTED
                this.webView.Navigate(new Uri(Conf.uri));
            }
            else {
                // DISCONNECTED
                this.displayMessage("Check your network connection.");
            }
        }

        public static async Task<CaptivePortalDetector> getInstance() {
            if (instance==null) {
                instance = new CaptivePortalDetector();
                await instance.storage.setup();
            }
            return instance;
        }

        public async void onLoad() {
            if (this.ssid!=null) {
                // GET FINGERPRINT
                this.currentFingerprint = await this.getFingerprint();
                string body = await this.getBody();

                this.currentActionSequence = this.storage.getLoginInfo().getActionSequence(this.currentFingerprint);
                bool hasActionSequence = true;
                if (this.currentActionSequence == null) {
                    hasActionSequence = false;
                    this.currentActionSequence = new ActionSequence();
                    this.storage.getLoginInfo().addActionSequence(this.currentFingerprint, this.currentActionSequence);
                    this.storage.saveData();
                }

                if (!body.Trim().Equals("connected")) {
                    // Not Connected

                    if (hasActionSequence) {
                        this.displayMessage("Executing recorded actions...\r\n\r\n" + "(" + this.currentFingerprint.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[1] + ")");
                    }

                    IEnumerable<string> actions = this.currentActionSequence.getEnumerable();
                    string compiledActions = "";
                    foreach (string action in actions) {
                        compiledActions += action;
                    }
                    await this.webView.InvokeScriptAsync("eval", new string[] { compiledActions });

                    // Deploy Listeners
                    this.deployListeners();
                    //await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML = " + (await this.EscapeJSONString(WebUtility.HtmlEncode(this.currentFingerprint))) });
                    if (this.uriQueue.Count > 0) {
                        this.startTimer(this.currentFingerprint);
                    }

                    if (!hasActionSequence) {
                        this.displayWebView();
                    }
                    else {
                        this.startRetryTimer(this.currentFingerprint);
                    }
                }
                else {
                    // Connected
                    this.displayMessage("Connected.");
                }
            }
        }

        private void startRetryTimer(object oldFingerprint) {
            ThreadPoolTimer.CreateTimer(async (source) => {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    if (this.currentFingerprint.Equals(oldFingerprint)) {
                        this.displayWebView();
                    }
                });
            }, TimeSpan.FromSeconds(5));
        }

        private async void timerCallback(object oldFingerprint) {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                if (((string)oldFingerprint).Equals(await this.getFingerprint())) {
                    this.dequeueUri();
                }
                else {
                    this.webView.Navigate(new Uri("http://107.172.253.111/network_status.html"));
                }
            });
        }

        private async void deployListeners() {
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(@"JavaScript\DeployListeners.js");
            string js = await FileIO.ReadTextAsync(file);
            await this.webView.InvokeScriptAsync("eval", new string[] { js });
        }

        private async Task<string> getFingerprint() {
            string uri = "";
            string title = "";
            try {
                uri = await this.webView.InvokeScriptAsync("eval", new string[] { "window.location.href;" });
                title = await this.webView.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName(\"title\")[0].innerHTML.trim();" });
            } catch (Exception e) {
            }
            return this.ssid + "::" + uri + "::" + title;
        }

        private async Task<string> getBody() {
            return await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML;" });
        }

        public void passAction(string args) {
            if (this.currentActionSequence!=null) {
                this.currentActionSequence.add(args);
                this.storage.saveData();
            }
            //await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML = '" + args + "';" });
        }

        public void updateSSID() {
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

        public string getSSID() {
            return this.ssid;
        }

        private void dequeueUri() {
            Uri uri;
            try {
                uri = this.uriQueue.Dequeue();
            } catch (Exception e) {
                uri = null;
            }

            if (uri!=null) {
                
                this.webView.Navigate(uri);
            }
        }

        private async Task<string> EscapeJSONString(string unescaped) {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoginInformation));
            MemoryStream stream = new MemoryStream(); ;
            serializer.WriteObject(stream, unescaped);
            stream.Position = 0;
            return await (new StreamReader(stream)).ReadToEndAsync();
        }

        public void queueUri(Uri uri) {
            this.uriQueue.Enqueue(uri);
            if (this.uriQueue.Count == 1) {
                this.startTimer(this.currentFingerprint);
            }
        }

        private void startTimer(string cf) {
            this.timer = ThreadPoolTimer.CreateTimer((source) => {
                this.timerCallback(cf);
            }, TimeSpan.FromSeconds(5));
        }

        private void displayMessage(string message) {
            ApplicationView.GetForCurrentView().TryResizeView(new Size { Width = 600, Height = 150 });
            this.textBlock.Text = message;
            this.webView.Margin = new Thickness(0, int.MaxValue, 0, int.MinValue);
        }

        private void displayWebView() {
            ApplicationView.GetForCurrentView().TryResizeView(new Size { Width = 800, Height = 500 });
            this.textBlock.Text = "";
            this.webView.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
