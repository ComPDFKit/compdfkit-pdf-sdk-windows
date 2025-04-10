using System.Windows;
using ComPDFKit.Compare;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Common.BaseControl;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Controls.Properties;
using CheckBox = System.Windows.Forms.CheckBox;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ComboBox = System.Windows.Forms.ComboBox;
using Image = System.Windows.Controls.Image;

namespace ComPDFKit.Controls.Comparison
{
    /// <summary>
    /// ConvertPage.xaml 的交互逻辑
    /// </summary>
    /// 
    public enum CompareType
    {
        ContentCompare = 1,
        OverwriteCompare = 2
    }
    public partial class ComparisonSettingDialog : Window
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        /// <summary>
        /// 0 Unknow  1 Content Compare 2 Overwrite Compare
        /// </summary>
        private CompareType CompareType { get; set; } = CompareType.ContentCompare;
        public CPDFDocument OldDocument { get; private set; }
        public CPDFDocument NewDocument { get; private set; }
        
        // Documents after page range selection
        public CPDFDocument OldCompareDocument { get; private set; }
        
        public CPDFDocument NewCompareDocument { get; private set; }

        private static List<string> NewFilePathList { get; set; } = new List<string>();

        private static List<string> OldFilePathList { get; set; } = new List<string>();

        public static Dictionary<string, string> FileAndPassword = new Dictionary<string, string>();

        //private List<int> OldRange { get; set; } = new List<int>();
        //private List<int> NewRange { get; set; } = new List<int>();
        private CPDFCompareType ObjectCompareType { get; set; } = CPDFCompareType.CPDFCompareTypeAll;
        private static Color ReplaceColor { get; set; } = Color.FromRgb(255, 214, 102);
        private static Color InsertColor { get; set; } = Color.FromRgb(51, 135, 255);
        private static Color DeleteColor { get; set; } = Color.FromRgb(255, 51, 51);
        private static bool IsCompareImage { get; set; } = true;
        private static bool IsCompareText { get; set; } = true;
        private double OldOpacity { get; set; } = 1;
        private double NewOpacity { get; set; } = 1;
        private Color OldMarkColor { get; set; } = Color.FromRgb(255, 51, 51);
        private Color NewMarkColor { get; set; } = Color.FromRgb(51, 135, 255);
        private CPDFBlendMode MixMode { get; set; } = CPDFBlendMode.CPDFBlendModeDarken;

        private PDFViewControl viewCtrl = null;

        private List<CPDFBlendMode> MixModeList { get; set; } = new List<CPDFBlendMode>()
        {
            CPDFBlendMode.CPDFBlendModeNormal,
            CPDFBlendMode.CPDFBlendModeMultiply,
            CPDFBlendMode.CPDFBlendModeScreen,
            CPDFBlendMode.CPDFBlendModeOverlay,
            CPDFBlendMode.CPDFBlendModeDarken,
            CPDFBlendMode.CPDFBlendModeLighten,
            CPDFBlendMode.CPDFBlendModeColorDodge,
            CPDFBlendMode.CPDFBlendModeColorBurn,
            CPDFBlendMode.CPDFBlendModeHardLight,
            CPDFBlendMode.CPDFBlendModeSoftLight,
            CPDFBlendMode.CPDFBlendModeDifference,
            CPDFBlendMode.CPDFBlendModeExclusion,
            CPDFBlendMode.CPDFBlendModeHue,
            CPDFBlendMode.CPDFBlendModeSaturation,
            CPDFBlendMode.CPDFBlendModeColor,
            CPDFBlendMode.CPDFBlendModeLuminosity
        };
        private bool IsFillWhite { get; set; } = false;
        public event EventHandler<UIElement> OnCompareStatusChanged;

        public ComparisonSettingDialog(PDFViewControl viewCtrl)
        {
            InitializeComponent();
            this.viewCtrl = viewCtrl;
            SwapImage.IsEnabled = false;
            Title = LanguageHelper.CompareManager.GetString("Title_CompareDoc");
            SetData();
        }
        
