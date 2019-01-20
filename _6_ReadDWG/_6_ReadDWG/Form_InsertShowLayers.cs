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
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private List<string> CADLayers;
        private List<string> SelectedCADLayers;
        private List<CADGeoObject> DATA;
        public List<CADGeoObject> Selected_DATA;

        public Form_InsertShowLayers(List<CADGeoObject> DATA_CAD_TEXT)
        {
            DATA = DATA_CAD_TEXT.OrderBy(t => t.Layer).ToList();
            this.CADLayers = new List<string>();
            this.SelectedCADLayers = new List<string>();
            this.Selected_DATA = new List<CADGeoObject>();
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
            List<string> PastData = readData();
            foreach (string Layer in this.CADLayers)
            {
                if (PastData.Contains(Layer))
                {
                    this.SelectedCADLayers.Add(Layer);
                }
                else
                { 
                    this.listOri.Items.Add(Layer);
                }
            }


            foreach (string Layer in this.SelectedCADLayers)
            {
                listSelected.Items.Add(Layer);
            }



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
            foreach (string layer in this.listSelected.Items)
            {
                List<CADGeoObject> tmpData = GetSelectLayerDatas(layer);
                foreach (CADGeoObject item in tmpData)
                {
                    this.Selected_DATA.Add(item);
                }
            } 
        }

        private List<CADGeoObject> GetSelectLayerDatas(string Layers)
        {
            var tmp = from tt in this.DATA
                      where tt.Layer.Trim() == Layers
                      select tt;

            return tmp.ToList();

        }
       


        private List<string> readData()
        {
            string path = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_PAST_SELECTED_ITEM.txt");
            List<string> tmpData = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.Peek() != -1)
                    {
                        tmpData.Add(sr.ReadLine());
                    }
                }
            }
            catch (Exception)
            {
            }

            return tmpData;

        }

        private void saveData()
        {
            string path = Path.Combine(LastIndexesSavePath, "REVIT_INSERT_PAST_SELECTED_ITEM.txt");
            StreamWriter sw = new StreamWriter(path);
            foreach (string item in this.listSelected.Items)
            {
                sw.WriteLine(item);
            }
            sw.Close();
        }

        private void listOriLayers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.CADLayers.Count != 0 && listOri.SelectedIndex != -1)
            {
                this.SelectedCADLayers.Add(this.CADLayers[listOri.SelectedIndex]);
                this.CADLayers.Remove(this.CADLayers[listOri.SelectedIndex]);
                this.CADLayers.Sort();
                this.SelectedCADLayers.Sort();
                this.listOri.Items.Clear();

                foreach (string item in this.CADLayers)
                {
                    this.listOri.Items.Add(item);
                }

                this.listSelected.Items.Clear();
                foreach (string item in this.SelectedCADLayers)
                {
                    this.listSelected.Items.Add(item);
                }

            }
        }

        private void listSelectedLayers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.SelectedCADLayers.Count != 0 && this.listSelected.SelectedIndex != -1)
            {
                this.CADLayers.Add(this.SelectedCADLayers[this.listSelected.SelectedIndex]);
                this.SelectedCADLayers.Remove(this.SelectedCADLayers[this.listSelected.SelectedIndex]);

                this.CADLayers.Sort();
                this.SelectedCADLayers.Sort();
                this.listOri.Items.Clear();

                foreach (string item in this.CADLayers)
                {
                    this.listOri.Items.Add(item);
                }


                this.listSelected.Items.Clear();
                foreach (string item in this.SelectedCADLayers)
                {
                    this.listSelected.Items.Add(item);
                }

            }
        }
    }
}
