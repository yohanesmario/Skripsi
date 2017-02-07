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
            this.setup();
        }

        public async void setup() {
            cpd = await CaptivePortalDetector.getInstance();
            if (!cpd.isSetup()) {
                await cpd.setup(MainWebView);
            }
            else {
                MainWebView = cpd.getWebView();
            }
        }

        private async void MainWebView_LoadCompleted(object sender, NavigationEventArgs e) {
            if (cpd != null) {
                MemoryStream stream = new MemoryStream();
                StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile file = await InstallationFolder.GetFileAsync(@"JavaScript\DeployListeners.js");
                string js = await FileIO.ReadTextAsync(file);
                await MainWebView.InvokeScriptAsync("eval", new string[] { js });
            }
        }
    }
}
