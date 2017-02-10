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
        private Dictionary<string, ActionSequence> actionSequences;

        public LoginInformation() {
            this.actionSequences = new Dictionary<string, ActionSequence>();
            ActionSequence actionSequence = new ActionSequence();
            //actionSequence.add(new Action(Action.ACTION_TYPE_INPUT, "#username", "TEST"));
            //this.actionSequences.Add("TEST", actionSequence);
        }

        public ActionSequence getActionSequence(string fingerprint) {
            try {
                return this.actionSequences[fingerprint];
            } catch (Exception e) {
                return null;
            }
        }

        public void addActionSequence(string fingerprint, ActionSequence actionSequence) {
            this.actionSequences.Add(fingerprint, actionSequence);
        }
    }
}
