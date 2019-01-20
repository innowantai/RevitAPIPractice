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
    public partial class Form_CreateWallByRoomEdgeOrClickWall : System.Windows.Forms.Form
    {
        List<string> Levels;
        List<string> FloorTypes;
        List<System.Windows.Forms.ComboBox> Comboxes;
        List<System.Windows.Forms.TextBox> Texts;
        List<System.Windows.Forms.RadioButton> Radios;
        List<System.Windows.Forms.CheckBox> CheckBoxes;
        FormDataProcessing FDP;
        string FileName = "";


        public Form_CreateWallByRoomEdgeOrClickWall(List<Level> levels, List<Element> floorTypes)
        {
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

        private void Form_CreateWallByRoomEdgeOrClickWall_Load(object sender, EventArgs e)
        {

            FileName = "CREATE_WALL_BY_ROOMEDGE_OR_CLICKWALL";
            this.Comboxes = new List<System.Windows.Forms.ComboBox>() { this.cmbFloorTypes, this.cmbBaseLevels };
            this.Texts = new List<System.Windows.Forms.TextBox>() { this.txtShift };
            this.Radios = new List<RadioButton>() { };
            this.CheckBoxes = new List<CheckBox>() {   };

            FDP = new FormDataProcessing(FileName);
            FDP.SetParameters(this.Comboxes, this.Texts, this.Radios, this.CheckBoxes);
            FDP.InsertItems(new List<List<string>>() { this.FloorTypes, this.Levels });
            FDP.Loading();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

            FDP.Saving();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
