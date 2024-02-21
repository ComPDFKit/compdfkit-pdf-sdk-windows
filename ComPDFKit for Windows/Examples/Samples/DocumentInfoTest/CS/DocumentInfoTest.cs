using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentInfoTest
{
    internal class DocumentInfoTest
    {
        static void Main(string[] args)
        { 
            Console.WriteLine("Running DocumentInfo test sample…\r\n");
            SDKLicenseHelper.LicenseVerify();

            #region Sample 1: Print information
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");
            PrintDocumentInfo(document); 
            Console.WriteLine("--------------------");
            #endregion

            Console.WriteLine("Done.");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Returns the file size based on the specified file path, with the smallest unit being bytes (B).
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>
        /// The calculated file size, with units in bytes (B), kilobytes (KB), megabytes (MB), or gigabytes (GB).
        /// </returns>
        public static string GetFileSize(string filePath)
        {
            FileInfo fileInfo = null;
            try
            {
                fileInfo = new FileInfo(filePath);
            }
            catch
            {
                return "0B";
            }
            if (fileInfo != null && fileInfo.Exists)
            {
                double fileSize = fileInfo.Length;
                if (fileSize > 1024)
                {
                    fileSize = Math.Round(fileSize / 1024, 2);
                    if (fileSize > 1024)
                    {
                        fileSize = Math.Round(fileSize / 1024, 2);
                        if (fileSize > 1024)
                        {
                            fileSize = Math.Round(fileSize / 1024, 2);
                            return fileSize + " GB";
                        }
                        else
                        {
                            return fileSize + " MB";
                        }
                    }
                    else
                    {
                        return fileSize + " KB";
                    }
                }
                else
                {
                    return fileSize + " B";
                }
            }
            return "0B";
        }

        /// <summary>
        /// Print all of the infomations
        /// </summary> 
        static private void PrintDocumentInfo(CPDFDocument document)
        {
            Console.WriteLine("File Name: {0}", document.FileName);
            Console.WriteLine("File Size: {0}", GetFileSize(document.FilePath));
            Console.WriteLine("Title: {0}", document.GetInfo().Title);
            Console.WriteLine("Author: {0}", document.GetInfo().Author);
            Console.WriteLine("Subject: {0}", document.GetInfo().Subject);
            Console.WriteLine("Keywords: {0}", document.GetInfo().Keywords);
            Console.WriteLine("Version: {0}", document.GetInfo().Version);
            Console.WriteLine("Page Count: {0}", document.PageCount);
            Console.WriteLine("Creator: {0}", document.GetInfo().Creator);
            Console.WriteLine("Creation Data: {0}",document.GetInfo().CreationDate);
            Console.WriteLine("Allows Printing: {0}", document.GetPermissionsInfo().AllowsPrinting);
            Console.WriteLine("Allows Copying: {0}", document.GetPermissionsInfo().AllowsCopying);
            Console.WriteLine("Allows Document Changes: {0}", document.GetPermissionsInfo().AllowsDocumentChanges);
            Console.WriteLine("Allows Document Assembly: {0}", document.GetPermissionsInfo().AllowsDocumentAssembly);
            Console.WriteLine("Allows Commenting: {0}", document.GetPermissionsInfo().AllowsCommenting);
            Console.WriteLine("Allows FormField Entry: {0}", document.GetPermissionsInfo().AllowsFormFieldEntry);
        }
    }
}
