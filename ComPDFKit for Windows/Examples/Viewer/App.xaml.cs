using System.Globalization;
using ComPDFKit.NativeMethod;
using Compdfkit_Tools.Helper;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml; 

namespace Viewer
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string str = this.GetType().Assembly.Location;
            base.OnStartup(e);
            LicenseVerify();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        public static bool LicenseVerify()
        {
            if (!CPDFSDKVerifier.LoadNativeLibrary())
                return false;

            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(SDKLicenseHelper.ParseLicenseXML(), false);
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
        }
    }
}

