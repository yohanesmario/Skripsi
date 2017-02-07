using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;

namespace WiFiWebAutoLogin {
    class Storage {
        private string fileName;
        private string password;

        public Storage(string fileName) {
            this.fileName = new String(fileName.ToCharArray());
            PasswordVault vault = new PasswordVault();

            try {
                this.password = vault.Retrieve(Conf.resource, Conf.username).Password;
            }
            catch (Exception e) {
                vault.Add(new PasswordCredential(Conf.resource, Conf.username, CryptographicBuffer.EncodeToBase64String(CryptographicBuffer.GenerateRandom(64))));
                this.password = vault.Retrieve(Conf.resource, Conf.username).Password;
            }
        }

        public string getPassword() {
            return new string(this.password.ToCharArray());
        }

        public async Task setup() {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            try {
                file = await folder.GetFileAsync(this.fileName);
            } catch (Exception e) {
                file = await folder.CreateFileAsync(this.fileName);
                await FileIO.WriteTextAsync(file, this.password);
            }
        }
    }
}
