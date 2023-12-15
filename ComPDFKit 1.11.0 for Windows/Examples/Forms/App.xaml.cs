using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml;
using ComPDFKit.NativeMethod;
using Compdfkit_Tools.Helper;
using static ComPDFKit.NativeMethod.CPDFSDKVerifier;

namespace FormViewControl
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LicenseVerify();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        private static bool LicenseVerify()
        {
            if (!LoadNativeLibrary())
                return false;

            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify("license_key_windows.txt", true);
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
        }
    }
}