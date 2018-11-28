using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public Form2_Floor(List<Level> levels_, List<Element> floorTypes_)
        {
            this.levels = levels_;
            this.floorTypes = floorTypes_;
            InitializeComponent();
        }

        private void Form2_Floor_Load(object sender, EventArgs e)
        {
            /// 載入Level名稱
            foreach (Level item in this.levels)
            { 
                this.cmbfloorLevel.Items.Add(item.Name);
            }

            /// 載入 Floor名稱
            foreach (Element item in this.floorTypes)
            {
                this.cmbFloorTypes.Items.Add(item.Name);
            }


            cmbfloorLevel.SelectedIndex = 0;
            cmbFloorTypes.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
