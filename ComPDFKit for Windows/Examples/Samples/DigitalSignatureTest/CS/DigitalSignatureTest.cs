using ComPDFKit.DigitalSign;
using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DigitalSignatureTest
{
    internal class DigitalSignatureTest
    {
        static private string parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
        static private string outputPath = Path.Combine(parentPath, "Output", "CS");

        static void Main()
        {
            #region Preparation work
            Console.WriteLine("Running digital signature sample...\n");

            SDKLicenseHelper.LicenseVerify();
            string certificatePath = "Certificate.pfx";
            string password = "ComPDFKit";

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            #endregion

            #region Sample 0: Create certificate 
            GenerateCertificate();
            #endregion

            #region Sample 1: Create digital signature 
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            CreateDigitalSignature(document, certificatePath, password);
            document.Release();
            #endregion

            #region Sample 2: Verify signature
            CPDFDocument signedDoc = CPDFDocument.InitWithFilePath("Signed.pdf");
            VerifyDigitalSignature(signedDoc);
            #endregion

            #region Sample 3: Verify certificate
            VerifyCertificate(certificatePath, password);
            #endregion

            #region Sample 4: Print digital signature info
            PrintDigitalSignatureInfo(signedDoc);
            #endregion

            #region Sample 5: Trust Certificate
            TrustCertificate(signedDoc);
            #endregion

            #region Sample 6: Remove digital signature
            RemoveDigitalSignature(signedDoc);
            signedDoc.Release();
            #endregion


            Console.WriteLine("Done!");

            Console.ReadLine();
        }

        /// <summary>
        /// in the core function "CPDFPKCS12CertHelper.GeneratePKCS12Cert": 
        /// 
        /// Generate certificate
        /// 
        /// Password: ComPDFKit
        /// 
        /// info: /C=SG/O=ComPDFKit/D=R&D Department/CN=Alan/emailAddress=xxxx@example.com
        /// 
        /// C=SG: This represents the country code "SG," which typically stands for Singapore.
        /// O=ComPDFKit: This is the Organization (O) field, indicating the name of the organization or entity, in this case, "ComPDFKit."
        /// D=R&D Department: This is the Department (D) field, indicating the specific department within the organization, in this case, "R&D Department."
        /// CN=Alan: This is the Common Name (CN) field, which usually represents the name of the individual or entity. In this case, it is "Alan."
        /// emailAddress=xxxx@example.com: Email is xxxx@example.com
        /// 
        /// CPDFCertUsage.CPDFCertUsageAll: Used for both digital signing and data validation simultaneously.
        /// 
        /// is_2048 = true: Enhanced security encryption.
        /// </summary>
        private static void GenerateCertificate()
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Generate certificate signature.");

            string info = "/C=SG/O=ComPDFKit/D=R&D Department/CN=Alan/emailAddress=xxxx@example.com";
            string password = "ComPDFKit";
            string filePath = outputPath + "\\Certificate.pfx";
            if (CPDFPKCS12CertHelper.GeneratePKCS12Cert(info, password, filePath, CPDFCertUsage.CPDFCertUsageAll, true))
            {
                Console.WriteLine("File saved in " + filePath);
                Console.WriteLine("Generate PKCS12 certificate done.");
            }
            else
            {
                Console.WriteLine("Generate PKCS12 certificate failed.");

            }
            Console.WriteLine("--------------------");
        }

        private static void ImagePathToByte(string imagePath, ref byte[] imageData, ref int imageWidth, ref int imageHeight)
        {
            if (!File.Exists(imagePath))
                return;

            imagePath = Path.GetFullPath(imagePath);
            BitmapFrame frame = null;
            BitmapDecoder decoder = BitmapDecoder.Create(new Uri(imagePath), BitmapCreateOptions.None, BitmapCacheOption.Default);
            if (decoder != null && decoder.Frames.Count > 0)
            {
                frame = decoder.Frames[0];
            }
            if (frame != null)
            {
                imageData = new byte[frame.PixelWidth * frame.PixelHeight * 4];
                if (frame.Format != PixelFormats.Bgra32)
                {
                    FormatConvertedBitmap covert = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, frame.Palette, 0);
                    covert.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                }
                else
                {
                    frame.CopyPixels(imageData, frame.PixelWidth * 4, 0);
                }

                imageWidth = frame.PixelWidth;
                imageHeight = frame.PixelHeight;
            }
        }


        /// <summary>
        /// 
        /// Adding a signature is divided into two steps:
        /// creating a signature field and filling in the signature.
        /// 
        /// Page Index: 0
        /// Rect: CRect(28, 420, 150, 370)
        /// Border RGB:{ 0, 0, 0 }  
        /// Widget Background RGB: { 150, 180, 210 }
        /// 
        /// Text: Grantor Name
        /// Content:
        ///     Name: get grantor name from certificate
        ///     Date: now(yyyy.mm.dd)
        ///     Reason: I am the owner of the document.
        ///     DN: Subject
        ///     Location: Singapor
        ///     IsContentAlignLeft: false
        ///     IsDrawLogo: True
        ///     LogoBitmap: logo.png
        ///     text color RGB: { 0, 0, 0 }
        ///     content color RGB: { 0, 0, 0 }
        ///     Output file name: document.FileName + "_Signed.pdf"
        /// </summary>
        private static void CreateDigitalSignature(CPDFDocument document, string certificatePath, string password)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Create digital signature.");
            CPDFSignatureCertificate certificate = CPDFPKCS12CertHelper.GetCertificateWithPKCS12Path("Certificate.pfx", "ComPDFKit");

            CPDFPage page = document.PageAtIndex(0);
            CPDFSignatureWidget signatureField = page.CreateWidget(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS) as CPDFSignatureWidget;
            signatureField.SetRect(new CRect(28, 420, 150, 370));
            signatureField.SetWidgetBorderRGBColor(new byte[] { 0, 0, 0 });
            signatureField.SetWidgetBgRGBColor(new byte[] { 150, 180, 210 });
            signatureField.UpdateAp();

            string name = GetGrantorFromDictionary(certificate.SubjectDict) + "\n";
            string date = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
            string reason = "I am the owner of the document.";
            string location = certificate.SubjectDict["C"];
            string DN = certificate.Subject;
            CPDFSignatureConfig signatureConfig = new CPDFSignatureConfig
            {
                Text = GetGrantorFromDictionary(certificate.SubjectDict),
                Content =
                "Name: " + name + "\n" +
                "Date: " + date + "\n" +
                "Reason: " + reason + " \n" +
                "Location: " + location + "\n" +
                "DN: " + DN + "\n",
                IsContentAlignLeft = false,
                IsDrawLogo = true,
                TextColor = new float[] { 0, 0, 0 },
                ContentColor = new float[] { 0, 0, 0 }
            };

            //using (var image = new MagickImage("Logo.png"))
            //{
            //    byte[] byteArray = image.ToByteArray(MagickFormat.Bgra);
            //    signatureConfig.LogoData = byteArray;
            //    signatureConfig.LogoHeight = (int)image.Height;
            //    signatureConfig.LogoWidth = (int)image.Width;
            //}

            byte[] imageData = null;
            int imageWidth = 0;
            int imageHeight = 0;
            ImagePathToByte("Logo.png", ref imageData, ref imageWidth, ref imageHeight);
            if (imageData != null && imageWidth > 0 && imageHeight > 0)
            {
                signatureConfig.LogoData = imageData;
                signatureConfig.LogoWidth = imageWidth;
                signatureConfig.LogoHeight = imageHeight;
            }
            else
            {
                signatureConfig.IsDrawLogo = false;
            }

            string filePath = Path.Combine(outputPath, document.FileName + "_Signed.pdf");
            signatureField.UpdataApWithSignature(signatureConfig);
            if (document.WriteSignatureToFilePath(signatureField,
                filePath,
                certificatePath, password,
                location,
                reason, CPDFSignaturePermissions.CPDFSignaturePermissionsNone))
            {
                Console.WriteLine("File saved in " + filePath);
                Console.WriteLine("Create digital signature done.");
            }
            else
            {
                Console.WriteLine("Create digital signature failed.");
            }
            Console.WriteLine("--------------------");
        }

        /// <summary>
        /// Remove digital signature
        /// You can choose if you want to remove the appearance 
        /// </summary>
        /// <param name="document"></param>
        private static void RemoveDigitalSignature(CPDFDocument document)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Remove digital signature.");

            CPDFSignature signature = document.GetSignatureList()[0];
            document.RemoveSignature(signature, true);
            string filePath = outputPath + "\\" + document.FileName + "_RemovedSign.pdf";
            document.WriteToFilePath(filePath);
            Console.WriteLine("File saved in " + filePath);

            Console.WriteLine("Remove digital signature done.");
            Console.WriteLine("--------------------");
        }

        /// <summary>
        /// There are two steps can help you to trust a certificate.
        /// Set your trust path as a folder path,
        /// then add your certificate to the trust path.
        /// </summary>
        private static void TrustCertificate(CPDFDocument document)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Trust certificate.");

            CPDFSignature signature = document.GetSignatureList()[0];
            CPDFSignatureCertificate signatureCertificate = signature.SignerList[0].CertificateList[0];

            Console.WriteLine("Certificate trusted status: " + signatureCertificate.IsTrusted.ToString());

            Console.WriteLine("---Begin trusted---");

            string trustedFolder = AppDomain.CurrentDomain.BaseDirectory + @"\TrustedFolder\";
            if (!Directory.Exists(trustedFolder))
            {
                Directory.CreateDirectory(trustedFolder);
            }
            CPDFSignature.SignCertTrustedFolder = trustedFolder;
            if (signatureCertificate.AddToTrustedCertificates())
            {
                Console.WriteLine("Certificate trusted status: " + signatureCertificate.IsTrusted.ToString());
                Console.WriteLine("Trust certificate done.");
            }
            else
            {
                Console.WriteLine("Trust certificate failed.");
            }
            Console.WriteLine("--------------------");
        }

        /// <summary>
        /// Verify certificate
        /// 
        /// To verify the trustworthiness of a certificate, 
        /// you need to verify that all certificates in the certificate chain are trustworthy.
        /// 
        /// In ComPDFKit，this progess is automatic.
        /// You should call the "CPDFSignatureCertificate.CheckCertificateIsTrusted" first.
        /// then you can view the "CPDFSignatureCertificate.IsTrusted" property.
        /// </summary>
        /// <param name="document">A signed document</param>
        private static void VerifyCertificate(string certificatePath, string password)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Verify certificate.");
            CPDFSignatureCertificate certificate = CPDFPKCS12CertHelper.GetCertificateWithPKCS12Path(certificatePath, password);
            certificate.CheckCertificateIsTrusted();
            if (certificate.IsTrusted)
            {
                Console.WriteLine("Certificate is trusted");
            }
            else
            {
                Console.WriteLine("Certificate is not trusted");
            }
            Console.WriteLine("Verify certificate done.");
            Console.WriteLine("--------------------");
        }

        /// <summary>
        /// Verify digital signature
        /// 
        /// Refresh the validation status before reading the attributes, or else you may obtain inaccurate results.
        /// Is the signature verified: indicating whether the document has been tampered with.
        /// Is the certificate trusted: referring to the trust status of the certificate.
        /// </summary> 
        private static void VerifyDigitalSignature(CPDFDocument document)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Verify digital signature.");
            foreach (var signature in document.GetSignatureList())
            {
                signature.VerifySignatureWithDocument(document);
                foreach (var signer in signature.SignerList)
                {
                    Console.WriteLine("Is the certificate trusted: " + signer.IsCertTrusted.ToString());
                    Console.WriteLine("Is the signature verified: " + signer.IsSignVerified.ToString());
                    if (signer.IsCertTrusted && signer.IsSignVerified)
                    {
                        // Signature is valid and the certificate is trusted
                        // Perform corresponding actions
                    }
                    else if (!signer.IsCertTrusted && signer.IsSignVerified)
                    {
                        // Signature is valid but the certificate is not trusted
                        // Perform corresponding actions
                    }
                    else
                    {
                        // Signature is invalid
                        // Perform corresponding actions
                    }
                }
            }
            Console.WriteLine("Verify digital signature done.");
            Console.WriteLine("--------------------");
        }

        public static string GetGrantorFromDictionary(Dictionary<string, string> dictionary)
        {
            string grantor = string.Empty;
            dictionary.TryGetValue("CN", out grantor);
            if (string.IsNullOrEmpty(grantor))
            {
                dictionary.TryGetValue("OU", out grantor);
            }
            if (string.IsNullOrEmpty(grantor))
            {
                dictionary.TryGetValue("O", out grantor);
            }
            if (string.IsNullOrEmpty(grantor))
            {
                grantor = "Unknown Signer";
            }
            return grantor;
        }

        /// <summary>
        /// this samples shows how to get main properties in digital signature.
        /// read API reference to see all of the properties can get 
        /// </summary>
        /// <param name="document"></param>
        private static void PrintDigitalSignatureInfo(CPDFDocument document)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Print digital signature info.");
            foreach (var signature in document.GetSignatureList())
            {
                signature.VerifySignatureWithDocument(document);
                Console.WriteLine("Name: " + signature.Name);
                Console.WriteLine("Location: " + signature.Location);
                Console.WriteLine("Reason: " + signature.Reason);
                foreach (var signer in signature.SignerList)
                {
                    Console.WriteLine("Date: " + signer.AuthenDate);
                    foreach (var certificate in signer.CertificateList)
                    {
                        Console.WriteLine("Subject: " + certificate.Subject);
                    }
                }
            }
            Console.WriteLine("Print digital signature info done.");
            Console.WriteLine("--------------------");
        }
    }
}