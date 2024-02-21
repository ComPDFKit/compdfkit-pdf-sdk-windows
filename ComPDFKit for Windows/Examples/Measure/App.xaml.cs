using ComPDFKit.NativeMethod;
using Compdfkit_Tools.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static ComPDFKit.NativeMethod.CPDFSDKVerifier;

namespace Measure
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
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
