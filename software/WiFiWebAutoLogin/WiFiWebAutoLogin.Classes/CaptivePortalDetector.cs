using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private ComboBox comboBox;

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

        public void setup(WebView webView, TextBlock textBlock, ComboBox comboBox) {
            this.webView = webView;
            this.textBlock = textBlock;
            this.comboBox = comboBox;

            this.refreshList();

            this.updateSSID();
            this.updateWebView();
        }

        public void refreshList() {
            comboBox.ItemsSource = this.storage.getLoginInfo().getList();
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
                    this.refreshList();
                }

                if (!body.Trim().Equals("connected")) {
                    // Not Connected

                    if (hasActionSequence) {
                        this.displayMessage("Executing recorded actions...\r\n\r\n" + "(" + this.currentFingerprint.Split(new string[] { Conf.separator }, StringSplitOptions.RemoveEmptyEntries)[1] + ")");
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
                        this.startTimer();
                    }

                    if (!hasActionSequence) {
                        this.displayWebView();
                    }
                    else {
                        this.startRetryTimer(this.currentFingerprint);
                    }

                    HttpClient client = new HttpClient();
                    string result = await client.GetStringAsync(new Uri(await this.getUri()));
                    Debug.WriteLine(result);
                }
                else {
                    // Connected
                    this.displayMessage("Connected.");
                    this.uriQueue.Clear();
                }
            }
        }

        public void navigationStarting() {
            if (this.timer!=null) {
                this.timer.Cancel();
                this.timer = null;
            }
        }

        private void startRetryTimer(string oldFingerprint) {
            ThreadPoolTimer.CreateTimer(async (source) => {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    if (this.currentFingerprint.Equals(oldFingerprint)) {
                        this.displayWebView();
                    }
                });
            }, TimeSpan.FromSeconds(5));
        }

        private async void timerCallback() {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                if (this.timer!=null) {
                    this.timer = null;
                }
                this.dequeueUri();
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
            return this.ssid + Conf.separator + uri + Conf.separator + title;
        }

        private async Task<string> getUri() {
            string uri = "";
            try {
                uri = await this.webView.InvokeScriptAsync("eval", new string[] { "window.location.href;" });
            }
            catch (Exception e) {
            }
            return uri;
        }

        private async Task<string> getBody() {
            return await this.webView.InvokeScriptAsync("eval", new string[] { "document.body.innerHTML;" });
        }

        private async Task<string> getScripts() {
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
                Debug.WriteLine("[NETWORK]: "+connectionProfile.GetNetworkConnectivityLevel().ToString());

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
                this.startTimer();
            }
        }

        private void startTimer() {
            this.timer = ThreadPoolTimer.CreateTimer((source) => {
                this.timerCallback();
            }, TimeSpan.FromSeconds(1));
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

        public void timeout() {
            this.displayMessage("Operation timeout.\r\nCheck your network connection.");
        }

        public void removeLoginInformation(string ssid) {
            if (ssid != null) {
                this.storage.getLoginInfo().removeBySSID(ssid);
                this.storage.saveData();
                this.refreshList();
            }
        }
    }
}
