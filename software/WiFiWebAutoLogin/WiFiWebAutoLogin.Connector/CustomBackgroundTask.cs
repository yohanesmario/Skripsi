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
        private static string lastSSID = "";

        public void Run(IBackgroundTaskInstance taskInstance) {
            if (this.connectionChanged() && lastSSID!=null && !this.hasInternetAccess()) {

                string xmlText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                    "<toast launch=\"app-defined-string\">" +
                        "<visual>" +
                            "<binding template=\"ToastGeneric\">" +
                                "<text>Network Detected</text>" +
                                "<text>Would you like to run WiFiWebAutoLogin?</text>" +
                            "</binding>" +
                        "</visual>" +
                        "<actions>" +
                            "<action content=\"Yes\" arguments=\"Yes\" />" +
                            "<action content=\"No\" arguments=\"No\" activationType=\"background\" />" +
                        "</actions>" +
                        "<audio src=\"ms-winsoundevent:Notification.Reminder\"/>" +
                    "</toast>";

                XmlDocument xmlContent = new XmlDocument();
                xmlContent.LoadXml(xmlText);

                ToastNotification notification = new ToastNotification(xmlContent);
                notification.Tag = "WWAL_TOAST";
                notification.Dismissed += (ToastNotification n, ToastDismissedEventArgs args) => {
                    ToastNotificationManager.History.Remove("WWAL_TOAST");
                };
                ToastNotificationManager.CreateToastNotifier().Show(notification);
            }
        }

        private bool hasInternetAccess() {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            
            if (connectionProfile != null) {
                if (connectionProfile.GetNetworkConnectivityLevel().ToString().Trim().Equals("InternetAccess")) {
                    return true;
                }
            }

            return false;
        }

        private bool connectionChanged() {
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

            if (lastSSID != null) {
                if (lastSSID.Equals(ssid)) {
                    lastSSID = ssid;
                    return false;
                }
                else {
                    lastSSID = ssid;
                    return true;
                }
            }
            else {
                if (ssid==null) {
                    lastSSID = ssid;
                    return false;
                }
                else {
                    lastSSID = ssid;
                    return true;
                }
            }
        }
    }
}
