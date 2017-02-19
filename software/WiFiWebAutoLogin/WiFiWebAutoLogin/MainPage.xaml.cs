using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Reflection;
using WiFiWebAutoLogin.Connector;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WiFiWebAutoLogin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CaptivePortalDetector cpd = null;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 600, Height = 150 });
            ApplicationView.PreferredLaunchViewSize = new Size(600, 150);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            this.setup();
        }

        private async void setup() {
            cpd = await CaptivePortalDetector.getInstance();
            if (!cpd.isSetup()) {
                cpd.setup(MainWebView, textBlock);
            }
            else {
                MainWebView = cpd.getWebView();
            }
            textBlock.Text = "Detecting captive portal...";
        }

        private void MainWebView_LoadCompleted(object sender, NavigationEventArgs e) {
            if (cpd != null) {
                cpd.onLoad();
            }
        }

        private void MainWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {
            if (cpd != null) {
                ScriptNotifyHandler scriptNotify = new ScriptNotifyHandler();
                MainWebView.AddWebAllowedObject("ScriptNotifyHandler", scriptNotify);
            }
        }

        private void MainWebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args) {
            args.Handled = true;
            cpd.queueUri(args.Uri);
        }

        private async void MainWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {
            if (cpd != null) {
                await MainWebView.InvokeScriptAsync("eval", new string[] { "window.open = function(url){ScriptNotifyHandler.windowOpen(url)};" });
            }
        }
    }
}
