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
    public partial class Form_CreateFloorByCADHash : System.Windows.Forms.Form
    {
        List<string> CADLayers;
        List<string> Levels;
        List<string> FloorTypes;
        List<System.Windows.Forms.ComboBox> Comboxes;
        List<System.Windows.Forms.TextBox> Texts;
        List<System.Windows.Forms.RadioButton> Radios;
        List<System.Windows.Forms.CheckBox> CheckBoxes;
        FormDataProcessing FDP;
        string FileName = "";

        public Form_CreateFloorByCADHash(List<string> cadLayers, List<Level> levels, List<Element> floorTypes)
        {
            this.CADLayers = cadLayers;
            this.FloorTypes = new List<string>();
            this.Levels = new List<string>();

            foreach (var item in levels)
            {
                this.Levels.Add(item.Name);

            }
            foreach (var item in floorTypes)
            {
                this.FloorTypes.Add(item.Name);

            }

            InitializeComponent();
        }

        private void Form_CreateFloorByCADHash_Load(object sender, EventArgs e)
        {
            FileName = "CREATE_FLOOR_BY_CAH_HASH";
            this.Comboxes = new List<System.Windows.Forms.ComboBox>() { this.cmbCADLayers, this.cmbFloorTypes, this.cmbBaseLevels };
            this.Texts = new List<System.Windows.Forms.TextBox>() { this.txtShift };
            this.Radios = new List<RadioButton>() { };
            this.CheckBoxes = new List<CheckBox>() { this.chIsIndicatedLayers ,this.chIsStructural};

            FDP = new FormDataProcessing(FileName);
            FDP.SetParameters(this.Comboxes, this.Texts, this.Radios, this.CheckBoxes);
            FDP.InsertItems(new List<List<string>>() { this.CADLayers, this.FloorTypes, this.Levels });
            FDP.Loading();
            this.cmbCADLayers.Enabled = this.chIsIndicatedLayers.Checked ? true : false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            FDP.Saving();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        private void chIsIndicatedLayers_CheckedChanged(object sender, EventArgs e)
        {
            this.cmbCADLayers.Enabled = this.chIsIndicatedLayers.Checked ? true : false;
        }
    }
}
