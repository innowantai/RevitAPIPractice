using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.IO;

namespace HelloRevitRibbon
{
    public class Class1 : Autodesk.Revit.UI.IExternalApplication  
    {

        public Autodesk.Revit.UI.Result OnShutdown(UIControlledApplication application)
        {

            return Result.Succeeded;
        }

        public Autodesk.Revit.UI.Result OnStartup(UIControlledApplication application)
        { 
            StreamReader sr = new StreamReader("0_dllPath.txt");
            string imgPath = sr.ReadLine().Trim();
            string path = sr.ReadLine().Trim();
            string targetName = sr.ReadLine().Trim();
            string @dllpath = path + @"\" + targetName +  @"\" + targetName + @"\bin\Debug\" + targetName + ".dll";
            string className = targetName + ".Class1";
       
            sr.Close();
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("RevitAPI");
            PushButton pushButton = (PushButton)ribbonPanel.AddItem(new PushButtonData("RevitAPITest",
                                                                            "Click Me",
                                                                            @dllpath,
                                                                            className));

            Uri uriImage = new Uri(@imgPath);
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushButton.LargeImage = largeImage;
            return Result.Succeeded;
        }

         
    }
}
