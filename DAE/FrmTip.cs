using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DAERun
{
    public partial class FrmTip : Form
    {
        public FrmTip()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void FrmTip_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void FrmTip_FormClosing(object sender, FormClosingEventArgs e)
        {
          //  e.Cancel = true;
        }
    }
}
