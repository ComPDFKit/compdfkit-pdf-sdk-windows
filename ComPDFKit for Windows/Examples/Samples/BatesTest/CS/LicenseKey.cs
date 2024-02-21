using ComPDFKit.NativeMethod;
using System.Xml;

public class SDKLicenseHelper
{
    public string key = string.Empty;
    public string secret = string.Empty;

    public SDKLicenseHelper()
    {
        string sdkLicensePath = "license_key_windows.xml";
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(sdkLicensePath);
        var node = xmlDocument.SelectSingleNode("License");
        if (node != null)
        {
            key = node.Attributes["key"].Value;
            secret = node.Attributes["secret"].Value;
        }
    }

    public static bool LicenseVerify()
    {
        bool result = false;

        result = CPDFSDKVerifier.LoadNativeLibrary();
        if (!result)
            return false;

        SDKLicenseHelper sdkLicenseHelper = new SDKLicenseHelper();

        LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(sdkLicenseHelper.key, sdkLicenseHelper.secret);
        if (verifyResult != LicenseErrorCode.LICENSE_ERR_SUCCESS)
            return false;
        return result;
    }
}