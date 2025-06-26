using ComPDFKit.NativeMethod;
using ComPDFKit.Controls.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; 

namespace ContentEditorViewControl
{
     
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

            LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(SDKLicenseHelper.GetLicenseXMLPath());
            return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
        }
    }
}
