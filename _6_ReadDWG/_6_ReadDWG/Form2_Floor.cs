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
    public partial class Form2_Floor : System.Windows.Forms.Form
    {
        List<Level> levels;
        List<Element> floorTypes;
        List<System.Windows.Forms.ComboBox> CMBS;
        private string LastIndexesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public Form2_Floor(List<Level> levels_, List<Element> floorTypes_)
        {
            this.levels = levels_;
            this.floorTypes = floorTypes_;
            this.CMBS = new List<System.Windows.Forms.ComboBox>();
            InitializeComponent();

        }

        private void Form2_Floor_Load(object sender, EventArgs e)
        {
            /// 載入Level名稱
            foreach (Level item in this.levels)
            { 
                this.cmbfloorLevel.Items.Add(item.Name);
            }

            /// 載入Level名稱
            foreach (Level item in this.levels)
            {
                this.cmbColLevel.Items.Add(item.Name);
            }

            /// 載入 Floor名稱
            foreach (Element item in this.floorTypes)
            {
                this.cmbFloorTypes.Items.Add(item.Name);
            }

            this.CMBS.Add(this.cmbfloorLevel);
            this.CMBS.Add(this.cmbFloorTypes);
            this.CMBS.Add(this.cmbColLevel);
            readData();


        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveData();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }


        private void readData()
        {

            string path = Path.Combine(LastIndexesSavePath, "REVIT_FLOOR_FORM_INDEX.txt");

            try
            {
                using (StreamReader sr  = new StreamReader(path))
                {
                    int kk = 0;
                    while (sr.Peek() != -1)
                    {
                        int po = Convert.ToInt32(sr.ReadLine());
                        CMBS[kk].SelectedIndex = po;
                        kk++;
                    }
                } 
            }
            catch (Exception)
            {
                foreach (System.Windows.Forms.ComboBox item in CMBS)
                {
                    item.SelectedIndex = 0;
                }
            }
        }

        private void saveData()
        {
            string path = Path.Combine(LastIndexesSavePath,"REVIT_FLOOR_FORM_INDEX.txt");
            StreamWriter sw = new StreamWriter(path);
            foreach (System.Windows.Forms.ComboBox item in CMBS)
            {
                sw.WriteLine(item.SelectedIndex);
            } 
            sw.Close();
        }
    }
}
