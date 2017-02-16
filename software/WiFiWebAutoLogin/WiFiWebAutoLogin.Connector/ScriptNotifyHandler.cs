using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;

namespace WiFiWebAutoLogin.Connector
{
    [AllowForWeb]
    public sealed class ScriptNotifyHandler
    {
        public async void passAction(string args) {
            CaptivePortalDetector cpd = await CaptivePortalDetector.getInstance();
            cpd.passAction(args);
        }

        public async void windowOpen(string args) {
            CaptivePortalDetector cpd = await CaptivePortalDetector.getInstance();
            cpd.queueUri(new Uri(args));
        }
    }
}
