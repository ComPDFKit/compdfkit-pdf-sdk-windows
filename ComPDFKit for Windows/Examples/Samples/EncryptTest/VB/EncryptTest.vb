Imports ComPDFKit.PDFDocument
Imports System
Imports System.IO

Module EncryptTest
    Private outputPath As String = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) & "\Output\VB"
    Private userPassword As String = String.Empty
    Private ownerPassword As String = String.Empty

    Sub Main(args As String())
        Console.WriteLine("Running Encrypt test sample..." & vbCrLf)
        SDKLicenseHelper.LicenseVerify()

        If Not Directory.Exists(outputPath) Then
            Directory.CreateDirectory(outputPath)
        End If

        ' Encrypt by user password
        Dim document As CPDFDocument = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If EncryptByUserPassword(document) Then
            Console.WriteLine("Encrypt by user password done.")
        Else
            Console.WriteLine("Encrypt by user password failed.")
        End If

        document.Release()
        Console.WriteLine("--------------------")

        ' Encrypt by owner password
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If EncryptByOwnerPassword(document) Then
            Console.WriteLine("Encrypt by owner password done.")
        Else
            Console.WriteLine("Encrypt by owner password failed.")
        End If

        document.Release()
        Console.WriteLine("--------------------")

        ' Encrypt by both user and owner passwords
        document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf")

        If EncryptByAllPasswords(document) Then
            Console.WriteLine("Encrypt by Both user and owner passwords done.")
        Else
            Console.WriteLine("Encrypt by Both user and owner passwords failed.")
        End If

        document.Release()
        Console.WriteLine("--------------------")

        ' Unlock
        document = CPDFDocument.InitWithFilePath("AllPasswords.pdf")

        If Unlock(document) Then
            Console.WriteLine("Unlock done.")
        Else
            Console.WriteLine("Unlock failed.")
        End If

        document.Release()
        Console.WriteLine("--------------------")

        ' Decrypt
        document = CPDFDocument.InitWithFilePath("AllPasswords.pdf")

        If Decrypt(document) Then
            Console.WriteLine("Decrypt done.")
        Else
            Console.WriteLine("Decrypt failed.")
        End If

        document.Release()
        Console.WriteLine("--------------------")

        Console.WriteLine("Done!")
        Console.WriteLine("--------------------")
        Console.ReadLine()
    End Sub

    Private Function EncryptUseRC4Algo(document As CPDFDocument, permissionsInfo As CPDFPermissionsInfo) As Boolean
        Dim encryptionLevel As CPDFDocumentEncryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelRC4
        document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel)
        Dim encryptPath As String = outputPath & "\EncryptUseRC4Test.pdf"

        If Not document.WriteToFilePath(encryptPath) Then
            Return False
        End If

        Dim encryptedDoc As CPDFDocument = CPDFDocument.InitWithFilePath(encryptPath)

        If encryptedDoc.IsEncrypted Then
            Console.WriteLine("File is encrypted")
            Console.WriteLine("Browse the changed file in: " & encryptPath)
            Console.WriteLine("User password is: {0}", userPassword)
        Else
            Console.WriteLine("File encrypt failed")
            Return False
        End If

        Return True
    End Function

    Private Function EncryptUseAES128Algo(document As CPDFDocument, permissionsInfo As CPDFPermissionsInfo) As Boolean
        Dim encryptionLevel As CPDFDocumentEncryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelAES128
        document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel)
        Dim encryptPath As String = outputPath & "\EncryptUseAES128Test.pdf"

        If Not document.WriteToFilePath(encryptPath) Then
            Return False
        End If

        Dim encryptedDoc As CPDFDocument = CPDFDocument.InitWithFilePath(encryptPath)

        If encryptedDoc.IsEncrypted Then
            Console.WriteLine("File is encrypted")
            Console.WriteLine("Browse the changed file in: " & encryptPath)
            Console.WriteLine("User password is: {0}", userPassword)
        Else
            Console.WriteLine("File encrypt failed")
            Return False
        End If

        Return True
    End Function

    Private Function EncryptUseAES256Algo(document As CPDFDocument, permissionsInfo As CPDFPermissionsInfo) As Boolean
        Dim encryptionLevel As CPDFDocumentEncryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelAES256
        document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel)
        Dim encryptPath As String = outputPath & "\EncryptUseAES256Test.pdf"

        If Not document.WriteToFilePath(encryptPath) Then
            Return False
        End If

        Dim encryptedDoc As CPDFDocument = CPDFDocument.InitWithFilePath(encryptPath)

        If encryptedDoc.IsEncrypted Then
            Console.WriteLine("File is encrypted")
            Console.WriteLine("Browse the changed file in " & encryptPath)
            Console.WriteLine("User password is: {0}", userPassword)
        Else
            Console.WriteLine("File encrypt failed")
            Return False
        End If

        Return True
    End Function

    Private Function EncryptUseNoEncryptAlgo(document As CPDFDocument, permissionsInfo As CPDFPermissionsInfo) As Boolean
        Dim encryptionLevel As CPDFDocumentEncryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelNoEncryptAlgo
        document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel)
        Dim encryptPath As String = outputPath & "\EncryptUseNoEncryptAlgoTest.pdf"

        If Not document.WriteToFilePath(encryptPath) Then
            Return False
        End If

        Dim encryptedDoc As CPDFDocument = CPDFDocument.InitWithFilePath(encryptPath)

        If encryptedDoc.IsEncrypted Then
            Console.WriteLine("File is encrypted.")
            Console.WriteLine("Browse the changed file in " & encryptPath)
            Console.WriteLine("User password is: {0}", userPassword)
        Else
            Console.WriteLine("File encrypt failed")
            Return False
        End If

        Return True
    End Function

    Private Function EncryptByUserPassword(document As CPDFDocument) As Boolean
        Dim result As Boolean = True
        userPassword = "User"
        ownerPassword = String.Empty
        Dim permissionsInfo As New CPDFPermissionsInfo()

        If EncryptUseRC4Algo(document, permissionsInfo) Then
            Console.WriteLine("RC4 encrypt done." & vbCrLf)
        Else
            Console.WriteLine("RC4 encrypt failed." & vbCrLf)
            result = False
        End If

        If EncryptUseAES128Algo(document, permissionsInfo) Then
            Console.WriteLine("AES128 encrypt done." & vbCrLf)
        Else
            Console.WriteLine("AES128 encrypt failed." & vbCrLf)
            result = False
        End If

        If EncryptUseAES256Algo(document, permissionsInfo) Then
            Console.WriteLine("AES256 encrypt done." & vbCrLf)
        Else
            Console.WriteLine("AES256 encrypt failed." & vbCrLf)
            result = False
        End If

        If EncryptUseNoEncryptAlgo(document, permissionsInfo) Then
            Console.WriteLine("NoEncryptAlgo encrypt done." & vbCrLf)
        Else
            Console.WriteLine("NoEncryptAlgo encrypt failed." & vbCrLf)
            result = False
        End If

        Return result
    End Function

    Private Function EncryptByOwnerPassword(document As CPDFDocument) As Boolean
        userPassword = Nothing
        ownerPassword = "Owner"
        Dim permissionsInfo As New CPDFPermissionsInfo()
        permissionsInfo.AllowsPrinting = False
        permissionsInfo.AllowsCopying = False
        Dim encryptionLevel As CPDFDocumentEncryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelRC4
        document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel)
        Dim encryptPath As String = outputPath & "\EncryptByOwnerPasswordTest.pdf"

        If Not document.WriteToFilePath(encryptPath) Then
            Return False
        End If

        Dim encryptedDoc As CPDFDocument = CPDFDocument.InitWithFilePath(encryptPath)

        If encryptedDoc.IsEncrypted Then
            Console.WriteLine("File is encrypted.")
            Console.WriteLine("Browse the changed file in " & encryptPath)
            Console.WriteLine("Owner password is: {0}", ownerPassword)
        Else
            Console.WriteLine("File encrypt failed")
            Return False
        End If

        Return True
    End Function

    Private Function EncryptByAllPasswords(document As CPDFDocument) As Boolean
        userPassword = "User"
        ownerPassword = "Owner"
        Dim permissionsInfo As New CPDFPermissionsInfo()
        permissionsInfo.AllowsPrinting = False
        permissionsInfo.AllowsCopying = False
        Dim encryptionLevel As CPDFDocumentEncryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelRC4
        document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel)
        Dim encryptPath As String = outputPath & "\EncryptByAllPasswordsTest.pdf"

        If Not document.WriteToFilePath(encryptPath) Then
            Return False
        End If

        Dim encryptedDoc As CPDFDocument = CPDFDocument.InitWithFilePath(encryptPath)

        If encryptedDoc.IsEncrypted Then
            Console.WriteLine("File is encrypted.")
            Console.WriteLine("Browse the changed file in " & encryptPath)
            Console.WriteLine("User password is: {0}", userPassword)
            Console.WriteLine("Owner password is: {0}", ownerPassword)
        Else
            Console.WriteLine("File encrypt failed")
            Return False
        End If

        Return True
    End Function

    Private Sub PrintPermissionsInfo(permissionsInfo As CPDFPermissionsInfo)
        Console.Write("AllowsPrinting: ")
        Console.Write(If(permissionsInfo.AllowsPrinting = True, "Yes" & vbCrLf, "No" & vbCrLf))
        Console.Write("AllowsCopying: ")
        Console.Write(If(permissionsInfo.AllowsCopying = True, "Yes" & vbCrLf, "No" & vbCrLf))
    End Sub

    Private Function Unlock(document As CPDFDocument) As Boolean
        userPassword = "User"
        ownerPassword = "Owner"

        If document.IsLocked Then
            Console.WriteLine("Document is locked")
        End If

        PrintPermissionsInfo(document.GetPermissionsInfo())

        Console.WriteLine("Unlock with owner password")
        document.CheckOwnerPassword("123")

        PrintPermissionsInfo(document.GetPermissionsInfo())

        Return True
    End Function

    Private Function Decrypt(document As CPDFDocument) As Boolean
        userPassword = "User"
        ownerPassword = "Owner"
        Dim decryptPath As String = outputPath & "\DecryptTest.pdf"
        document.UnlockWithPassword(userPassword)

        If Not document.Decrypt(decryptPath) Then
            Return False
        End If

        Dim decryptDocument As CPDFDocument = CPDFDocument.InitWithFilePath(decryptPath)

        If decryptDocument.IsEncrypted Then
            Return False
        Else
            Console.WriteLine("Document decrypt done.")
        End If

        Return True
    End Function
End Module
