using ComPDFKit.NativeMethod;
using System;
using System.IO;
using System.Reflection;
using static ComPDFKit.NativeMethod.CPDFSDKVerifier;

public static class SDKLicenseHelper
{
    public static string GetLicenseXMLPath()
    {
        try
        {
            string callPath = System.IO.Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            string xmlPath = System.IO.Path.Combine(callPath, "license_key_windows.xml");

            if (File.Exists(xmlPath))
            {
                return xmlPath;
            }
        }
        catch (Exception ex)
        {

        }

        return string.Empty;
    }
    public static bool LicenseVerify()
    {
        if (!LoadNativeLibrary())
        {
            return false;
        }

        LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(GetLicenseXMLPath());
        return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
    }
}
