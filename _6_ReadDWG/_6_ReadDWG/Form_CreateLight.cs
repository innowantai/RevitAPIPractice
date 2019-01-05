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
    public partial class Form_CreateLight : System.Windows.Forms.Form
    {
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private List<System.Windows.Forms.ComboBox> comboBoxes;
        private List<System.Windows.Forms.CheckBox> checkBoxes;
        private List<System.Windows.Forms.TextBox> txtBox;

        List<Level> levels;
        Dictionary<string, List<LINE>> CADLayers;
        Dictionary<string, List<FamilySymbol>> colFamilyTypes; 
        public List<string> returnCADLayers = new List<string>();
        public List<FamilySymbol> returnType = new List<FamilySymbol>();
        public List<Level> returnBaseLevel = new List<Level>();
        public List<Level> returnTopLevel = new List<Level>();
        public bool returnCol = false;
        public bool returnBeam = false;

        public Form_CreateLight(Dictionary<string, List<FamilySymbol>> _colFamilyTypes, 
                                List<Level> _levels,
                                Dictionary<string, List<LINE>> _CADLayers)
        {
            InitializeComponent();
            this.colFamilyTypes = _colFamilyTypes; 
            this.levels = _levels;
            this.CADLayers = _CADLayers;
        }

        private void Form_CreateLight_Load(object sender, EventArgs e)
        {
            this.checkBoxes = new List<CheckBox>();

            /// 載入燈的族群與類型名稱
            foreach (KeyValuePair<string, List<FamilySymbol>> entry in this.colFamilyTypes)
                this.cmbColFamilyType.Items.Add(entry.Key);

            /// 載入Level名稱
            foreach (Level item in this.levels)
            {
                this.cmbColBaseLevel.Items.Add(item.Name);
                this.cmbColTopLevel.Items.Add(item.Name); 
            }

            /// 載入CAD layer名稱
            foreach (KeyValuePair<string, List<LINE>> entry in this.CADLayers)
            {
                this.cmbColCADLayers.Items.Add(entry.Key); 
            } 

            /// Setting combox and checkbox list
            this.comboBoxes = new List<System.Windows.Forms.ComboBox>() { 
                        this.cmbColBaseLevel,
                        this.cmbColTopLevel, 
                        this.cmbColCADLayers,
                        this.cmbColFamilyType};

            this.txtBox = new List<System.Windows.Forms.TextBox>() { this.txtShift};



            GetLastIndexes();
        }



        private void button1_Click(object sender, EventArgs e)
        {

            returnType.Add(colFamilyTypes[cmbColFamilyType.Text][cmbColType.SelectedIndex]);
            returnBaseLevel.Add(levels[cmbColBaseLevel.SelectedIndex]);
            returnTopLevel.Add(levels[cmbColTopLevel.SelectedIndex]);
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            SaveLastIndexes();
            this.Close();
        }



        private void SaveLastIndexes()
        {
            StreamWriter sw = new StreamWriter(Path.Combine(LastIndexesSavePath, "RevitCreatedSelectedIndex_Light.txt"));
            foreach (System.Windows.Forms.ComboBox ss in this.comboBoxes)
            {
                sw.WriteLine(ss.SelectedIndex.ToString());
                sw.Flush();
            }
            sw.Close();


            StreamWriter sw2 = new StreamWriter(Path.Combine(LastIndexesSavePath, "RevitCreatedchecked_Light.txt"));
            foreach (System.Windows.Forms.CheckBox ss in this.checkBoxes)
            {
                sw2.WriteLine(ss.Checked.ToString());
                sw2.Flush();
            }
            sw2.Close();


            StreamWriter sw3 = new StreamWriter(Path.Combine(LastIndexesSavePath, "RevitCreatedShiftText_Light.txt"));
            foreach (System.Windows.Forms.TextBox ss in this.txtBox)
            {
                sw3.WriteLine(ss.Text);
                sw3.Flush();
            }
            sw3.Close();

        }

        private void GetLastIndexes()
        {
            List<int> lastIndexs = new List<int>();
            try
            {
                StreamReader sr = new StreamReader(Path.Combine(LastIndexesSavePath, "RevitCreatedSelectedIndex_Light.txt"));
                while (sr.Peek() != -1)
                {
                    lastIndexs.Add(Convert.ToInt32(sr.ReadLine()));
                }
                sr.Close();
            }
            catch (Exception)
            {
                for (int i = 0; i < this.comboBoxes.Count; i++)
                {
                    lastIndexs.Add(0);
                }
            }


            List<string> laseChecked = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(Path.Combine(LastIndexesSavePath, "RevitCreatedchecked_Light.txt"));
                while (sr.Peek() != -1)
                {
                    laseChecked.Add(sr.ReadLine());
                }
                sr.Close();

            }
            catch (Exception)
            {

                for (int i = 0; i < this.checkBoxes.Count; i++)
                {
                    laseChecked.Add("0");
                }
            }


            List<string> lastShiftText = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(Path.Combine(LastIndexesSavePath, "RevitCreatedShiftText_Light.txt"));
                while (sr.Peek() != -1)
                {
                    lastShiftText.Add(sr.ReadLine());
                }
                sr.Close();

            }
            catch (Exception)
            {

                for (int i = 0; i < this.txtBox.Count; i++)
                {
                    lastShiftText.Add("false");
                }
            }


            int kk = 0;
            foreach (System.Windows.Forms.TextBox item in this.txtBox)
            {
                try
                {
                    item.Text = lastShiftText[kk];
                }
                catch (Exception)
                {

                    item.Text = "0";
                }
                kk++;
            }

             kk = 0;
            foreach (System.Windows.Forms.ComboBox item in this.comboBoxes)
            {
                try
                {
                    item.SelectedIndex = lastIndexs[kk];
                }
                catch (Exception)
                {

                    item.SelectedIndex = 0;
                }
                kk++;
            }

            kk = 0;
            foreach (System.Windows.Forms.CheckBox item in this.checkBoxes)
            {
                item.Checked = laseChecked[kk] == "True" ? true : false;
                kk++;
            }

        }
         

        private void cmbColFamilyType_SelectedIndexChanged(object sender, EventArgs e)
        { 
            this.cmbColType.Items.Clear();
            foreach (FamilySymbol defaultType in this.colFamilyTypes[this.cmbColFamilyType.Text])
                this.cmbColType.Items.Add(defaultType.Name);
            this.cmbColType.SelectedIndex = 0;
        }
    }
}
