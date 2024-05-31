using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.IO; 

namespace FlattenTest
{
    internal class FlattenTest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");
        static void Main(string[] args)
        {
            #region Perparation work
            Console.WriteLine("Running Flatten test sample…\r\n");
            SDKLicenseHelper.LicenseVerify();
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            #endregion

            #region Sample1: Flatten

            CPDFDocument document = CPDFDocument.InitWithFilePath("Annotations.pdf");

            if (Flatten(document))
            {
                Console.WriteLine("Flatten done.");
            }
            else
            {
                Console.WriteLine("Flatten failed.");
            }
            Console.WriteLine("--------------------");
            document.Release();

            #endregion

            Console.WriteLine("Done");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Flatten documentation with comments
        /// </summary>
        /// <param name="document">document with many annotation. </param>
        /// <returns></returns>
        static private bool Flatten(CPDFDocument document)
        {
            int annotationCount = 0;
            for (int i = 0; i < document.PageCount; i++)
            {
                CPDFPage page = document.PageAtIndex(i);
                annotationCount += page.GetAnnotCount();
            }
            Console.Write("{0} annotations in the file. ", annotationCount);
            string flattenPath = Path.Combine(outputPath, "FlattenTest.pdf");
            if (!document.WriteFlattenToFilePath(flattenPath))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + flattenPath);

            //Verify: Check if the number of comments in the new document is 0
            CPDFDocument flattenDocument = CPDFDocument.InitWithFilePath(flattenPath);
            int newCount = 0;
            for (int i = 0; i < flattenDocument.PageCount; i++)
            {
                CPDFPage page = flattenDocument.PageAtIndex(i);
                newCount += page.GetAnnotCount();
            }
            Console.WriteLine("{0} annotations in the new file. ", newCount);
            if (!(newCount == 0))
            {
                return false;
            }
            return true;

        }
    }
}
