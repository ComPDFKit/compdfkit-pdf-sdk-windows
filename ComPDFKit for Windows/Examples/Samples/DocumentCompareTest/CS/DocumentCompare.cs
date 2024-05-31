using ComPDFKit.Compare;
using ComPDFKit.PDFDocument;
using System; 
using System.IO; 

namespace DocumentCompareTest
{
    internal class DocumentCompare
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");

        static void Main(string[] args)
        {
            Console.WriteLine("Running PDFPage test sample…\r\n");

            SDKLicenseHelper.LicenseVerify();
            

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            CPDFDocument document1 = CPDFDocument.InitWithFilePath("ElectricalDiagram.pdf");
            CPDFDocument document2 = CPDFDocument.InitWithFilePath("ElectricalDiagram_New.pdf");
            if (OverlayCompareDocument(document1, document2))
            {
                Console.WriteLine("Compare document done.");
            }
            else
            {
                Console.WriteLine("Compare document failed."); 
            }
            document1.Release();
            document2.Release();
            Console.WriteLine("--------------------");

            CPDFDocument document3 = CPDFDocument.InitWithFilePath("Text.pdf");
            CPDFDocument document4 = CPDFDocument.InitWithFilePath("TextChanged.pdf");
            if (ContentCompareDocument(document3, document4))
            {
                Console.WriteLine("Compare document done.");
            }
            else
            {
                Console.WriteLine("Compare document failed.");
            }
            document3.Release();
            document4.Release();
            Console.WriteLine("--------------------");

            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");
            Console.ReadLine();
        }

        static private bool OverlayCompareDocument(CPDFDocument document1, CPDFDocument document2)
        {
            CPDFCompareOverlay compareOverlay = new CPDFCompareOverlay(document1, "1-5", document2, "1-5");
            compareOverlay.Compare();
            CPDFDocument comparisonDocument = compareOverlay.ComparisonDocument();
            string path = Path.Combine(outputPath, "CompareDocumentTest.pdf");
            if (!comparisonDocument.WriteToFilePath(path))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + path);
            return true;
        }

        static private bool ContentCompareDocument(CPDFDocument document, CPDFDocument documentNew)
        {
            CPDFCompareContent compareContent = new CPDFCompareContent(document, documentNew);
            int pageCount = Math.Min(document.PageCount, documentNew.PageCount);
            for (int i = 0; i < pageCount; i++)
            {
                Console.WriteLine("Page: {0}", i);

                CPDFCompareResults compareResults = compareContent.Compare(i, i, CPDFCompareType.CPDFCompareTypeAll, true);
                Console.WriteLine("Replace count: {0}", compareResults.ReplaceCount);
                Console.WriteLine("TextResults count: {0}", compareResults.TextResults.Count);
                Console.WriteLine("Delete count: {0}", compareResults.DeleteCount);
                Console.WriteLine("Insert count: {0}", compareResults.InsertCount); 
            }
            return true;
        }
    }
}
