Imports System.IO
Imports ComPDFKit.DigitalSign
Imports ComPDFKit.Import
Imports ComPDFKit.PDFAnnotation.Form
Imports ComPDFKit.PDFDocument
Imports ComPDFKit.PDFPage

Module DigitalSignatureTest

    Private parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())))
    Private outputPath As String = Path.Combine(parentPath, "Output", "VB")

    Sub Main()
#Region "Preparation work"
        Console.WriteLine("Running digital signature sample..." & vbCrLf)

        SDKLicenseHelper.LicenseVerify()
        Dim certificatePath As String = "Certificate.pfx"
        Dim password As String = "ComPDFKit"

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If
#End Region

#Region "Sample 0: Create certificate"
        GenerateCertificate()
#End Region

#Region "Sample 1: Create digital signature"
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")
        CreateDigitalSignature(document, certificatePath, password)
        document.Release()
#End Region

#Region "Sample 2: Verify signature"
        Dim signedDoc As CPDFDocument = CPDFDocument.InitWithFilePath("Signed.pdf")
        VerifyDigitalSignature(signedDoc)
#End Region

#Region "Sample 3: Verify certificate"
        VerifyCertificate(certificatePath, password)
#End Region

#Region "Sample 4: Print digital signature info"
        PrintDigitalSignatureInfo(signedDoc)
#End Region

#Region "Sample 5: Trust Certificate"
        TrustCertificate(signedDoc)
#End Region

#Region "Sample 6: Remove digital signature"
        RemoveDigitalSignature(signedDoc)
        signedDoc.Release()
