Imports System
Imports System.Xml
Imports ComPDFKit.NativeMethod

Public NotInheritable Class SDKLicenseHelper

    Public Shared Function ParseLicenseXML() As String
        Try
            Dim xmlDocument As New XmlDocument()
            xmlDocument.Load("license_key_windows.xml")
            Dim xmlNode As XmlNode = xmlDocument.SelectSingleNode("/license/key")
            If xmlNode Is Nothing Then
                Return String.Empty
            Else
                Return xmlNode.InnerText
            End If
        Catch
            Return String.Empty
        End Try
    End Function

    Public Shared Function LicenseVerify() As Boolean
        If Not CPDFSDKVerifier.LoadNativeLibrary() Then
            Return False
        End If

        Dim verifyResult As LicenseErrorCode = CPDFSDKVerifier.LicenseVerify(ParseLicenseXML(), False)
        Return (verifyResult <> LicenseErrorCode.E_LICENSE_SUCCESS)
    End Function
End Class
