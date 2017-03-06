using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace WiFiWebAutoLogin.Classes {
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

        public List<string> getList() {
            Dictionary<string, ActionSequence>.KeyCollection.Enumerator loginInfoEnumerator = actionSequences.Keys.GetEnumerator();
            List<string> list = new List<string>();
            while (loginInfoEnumerator.MoveNext()) {
                string ssid = loginInfoEnumerator.Current.Split(new string[] { Conf.separator }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (!list.Contains(ssid)) {
                    list.Add(ssid);
                }
            }
            return list;
        }

        public void removeBySSID(string ssid) {
            Dictionary<string, ActionSequence>.KeyCollection.Enumerator loginInfoEnumerator = actionSequences.Keys.GetEnumerator();
            List<string> removalList = new List<string>();
            while (loginInfoEnumerator.MoveNext()) {
                string enumSSID = loginInfoEnumerator.Current.Split(new string[] { Conf.separator }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (enumSSID.Equals(ssid)) {
                    removalList.Add(loginInfoEnumerator.Current);
                }
            }
            List<string>.Enumerator removalListEnumerator = removalList.GetEnumerator();
            while (removalListEnumerator.MoveNext()) {
                actionSequences.Remove(removalListEnumerator.Current);
            }
        }
    }
}