        public string ConvertBrushToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static Color ConvertHexToColor(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private int AddNewFileList(string filename, string filepath)
        {
            if (!NewFilePathList.Contains(filepath))
            {

                NewFileComboBox.Items.Insert(0, filename);
                NewFilePathList.Insert(0, filepath);
                NewFileComboBox.IsEnabled = true;
                if (NewFilePathList.Count > 5 && NewFileComboBox.Items.Count > 5)
                {
                    NewFilePathList.RemoveAt(NewFilePathList.Count - 1);
                    NewFileComboBox.Items.RemoveAt(NewFileComboBox.Items.Count - 1);
                }
                return 0;
            }
            else
            {
                return NewFilePathList.IndexOf(filepath);
            }

        }

        private int AddOldFileList(string filename, string filepath)
        {
            if (!OldFilePathList.Contains(filepath))
            {
                OldFileComboBox.Items.Insert(0, filename);
                OldFilePathList.Insert(0, filepath);
                if (OldFilePathList.Count > 5 && OldFileComboBox.Items.Count > 5)
                {
                    OldFilePathList.RemoveAt(OldFilePathList.Count - 1);
                    OldFileComboBox.Items.RemoveAt(OldFileComboBox.Items.Count - 1);
                }
                return 0;
            }
            else
            {
                return OldFilePathList.IndexOf(filepath);
            }
        }

        private void CompareType_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {

        }

        public void SetCompareType(CompareType compareType)
        {
            if (compareType == CompareType.ContentCompare)
            {
                CompareTypeTab.SelectedIndex = 0;
            }
            else
            {
                CompareTypeTab.SelectedIndex = 1;
            }
        }

        private void CompareTypeTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompareTypeTab != null)
            {
                if (CompareTypeTab.SelectedIndex == 0)
                {
                    CompareType = CompareType.ContentCompare;
                    ContentSettingBox.Visibility = Visibility.Visible;
                    OverlaySettingBox.Visibility = Visibility.Collapsed;
                }
                if (CompareTypeTab.SelectedIndex == 1)
                {
                    CompareType = CompareType.OverwriteCompare;
                    ContentSettingBox.Visibility = Visibility.Collapsed;
                    OverlaySettingBox.Visibility = Visibility.Visible;
                }

            }
        }
        private void Close_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            //MainWindow parentWnd = Window.GetWindow(this) as MainWindow;
            //if (parentWnd == null)
            //{
            //    return;
            //}
            //parentWnd.ContentGrid.Children.Clear();
            //if (parentWnd.PrevElement != null)
            //{
            //    parentWnd.ContentGrid.Children.Add(parentWnd.PrevElement);
            //}
        }
        public void UpdateDocCover(bool isOld)
        {
            if (isOld)
            {
                InitOldDocument();
                int select = AddOldFileList(OldDocument.FileName, OldDocument.FilePath);
                OldFileComboBox.SelectedIndex = select;
            }
            else
            {
                InitNewDocument();
                int select = AddNewFileList(NewDocument.FileName, NewDocument.FilePath);
                NewFileComboBox.SelectedIndex = select;

            }
        }
        public void SetDocument(CPDFDocument doc, bool isOld)
        {
            if (isOld)
            {
                OldDocument = doc;
                if (OldRange != null && OldRange.Count > 0)
                {
                    List<int> rangeList = new List<int>();
                    for (int i = OldRange.Min(); i <= OldRange.Max(); i++)
                    {
                        if (i >= doc.PageCount)
                        {
                            break;
                        }
                        rangeList.Add(i);
                    }
                    OldRange = rangeList;
                }
            }
            else
            {
                NewDocument = doc;
                if (NewRange != null && NewRange.Count > 0)
                {
                    List<int> rangeList = new List<int>();
                    for (int i = NewRange.Min(); i <= NewRange.Max(); i++)
                    {
                        if (i >= doc.PageCount)
                        {
                            break;
                        }
                        rangeList.Add(i);
                    }
                    NewRange = rangeList;
                }
            }
        }
        private void UpdateCover(CPDFDocument doc, Image imageControl, int pageIndex)
        {
            imageControl.Source = null;
            if (doc != null && doc.PageCount > 0 && doc.PageCount > pageIndex)
            {
                CPDFPage pdfPage = doc.PageAtIndex(pageIndex, true);
                int width = (int)pdfPage.PageSize.width;
                int height = (int)pdfPage.PageSize.height;
                Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                pdfPage.RenderPageBitmap(0, 0, width, height, uint.MaxValue, bitmapData.Scan0, 1 , true);
                bitmap.UnlockBits(bitmapData);
                IntPtr bitmapHandle = bitmap.GetHbitmap();
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                imageControl.Source = bitmapSource;
                DeleteObject(bitmapHandle);
            }
        }
        private string GetChoosePdf()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "PDF Files(*.pdf;)|*.pdf;";
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return string.Empty;
        }

        private void CheckDocPassword(CPDFDocument checkDoc, bool isOld = true)
        {

            if (checkDoc == null)
            {
                return;
            }
            if (checkDoc.IsLocked)
            {
                if (FileAndPassword.ContainsKey(checkDoc.FilePath))
                {
                    checkDoc.UnlockWithPassword(FileAndPassword[checkDoc.FilePath]);
                }
                if (checkDoc.IsLocked)
                {
                    PasswordDialog dialog = new PasswordDialog();
                    dialog.Confirmed += (sender, s) =>
                    {
                        if (checkDoc.UnlockWithPassword(s))
                        {
                            FileAndPassword.Add(checkDoc.FilePath, s);
                            if (isOld)
                            {
                                OldFilePathList.Add(checkDoc.FilePath);
                                OldFileComboBox.Items.Add(checkDoc.FileName);
                                OldFileComboBox.SelectedIndex = OldFilePathList.IndexOf(checkDoc.FilePath);
                            }
                            else
                            {
                                NewFilePathList.Add(checkDoc.FilePath);
                                NewFileComboBox.Items.Add(checkDoc.FileName);
                                NewFileComboBox.SelectedIndex = NewFilePathList.IndexOf(checkDoc.FilePath);
                            }
                            PasswordBorder.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            dialog.ClearPassword();
                            dialog.SetShowError(LanguageHelper.CommonManager.GetString("Tip_WrongPassword"), Visibility.Visible);
                        }
                    };
                    dialog.Canceled += (sender, s) =>
                    {
                        PasswordBorder.Visibility = Visibility.Collapsed;
                    };
                    dialog.Closed += (sender, s) =>
                    {
                        PasswordBorder.Visibility = Visibility.Collapsed;
                    };
                    dialog.SetShowText(Path.GetFileName(checkDoc.FileName) + " " + LanguageHelper.CommonManager.GetString("Tip_Encrypted"));
                    PasswordGrid.Children.Clear();
                    PasswordGrid.Children.Add(dialog);
                    PasswordBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void InitOldDocument()
        {
            TxtOldPage.Text = "1";
            CurrentOldPageIndex = 0;
            TxtOldPageCount.Text = "/" + OldDocument.PageCount;
            OldRange.Clear();
            for (int i = 0; i < OldDocument.PageCount; i++)
            {
                if (i < OldDocument.PageCount)
                {
                    OldRange.Add(i);
                }
            }
            CmbOldPageRange.SelectedIndex = "0";
            if (OldFileComboBox.SelectedIndex != OldFilePathList.IndexOf(OldDocument.FilePath))
            {
                OldFileComboBox.SelectedIndex = OldFilePathList.IndexOf(OldDocument.FilePath);
            }
            UpdateCover(OldDocument, OldImageControl, CurrentOldPageIndex);
            CheckOldPageBtnState();
            UpdateOldPageIndex();
        }

        private void InitNewDocument()
        {
            TxtNewPage.Text = "1";
            CurrentNewPageIndex = 0;
            TxtNewPageCount.Text = "/" + NewDocument.PageCount;
            NewRange.Clear();
            for (int i = 0; i < NewDocument.PageCount; i++)
            {
                if (i < NewDocument.PageCount)
                {
                    NewRange.Add(i);
                }
            }
            CmbNewPageRange.SelectedIndex = "0";
            if (NewFileComboBox.SelectedIndex != NewFilePathList.IndexOf(NewDocument.FilePath))
            {
                NewFileComboBox.SelectedIndex = NewFilePathList.IndexOf(NewDocument.FilePath);
            }
            UpdateCover(NewDocument, NewImageControl, CurrentNewPageIndex);
            CheckNewPageBtnState();
            UpdateNewPageIndex();
        }

        private void OldFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OldFilePathList != null && OldFilePathList.Count > OldFileComboBox.SelectedIndex && OldFileComboBox.SelectedIndex > -1)
            {
                string pdfFile = OldFilePathList[OldFileComboBox.SelectedIndex];
                if (File.Exists(pdfFile))
                {
                    if (NewFilePathList != null && NewFileComboBox.SelectedIndex > -1 && !swapeImage)
                    {
                        if (pdfFile == NewFilePathList[NewFileComboBox.SelectedIndex])
                        {
                            OldFileComboBox.SelectedIndex = 0;
                            // SameFileBorder.Visibility = Visibility.Visible;
                            return;
                        }
                    }
                    if (OldDocument != null)
                    {
                        if (OldDocument.FilePath != pdfFile)
                        {
                            CPDFDocument oldDocument = CPDFDocument.InitWithFilePath(pdfFile);
                            CheckDocPassword(oldDocument);
                            if (oldDocument.IsLocked == false)
                            {
                                OldDocument = oldDocument;
                            }
                        }

                    }
                    else
                    {
                        CPDFDocument oldDocument = CPDFDocument.InitWithFilePath(pdfFile);
                        CheckDocPassword(oldDocument);
                        if (oldDocument.IsLocked == false)
                        {
                            NewDocument = oldDocument;
                        }
                    }
                    if (OldDocument != null && OldDocument.IsLocked == false)
                    {
                        if (OldFilePathList.IndexOf(OldDocument.FilePath) != 0)
                        {
                            OldFileComboBox.Items.RemoveAt(OldFilePathList.IndexOf(OldDocument.FilePath));
                            OldFileComboBox.Items.Insert(0, OldDocument.FileName);

                            OldFilePathList.Remove(OldDocument.FilePath);
                            OldFilePathList.Insert(0, OldDocument.FilePath);
                            OldFileComboBox.SelectedIndex = 0;
                        }
                        InitOldDocument();
                    }
                    else
                    {
                        NewFileComboBox.SelectedIndex = -1;
                    }
                }
                else
                {
                    OldFilePathList.RemoveAt(OldFileComboBox.SelectedIndex);
                    NewFileComboBox.Items.RemoveAt(OldFileComboBox.SelectedIndex);
                    if (OldFileComboBox.Items.Count > 0)
                    {
                        OldFileComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        OldFileComboBox.SelectedIndex = -1;
                    }
                    // // MessageBoxEx.Show(App.MainPageLoader.GetString("FileCompare_FileRemoved"));
                    return;
                }
            }
        }

        private void NewFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewFilePathList != null && NewFilePathList.Count > NewFileComboBox.SelectedIndex && NewFileComboBox.SelectedIndex > -1)
            {

                string pdfFile = NewFilePathList[NewFileComboBox.SelectedIndex];
                if (File.Exists(pdfFile))
                {
                    if (OldFilePathList != null && OldFileComboBox.SelectedIndex > -1 && !swapeImage)
                    {
                        if (pdfFile == OldFilePathList[OldFileComboBox.SelectedIndex])
                        {
                            if (ADDFileBorder.Visibility != Visibility.Visible)
                            {
                                NewFileComboBox.SelectedIndex = 0;
                            }
                            else
                            {
                                NewFileComboBox.SelectedIndex = -1;
                            }
                            // SameFileBorder.Visibility = Visibility.Visible;
                            return;
                        }
                    }
                    if (NewDocument != null)
                    {
                        if (NewDocument.FilePath != pdfFile)
                        {
                            CPDFDocument newDocument = CPDFDocument.InitWithFilePath(pdfFile);
                            CheckDocPassword(newDocument, false);
                            if (newDocument.IsLocked == false)
                            {
                                NewDocument = newDocument;
                            }
                        }

                    }
                    else
                    {

                        CPDFDocument newDocument = CPDFDocument.InitWithFilePath(pdfFile);
                        CheckDocPassword(newDocument, false);
                        if (newDocument.IsLocked == false)
                        {
                            NewDocument = newDocument;
                        }
                    }

                    if (NewDocument != null && NewDocument.IsLocked == false)
                    {
                        if (ADDFileBorder.Visibility == Visibility.Visible)
                        {
                            ADDFileBorder.Visibility = Visibility.Collapsed;
                        }
                        if (NewFilePathList.IndexOf(NewDocument.FilePath) != 0)
                        {
                            NewFileComboBox.Items.RemoveAt(NewFilePathList.IndexOf(NewDocument.FilePath));
                            NewFileComboBox.Items.Insert(0, NewDocument.FileName);

                            NewFilePathList.Remove(NewDocument.FilePath);
                            NewFilePathList.Insert(0, NewDocument.FilePath);
                            NewFileComboBox.SelectedIndex = 0;
                        }
                        SwapImage.IsEnabled = true;
                        InitNewDocument();
                    }
                    else
                    {
                        NewFileComboBox.SelectedIndex = -1;
                    }
                }
                else
                {
                    NewFilePathList.RemoveAt(NewFileComboBox.SelectedIndex);
                    NewFileComboBox.Items.RemoveAt(NewFileComboBox.SelectedIndex);
                    if (NewFileComboBox.Items.Count > 0)
                    {
                        NewFileComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        NewFileComboBox.SelectedIndex = -1;
                    }
                    // MessageBoxEx.Show(App.MainPageLoader.GetString("FileCompare_FileRemoved"));
                    return;
                }
            }
        }

        private void BrowseOldBtn_Click(object sender, RoutedEventArgs e)
        {
            string pdfFile = GetChoosePdf();
            if (File.Exists(pdfFile))
            {
                if (NewFilePathList != null && NewFileComboBox.SelectedIndex > -1)
                {
                    if (pdfFile == NewFilePathList[NewFileComboBox.SelectedIndex])
                    {
                        SameFileBorder.Visibility = Visibility.Visible;
                        return;
                    }
                }
                OldDocument = CPDFDocument.InitWithFilePath(pdfFile);
                CheckDocPassword(OldDocument);
                if (OldDocument != null && OldDocument.IsLocked == false)
                {
                    int select = AddOldFileList(OldDocument.FileName, pdfFile);
                    OldFileComboBox.SelectedIndex = select;
                    InitOldDocument();
                }
            }
        }

        public void OpenOldFile(CPDFDocument document)
        {
            OldDocument = document;
            int select = AddOldFileList(OldDocument.FileName, document.FilePath);
            OldFileComboBox.SelectedIndex = select;
            InitOldDocument();
        }

        private void BrowseNewBtn_Click(object sender, RoutedEventArgs e)
        {
            string pdfFile = GetChoosePdf();
            if (File.Exists(pdfFile))
            {
                if (OldFilePathList != null && OldFileComboBox.SelectedIndex > -1)
                {
                    if (pdfFile == OldFilePathList[OldFileComboBox.SelectedIndex])
                    {
                        SameFileBorder.Visibility = Visibility.Visible;
                        return;
                    }
                }
                NewDocument = CPDFDocument.InitWithFilePath(pdfFile);
                CheckDocPassword(NewDocument, false);
                if (NewDocument != null && NewDocument.IsLocked == false)
                {
                    int select = AddNewFileList(NewDocument.FileName, pdfFile);
                    NewFileComboBox.SelectedIndex = select;
                    InitNewDocument();
                }
            }
        }
        public bool swapeImage = false;
        private void SwapeImage_MouseLeftDown(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NewDocument != null && OldDocument != null)
                {
                    swapeImage = true;
                    UpdateCover(NewDocument, OldImageControl, CurrentOldPageIndex);
                    UpdateCover(OldDocument, NewImageControl, CurrentNewPageIndex);
                    CPDFDocument tempDoc = NewDocument;
                    NewDocument = OldDocument;
                    OldDocument = tempDoc;
                    int oldselect = AddOldFileList(OldDocument.FileName, OldDocument.FilePath);
                    OldFileComboBox.SelectedIndex = oldselect;
                    int newselect = AddNewFileList(NewDocument.FileName, NewDocument.FilePath);
                    NewFileComboBox.SelectedIndex = newselect;
                    InitOldDocument();
                    InitNewDocument();
                    swapeImage = false;
                }
                else
                {
                    swapeImage = false;
                    // MessageBoxEx.Show(App.MainPageLoader.GetString("Main_NoSelectedFilesWarning"));
                }
            }
            catch
            {
                swapeImage = false;
            }
        }

        // Get the page range string, 0: All, 1: odd pages, 2: even pages, 3: Custom. String should looks like "1,2,3-5,6"
        private string GetPageRangeString(WritableComboBox cmb, int pageCount)
        {
            string range = "";
            switch (cmb.SelectedIndex)
            {
                case "0":
                    range = "1-" + pageCount;
                    break;
                case "1":
                {
                    for (int i = 0; i < pageCount; i++)
                    {
                        if (i % 2 == 0)
                        {
                            if (range != "")
                            {
                                range += ",";
                            }
                            range += (i + 1);
                        }
                    }

                    break;
                }
                case "2":
                {
                    for (int i = 0; i < pageCount; i++)
                    {
                        if (i % 2 != 0)
                        {
                            if (range != "")
                            {
                                range += ",";
                            }
                            range += (i + 1);
                        }
                    }

                    break;
                }
                case "3":
                    range = cmb.Text;
                    break;
            }

            return range;
        }
        
        private void SavePageRanges(System.Windows.Controls.TextBox textBox, CPDFDocument doc, List<int> rangeList)
        {
            rangeList.Clear();
            if (textBox.Text != string.Empty && doc != null)
            {
                string[] checkTextArray = textBox.Text.Split(',');
                foreach (string checkText in checkTextArray)
                {
                    if (Regex.IsMatch(checkText, "[0-9]+") && Regex.IsMatch(checkText, "[^0-9]") == false)
                    {
                        int range = int.Parse(checkText) - 1;
                        if (range < 0)
                        {
                            continue;
                        }
                        if (rangeList.Contains(range) == false)
                        {
                            rangeList.Add(range);
                        }
                        continue;
                    }
                    if (Regex.IsMatch(checkText, "[1-9]+[0-9]*(-)[0-9]+") && Regex.IsMatch(checkText, "[^0-9\\-]") == false)
                    {
                        if (Regex.Matches(checkText, "[-]+").Count == 1)
                        {
                            string[] pagesArray = checkText.Split('-');
                            int rangeLeft = int.Parse(pagesArray[0]);
                            int rangeRight = int.Parse(pagesArray[1]);
                            for (int i = Math.Min(rangeLeft, rangeRight) - 1; i < Math.Max(rangeLeft, rangeRight); i++)
                            {
                                if (i >= doc.PageCount)
                                {
                                    break;
                                }
                                if (rangeList.Contains(i) == false)
                                {
                                    rangeList.Add(i);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ShowSelectFileBorder(bool isShow)
        {
            SelectFileBorder.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
        }
            
        private void SaveData()
        {
            if (ReplaceColorRect.Fill != null && ((SolidColorBrush)ReplaceColorRect.Fill).Color != ReplaceColor)
            {
                ReplaceColor = ((SolidColorBrush)ReplaceColorRect.Fill).Color;
            }
            if (InsertColorRect.Fill != null && ((SolidColorBrush)InsertColorRect.Fill).Color != InsertColor)
            {
                InsertColor = ((SolidColorBrush)InsertColorRect.Fill).Color;
            }
            if (DeleteColorRect.Fill != null && ((SolidColorBrush)DeleteColorRect.Fill).Color != DeleteColor)
            {
                DeleteColor = ((SolidColorBrush)DeleteColorRect.Fill).Color;
            }

            IsCompareImage = ImageCheckBox.IsChecked == true;
            IsCompareText = TextCheckBox.IsChecked == true;
        }

        private void SetData()
        {
            ReplaceColorRect.Fill = new SolidColorBrush(ReplaceColor);
            InsertColorRect.Fill = new SolidColorBrush(InsertColor);
            DeleteColorRect.Fill = new SolidColorBrush(DeleteColor);
            ImageCheckBox.IsChecked = IsCompareImage;
            TextCheckBox.IsChecked = IsCompareText;

            foreach (var oldFile in OldFilePathList)
            {
                OldFileComboBox.Items.Add(Path.GetFileNameWithoutExtension(oldFile));
            }
            foreach (var newFile in NewFilePathList)
            {
                NewFileComboBox.Items.Add(Path.GetFileNameWithoutExtension(newFile));
            }
        }

        private List<int> GetOrderRangeListWithSize(int size)
        {
            List<int> rangeList = new List<int>();
            for (int i = 0; i < size; i++)
            {
                rangeList.Add(i);
            }
            return rangeList;
        }
        
        private void CompareDocBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(OldFileComboBox.Text == "" || NewFileComboBox.Text == "")
                {
                    ShowSelectFileBorder(true);
                    return;
                }
                if (NewDocument != null && OldDocument != null && NewDocument.IsLocked == false && OldDocument.IsLocked == false && viewCtrl != null)
                {
                    if (Math.Min(OldRange.Count, NewRange.Count) == 0 || (CmbOldPageRange.SelectedIndex == "3" && CmbOldPageRange.Text == "") || (CmbNewPageRange.SelectedIndex == "3" && CmbNewPageRange.Text == ""))
                    {
                        PageRangeBorder.Visibility = Visibility.Visible;
                        return;
                    }

                    this.Close();
                    SaveData();
                    bool cancel = false;
                    // Dispatcher.Invoke(() =>
                    // {
                        CompareProgressControl progressControl = new CompareProgressControl();
                        progressControl.CloseClick += (s, r) =>
                        {
                            cancel = true;
                            OnCompareStatusChanged?.Invoke(this, null);
                        };
                        OnCompareStatusChanged?.Invoke(this, progressControl);
                    // });
                    
                    string oldRange = GetPageRangeString(CmbOldPageRange, OldDocument.PageCount);
                    string newRange =  GetPageRangeString(CmbNewPageRange, NewDocument.PageCount);
                    OldCompareDocument = CPDFDocument.CreateDocument();
                    OldCompareDocument.ImportPages(OldDocument,oldRange);
                    NewCompareDocument = CPDFDocument.CreateDocument();
                    NewCompareDocument.ImportPages(NewDocument, newRange);
                    
                    if (CompareType == CompareType.ContentCompare)
                    {
                        ObjectCompareType = CPDFCompareType.CPDFCompareTypeAll;
                        if (TextCheckBox.IsChecked == true && ImageCheckBox.IsChecked == true)
                        {
                            ObjectCompareType = CPDFCompareType.CPDFCompareTypeAll;
                        }
                        else
                        {
                            if (TextCheckBox.IsChecked == true)
                            {
                                ObjectCompareType = CPDFCompareType.CPDFCompareTypeText;
                            }
                            if (ImageCheckBox.IsChecked == true)
                            {
                                ObjectCompareType = CPDFCompareType.CPDFCompareTypeImage;
                            }
                        }
                        Task.Factory.StartNew(() =>
                        {
                            CPDFCompareContent CPdfContent = new CPDFCompareContent(OldCompareDocument, NewCompareDocument);
                            CPdfContent.SetInsertColor(new byte[] { InsertColor.R, InsertColor.G, InsertColor.B });
                            CPdfContent.SetReplaceColor(new byte[] { ReplaceColor.R, ReplaceColor.G, ReplaceColor.B });
                            CPdfContent.SetDeleteColor(new byte[] { DeleteColor.R, DeleteColor.G, DeleteColor.B });
                            CPdfContent.SetReplaceTransparency(ReplaceColor.A);
                            CPdfContent.SetDeleteTransparency(DeleteColor.A);
                            CPdfContent.SetInsertTransparency(InsertColor.A);

                            List<CPDFCompareResults> resultList = new List<CPDFCompareResults>();
                            List<int> oldTemp = GetOrderRangeListWithSize(OldCompareDocument.PageCount);
                            List<int> newTemp = GetOrderRangeListWithSize(NewCompareDocument.PageCount);
                            int minLength = Math.Min(oldTemp.Count, newTemp.Count);

                            if (minLength == 0)
                            {
                                int maxCount = Math.Max(OldCompareDocument.PageCount, NewCompareDocument.PageCount);

                                for (int i = 0; i < maxCount; i++)
                                {
                                    if (cancel) return;
                                    if (i < OldCompareDocument.PageCount)
                                    {
                                        oldTemp.Add(i);
                                    }
                                    if (i >= OldCompareDocument.PageCount)
                                    {
                                        oldTemp.Add(OldCompareDocument.PageCount);
                                    }

                                    if (i < NewCompareDocument.PageCount)
                                    {
                                        newTemp.Add(i);
                                    }
                                    if (i >= NewCompareDocument.PageCount)
                                    {
                                        newTemp.Add(OldCompareDocument.PageCount);
                                    }
                                }
                                minLength = oldTemp.Count;
                            }
                            else
                            {
                                if (oldTemp.Count != newTemp.Count)
                                {
                                    if (oldTemp.Count < newTemp.Count)
                                    {
                                        for (int i = oldTemp.Count; i < newTemp.Count; i++)
                                        {
                                            if (cancel) return;
                                            oldTemp.Add(OldCompareDocument.PageCount);
                                        }
                                    }
                                    if (oldTemp.Count > newTemp.Count)
                                    {
                                        for (int i = newTemp.Count; i < oldTemp.Count; i++)
                                        {
                                            newTemp.Add(NewCompareDocument.PageCount);
                                        }
                                    }
                                    minLength = oldTemp.Count;
                                }
                            }

                            for (int i = 0; i < minLength; i++)
                            {
                                CPDFCompareResults result = CPdfContent.Compare(oldTemp[i], newTemp[i], ObjectCompareType, true);
                                if (result != null && (result.TextResults.Count > 0 || result.ImageResults.Count > 0))
                                {
                                    if (cancel) return;
                                    resultList.Add(result);
                                    Dispatcher.Invoke(() =>
                                    {
                                        if (minLength != 0)
                                        {
                                            progressControl.SetValue((0.3 + ((double)i / (double)minLength) / (double)2));
                                        }
                                    });
                                }

                            }

                            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");
                            CPdfContent.SaveAsComparisonDocument(tempPath);
                            CPDFDocument combineDoc = CPDFDocument.InitWithFilePath(tempPath);

                            Dispatcher.Invoke(() =>
                            {
                                if (viewCtrl != null)
                                {
                                    FileCompareGrid.Visibility = Visibility.Collapsed;
                                    CompareContentResultControl resultPage = new CompareContentResultControl();
                                    resultPage.SetCompareColor(new SolidColorBrush(DeleteColor), new SolidColorBrush(ReplaceColor), new SolidColorBrush(InsertColor));
                                    resultPage.SetCompareName(OldDocument.FileName, NewDocument.FileName);
                                    resultPage.pdfViewerCtrl = viewCtrl;
                                    resultPage.LoadComparePdf(combineDoc, OldCompareDocument, NewCompareDocument);
                                    resultPage.SetCompareResult(resultList);
                                    resultPage.ExitCompareEvent += (s, r) =>
                                    {
                                        OnCompareStatusChanged?.Invoke(this, null);
                                    };
                                    OnCompareStatusChanged?.Invoke(this, resultPage);
                                }
                            });
                        });
                    }

                    if (CompareType == CompareType.OverwriteCompare)
                    {
                        Dispatcher.Invoke(() => { progressControl.SetValue(0.3); });
                        List<int> oldTemp = GetOrderRangeListWithSize(OldCompareDocument.PageCount);
                        List<int> newTemp = GetOrderRangeListWithSize(NewCompareDocument.PageCount);

                        string oldRnage = "";
                        string newRnage = "";
                        foreach (var odlrange in oldTemp)
                        {
                            if (oldRnage != "")
                            {

                                oldRnage += ",";
                            }
                            oldRnage += (odlrange + 1);
                        }
                        foreach (var newrange in newTemp)
                        {
                            if (newRnage != "")
                            {

                                newRnage += ",";
                            }
                            newRnage += (newrange + 1);
                        }
                        Task.Factory.StartNew(() =>
                        {
                            Dispatcher.Invoke(() => { progressControl.SetValue(0.6); });
                            CPDFCompareOverlay CPdfOverlay = null;
                            if (string.IsNullOrEmpty(oldRnage) || string.IsNullOrEmpty(newRnage))
                            {
                                CPdfOverlay = new CPDFCompareOverlay(OldCompareDocument, NewCompareDocument);
                            }
                            else
                            {
                                CPdfOverlay = new CPDFCompareOverlay(OldCompareDocument, oldRnage, NewCompareDocument, newRnage);
                            }
                            CPdfOverlay?.SetBlendMode(MixMode);
                            CPdfOverlay?.SetNoFill(IsFillWhite);
                            CPdfOverlay?.SetNewDocumentStrokeAlpha((float)NewOpacity);
                            CPdfOverlay?.SetOldDocumentStrokeAlpha((float)OldOpacity);
                            CPdfOverlay?.SetNewDocumentFillAlpha((float)NewOpacity);
                            CPdfOverlay?.SetOldDocumentFillAlpha((float)OldOpacity);
                            CPdfOverlay?.SetNewDocumentStrokeColor(new byte[] { NewMarkColor.R, NewMarkColor.G, NewMarkColor.B });
                            CPdfOverlay?.SetOldDocumentStrokeColor(new byte[] { OldMarkColor.R, OldMarkColor.G, OldMarkColor.B });

                            CPdfOverlay?.Compare();
                            CPDFDocument resultDoc = CPdfOverlay?.ComparisonDocument();

                            Dispatcher.Invoke(() =>
                            {
                                if (viewCtrl != null)
                                {
                                    // if (viewCtrl.ParentPage.loadingConceal.Visibility == Visibility.Collapsed)
                                    // {
                                    //     return;
                                    // }
                                    progressControl.SetValue(0.9);
                                    FileCompareGrid.Visibility = Visibility.Collapsed;
                                    CompareOverwriteResultControl resultPage = new CompareOverwriteResultControl();
                                    resultPage.SetCompareColor(new SolidColorBrush(NewMarkColor), new SolidColorBrush(OldMarkColor));
                                    resultPage.pdfViewerCtrl = viewCtrl;
                                    // resultPage.pdfViewerCtrl.ParentPage.SetCompareModel(false);
                                    // viewCtrl.ParentPage.loadingConcealClose.Visibility = Visibility.Visible;
                                    // viewCtrl.ParentPage.loadingConceal.Visibility = Visibility.Collapsed;
                                    resultPage.LoadComparePdf(resultDoc);
                                    resultPage.LeftDoc = OldCompareDocument;
                                    resultPage.RightDoc = NewCompareDocument;
                                    resultPage.ExitCompareEvent += (s, r) =>
                                    {
                                        OnCompareStatusChanged?.Invoke(this, null);
                                    };
                                    OnCompareStatusChanged?.Invoke(this, resultPage);
                                }
                            });
                        });
                    }
                }
                else
                {
                    // MessageBoxEx.Show(App.MainPageLoader.GetString("FileCompare_PleaseSelect"));
                }
            }
            catch
            {
                return;
            }
        }
        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            List<Key> allowKeyStroke = new List<Key>()
            {
                Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5,
                Key.D6,Key.D7,Key.D8,Key.D9,Key.NumPad0,Key.NumPad1,
                Key.NumPad2,Key.NumPad3,Key.NumPad4,Key.NumPad5,
                Key.NumPad6,Key.NumPad7,Key.NumPad8,Key.NumPad9,
                Key.Delete,Key.Back
            };

            if (allowKeyStroke.Contains(e.Key) == false)
            {
                e.Handled = true;
            }
        }
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }
        private void PageRangeCheck_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null && string.IsNullOrEmpty(textBox.Text) == false)
            {
                bool isMatch = true;
                string[] checkTextArray = textBox.Text.Split(',');
                foreach (string checkText in checkTextArray)
                {
                    if (Regex.IsMatch(checkText, "[0-9]+") && Regex.IsMatch(checkText, "[^0-9]") == false)
                    {
                        continue;
                    }
                    if (Regex.IsMatch(checkText, "[1-9]+[0-9]*(-)[0-9]+") && Regex.IsMatch(checkText, "[^0-9\\-]") == false)
                    {
                        if (Regex.Matches(checkText, "[-]+").Count == 1)
                        {
                            continue;
                        }
                    }
                    isMatch = false;
                }

                if (isMatch == false)
                {
                    textBox.Text = string.Empty;
                }
            }
        }

        int CurrentOldPageIndex = 0;

        private List<int> OldRange = new List<int>();
        private void UpdateOldPageIndex()
        {
            TxtOldPage.Text = (CurrentOldPageIndex + 1).ToString();
            TxtOldPageCount.Text = "/" + OldRange.Count;
        }
        private void CheckOldPageBtnState()
        {
            if (CurrentOldPageIndex + 1 >= OldRange.Count)
            {
                BtnOldNext.IsEnabled = false;
            }
            else
            {
                BtnOldNext.IsEnabled = true;
            }
            if (CurrentOldPageIndex + 1 <= 1)
            {
                BtnOldPre.IsEnabled = false;
            }
            else
            {
                BtnOldPre.IsEnabled = true;
            }
        }

        private void btnOldPre_Click(object sender, RoutedEventArgs e)
        {
            if (OldDocument == null)
            {
                return;
            }
            CurrentOldPageIndex--;
            if (CurrentOldPageIndex < 0 || CurrentOldPageIndex >= OldRange.Count)
            {
                CheckOldPageBtnState();
                TxtOldPage.Focus();
                return;
            }
            UpdateCover(OldDocument, OldImageControl, OldRange[CurrentOldPageIndex]);
            //RefreshPicture(pageOldIndexLists[CurrentPageIndex]);
            CheckOldPageBtnState();
            UpdateOldPageIndex();
        }

        private void txtOldPage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (OldDocument == null)
            {
                return;
            }
            if (e == null)
            {
                return;
            }
            //限制文本框输入内容
            List<Key> NumberKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.Delete, Key.Back, Key.Enter, Key.Right, Key.Left };
            if (!NumberKeys.Contains(e.Key))
            {
                e.Handled = true;
            }
            if (e.Key == Key.Enter)
            {
                int value = 0;
                bool result = int.TryParse(TxtOldPage.Text, out value);
                if (!result || value <= 0 || value > OldRange.Count)
                {
                    TxtOldPage.Text = "1";
                    CurrentOldPageIndex = 0;
                }
                else
                {
                    CurrentOldPageIndex = value - 1;
                    UpdateCover(OldDocument, OldImageControl, OldRange[CurrentOldPageIndex]);
                }
                CheckOldPageBtnState();
            }
        }

        private void txtOldPage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (OldDocument == null)
            {
                return;
            }
            int value = 0;
            bool result = int.TryParse(TxtOldPage.Text, out value);
            if (!result || value < 0 || value > OldRange.Count)
            {
                TxtOldPage.Text = "1";
                CurrentOldPageIndex = 0;
            }
            else
            {
                CurrentOldPageIndex = value - 1;
                if(CurrentOldPageIndex < 0 || CurrentOldPageIndex >= OldRange.Count)
                {
                    CurrentOldPageIndex = 0;
                }
                UpdateCover(OldDocument, OldImageControl, OldRange[CurrentOldPageIndex]);
            }
            CheckOldPageBtnState();
        }

        private void btnOldNext_Click(object sender, RoutedEventArgs e)
        {
            if (OldDocument == null)
            {
                return;
            }
            CurrentOldPageIndex++;
            if (CurrentOldPageIndex < 0 || CurrentOldPageIndex >= OldRange.Count)
            {
                CheckOldPageBtnState();
                TxtOldPage.Focus();
                return;
            }

            UpdateCover(OldDocument, OldImageControl, OldRange[CurrentOldPageIndex]);
            CheckOldPageBtnState();
            UpdateOldPageIndex();
        }

        int CurrentNewPageIndex = 0;

        private List<int> NewRange = new List<int>();

        private void UpdateNewPageIndex()
        {
            TxtNewPage.Text = (CurrentNewPageIndex + 1).ToString();
            TxtNewPageCount.Text = "/" + NewRange.Count;
        }
        private void CheckNewPageBtnState()
        {
            if (CurrentNewPageIndex + 1 >= NewRange.Count)
            {
                BtnNewNext.IsEnabled = false;
            }
            else
            {
                BtnNewNext.IsEnabled = true;
            }
            if (CurrentNewPageIndex + 1 <= 1)
            {
                BtnNewPre.IsEnabled = false;
            }
            else
            {
                BtnNewPre.IsEnabled = true;
            }
        }

        private void btnNewPre_Click(object sender, RoutedEventArgs e)
        {
            if (NewDocument == null)
            {
                return;
            }
            CurrentNewPageIndex--;
            if (CurrentNewPageIndex < 0 || CurrentNewPageIndex >= NewRange.Count)
            {
                CheckNewPageBtnState();
                TxtNewPage.Focus();
                return;
            }
            UpdateCover(NewDocument, NewImageControl, NewRange[CurrentNewPageIndex]);
            CheckNewPageBtnState();
            UpdateNewPageIndex();
        }

        private void txtNewPage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (NewDocument == null)
            {
                return;
            }
            if (e == null)
            {
                return;
            }
            //限制文本框输入内容
            List<Key> NumberKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.Delete, Key.Back, Key.Enter, Key.Right, Key.Left };
            if (!NumberKeys.Contains(e.Key))
            {
                e.Handled = true;
            }
            if (e.Key == Key.Enter)
            {
                int value = 0;
                bool result = int.TryParse(TxtNewPage.Text, out value);
                if (!result || value <= 0 || value > NewRange.Count)
                {
                    TxtNewPage.Text = "1";
                    CurrentNewPageIndex = 0;
                }
                else
                {
                    CurrentNewPageIndex = value - 1;
                    UpdateCover(NewDocument, NewImageControl, NewRange[CurrentNewPageIndex]);
                }
                CheckNewPageBtnState();
            }
        }

        private void txtNewPage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NewDocument == null)
            {
                return;
            }
            int value = 0;
            bool result = int.TryParse(TxtNewPage.Text, out value);
            if (!result || value < 0 || value > NewRange.Count)
            {
                TxtNewPage.Text = "1";
                CurrentNewPageIndex = 0;
            }
            else
            {
                CurrentNewPageIndex = value - 1;
                UpdateCover(NewDocument, NewImageControl, NewRange[CurrentNewPageIndex]);
            }
            CheckNewPageBtnState();
        }

        private void btnNewNext_Click(object sender, RoutedEventArgs e)
        {
            if (NewDocument == null)
            {
                return;
            }
            CurrentNewPageIndex++;
            if (CurrentNewPageIndex < 0 || CurrentNewPageIndex >= NewRange.Count)
            {
                CheckNewPageBtnState();
                TxtNewPage.Focus();
                return;
            }

            UpdateCover(NewDocument, NewImageControl, NewRange[CurrentNewPageIndex]);
            CheckNewPageBtnState();
            UpdateNewPageIndex();
        }

        private void CmbOldPageRange_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (OldDocument != null)
            {
                switch (int.Parse(CmbOldPageRange.SelectedIndex))
                {
                    case 0:
                        OldRange.Clear();
                        for (int i = 0; i < OldDocument.PageCount; i++)
                        {
                            if (i < OldDocument.PageCount)
                            {
                                OldRange.Add(i);
                            }
                        }

                        break;

                    case 1:
                        var numType1 = int.Parse(CmbOldPageRange.SelectedIndex) == 1 ? 1 : 0;

                        int[] page1 = new int[(OldDocument.PageCount + numType1) / 2];
                        for (int i = 0; i < page1.Length; i++)
                        {
                            page1[i] = i * 2 + 1 - numType1;
                        }
                        OldRange = page1.ToList();
                        break;

                    case 2:
                        var numType2 = int.Parse(CmbOldPageRange.SelectedIndex) == 1 ? 1 : 0;

                        int[] page2 = new int[(OldDocument.PageCount + numType2) / 2];
                        for (int i = 0; i < page2.Length; i++)
                        {
                            page2[i] = i * 2 + 1 - numType2;
                        }
                        OldRange = page2.ToList();
                        break;

                    case 3:
                        CmbOldPageRange.MaxPageRange = OldRange.Count;
                        if (CmbOldPageRange.PageIndexList != null && CmbOldPageRange.PageIndexList.Count > 0)
                        {
                            OldRange = CmbOldPageRange.PageIndexList;
                        }
                        break;
                }
                if (CurrentOldPageIndex > OldRange.Count)
                {
                    CurrentOldPageIndex = OldRange.Count - 1;
                }
                if (OldRange.Count != 0)
                {
                    UpdateCover(OldDocument, OldImageControl, OldRange[CurrentOldPageIndex]);
                    CheckOldPageBtnState();
                    UpdateOldPageIndex();
                }
                else
                {
                    CmbOldPageRange.SelectedIndex = "0";
                }
            }

        }

        private void CmbOldPageRange_TextChanged(object sender, RoutedEventArgs e)
        {
            if (OldDocument != null)
            {
                List<int> TargetPages = new List<int>();
                if (CommonHelper.GetPagesInRange(ref TargetPages, CmbOldPageRange.Text, OldDocument.PageCount, new char[] { ',' }, new char[] { '-' }))
                {
                    OldRange = TargetPages;
                    if (CurrentOldPageIndex > OldRange.Count)
                    {
                        CurrentOldPageIndex = OldRange.Count - 1;
                    }
                    UpdateCover(OldDocument, OldImageControl, OldRange[CurrentOldPageIndex]);
                    CheckOldPageBtnState();
                    UpdateOldPageIndex();
                }
            }
        }

        private void CmbNewPageRange_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (NewDocument != null)
            {
                switch (int.Parse(CmbNewPageRange.SelectedIndex))
                {
                    case 0:
                        NewRange.Clear();
                        for (int i = 0; i < NewDocument.PageCount; i++)
                        {
                            if (i < NewDocument.PageCount)
                            {
                                NewRange.Add(i);
                            }
                        }

                        break;

                    case 1:
                        var numType1 = int.Parse(CmbNewPageRange.SelectedIndex) == 1 ? 1 : 0;

                        int[] page1 = new int[(NewDocument.PageCount + numType1) / 2];
                        for (int i = 0; i < page1.Length; i++)
                        {
                            page1[i] = i * 2 + 1 - numType1;
                        }
                        NewRange = page1.ToList();
                        break;

                    case 2:
                        var numType2 = int.Parse(CmbNewPageRange.SelectedIndex) == 1 ? 1 : 0;

                        int[] page2 = new int[(NewDocument.PageCount + numType2) / 2];
                        for (int i = 0; i < page2.Length; i++)
                        {
                            page2[i] = i * 2 + 1 - numType2;
                        }
                        NewRange = page2.ToList();
                        break;

                    case 3:
                        CmbNewPageRange.MaxPageRange = NewRange.Count;
                        if (CmbNewPageRange.PageIndexList != null && CmbNewPageRange.PageIndexList.Count > 0)
                        {
                            NewRange = CmbNewPageRange.PageIndexList;
                        }
                        break;
                }
                if (CurrentNewPageIndex > NewRange.Count)
                {
                    CurrentNewPageIndex = NewRange.Count - 1;
                }
                if (NewRange.Count != 0)
                {
                    UpdateCover(NewDocument, NewImageControl, NewRange[CurrentNewPageIndex]);
                    CheckNewPageBtnState();
                    UpdateNewPageIndex();
                }
                else
                {
                    CmbNewPageRange.SelectedIndex = "0";
                }
            }
        }

        private void CmbNewPageRange_TextChanged(object sender, RoutedEventArgs e)
        {
            if (NewDocument != null)
            {
                List<int> TargetPages = new List<int>();
                if (CommonHelper.GetPagesInRange(ref TargetPages, CmbNewPageRange.Text, NewDocument.PageCount, new char[] { ',' }, new char[] { '-' }))
                {
                    NewRange = TargetPages;
                    if (CurrentNewPageIndex > NewRange.Count)
                    {
                        CurrentNewPageIndex = NewRange.Count - 1;
                    }
                    UpdateCover(NewDocument, NewImageControl, NewRange[CurrentNewPageIndex]);
                    CheckNewPageBtnState();
                    UpdateNewPageIndex();
                }
            }
        }

        public System.Drawing.Color mediaColorToDrawing(System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }
        public System.Windows.Media.Color drawingColorToMedia(System.Drawing.Color drawColor)
        {
            return System.Windows.Media.Color.FromArgb(drawColor.A, drawColor.R, drawColor.G, drawColor.B);
        }

        private void ReplaceColorRect_Click(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ReplaceColor = drawingColorToMedia(colorDialog.Color);
                ReplaceColorRect.Fill = new SolidColorBrush(ReplaceColor);
            }
        }

        private void InsertColorRect_Click(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InsertColor = drawingColorToMedia(colorDialog.Color);
                InsertColorRect.Fill = new SolidColorBrush(InsertColor);
            }
        }

        private void DeleteColorRect_Click(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DeleteColor = drawingColorToMedia(colorDialog.Color);
                DeleteColorRect.Fill = new SolidColorBrush(DeleteColor);
            }
        }

        private void OldMarkColorRect_Click(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OldMarkColor = drawingColorToMedia(colorDialog.Color);
                OldColorRect.Fill = new SolidColorBrush(OldMarkColor);
            }
        }

        private void NewMarkColorRect_Click(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NewMarkColor = drawingColorToMedia(colorDialog.Color);
                NewColorRect.Fill = new SolidColorBrush(NewMarkColor);
            }
        }

        private void CancleBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string pdfFile = GetChoosePdf();
            if (File.Exists(pdfFile))
            {
                if (OldFilePathList != null && OldFileComboBox.SelectedIndex > -1)
                {
                    if (pdfFile == OldFilePathList[OldFileComboBox.SelectedIndex])
                    {
                        SameFileBorder.Visibility = Visibility.Visible;
                        return;
                    }
                }
                NewDocument = CPDFDocument.InitWithFilePath(pdfFile);
                CheckDocPassword(NewDocument, false);
                if (NewDocument != null && NewDocument.IsLocked == false)
                {
                    int select = AddNewFileList(NewDocument.FileName, pdfFile);
                    NewFileComboBox.SelectedIndex = select;
                    InitNewDocument();
                }
            }
        }

        private void ADDFileBorder_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) ? System.Windows.DragDropEffects.Copy : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        private void ADDFileBorder_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // 检查拖拽的文件类型
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                var pdfFiles = files.Where(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)).ToArray();

                if (pdfFiles.Length > 0)
                {
                    if (File.Exists(pdfFiles[0]))
                    {
                        if (OldFilePathList != null && OldFileComboBox.SelectedIndex > -1)
                        {
                            if (pdfFiles[0] == OldFilePathList[OldFileComboBox.SelectedIndex])
                            {
                                SameFileBorder.Visibility = Visibility.Visible;
                                return;
                            }
                        }
                        NewDocument = CPDFDocument.InitWithFilePath(pdfFiles[0]);
                        CheckDocPassword(NewDocument, false);
                        if (NewDocument != null && NewDocument.IsLocked == false)
                        {
                            int select = AddNewFileList(NewDocument.FileName, pdfFiles[0]);
                            NewFileComboBox.SelectedIndex = select;
                            InitNewDocument();
                        }
                    }
                }
                else
                {
                }
            }
        }

        private void SelectFileCancel_OnClick(object sender, RoutedEventArgs e)
        {
            ShowSelectFileBorder(false);
        }

        private void PageRangeCancel_OnClick(object sender, RoutedEventArgs e)
        {
            PageRangeBorder.Visibility = Visibility.Collapsed;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb)
            {
                if(cmb.SelectedIndex < 0 || cmb.SelectedIndex > 16)
                {
                    return;
                }
                MixMode = (CPDFBlendMode)cmb.SelectedIndex;
            }
        }

        private void OldOpacityControl_OnTextInput(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(OldOpacityControl.Text, out float oldOpacity))
            {
                if(oldOpacity > 100)
                {
                    OldOpacityControl.Text = "100";
                    OldOpacity = 100;
                    return;
                }
                if(oldOpacity < 0)
                {
                    OldOpacityControl.Text = "0";
                    OldOpacity = 0;
                    return;
                }
                OldOpacity = oldOpacity / 100.0;
            }
        }

        private void NewOpacityControl_OnTextInput(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(NewOpacityControl.Text, out float newOpacity))
            {
                if(newOpacity > 100)
                {
                    NewOpacityControl.Text = "100";
                    NewOpacity = 100;
                    return;
                }
                if(newOpacity < 0)
                {
                    NewOpacityControl.Text = "0";
                    NewOpacity = 0;
                    return;
                }
                NewOpacity = newOpacity / 100.0;
            }
        }

        private void TextCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            IsCompareText = TextCheckBox.IsChecked == true;
        }

        private void ImageCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            IsCompareImage = ImageCheckBox.IsChecked == true;
        }

        private void SameFileCancel_OnClick(object sender, RoutedEventArgs e)
        {
            SameFileBorder.Visibility = Visibility.Collapsed;
        }
    }
}
