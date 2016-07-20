using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;

namespace WindowsFormsApplication1 {
    public partial class Form1 : Form {
        public static bool initialized;
        public static bool wwlDetected;
        public static bool loginTried;
        private bool allowshowdisplay = false;
        private Form3 waitingMessage;

        public Form1() {
            InitializeComponent();
            Form1.initialized = false;
            Form1.wwlDetected = false;
            Form1.loginTried = false;

            this.waitingMessage = new Form3();
            this.waitingMessage.Show();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            if (!this.webBrowser1.DocumentText.Equals("connected") && !Form1.initialized) {
                this.waitingMessage.changeText("Wifi Web Login detected. Reading configuration...");
                Form1.wwlDetected = true;
            }
            else {
                if (this.webBrowser1.DocumentText.Equals("connected")) {
                    //this.webBrowser1.Url = new System.Uri("http://yohanesmario.com/testingSkripsi.php", System.UriKind.Absolute);
                    base.SetVisibleCore(false);

                    WlanClient wlan = new WlanClient();
                    Wlan.Dot11Ssid ssid;
                    String ssidString = "";
                    foreach (WlanClient.WlanInterface wlanInterface in wlan.Interfaces) {
                        ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                        ssidString = new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));
                    }

                    if (!this.waitingMessage.IsDisposed) this.waitingMessage.Close();

                    MessageBox.Show("You are connected to the internet via " + ssidString + ".", "Connected");

                    this.Opacity = 0;
                    base.SetVisibleCore(true);
                    this.Close();
                }
                else if (this.webBrowser1.DocumentText.Contains("<title>TESTING</title>")) {
                    foreach (HtmlElement el in this.webBrowser1.Document.GetElementsByTagName("button")) {
                        if (el.GetAttribute("name").Equals("button")) {
                            el.InvokeMember("Click");
                        }
                    }

                    /*
                    Form2 popup1 = new Form2();
                    popup1.setWebBrowserUrl("http://google.com");
                    popup1.Show();

                    Form2 popup2 = new Form2();
                    popup2.setWebBrowserUrl("http://yohanesmario.com");
                    popup2.Show();

                    Form2 popup3 = new Form2();
                    popup3.setWebBrowserUrl("http://tommylie.com");
                    popup3.Show();
                    */
                }
            }

            if (Form1.wwlDetected && !this.webBrowser1.IsDisposed) {
                if (this.webBrowser1.Document.Url.ToString().Contains("res://ieframe.dll")) {
                    if (!this.waitingMessage.IsDisposed) this.waitingMessage.Close();

                    MessageBox.Show("Please check your connection. If problem persist, contact developer.", "Unable to Connect");

                    this.Opacity = 0;
                    base.SetVisibleCore(true);
                    this.Close();
                }
                else {
                    //TODO: Read configuration and generate the coresponding config objects
                    Config conf = null;
                    Config[] configs = Config.readFromFile();

                    foreach (Config config in configs) {
                        if (config.identify(this.webBrowser1.DocumentText)) {
                            conf = config;
                        }
                    }

                    if (conf == null) { //Config not found
                        //TODO: Detect 
                        this.Text = "Page not recognized: " + this.webBrowser1.Url.ToString();
                        if (!this.waitingMessage.IsDisposed) this.waitingMessage.Close();
                        base.SetVisibleCore(true);
                    }
                    else if (conf != null && Form1.loginTried) { //Failed login attempt
                        this.Text = this.webBrowser1.Url.ToString() + " - Login Failed!";
                        if (!this.waitingMessage.IsDisposed) this.waitingMessage.Close();
                        base.SetVisibleCore(true);
                    }
                    else {
                        Form1.loginTried = true;
                        //TODO: Try to login using configuration

                        for (int i = 0; i < conf.actionType.Length; i++) {
                            if (conf.actionType[i]==Config.ACTION_TYPE_FILL) {
                                this.webBrowser1.Document.GetElementsByTagName(conf.actionTag[i])[conf.actionIndex[i]].SetAttribute("value", conf.actionValue[i]);
                            }
                            else if (conf.actionType[i]==Config.ACTION_TYPE_CLICK) {
                                this.webBrowser1.Document.GetElementsByTagName(conf.actionTag[i])[conf.actionIndex[i]].InvokeMember("Click");
                            }
                        }

                        //if (!this.waitingMessage.IsDisposed) this.waitingMessage.Close();
                        //MessageBox.Show("Login hasn't been implemented.", "Not Yet Implemented");
                        //this.Opacity = 0;
                        //base.SetVisibleCore(true);
                        //this.Close();
                    }
                }
            }

            Form1.initialized = true;
        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }
    }
}
