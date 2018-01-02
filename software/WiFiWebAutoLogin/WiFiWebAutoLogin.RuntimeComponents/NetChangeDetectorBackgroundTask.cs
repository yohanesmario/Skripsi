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
    public sealed class NetChangeDetectorBackgroundTask : IBackgroundTask {
        private static string lastSSID = "";
        private static Boolean lastConnectionChanged = false;

        public void Run(IBackgroundTaskInstance taskInstance) {
            var mDeferral = taskInstance.GetDeferral();

            Debug.WriteLine("Result:");
            Debug.WriteLine(this.connectionChanged());
            Debug.WriteLine(lastSSID != null);
            Debug.WriteLine(this.hasNoInternetAccess());
            if ((lastConnectionChanged || this.connectionChanged()) && lastSSID!=null && this.hasNoInternetAccess()) {

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

            mDeferral.Complete();
        }

        private bool hasNoInternetAccess() {
            ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            
            if (connectionProfile != null) {
                if (connectionProfile.GetNetworkConnectivityLevel().ToString().Trim().Equals("InternetAccess")) {
                    return false;
                }
            }

            return true;
        }

        private void testNotification() {
            string xmlText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                    "<toast launch=\"app-defined-string\">" +
                        "<visual>" +
                            "<binding template=\"ToastGeneric\">" +
                                "<text>WiFiWebAutoLogin</text>" +
                            "</binding>" +
                        "</visual>" +
                        "<audio src=\"ms-winsoundevent:Notification.Reminder\"/>" +
                    "</toast>";

            XmlDocument xmlContent = new XmlDocument();
            xmlContent.LoadXml(xmlText);

            ToastNotification notification = new ToastNotification(xmlContent);
            notification.Tag = "WWAL_TOAST_TEST";
            notification.Dismissed += (ToastNotification n, ToastDismissedEventArgs args) => {
                ToastNotificationManager.History.Remove("WWAL_TOAST_TEST");
            };
            ToastNotificationManager.CreateToastNotifier().Show(notification);
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
                    lastConnectionChanged = false;
                    return false;
                }
                else {
                    lastSSID = ssid;
                    lastConnectionChanged = true;
                    return true;
                }
            }
            else {
                if (ssid==null) {
                    lastSSID = ssid;
                    lastConnectionChanged = false;
                    return false;
                }
                else {
                    lastSSID = ssid;
                    lastConnectionChanged = true;
                    return true;
                }
            }
        }
    }
}
