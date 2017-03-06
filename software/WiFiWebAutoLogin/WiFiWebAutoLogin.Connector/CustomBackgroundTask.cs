using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Connectivity;
using WiFiWebAutoLogin.Classes;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.IO;

namespace WiFiWebAutoLogin.RuntimeComponents {
    public sealed class CustomBackgroundTask : IBackgroundTask {
        private BackgroundTaskDeferral _deferral;

        public async void Run(IBackgroundTaskInstance taskInstance) {
            _deferral = taskInstance.GetDeferral();

            if (this.isNetworkConnected()) {
                string xmlText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<toast launch=\"app-defined-string\">\r\n  <visual>\r\n    <binding template=\"ToastGeneric\">\r\n      <text>Network Detected</text>\r\n      <text>Would you like to run WiFiWebAutoLogin?</text>\r\n    </binding>\r\n  </visual>\r\n  <actions>\r\n    <action content=\"Yes\" arguments=\"Yes\" />\r\n    <action content=\"No\" arguments=\"No\" />\r\n  </actions>\r\n  <audio src=\"ms-winsoundevent:Notification.Reminder\"/>\r\n</toast>";
                XmlDocument xmlContent = new XmlDocument();
                xmlContent.LoadXml(xmlText);

                ToastNotification notification = new ToastNotification(xmlContent);
                ToastNotificationManager.CreateToastNotifier().Show(notification);
            }

            CaptivePortalDetector cpd = await CaptivePortalDetector.getInstance();
            if (cpd.isSetup()) {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    //this.onNetworkChange();
                });

                Debug.WriteLine("TEST");
            }
            else {
                Debug.WriteLine("Not initialized");

                _deferral.Complete();
            }
        }

        private async void onNetworkChange() {
            /*
            captive.updateSSID();
            if (ssid != null) {
                // CONNECTED
                this.webView.Navigate(new Uri(Conf.uri));
            }
            else {
                // DISCONNECTED
                this.displayMessage("Check your network connection.");
            }
            */
            CaptivePortalDetector cpd = await CaptivePortalDetector.getInstance();
            cpd.updateSSID();
            cpd.updateWebView();

            _deferral.Complete();
        }

        public bool isNetworkConnected() {
            string ssid;
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
                    ssid = null;
                }
                else {
                    ssid = data;
                }
            }
            else {
                ssid = null;
            }

            return ssid != null;
        }
    }
}
