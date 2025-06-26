using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml;
using ComPDFKit.NativeMethod;
using ComPDFKit.Controls.Helper; 

namespace FormViewControl
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application
    {
        public static bool LicenseVerify()
        {
            if (!CPDFSDKVerifier.LoadNativeLibrary())
                return false;

            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(SDKLicenseHelper.GetLicenseXMLPath());
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LicenseVerify();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }
    }
}