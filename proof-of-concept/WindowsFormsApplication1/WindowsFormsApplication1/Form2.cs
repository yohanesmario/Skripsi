using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1 {
    public partial class Form2 : Form {
        public Form2() {
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            this.Text = this.webBrowser1.Url.ToString();
        }

        public void setWebBrowserUrl(String url) {
            this.webBrowser1.Url = new System.Uri(url, System.UriKind.Absolute);
        }
    }
}
