using ComPDFKit.Controls.PDFControl;
using ComPDFKit.DigitalSign;
using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace ComPDFKit.Controls.Printer
{
    public enum DuplexingStage
    {
        None,
        FontSide,
        BackSide
    }

    public static class PrintHelper
    {
        private static PrintSettingsInfo printSettingsInfo;
        private static int MaxCopies = 1;
        private static bool FinishedFrontSide = false;
        private static bool isManualDuplex = false;
        private static List<OutputColor> outputColors;
        private static List<Duplexing> duplexings;
        private static Bitmap blankPageBitmapForPrint;
        private static int PrintIndex;

        public static PrintQueue printQueue;

        public static void InitPrint()
        {
            MaxCopies = 1;
            FinishedFrontSide = false;
            isManualDuplex = false;
            outputColors = new List<OutputColor>();
            duplexings = new List<Duplexing>();
            blankPageBitmapForPrint = null;
            PrintIndex = 0;
        }

        private static void SetPrinterLimits()
        {
            MaxCopies = printQueue.GetPrintCapabilities().MaxCopyCount.HasValue ? printQueue.GetPrintCapabilities().MaxCopyCount.Value : 1;
            outputColors = new List<OutputColor>(printQueue.GetPrintCapabilities().OutputColorCapability);
            duplexings = new List<Duplexing>(printQueue.GetPrintCapabilities().DuplexingCapability);
        }
         
        public static PrintQueue GetPrintQueue(PrintServer server, string printerName)
        {
            return server.GetPrintQueue(printerName);
        }

        public static void PrintDocument(PrintSettingsInfo printSettings)
        { 
            printSettingsInfo = printSettings; 

            SetPrinterLimits();

            if (printSettings.DuplexPrintMod == DuplexPrintMod.None ||
                (printSettings.PrintMode is BookletModeInfo bookletModeInfo && bookletModeInfo.Subset != BookletSubset.BothSides))
            {
                HandlePrintQueue(DuplexingStage.None);
            }
            else if (printSettings.DuplexPrintMod > 0 ||
                (printSettings.PrintMode is BookletModeInfo bookletModeInfo1 && bookletModeInfo1.Subset == BookletSubset.BothSides))
            {
                if (duplexings.Contains(Duplexing.TwoSidedShortEdge) || duplexings.Contains(Duplexing.TwoSidedLongEdge))
                {
                    HandlePrintQueue(DuplexingStage.None);
                }
                else
                {
                    if (OnlyOnePage())
                    {
                        HandlePrintQueue(DuplexingStage.None);
                    }
                    else
                    {
                        HandlePrintQueue(DuplexingStage.FontSide);
                        if (FinishedFrontSide)
                        {
                            DialogResult dialogResult = MessageBox.Show("Print the back side.", "Print", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.Yes)
                            {
                                HandlePrintQueue(DuplexingStage.BackSide);
                            }
                        }
                    }
                }
            }

            bool OnlyOnePage()
            {
                return ((printSettings.PrintMode is SizeModeInfo || printSettings.PrintMode is PosterModeInfo) &&
                    printSettings.PageRangeList.Count == 1) ||
                    (printSettings.PrintMode is MultipleModeInfo multipleModeInfo &&
                    (int)Math.Ceiling(printSettings.PageRangeList.Count / (double)(multipleModeInfo.Sheet.TotalPageNumber)) == 1);
            }
        }

        private static void HandlePrintQueue(DuplexingStage stage)
        {
            if (printQueue != null)
            {
                using (var printDocument = new PrintDocument())
                {
                    var printTicket = new PrintTicket();

                    printDocument.PrinterSettings.PrinterName = printQueue.Name;
                      
                    printDocument.DefaultPageSettings.Color = !printSettingsInfo.IsGrayscale && outputColors.Contains(OutputColor.Color);

                    printTicket.PageOrientation = printSettingsInfo.PrintOrientation;
                      
                    if (printSettingsInfo.DuplexPrintMod > 0 || printSettingsInfo.PrintMode is BookletModeInfo)
                    {
                        if (stage == DuplexingStage.None)
                        {
                            if (printSettingsInfo.DuplexPrintMod == DuplexPrintMod.FlipLongEdge)
                            {
                                printDocument.PrinterSettings.Duplex = Duplex.Horizontal;
                            }
                            else
                            {
                                printDocument.PrinterSettings.Duplex = Duplex.Vertical; 
                            }
                            PrintIndex = 0;
                        }
                        else
                        {
                            printDocument.PrinterSettings.Duplex = Duplex.Simplex;
                            isManualDuplex = true;
                            PrintIndex = stage == DuplexingStage.FontSide ? 0 : 1;
                        }
                    }
                    else
                    {
                        printDocument.PrinterSettings.Duplex = Duplex.Simplex;
                        PrintIndex = 0;
                    }

                    printQueue.DefaultPrintTicket = printTicket;
                    printDocument.DefaultPageSettings.PaperSize = printSettingsInfo.PaperSize;
                    printDocument.DefaultPageSettings.Landscape = (printSettingsInfo.PrintOrientation == PageOrientation.Landscape);
                    List<PaperSource> paperSources;
                    if (printDocument.PrinterSettings.PaperSources.Count > 0)
                    {
                        paperSources = Enumerable.Range(0, printDocument.PrinterSettings.PaperSources.Count).
    Select(i => printDocument.PrinterSettings.PaperSources[i]).ToList();
                    }
                    else
                    {
                        PaperSource defaultPaperSource = new PaperSource();
                        defaultPaperSource.SourceName = "Default";
                        paperSources = new List<PaperSource> { defaultPaperSource };
                    }

                    printDocument.DefaultPageSettings.Margins = new Margins() { Left = (int)printSettingsInfo.Margins.Left, Bottom = (int)printSettingsInfo.Margins.Bottom, Right = (int)printSettingsInfo.Margins.Right, Top = (int)printSettingsInfo.Margins.Top};
                    if (printSettingsInfo.Copies >= printQueue.GetPrintCapabilities().MaxCopyCount)
                    {
                        printDocument.PrinterSettings.Copies = (short)MaxCopies;
                    }
                    else
                    {
                        printDocument.PrinterSettings.Copies = (short)printSettingsInfo.Copies;
                    }

                    printDocument.PrintPage -= PrintDocument_PrintPage;
                    printDocument.PrintPage += PrintDocument_PrintPage;

                    printDocument.PrinterSettings.PrintFileName = printSettingsInfo.Document.FileName;

                    try
                    {
                        printDocument.Print();
                        if(stage == DuplexingStage.FontSide)
                        {
                            FinishedFrontSide = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }
            }
        }

        public static Bitmap ToGray(Bitmap bmp, int mode)
        {
            if (bmp == null)
            {
                return null;
            }

            int w = bmp.Width;
            int h = bmp.Height;
            try
            {
                byte newColor = 0;
                BitmapData srcData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* p = (byte*)srcData.Scan0.ToPointer();
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            if (mode == 0) 
                            {
                                newColor = (byte)((float)p[0] * 0.114f + (float)p[1] * 0.587f + (float)p[2] * 0.299f);
                            }
                            else   
                            {
                                newColor = (byte)((float)(p[0] + p[1] + p[2]) / 3.0f);
                            }
                            p[0] = newColor;
                            p[1] = newColor;
                            p[2] = newColor;

                            p += 3;
                        }
                        p += srcData.Stride - w * 3;
                    }
                    bmp.UnlockBits(srcData);
                    return bmp;
                }
            }
            catch
            {
                return null;
            }
        }

        public static Bitmap BuildBmp(int width, int height, byte[] imgBytes)
        {
            // Check if the byte array length matches the expected size
            int expectedLength = width * height * 4;
            if (imgBytes.Length != expectedLength)
            {
                throw new ArgumentException("The length of imgBytes does not match the expected size.");
            }

            // Create a new bitmap with the specified width, height, and pixel format
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Lock the bitmap's bits for writing
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            try
            {
                // Copy the byte array to the bitmap's scan0 pointer
                Marshal.Copy(imgBytes, 0, bmpData.Scan0, imgBytes.Length);
            }
            finally
            {
                // Ensure that the bitmap is always unlocked, even if an exception occurs
                bitmap.UnlockBits(bmpData);
            }

            return bitmap;
        }

        public static int CaculatePrintedPageCount(PrintSettingsInfo printSettingsInfo)
        {
            int tempCount = printSettingsInfo.PageRangeList.Count;
            if (printSettingsInfo.PrintMode is MultipleModeInfo multipleModeInfo)
            {
                tempCount = (int)Math.Ceiling((double)tempCount /
                    multipleModeInfo.Sheet.TotalPageNumber);
            }
            else if (printSettingsInfo.PrintMode is BookletModeInfo bookletInfo)
            {
                if (tempCount != 1)
                {
                    tempCount = (bookletInfo.EndPageIndex - bookletInfo.BeginPageIndex + 1) / 2;
                    if (bookletInfo.Subset != BookletSubset.BothSides)
                    {
                        tempCount /= 2;
                    }
                }
            }
            return tempCount;
        }

        public static Bitmap CombineBitmap(Bitmap background, Bitmap foreground, System.Drawing.Point point)
        {
            if (background == null)
            {
                return null;
            }
            int bgWidth = background.Width;
            int bgHeight = background.Height;
            Bitmap newMap = new Bitmap(bgWidth, bgHeight);
            Graphics graphics = Graphics.FromImage(newMap);
            graphics.DrawImage(background, new System.Drawing.Point(0, 0));
            graphics.DrawImage(foreground, point);
            graphics.Dispose();
            return newMap;
        }

        private static void PrintDocumentModSize(PrintPageEventArgs e)
        {
            int PrintedPageCount = CaculatePrintedPageCount(printSettingsInfo);
            int widthDpiRatio = (int)e.Graphics.DpiX / 100;
            int heightDpiRatio = (int)e.Graphics.DpiY / 100;

            if (printSettingsInfo.PrintMode is SizeModeInfo sizeMode)
            {
                CPDFPage page = printSettingsInfo.Document.PageAtIndex(printSettingsInfo.TargetPaperList[PrintIndex]);
                Rectangle realBound = e.PageBounds;
                if (!printSettingsInfo.IsBorderless)
                {
                    realBound.Width = realBound.Width - (int)printSettingsInfo.Margins.Left - (int)printSettingsInfo.Margins.Right;
                    realBound.Height = realBound.Height - (int)printSettingsInfo.Margins.Top - (int)printSettingsInfo.Margins.Bottom;
                }
                Margins margins = e.PageSettings.Margins;
                Point point = new Point(0, 0);

                if (page != null)
                {
                    CSize cSize = page.PageSize;
                    System.Drawing.Size pageSize = new System.Drawing.Size((int)cSize.width * widthDpiRatio, (int)cSize.height * heightDpiRatio);

                    byte[] bmpData = new byte[(int)(pageSize.Width * pageSize.Height * 4)];
                    Bitmap bitmap = null;
                    if (PrintHelper.IsPageHaveSignAP(page))
                    {
                        bitmap = GetPageBitmapWithFormDynamicAP(printSettingsInfo.Document, page, widthDpiRatio, heightDpiRatio, new CRect(0, pageSize.Height, pageSize.Width, 0), 0xFFFFFFFF, bmpData, printSettingsInfo.IsPrintAnnot ? 1 : 0, printSettingsInfo.IsPrintForm);
                    }

                    if (bitmap == null)
                    {
                        page.RenderPageBitmap(0, 0, pageSize.Width, pageSize.Height, 0xFFFFFFFF, bmpData, printSettingsInfo.IsPrintAnnot ? 1 : 0, printSettingsInfo.IsPrintForm);
                        bitmap = BuildBmp((int)pageSize.Width, (int)pageSize.Height, bmpData);
                    }

                    if (printSettingsInfo.IsGrayscale)
                    {
                        bitmap = ToGray(bitmap, 0);
                    }

                    if (sizeMode.SizeType == SizeType.Adaptive)
                    {
                        int resizeHeight;
                        int resizeWidth;

                        if (bitmap.Height / bitmap.Width >= (printSettingsInfo.ActualHeight / printSettingsInfo.ActualWidth))
                        {
                            if (printSettingsInfo.PrintOrientation == PageOrientation.Portrait)
                            {
                                resizeHeight = (int)printSettingsInfo.ActualHeight;
                                resizeWidth = (int)(printSettingsInfo.ActualHeight / bitmap.Height * bitmap.Width);
                            }
                            else
                            {
                                resizeWidth = (int)printSettingsInfo.ActualHeight;
                                resizeHeight = (int)(printSettingsInfo.ActualHeight / bitmap.Height * bitmap.Width);
                            }
                        }
                        else
                        {
                            if (printSettingsInfo.PrintOrientation == PageOrientation.Portrait)
                            {
                                resizeWidth = (int)printSettingsInfo.ActualWidth;
                                resizeHeight = (int)(printSettingsInfo.ActualWidth / bitmap.Width * bitmap.Height);
                            }
                            else
                            {
                                resizeHeight = (int)printSettingsInfo.ActualWidth;
                                resizeWidth = (int)(printSettingsInfo.ActualWidth / bitmap.Height * bitmap.Width);
                            }
                        }

                        using (Bitmap resizedBitmap = new Bitmap(bitmap, resizeWidth * widthDpiRatio, resizeHeight * heightDpiRatio))
                        {
                            if (isManualDuplex && PrintIndex % 2 == 1 && printSettingsInfo.DuplexPrintMod == DuplexPrintMod.FlipShortEdge)
                            {
                                resizedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            }
                            float aspectRatioResizedBitmap = (float)resizedBitmap.Width / resizedBitmap.Height;
                            float aspectRatioRealBound = (float)realBound.Width / realBound.Height;

                            if (aspectRatioResizedBitmap != aspectRatioRealBound)
                            {
                                if (realBound.Width / aspectRatioResizedBitmap <= realBound.Height)
                                {
                                    realBound.Height = (int)(realBound.Width / aspectRatioResizedBitmap);
                                }
                                else
                                {
                                    realBound.Width = (int)(realBound.Height * aspectRatioResizedBitmap);
                                }
                            }

                            int originX = (e.PageBounds.Width - realBound.Width)/2;
                            int originY = (e.PageBounds.Height - realBound.Height)/2;
                            if (!printSettingsInfo.IsBorderless)
                            {
                                if (originX < printSettingsInfo.Margins.Left)
                                {
                                    originX = (int)printSettingsInfo.Margins.Left;
                                }

                                if (originY < printSettingsInfo.Margins.Top)
                                {
                                    originY = (int)printSettingsInfo.Margins.Top;
                                }
                            } 

                            e.Graphics.DrawImage(resizedBitmap, new Rectangle(originX, originY, realBound.Width, realBound.Height), new Rectangle(0, 0, resizedBitmap.Width, resizedBitmap.Height), GraphicsUnit.Pixel);
                        }
                    }
                    else if (sizeMode.SizeType == SizeType.Actural)
                    {
                        using (Bitmap resizedBitmap = ResizeBitmap(bitmap, 100))
                        {
                            realBound.Width = (int)(cSize.width * 1.4);
                            realBound.Height = (int)(cSize.height * 1.4);
                            if (isManualDuplex && PrintIndex % 2 == 1 && printSettingsInfo.DuplexPrintMod == DuplexPrintMod.FlipShortEdge)
                            {
                                resizedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            }
                            float aspectRatioResizedBitmap = (float)resizedBitmap.Width / resizedBitmap.Height;
                            float aspectRatioRealBound = (float)realBound.Width / realBound.Height;

                            if (aspectRatioResizedBitmap != aspectRatioRealBound)
                            {
                                if (realBound.Width / aspectRatioResizedBitmap <= realBound.Height)
                                {
                                    realBound.Height = (int)(realBound.Width / aspectRatioResizedBitmap);
                                }
                                else
                                {
                                    realBound.Width = (int)(realBound.Height * aspectRatioResizedBitmap);
                                }
                            }
                            int originX = (e.PageBounds.Width - realBound.Width) / 2;
                            int originY = (e.PageBounds.Height - realBound.Height) / 2;
                            if (!printSettingsInfo.IsBorderless)
                            {
                                if (originX < printSettingsInfo.Margins.Left)
                                {
                                    originX = (int)printSettingsInfo.Margins.Left;
                                }

                                if (originY < printSettingsInfo.Margins.Top)
                                {
                                    originY = (int)printSettingsInfo.Margins.Top;
                                }
                            } 
                            e.Graphics.DrawImage(resizedBitmap, new Rectangle(originX, originY, realBound.Width, realBound.Height), new Rectangle(0, 0, resizedBitmap.Width, resizedBitmap.Height), GraphicsUnit.Pixel);
                        }
                    }
                    else if (sizeMode.SizeType == SizeType.Customized)
                    {
                        using (Bitmap resizedBitmap = ResizeBitmap(bitmap, (printSettingsInfo.PrintMode as SizeModeInfo).Scale))
                        {
                            if (isManualDuplex && PrintIndex % 2 == 1 && printSettingsInfo.DuplexPrintMod == DuplexPrintMod.FlipShortEdge)
                            {
                                resizedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            }
                            float aspectRatioResizedBitmap = (float)resizedBitmap.Width / resizedBitmap.Height;
                            float aspectRatioRealBound = (float)realBound.Width / realBound.Height;

                            realBound.Width = (int)(cSize.width * 1.4);
                            realBound.Height = (int)(cSize.height * 1.4);
                            realBound.Width = (int)(realBound.Width * (printSettingsInfo.PrintMode as SizeModeInfo).Scale / 100.0); 
                            realBound.Height = (int)(realBound.Height * (printSettingsInfo.PrintMode as SizeModeInfo).Scale / 100.0);

                            if (aspectRatioResizedBitmap != aspectRatioRealBound)
                            {
                                if (realBound.Width / aspectRatioResizedBitmap <= realBound.Height)
                                {
                                    realBound.Height = (int)(realBound.Width / aspectRatioResizedBitmap);
                                }
                                else
                                {
                                    realBound.Width = (int)(realBound.Height * aspectRatioResizedBitmap);
                                }
                            }

                            int originX = (e.PageBounds.Width - realBound.Width) / 2;
                            int originY = (e.PageBounds.Height - realBound.Height) / 2;
                            if (!printSettingsInfo.IsBorderless)
                            {
                                if (originX < printSettingsInfo.Margins.Left)
                                {
                                    originX = (int)printSettingsInfo.Margins.Left;
                                }

                                if (originY < printSettingsInfo.Margins.Top)
                                {
                                    originY = (int)printSettingsInfo.Margins.Top;
                                }
                            }
                            e.Graphics.DrawImage(resizedBitmap, new Rectangle(originX, originY, realBound.Width, realBound.Height),  new Rectangle(0, 0, resizedBitmap.Width, resizedBitmap.Height), GraphicsUnit.Pixel);
                        }
                    }

                    bitmap.Dispose();
                    GC.Collect();

                    if (isManualDuplex && PrintedPageCount != 1)
                    {
                        if (PrintIndex < PrintedPageCount - 2)
                        {
                            PrintIndex += 2;
                            e.HasMorePages = true;
                        }
                        else
                        {
                            e.HasMorePages = false;
                            if (PrintIndex % 2 == 0)
                            {
                                //
                            }
                        }
                    }
                    else
                    {
                        if (PrintIndex < PrintedPageCount - 1)
                        {
                            PrintIndex++;
                            e.HasMorePages = true;
                        }
                        else
                        {
                            e.HasMorePages = false;  
                        }
                    }
                }
            }
        }

        private static void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            switch (printSettingsInfo.PrintMode)
            {
                case SizeModeInfo _:
                    PrintDocumentModSize(e);
                    break;
                case PosterModeInfo _:
                    break;
                case MultipleModeInfo _: 

                    break;
                case BookletModeInfo _: 
                    break;
            }
        }

        internal static Bitmap ResizeBitmap(Bitmap bitmap, float scale)
        {
            int newWidth = (int)(bitmap.Width * scale / 72);
            int newHeight = (int)(bitmap.Height * scale / 72);
            Bitmap newBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
            }
            return newBitmap;
        }

        public static Bitmap GetPageBitmapWithFormDynamicAP(CPDFDocument pdfDoc, CPDFPage page,float scaleX,float scaleY, CRect rangeRect,uint bgColor, byte[] bmpData, int flag, bool form = false)
        {
            if (pdfDoc==null || pdfDoc.IsValid()==false || page == null || page.IsValid() == false)
            {
                return null;
            }

            if (bmpData == null || bmpData.Length == 0)
            {
                return null;
            }
            if(scaleX==scaleY)
            {
                page.RenderPageBitmapWithMatrix(scaleX, rangeRect, bgColor, bmpData, flag, false);
            }
            else
            {
                page.RenderPageBitmap((int)rangeRect.left, (int)rangeRect.top, (int)rangeRect.width(), (int)rangeRect.height(), bgColor, bmpData, flag, false);
            }

            if (form == false)
            {
                return BuildBmp((int)rangeRect.width(), (int)rangeRect.height(), bmpData);
            }

            List<CPDFAnnotation> annotList = page.GetAnnotations();
            if (annotList == null || annotList.Count == 0)
            {
                return BuildBmp((int)rangeRect.width(), (int)rangeRect.height(), bmpData);
            }

            Bitmap writeBitmap = BuildBmp((int)rangeRect.width(), (int)rangeRect.height(), bmpData);
            Graphics drawGraph = Graphics.FromImage(writeBitmap);
          
            foreach (CPDFAnnotation rawAnnot in annotList)
            {
                if (rawAnnot.Type != C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                {
                    continue;
                }
              
                CRect annotRect = rawAnnot.GetRect();
                int xPos = (int)(annotRect.left * scaleX);
                int yPos = (int)(annotRect.top * scaleY);
                int annotWidth = (int)(annotRect.width()*scaleX);
                int annotHeight = (int)(annotRect.height()*scaleY);

                int renderWidth = annotWidth;
                int renderHeight = annotHeight;

                System.Windows.Rect rotateRect = new System.Windows.Rect(xPos, yPos, annotWidth, annotHeight);
                if (page.Rotation != 0)
                {
                    Matrix rotateMatrix = new Matrix();
                    rotateMatrix.RotateAt(-90 * page.Rotation, rotateRect.Left + annotWidth / 2, rotateRect.Top + annotHeight / 2);
                    rotateRect.Transform(rotateMatrix);

                    renderWidth = (int)rotateRect.Width;
                    renderHeight = (int)rotateRect.Height;
                }

                byte[] annotData = new byte[renderWidth * renderHeight * 4];
                CPDFWidget widgetAnnot = rawAnnot as CPDFWidget;
                CPDFSignatureWidget signatureWidget = null;
                if (widgetAnnot.WidgetType==C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                {
                     signatureWidget = widgetAnnot as CPDFSignatureWidget;
                }
                bool getSignAp = false;
                if (signatureWidget != null && signatureWidget.IsSignAP())
                {
                    CPDFSignature signature = signatureWidget.GetSignature(pdfDoc);
                    if (signature != null)
                    {
                        signatureWidget.GetSignatureAppearance(renderWidth, renderHeight, annotData, signature.GetSignState());
                        getSignAp = true;
                    }
                }

                if (!getSignAp)
                {
                    rawAnnot.RenderAnnot(renderWidth, renderHeight, annotData, CPDFAppearanceType.Normal);
                }
                Bitmap annotBitmap = BuildBmp(renderWidth, renderHeight, annotData);
                
                if (page.Rotation != 0)
                {
                    switch(page.Rotation)
                    {
                        case 1:
                            annotBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 2:
                            annotBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 3:
                            annotBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                  
                }
                drawGraph.DrawImage(annotBitmap,new Rectangle(xPos, yPos, annotWidth, annotHeight));
            }

            return writeBitmap;
        }

        /// <summary>
        /// Check whether the page has a digitally signed dynamic appearance
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static bool IsPageHaveSignAP(CPDFPage page)
        {
            if (page == null || page.IsValid() == false)
            {
               return false;
            }

            List<CPDFAnnotation> annotList = page.GetAnnotations();
            if (annotList == null || annotList.Count == 0)
            {
                return false;
            }

            bool hasSignAp = false;

            foreach (CPDFAnnotation rawAnnot in annotList)
            {
                if (rawAnnot.Type != C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                {
                    continue;
                }
               
                CPDFWidget widgetAnnot = rawAnnot as CPDFWidget;
                CPDFSignatureWidget signatureWidget = null;
                if (widgetAnnot.WidgetType == C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS)
                {
                    signatureWidget = widgetAnnot as CPDFSignatureWidget;
                }
              
                if (signatureWidget != null && signatureWidget.IsSignAP())
                {
                    hasSignAp = true;
                    break;
                }
            }

            return hasSignAp;
        }
    }
}
