using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.Printer;
using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for PrinterDialog.xaml
    /// </summary>
    public partial class PrinterDialog : Window, INotifyPropertyChanged
    {
        private bool isInited = false;

        private PrintServer PrintServer = new PrintServer();
        private PrintQueue printQueue;
        private PrinterSettings printerSettings;

        private PrintSettingsInfo printSettingsInfo = new PrintSettingsInfo();
        private SizeModeInfo sizeModeInfo = new SizeModeInfo();
        private PosterModeInfo posterModeInfo = new PosterModeInfo();

        private MultipleModeInfo multipleModeInfo = new MultipleModeInfo();
        private BookletModeInfo bookletModeInfo = new BookletModeInfo();
        private PrintDocument printDoc = new PrintDocument();


        private int _sizeScale = 100;
        public int SizeScale
        {
            get { return _sizeScale; }
            set
            {
                if (UpdateProper(ref _sizeScale, value))
                {
                    sizeModeInfo.Scale = value;
                    ctlPreview.Init(printSettingsInfo);
                }
            }
        }

        private string _pageRangeText;
        public string PageRangeText
        {
            get => _pageRangeText;
            set
            {
                _pageRangeText = value;
                if (!CommonHelper.GetPagesInRange(ref printSettingsInfo.PageRangeList, PageRangeText, Document.PageCount, new char[] { ',' }, new char[] { '-' }))
                {
                    printSettingsInfo.PageRangeList = Enumerable.Range(0, Document.PageCount).ToList();
                }
                ctlPreview.Init(printSettingsInfo, true);
            }
        }

        private bool _isSheetEnabled = false;
        public bool IsSheetEnabled
        {
            get => _isSheetEnabled;
            set => UpdateProper(ref _isSheetEnabled, value);
        }

        private bool _isEvenEnabled = false;
        public bool IsEvenEnabled
        {
            get => _isEvenEnabled;
            set => UpdateProper(ref _isEvenEnabled, value);
        }

        private string _sheetColumn = 2.ToString();
        public string SheetColumn
        {
            get => _sheetColumn;
            set
            {
                if (UpdateProper(ref _sheetColumn, value))
                {
                    if (SheetColumn != string.Empty && SheetRow != string.Empty)
                    {
                        multipleModeInfo.Sheet = new MultipleModeInfo.SheetPair(int.Parse(SheetColumn), int.Parse(SheetRow));
                    }
                }
            }
        }

        private string _sheetRow = 2.ToString();
        public string SheetRow
        {
            get => _sheetRow;
            set
            {
                if (UpdateProper(ref _sheetRow, value))
                {
                    if (SheetColumn != string.Empty && SheetRow != string.Empty)
                    {
                        multipleModeInfo.Sheet = new MultipleModeInfo.SheetPair(int.Parse(SheetColumn), int.Parse(SheetRow));
                    }
                }
            }
        }

        public CPDFDocument Document
        {
            set
            {
                printSettingsInfo.Document = value;
                IsEvenEnabled = value.PageCount > 1;
            }
            private get => printSettingsInfo.Document;
        }

        public PrinterDialog()
        {
            InitializeComponent();
            InitPaperList();
            DataContext = this;
        }

        #region printer settings 

        private void InitPrinterNameList()
        {
            List<string> printerList = Enumerable.Reverse(PrinterSettings.InstalledPrinters.Cast<string>()).ToList();
            cmbPrinterName.ItemsSource = printerList;
            cmbPrinterName.SelectedIndex = printerList.Contains("Microsoft Print to PDF") ? printerList.IndexOf("Microsoft Print to PDF") : 0;
        }
        #endregion 

        private void PrinterDialog_Loaded(object sender, RoutedEventArgs e)
        {

            InitPrinterNameList();
            InitPrinterSettingsMode();
            ctlPreview.Init(printSettingsInfo);
        }

        private void InitPrinterSettingsMode()
        {
            printSettingsInfo.Copies = 1;
            printSettingsInfo.PrinterName = cmbPrinterName.SelectedItem.ToString();
            printSettingsInfo.PrintMode = sizeModeInfo;
            printSettingsInfo.PrintOrientation = PageOrientation.Portrait;
            printSettingsInfo.PaperSize = printerSettings.PaperSizes[cmbPaper.SelectedIndex];

            printSettingsInfo.IsPrintAnnot = true;
            printSettingsInfo.IsPrintForm = true;
            printSettingsInfo.IsReverseOrder = false;
            printSettingsInfo.IsGrayscale = false;

            printSettingsInfo.DuplexPrintMod = DuplexPrintMod.None;
            printSettingsInfo.PageRangeList = Enumerable.Range(0, Document.PageCount).ToList();
            printSettingsInfo.PrintDocument = printDoc;

            printSettingsInfo.NeedRerendering = true;
            printSettingsInfo.IsPaperSizeChanged = false;
            printSettingsInfo.NeedReversePage = false;

            printSettingsInfo.PrintMode = sizeModeInfo;
            isInited = true;
        }

        private void InitPaperList()
        {
            printerSettings = printDoc.PrinterSettings;
            foreach (PaperSize paperSize in printerSettings.PaperSizes)
            {
                cmbPaper.Items.Add(new ComboBoxItem
                {
                    Content = $"{paperSize.PaperName} - {paperSize.Width} x {paperSize.Height}"
                });
            }
            int IndexOfA4 = printerSettings.PaperSizes.Cast<PaperSize>().ToList().FindIndex(x => x.PaperName == "A4");
            cmbPaper.SelectedIndex = IndexOfA4 != -1 ? IndexOfA4 : 0;
            printSettingsInfo.PaperSize = printerSettings.PaperSizes[cmbPaper.SelectedIndex];
        }

        private void cmbPrinterName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            printSettingsInfo.PrinterName = e.AddedItems[0].ToString();
            if (printSettingsInfo.PrinterName != string.Empty)
            {
                try
                {
                    printQueue = PrintHelper.GetPrintQueue(PrintServer, printSettingsInfo.PrinterName);
                    PrintHelper.printQueue = printQueue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }

                PrintTicket printTicket = printQueue.DefaultPrintTicket;
                PrintCapabilities printCapabilities = printQueue.GetPrintCapabilities();
                double printableWidth = printCapabilities.PageImageableArea.ExtentWidth / 0.96;
                double printableHeight = printCapabilities.PageImageableArea.ExtentHeight / 0.96;
                double originWidth = printCapabilities.PageImageableArea.OriginWidth / 0.96;
                double originHeight = printCapabilities.PageImageableArea.OriginHeight / 0.96;
                double marginRight = printSettingsInfo.PaperSize.Width - originWidth - printableWidth;
                double marginBottom = printSettingsInfo.PaperSize.Height - originHeight - printableHeight;
                printSettingsInfo.Margins = new Thickness() { Left = originWidth, Top = originHeight, Right = (marginRight >= 0) ? marginRight : 0, Bottom = (marginBottom >= 0) ? marginBottom : 0 };
                if (isInited)
                {
                    ctlPreview.Init(printSettingsInfo);
                }
            }
        }

        private void cmbOrientation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            string tag = (e.AddedItems[0] as ComboBoxItem).Tag.ToString();
            switch (tag)
            {
                case "Portrait":
                    printSettingsInfo.PrintOrientation = PageOrientation.Portrait;
                    break;
                case "Landscape":
                    printSettingsInfo.PrintOrientation = PageOrientation.Landscape;
                    break;
            }
            ctlPreview.Init(printSettingsInfo);
        }

        private void cmbContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            string tag = (e.AddedItems[0] as ComboBoxItem).Tag.ToString();
            switch (tag)
            {
                case "Document":
                    printSettingsInfo.IsPrintAnnot = false;
                    printSettingsInfo.IsPrintForm = false;
                    break;
                case "Document and Markups":
                    printSettingsInfo.IsPrintAnnot = true;
                    printSettingsInfo.IsPrintForm = true;
                    break;
                case "Document and Stamps":
                    printSettingsInfo.IsPrintAnnot = true;
                    printSettingsInfo.IsPrintForm = false;
                    break;
            }
            ctlPreview.Init(printSettingsInfo, true);
        }

        private void chkReversePage_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            printSettingsInfo.IsReverseOrder = chk.IsChecked.Value;
            ctlPreview.Init(printSettingsInfo, true);
        }

        private void chkPrintBorder_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
        }

        private void chkGrayScale_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            printSettingsInfo.IsGrayscale = chk.IsChecked.Value;
            ctlPreview.Init(printSettingsInfo);
        }

        private void chkDuplex_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk.IsChecked.GetValueOrDefault())
            {
                if (rdoLongEdge.IsChecked.GetValueOrDefault())
                {
                    printSettingsInfo.DuplexPrintMod = DuplexPrintMod.FlipLongEdge;
                }
                else
                {
                    printSettingsInfo.DuplexPrintMod = DuplexPrintMod.FlipShortEdge;
                }
            }
            else
            {
                printSettingsInfo.DuplexPrintMod = DuplexPrintMod.None;
            }
        }


        private void rdoDuplex_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as RadioButton;
            if (chkDuplex.IsChecked.GetValueOrDefault())
            {
                if (chk.IsChecked.GetValueOrDefault())
                {
                    printSettingsInfo.DuplexPrintMod = chk.Tag.ToString() == "LongEdge" ? DuplexPrintMod.FlipLongEdge : DuplexPrintMod.FlipShortEdge;
                }
            }
            else
            {
                printSettingsInfo.DuplexPrintMod = DuplexPrintMod.None;
            }

        }

        private void cmbPaper_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            sbyte index = (sbyte)cmbPaper.SelectedIndex;
            printSettingsInfo.PaperSize = printerSettings.PaperSizes[index];
            ctlPreview.Init(printSettingsInfo);
        }

        private void rdoPageRange_Click(object sender, RoutedEventArgs e)
        {
            var rdo = sender as RadioButton;
            string rdoTag = rdo.Tag.ToString();
            switch (rdoTag)
            {
                case "All":
                    printSettingsInfo.PageRangeList = Enumerable.Range(0, Document.PageCount).ToList();
                    break;
                case "Odd":
                    printSettingsInfo.PageRangeList = Enumerable.Range(0, Document.PageCount).Where(x => x % 2 == 0).ToList();
                    break;
                case "Even":
                    printSettingsInfo.PageRangeList = Enumerable.Range(0, Document.PageCount).Where(x => x % 2 != 0).ToList();
                    break;
                case "Custom":
                    if (!CommonHelper.GetPagesInRange(ref printSettingsInfo.PageRangeList, PageRangeText, Document.PageCount, new char[] { ',' }, new char[] { '-' }))
                    {
                        printSettingsInfo.PageRangeList = Enumerable.Range(0, Document.PageCount).ToList();
                    }
                    break;
            }
            ctlPreview.Init(printSettingsInfo, true);
        }

        private void SizeMode_Click(object sender, RoutedEventArgs e)
        {
            var rdo = sender as RadioButton;
            string rdoTag = rdo.Tag.ToString();
            switch (rdoTag)
            {
                case "AutoAdapt":
                    sizeModeInfo.SizeType = SizeType.Adaptive;
                    break;
                case "ActualSize":
                    sizeModeInfo.SizeType = SizeType.Actural;
                    break;
                case "CustomScale":
                    sizeModeInfo.SizeType = SizeType.Customized;
                    sizeModeInfo.Scale = SizeScale;
                    break;
            }
            ctlPreview.Init(printSettingsInfo);
        }

        private void cmbPageOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            string tag = (e.AddedItems[0] as ComboBoxItem).Tag.ToString();
            switch (tag)
            {
                case "Horizontal":
                    multipleModeInfo.PageOrder = PageOrder.Horizontal;
                    break;
                case "Horizontal Reverse":
                    multipleModeInfo.PageOrder = PageOrder.HorizontalReverse;
                    break;
                case "TopToBottom":
                    multipleModeInfo.PageOrder = PageOrder.Vertical;
                    break;
                case "BottomToTop":
                    multipleModeInfo.PageOrder = PageOrder.VerticalReverse;
                    break;
            }
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

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            PrintHelper.InitPrint();
            PrintHelper.PrintDocument(printSettingsInfo);
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cmbOrientation_Loaded(object sender, RoutedEventArgs e)
        {
            bool isHeightWidthRatioGreaterThanOne = (printSettingsInfo.ActualHeight / printSettingsInfo.ActualWidth > 1) &&
                                                    (Document.GetPageSize(0).height / Document.GetPageSize(0).width > 1);

            bool isHeightWidthRatioLessThanOne = (printSettingsInfo.ActualHeight / printSettingsInfo.ActualWidth < 1) &&
                                                 (Document.GetPageSize(0).height / Document.GetPageSize(0).width < 1);

            cmbOrientation.SelectedIndex = (isHeightWidthRatioGreaterThanOne || isHeightWidthRatioLessThanOne) ? 0 : 1;
        }

        private void cmbContent_Loaded(object sender, RoutedEventArgs e)
        {
            cmbContent.SelectedIndex = 1;
        }

        private void chkBorderless_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            printSettingsInfo.IsBorderless = chk.IsChecked.Value;
            ctlPreview.Init(printSettingsInfo);
        }

        private void rdoFitPrintable_Checked(object sender, RoutedEventArgs e)
        {
            printSettingsInfo.IsBorderless = false;
            if (isInited)
            {
                ctlPreview.Init(printSettingsInfo);
            }
        }

        private void rdoFitPage_Checked(object sender, RoutedEventArgs e)
        {
            printSettingsInfo.IsBorderless = true;
            if (isInited)
            {
                ctlPreview.Init(printSettingsInfo);
            }
        }
    }
}
