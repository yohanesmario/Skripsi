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
using WiFiWebAutoLogin.RuntimeComponents;
using Windows.UI.ViewManagement;
using WiFiWebAutoLogin.Classes;
using System.Diagnostics;
using Windows.System.Threading;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WiFiWebAutoLogin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CaptivePortalDetector cpd = null;
        private ThreadPoolTimer timeoutTimer = null;
        private bool loaded = true;

        private bool settings = true;

        public MainPage()
        {
            this.InitializeComponent();
            settings = true;
            this.toggleSettings();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 600, Height = 150 });
            ApplicationView.PreferredLaunchViewSize = new Size(600, 150);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            MainWebView.Margin = new Thickness(0, int.MaxValue, 0, int.MinValue);
            textBlock.Text = "Initializing...";
            this.setup();
        }

        private void toggleSettings() {
            if (settings == true) {
                settings = false;
                textBlock1.Opacity = 0;
                button.Opacity = 0;
                comboBox.Opacity = 0;
                button1.Content = "Show Settings";
            } else {
                settings = true;
                textBlock1.Opacity = 1;
                button.Opacity = 1;
                comboBox.Opacity = 1;
                button1.Content = "Hide Settings";
            }
        }

        private async void setup() {
            cpd = await CaptivePortalDetector.getInstance();
            cpd.setup(MainWebView, textBlock, comboBox);
            Debug.WriteLine("TEST SETUP");
        }

        private void MainWebView_LoadCompleted(object sender, NavigationEventArgs e) {
            if (cpd != null) {
                this.loaded = true;
                cpd.onLoad();
            }
        }

        private void MainWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {
            Debug.WriteLine(args.Uri);
            if (cpd != null) {
                this.cpd.navigationStarting();

                if (this.timeoutTimer!=null) {
                    this.timeoutTimer.Cancel();
                    this.timeoutTimer = null;
                }

                ScriptNotifyHandler scriptNotify = new ScriptNotifyHandler();
                MainWebView.AddWebAllowedObject("ScriptNotifyHandler", scriptNotify);

                // Handle Timeout
                this.loaded = false;
                this.timeoutTimer = ThreadPoolTimer.CreateTimer(async (source) => {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                        this.timeoutTimer = null;
                        if (!this.loaded) {
                            this.cpd.timeout();
                        }
                    });
                }, TimeSpan.FromSeconds(20));
            }
        }

        private void MainWebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args) {
            args.Handled = true;
            cpd.queueUri(args.Uri);
        }

        private async void MainWebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {
            if (cpd != null) {
                await sender.InvokeScriptAsync("eval", new string[] {
                    "window.open = function(url){ScriptNotifyHandler.windowOpen(url)};" +
                    "var open = window.open;" +
                    "document.open = window.open;"
                });
            }
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            cpd.removeLoginInformation((string)comboBox.SelectedItem);
        }

        private void MainWebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args) {
            // DISABLED
        }

        private void MainWebView_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args) {
            // DISABLED
        }

        private void MainWebView_LongRunningScriptDetected(WebView sender, WebViewLongRunningScriptDetectedEventArgs args) {
            // DISABLED
        }

        private void MainWebView_PermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args) {
            // DISABLED
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            this.toggleSettings();
        }
    }
}
