using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using ComPDFKit.NativeMethod;
using Compdfkit_Tools.Helper;
using PDFViewer.Properties;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application
    {
        public static string CurrentCulture = Settings.Default.Cultrue;
        public static List<string> SupportedCultureList= new List<string>()
        {
            "en-US", "zh-CN"
        };
        public static FilePathList OpenedFilePathList = new FilePathList();
        public static ResourceManager MainResourceManager = new ResourceManager("PDFViewer.Strings.SettingDialog", Assembly.GetExecutingAssembly());

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
            HistoryFile(@"TestFile\ComPDFKit_Sample_File_Windows.pdf");
            FileHistoryHelper<PDFFileInfo>.Instance.LoadHistory();
        }
        
        private static bool LicenseVerify()
        {
            if (!CPDFSDKVerifier.LoadNativeLibrary())
                return false;

            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify("license_key_windows.txt", true);
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
        }
        
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
