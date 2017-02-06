using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;

namespace WiFiWebAutoLogin {
    class Storage {
        private string fileName;
        private PasswordVault vault;

        public Storage(string fileName) {
            this.fileName = new String(fileName.ToCharArray());
            this.vault = new PasswordVault();

            string pass;
            try {
                pass = this.vault.Retrieve(Conf.resource, Conf.username).Password;
            }
            catch (Exception e) {
                this.vault.Add(new PasswordCredential(Conf.resource, Conf.username, CryptographicBuffer.EncodeToBase64String(CryptographicBuffer.GenerateRandom(64))));
            }
        }

        public string getPassword() {
            return this.vault.Retrieve(Conf.resource, Conf.username).Password;
        }
    }
}
