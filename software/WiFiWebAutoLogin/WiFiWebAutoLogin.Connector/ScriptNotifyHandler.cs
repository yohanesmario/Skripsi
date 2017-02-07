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
        public async void notify(string args) {
            CaptivePortalDetector cpd = await CaptivePortalDetector.getInstance();
            cpd.notify(args);
        }
    }
}
