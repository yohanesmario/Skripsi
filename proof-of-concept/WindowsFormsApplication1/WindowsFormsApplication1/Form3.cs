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
    public partial class Form3 : Form {
        public Form3() {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e) {

        }

        public void changeText(String text) {
            this.label1.Text = text;
        }

        public void changeTitle(String text) {
            this.Text = text;
        }
    }
}
