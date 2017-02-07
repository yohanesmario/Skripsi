using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WiFiWebAutoLogin {
    [DataContract]
    class ActionSequence {
        [DataMember]
        private LinkedList<Action> actions;

        public ActionSequence() {
            this.actions = new LinkedList<Action>();
        }

        public void add(Action action) {
            this.actions.AddLast(action);
        }

        public void reset() {
            this.actions.Clear();
        }
    }
}
