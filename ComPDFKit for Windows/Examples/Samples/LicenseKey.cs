using ComPDFKit.NativeMethod;
using System.Xml;

public static class SDKLicenseHelper
{
    public static string ParseLicenseXML()
    {
        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("license_key_windows.xml");
            XmlNode xmlNode = xmlDocument.SelectSingleNode("/license/key");
            if (xmlNode == null)
            {
                return string.Empty;
            }
            else
            {
                return xmlNode.InnerText;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool LicenseVerify()
    {
        if (!CPDFSDKVerifier.LoadNativeLibrary())
            return false;

        LicenseErrorCode verifyResult = CPDFSDKVerifier.LicenseVerify(ParseLicenseXML(), false);
        return (verifyResult == LicenseErrorCode.E_LICENSE_SUCCESS);
    }
}
