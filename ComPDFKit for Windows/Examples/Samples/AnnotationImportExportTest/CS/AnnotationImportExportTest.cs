using ComPDFKit.PDFDocument;
using System;
using System.IO; 

namespace AnnotationImportExportTest
{
    internal class AnnotationImportExportTest
    {
        static private string parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
        static private string outputPath = Path.Combine(parentPath, "Output", "CS");
        private static string tempPath = Path.Combine(outputPath, "temp");

        static void Main(string[] args)
        {

            Console.WriteLine("Running header and footer test sample…" + Environment.NewLine);

            #region Preparation work

            SDKLicenseHelper.LicenseVerify();
             
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            #endregion

            #region Sample 1: Export Annotation 

            CPDFDocument annotationsDocument = CPDFDocument.InitWithFilePath("Annotations.pdf"); 
            if (ExportAnnotaiton(annotationsDocument))
            {
                Console.WriteLine("Export annotaiton done.");
            }
            Console.WriteLine("--------------------");
            
            #endregion

            #region Sample 2: Import Annotations

            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf"); 
            if (ImportAnnotaiton(document))
            {
                Console.WriteLine("Import annotaiton done.");
            }

            Console.WriteLine("--------------------");
            
            #endregion
             
            Console.WriteLine("Done");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        /// <summary>
        ///  Export the annotations in the document to XFDF format.
        ///  Parameters in function "ImportAnnotationFromXFDFPath":
        ///  1. path: The path to the exported XFDF file
        ///  2. tempPath: The path for storing temporary files. 
        /// </summary>
        /// <param name="document">A document with multiple annotations</param>
        /// <returns></returns>
        static private bool ExportAnnotaiton(CPDFDocument document)
        {
            var path = Path.Combine(outputPath, "ExportAnnotationTest.xfdf");
            if (!document.ExportAnnotationToXFDFPath(path, tempPath))
            {
                return false;
            }
            Console.WriteLine("Xfdf file in " + path);

            return true;
        }

        /// <summary>
        ///  Importing XFDF into the document.
        ///  Parameters in function "ImportAnnotationFromXFDFPath":
        ///  1. The path to the XFDF file for importing.
        ///  2. The path for storing temporary files.  
        /// </summary>
        /// <param name="document">A document without annotations used for importing XFDF.</param>
        /// <returns></returns>
        static private bool ImportAnnotaiton(CPDFDocument document)
        {
            var path = Path.Combine(outputPath, "ImportAnnotationTest.pdf");

            if (!document.ImportAnnotationFromXFDFPath("Annotations.xfdf", tempPath))
            {
                return false;
            }
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path); 
            return true;
        }
    }
}
