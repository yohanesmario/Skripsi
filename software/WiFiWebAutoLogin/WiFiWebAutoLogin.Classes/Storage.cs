﻿using System;
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

            string json = await FileIO.ReadTextAsync(file);
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
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file;
            try {
                file = await folder.GetFileAsync(this.fileName);
            }
            catch (Exception e) {
                file = await folder.CreateFileAsync(this.fileName);
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LoginInformation));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, loginInfo);
            stream.Position = 0;
            await FileIO.WriteTextAsync(file, await (new StreamReader(stream)).ReadToEndAsync());
        }
    }
}
