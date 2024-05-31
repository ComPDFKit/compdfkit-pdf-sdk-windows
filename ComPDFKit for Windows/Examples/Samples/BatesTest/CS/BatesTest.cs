using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.IO; 

namespace BatesTest
{
    internal class BatesTest
    {
        static private string parentPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
        static private string outputPath = Path.Combine(parentPath, "Output", "CS");

        private static Dictionary<int, string> IntToLocationDic = new Dictionary<int, string>()
        {
            {0, "Top Left" },
            {1, "Top Middle" },
            {2, "Top Right" },
            {3, "Bottom Left" },
            {4, "Bottom Middle" },
            {5, "Bottom Right" }
        };

        static void Main(string[] args)
        {
            #region Preparation work

            Console.WriteLine("Running bates test sample…\r\n");
            SDKLicenseHelper.LicenseVerify();


            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            #endregion

            #region Sample 1: Add bates
            CPDFDocument document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (AddBates(document))
            {
                Console.WriteLine("Add bates done.");
            } 

            document.Release();

            Console.WriteLine("--------------------");
            #endregion

            #region Samles 2: Edit bates
             document = CPDFDocument.InitWithFilePath("Bates.pdf");

            if (EditBates(document))
            {
                Console.WriteLine("Edit bates done.");
            } 

            document.Release();

            Console.WriteLine("--------------------");

            #endregion

            #region Sample 3: Clear bates
            document = CPDFDocument.InitWithFilePath("CommonFivePage.pdf");

            if (ClearBates(document))
            {
                Console.WriteLine("Clear bates done.");
            }

            document.Release();
            Console.WriteLine("--------------------");

            #endregion

            Console.WriteLine("Done!");
            Console.WriteLine("--------------------");
             
            Console.ReadLine();
        }

        /// <summary>
        /// Add a new bates
        /// </summary>
        /// <param name="document">Regular document</param> 
        private static bool AddBates(CPDFDocument document)
        {
            string addBatesPath = Path.Combine(outputPath, "AddBatesTest.pdf");

            CPDFBates bates = document.GetBates();
            byte[] color = { 255, 0, 0 };
             
            bates.SetPages("0-" + (document.PageCount - 1));//Page numbering from 0

            for (int i = 0; i <= 5; i++)
            {
                bates.SetText(i, @"<<#3#5#Prefix-#-Suffix>>");  //3 digits, starting from 5
                bates.SetTextColor(i, color);
                bates.SetFontSize(i, 14);
                 
                Console.WriteLine("Text: {0}", bates.GetText(i));
                Console.WriteLine("Location: {0}\n", IntToLocationDic[i]);
            }

            bates.Update();

            if (!document.WriteToFilePath(addBatesPath))
            {
                return false;
            }
            Console.WriteLine("Browse the changed file in " + addBatesPath);
            return true;
        }

        /// <summary>
        /// Edit bates, <<#3#5#Prefix-#-Suffix>> -> <<#3#1#ComPDFKit-#-ComPDFKit>>
        /// get current bates,
        /// then edit it
        /// </summary>
        /// <param name="document">documet with bates</param> 
        private static bool EditBates(CPDFDocument document)
        {

            CPDFBates bates = document.GetBates();
            if(bates.GetText(0) != string.Empty)
            {
                Console.WriteLine("Get old bates 0 done, text is {0}", bates.GetText(0));
            }
            else
            {
                Console.WriteLine("Get bates 0 failed, or it does not exist");
                return false;
            }

            bates.SetText(0, @"<<#3#1#ComPDFKit-#-ComPDFKit>>");

            bates.Update();
             
            Console.WriteLine("Change bates 0 done, new text is {0}", bates.GetText(0));

            string editBatesPath = Path.Combine(outputPath, "EditBatesTest.pdf");

            if (document.WriteToFilePath(editBatesPath))
            {
                Console.WriteLine("Browse the changed file in " + editBatesPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Clear bates.
        /// </summary>
        /// <param name="document">documet with bates</param> 
        private static bool ClearBates(CPDFDocument document)
        {
            CPDFBates bates = document.GetBates();

            bates.Clear();

            string clearBatesPath = Path.Combine(outputPath, "ClearBatesTest.pdf");

            if (document.WriteToFilePath(clearBatesPath))
            {
                Console.WriteLine("Browse the changed file in " + clearBatesPath);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
