using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;



namespace _6_ReadDWG
{
    public class FormDataProcessing
    {
        List<System.Windows.Forms.ComboBox> Comboxes;
        List<System.Windows.Forms.TextBox> Texts;
        List<System.Windows.Forms.RadioButton> Radios;
        List<System.Windows.Forms.CheckBox> Checkboxes;
        private string AppPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string FileName;
        string Path_CMB;
        string Path_TXT;
        string Path_RAD;
        string Path_CHE;

        public FormDataProcessing(string fileName)
        {
            this.FileName = fileName;
            Path_CMB = Path.Combine(this.AppPath, this.FileName + "_CMB.txt");
            Path_TXT = Path.Combine(this.AppPath, this.FileName + "_TXT.txt");
            Path_RAD = Path.Combine(this.AppPath, this.FileName + "_RAD.txt");
            Path_CHE = Path.Combine(this.AppPath, this.FileName + "_CHE.txt");
        }
         

        public void SetParameters(
                                    List<System.Windows.Forms.ComboBox> comboxes,
                                    List<System.Windows.Forms.TextBox> texts,
                                    List<System.Windows.Forms.RadioButton> radios,
                                    List<System.Windows.Forms.CheckBox> checkboxes)
        {
            this.Comboxes = comboxes == null ? new List<System.Windows.Forms.ComboBox>() : comboxes;
            this.Texts = texts == null ? new List<System.Windows.Forms.TextBox>() : texts;
            this.Radios = radios == null ? new List<System.Windows.Forms.RadioButton>() : radios;
            this.Checkboxes = checkboxes == null ? new List<System.Windows.Forms.CheckBox>() : checkboxes;

        }


        public void InsertItems(List<List<string>> cmb)
        {
            for (int i = 0; i < cmb.Count; i++)
            {
                for (int j = 0; j < cmb[i].Count; j++)
                {
                    this.Comboxes[i].Items.Add(cmb[i][j]);
                }
            }  
        }


        public void Loading()
        {

            try
            {
                using (StreamReader sr = new StreamReader(this.Path_CMB))
                {
                    int kk = 0;
                    while (sr.Peek() != -1)
                    {
                        try
                        {
                            int index = Convert.ToInt32(sr.ReadLine());
                            this.Comboxes[kk].SelectedIndex = index;
                        }
                        catch (Exception)
                        {
                            if (this.Comboxes.Count != 0)
                            {
                                this.Comboxes[kk].SelectedIndex = 0;
                            }
                        }
                        kk++;
                    }
                }
            }
            catch (Exception)
            {
                foreach (var item in this.Comboxes)
                {
                    if (item.Items.Count != 0)
                    {
                        item.SelectedIndex = 0;
                    }
                }
            }

            try
            {
                using (StreamReader sr = new StreamReader(this.Path_TXT))
                {
                    int kk = 0;
                    while (sr.Peek() != -1)
                    {
                        try
                        {
                            string context = sr.ReadLine();
                            this.Texts[kk].Text = context;
                        }
                        catch (Exception)
                        {
                            this.Texts[kk].Text = "";
                        }
                        kk++;
                    }
                }


                using (StreamReader sr = new StreamReader(this.Path_RAD))
                {
                    int kk = 0;
                    while (sr.Peek() != -1)
                    {
                        try
                        {
                            bool context = Convert.ToBoolean(sr.ReadLine());
                            this.Radios[kk].Checked = context;
                        }
                        catch (Exception)
                        {
                            this.Radios[kk].Checked = false;
                        }
                        kk++;
                    }
                }


                using (StreamReader sr = new StreamReader(this.Path_CHE))
                {
                    int kk = 0;
                    while (sr.Peek() != -1)
                    {
                        try
                        {
                            bool context = Convert.ToBoolean(sr.ReadLine());
                            this.Checkboxes[kk].Checked = context;
                        }
                        catch (Exception)
                        {
                            this.Checkboxes[kk].Checked = false;
                        }
                        kk++;
                    }
                }
            }
            catch (Exception)
            {

            }

        }


         

        public void Saving()
        {
            using (StreamWriter sw = new StreamWriter(this.Path_CMB))
            {
                foreach (System.Windows.Forms.ComboBox item in this.Comboxes)
                {
                    sw.WriteLine(item.SelectedIndex.ToString());
                    sw.Flush();
                }
            }

            using (StreamWriter sw = new StreamWriter(this.Path_TXT))
            {
                foreach (System.Windows.Forms.TextBox item in this.Texts)
                {
                    sw.WriteLine(item.Text);
                    sw.Flush();
                }
            }


            using (StreamWriter sw = new StreamWriter(this.Path_RAD))
            {
                foreach (System.Windows.Forms.RadioButton item in this.Radios)
                {
                    sw.WriteLine(item.Checked.ToString());
                    sw.Flush();
                }
            }



            using (StreamWriter sw = new StreamWriter(this.Path_CHE))
            {
                foreach (System.Windows.Forms.CheckBox item in this.Checkboxes)
                {
                    sw.WriteLine(item.Checked.ToString());
                    sw.Flush();
                }
            }

        }


    }
}
