using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using ComPDFKit.NativeMethod;
using ComPDFKit.Controls.Helper;
using PDFViewer.Properties;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string CurrentCulture = Settings.Default.Cultrue;
        public static List<string> SupportedCultureList = new List<string>()
        {
            "en-US", "zh-CN"
        };

        public static FilePathList OpenedFilePathList = new FilePathList();
        public static ResourceManager MainResourceManager = new ResourceManager("PDFViewer.Strings.SettingDialog", Assembly.GetExecutingAssembly());

        public static bool LicenseVerify()
        {
            if (!CPDFSDKVerifier.LoadNativeLibrary())
            { 
                return false; 
            }

            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(SDKLicenseHelper.ParseLicenseXML());
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS); 
        }

        protected override void OnStartup(StartupEventArgs e)
        { 
            if (string.IsNullOrEmpty(CurrentCulture))
            {
                CurrentCulture = CultureInfo.CurrentCulture.Name;
                if (!SupportedCultureList.Contains(CurrentCulture))
                {
                    CurrentCulture = "en-US";
                }
                Settings.Default.Cultrue = CurrentCulture;
                Settings.Default.Save();
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(CurrentCulture);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(CurrentCulture);

            base.OnStartup(e);
             
            LicenseVerify();
            //CPDFSDKVerifier.PrintPermissions();
            //CPDFSDKVerifier.LicenseRefreshed -= CPDFSDKVerifier_LicenseRefreshed;
            //CPDFSDKVerifier.LicenseRefreshed += CPDFSDKVerifier_LicenseRefreshed;
            HistoryFile(@"TestFile\ComPDFKit_Sample_File_Windows.pdf");
            FileHistoryHelper<PDFFileInfo>.Instance.LoadHistory();
        }

        //private void CPDFSDKVerifier_LicenseRefreshed(object sender, ResponseModel e)
        //{
        //    if(e != null)
        //    {
        //        string message = string.Format("{0} {1}", e.Code, e.Message);
        //        Trace.WriteLine(message);
        //    }
        //    else
        //    {
        //        Trace.WriteLine("Network not connected."); 
        //    } 
        //}

        private void HistoryFile(string item)
        {
            PDFFileInfo fileInfo = new PDFFileInfo();
            fileInfo.FilePath = item;
            fileInfo.FileName = Path.GetFileName(item);
            fileInfo.FileSize = CommonHelper.GetFileSize(fileInfo.FilePath);
            fileInfo.OpenDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            FileHistoryHelper<PDFFileInfo>.Instance.AddHistory(fileInfo);
            FileHistoryHelper<PDFFileInfo>.Instance.SaveHistory();
        }
    }

    public class FilePathList : List<string>
    {
        public new void Add(string item)
        {
            base.Add(item);
        }
    }

    public class ResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                return string.Empty;
            }
            return App.MainResourceManager.GetString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