#End Region

        Console.WriteLine("Done!")

        Console.ReadLine()

    End Sub


    ''' <summary>
    ''' In the core function "CPDFPKCS12CertHelper.GeneratePKCS12Cert":
    '''
    ''' Generate certificate
    '''
    ''' Password: ComPDFKit
    '''
    ''' info: /C=SG/O=ComPDFKit/D=R&D Department/CN=Alan/emailAddress=xxxx@example.com
    '''
    ''' C=SG: This represents the country code "SG," which typically stands for Singapore.
    ''' O=ComPDFKit: This is the Organization (O) field, indicating the name of the organization or entity, in this case, "ComPDFKit."
    ''' D=R&D Department: This is the Department (D) field, indicating the specific department within the organization, in this case, "R&D Department."
    ''' CN=Alan: This is the Common Name (CN) field, which usually represents the name of the individual or entity. In this case, it is "Alan."
    ''' emailAddress=xxxx@example.com: Email is xxxx@example.com
    '''
    ''' CPDFCertUsage.CPDFCertUsageAll: Used for both digital signing and data validation simultaneously.
    '''
    ''' is_2048 = True: Enhanced security encryption.
    ''' </summary>
    Private Sub GenerateCertificate()
        Console.WriteLine("--------------------")
        Console.WriteLine("Generate certificate signature.")

        Dim info As String = "/C=SG/O=ComPDFKit/D=R&D Department/CN=Alan/emailAddress=xxxx@example.com"
        Dim password As String = "ComPDFKit"
        Dim filePath As String = outputPath & "\Certificate.pfx"

        If CPDFPKCS12CertHelper.GeneratePKCS12Cert(info, password, filePath, CPDFCertUsage.CPDFCertUsageAll, True) Then
            Console.WriteLine("File saved in " & filePath)
            Console.WriteLine("Generate PKCS12 certificate done.")
        Else
            Console.WriteLine("Generate PKCS12 certificate failed.")
        End If

        Console.WriteLine("--------------------")
    End Sub
    ''' <summary>
    ''' Adding a signature is divided into two steps:
    ''' creating a signature field and filling in the signature.
    '''
    ''' Page Index: 0
    ''' Rect: CRect(28, 420, 150, 370)
    ''' Border RGB: {0, 0, 0}
    ''' Widget Background RGB: {150, 180, 210}
    '''
    ''' Text: Grantor Name
    ''' Content:
    '''     Name: get grantor name from certificate
    '''     Date: now(yyyy.mm.dd)
    '''     Reason: I am the owner of the document.
    '''     DN: Subject
    '''     Location: Singapor
    '''     IsContentAlignLeft: False
    '''     IsDrawLogo: True
    '''     LogoBitmap: logo.png
    '''     text color RGB: {0, 0, 0}
    '''     content color RGB: {0, 0, 0}
    '''     Output file name: document.FileName + "_Signed.pdf"
    ''' </summary>
    Private Sub CreateDigitalSignature(document As CPDFDocument, certificatePath As String, password As String)
        Console.WriteLine("--------------------")
        Console.WriteLine("Create digital signature.")
        Dim certificate As CPDFSignatureCertificate = CPDFPKCS12CertHelper.GetCertificateWithPKCS12Path("Certificate.pfx", "ComPDFKit")

        Dim page As CPDFPage = document.PageAtIndex(0)
        Dim signatureField As CPDFSignatureWidget = TryCast(page.CreateWidget(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS), CPDFSignatureWidget)
        signatureField.SetRect(New CRect(28, 420, 150, 370))
        signatureField.SetWidgetBorderRGBColor(New Byte() {0, 0, 0})
        signatureField.SetWidgetBgRGBColor(New Byte() {150, 180, 210})
        signatureField.UpdateAp()

        Dim name As String = GetGrantorFromDictionary(certificate.SubjectDict) & vbCrLf
        Dim [date] As String = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss")
        Dim reason As String = "I am the owner of the document."
        Dim location As String = certificate.SubjectDict("C")
        Dim DN As String = certificate.Subject
        Dim signatureConfig As New CPDFSignatureConfig With {
        .Text = GetGrantorFromDictionary(certificate.SubjectDict),
        .Content = "Name: " & name & Environment.NewLine &
                   "Date: " & [date] & Environment.NewLine &
                   "Reason: " & reason & " " & Environment.NewLine &
                   "Location: " & location & Environment.NewLine &
                   "DN: " & DN & Environment.NewLine,
        .IsContentAlignLeft = False,
        .IsDrawLogo = False,
        .LogoData = File.ReadAllBytes("logo.png"),
        .TextColor = New Single() {0, 0, 0},
        .ContentColor = New Single() {0, 0, 0}
    }
        Dim filePath As String = outputPath & "\" & document.FileName & "_Signed.pdf"
        signatureField.UpdataApWithSignature(signatureConfig)
        If document.WriteSignatureToFilePath(signatureField,
        filePath,
        certificatePath, password,
        location,
        reason, CPDFSignaturePermissions.CPDFSignaturePermissionsNone) Then
            Console.WriteLine("File saved in " & filePath)
            Console.WriteLine("Create digital signature done.")
        Else
            Console.WriteLine("Create digital signature failed.")
        End If
        Console.WriteLine("--------------------")
    End Sub

    ''' <summary>
    ''' Remove digital signature
    ''' You can choose if you want to remove the appearance
    ''' </summary>
    ''' <param name="document"></param>
    Private Sub RemoveDigitalSignature(document As CPDFDocument)
        Console.WriteLine("--------------------")
        Console.WriteLine("Remove digital signature.")

        Dim signature As CPDFSignature = document.GetSignatureList()(0)
        document.RemoveSignature(signature, True)
        Dim filePath As String = outputPath & "\" & document.FileName & "_RemovedSign.pdf"
        document.WriteToFilePath(filePath)
        Console.WriteLine("File saved in " & filePath)

        Console.WriteLine("Remove digital signature done.")
        Console.WriteLine("--------------------")
    End Sub

    ''' <summary>
    ''' There are two steps can help you to trust a certificate.
    ''' Set your trust path as a folder path,
    ''' then add your certificate to the trust path.
    ''' </summary>
    Private Sub TrustCertificate(document As CPDFDocument)
        Console.WriteLine("--------------------")
        Console.WriteLine("Trust certificate.")

        Dim signature As CPDFSignature = document.GetSignatureList()(0)
        Dim signatureCertificate As CPDFSignatureCertificate = signature.SignerList(0).CertificateList(0)

        Console.WriteLine("Certificate trusted status: " & signatureCertificate.IsTrusted.ToString())

        Console.WriteLine("---Begin trusted---")

        Dim trustedFolder As String = AppDomain.CurrentDomain.BaseDirectory & "\TrustedFolder\"
        If Not Directory.Exists(trustedFolder) Then
            Directory.CreateDirectory(trustedFolder)
        End If
        CPDFSignature.SignCertTrustedFolder = trustedFolder
        If signatureCertificate.AddToTrustedCertificates() Then
            Console.WriteLine("Certificate trusted status: " & signatureCertificate.IsTrusted.ToString())
            Console.WriteLine("Trust certificate done.")
        Else
            Console.WriteLine("Trust certificate failed.")
        End If
        Console.WriteLine("--------------------")
    End Sub

    ''' <summary>
    ''' Verify certificate
    '''
    ''' To verify the trustworthiness of a certificate,
    ''' you need to verify that all certificates in the certificate chain are trustworthy.
    '''
    ''' In ComPDFKit, this progress is automatic.
    ''' You should call the "CPDFSignatureCertificate.CheckCertificateIsTrusted" first.
    ''' then you can view the "CPDFSignatureCertificate.IsTrusted" property.
    ''' </summary>
    ''' <param name="certificatePath">Path to the certificate</param>
    ''' <param name="password">Password for the certificate</param>
    Private Sub VerifyCertificate(certificatePath As String, password As String)
        Console.WriteLine("--------------------")
        Console.WriteLine("Verify certificate.")
        Dim certificate As CPDFSignatureCertificate = CPDFPKCS12CertHelper.GetCertificateWithPKCS12Path(certificatePath, password)
        certificate.CheckCertificateIsTrusted()
        If certificate.IsTrusted Then
            Console.WriteLine("Certificate is trusted")
        Else
            Console.WriteLine("Certificate is not trusted")
        End If
        Console.WriteLine("Verify certificate done.")
        Console.WriteLine("--------------------")
    End Sub

    ''' <summary>
    ''' Verify digital signature
    '''
    ''' Refresh the validation status before reading the attributes, or else you may obtain inaccurate results.
    ''' Is the signature verified: indicating whether the document has been tampered with.
    ''' Is the certificate trusted: referring to the trust status of the certificate.
    ''' </summary> 
    ''' <param name="document">A signed document</param>
    Private Sub VerifyDigitalSignature(document As CPDFDocument)
        Console.WriteLine("--------------------")
        Console.WriteLine("Verify digital signature.")
        For Each signature As CPDFSignature In document.GetSignatureList()
            signature.VerifySignatureWithDocument(document)
            For Each signer As CPDFSigner In signature.SignerList
                Console.WriteLine("Is the certificate trusted: " & signer.IsCertTrusted.ToString())
                Console.WriteLine("Is the signature verified: " & signer.IsSignVerified.ToString())
                If signer.IsCertTrusted AndAlso signer.IsSignVerified Then
                    ' Signature is valid and the certificate is trusted
                    ' Perform corresponding actions
                ElseIf Not signer.IsCertTrusted AndAlso signer.IsSignVerified Then
                    ' Signature is valid but the certificate is not trusted
                    ' Perform corresponding actions
                Else
                    ' Signature is invalid
                    ' Perform corresponding actions
                End If
            Next
        Next
        Console.WriteLine("Verify digital signature done.")
        Console.WriteLine("--------------------")
    End Sub

    Public Function GetGrantorFromDictionary(dictionary As Dictionary(Of String, String)) As String
        Dim grantor As String = String.Empty
        dictionary.TryGetValue("CN", grantor)
        If String.IsNullOrEmpty(grantor) Then
            dictionary.TryGetValue("OU", grantor)
        End If
        If String.IsNullOrEmpty(grantor) Then
            dictionary.TryGetValue("O", grantor)
        End If
        If String.IsNullOrEmpty(grantor) Then
            grantor = "Unknown Signer"
        End If
        Return grantor
    End Function

    ''' <summary>
    ''' This sample shows how to get main properties in a digital signature.
    ''' Read API reference to see all of the properties that can be obtained.
    ''' </summary>
    ''' <param name="document">A signed document</param>
    Private Sub PrintDigitalSignatureInfo(document As CPDFDocument)
        Console.WriteLine("--------------------")
        Console.WriteLine("Print digital signature info.")
        For Each signature As CPDFSignature In document.GetSignatureList()
            signature.VerifySignatureWithDocument(document)
            Console.WriteLine("Name: " & signature.Name)
            Console.WriteLine("Location: " & signature.Location)
            Console.WriteLine("Reason: " & signature.Reason)
            For Each signer As CPDFSigner In signature.SignerList
                Console.WriteLine("Date: " & signer.AuthenDate)
                For Each certificate As CPDFSignatureCertificate In signer.CertificateList
                    Console.WriteLine("Subject: " & certificate.Subject)
                Next
            Next
        Next
        Console.WriteLine("Print digital signature info done.")
        Console.WriteLine("--------------------")
    End Sub

End Module
