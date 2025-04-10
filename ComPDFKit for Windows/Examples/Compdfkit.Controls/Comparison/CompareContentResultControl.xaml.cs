using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ComPDFKit.Compare;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Controls.Properties;
using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKitViewer;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ComPDFKit.Controls.Comparison
{
    public class RichTextBoxHelper : DependencyObject
    {

        public static FlowDocument GetDocumentBind(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(DocumentBindProperty);
        }
        public static void SetDocumentBind(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(DocumentBindProperty, value);
        }

        public static readonly DependencyProperty DocumentBindProperty =
            DependencyProperty.RegisterAttached("DocumentBind",
                typeof(TextBindProperty),
                typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = (obj, e) =>
                    {
                        RichTextBox richTextBox = obj as RichTextBox;
                        TextBindProperty bindItem = e.NewValue as TextBindProperty;
                        if (richTextBox != null && bindItem != null)
                        {
                            richTextBox.Document = GetFlowDocument(bindItem);
                        }
                    }
                });

        public static FlowDocument GetFlowDocument(TextBindProperty bindItem)
        {
            FlowDocument Document = new FlowDocument();
            Paragraph textPara = new Paragraph();
            TextBlock addBlock = new TextBlock();
            textPara.Inlines.Add(addBlock);
            Document.Blocks.Add(textPara);

            if (bindItem.ResultType == 2)
            {
                if (bindItem.ObjType == 1)
                {
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Insert_Text"));
                    if (bindItem.OldPageIndex == -1 || bindItem.NewPageIndex == -1)
                    {
                        textRun = new Run(LanguageHelper.CompareManager.GetString("Title_Insert"));
                    }


                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
                if (bindItem.ObjType == 2)
                {
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Insert_Image"));
                    if (bindItem.OldPageIndex == -1 || bindItem.NewPageIndex == -1)
                    {
                        textRun = new Run(LanguageHelper.CompareManager.GetString("Title_Insert"));
                    }
                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
            }

            if (bindItem.ResultType == 1)
            {
                if (bindItem.ObjType == 1)
                {
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Delete_Text"));
                    if (bindItem.OldPageIndex == -1 || bindItem.NewPageIndex == -1)
                    {

                        textRun = new Run(LanguageHelper.CompareManager.GetString("Delete_Page"));
                    }
                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
                if (bindItem.ObjType == 2)
                {
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Delete_Image"));
                    if (bindItem.OldPageIndex == -1 || bindItem.NewPageIndex == -1)
                    {
                        textRun = new Run(LanguageHelper.CompareManager.GetString("Title_Delete"));
                    }
                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
            }

            if (bindItem.ResultType == 3)
            {
                if (bindItem.ObjType == 1)
                {
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Replaced_Text"));
                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
                if (bindItem.ObjType == 2)
                {
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Replaced_Image"));
                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
            }
            if (bindItem.ResultType == 4)
            {
                if (bindItem.ObjType == 1)
                {
                    //更改的文本
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Insert_Text"));
                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
                if (bindItem.ObjType == 2)
                {
                    //更改的文本
                    Run textRun = new Run(LanguageHelper.CompareManager.GetString("Replaced_Image"));
                    textRun.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#42464D"));
                    addBlock.Inlines.Add(textRun);
                }
            }
            return Document;
        }
    }
    public class BoolToVisibleConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }
            else
            {
                if ((bool)value)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class GroupHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null && value is string name)
                {
                    return name;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TextBindProperty
    {
        /// <summary>
        /// 1 text 2 image
        /// </summary>
        public int ObjType { get; set; }
        /// <summary>
        /// 0 NULL 1 Delete 2 Insert 3 Replace 4 Change
        /// </summary>
        public int ResultType { get; set; }
        public CPDFCompareResult RawResult { get; set; }
        public int OldPageIndex { get; set; }
        public int NewPageIndex { get; set; }
    }
    
    public class CompareBindItem : INotifyPropertyChanged
    {
        public string ShowPageIndex { get; set; }
        public TextBindProperty BindProperty { get; set; }

        public Brush BindColorProperty { get; set; }

        public int BindIndexProperty { get; set; }
        public CompareBindItem()
        {
            BindProperty = new TextBindProperty();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    
    public partial class CompareContentResultControl : UserControl
    {
        public List<CPDFCompareResults> CompareResultList { get; private set; } = new List<CPDFCompareResults>();
        private double[] zoomLevel = { 1.00f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        private ObservableCollection<CompareBindItem> BindList = new ObservableCollection<CompareBindItem>();
        private string filePath { get; set; } = string.Empty;
        private bool IsOldSave { get; set; } = true;
        private string OldFileName { get; set; } = string.Empty;
        private bool IsNewSave { get; set; } = false;
        private string NewFileName { get; set; } = string.Empty;
        private bool IsCombineSave { get; set; } = false;
        private string CombineFileName { get; set; } = string.Empty;


        private string oldName = "";
        private string newName = "";

        private bool IsCanScrollChanged = true;
        private CPDFDocument CombineDoc { get; set; }
        private CPDFDocument OldDoc { get; set; }
        private CPDFDocument NewDoc { get; set; }


        private ObservableCollection<ListBoxItem> bindNewPageList = new ObservableCollection<ListBoxItem>();

        private ObservableCollection<ListBoxItem> bindOldPageList = new ObservableCollection<ListBoxItem>();

        private List<int> visibleOldPageIndexes = new List<int>();

        private List<int> visibleNewPageIndexes = new List<int>();

        private DispatcherTimer oldTimer = new DispatcherTimer();

        private DispatcherTimer newTimer = new DispatcherTimer();

        public PDFViewControl pdfViewerCtrl { get; set; }
        
        private delegate void OnThumbnailGeneratedEventHandler(int pageIndex, byte[] thumb, int w, int h);
        private OnThumbnailGeneratedEventHandler OnLeftThumbnailGenerated;
        private OnThumbnailGeneratedEventHandler OnRightThumbnailGenerated;
        
        public event EventHandler ExitCompareEvent;
        public CompareContentResultControl()
        {
            InitializeComponent();
            ICollectionView groupView = CollectionViewSource.GetDefaultView(BindList);
            groupView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CompareBindItem.ShowPageIndex)));
            SynchronizedScrollingCKBox.IsChecked = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (LeftViewer == null || LeftViewer.GetCPDFViewer().GetDocument() == null)
                return;
            ItemsInViewHitTest(LeftViewer, PageOldEditListBox, bindOldPageList, visibleOldPageIndexes);
            oldTimer.Stop();
        }

        private void NewTimer_Tick(object sender, EventArgs e)
        {
            if (RightViewer == null || RightViewer.GetCPDFViewer().GetDocument() == null)
                return;
            ItemsInViewHitTest(RightViewer, PageNewEditListBox, bindNewPageList, visibleNewPageIndexes);
            newTimer.Stop();
        }
        
        public void LoadComparePdf(CPDFDocument combineDoc, CPDFDocument oldDoc, CPDFDocument newDoc)
        {
            CombineDoc = combineDoc;
            OldDoc = oldDoc;
            NewDoc = newDoc;
            OldFileTxb.Text = oldName + ".pdf";
            NewFileTxb.Text = newName + ".pdf";
            OldFileTxbToolTip.Text = oldName + ".pdf";
            NewFileTxbToolTip.Text = newName + ".pdf";
            LeftViewer.InitDocument(oldDoc);
            LeftViewer.SetToolType(ComPDFKit.Tool.ToolType.Viewer);
            OldScalingControl.InitWithPDFViewer(LeftViewer);
            RightViewer.InitDocument(newDoc);
            RightViewer.SetToolType(ComPDFKit.Tool.ToolType.Viewer);
            NewScalingControl.InitWithPDFViewer(RightViewer);
            OnLeftThumbnailGenerated -= _leftviewer_OnThumbnailGenerated;
            OnLeftThumbnailGenerated += _leftviewer_OnThumbnailGenerated;
            OnRightThumbnailGenerated -= _rightviewer_OnThumbnailGenerated;
            OnRightThumbnailGenerated += _rightviewer_OnThumbnailGenerated;
            LeftViewer.PDFViewTool.ScrollChangedHandler -= LeftViewerTool_ScrollChangedHandler;
            LeftViewer.PDFViewTool.ScrollChangedHandler += LeftViewerTool_ScrollChangedHandler;
            RightViewer.PDFViewTool.ScrollChangedHandler -= RightViewerTool_ScrollChangedHandler;
            RightViewer.PDFViewTool.ScrollChangedHandler += RightViewerTool_ScrollChangedHandler;
            PopulateOldThumbnailList();
            PopulateNewThumbnailList();
            oldTimer.Interval = TimeSpan.FromSeconds(0.3);
            oldTimer.Tick += Timer_Tick;
            newTimer.Interval = TimeSpan.FromSeconds(0.3);
            newTimer.Tick += NewTimer_Tick;
            oldTimer.Start();
            newTimer.Start();
        }

        public bool synchronizedScrolling = true;

        private void LeftViewerTool_ScrollChangedHandler(object sender, ScrollChangedEventArgs e)
        {
            if (SynchronizedScrollingCKBox != null && SynchronizedScrollingCKBox.IsChecked.Value&& synchronizedScrolling)
            {
                if (RightViewer != null)
                {
                    RightViewer.ScrollToVerticalOffset(LeftViewer.GetVerticalOffset());
                }
            }
            synchronizedScrolling = true;
        }

        private void RightViewerTool_ScrollChangedHandler(object sender, ScrollChangedEventArgs e)
        {
            if (SynchronizedScrollingCKBox != null && SynchronizedScrollingCKBox.IsChecked.Value&& synchronizedScrolling)
            {
                if (LeftViewer != null)
                {
                    LeftViewer.ScrollToVerticalOffset(RightViewer.GetVerticalOffset());
                }
            }
        }

        private void _rightviewer_OnThumbnailGenerated(int pageIndex, byte[] thumb, int w, int h)
        {
            try
            {
                if (PageNewEditListBox.Items.IsEmpty)
                {
                    return;
                }
                ScrollViewer sv = GetScrollHost(PageNewEditListBox);

                //ListBoxItem item = PageEditListBox.Items[pageIndex] as ListBoxItem;
                ListBoxItem listboxitem = PageNewEditListBox.ItemContainerGenerator.ContainerFromIndex(pageIndex) as ListBoxItem;
                if (CommonHelper.ViewportHelper.IsInViewport(sv, listboxitem))
                {
                    Debug.WriteLine("Got thumbnail for page {0}. It is visible, so adding thumb", pageIndex);
                    PixelFormat fmt = PixelFormats.Bgra32;
                    BitmapSource bps = BitmapSource.Create(w, h, 96.0, 96.0, fmt, null, thumb, (w * fmt.BitsPerPixel + 7) / 8);

                    Image image = GetImageElement(PageNewEditListBox.Items[pageIndex] as ListBoxItem);
                    image.Source = bps;
                }
                else
                {
                    Debug.WriteLine("Got thumbnail for page {0}. It is NOT visible, so ignoring thumb", pageIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void _leftviewer_OnThumbnailGenerated(int pageIndex, byte[] thumb, int w, int h)
        {
            try
            {
                if (PageOldEditListBox.Items.IsEmpty)
                {
                    return;
                }
                ScrollViewer sv = GetScrollHost(PageOldEditListBox);

                //ListBoxItem item = PageEditListBox.Items[pageIndex] as ListBoxItem;
                ListBoxItem listboxitem = PageOldEditListBox.ItemContainerGenerator.ContainerFromIndex(pageIndex) as ListBoxItem;
                if (CommonHelper.ViewportHelper.IsInViewport(sv, listboxitem))
                {
                    Debug.WriteLine("Got thumbnail for page {0}. It is visible, so adding thumb", pageIndex);
                    PixelFormat fmt = PixelFormats.Bgra32;
                    BitmapSource bps = BitmapSource.Create(w, h, 96.0, 96.0, fmt, null, thumb, (w * fmt.BitsPerPixel + 7) / 8);

                    Image image = GetImageElement(PageOldEditListBox.Items[pageIndex] as ListBoxItem);
                    image.Source = bps;
                }
                else
                {
                    Debug.WriteLine("Got thumbnail for page {0}. It is NOT visible, so ignoring thumb", pageIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async void ItemsInViewHitTest(PDFViewControl pdfViewer, ListBox PageEditListBox, ObservableCollection<ListBoxItem> bindPageList, List<int> visiblePageIndexes)
        {
            if (pdfViewer == null || pdfViewer.GetCPDFViewer().GetDocument() == null)
                return;
            try
            {
                ScrollViewer sv = GetScrollHost(PageEditListBox);

                if (sv == null) return;

                if (VisualTreeHelper.GetParent(this) == null)
                    return;

                //List<int> pagesOnScreen = new List<int>();
                //pageThumbnailsToRequest.Clear();

                var range = GetRoughViewportRange(pdfViewer.GetCPDFViewer().GetDocument(), PageEditListBox, (PageEditListBox.Items[0] as ListBoxItem).DesiredSize, new Thickness(6, 10, 6, 10));
                for (int i = 0; i < PageEditListBox.Items.Count; ++i)
                {
                    //if (i>=pdfViewer.Document.PageCount)
                    //{
                    //    break;
                    //}
                    ListBoxItem item = PageEditListBox.Items[i] as ListBoxItem;
                    // ListBoxItem listboxitem = PageEditListBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                    Image img = GetImageElement(item);
                    if (i >= (range.Item1 - 1) && i <= range.Item2 || CommonHelper.ViewportHelper.IsInViewport(sv, item))//更改判断方式  因为BOTA缩略图不准确
                    {
                        if (img.Source == null && !visiblePageIndexes.Contains(i))
                        {
                            visiblePageIndexes.Add(i);
                            await Task.Delay(1);//有刷不出图的情况 增减页面时
                            await GetThumbnail(pdfViewer, i, (int)img.Width, (int)img.Height);
                            Debug.WriteLine("Page {0} is visible, asking for thumb", (i + 1));
                        }
                        else if (img.Source == null)
                        {
                            // await pdfViewer.GetThumbnail(i, (int)img.Width, (int)img.Height);
                            Debug.WriteLine("Page {0} is visible, asking for thumb", (i + 1));
                        }
                    }
                    else
                    {
                        if (visiblePageIndexes.Contains(i))
                        {
                            Image image = GetImageElement(PageEditListBox.Items[i] as ListBoxItem);
                            if (image.Source != null)
                            {
                                image.Source = null;
                                //(image.Parent as Border).BorderBrush = Brushes.Transparent;
                                Debug.WriteLine("Page {0} is out of range, removed thumb", (i + 1));
                            }
                            else
                            {
                                Debug.WriteLine("Page {0} is out of range, but had no thumb", (i + 1));
                            }
                            visiblePageIndexes.Remove(i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        
        public async Task GetThumbnail(PDFViewControl pdfViewer,int pageIndex, int imageWidth, int imageHeight)
        {
            if (imageWidth <= 0 || imageHeight <= 0)
            {
                return;
            }

            CPDFDocument pdfdoc = pdfViewer?.GetCPDFViewer()?.GetDocument();
            if (pdfdoc == null)
            {
                return;
            }

            CPDFPage page = pdfdoc.PageAtIndex(pageIndex);
            if (page == null)
            {
                return;
            }
            byte[] bmpData = new byte[imageWidth * imageHeight * 4];

            await Task.Run(() => page.RenderPageBitmap(0, 0, imageWidth, imageHeight, 0xFFFFFFFF, bmpData, 1, true));
            if (pdfViewer == LeftViewer && OnLeftThumbnailGenerated != null)
            {
                OnLeftThumbnailGenerated(pageIndex, bmpData, imageWidth, imageHeight);
            }
            else if (pdfViewer == RightViewer && OnRightThumbnailGenerated != null)
            {
                OnRightThumbnailGenerated(pageIndex, bmpData, imageWidth, imageHeight);
            }
        }

        public void SetCompareResult(List<CPDFCompareResults> resultList)
        {
            CompareResultList = resultList;
            BindList.Clear();
            int ResultIndex = 0;
            int ResultPage = 1;
            foreach (CPDFCompareResults result in CompareResultList)
            {
                if (result.TextResults != null && result.TextResults.Count > 0)
                {
                    for (int i = result.TextResults.Count - 1; i >= 0; i--)
                    {
                        CPDFCompareResult item = result.TextResults[i];
                        if ((item.OldRect.width() > 0 && item.OldRect.height() > 0) || (item.NewRect.width() > 0 && item.NewRect.height() > 0))
                        {
                            CompareBindItem bindItem = new CompareBindItem();
                            string page = LanguageHelper.CompareManager.GetString("Text_Page");
                            if (item.OldPageIndex == -1)
                            {
                                bindItem.ShowPageIndex = "-" + " VS " + page + " " + (item.NewPageIndex + 1);
                            }
                            if (item.NewPageIndex == -1)
                            {
                                bindItem.ShowPageIndex = page + " " + (item.OldPageIndex + 1) + " VS " + "-";
                            }
                            if (item.NewPageIndex != -1 && item.OldPageIndex != -1)
                            {
                                bindItem.ShowPageIndex = page + " " + (item.OldPageIndex + 1) + " VS " + page + " " + (item.NewPageIndex + 1);
                            }
                            bindItem.BindProperty.ResultType = (int)item.Type;
                            bindItem.BindProperty.ObjType = 1;
                            bindItem.BindProperty.RawResult = item;
                            bindItem.BindProperty.OldPageIndex = item.OldPageIndex;
                            bindItem.BindProperty.NewPageIndex = item.NewPageIndex;
                            bindItem.BindColorProperty = GetBrushFlowDocument(item);
                            BindList.Add(bindItem);
                        }
                    }
                }
                if (result.ImageResults != null && result.ImageResults.Count > 0)
                {
                    for (int i = 0; i < result.ImageResults.Count; i++)
                    {
                        CPDFCompareResult item = result.ImageResults[i];
                        if ((item.OldRect.width() > 0 && item.OldRect.height() > 0) || (item.NewRect.width() > 0 && item.NewRect.height() > 0))
                        {
                            CompareBindItem bindItem = new CompareBindItem();
                            string page = LanguageHelper.CompareManager.GetString("Text_Page");
                            if (item.OldPageIndex == -1)
                            {
                                bindItem.ShowPageIndex = "-" + " VS " + page + " " + (item.NewPageIndex + 1);
                            }
                            if (item.NewPageIndex == -1)
                            {
                                bindItem.ShowPageIndex = page + " " + (item.OldPageIndex + 1) + " VS " + "-";
                            }
                            if (item.NewPageIndex != -1 && item.OldPageIndex != -1)
                            {
                                bindItem.ShowPageIndex = page + " " + (item.OldPageIndex + 1) + " VS " + page + " " + (item.NewPageIndex + 1);
                            }
                            bindItem.BindProperty.ResultType = (int)item.Type;
                            bindItem.BindProperty.ObjType = 2;
                            bindItem.BindProperty.RawResult = item;
                            bindItem.BindProperty.OldPageIndex = item.OldPageIndex;
                            bindItem.BindProperty.NewPageIndex = item.NewPageIndex;
                            bindItem.BindColorProperty = GetBrushFlowDocument(item);
                            BindList.Add(bindItem);
                        }
                    }
                }
            }

            if (BindList.Count > 0)
            {
                List<string> pageList=BindList.AsEnumerable().Select(x=>x.ShowPageIndex).Distinct().ToList();
                if(pageList!=null && pageList.Count > 0)
                {
                    foreach (string page in pageList)
                    {
                        int orderIndex = 1;
                        List<CompareBindItem> orderList=  BindList.AsEnumerable().Where(x => x.ShowPageIndex == page).ToList();
                        foreach (CompareBindItem order in orderList)
                        {
                            order.BindIndexProperty=orderIndex++;
                        }
                    }
                }

                ResultList.ItemsSource = BindList;
                TotalResultText.Text = string.Format("{0}", BindList.Count);
            }
            else
            {
                NoCompareGrid.Visibility = Visibility.Visible;
            }
        }

        public Brush GetBrushFlowDocument(CPDFCompareResult bindItem)
        {
            if ((int)bindItem.Type == 2)
            {
                return InsertColorRect.Fill;
            }

            if ((int)bindItem.Type == 1)
            {
                return DeleteColorRect.Fill;
            }

            if ((int)bindItem.Type == 3)
            {
                return ReplaceColorRect.Fill;
            }
            if ((int)bindItem.Type == 4)
            {
                return ReplaceColorRect.Fill;
            }
            return new SolidColorBrush(Colors.Red);
        }
        
        private double CheckZoomLevel(double zoom, bool IsGrowth)
        {
            double standardZoom = 100;
            if (zoom <= 0.01)
            {
                return 0.01;
            }
            if (zoom >= 10)
            {
                return 10;
            }

            zoom *= 100;
            for (int i = 0; i < zoomLevel.Length - 1; i++)
            {
                if (zoom > zoomLevel[i] && zoom <= zoomLevel[i + 1] && IsGrowth)
                {
                    standardZoom = zoomLevel[i + 1];
                    break;
                }
                if (zoom >= zoomLevel[i] && zoom < zoomLevel[i + 1] && !IsGrowth)
                {
                    standardZoom = zoomLevel[i];
                    break;
                }
            }
            return standardZoom / 100;
        }
        private void SearchResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            synchronizedScrolling = false; 
            if (ResultList.SelectedIndex == -1)
            {
                //synchronizedScrolling = true;
                return;
            }
            CompareBindItem bindItem = ResultList.SelectedItem as CompareBindItem;
            if (bindItem != null && LeftViewer != null && RightViewer != null)
            {
                CPDFCompareResult rawResult = bindItem.BindProperty.RawResult;
                int boundOffset = 3;
                SolidColorBrush fillBrush = Brushes.Transparent;
                switch (rawResult.Type)
                {
                    case CPDFCompareResultType.CPDFCompareResultTypeDelete:
                        fillBrush = Brushes.Red;
                        break;
                    case CPDFCompareResultType.CPDFCompareResultTypeInsert:
                        fillBrush = Brushes.Blue;
                        break;
                    case CPDFCompareResultType.CPDFCompareResultTypeReplace:
                        fillBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0x90, 0x24));
                        break;
                    case CPDFCompareResultType.CPDFCompareResultTypeChange:
                        fillBrush = new SolidColorBrush(Color.FromRgb(0xF3, 0x90, 0x24));
                        break;
                    default:
                        break;
                }

                if (rawResult.OldPageIndex >= 0)
                {
                    CPDFViewer pdfViewer = LeftViewer.GetCPDFViewer();
                    if (pdfViewer != null)
                    {
                        pdfViewer.GoToPage(rawResult.OldPageIndex, new Point(rawResult.OldRect.left - 5, rawResult.OldRect.top - 5));
                    }
                    if (rawResult.Type != CPDFCompareResultType.CPDFCompareResultTypeInsert)
                    {
                        LeftViewer.PDFViewTool.SetPDFCompareView(null, new Pen(Brushes.Red, 2), rawResult.OldPageIndex, new List<Rect>()
                        {
                            new Rect(rawResult.OldRect.left-boundOffset,
                            rawResult.OldRect.top-boundOffset,
                            rawResult.OldRect.width()+boundOffset*2,
                            rawResult.OldRect.height()+boundOffset*2)
                        });
                    }
                    else
                    {
                        LeftViewer.PDFViewTool.ClearPDFCompareView();
                    }
                        
                }

                if (rawResult.NewPageIndex >= 0)
                {
                    CPDFViewer pdfViewer = RightViewer.GetCPDFViewer();
                    if (pdfViewer != null)
                    {
                        pdfViewer.GoToPage(rawResult.NewPageIndex, new Point(rawResult.NewRect.left - 5, rawResult.NewRect.top - 5));
                    }

                    if (rawResult.Type != CPDFCompareResultType.CPDFCompareResultTypeDelete)
                    {
                        RightViewer.PDFViewTool.SetPDFCompareView(null, new Pen(Brushes.Red, 2), rawResult.NewPageIndex, new List<Rect>()
                        {
                            new Rect(rawResult.NewRect.left-boundOffset,
                            rawResult.NewRect.top-boundOffset,
                            rawResult.NewRect.width()+boundOffset*2,
                            rawResult.NewRect.height()+boundOffset*2)
                        });
                    }
                    else
                    {
                        RightViewer.PDFViewTool.ClearPDFCompareView();
                    }

                }
            }
            //synchronizedScrolling = true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SavePopGrid.Visibility = Visibility.Visible;
        }

        private void ConfirmSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string oldSaveName = string.Empty;
            string newSaveName = string.Empty;
            string combineSaveName = string.Empty;
            
            filePath = CmbFilePath.Text;
            IsOldSave = OldCheckBox.IsChecked == true;
            OldFileName = filePath + "\\old.pdf";
            IsNewSave = NewCheckBox.IsChecked == true;
            NewFileName = filePath + "\\new.pdf";
            IsCombineSave = CombineCheckBox.IsChecked == true;
            CombineFileName = filePath + "\\combine.pdf";
            
            if (IsOldSave)
            {
                oldSaveName = CommonHelper.GetFileNameAddSuffix(filePath, oldName + "_compare", ".pdf");
            }
            if (IsNewSave)
            {
                newSaveName = CommonHelper.GetFileNameAddSuffix(filePath, newName + "_compare", ".pdf");
            }
            if (IsCombineSave)
            {
                combineSaveName = CommonHelper.GetFileNameAddSuffix(filePath, "MergedCompareFile", ".pdf");
            }
            if (oldSaveName != string.Empty || newSaveName != string.Empty || combineSaveName != string.Empty)
            {
                string openPath = "";
                if (oldSaveName != string.Empty && OldDoc != null && IsOldSave)
                {
                    OldDoc.WriteToFilePath(oldSaveName);
                    openPath = oldSaveName;
                }
                if (newSaveName != string.Empty && NewDoc != null && IsNewSave)
                {
                    NewDoc.WriteToFilePath(newSaveName);
                    openPath = newSaveName;
                }
                if (combineSaveName != string.Empty && CombineDoc != null && IsCombineSave)
                {
                    CombineDoc.WriteToFilePath(combineSaveName);
                    openPath = combineSaveName;
                }
                if(openPath!=string.Empty)
                {
                    Process.Start(@"explorer.exe", "/select,\"" + openPath + "\"");
                }
            }

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseConfirmGrid.Visibility = Visibility.Visible;
        }
        private void CloseLeave()
        {
            ExitCompareEvent?.Invoke(null,null);
        }

        private void CompareBtn_Click(object sender, RoutedEventArgs e)
        {
            ThumbnailBtn.IsChecked = false;
            CompareBtn.IsChecked = true;
            ThumbnailBtnPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
            CompareBtnPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        }

        private void ThumbnailBtn_Click(object sender, RoutedEventArgs e)
        {
            ThumbnailBtn.IsChecked = true;
            CompareBtn.IsChecked = false;
            ThumbnailBtnPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            CompareBtnPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        }

        private void ListBoxItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void PageEditListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PageOldEditListBox.ScrollIntoView(PageOldEditListBox.SelectedItem);
            PageNewEditListBox.ScrollIntoView(PageNewEditListBox.SelectedItem);
        }

        private void PageEditListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListBox;
            if (list != null)
            {
                if (SynchronizedScrollingCKBox.IsChecked.Value)
                {


                    if (list.Name == "PageOldEditListBox")
                    {

                        PageNewEditListBox.SelectedIndex = PageOldEditListBox.SelectedIndex;
                        e.Handled = false;
                    }
                    if (list.Name == "PageNewEditListBox")
                    {

                        PageOldEditListBox.SelectedIndex = PageNewEditListBox.SelectedIndex;
                        e.Handled = false;
                    }
                    if (PageNewEditListBox.SelectedIndex < LeftViewer.GetCPDFViewer().GetDocument().PageCount)
                    {
                        LeftViewer.GetCPDFViewer().GoToPage(PageNewEditListBox.SelectedIndex);
                    }
                    else
                    {
                        LeftViewer.GetCPDFViewer().GoToPage(LeftViewer.GetCPDFViewer().GetDocument().PageCount - 1);
                    }
                    if (PageOldEditListBox.SelectedIndex < RightViewer.GetCPDFViewer().GetDocument().PageCount)
                    {
                        RightViewer.GetCPDFViewer().GoToPage(PageNewEditListBox.SelectedIndex);
                    }
                    else
                    {
                        RightViewer.GetCPDFViewer().GoToPage(RightViewer.GetCPDFViewer().GetDocument().PageCount - 1);
                    }
                }
                else
                {
                    if (list.Name == "PageNewEditListBox")
                    {
                        if (PageNewEditListBox.SelectedIndex < RightViewer.GetCPDFViewer().GetDocument().PageCount)
                        {
                            if (PageNewEditListBox.SelectedIndex != -1)
                            {
                                RightViewer.GetCPDFViewer().GoToPage(PageNewEditListBox.SelectedIndex);
                                PageOldEditListBox.SelectedIndex = -1;
                            }
                        }
                        else
                        {
                            PageNewEditListBox.SelectedIndex = -1;
                            LeftViewer.GetCPDFViewer().GoToPage(LeftViewer.GetCPDFViewer().GetDocument().PageCount - 1);
                        }
                    }
                    if (list.Name == "PageOldEditListBox")
                    {
                        if (PageOldEditListBox.SelectedIndex < LeftViewer.GetCPDFViewer().GetDocument().PageCount)
                        {
                            if (PageOldEditListBox.SelectedIndex != -1)
                            {
                                LeftViewer.GetCPDFViewer().GoToPage(PageOldEditListBox.SelectedIndex);
                                PageNewEditListBox.SelectedIndex = -1;
                            }
                        }
                        else
                        {
                            PageOldEditListBox.SelectedIndex = -1;
                            RightViewer.GetCPDFViewer().GoToPage(RightViewer.GetCPDFViewer().GetDocument().PageCount - 1);
                        }
                    }

                }
            }
        }

        private void PageEditListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ItemsInViewHitTest(LeftViewer, PageOldEditListBox, bindOldPageList, visibleOldPageIndexes);
            ItemsInViewHitTest(RightViewer, PageNewEditListBox, bindNewPageList, visibleNewPageIndexes);
        }

        private ScrollViewer GetScrollHost(ListBox listBox)
        {
            if (VisualTreeHelper.GetChildrenCount(listBox) > 0)
            {
                int s = VisualTreeHelper.GetChildrenCount(listBox);

                Border border = VisualTreeHelper.GetChild(listBox, 0) as Border;
                if (border != null)
                {
                    return VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                }
            }
            return null;
        }

        // return the range of items in the current view
        private Tuple<int, int, int> GetRoughViewportRange(CPDFDocument doc, ListBox view, Size itemSize, Thickness itemMargin)
        {
            var scrollViewer = GetScrollHost(view);
            if (doc == null || scrollViewer == null || scrollViewer.ActualHeight == 0 || scrollViewer.ActualWidth == 0)//视图展开
                return new Tuple<int, int, int>(0, 0, 0);
            try
            {
                var currentHeight = scrollViewer.ActualHeight - view.Padding.Top;
                var currentWidth = scrollViewer.ActualWidth;
                //计算当前窗口大小能显示的行数和列数
                var columnCount = (int)(currentWidth / (itemSize.Width + itemMargin.Left));
                var rowCount = (int)Math.Ceiling(currentHeight / (itemSize.Height + itemMargin.Bottom));

                var preItemCount = (int)((scrollViewer.VerticalOffset / scrollViewer.ExtentHeight) * ((doc.PageCount + columnCount - 1) / columnCount));//滑动百分比*行数 = 大概的垂直位置
                preItemCount = preItemCount * columnCount;
                var preEnd = (int)(((scrollViewer.VerticalOffset + scrollViewer.ActualHeight) / scrollViewer.ExtentHeight) * ((doc.PageCount + columnCount - 1) / columnCount));
                preEnd = preEnd * columnCount + columnCount - 1;

                var middle = (int)Math.Ceiling(preItemCount + preEnd / 2d);

                return new Tuple<int, int, int>(
                    Math.Max(preItemCount, 0),
                    Math.Min(view.Items.Count, preEnd),
                    middle);
            }
            catch (Exception ex)
            {
            }
            return new Tuple<int, int, int>(0, 0, 0);
        }


        private Image GetImageElement(ListBoxItem item)
        {
            Viewbox viewBox = (item.Content as StackPanel).Children[0] as Viewbox;
            Image img = ((viewBox.Child as Border).Child as Grid).Children[0] as Image;
            return img;
        }

        private int[] thumbnailSize = { 100, 150, 200, 300, 500 };

        public void PopulateOldThumbnailList()
        {
            visibleOldPageIndexes.Clear();
            bindOldPageList.Clear();
            // GC.Collect();

            int thumbnailWidth = thumbnailSize[0];
            bool pageRatio = OldDoc.PageCount >= NewDoc.PageCount;
            int pageCount = pageRatio ? OldDoc.PageCount : NewDoc.PageCount;
            Brush borderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#A0A2AE"));
            Brush fillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB00"));
            Brush fontBrush = Brushes.Black;
            Brush transparentBrush = new SolidColorBrush(Colors.Transparent);
            for (int i = 0; i < pageCount; i++)
            {
                CSize pageSize = new CSize();
                if (i < OldDoc.PageCount)
                {
                    pageSize = OldDoc.GetPageSize(i);
                }
                else
                {
                    borderBrush = transparentBrush;
                    fillBrush = transparentBrush;
                    fontBrush = transparentBrush;
                    pageSize = NewDoc.GetPageSize(i);
                }

                if (pageSize.height == 0 || pageSize.width == 0)
                    continue;

                int imageWidth = pageSize.width > pageSize.height ? thumbnailWidth * 2 : (int)(pageSize.width / pageSize.height * thumbnailWidth * 2);
                int imageHeight = pageSize.height > pageSize.width ? thumbnailWidth * 2 : (int)(pageSize.height / pageSize.width * thumbnailWidth * 2);

                Image img = new Image()
                {
                    //Margin = new Thickness(0, 0, 5, 0),
                    Width = imageWidth,
                    Height = imageHeight,
                    Stretch = Stretch.Uniform,
                };

                Grid grid = new Grid();
                grid.Children.Add(img);
                List<Point> points = new List<Point>()
                                                                                                                                                                                                                        {
                                                                                                                                                                                                                        new Point(16.75,1.25),
                                                                                                                                                                                                                        new Point(3.25,1.25 ),
                                                                                                                                                                                                                        new Point(3.25,19.4013878),
                                                                                                                                                                                                                        new Point(10,14.902),
                                                                                                                                                                                                                        new Point(16.75,19.4013878),
                                                                                                                                                                                                                        };
                Polygon bookmark = new Polygon()
                {
                    Points = new PointCollection(points),
                    Fill = fillBrush,
                    Margin = new Thickness(0, 16, 16, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Visibility = Visibility.Collapsed,
                };
                grid.Children.Add(bookmark);

                Border border = new Border()
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = borderBrush,
                    Child = grid,
                };

                Viewbox viewBox = new Viewbox()
                {
                    Margin = new Thickness(0, 8, 0, 0),
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Width = thumbnailWidth,
                    Height = thumbnailWidth,
                    Child = border,
                };

                TextBlock text = new TextBlock()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    Text = (i + 1).ToString(),
                    Foreground = fontBrush,
                    Margin = new Thickness(0, 8, 0, 8),
                };

                StackPanel panel = new StackPanel();
                panel.Children.Add(viewBox);
                panel.Children.Add(text);

                ListBoxItem item = new ListBoxItem();
                item.Content = panel;
                item.Tag = i;
                item.Margin = new Thickness(6, 10, 6, 10);
                bindOldPageList.Add(item);
            }
            PageOldEditListBox.ItemsSource = bindOldPageList;
        }

        public void PopulateNewThumbnailList()
        {
            visibleNewPageIndexes.Clear();
            bindNewPageList.Clear();
            // GC.Collect();
            bool pageRatio = NewDoc.PageCount >= OldDoc.PageCount;
            int pageCount = pageRatio ? NewDoc.PageCount : OldDoc.PageCount;
            int thumbnailWidth = thumbnailSize[0];
            Brush borderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#A0A2AE"));
            Brush fillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB00"));
            Brush fontBrush = Brushes.Black;
            Brush transparentBrush = new SolidColorBrush(Colors.Transparent);
            for (int i = 0; i < pageCount; i++)
            {
                CSize pageSize = new CSize();
                if (i < NewDoc.PageCount)
                {
                    pageSize = NewDoc.GetPageSize(i);
                }
                else
                {
                    borderBrush = transparentBrush;
                    fillBrush = transparentBrush;
                    fontBrush = transparentBrush;
                    pageSize = OldDoc.GetPageSize(i);
                }
                if (pageSize.height == 0 || pageSize.width == 0)
                    continue;

                int imageWidth = pageSize.width > pageSize.height ? thumbnailWidth * 2 : (int)(pageSize.width / pageSize.height * thumbnailWidth * 2);
                int imageHeight = pageSize.height > pageSize.width ? thumbnailWidth * 2 : (int)(pageSize.height / pageSize.width * thumbnailWidth * 2);

                Image img = new Image()
                {
                    //Margin = new Thickness(0, 0, 5, 0),
                    Width = imageWidth,
                    Height = imageHeight,
                    Stretch = Stretch.Uniform,
                };

                Grid grid = new Grid();
                grid.Children.Add(img);
                List<Point> points = new List<Point>()
                {
                    new Point(16.75,1.25),
                    new Point(3.25,1.25 ),
                    new Point(3.25,19.4013878),
                    new Point(10,14.902),
                    new Point(16.75,19.4013878),
                };
                Polygon bookmark = new Polygon()
                {
                    Points = new PointCollection(points),
                    Fill = fillBrush,
                    Margin = new Thickness(0, 16, 16, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Visibility = Visibility.Collapsed,
                };
                grid.Children.Add(bookmark);

                Border border = new Border()
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = borderBrush,
                    Child = grid,
                };

                Viewbox viewBox = new Viewbox()
                {
                    Margin = new Thickness(0, 8, 0, 0),
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Width = thumbnailWidth,
                    Height = thumbnailWidth,
                    Child = border,
                };

                TextBlock text = new TextBlock()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    Text = (i + 1).ToString(),
                    Foreground = fontBrush,
                    Margin = new Thickness(0, 8, 0, 8),
                };

                StackPanel panel = new StackPanel();
                panel.Children.Add(viewBox);
                panel.Children.Add(text);

                ListBoxItem item = new ListBoxItem();
                item.Content = panel;
                item.Tag = i;
                item.Margin = new Thickness(6, 10, 6, 10);
                bindNewPageList.Add(item);
            }
            PageNewEditListBox.ItemsSource = bindNewPageList;
        }

        public void SetCompareColor(Brush dbrush, Brush rbrush, Brush Ibrush)
        {
            DeleteColorRect.Fill = dbrush;
            ReplaceColorRect.Fill = rbrush;
            InsertColorRect.Fill = Ibrush;
        }
        public void SetCompareName(string oldname, string newname)
        {
            oldName = oldname;
            newName = newname;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double currentOffset = EditListBoxSV.VerticalOffset;

            // Manually handle scrolling
            // Scroll up
            if (e.Delta > 0) 
            {
                EditListBoxSV.ScrollToVerticalOffset(currentOffset - 20);
            }
            // Scroll down
            else if (e.Delta < 0) 
            {
                EditListBoxSV.ScrollToVerticalOffset(currentOffset + 20);
            }
            // Mark the event as handled, to prevent it from bubbling
            e.Handled = true; 
        }

        private void LeftToolPanelButton_Click(object sender, RoutedEventArgs e)
        {
            PanelToolTip.Content = LanguageHelper.CompareManager.GetString(LeftToolPanelButton.IsChecked==true ? "Tooltip_CloseList" : "Tooltip_OpenList");
        }
        
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            SavePopGrid.Visibility = Visibility.Collapsed;
        }
        
        private void ConfirmExitBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseConfirmGrid.Visibility = Visibility.Collapsed;
            CloseLeave();
        }
        
        private void BrowseFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog commonFileDialog = new CommonOpenFileDialog(LanguageHelper.CompressManager.GetString("Main_OpenFolderNoteWarning"));
            commonFileDialog.IsFolderPicker = true;
            if (commonFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                CmbFilePath.Text = commonFileDialog.FileName;
            }
        }

        private void CancelCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseConfirmGrid.Visibility = Visibility.Collapsed;
        }

        private void SynchronizedScrollingCKBox_OnChecked(object sender, RoutedEventArgs e)
        {
            OldScalingControl.CPDFScalingUI.ZoomTextChangedEvent += OnOldZoomTextChangedEvent;
            NewScalingControl.CPDFScalingUI.ZoomTextChangedEvent += OnNewZoomTextChangedEvent;
            
            NewScalingControl.SetScale(OldScalingControl.CPDFScalingUI.Scale.ToString());
            if (SynchronizedScrollingCKBox != null && SynchronizedScrollingCKBox.IsChecked.Value&& synchronizedScrolling)
            {
                if (RightViewer != null)
                {
                    RightViewer.ScrollToVerticalOffset(LeftViewer.GetVerticalOffset());
                }
            }
        }

        private void SynchronizedScrollingCKBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            OldScalingControl.CPDFScalingUI.ZoomTextChangedEvent -= OnOldZoomTextChangedEvent;
            NewScalingControl.CPDFScalingUI.ZoomTextChangedEvent -= OnNewZoomTextChangedEvent;
        }
        
        private void OnOldZoomTextChangedEvent(object sender, string s)
        {
            NewScalingControl.SetScale(s);
        }
        
        private void OnNewZoomTextChangedEvent(object sender, string s)
        {
            OldScalingControl.SetScale(s);
        }
    }
}