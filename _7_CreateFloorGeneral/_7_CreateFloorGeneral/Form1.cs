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

namespace _7_CreateFloorGeneral
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private List<string> CADLayers = new List<string>();
        private List<string> floorTypes = new List<string>();
        private List<string> wallTypes = new List<string>();
        private List<string> levels = new List<string>();
        private List<System.Windows.Forms.ComboBox> Form_comboxes ;
        private List<System.Windows.Forms.RadioButton> Form_radioes;
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public string test;

        public Form1(List<string> CADLayers_, List<Element> floorTypes_, List<Element> wallTypes_, List<Level> levels_)
        {
            foreach (Element item in floorTypes_)
                this.floorTypes.Add(item.Name);

            foreach (Element item in wallTypes_)
                this.wallTypes.Add(item.Name);

            foreach (Element item in levels_)
                this.levels.Add(item.Name);

            this.CADLayers = CADLayers_;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Form_comboxes = new List<System.Windows.Forms.ComboBox>() {
                cmbCADLayers,
                cmbTypes,
                cmbLevels,
                cmbCurvesType, 
            };

            this.Form_radioes = new List<RadioButton>()
            {
                radFloor,
                radWall,
            };
             

            foreach (string item in levels) 
                this.cmbLevels.Items.Add(item); 


            foreach (string item in CADLayers) 
                this.cmbCADLayers.Items.Add(item);

            ReadData();

        }

        private void ReadData()
        {
            string path = Path.Combine(LastIndexesSavePath, "GeneralCaseIndexs.txt");

            try
            {
                StreamReader sr = new StreamReader(path);
                List<string> indexes = new List<string>();
                while (sr.Peek() != -1)
                {
                    indexes.Add(sr.ReadLine()); 
                } 
                sr.Close();

                Form_radioes[Convert.ToInt32(indexes[0])].Checked = true;

                this.txtHeigth.Text = indexes[1];
                for (int i = 2; i < indexes.Count; i++)
                {
                    Form_comboxes[i-2].SelectedIndex = Convert.ToInt32(indexes[i]);
                }
            }
            catch (Exception)
            {
                this.radFloor.Checked = true;
                foreach (System.Windows.Forms.ComboBox item in Form_comboxes)
                {
                    item.SelectedIndex = 0;
                }
                this.txtHeigth.Text = "0";
            } 
        }


        private void SaveData()
        {
            string path = Path.Combine(LastIndexesSavePath, "GeneralCaseIndexs.txt");
            StreamWriter sw = new StreamWriter(path);

            sw.WriteLine(Form_radioes[0].Checked == true ? 0 : 1);
            sw.Flush();
            sw.WriteLine(this.txtHeigth.Text);
            sw.Flush();
            foreach (System.Windows.Forms.ComboBox item in Form_comboxes)
            {
                sw.WriteLine(item.SelectedIndex);
                sw.Flush();
            }
            sw.Close();

        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            SaveData(); 
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }


        private void radFloor_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radFloor.Checked)
            {
                this.cmbTypes.Items.Clear();
                foreach (string item in this.floorTypes)
                {
                    this.cmbTypes.Items.Add(item);
                }
                this.cmbTypes.SelectedIndex = 0;
                this.cmbCurvesType.SelectedIndex = 0;
                this.cmbCurvesType.Enabled = false;
                this.lblWall1.Visible = false;
                this.lblWall2.Visible = false;
                this.txtHeigth.Visible = false;
            }
        }

        private void radWall_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radWall.Checked)
            {
                this.cmbTypes.Items.Clear();
                foreach (string item in this.wallTypes)
                {
                    this.cmbTypes.Items.Add(item);
                }
                this.cmbTypes.SelectedIndex = 0;
                this.cmbCurvesType.Enabled = true;
                this.lblWall1.Visible = true;
                this.lblWall2.Visible = true;
                this.txtHeigth.Visible = true;
            }
        }
    }
}
