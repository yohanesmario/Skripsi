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
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WiFiWebAutoLogin.Classes {
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

        public async Task setup() {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            try {
                file = await folder.GetFileAsync(this.fileName);
            } catch (Exception e) {
                file = await folder.CreateFileAsync(this.fileName);
            }

            IBuffer encryptedJson = await FileIO.ReadBufferAsync(file);
            SymmetricKeyAlgorithmProvider algorithmProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            IBuffer bufferedPassword = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
            IBuffer decryptedJson = CryptographicEngine.Decrypt(algorithmProvider.CreateSymmetricKey(bufferedPassword), encryptedJson, bufferedPassword);
            DataReader dataReader = Windows.Storage.Streams.DataReader.FromBuffer(decryptedJson);
            string json = dataReader.ReadString(decryptedJson.Length);

            if (json.Trim().Equals("")) {
                loginInfo = new LoginInformation();
                this.saveData();
            }
            else {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoginInformation));
                loginInfo = (LoginInformation)serializer.ReadObject(new MemoryStream(Encoding.Unicode.GetBytes(json)));
            }
        }

        public LoginInformation getLoginInfo() {
            return this.loginInfo;
        }

        public async void saveData() {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoginInformation));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, loginInfo);
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);
            string data = sr.ReadToEnd();

            SymmetricKeyAlgorithmProvider algorithmProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            IBuffer keyMaterial = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
            IBuffer bufferedData = CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(data));
            CryptographicKey key = algorithmProvider.CreateSymmetricKey(keyMaterial);
            IBuffer encryptedData = CryptographicEngine.Encrypt(key, bufferedData, keyMaterial);

            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            try {
                file = await folder.GetFileAsync(this.fileName);
            }
            catch (Exception e) {
                file = await folder.CreateFileAsync(this.fileName);
            }
            
            await FileIO.WriteBufferAsync(file, encryptedData);
        }
    }
}
