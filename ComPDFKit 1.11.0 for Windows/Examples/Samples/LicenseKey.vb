Imports ComPDFKit.NativeMethod
Imports System
Imports System.Xml
Imports ComPDFKit.NativeMethod.CPDFSDKVerifier


Public NotInheritable Class SDKLicenseHelper

    Public Shared Function LicenseVerify() As Boolean
        If Not LoadNativeLibrary() Then
            Return False
        End If

        Dim verifyResult As LicenseErrorCode = CPDFSDKVerifier.LicenseVerify("license_key_windows.txt", True)
        Return (verifyResult <> LicenseErrorCode.E_LICENSE_SUCCESS)
    End Function
End Class
