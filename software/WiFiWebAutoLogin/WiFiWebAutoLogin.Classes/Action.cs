using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace WiFiWebAutoLogin.Classes {
    [DataContract]
    class Action {
        public static readonly int ACTION_TYPE_CLICK = 0;
        public static readonly int ACTION_TYPE_INPUT = 1;

        [DataMember]
        private int type;
        [DataMember]
        private string query;
        [DataMember]
        private string value;

        public Action(int type, string query, string value) {
            this.type = type;
            this.query = query;
            this.value = value;
        }

        public int getType() {
            return this.type;
        }

        public string getQuery() {
            return this.query;
        }

        public string getValue() {
            return this.value;
        }
    }
}
