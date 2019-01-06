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

namespace InstallRevitAPI
{
    public partial class RevitAPI : Form
    {
        private string REVIT_FOLDER_PATH = "";
        private string API_DLL_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public RevitAPI()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string tmpPath = Path.Combine(API_DLL_PATH, "Autodesk", "Revit"); 
            string tmpPath2 = Path.Combine(API_DLL_PATH, "Autodesk", "Revit", "Addins"); 
            if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);
            if (!Directory.Exists(tmpPath2)) Directory.CreateDirectory(tmpPath2); 
            this.txtVersion.Text = "2018"; 

        }
         
        private void btnStart_Click(object sender, EventArgs e)
        {

            REVIT_FOLDER_PATH = Path.Combine(API_DLL_PATH, "Autodesk", "Revit", "Addins", this.txtVersion.Text.Trim());
            if (!Directory.Exists(REVIT_FOLDER_PATH)) Directory.CreateDirectory(REVIT_FOLDER_PATH);

            string FILE_NAME_1 = "Autodesk.AddInManager";
            string FILE_NAME_2 = "HelloWorldRibbon";
            List<string> FILE_NAME = new List<string>() { FILE_NAME_1, FILE_NAME_2 };
            List<string> DLL_FILE_NAME = new List<string>() { "AddInManager.dll", "HelloRevitRibbon.dll" };

            int kk = 0;
            foreach (string FILE in FILE_NAME)
            {
                string saveFileName = FILE + ".addin";
                string replaceFileName = DLL_FILE_NAME[kk];

                /// addin樣板檔案的路徑
                string ORI_ADDIN_PATH = Path.Combine(Directory.GetCurrentDirectory(), saveFileName);
                /// 欲新增addin檔案的路徑
                string SAVEPATH = Path.Combine(REVIT_FOLDER_PATH, saveFileName);
                /// DLL檔案的路徑
                string ORI_DLL_PATH = Path.Combine(Directory.GetCurrentDirectory(), replaceFileName);
                /// 欲新增DLL檔案的路徑
                string MOVE_DLL_PATH = Path.Combine(API_DLL_PATH, replaceFileName);
                /// 載入addin檔案並替換其內容之路徑
                List<string> DATA = LoadingText(ORI_ADDIN_PATH, MOVE_DLL_PATH);
                /// 將addin另存於新路徑
                SaveText(SAVEPATH, DATA);

                COPY_TO(ORI_DLL_PATH, MOVE_DLL_PATH);

                kk++;
            }



            /// API 處理
            string TargetAPIName = LoadingText(Path.Combine(Directory.GetCurrentDirectory(), "0_dllPath.txt")) + ".dll";
            string ori_dll_path = Path.Combine(Directory.GetCurrentDirectory(), TargetAPIName);
            string dll_Path = Path.Combine(API_DLL_PATH, TargetAPIName);
            COPY_TO(ori_dll_path, dll_Path);

            ori_dll_path = Path.Combine(Directory.GetCurrentDirectory(), "Img1.png");
            string image_path = Path.Combine(API_DLL_PATH, "Img1.png");

            COPY_TO(ori_dll_path, image_path);

            List<string> nData = new List<string>() { dll_Path, image_path };
            SaveText(Path.Combine(API_DLL_PATH, "0_dllPath.txt"), nData);


            MessageBox.Show("完成");
            this.Close();

        }


        private void COPY_TO(string ori_dll_path, string image_path)
        {
            try
            {
                if (File.Exists(image_path)) File.Delete(image_path);

                File.Copy(ori_dll_path, image_path);

            }
            catch (Exception)
            {

            }
        }

        private string LoadingText(string PATH)
        {
            StreamReader sr = new StreamReader(PATH);
            string DATA = "";
            DATA = sr.ReadLine();
            sr.Close();

            return DATA;
        }

        private List<string> LoadingText(string PATH, string ReplaceString)
        {
            StreamReader sr = new StreamReader(PATH);
            List<string> DATA = new List<string>();
            while (sr.Peek() != -1)
            {
                string tmpData = sr.ReadLine();
                tmpData = tmpData.Replace("***REPLACE_STRING***", ReplaceString);
                DATA.Add(tmpData);
            }

            sr.Close();

            return DATA;
        }

        private void SaveText(string PATH, List<string> DATA)
        {
            StreamWriter sw = new StreamWriter(PATH);
            foreach (string data in DATA)
            {
                sw.WriteLine(data);
                sw.Flush();
            }
            sw.Close();
        }

    }
}
