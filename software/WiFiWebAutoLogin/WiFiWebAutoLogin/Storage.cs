using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;

namespace WiFiWebAutoLogin {
    class Storage {
        private string fileName;
        private string password;
        private LoginInformation loginInfo;

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

            await FileIO.WriteTextAsync(file, "");

            DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof(LoginInformation) );
            string json = await FileIO.ReadTextAsync(file);
            if (json.Trim().Equals("")) {
                loginInfo = new LoginInformation("ABC");
                MemoryStream stream = new MemoryStream();
                serializer.WriteObject(stream, loginInfo);
                stream.Position = 0;
                await FileIO.WriteTextAsync(file, await (new StreamReader(stream)).ReadToEndAsync());
            }
            else {
                loginInfo = (LoginInformation)serializer.ReadObject(new MemoryStream(Encoding.Unicode.GetBytes(json)));
            }
        }
    }
}
