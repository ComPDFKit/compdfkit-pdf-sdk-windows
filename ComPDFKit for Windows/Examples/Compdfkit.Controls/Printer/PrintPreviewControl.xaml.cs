using ComPDFKit.Controls.Printer;
using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for PrintPreviewControl.xaml
    /// </summary>
    public partial class PrintPreviewControl : UserControl, INotifyPropertyChanged
    {
        private double PDFToMediaDpiRatio = 100.0 / 72.0;
        private double mmToDpiRatio = 100 / 25.4;

        private PrintSettingsInfo printSettingsInfo;

        private int originalPaperIndex;
        private string _paperIndex = "1";
        public string PaperIndex
        {
            get
            {
                return _paperIndex;
            }
            set
            {
                if (int.TryParse(value, out int paperIndex))
                {
                    if (paperIndex > 0 && paperIndex <= printSettingsInfo.TargetPaperList.Count)
                    {
                        originalPaperIndex = paperIndex;
                        if (UpdateProper(ref _paperIndex, value))
                        {
                            TargetPaperIndex = paperIndex - 1;
                        }
                    }
                    else
                    {
                        OnPropertyChanged();
                    }
                }
                else if (value == string.Empty)
                {
                    _paperIndex = value;
                }
            }
        }

        private int _printedPageCount;
        public int PrintedPageCount
        {
            get => _printedPageCount;
            set => UpdateProper(ref _printedPageCount, value);
        }

        private string _paperType;
        public string PaperKind
        {
            get => _paperType;
            set => UpdateProper(ref _paperType, value);
        }

        private string _paperWidth;
        public string PaperWidth
        {
            get => _paperWidth;
            set => UpdateProper(ref _paperWidth, value);
        }

        private string _paperHeight;
        public string PaperHeight
        {
            get => _paperHeight;
            set => UpdateProper(ref _paperHeight, value);
        }

        private string _viewBoxWidth = "0";
        public string ViewBoxWidth
        {
            get => _viewBoxWidth;
            set => UpdateProper(ref _viewBoxWidth, value);
        }

        private string _viewBoxHeight = "0";
        public string ViewBoxHeight
        {
            get => _viewBoxHeight;
            set => UpdateProper(ref _viewBoxHeight, value);
        }

        private BitmapSource _priviewBitmapSource;
        public BitmapSource PreviewBitmapSource
        {
            get => _priviewBitmapSource;
            set => UpdateProper(ref _priviewBitmapSource, value);
        }

        private Thickness _margins;
        public Thickness Margins
        {
            get => _margins;
            set => UpdateProper(ref _margins, value);
        }


        private int _targetPaperIndex = 0;

        private Bitmap blankPageBitmap;

        public int TargetPaperIndex
        {
            get => _targetPaperIndex;
            set
            {
                if (UpdateProper(ref _targetPaperIndex, value))
                {
                    JumpToSelectedPage();
                }
            }
        }

        public void SetViewBox(double height, double width)
        {
            if (height / width >= (248.0 / 180.0))
            {
                ViewBoxHeight = "248.0";
                ViewBoxWidth = (width / height * 248.0).ToString();
                Margins = new Thickness()
                {
                    Left = printSettingsInfo.Margins.Left * (248.0 / printSettingsInfo.PaperSize.Height),
                    Right = printSettingsInfo.Margins.Right * (248.0 / printSettingsInfo.PaperSize.Height),
                    Top = printSettingsInfo.Margins.Top * (248.0 / printSettingsInfo.PaperSize.Height),
                    Bottom = printSettingsInfo.Margins.Bottom * (248.0 / printSettingsInfo.PaperSize.Height)
                };
            }
            else
            {
                ViewBoxWidth = "180.0";
                ViewBoxHeight = (height / width * 180.0).ToString();
                Margins = new Thickness()
                {
                    Left = printSettingsInfo.Margins.Left * (180.0 / printSettingsInfo.PaperSize.Width),
                    Right = printSettingsInfo.Margins.Right * (180.0 / printSettingsInfo.PaperSize.Width),
                    Top = printSettingsInfo.Margins.Top * (180.0 / printSettingsInfo.PaperSize.Width),
                    Bottom = printSettingsInfo.Margins.Bottom * (180.0 / printSettingsInfo.PaperSize.Width)
                };
            }
        }

        private void SetPreviewBox()
        {
            PaperKind = printSettingsInfo.PaperSize.Kind.ToString();
            if (printSettingsInfo.PrintOrientation == PageOrientation.Portrait)
            {
                SetPortrait();
            }
            else if (printSettingsInfo.PrintOrientation == PageOrientation.Landscape)
            {
                SetLandscape();
            }
            else
            {
                CPDFPage page = printSettingsInfo.Document.PageAtIndex(printSettingsInfo.TargetPaperList[TargetPaperIndex]);
                if (page.PageSize.height > page.PageSize.width)
                {
                    SetPortrait();
                }
                else
                {
                    SetLandscape();
                }
            }
            if (!(printSettingsInfo.PrintMode is PosterModeInfo))
            {
                SetViewBox(double.Parse(PaperHeight), double.Parse(PaperWidth));
            }

            void SetPortrait()
            {
                PaperWidth = string.Format("{0:F1}", (printSettingsInfo.PaperSize.Width));
                PaperHeight = string.Format("{0:F1}", (printSettingsInfo.PaperSize.Height));
            }

            void SetLandscape()
            {
                PaperWidth = string.Format("{0:F1}", (printSettingsInfo.PaperSize.Height));
                PaperHeight = string.Format("{0:F1}", (printSettingsInfo.PaperSize.Width));
            }
        }

        internal void Init(PrintSettingsInfo printSettingsInfo, bool needPageReset = false)
        {
            this.printSettingsInfo = printSettingsInfo;
            PrintedPageCount = PrintHelper.CaculatePrintedPageCount(printSettingsInfo);
            CalculatePaperCollection(needPageReset);
            SetPreviewBox();
            PaintPageByCurrentPreviewIndex();
        }

        private void CreateBlankBitmap()
        {
            CPDFPage page = printSettingsInfo.Document.PageAtIndex(printSettingsInfo.TargetPaperList[TargetPaperIndex]);
            double height = 0;
            double width = 0;
            width = printSettingsInfo.PrintOrientation == PageOrientation.Portrait
                ? printSettingsInfo.ActualWidth
                : printSettingsInfo.ActualHeight;

            height = printSettingsInfo.PrintOrientation == PageOrientation.Portrait
                ? printSettingsInfo.ActualHeight
                : printSettingsInfo.ActualWidth;

            blankPageBitmap = new Bitmap((int)width, (int)height);

            using (Graphics g = Graphics.FromImage(blankPageBitmap))
            {
                g.Clear(Color.White);
            }
        }

        public void CalculatePaperCollection(bool needPageReset = false)
        {
            printSettingsInfo.TargetPaperList = new List<int>(printSettingsInfo.PageRangeList);
            if (printSettingsInfo.IsReverseOrder)
            {
                printSettingsInfo.TargetPaperList.Reverse();
            }
            if (needPageReset)
            {
                PaperIndex = "1";
            }
        }

        public PrintPreviewControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private readonly object lockObject = new object();
        private void PaintPageByCurrentPreviewIndex()
        {
            lock (lockObject)
            {
                CreateBlankBitmap();
                PreviewPageBySizeMode(printSettingsInfo.TargetPaperList[TargetPaperIndex]);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public BitmapSource ToBitmapSource(System.Drawing.Bitmap bmp)
        {
            IntPtr ptr = bmp.GetHbitmap();//obtain the Hbitmap
            try
            {
                BitmapSource bmpsrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
                (
                    ptr,
                    IntPtr.Zero,
                    new Int32Rect(0, 0, bmp.Width, bmp.Height),
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
                );
                return bmpsrc;
            }
            finally
            {
                DeleteObject(ptr);
            }
        }

        public Bitmap Resize(Bitmap input, int targetWidth, int targetHeight)
        {
            try
            {
                var actualBitmap = new Bitmap(targetWidth, targetHeight);

                var g = Graphics.FromImage(actualBitmap);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(input,
                    new Rectangle(0, 0, targetWidth, targetHeight),
                    new Rectangle(0, 0, input.Width, input.Height),
                    GraphicsUnit.Pixel);
                g.Dispose();
                return actualBitmap;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void PreviewPageBySizeMode(int index)
        {
            if (!(printSettingsInfo.PrintMode is SizeModeInfo sizeModeInfo))
            {
                return;
            }

            Bitmap printBitmap = new Bitmap((int)double.Parse(PaperWidth), (int)double.Parse(PaperHeight));
            CPDFPage page = printSettingsInfo.Document.PageAtIndex(index);
            CSize cSize = page.PageSize;
            System.Drawing.Size pageSize = new System.Drawing.Size((int)cSize.width, (int)cSize.height);
            CRect pageRect = new CRect(0, (int)(pageSize.Height), (int)(pageSize.Width), 0);
            byte[] bmpData = new byte[(int)(pageRect.width() * pageRect.height() * 4)];

            if (page != null)
            {
                Bitmap bitmap = null;
                if (PrintHelper.IsPageHaveSignAP(page))
                {
                    bitmap = PrintHelper.GetPageBitmapWithFormDynamicAP(printSettingsInfo.Document, page, 1, 1, pageRect, 0xFFFFFFFF, bmpData, printSettingsInfo.IsPrintAnnot ? 1 : 0, printSettingsInfo.IsPrintForm);
                }

                if (bitmap == null)
                {
                    page.RenderPageBitmapWithMatrix((float)1, pageRect, 0xFFFFFFFF, bmpData, printSettingsInfo.IsPrintAnnot ? 1 : 0, printSettingsInfo.IsPrintForm);
                    bitmap = PrintHelper.BuildBmp((int)pageRect.width(), (int)pageRect.height(), bmpData);
                }
                Point startPoint = new Point(0, 0);

                if (printSettingsInfo.IsGrayscale)
                {
                    bitmap = PrintHelper.ToGray(bitmap, 0);
                }

                if (sizeModeInfo.SizeType == SizeType.Adaptive)
                {
                    int resizedHeight = 0;
                    int resizedWidth = 0;
                    if (bitmap.Height / bitmap.Width >= (printSettingsInfo.ActualHeight / printSettingsInfo.ActualWidth))
                    {
                        if (printSettingsInfo.PrintOrientation == PageOrientation.Portrait)
                        {
                            resizedHeight = (int)printSettingsInfo.ActualHeight;
                            resizedWidth = (int)(printSettingsInfo.ActualHeight / bitmap.Height * bitmap.Width);
                        }
                        else
                        {
                            resizedWidth = (int)printSettingsInfo.ActualHeight;
                            resizedHeight = (int)(printSettingsInfo.ActualHeight / bitmap.Height * bitmap.Width);
                        }
                    }
                    else
                    {
                        if (printSettingsInfo.PrintOrientation == PageOrientation.Portrait)
                        {
                            resizedWidth = (int)printSettingsInfo.ActualWidth;
                            resizedHeight = (int)printSettingsInfo.ActualHeight;
                        }
                        else
                        {
                            resizedHeight = (int)printSettingsInfo.ActualWidth;
                            resizedWidth = (int)printSettingsInfo.ActualHeight;
                        }
                    }

                    bitmap = Resize(bitmap, resizedWidth, resizedHeight);
                    startPoint.X = (blankPageBitmap.Width - resizedWidth) / 2;
                    startPoint.Y = (blankPageBitmap.Height - resizedHeight) / 2;
                    printBitmap = PrintHelper.CombineBitmap(blankPageBitmap, bitmap, startPoint);
                }

                else if (sizeModeInfo.SizeType == SizeType.Actural)
                {
                    bitmap = PrintHelper.ResizeBitmap(bitmap, 100);
                    startPoint.X = (blankPageBitmap.Width - bitmap.Width) / 2;
                    startPoint.Y = (blankPageBitmap.Height - bitmap.Height) / 2;
                    printBitmap = PrintHelper.CombineBitmap(blankPageBitmap, bitmap, startPoint);

                }

                else
                {
                    float scale = sizeModeInfo.SizeType == SizeType.Customized ? sizeModeInfo.Scale : 1;
                    bitmap = PrintHelper.ResizeBitmap(bitmap, scale);
                    startPoint.X = (blankPageBitmap.Width - bitmap.Width) / 2;
                    startPoint.Y = (blankPageBitmap.Height - bitmap.Height) / 2;
                    printBitmap = PrintHelper.CombineBitmap(blankPageBitmap, bitmap, startPoint);
                }
            }

            PreviewBitmapSource = ToBitmapSource(printBitmap);
        }


        public void JumpToSelectedPage()
        {
            PaintPageByCurrentPreviewIndex();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool UpdateProper<T>(ref T properValue,
                                           T newValue,
                                           [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return false;

            properValue = newValue;
            OnPropertyChanged(properName);
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void btnPreButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PaperIndex, out int _))
            {
                _paperIndex = originalPaperIndex.ToString();
            }
            PaperIndex = (int.Parse(PaperIndex) - 1).ToString();
        }

        private void btnNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PaperIndex, out int _))
            {
                _paperIndex = originalPaperIndex.ToString();
            }
            PaperIndex = (int.Parse(PaperIndex) + 1).ToString();
        }

        private void txbPageIndex_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PaperIndex == string.Empty)
            {
                PaperIndex = (originalPaperIndex).ToString();
            }
        }

        private void txbPageIndex_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus is Button button && (button == btnNextButton || button == btnPreButton))
            {
                e.Handled = true;
            }
        }
    }
}
