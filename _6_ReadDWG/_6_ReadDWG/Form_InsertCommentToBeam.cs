using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

namespace _6_ReadDWG
{
    public partial class Form_InsertCommentToBeam : System.Windows.Forms.Form
    {
        public List<Level> levels;
        private List<System.Windows.Forms.ComboBox> comboBoxes;
        private List<System.Windows.Forms.TextBox> txtBoxes;
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private List<string> CADLayers;
        public Form_InsertCommentToBeam(List<Level> levels_)
        {
            this.levels = new List<Level>();
            foreach (Level item in levels_.OrderBy(t => t.Name))
            {
                this.levels.Add(item);
            }

            InitializeComponent();
        }

        private void Form_InsertCommentToBeam_Load(object sender, EventArgs e)
        {
            /// 將Levels加入cmvLevel中
            foreach (Level level in this.levels)
            {
                cmbLevels.Items.Add(level.Name);
            }
            /// 

            this.comboBoxes = new List<System.Windows.Forms.ComboBox>() { this.cmbLevels };
            this.txtBoxes = new List<System.Windows.Forms.TextBox>() { this.txtFilePath };
            readData();
            txtFilePath.Text = txtFilePath.Text == "0" ? "D:" : txtFilePath.Text;

        }



        private void button1_Click(object sender, EventArgs e)
        {

            saveData();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            SelectedDialog();
        }




        private void txtFilePath_MouseClick(object sender, MouseEventArgs e)
        {
            SelectedDialog();
        }


        private void SelectedDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog(); //不用從工具拉，要用到才NEW
            if (string.IsNullOrEmpty(ofd.InitialDirectory))
                ofd.InitialDirectory = txtFilePath.Text;  // 預設路徑

            ofd.Filter = "Text Files (*.csv)|*.csv";//|All Files (*.*)|*.*";//"Text Files (*.txt)|*.c; *.cpp|All Files (*.*)|*.*";  //因為我要選TXT來轉EXCEL，所以預設就用TXT
            ofd.Title = "請開啟文字檔案";

            if (ofd.ShowDialog(this) == DialogResult.Cancel)
                return;
            this.txtFilePath.Text = ofd.FileName;

        }



        private void readData()
        {
            string path = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_CMB.txt");

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    int kk = 0;
                    while (sr.Peek() != -1)
                    {
                        int po = Convert.ToInt32(sr.ReadLine());
                        this.comboBoxes[kk].SelectedIndex = po;
                        kk++;
                    }
                }
            }
            catch (Exception)
            {
                foreach (System.Windows.Forms.ComboBox item in this.comboBoxes)
                {
                    if (item.Items.Count != 0)
                    {
                        item.SelectedIndex = 0;
                    }
                }
            }


            string path2 = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_TEXT.txt");

            try
            {
                using (StreamReader sr = new StreamReader(path2))
                {
                    int kk = 0;
                    while (sr.Peek() != -1)
                    {
                        this.txtBoxes[kk].Text = sr.ReadLine();
                        kk++;
                    }
                }
            }
            catch (Exception)
            {
                foreach (System.Windows.Forms.TextBox item in this.txtBoxes)
                {
                    item.Text = "0";
                }
            }
        }


        private void saveData()
        {
            string path = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_CMB.txt");
            StreamWriter sw = new StreamWriter(path);
            foreach (System.Windows.Forms.ComboBox item in this.comboBoxes)
            {
                sw.WriteLine(item.SelectedIndex);
            }
            sw.Close();


            string path2 = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_TEXT.txt");
            StreamWriter sw2 = new StreamWriter(path2);
            foreach (System.Windows.Forms.TextBox item in this.txtBoxes)
            {
                sw2.WriteLine(item.Text);
            }
            sw2.Close();
        }

    }
}
