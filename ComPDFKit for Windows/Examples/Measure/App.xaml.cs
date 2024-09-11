using ComPDFKit.NativeMethod;
using ComPDFKit.Controls.Helper;
using System.Windows;

namespace Measure
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LicenseVerify();
        }

        private static bool LicenseVerify()
        {
            if (!CPDFSDKVerifier.LoadNativeLibrary())
                return false;

            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(SDKLicenseHelper.ParseLicenseXML(), false);
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
        }
    }
}
