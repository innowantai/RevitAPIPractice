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
    public partial class Form1 : System.Windows.Forms.Form
    {

        List<Level> levels;
        Dictionary<string, List<LINE>> CADLayers;
        Dictionary<string, List<FamilySymbol>> colFamilyTypes;
        Dictionary<string, List<FamilySymbol>> beamFamilyTypes;
        public List<string> returnCADLayers = new List<string>();
        public List<FamilySymbol> returnType = new List<FamilySymbol>();
        public List<Level> returnBaseLevel = new List<Level>();
        public List<Level> returnTopLevel = new List<Level>();
        public bool returnCol = false;
        public bool returnBeam = false;

        public Form1(Dictionary<string, List<FamilySymbol>> _colFamilyTypes, 
                     Dictionary<string, List<FamilySymbol>> _beamFamilyTypes, 
                     List<Level> _levels,
                     Dictionary<string, List<LINE>> _CADLayers)
        {
            InitializeComponent();
            this.colFamilyTypes = _colFamilyTypes;
            this.beamFamilyTypes = _beamFamilyTypes;
            this.levels = _levels;
            this.CADLayers = _CADLayers;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /// 載入柱的族群與類型名稱
            foreach (KeyValuePair<string, List<FamilySymbol>> entry in this.colFamilyTypes) 
                this.cmbColFamilyType.Items.Add(entry.Key); 
            this.cmbColFamilyType.SelectedIndex = 0; 

            /// 載入梁的族群與類型名稱
            foreach (KeyValuePair<string, List<FamilySymbol>> entry in this.beamFamilyTypes)
                this.cmbBeamFamilyType.Items.Add(entry.Key);
            this.cmbBeamFamilyType.SelectedIndex = 0;

            /// 載入Level名稱
            foreach (Level item in this.levels)
            {
                this.cmbColBaseLevel.Items.Add(item.Name);
                this.cmbColTopLevel.Items.Add(item.Name);
                this.cmbBeamBaseLevel.Items.Add(item.Name);
            }
            this.cmbColBaseLevel.SelectedIndex = 0;
            this.cmbColTopLevel.SelectedIndex = 0;
            this.cmbBeamBaseLevel.SelectedIndex = 0;

            /// 載入CAD layer名稱
            foreach (KeyValuePair<string, List<LINE>> entry in this.CADLayers)
            { 
                this.cmbColCADLayers.Items.Add(entry.Key);
                this.cmbBeamCADLayers.Items.Add(entry.Key);
            }
            cmbColCADLayers.SelectedIndex = 0;
            cmbBeamCADLayers.SelectedIndex = 1;
            chCol.Checked = true;
            chBeam.Checked = true;

        }



        /// <summary>
        /// 柱 combox Text change 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbColFamilyType_TextChanged(object sender, EventArgs e)
        { 
            this.cmbColType.Items.Clear();
            foreach (FamilySymbol defaultType in this.colFamilyTypes[this.cmbColFamilyType.Text])
                this.cmbColType.Items.Add(defaultType.Name); 
            this.cmbColType.SelectedIndex = 0;
        }

        /// <summary>
        /// 梁 combox Text change 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbBeamFamilyType_SelectedIndexChanged(object sender, EventArgs e)
        { 
            this.cmbBeamType.Items.Clear();
            foreach (FamilySymbol defaultType in this.beamFamilyTypes[this.cmbBeamFamilyType.Text])
                this.cmbBeamType.Items.Add(defaultType.Name);
            this.cmbBeamType.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {  
            returnType.Add(colFamilyTypes[cmbColFamilyType.Text][cmbColType.SelectedIndex]);
            returnType.Add(beamFamilyTypes[cmbBeamFamilyType.Text][cmbBeamType.SelectedIndex]);
            returnBaseLevel.Add(levels[cmbColBaseLevel.SelectedIndex]);
            returnBaseLevel.Add(levels[cmbBeamBaseLevel.SelectedIndex]); 
            returnTopLevel.Add(levels[cmbColTopLevel.SelectedIndex]); 
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();

        }
         
    }
}
