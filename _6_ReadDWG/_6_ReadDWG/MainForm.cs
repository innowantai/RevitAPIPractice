using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _6_ReadDWG
{
    public partial class MainForm : Form
    {
        public int CASEName;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CASEName = -1;
        }

        private void btnCreateBeamsColumns_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            CASEName = 0;
            this.Close();
        }

        private void btnCreateFloors_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            CASEName = 1;
            this.Close();

        }
    }
}
