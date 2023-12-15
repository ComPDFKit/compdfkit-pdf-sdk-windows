using ComPDFKit.NativeMethod;
using System;
using System.Xml;
using static ComPDFKit.NativeMethod.CPDFSDKVerifier;

public static class SDKLicenseHelper
{ 
    public static bool LicenseVerify()
    {
        if (!LoadNativeLibrary())
            return false;

        LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify("license_key_windows.txt", true);
        return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
    }
}
