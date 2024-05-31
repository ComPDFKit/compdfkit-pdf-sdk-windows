using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncryptTest
{
    internal class EncryptTest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");
        static private string userPassword = string.Empty;
        static private string ownerPassword = string.Empty;

        static void Main(string[] args)
        {
            #region Perparation work

            Console.WriteLine("Running Encrypt test sample…\r\n");

            SDKLicenseHelper.LicenseVerify();

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            #endregion

            #region Sample 1: Encrypt by user password

            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (EncryptByUserPassword(document))
            {
                Console.WriteLine("Encrypt by user password done.");
            }
            else
            {
                Console.WriteLine("Encrypt by user password failed.");
            }

            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            #region Sample 2:  Encrypt by owner password

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (EncryptByOwnerPassword(document))
            {
                Console.WriteLine("Encrypt by owner password done.");
            }
            else
            {
                Console.WriteLine("Encrypt by owner password failed.");
            }
            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 3: Encrypt by all passwords

            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (EncryptByAllPasswords(document))
            {
                Console.WriteLine("Encrypt by Both user and owner passwords done.");
            }
            else
            {
                Console.WriteLine("Encrypt by Both user and owner passwords failed.");
            }

            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 4: Unlock
            document = CPDFDocument.InitWithFilePath("AllPasswords.pdf");
            if (Unlock(document))
            {
                Console.WriteLine("Unlock done.");
            }
            else
            {
                Console.WriteLine("Unlock failed.");
            }
            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 5: Decrypt

            document = CPDFDocument.InitWithFilePath("AllPasswords.pdf");
            if (Decrypt(document))
            {
                Console.WriteLine("Decrypt done.");
            }
            else
            {
                Console.WriteLine("Decrypt failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        static private bool EncryptUseRC4Algo(CPDFDocument document, CPDFPermissionsInfo permissionsInfo)
        {
            CPDFDocumentEncryptionLevel encryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelRC4;
            document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel);
            string encryptPath = Path.Combine(outputPath, "EncryptUseRC4Test.pdf");
            if (!document.WriteToFilePath(encryptPath))
            {
                return false;
            }

            CPDFDocument encryptedDoc = CPDFDocument.InitWithFilePath(encryptPath);
            if (encryptedDoc.IsEncrypted)
            {
                Console.WriteLine("File is encrypted");
                Console.WriteLine("Browse the changed file in: " + encryptPath);
                Console.WriteLine("User password is: {0}", userPassword);
            }
            else
            {
                Console.WriteLine("File encrypt failed");
                return false;
            }

            return true;
        }

        static private bool EncryptUseAES128Algo(CPDFDocument document, CPDFPermissionsInfo permissionsInfo)
        {
            CPDFDocumentEncryptionLevel encryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelAES128;
            document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel);
            string encryptPath = Path.Combine(outputPath, "EncryptUseAES128Test.pdf");
            if (!document.WriteToFilePath(encryptPath))
            {
                return false;
            }

            CPDFDocument encryptedDoc = CPDFDocument.InitWithFilePath(encryptPath);
            if (encryptedDoc.IsEncrypted)
            {
                Console.WriteLine("File is encrypted");
                Console.WriteLine("Browse the changed file in: " + encryptPath);
                Console.WriteLine("User password is: {0}", userPassword);
            }
            else
            {
                Console.WriteLine("File encrypt failed");
                return false;
            }

            return true;
        }

        static private bool EncryptUseAES256Algo(CPDFDocument document, CPDFPermissionsInfo permissionsInfo)
        {
            CPDFDocumentEncryptionLevel encryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelAES256;
            document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel);
            string encryptPath = Path.Combine(outputPath, "EncryptUseAES256Test.pdf");
            if (!document.WriteToFilePath(encryptPath))
            {
                return false;
            }

            CPDFDocument encryptedDoc = CPDFDocument.InitWithFilePath(encryptPath);
            if (encryptedDoc.IsEncrypted)
            {
                Console.WriteLine("File is encrypted");
                Console.WriteLine("Browse the changed file in " + encryptPath);
                Console.WriteLine("User password is: {0}", userPassword);
            }
            else
            {
                Console.WriteLine("File encrypt failed");
                return false;
            }

            return true;
        }

        static private bool EncryptUseNoEncryptAlgo(CPDFDocument document, CPDFPermissionsInfo permissionsInfo)
        {
            CPDFDocumentEncryptionLevel encryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelNoEncryptAlgo;
            document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel);
            string encryptPath = Path.Combine(outputPath, "EncryptUseNoEncryptAlgoTest.pdf");
            if (!document.WriteToFilePath(encryptPath))
            {
                return false;
            }

            CPDFDocument encryptedDoc = CPDFDocument.InitWithFilePath(encryptPath);
            if (encryptedDoc.IsEncrypted)
            {
                Console.WriteLine("File is encrypted.");
                Console.WriteLine("Browse the changed file in " + encryptPath);
                Console.WriteLine("User password is: {0}", userPassword);
            }
            else
            {
                Console.WriteLine("File encrypt failed");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Using RC4, AES128, AES256, NoEncrypt algorithm to encrypt document. 
        /// User password: User
        /// No owner password
        /// </summary>
        /// <param name="document">Regular document</param>
        static private bool EncryptByUserPassword(CPDFDocument document)
        {
            bool result = true;

            userPassword = "User";
            ownerPassword = string.Empty;
            CPDFPermissionsInfo permissionsInfo = new CPDFPermissionsInfo();

            if (EncryptUseRC4Algo(document, permissionsInfo))
            {
                Console.WriteLine("RC4 encrypt done.\n");
            }
            else
            {
                Console.WriteLine("RC4 encrypt failed.\n");
                result = false;
            }

            if (EncryptUseAES128Algo(document, permissionsInfo))
            {
                Console.WriteLine("AES128 encrypt done.\n");
            }
            else
            {
                Console.WriteLine("AES128 encrypt failed.\n");
                result = false;
            }

            if (EncryptUseAES256Algo(document, permissionsInfo))
            {
                Console.WriteLine("AES256 encrypt done.\n");
            }
            else
            {
                Console.WriteLine("AES256 encrypt failed.\n");
                result = false;
            }

            if (EncryptUseNoEncryptAlgo(document, permissionsInfo))
            {
                Console.WriteLine("NoEncryptAlgo encrypt done.\n");
            }
            else
            {
                Console.WriteLine("NoEncryptAlgo encrypt failed.\n");
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Encrypt by owner password
        /// No user password
        /// Owner password: Owner
        /// </summary>
        /// <param name="document">Regular document</param> 
        static private bool EncryptByOwnerPassword(CPDFDocument document)
        {
            userPassword = null;
            ownerPassword = "Owner";
            CPDFPermissionsInfo permissionsInfo = new CPDFPermissionsInfo();
            permissionsInfo.AllowsPrinting = false;
            permissionsInfo.AllowsCopying = false;
            CPDFDocumentEncryptionLevel encryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelRC4;
            document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel);
            string encryptPath = Path.Combine(outputPath, "EncryptByOwnerPasswordTest.pdf");
            if (!document.WriteToFilePath(encryptPath))
            {
                return false;
            }

            CPDFDocument encryptedDoc = CPDFDocument.InitWithFilePath(encryptPath);
            if (encryptedDoc.IsEncrypted)
            {
                Console.WriteLine("File is encrypted.");
                Console.WriteLine("Browse the changed file in " + encryptPath);
                Console.WriteLine("Owner password is: {0}", ownerPassword);
            }
            else
            {
                Console.WriteLine("File encrypt failed");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Encrypt by all passwords
        /// User password: User
        /// Owner password: Owner
        /// </summary>
        /// <param name="document">Regular document</param> 
        static private bool EncryptByAllPasswords(CPDFDocument document)
        {
            userPassword = "User";
            ownerPassword = "Owner";
            CPDFPermissionsInfo permissionsInfo = new CPDFPermissionsInfo();
            permissionsInfo.AllowsPrinting = false;
            permissionsInfo.AllowsCopying = false;
            CPDFDocumentEncryptionLevel encryptionLevel = CPDFDocumentEncryptionLevel.CPDFDocumentEncryptionLevelRC4;
            document.Encrypt(userPassword, ownerPassword, permissionsInfo, encryptionLevel);
            string encryptPath = Path.Combine(outputPath, "EncryptByAllPasswordsTest.pdf");
            if (!document.WriteToFilePath(encryptPath))
            {
                return false;
            }

            CPDFDocument encryptedDoc = CPDFDocument.InitWithFilePath(encryptPath);
            if (encryptedDoc.IsEncrypted)
            {
                Console.WriteLine("File is encrypted.");
                Console.WriteLine("Browse the changed file in " + encryptPath);
                Console.WriteLine("User password is: {0}", userPassword);
                Console.WriteLine("Owner password is: {0}", ownerPassword);
            }
            else
            {
                Console.WriteLine("File encrypt failed");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Print printing and copying permissions info.
        /// </summary>
        /// <param name="permissionsInfo">Permissions info about a document</param>
        static private void PrintPermissionsInfo(CPDFPermissionsInfo permissionsInfo)
        {
            Console.Write("AllowsPrinting: ");
            Console.Write(permissionsInfo.AllowsPrinting == true ? "Yes\n" : "No\n");
            Console.Write("AllowsCopying: ");
            Console.Write(permissionsInfo.AllowsCopying == true ? "Yes\n" : "No\n");
        }

        /// <summary>
        /// Unlock document
        /// </summary>
        /// <param name="document"></param>
        static private bool Unlock(CPDFDocument document)
        {
            userPassword = "User";
            ownerPassword = "Owner";
            //Check if document is locked
            if (document.IsLocked)
            {
                Console.WriteLine("Document is locked");
            }
               
            PrintPermissionsInfo(document.GetPermissionsInfo());
             
            Console.WriteLine("Unlock with owner password");
            document.CheckOwnerPassword("123");
            //Check permissions info.
            PrintPermissionsInfo(document.GetPermissionsInfo());
            return true;
        }

        /// <summary>
        /// Decrypted
        /// </summary>
        /// <param name="document"></param>
        static private bool Decrypt(CPDFDocument document)
        {
            userPassword = "User";
            ownerPassword = "Owner";
            string decryptPath = Path.Combine(outputPath, "DecryptTest.pdf");
            document.UnlockWithPassword(userPassword);
            if (!document.Decrypt(decryptPath))
            {
                return false;
            }
            CPDFDocument decryptDocument = CPDFDocument.InitWithFilePath(decryptPath);
            if (decryptDocument.IsEncrypted)
            {
                return false;
            }
            else
            {
                Console.WriteLine("Document decrypt done.");
            } 
            return true;
        }
    }
}
