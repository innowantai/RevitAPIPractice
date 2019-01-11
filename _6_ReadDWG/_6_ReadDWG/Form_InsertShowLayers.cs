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
    public partial class Form_InsertShowLayers : System.Windows.Forms.Form
    {
        private List<System.Windows.Forms.ComboBox> comboBoxes;
        private List<System.Windows.Forms.TextBox> txtBoxes;
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private List<string> CADLayers;
        private List<CADGeoObject> DATA;
        public List<CADGeoObject> Selected_DATA;

        public Form_InsertShowLayers(List<CADGeoObject> DATA_CAD_TEXT)
        {
            DATA = DATA_CAD_TEXT.OrderBy(t => t.Layer).ToList(); 
            this.CADLayers = new List<string>();
            foreach (CADGeoObject item in DATA)
            {
                if (!this.CADLayers.Contains(item.Layer))
                {
                    this.CADLayers.Add(item.Layer);
                }
            }
             
            InitializeComponent();
        }

        private void Form_InsertShowLayers_Load(object sender, EventArgs e)
        {
            foreach (string Layer in this.CADLayers)
            {
                cmbLayerCAD.Items.Add(Layer);
            }
            this.comboBoxes = new List<System.Windows.Forms.ComboBox>() { this.cmbLayerCAD };
            this.txtBoxes = new List<System.Windows.Forms.TextBox>() { };
            readData();
        }




        private void btnStart_Click(object sender, EventArgs e)
        {

            saveData();
            GetSelectLayerData();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        private void GetSelectLayerData()
        {
            var tmp = from tt in this.DATA
                      where tt.Layer.Trim() == this.cmbLayerCAD.SelectedItem.ToString().Trim()
                      select tt;

            this.Selected_DATA = tmp.ToList();

        }











        private void readData()
        {
            string path = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_CMB_2.txt");

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


            string path2 = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_TEXT_2.txt");

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
            string path = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_CMB_2.txt");
            StreamWriter sw = new StreamWriter(path);
            foreach (System.Windows.Forms.ComboBox item in this.comboBoxes)
            {
                sw.WriteLine(item.SelectedIndex);
            }
            sw.Close();


            string path2 = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_COMMENTtoBEAM_TEXT_2.txt");
            StreamWriter sw2 = new StreamWriter(path2);
            foreach (System.Windows.Forms.TextBox item in this.txtBoxes)
            {
                sw2.WriteLine(item.Text);
            }
            sw2.Close();
        }

    }
}
