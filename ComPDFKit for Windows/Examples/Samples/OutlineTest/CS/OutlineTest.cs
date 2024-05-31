using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using System;
using System.Collections.Generic;
using System.IO; 

namespace OutlineTest
{
    internal class OutlineTest
    {
        private static string outputPath =Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()))) ?? string.Empty, "Output", "CS");
        static private int outlineCounter = 0;
        static private int outlineNumber = 0;

        static void Main(string[] args)
        {
            #region Perparation work

            Console.WriteLine("Running Outline test sample…\r\n");

            SDKLicenseHelper.LicenseVerify();

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            #endregion

            #region Sample 1: Print outline

            CPDFDocument document = CPDFDocument.InitWithFilePath("FourOutline.pdf");
                                                                                                
            if (PrintOutline(document))
            {
                Console.WriteLine("Print outline done.");
            }
            else
            {
                Console.WriteLine("Print outline failed.");
            }
            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 2: Create outline

            document = CPDFDocument.InitWithFilePath("FourOutline.pdf");

            if (CreateOutline(document))
            {
                Console.WriteLine("Create outline done.");
            }
            else
            {
                Console.WriteLine("Create outline failed.");
            }
            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 3: Move outline

            document = CPDFDocument.InitWithFilePath("FourOutline.pdf");
            if (MoveOutline(document))
            {
                Console.WriteLine("Move outline done.");
            }
            else
            {
                Console.WriteLine("Move outline failed.");
            }
            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 4: Remove outline

            document = CPDFDocument.InitWithFilePath("FourOutline.pdf");
            if (RemoveOutline(document))
            {
                Console.WriteLine("Remove outline done.");
            }
            else
            {
                Console.WriteLine("Remove outline failed.");
            }
            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            Console.WriteLine("Done.");
            Console.WriteLine("--------------------");

            Console.ReadLine();
        }

        /// <summary>
        /// Traverse outline and print it as a list
        /// </summary>
        static private void TraverseOutline(List<CPDFOutline> outlineList)
        {
            foreach (var outline in outlineList)
            {
                for (var i = 0; i < outlineCounter; i++)
                {
                    Console.Write("    ");
                }
                Console.Write("-> " + outline.Title + "\n");
                outlineNumber++;
                var childList = outline.ChildList;
                if (childList != null && childList.Count != 0)
                {
                    outlineCounter++;
                    TraverseOutline(childList);
                }
                else
                { 
                    if (outlineList.IndexOf(outline)+1 == outlineList.Count)
                    {
                        outlineCounter--;
                    }
                }
            }
        }

        /// <summary>
        /// Print all outlines in the file
        /// </summary>
        /// <param name="document">Document with some outlines</param> 
        static private bool PrintOutline(CPDFDocument document)
        {
            List<CPDFOutline> outlineList = document.GetOutlineList();
            outlineNumber = 0;
            outlineCounter = 0;
            TraverseOutline(outlineList);
            Console.WriteLine("Outline number: {0}", outlineNumber);
            return true;
        }

        /// <summary>
        /// Create an outline at the top of the first page
        /// </summary>
        /// <param name="document">Document with some outlines</param>
        static private bool CreateOutline(CPDFDocument document)
        {
            CPDFOutline outline = document.GetOutlineRoot();
            CPDFOutline childOutline = null;
            outline.InsertChildAtIndex(document, 0, ref childOutline);
            CPDFGoToAction gotoAction = new CPDFGoToAction();
            CPDFDestination dest = new CPDFDestination();
            dest.PageIndex = 1;
            gotoAction.SetDestination(document, dest);
            childOutline.SetAction(gotoAction);
            childOutline.SetTitle("New outline"); 
            PrintOutline(document);
            return true;
        }

        /// <summary>
        /// Create an outline at the top of the first page
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        static private bool MoveOutline(CPDFDocument document)
        {
            CPDFOutline outline = document.GetOutlineRoot();
            outline.InsertChildAtIndex(document, outline.ChildList.Count, ref outline);
            outline.SetTitle("new outline");
            CPDFOutline targetOutline = document.GetOutlineList()[1];
            targetOutline.MoveChildAtIndex(document, outline, targetOutline.ChildList.Count);

            string path = Path.Combine(outputPath, "MoveOutlineTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            CPDFDocument newDocument = CPDFDocument.InitWithFilePath(path);
            PrintOutline(newDocument);
            return true;
        }

        /// <summary>
        /// Remove outline
        /// </summary>
        static private bool RemoveOutline(CPDFDocument document)
        { 
            document.GetOutlineList()[0].RemoveFromParent(document);

            string path = Path.Combine(outputPath, "RemoveOutlineTest.pdf");
            if (!document.WriteToFilePath(path))
            {
                return false;
            }
            CPDFDocument newDocument = CPDFDocument.InitWithFilePath(path);
            PrintOutline(newDocument);
            return true;
        }
    }
}
