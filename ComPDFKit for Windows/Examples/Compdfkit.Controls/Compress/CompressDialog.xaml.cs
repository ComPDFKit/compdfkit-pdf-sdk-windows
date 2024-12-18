using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Helper;
using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DragEventArgs = System.Windows.DragEventArgs;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace ComPDFKit.Controls.Compress
{
    public partial class CompressDialog : Window
    {
        private CPDFDocument tempDocument;
        private List<string> pathlist = new List<string>();
        private float quality = 45;
        private int compressindex = -1;
        private IntPtr compressingIntpr = IntPtr.Zero;
        private string compressingfilepath = "";
        private CPDFDocument.GetPageIndexDelegate indexDelegate = null;
        private delegate void RefreshPageIndex(int pageIndex);
        private bool stopClose = false;
        private bool isCanceled = false;

        public ObservableCollection<CompressDataItem> CompressDatas { get; set; }
        
        public CompressDialog()
        {
            InitializeComponent();
            SetLangText();
            rbtnMedium.IsChecked = true;
            CompressDatas = new ObservableCollection<CompressDataItem>();
            CompressListView.ItemsSource = CompressDatas;
        }

        private BitmapSource GetImagePath(string filePath, out IntPtr bitmapHandle)
        {
            try
            {
                Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                Bitmap bitmap = ico.ToBitmap();

                bitmapHandle = bitmap.GetHbitmap();
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                return bitmapSource;
            }
            catch
            {
                bitmapHandle = IntPtr.Zero;
                return null;
            }
        }

        private void SetLangText()
        {
            this.Title = LanguageHelper.CompressManager.GetString("CompressStr");
            btnAddFile.Content = LanguageHelper.CompressManager.GetString("Main_AddFile");

            btnRemove.Content = LanguageHelper.CompressManager.GetString("Main_RemoveAll");
            btnMoveUp.Content = LanguageHelper.CompressManager.GetString("Merge_MoveUp");
            btnMoveDown.Content = LanguageHelper.CompressManager.GetString("Merge_MoveDown");
            btnCompress.Content = LanguageHelper.CompressManager.GetString("CompressStr");
            btnCancel.Content = LanguageHelper.CompressManager.GetString("Main_Cancel");

            groupBox1Text.Text = LanguageHelper.CompressManager.GetString("Compress_OptimizationQuality");
            rbtnLow.Content = LanguageHelper.CompressManager.GetString("Compress_Low");
            rbtnMedium.Content = LanguageHelper.CompressManager.GetString("Compress_Medium");
            rbtnMedium.IsChecked = true;
            rbtnHigh.Content = LanguageHelper.CompressManager.GetString("Compress_High");
            rbtnCustom.Content = LanguageHelper.CompressManager.GetString("Compress_Custom");

            PathHeader.Header = LanguageHelper.CompressManager.GetString("FileInfo_Location");
            ProgressHeader.Header = LanguageHelper.CompressManager.GetString("Convert_Progress");
            FileName.Header = LanguageHelper.CompressManager.GetString("Main_FileName");
            SizeHeader.Header = LanguageHelper.CompressManager.GetString("FileInfo_Size");

            lbTotalFiles.Content = string.Format(LanguageHelper.CompressManager.GetString("Merge_TotalPage"), 0);
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Multiselect = true;
            file.Filter = "PDF Files (*.pdf)|*.pdf";
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var files = file.FileNames;
                for (int i = 0; i < files.Length; i++)
                {
                    AddFiletoList(files[i]);
                }

                UpdateMoveButtonState();
            }
        }

        private void AddFiletoList(string filePath)
        {
            string password = "";
            if (pathlist.Contains(filePath) || Path.GetExtension(filePath).ToLower() != ".pdf")
            {
                return;
            }

            CPDFDocument doc = CPDFDocument.InitWithFilePath(filePath);
            if (doc == null)
            {
                MessageBoxEx.Show(LanguageHelper.CompressManager.GetString("Main_OpenFileFailedWarning"));
                return;
            }

            if (doc.IsLocked)
            {
                PasswordWindow passwordWindow = new PasswordWindow();
                passwordWindow.InitDocument(doc);
                passwordWindow.Owner = Window.GetWindow(this);
                passwordWindow.PasswordDialog.SetShowText(filePath + " is encrypted.");
                passwordWindow.ShowDialog();
                if (doc.IsLocked)
                {
                    doc.Release();
                    return;
                }

                password = passwordWindow.Password;
            }

            pathlist.Add(filePath);
            BitmapSource bitmapSource = GetImagePath(doc.FilePath, out IntPtr bitmapHandle);
            CompressDataItem newdata = new CompressDataItem()
            {
                Name = doc.FileName,
                Size = CommonHelper.GetFileSize(filePath),
                Progress = "0/" + doc.PageCount,
                Path = doc.FilePath,
                PageCount = doc.PageCount,
                ImagePath = bitmapSource

            };
            AddListViewItem(newdata, password);
            doc.Release();
            UpdateTotalCount();
        }

        private void UpdateTotalCount()
        {
            lbTotalFiles.Content = string.Format(LanguageHelper.CompressManager.GetString("Merge_TotalPage"), CompressListView.Items.Count);
            if (CompressListView.Items.Count == 0)
                btnCompress.IsEnabled = false;
            else
                btnCompress.IsEnabled = true;
        }

        private void AddListViewItem(CompressDataItem data, string password)
        {
            if (!string.IsNullOrEmpty(password))
                data.PassWord = password;

            CompressDatas.Add(data);
            UpdateTotalCount();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (CompressListView.SelectedItems.Count > 0)
            {
                List<int> pages = new List<int>();
                foreach (var selectedItem in CompressListView.SelectedItems)
                {
                    var imageItem = (CompressDataItem)selectedItem;
                    int index = CompressListView.Items.IndexOf(imageItem);
                    pages.Add(index);
                }
                pages.Sort();

                for (int i = pages.Count - 1; i >= 0; i--)
                {
                    int index = pages[i];
                    string path = ((CompressDataItem)CompressListView.Items[index]).Path;
                    pathlist.Remove(path);
                    CompressDatas.RemoveAt(index);
                }
            }
            else
            {
                CompressDatas.Clear();
                pathlist.Clear();
            }
            UpdateTotalCount();
        }

        private void btnChoosePage_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnCompress_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)rbtnLow.IsChecked)
                quality = 10;
            else if ((bool)rbtnMedium.IsChecked)
                quality = 40;
            else if ((bool)rbtnHigh.IsChecked)
                quality = 80;
            else
            {
                int q = 0;
                bool r = int.TryParse(txtQuality.Text, out q);
                if (!r || q > 100 || q < 0)
                {
                    MessageBoxEx.Show(LanguageHelper.CompressManager.GetString("Compress_NumberErrorWarning"));
                    txtQuality.Focus();
                    return;
                }
                quality = q;
            }

            CommonOpenFileDialog commonFileDialog = new CommonOpenFileDialog(LanguageHelper.CompressManager.GetString("Main_OpenFolderNoteWarning"));
            commonFileDialog.IsFolderPicker = true;
            if (commonFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (string.IsNullOrEmpty(commonFileDialog.FileName))
                {
                    MessageBoxEx.Show(LanguageHelper.CompressManager.GetString("Main_NoSelectedFilesWarning"),
                        LanguageHelper.CompressManager.GetString("Main_HintWarningTitle"),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
                return;

            stopClose = true;
            groupBox1.IsEnabled = false;
            btnAddFile.IsEnabled = false;
            btnCompress.IsEnabled = false;
            btnRemove.Content = LanguageHelper.CompressManager.GetString("Main_RemoveAll");
            btnRemove.IsEnabled = false;
            btnMoveDown.IsEnabled = false;
            btnMoveUp.IsEnabled = false;
            bool isFailed = false;
            string selectedFile = "";
            for (int i = 0; i < CompressListView.Items.Count; i++)
            {
                var item = (CompressDataItem)CompressListView.Items[i];
                string path = item.Path;
                if (isCanceled)
                {
                    item.Progress = LanguageHelper.CompressManager.GetString("Main_Interrupt");
                    continue;
                }

                CPDFDocument doc = CPDFDocument.InitWithFilePath(path);
                if (doc.IsLocked && item.PassWord != null && !string.IsNullOrEmpty(item.PassWord))
                {
                    doc.UnlockWithPassword(item.PassWord.ToString());
                }

                var filename = doc.FileName + " compressed";
                var filepath = commonFileDialog.FileName + "\\" + filename + ".pdf";
                filepath = CommonHelper.CreateFilePath(filepath);
                selectedFile = compressingfilepath = filepath;

                indexDelegate += GetIndex;
                compressingIntpr = doc.CompressFile_Init(quality, indexDelegate);
                GC.KeepAlive(indexDelegate);
                compressindex = i;
                tempDocument = doc;
                var r = await Task.Run<bool>(() => { return doc.CompressFile_Start(compressingIntpr, filepath); });
                if (!r)
                {
                    item.Progress = LanguageHelper.CompressManager.GetString("Main_FailedState");
                    doc.Release();
                    if (File.Exists(filepath))
                        File.Delete(filepath);
                    isFailed = true;
                    continue;
                }
                compressingfilepath = "";
                doc.Release();
            }

            int itemCount = CompressListView.Items.Count;
            if (!isFailed)
            {
                System.Diagnostics.Process.Start("explorer", "/select,\"" + selectedFile + "\"");
            }

            isCanceled = false;
            compressindex = -1;
            stopClose = false;
            btnAddFile.IsEnabled = true;
            btnCompress.IsEnabled = true;
            btnRemove.Content = LanguageHelper.CompressManager.GetString("Main_Delete");
            btnRemove.IsEnabled = true;
            btnMoveDown.IsEnabled = true;
            btnMoveUp.IsEnabled = true;
            groupBox1.IsEnabled = true;
        }

        private int GetIndex(int pageindex)
        {
            try
            {
                if (Dispatcher.CheckAccess() == false)
                {
                    return Dispatcher.Invoke(new CPDFDocument.GetPageIndexDelegate(delegate (int s)
                    {
                        if (CompressListView.Items.Count >= compressindex)
                        {
                            if (CompressListView.Items[compressindex] is CompressDataItem dataItem)
                            {
                                dataItem.Progress = pageindex + "/" + dataItem.PageCount;
                                if (pageindex == dataItem.PageCount - 1)
                                {
                                    dataItem.Progress = LanguageHelper.CompressManager.GetString("Main_CompletedState");
                                }
                            }
                        }
                        return 0;
                    }), pageindex).ToString().Length;
                }
                else
                {
                    if (CompressListView.Items.Count >= compressindex)
                    {
                        if (CompressListView.Items[compressindex] is CompressDataItem dataItem)
                        {
                            dataItem.Progress = pageindex + "/" + dataItem.PageCount;
                            if (pageindex == dataItem.PageCount - 1)
                            {
                                dataItem.Progress = LanguageHelper.CompressManager.GetString("Main_CompletedState");
                            }
                        }
                    }
                    return 0;
                }
            }
            catch { return -1; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (compressindex != -1 && !compressingIntpr.Equals(IntPtr.Zero))
                {
                    if (MessageBoxEx.Show(LanguageHelper.CompressManager.GetString("CompressInterruptWarning"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes && tempDocument != null)
                    {
                        tempDocument.CompressFile_Cancel(compressingIntpr);
                        isCanceled = true;
                        if (File.Exists(compressingfilepath))
                            File.Delete(compressingfilepath);
                    }
                }
                else
                {
                    this.Close();
                    this.DialogResult = false;
                }
            }
            catch { }
        }

        private void ConverterListView_DragOver(object sender, DragEventArgs e)
        {
            var files = (Array)e.Data.GetData(System.Windows.DataFormats.FileDrop);
            int count = 0;
            string pdf = "pdf";
            foreach (string file in files)
            {
                string text = Path.GetExtension(file);
                if (text.IndexOf(pdf, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    count++;
                }
            }
            if (count < 1)
            {
                e.Effects = System.Windows.DragDropEffects.None;
                Mouse.SetCursor(System.Windows.Input.Cursors.No);
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
                Mouse.SetCursor(System.Windows.Input.Cursors.Arrow);
            }

            return;
        }

        private void ConverterListView_Drop(object sender, DragEventArgs e)
        {
            var files = (Array)e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                var Files = (string[])files;
                for (int i = 0; i < Files.Length; i++)
                {
                    AddFiletoList(Files[i]);
                }
            }
        }

        private void ConverterListView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void ConverterListView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject element = (DependencyObject)e.OriginalSource;
            if (element is Thumb thumb)
            {
                GridViewColumnHeader gridViewColumnHeader = CommonHelper.ViewportHelper.FindVisualParent<GridViewColumnHeader>(thumb);
                if (gridViewColumnHeader != null && gridViewColumnHeader.Content != null)
                {
                    GridViewColumn column = FindGridViewColumn(gridViewColumnHeader.Content.ToString(), CompressListView);
                    if (column != null)
                    {
                        if (column.Header == LanguageHelper.CompressManager.GetString("Main_FileName") && column.ActualWidth > 240)
                        {
                            TextBlock textBlock = FindTextBlockInCellTemplate(column);
                            if (textBlock != null)
                            {
                                textBlock.MaxWidth = textBlock.Width = column.ActualWidth - 20;
                                TxbName_SizeChanged(textBlock, null);
                            }
                        }
                    }
                }
            }
        }

        private TextBlock FindTextBlockInCellTemplate(GridViewColumn column)
        {
            DataTemplate dataTemplate = column.CellTemplate;
            if (dataTemplate != null)
            {
                FrameworkElement frameworkElement = dataTemplate.LoadContent() as FrameworkElement;
                if (frameworkElement != null)
                {
                    return CommonHelper.ViewportHelper.FindVisualChild<TextBlock>(frameworkElement);
                }
            }

            return null;
        }

        private GridViewColumn FindGridViewColumn(string columnHeader, System.Windows.Controls.ListView listView)
        {
            GridView gridView = listView.View as GridView;
            if (gridView != null)
            {
                foreach (var column in gridView.Columns)
                {
                    GridViewColumn gvColumn = column as GridViewColumn;
                    if (gvColumn != null && gvColumn.Header.ToString() == columnHeader)
                    {
                        return gvColumn;
                    }
                }
            }

            return null;
        }

        private void UpdateMoveButtonState()
        {
            if (compressindex != -1 && !compressingIntpr.Equals(IntPtr.Zero))
            {
                return;
            }

            if (CompressListView.Items.Count > 0 && CompressListView.SelectedItems.Count > 0)
            {
                btnRemove.Content = LanguageHelper.CompressManager.GetString("Main_Delete"); ;
                int count = CompressListView.Items.Count;
                if (CompressListView.SelectedItems.Contains(CompressListView.Items[CompressListView.Items.Count - 1]))
                {
                    btnMoveDown.IsEnabled = false;
                }
                else
                    btnMoveDown.IsEnabled = true;
                if (CompressListView.SelectedItems.Contains(CompressListView.Items[0]))
                    btnMoveUp.IsEnabled = false;
                else
                    btnMoveUp.IsEnabled = true;
            }
            else
            {
                btnRemove.Content = LanguageHelper.CompressManager.GetString("Main_RemoveAll");
                btnMoveDown.IsEnabled = false;
                btnMoveUp.IsEnabled = false;
            }
        }

        private void ConverterListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMoveButtonState();
        }
        private void UpdateColumnsWidth(System.Windows.Controls.ListView listView)
        {
            int autoFillColumnIndex = (listView.View as System.Windows.Controls.GridView).Columns.Count - 1;
            if (listView.ActualWidth == Double.NaN)
            {
                listView.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
            }
            double remainingSpace = listView.ActualWidth;
            for (int i = 0; i < (listView.View as System.Windows.Controls.GridView).Columns.Count; i++)
            {
                if (i != autoFillColumnIndex)
                {
                    remainingSpace -= (listView.View as System.Windows.Controls.GridView).Columns[i].ActualWidth;
                    (listView.View as System.Windows.Controls.GridView).Columns[autoFillColumnIndex].Width = remainingSpace >= 0 ? remainingSpace : 0;
                }
            }
        }

        private void ConverterListView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateColumnsWidth(sender as System.Windows.Controls.ListView);
        }

        private void ConverterListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(CompressListView);
            var result = VisualTreeHelper.HitTest(CompressListView, pos);
            if (result != null)
            {
                var listBoxItem = CommonHelper.ViewportHelper.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null)
                {
                    CompressListView.SelectedItems.Clear();
                }
            }
            CompressListView.Focus();
        }

        private void ConverterListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateColumnsWidth(sender as System.Windows.Controls.ListView);
        }

        private void TxbName_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void PART_HeaderGripper_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {

        }

        private void rbtnAllPage_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbtnCurrentPage_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbtnOldPageOnly_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RbtnEvenPageOnly_Checked(object sender, RoutedEventArgs e)
        {

        }

        public class CompressDataItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private string passWord;

            public string PassWord { get; set; }

            public BitmapSource ImagePath { get; set; }

            public string Name { get; set; }
            public string Size { get; set; }

            public string Path { get; set; }
            public int PageCount { get; set; }

            private string progress;

            public string Progress
            {
                get { return progress; }
                set
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = CompressListView.SelectedItems;
                if (items.Count > 0)
                {
                    List<int> indexs = new List<int>();
                    foreach (var selectedItem in CompressListView.SelectedItems)
                    {
                        var imageItem = (CompressDataItem)selectedItem;
                        int index = CompressListView.Items.IndexOf(imageItem);
                        indexs.Add(index);
                    }

                    indexs.Sort();
                    for (int i = 0; i < indexs.Count; i++)
                    {
                        var index = indexs[indexs.Count - 1 - i];
                        var item = CompressDatas[index];
                        CompressDatas.RemoveAt(index);
                        CompressDatas.Insert(index + 1, item);
                    }

                    CompressListView.SelectedItems.Clear();

                    for (int i = 0; i < indexs.Count; i++)
                    {
                        CompressListView.SelectedItems.Add(CompressListView.Items[indexs[i] + 1]);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = CompressListView.SelectedItems; 
                if (items.Count > 0)
                {
                    List<int> indexs = new List<int>();
                    foreach (var selectedItem in CompressListView.SelectedItems)
                    {
                        var imageItem = (CompressDataItem)selectedItem;
                        int index = CompressListView.Items.IndexOf(imageItem);
                        indexs.Add(index);
                    }
                    indexs.Sort();

                    for (int i = 0; i < indexs.Count; i++)
                    {
                        var index = indexs[i];
                        var item = CompressDatas[index];
                        CompressDatas.RemoveAt(index);
                        CompressDatas.Insert(index - 1, item);
                    }

                    CompressListView.SelectedItems.Clear();

                    for (int i = 0; i < indexs.Count; i++)
                    {
                        CompressListView.SelectedItems.Add(CompressListView.Items[indexs[i] - 1]);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    int value = int.Parse(textBox.Text);

                    if (value < 1)
                    {
                        textBox.Text = "1";
                        textBox.Select(textBox.Text.Length, 0);
                    }
                    else if (value > 100)
                    {
                        textBox.Text = "100";
                        textBox.Select(textBox.Text.Length, 0);
                    }
                }
                else
                {
                    textBox.Text = "1";
                    textBox.Select(textBox.Text.Length, 0);
                }
            }
        }
    }

    public class MaxWidthToTextTrimmingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double maxWidth && values[1] is string text)
            {
                if (maxWidth >= MeasureTextWidth(text))
                {
                    return TextTrimming.None;
                }
                else
                {
                    return TextTrimming.CharacterEllipsis;
                }
            }

            return TextTrimming.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private double MeasureTextWidth(string text)
        {
            var formattedText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Arial"),
            12,
            System.Windows.Media.Brushes.Black);

            return formattedText.WidthIncludingTrailingWhitespace;
        }
    }
}
