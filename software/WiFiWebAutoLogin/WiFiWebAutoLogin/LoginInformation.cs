using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace WiFiWebAutoLogin {
    [DataContract]
    class LoginInformation {
        [DataMember]
        private string text;

        public LoginInformation(string text) {
            this.text = text;
        }
    }
}
