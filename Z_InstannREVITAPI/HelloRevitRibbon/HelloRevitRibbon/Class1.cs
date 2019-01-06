using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.IO;
//using System.Windows.Forms;
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


            

            string filder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


            string test = Path.Combine(filder, "0_dllPath.txt");
             StreamReader sr = new StreamReader(test);

            string dllpath = sr.ReadLine().Trim();
            string imgPath = sr.ReadLine().Trim();
            string[] targetName_ = dllpath.Split('\\');
            string targetName = targetName_[targetName_.Length - 1].Replace(".dll", "").Trim();

            //MessageBox.Show(imgPath);

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










            //Guid guid1 = new Guid("0C3F66B5-3E26-4d24-A228-7A8358C76D39");
            //FailureDefinitionId m_idWarning = new FailureDefinitionId(guid1);
            //FailureDefinition m_fdWarning = FailureDefinition.CreateFailureDefinition(m_idWarning, FailureSeverity.Warning, "I am the warning.");
            //m_fdWarning.AddResolutionType(FailureResolutionType.MoveElements, "MoveElements", typeof(DeleteElements));

             
            //using (Transaction transaction = new Transaction(revitDoc))
            //{
            //    transaction.Start();
            //    FailureMessage fm = new FailureMessage(m_idWarning);
            //    m_doc.PostFailure(fm);
            //    transaction.Commit();
            //}


            return Result.Succeeded;
        }


















         
    }
}
