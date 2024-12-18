using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfToolkit.Controls;
using static ComPDFKit.Controls.Helper.CommonHelper;
using Winform = System.Windows.Forms;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFPageEditControl : UserControl
    {
        #region  Helper class Helper methods
        internal static class Utils
        {
            public static T FindVisualParent<T>(DependencyObject obj) where T : class
            {
                while (obj != null)
                {
                    if (obj is T)
                        return obj as T;

                    obj = VisualTreeHelper.GetParent(obj);
                }
                return null;
            }

            public static childItem FindVisualChild<childItem>(DependencyObject obj)
                 where childItem : DependencyObject
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is childItem)
                        return (childItem)child;
                    else
                    {
                        childItem childOfChild = FindVisualChild<childItem>(child);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
                return null;
            }
        }
        #endregion

        #region Properties
        private ObservableCollection<ListBoxItem> bindPageList = new ObservableCollection<ListBoxItem>();
        public bool isThumbInitialized = false;
        private const int THUMBNAIL_WIDTH = 150;
        private const int THUMBNAIL_HEIGHT = 150;
        public PDFViewControl viewControl;
        private List<int> pageThumbnailsToRequest = new List<int>();
        private List<int> visiblePageIndexes = new List<int>();
        private string dragingEnterPath;
        private ScrollEventType scrollType = ScrollEventType.EndScroll;
        private Point startPostion = new Point();
        private bool isFirstLoad = true;
        public bool isHided = false;
        private int zoomLevel = 2;
        private bool isZooming = false;
        private int[] thumbnailSize = { 100, 150, 200, 300, 500 };

        private bool startChoose = false;   //Whether to start multiple selections (box selection)
        private int speed = 0;  //Scrolling speed

        //The position in the item when the mouse clicks  
        private double item_x;
        private double item_y;
        private int InsertIndex = -1;

        //Drag the Item
        private ListBoxItem tempItem;

        //Whether the mouse stays on the front half of the item
        //When the first half is displayed, the index obtained is the actual index value
        //When the second half is displayed, the obtained index needs add 1
        //private bool isFrontHalf = false;
        private bool isDraging = false;

        //Whether the file is being dragged in from the outside
        private bool isDragingEnter = false;

        //It is used to assist in the implementation of multi-select and then single-select problems
        private bool iskeyDown = false;

        //When the order or total number of pages changes
        public event RoutedEventHandler PageMoved;

        //When zooming
        public event RoutedEventHandler ChangedZoomFactor;

        private bool isFirstScrollChange = true;
        private bool isFirstSizeChange = true;
        public string SelectedItemsRange
        {
            get { return GetPageParam(); }
        }

        public int SelectedIndex
        {
            get { return PageEditListBox.SelectedIndex; }
        }
        
        private bool CanSave
        {
            get => viewControl.PDFViewTool.IsDocumentModified;
            set => viewControl.PDFViewTool.IsDocumentModified = value;
        }

        public event EventHandler ExitPageEdit;
        private delegate void OnThumbnailGeneratedEventHandler(int pageIndex, byte[] thumb, int w, int h);
        private OnThumbnailGeneratedEventHandler OnThumbnailGenerated;
        #endregion

        public CPDFPageEditControl()
        {
            InitializeComponent();
        }

        public void PageEdit(string PageEditString)
        {
            var list = GetListFromSelectedItems();
            CPDFDocument pdfDoc = GetPDFDocument();
            if (PageEditString == "Insert" && pdfDoc!=null)
            {
                CPDFPageInsertWindow pageInsertWindow = new CPDFPageInsertWindow();
                Window parentWindow = Window.GetWindow(this);
                pageInsertWindow.InitPageInsertWindow(SelectedIndex, pdfDoc.PageCount);
                pageInsertWindow.DialogClosed -= PageInsertWindow_DialogClosed;
                pageInsertWindow.DialogClosed += PageInsertWindow_DialogClosed;
                pageInsertWindow.Owner = parentWindow;
                pageInsertWindow.ShowDialog();
            }
            else if (PageEditString == "Replace")
            {
                DoReplace();
            }
            else if (PageEditString == "Extract" && pdfDoc!=null)
            {
                CPDFPageExtractWindow pageExtractWindow = new CPDFPageExtractWindow();
                Window parentWindow = Window.GetWindow(this);
                pageExtractWindow.InitPageExtractWindow(SelectedItemsRange, pdfDoc.PageCount);
                pageExtractWindow.DialogClosed -= PageExtractWindow_DialogClosed;
                pageExtractWindow.DialogClosed += PageExtractWindow_DialogClosed;
                pageExtractWindow.Owner = parentWindow;
                pageExtractWindow.ShowDialog();  
            }
            else if (PageEditString == "Copy")
            {
                DoCopy();
                DoPaste();
                ViewportHelper.CopyDoc = null;
            }
            else if (PageEditString == "Rotate")
            {
                DoRotate(90);
            }
            else if (PageEditString == "Delete")
            {
                DoDelete(list, true);
            }
        }

        private void PageInsertWindow_DialogClosed(object sender, InsertDialogCloseEventArgs e)
        {
            InsertEventClass result = e.DialogResult;
            if (result != null)
            {
                DoInsert(result);
                CanSave = true;
            }
            else
            {

            }
        }

        private void PageExtractWindow_DialogClosed(object sender, ExtractDialogCloseEventArgs e)
        {
            ExtractEventClass result = e.DialogResult;
            if (result != null)
            {
                DoExtract(result);
            }
            else
            {

            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        { 
            AlertBorder.Visibility = Visibility.Collapsed;
             
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= Timer_Tick;
        }

        #region Shortcut key binding
        private void CommandBinding_Executed_LeftRotate(object sender, ExecutedRoutedEventArgs e)
        {
            DoRotate(-90);
        }

        private void CommandBinding_Executed_RightRotate(object sender, ExecutedRoutedEventArgs e)
        {
            DoRotate(90);
        }

        private void CommandBinding_Executed_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            var list = GetListFromSelectedItems();
            DoDelete(list, true);
        }

        private void CommandBinding_Executed_Copy(object sender, ExecutedRoutedEventArgs e)
        {
            DoCopy();
            DoPaste();
            ViewportHelper.CopyDoc = null;
        }

        #endregion

        #region Event
        private void PdfViewer_InfoChanged(object sender, KeyValuePair<string, object> e)
        {
            if (e.Key == "RenderNum" && this.Visibility == Visibility.Visible && !isHided)
            {
                UpdateSelectedIndex();
            }
        }

        private void _pdfviewer_OnThumbnailGenerated(int pageIndex, byte[] thumb, int w, int h)
        {
            try
            {
                if (PageEditListBox.Items.IsEmpty)
                {
                    return;
                }

                ListBoxItem listboxitem = PageEditListBox.Items[pageIndex] as ListBoxItem;
                PixelFormat fmt = PixelFormats.Bgra32;
                BitmapSource bps = BitmapSource.Create(w, h, 96.0, 96.0, fmt, null, thumb, (w * fmt.BitsPerPixel + 7) / 8);
                Image image = GetImageElement(listboxitem);
                image.Source = bps;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PageEditListBox.Focus();
            CPDFViewer pdfViewer = GetPDFViewer();
            if(pdfViewer!=null && pdfViewer.CurrentRenderFrame!=null)
            {
                int pageIndex=pdfViewer.CurrentRenderFrame.PageIndex;
                if (PageEditListBox.HasItems && pageIndex < PageEditListBox.Items.Count)
                {
                    PageEditListBox.ScrollIntoView(PageEditListBox.Items[pageIndex < 0 ? 0 : pageIndex]);
                    PageEditListBox.SelectedIndex = pageIndex;
                }
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ReleaseMouseCapture();
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var item = PageEditListBox.Items[0] as ListBoxItem;

                int oldZoomLevel = zoomLevel;

                if (e.Delta < 0)
                {
                    if (zoomLevel > 0)
                        zoomLevel--;
                }

                else
                {
                    if (zoomLevel < thumbnailSize.Length - 1 && thumbnailSize[zoomLevel + 1] <= PageEditListBox.ActualWidth - 2 * item.Margin.Left)
                        zoomLevel++;
                }

                if (zoomLevel != oldZoomLevel)
                {
                    isZooming = true;
                    RefreshThumbnail(true);
                    isZooming = false;
                }
                ItemsInViewHitTest();
                return;
            }
            if (startChoose)
            {
                ScrollViewer s = GetScrollHost(PageEditListBox);
                var vs = s.VerticalOffset;
                s.ScrollToVerticalOffset(vs - e.Delta);
                return;
            }
        }

        public void ZoomOut()
        {
            if (zoomLevel > 0)
            {
                zoomLevel--;
                isZooming = true;
                PopulateThumbnailList();
                CPDFViewer pdfViewer= GetPDFViewer();
                if(pdfViewer!=null && pdfViewer.CurrentRenderFrame!=null)
                {
                    int pageIndex=pdfViewer.CurrentRenderFrame.PageIndex;
                    if (pageIndex < PageEditListBox.Items.Count)
                    {
                        PageEditListBox.ScrollIntoView(PageEditListBox.Items[pageIndex]);
                    }
                    PageEditListBox.SelectedIndex = pageIndex;
                }
               
                ItemsInViewHitTest();
                isZooming = false;
            }
        }

        public void ZoomIn()
        {
            if (zoomLevel < thumbnailSize.Length - 1)
            {
                zoomLevel++;
                isZooming = true;
                PopulateThumbnailList();
                CPDFViewer pdfViewer = GetPDFViewer();
                if (pdfViewer != null && pdfViewer.CurrentRenderFrame != null)
                {
                    int pageIndex = pdfViewer.CurrentRenderFrame.PageIndex;
                    if (pageIndex < PageEditListBox.Items.Count)
                    {
                        PageEditListBox.ScrollIntoView(PageEditListBox.Items[pageIndex]);
                    }
                    PageEditListBox.SelectedIndex = pageIndex;
                }
                    
                ItemsInViewHitTest();
                isZooming = false;
            }
        }

        public int CanZoomOutOrIn()
        {
            if (zoomLevel <= 0)
                return 0;
            else if (zoomLevel > 0 && zoomLevel < thumbnailSize.Length - 1)
                return 1;
            else
                return 2;

        }

        private void TryChangedZoomFactor()
        {
            try
            {
                if (ChangedZoomFactor != null)
                    ChangedZoomFactor.Invoke(null, null);
            }
            catch
            {

            }
        }

        public void RefreshThumbnail(bool isZooming = false)
        {
            try
            {
                if (viewControl == null) 
                    return;

                var items = GetListFromSelectedItems();
                PopulateThumbnailList();

                CPDFViewer pdfViewer = GetPDFViewer();
                int pageIndex = 0;
                if(pdfViewer!=null && pdfViewer.CurrentRenderFrame != null)
                {
                    pageIndex=pdfViewer.CurrentRenderFrame.PageIndex;
                }

                if (pageIndex >= PageEditListBox.Items.Count)
                {
                    PageEditListBox.ScrollIntoView(PageEditListBox.Items[PageEditListBox.Items.Count - 1]);
                }
                else
                {
                    PageEditListBox.ScrollIntoView(PageEditListBox.Items[pageIndex]);
                }
                    
                if (!isZooming)
                {
                    PageEditListBox.SelectedIndex = pageIndex;
                }
                else
                {
                    PageEditListBox.SelectedItems.Clear();
                    for (int i = 0; i < items.Count; i++)
                    {
                        int index = int.Parse(items[i].Tag.ToString());
                        if (index < PageEditListBox.Items.Count)
                            PageEditListBox.SelectedItems.Add(PageEditListBox.Items[index]);
                    }
                }
                ItemsInViewHitTest();
                RefreshBookMarkList();
            }
            catch 
            { 
            
            }
        }

        private void PageEditListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ItemsInViewHitTest();

            if (startChoose)
            {
                DoSelectItems();
            }

            if (isFirstScrollChange)
            {
                isFirstScrollChange = false;
                TryChangedZoomFactor();
                return;
            }
        }

        private void DoSelectItems()
        {
            var s = GetScrollHost(PageEditListBox);
            Point start = new Point();
            start = new Point(startPostion.X, startPostion.Y - s.VerticalOffset);
            var rec = new Rect(start, Mouse.GetPosition(PageEditListBox));
            ChooseRect.Margin = new Thickness(rec.Left, rec.Top, 0, 0);
            ChooseRect.Width = rec.Width;
            ChooseRect.Height = rec.Height;
            ChooseRect.Visibility = Visibility.Visible;
            for (int i = 0; i < PageEditListBox.Items.Count; i++)
            {
                var _item = PageEditListBox.Items[i] as ListBoxItem;
                var parent = Utils.FindVisualParent<VirtualizingWrapPanel>(_item);
                if (parent == null)
                    continue;

                var v = VisualTreeHelper.GetOffset(_item);
                if (rec.IntersectsWith(new Rect(v.X, v.Y, _item.ActualWidth, _item.ActualHeight)))
                {
                    PageEditListBox.SelectedItems.Add(_item);
                }
                else
                {
                    PageEditListBox.SelectedItems.Remove(_item);
                }
            }
            return;
        }

        private void PageEditListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void PageEditListBox_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            scrollType = e.ScrollEventType;
            if (scrollType != ScrollEventType.EndScroll || isZooming)
                return;
            ItemsInViewHitTest();
        }

        private void PageEditListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.LeftCtrl)
            {
                iskeyDown = true;
                return;
            }
        }

        private void PageEditListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            iskeyDown = true;
            var pos = e.GetPosition(PageEditListBox);
            HitTestResult result = VisualTreeHelper.HitTest(PageEditListBox, pos);
            if (result == null)
            {
                return;
            }
            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            var scroller = Utils.FindVisualParent<ScrollBar>(result.VisualHit);
            if (listBoxItem == null)
            {
                if (scroller != null)
                {
                    startChoose = false;
                    return;
                }

                startChoose = true;
                PageEditListBox.SelectedItems.Clear();
                startPostion = e.GetPosition(PageEditListBox);
                startPostion = new Point(startPostion.X, startPostion.Y + GetWrapPanel(PageEditListBox).VerticalOffset);
                iskeyDown = false;
                Mouse.Capture(PageEditListBox);
                return;
            }
            startChoose = false;
            if (listBoxItem.IsSelected == true && !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                e.Handled = true;
            }
        }

        private void PageEditListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            startChoose = false;
            ChooseRect.Visibility = Visibility.Collapsed;
            var pos = e.GetPosition(PageEditListBox);
            HitTestResult result = VisualTreeHelper.HitTest(PageEditListBox, pos);
            if (result == null)
            {
                return;
            }

            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem == null)
            {
                return;
            }

            if (iskeyDown && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                PageEditListBox.SelectedItems.Clear();
                PageEditListBox.SelectedItem = listBoxItem;
                listBoxItem.IsSelected = true;
                return;
            }
        }

        private void PageEditListBox_Drop(object sender, DragEventArgs e)
        {
            if (!isDraging)
            {
                MidLane.Visibility = Visibility.Collapsed;
                return;
            }
            if (isDragingEnter)
            {
                CPDFDocument dragDoc = CPDFDocument.InitWithFilePath(dragingEnterPath);
                CPDFDocument pdfDoc = GetPDFDocument();
                CPDFViewer pdfViewer=GetPDFViewer();
                if (dragingEnterPath.Substring(dragingEnterPath.LastIndexOf(".")).ToLower() == ".pdf")
                {
                    if (dragDoc != null && pdfDoc!=null)
                    {
                        int index = InsertIndex == -1 ? 0 : InsertIndex;
                        pdfDoc.ImportPagesAtIndex(dragDoc, "1-" + dragDoc.PageCount, index);
                        PopulateThumbnailList();
                        ItemsInViewHitTest();

                        pdfViewer?.UndoManager.ClearHistory();
                        pdfViewer?.UpdateVirtualNodes();
                        PageEditListBox.ScrollIntoView(PageEditListBox.SelectedItem as ListBoxItem);
                        PageMoved?.Invoke(this, new RoutedEventArgs());
                        dragDoc.Release();
                    }
                    else
                    {
                    }
                }
                else if (!string.IsNullOrEmpty(dragingEnterPath))
                {
                }
                MidLane.Visibility = Visibility.Collapsed;
                isDragingEnter = false;
                dragingEnterPath = null;
                return;
            }
            var pos = e.GetPosition(PageEditListBox);
            var result = VisualTreeHelper.HitTest(PageEditListBox, pos);
            if (result == null)
            {
                MidLane.Visibility = Visibility.Collapsed;
                isDraging = false;
                return;
            }
            var sourcePerson = e.Data.GetData(typeof(StackPanel)) as StackPanel;
            if (sourcePerson == null)
            {
                MidLane.Visibility = Visibility.Collapsed;
                isDraging = false;
                return;
            }
            int targetindex = 0;
            if (InsertIndex != -1)
            {
                targetindex = InsertIndex;
            }
            else
            {
                var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null)
                {
                    MidLane.Visibility = Visibility.Collapsed;
                    isDraging = false;
                    return;
                }
                var targetPerson = listBoxItem;
                targetPerson.Opacity = 1;
                sourcePerson.Opacity = 1;
                if (ReferenceEquals(targetPerson, sourcePerson))
                {
                    MidLane.Visibility = Visibility.Collapsed;
                    isDraging = false;
                    return;
                }

                targetindex = PageEditListBox.Items.IndexOf(targetPerson);
            }

            List<ListBoxItem> list = new List<ListBoxItem>();
            List<int> sourceindex = new List<int>();
            List<int> pages = new List<int>();
            for (int i = 0; i < PageEditListBox.SelectedItems.Count; i++)
            {
                var pageindex = PageEditListBox.Items.IndexOf(PageEditListBox.SelectedItems[i] as ListBoxItem);
                pages.Add(pageindex);
            }
            pages.Sort();
            if (pages.Count <= 0)
            {
                MidLane.Visibility = Visibility.Collapsed;
                isDraging = false;
                return;
            }

            if (targetindex <= pages[0])
            {
                sourceindex.Add(-1);
                list = new List<ListBoxItem>();
                for (int i = 0; i < pages.Count; i++)
                {
                    list.Add(PageEditListBox.Items[pages[i]] as ListBoxItem);
                    sourceindex.Add(pages[i]);
                    DragToSort(pages[i], targetindex + i);
                }
            }
            else if (targetindex > pages[pages.Count - 1])
            {
                sourceindex.Add(1);
                list = new List<ListBoxItem>();
                for (int i = 0; i < pages.Count; i++)
                {
                    list.Add(PageEditListBox.Items[pages[pages.Count - 1 - i]] as ListBoxItem);
                    sourceindex.Add(pages[pages.Count - 1 - i]);
                    DragToSort(pages[pages.Count - 1 - i], targetindex - 1 - i);
                }
            }
            else
            {
                int i, j, k;
                for (k = 0; k < pages.Count - 1; k++)
                {
                    if (pages[k] < targetindex && pages[k + 1] >= targetindex)
                        break;
                }

                sourceindex.Add(0);
                list = new List<ListBoxItem>();
                for (i = 0; i <= k; i++)
                {
                    list.Add(PageEditListBox.Items[pages[k - i]] as ListBoxItem);
                    sourceindex.Add(pages[k - i]);
                    DragToSort(pages[k - i], targetindex - 1 - i);
                }
                for (j = i; j < pages.Count; j++)
                {
                    list.Add(PageEditListBox.Items[pages[j]] as ListBoxItem);
                    sourceindex.Add(pages[j]);
                    DragToSort(pages[j], targetindex);
                    targetindex++;
                }
                sourceindex.Add(k); 
            }
            isDraging = false;
        }

        private void MidLane_Drop(object sender, DragEventArgs e)
        {             
            MidLane.Visibility = Visibility.Collapsed;
            PageEditListBox_Drop(sender, e);
        }

        private void PageEditListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    iskeyDown = false;

                    if (startChoose)
                    {
                        var position = e.GetPosition(PageEditListBox);
                        if (position.X < 5 || position.X > PageEditListBox.ActualWidth - 5 || position.Y < 5 || position.Y > PageEditListBox.ActualHeight - 5)
                        {
                            startChoose = false;
                            ChooseRect.Visibility = Visibility.Collapsed;
                            Mouse.Capture(null); 
                            return;
                        }
                        DoSelectItems();
                        return;
                    }
                    var pos = e.GetPosition(PageEditListBox);
                    if (pos.Y < 0 || pos.Y > PageEditListBox.ActualHeight)
                    {
                        MidLane.Visibility = Visibility.Collapsed;
                        return;
                    }
                    HitTestResult result = VisualTreeHelper.HitTest(PageEditListBox, pos);
                    if (result == null)
                    {
                        return;
                    }
                    var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
                    if (listBoxItem == null)
                    {
                        return;
                    }
                    isDragingEnter = false;
                    tempItem = listBoxItem;
                    var panel = GetPanel(tempItem);

                    var item_pos = e.GetPosition(panel);
                    if (item_pos != null)
                    {
                        item_x = item_pos.X;
                        item_y = item_pos.Y;
                    }
                    var scroll = GetScrollHost(PageEditListBox);
                    DataObject dataObj = new DataObject(listBoxItem.Content as StackPanel);
                    DragDrop.DoDragDrop(PageEditListBox, dataObj, DragDropEffects.Move);
                    Mouse.Capture(PageEditListBox);
                    return;
                }
                ChooseRect.Visibility = Visibility.Collapsed;
                startChoose = false;
                Mouse.Capture(null);
            }
            catch (Exception ex)
            {
                MidLane.Visibility = Visibility.Collapsed;
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        { 
            try
            { 
                if (e.KeyStates == (DragDropKeyStates.ControlKey | DragDropKeyStates.LeftMouseButton) || e.KeyStates == (DragDropKeyStates.ShiftKey | DragDropKeyStates.LeftMouseButton | DragDropKeyStates.ControlKey))
                    return;
                 
                var pos = e.GetPosition(PageEditListBox);
                var result = VisualTreeHelper.HitTest(PageEditListBox, pos);
                if (result == null)
                { 
                }
                 
                var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
                if (listBoxItem == null)
                { 
                }

                double xPos, yPos;
                if (!isDragingEnter)
                {
                    //Get the item picture.
                    Image image = GetImageElement(tempItem);
                    Viewbox viewBox = (tempItem.Content as StackPanel).Children[0] as Viewbox;//获取item宽度

                    ShadowPicture.Width = viewBox.ActualWidth;
                    ShadowPicture.Height = viewBox.ActualHeight;
                    ShadowPicture.Source = image.Source;
                    xPos = e.GetPosition(PageEditListBox).X - item_x;
                    yPos = e.GetPosition(PageEditListBox).Y - item_y;
                }
                else
                {
                    var pic = ToBitmapSource(dragingEnterPath);
                    ShadowPicture.Width = pic.Width;
                    ShadowPicture.Height = pic.Height;
                    ShadowPicture.Source = pic;
                    xPos = e.GetPosition(PageEditListBox).X - pic.Width / 2;
                    yPos = e.GetPosition(PageEditListBox).Y - pic.Height / 2;
                }

                ShadowPicture.Margin = new Thickness(xPos, yPos, 0, 0);

                var scroll = GetScrollHost(PageEditListBox);
                if (listBoxItem != null)
                {
                    var p = VisualTreeHelper.GetOffset(listBoxItem);
                    MidLane.Visibility = Visibility.Visible;

                    var panel = GetWrapPanel(PageEditListBox);
                    var item = (PageEditListBox.Items[0] as ListBoxItem).DesiredSize.Width;

                    int count = (int)(panel.ViewportWidth / item);
                    var gap = (panel.ViewportWidth - count * item) / (count + 1) * 1.0;

                    MidLane.X2 = MidLane.X1 = p.X + gap / 2 + listBoxItem.DesiredSize.Width;

                    if (pos.X < p.X + gap / 2 + listBoxItem.ActualWidth / 2)
                    {
                        MidLane.X2 = MidLane.X1 = p.X - gap / 2;
                        InsertIndex = PageEditListBox.Items.IndexOf(listBoxItem);
                    }
                    else
                    {
                        InsertIndex = PageEditListBox.Items.IndexOf(listBoxItem) + 1;
                    }

                    MidLane.Y1 = p.Y;
                    if (MidLane.Y1 < 0)
                        MidLane.Y1 = 0;

                    MidLane.Y2 = p.Y + listBoxItem.ActualHeight;
                    if (MidLane.Y2 < 0)
                    {
                        MidLane.Y2 = 0;
                    }
                }
                 
                if (pos.Y <= 30 || pos.Y >= PageEditListBox.ActualHeight - 10)
                {
                    MidLane.Visibility = Visibility.Collapsed;
                }

                if (pos.X <= 40 || pos.X >= scroll.ViewportWidth - 50)
                {
                    MidLane.Visibility = Visibility.Collapsed;
                }

                speed = 0;
                if (pos.Y >= PageEditListBox.ActualHeight - 30)
                {
                    speed = 30 - (int)(PageEditListBox.ActualHeight - pos.Y);
                }
                else if (pos.Y <= 30)
                {
                    speed = (int)(pos.Y - 30);
                }

                var v = scroll.VerticalOffset;
                scroll.ScrollToVerticalOffset(v + speed); 
            }
            catch (Exception ex)
            {

            }
        }

        private void ShadowPicture_Drop(object sender, DragEventArgs e)
        {
            this.PageEditListBox_Drop(sender, e);
        }
         
        private void ListBoxItem_DragLeave(object sender, DragEventArgs e)
        { 
            isDraging = true;
        }

        public void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (viewControl == null)
            {
                return;
            }
               
            var list = GetListFromSelectedItems();
            DoDelete(list, true);
        }
         
        public void Rotate_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            double step = 90;
            if (item.Name == "LeftRotate")
            {
                step = -90;
            }
            else
            {
                step = 90;
            }
            DoRotate(step);
            PageEditListBox.Focus();
        }


        private void ListBoxItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.GetPosition(PageEditListBox);
            var pos = e.GetPosition(PageEditListBox);
            HitTestResult result = VisualTreeHelper.HitTest(PageEditListBox, pos);
            if (result == null)
            {
                return;
            }

            var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            if (listBoxItem != null)
            {
                CPDFViewer pdfViewer=GetPDFViewer();
            }
            ExitPageEdit?.Invoke(this, new EventArgs());
        }

        public void Copy_Click(object sender, RoutedEventArgs e)
        {
            DoCopy();
            DoPaste();
            ViewportHelper.CopyDoc = null;
        }

        private void PageEditListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PageEditListBox.ScrollIntoView(PageEditListBox.SelectedItem);
        }

        private void PageEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (viewControl == null)
            {
                return;
            }
               
            if (isFirstLoad)
            { 
                isFirstLoad = false;
                return;
            }

            CPDFViewer pdfViewer= GetPDFViewer();
            if(pdfViewer!=null && pdfViewer.CurrentRenderFrame!=null)
            {
                int pageIndex=pdfViewer.CurrentRenderFrame.PageIndex;
                if ((bool)e.NewValue == true && viewControl != null && pageIndex >= 0)
                {
                    RefreshThumbnail();
                }
                if (pageIndex >= PageEditListBox.Items.Count || PageEditListBox.Items.Count == 0)
                {
                    return;
                }
                ListBoxItem item = PageEditListBox.Items[pageIndex == -1 ? 0 : pageIndex] as ListBoxItem;
                PageEditListBox.ScrollIntoView(PageEditListBox.SelectedItem);
                PageEditListBox.SelectedItem = item;
            }
        }

        private async void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                var item = sender as ListBoxItem;
                Image img = GetImageElement(item);
                int itemindex = PageEditListBox.Items.IndexOf(item);
                if (img.Source == null)
                {
                    await GetThumbnail(itemindex, (int)img.Width, (int)img.Height);
                }
                CPDFViewer pdfViewer = GetPDFViewer();
                pdfViewer?.GoToPage(itemindex, new Point(0, 0));
            }
        }

        private void PageGrid_PreviewDragEnter(object sender, DragEventArgs e)
        {
            var file = (System.Array)e.Data.GetData(DataFormats.FileDrop);
            if (file == null || file.Length > 1) 
            { return; }
            foreach (var f in file)
            {
                dragingEnterPath = f.ToString();
                if (dragingEnterPath != null)
                {
                    isDragingEnter = true;
                }
            }
        }
         
        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewportHelper.CopyDoc == null)
            {
                var s = sender as ContextMenu;
                foreach (var item in s.Items)
                {
                    if ((item as MenuItem).Name == "Paste")
                    {
                        (item as MenuItem).IsEnabled = false;
                    }
                }
            }
            else
            {
                var s = sender as ContextMenu;
                foreach (var item in s.Items)
                {
                    if ((item as MenuItem).Name == "Paste")
                    {
                        (item as MenuItem).IsEnabled = true;
                    }
                }
            }

            if (PageEditListBox.SelectedItems.Count < 2)
            {
                var s = sender as ContextMenu;
                foreach (var item in s.Items)
                {
                    if ((item as MenuItem).Name == "Exchange")
                    {
                        (item as MenuItem).IsEnabled = false;
                    }
                }
            }
            else
            {
                var s = sender as ContextMenu;
                foreach (var item in s.Items)
                {
                    if ((item as MenuItem).Name == "Exchange")
                    {
                        (item as MenuItem).IsEnabled = true;
                    }
                }
            }
        }


        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A)
            {
                if (PageEditListBox.SelectedItems.Count == PageEditListBox.Items.Count)
                {
                    PageEditListBox.UnselectAll();
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Method
        public void LoadThumbnails(PDFViewControl viewer)
        {
            if (viewer == null)
            {
                return;
            }

            if (viewControl != null && viewer != viewControl) 
            {
                viewControl = viewer;
                this.PopulateThumbnailList();
                OnThumbnailGenerated -= _pdfviewer_OnThumbnailGenerated;
                OnThumbnailGenerated += _pdfviewer_OnThumbnailGenerated;
            }

            viewControl = viewer;

            if (!isThumbInitialized)
            {
                this.PopulateThumbnailList();
                isThumbInitialized = true;
                OnThumbnailGenerated -= _pdfviewer_OnThumbnailGenerated;
                OnThumbnailGenerated += _pdfviewer_OnThumbnailGenerated;
            }
            CPDFViewer pdfviewer = GetPDFViewer();
            if(pdfviewer != null && pdfviewer.CurrentRenderFrame!=null)
            {
                int pageIndex = pdfviewer.CurrentRenderFrame.PageIndex;
                if (pageIndex >= 0 && PageEditListBox.Items.Count > pageIndex)
                {
                    PageEditListBox.ScrollIntoView(PageEditListBox.Items[pageIndex]);
                    (PageEditListBox.Items[pageIndex] as ListBoxItem).IsSelected = true;
                    PageEditListBox.SelectedIndex = pageIndex;
                }
            }
           
            RefreshBookMarkList();
        }

        public void PopulateThumbnailList()
        {
            visiblePageIndexes.Clear();
            bindPageList.Clear();
            // GC.Collect();
            CPDFDocument pdfDoc= GetPDFDocument();
            if(pdfDoc == null || pdfDoc.IsValid()==false)
            {
                return;
            }
            int thumbnailWidth = thumbnailSize[zoomLevel];
            for (int i = 0; i < pdfDoc.PageCount; i++)
            {
                Size pageSize = DataConversionForWPF.CSizeConversionForSize(pdfDoc.GetPageSize(i));
                if (pageSize.Height == 0 || pageSize.Width == 0)
                    continue;

                int imageWidth = pageSize.Width > pageSize.Height ? thumbnailWidth * 2 : (int)(pageSize.Width / pageSize.Height * thumbnailWidth * 2);
                int imageHeight = pageSize.Height > pageSize.Width ? thumbnailWidth * 2 : (int)(pageSize.Height / pageSize.Width * thumbnailWidth * 2);

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
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB00")),
                    Margin = new Thickness(0, 16, 16, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Visibility = Visibility.Collapsed,
                };
                grid.Children.Add(bookmark);

                Border border = new Border()
                {
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A2AE")),
                    Child = grid,
                };

                Viewbox viewBox = new Viewbox()
                {
                    Margin = new Thickness(0, 8, 0, 0),
                    Stretch = Stretch.Uniform,
                    Width = thumbnailWidth,
                    Height = thumbnailWidth,
                    Child = border,
                };

                TextBlock text = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (i + 1).ToString(),
                    Foreground = Brushes.Black,
                    Margin = new Thickness(0, 8, 0, 8),
                };

                StackPanel panel = new StackPanel();
                panel.Children.Add(viewBox);
                panel.Children.Add(text);

                ListBoxItem item = new ListBoxItem();
                item.Content = panel;
                item.Tag = i;
                item.Margin = new Thickness(6, 10, 6, 10);
                item.HorizontalContentAlignment = HorizontalAlignment.Left;
                item.VerticalContentAlignment = VerticalAlignment.Center;
                bindPageList.Add(item);
            }

            PageEditListBox.ItemsSource = bindPageList;
        }
         
        public ListBoxItem GetNewItem(int ItemIndex)
        {
            int thumbnailWidth = thumbnailSize[zoomLevel];
           CPDFDocument pdfDoc= GetPDFDocument();
            if(pdfDoc == null)
            {
                return new ListBoxItem();
            }
            Size pageSize = DataConversionForWPF.CSizeConversionForSize(pdfDoc.GetPageSize(ItemIndex));
            if (pageSize.Width == 0 || pageSize.Height == 0) 
            {
                pageSize = new Size(228, 300);
            }
            int imageWidth = pageSize.Width > pageSize.Height ? thumbnailWidth * 2 : (int)(pageSize.Width / pageSize.Height * thumbnailWidth * 2);
            int imageHeight = pageSize.Height > pageSize.Width ? thumbnailWidth * 2 : (int)(pageSize.Height / pageSize.Width * thumbnailWidth * 2);

            Image img = new Image()
            { 
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
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBB00")),
                Margin = new Thickness(0, 8, 8, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Visibility = Visibility.Collapsed,
            };
            grid.Children.Add(bookmark);

            Border border = new Border()
            {
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A2AE")),
                Child = grid,
            };

            Viewbox viewBox = new Viewbox()
            {
                Margin = new Thickness(0, 8, 0, 0),
                Stretch = Stretch.Uniform,
                Width = thumbnailWidth,
                Height = thumbnailWidth,
                Child = border,
            };

            TextBlock text = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = (ItemIndex + 1).ToString(),
                Foreground = Brushes.Black,
                Margin = new Thickness(0, 8, 0, 8),
            };
            StackPanel panel = new StackPanel();
            panel.Children.Add(viewBox);
            panel.Children.Add(text);

            ListBoxItem item = new ListBoxItem();
            item.Content = panel;
            item.Tag = ItemIndex;
            item.Margin = new Thickness(6, 10, 6, 10);
            return item;
        }
         
        public void AddBlankPages(int pagecount, int insertindex)
        {
            for (int i = 0; i < pagecount; i++)
            {
                var item = GetNewItem(insertindex + i);
                bindPageList.Insert(insertindex + i, item);
                if (visiblePageIndexes.Contains(insertindex + i))
                    visiblePageIndexes.Remove(insertindex + i); 
            }
            PageEditListBox.UpdateLayout();
        }
         
        private async void RenderPage(int pageindex)
        {
            var range = GetRoughViewportRange(PageEditListBox, (PageEditListBox.Items[0] as ListBoxItem).DesiredSize, new Thickness(6, 10, 6, 10));
            if (pageindex < range.Item1 || pageindex > range.Item2)
                return;
            ListBoxItem item = PageEditListBox.Items[pageindex] as ListBoxItem;
            Image img = GetImageElement(item);
            await GetThumbnail(pageindex, (int)img.Width, (int)img.Height);
        }

        private async void ItemsInViewHitTest()
        {
            CPDFDocument pdfDoc= GetPDFDocument();
            if (viewControl == null || pdfDoc == null)
            {
                return;
            }
            try
            {
                ScrollViewer sv = GetScrollHost(PageEditListBox);

                if (sv == null || VisualTreeHelper.GetParent(this) == null)
                {
                    return;
                }

                var range = GetRoughViewportRange(PageEditListBox, (PageEditListBox.Items[0] as ListBoxItem).DesiredSize, new Thickness(6, 10, 6, 10));
                for (int i = 0; i < PageEditListBox.Items.Count; ++i)
                { 
                    ListBoxItem item = PageEditListBox.Items[i] as ListBoxItem; 
                    Image img = GetImageElement(item);
                    if (i >= (range.Item1 - 1) && i <= range.Item2 || ViewportHelper.IsInViewport(sv, item)) 
                    {
                        if (img.Source == null && !visiblePageIndexes.Contains(i))
                        {
                            visiblePageIndexes.Add(i);
                            await Task.Delay(1);
                            await GetThumbnail(i, (int)img.Width, (int)img.Height);
                            //Debug.WriteLine("Page {0} is visible, asking for thumb", (i + 1));
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
                                //Debug.WriteLine("Page {0} is out of range, removed thumb", (i + 1));
                            }
                            else
                            {
                                //Debug.WriteLine("Page {0} is out of range, but had no thumb", (i + 1));
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

        public void RefreshBookMarkList()
        {
            CPDFDocument pdfDoc = GetPDFDocument();
            if (pdfDoc != null)
            {
                var booklist = pdfDoc.GetBookmarkList();
                if (booklist == null) return;
                List<int> marks = new List<int>();
                for (int i = 0; i < booklist.Count; i++)
                {
                    marks.Add(booklist[i].PageIndex);
                }

                for (int k = 0; k < PageEditListBox.Items.Count; k++)
                {
                    Polygon polygon = GetBookMarkIco(PageEditListBox.Items[k] as ListBoxItem);
                    if (marks.Contains(k))
                        polygon.Visibility = Visibility.Visible;
                    else
                        polygon.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void UpdateSelectedIndex()
        {
            CPDFViewer pdfView=GetPDFViewer();
            if(pdfView != null && pdfView.CurrentRenderFrame!=null)
            {
                int pageIndex=pdfView.CurrentRenderFrame.PageIndex;

                if (PageEditListBox.Items.Count == 0)
                {
                    PageEditListBox.SelectedIndex = -1;
                }
                else if (PageEditListBox.Items.Count <= pageIndex)
                {
                    PageEditListBox.SelectedIndex = 0;
                }
                else if (PageEditListBox.SelectedIndex != pageIndex)
                {
                    PageEditListBox.SelectedIndex = pageIndex;
                }
                PageEditListBox.ScrollIntoView(PageEditListBox.Items[PageEditListBox.SelectedIndex]);
            }
        }

        private Image GetImageElement(ListBoxItem item)
        {
            Viewbox viewBox = (item.Content as StackPanel).Children[0] as Viewbox;
            Image img = ((viewBox.Child as Border).Child as Grid).Children[0] as Image;
            return img;
        }

        private Polygon GetBookMarkIco(ListBoxItem item)
        {
            Viewbox viewBox = (item.Content as StackPanel).Children[0] as Viewbox;
            Polygon ico = ((viewBox.Child as Border).Child as Grid).Children[1] as Polygon;
            return ico;
        }

        private StackPanel GetPanel(ListBoxItem item)
        {
            StackPanel panel = item.Content as StackPanel;
            return panel;
        }

        private TextBlock GetPageNumTextBlock(ListBoxItem item)
        {
            TextBlock text = (item.Content as StackPanel).Children[1] as TextBlock;
            return text;
        }

        public VirtualizingWrapPanel GetWrapPanel(ListBox listBox)
        {
            Border border = VisualTreeHelper.GetChild(listBox, 0) as Border;
            var panel = Utils.FindVisualChild<VirtualizingWrapPanel>(border);
            return panel;
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
         
        private void DragToSort(int sourceindex, int targetindex)
        {
            if (targetindex == sourceindex || targetindex < 0)
            {
                MidLane.Visibility = Visibility.Collapsed;
                return;
            }
            var source = PageEditListBox.Items[sourceindex];
            bindPageList.RemoveAt(sourceindex);
            MidLane.Visibility = Visibility.Collapsed;

            bindPageList.Insert(targetindex, source as ListBoxItem);
            PageEditListBox.SelectedItems.Add(source);

            CPDFDocument pdfDoc=GetPDFDocument();
            if(pdfDoc != null)
            {
                var result = pdfDoc.MovePage(sourceindex, targetindex);
                if (!result)
                {
                    MidLane.Visibility = Visibility.Collapsed;
                    return;
                }
            }
           
            CPDFViewer pdfViewer=GetPDFViewer();
            if(pdfViewer!=null)
            {
                pdfViewer.UpdateVirtualNodes();
                pdfViewer.GoToPage(PageEditListBox.SelectedIndex, new Point(0, 0));
                pdfViewer.UndoManager?.ClearHistory();
            }

            ItemsInViewHitTest();
            this.PageMoved?.Invoke(this, new RoutedEventArgs());
            UpdateSortedPageNum(sourceindex, targetindex);
            CanSave = true;
        }
         
        private void UpdateSortedPageNum(int sourceIndex, int targetIndex)
        {
            int sum = sourceIndex + targetIndex;
            targetIndex = targetIndex > sourceIndex ? targetIndex : sourceIndex; 
            sourceIndex = sum - targetIndex;
            sourceIndex = sourceIndex <= 0 ? 0 : sourceIndex;
            targetIndex = targetIndex + 1 > PageEditListBox.Items.Count ? PageEditListBox.Items.Count - 1 : targetIndex;
            for (int i = sourceIndex; i <= targetIndex; i++)
            {
                var item = PageEditListBox.Items[i] as ListBoxItem;
                item.Tag = i;
                TextBlock pagenum = GetPageNumTextBlock(item);
                pagenum.Text = (i + 1).ToString();
            }
        }
         
        private void UpdateAllPageNum()
        {
            for (int i = 0; i < PageEditListBox.Items.Count; i++)
            {
                var pagenum = GetPageNumTextBlock(PageEditListBox.Items[i] as ListBoxItem);
                pagenum.Text = (i + 1).ToString();
                (PageEditListBox.Items[i] as ListBoxItem).Tag = i;
            }
        }

        public void DoInsert(EventArgs e)
        {
            var data = e as InsertEventClass;
            CPDFDocument pdfDoc = GetPDFDocument();
            if (data.InsertType == InsertType.BlankPages)
            {
                if(pdfDoc!=null)
                {
                    var size = pdfDoc.GetPageSize(data.InsertIndex - 1);
                    if (size.width == 0 || size.height == 0)
                    {
                        size = pdfDoc.GetPageSize(data.InsertIndex);
                    }
                    pdfDoc.InsertPage(data.InsertIndex, size.width, size.height, "");
                }
                
                RefreshThumbnail();
                PageEditListBox.SelectedItems.Clear();
                PageEditListBox.SelectedIndex = data.InsertIndex;
            }
            else if (data.InsertType == InsertType.CustomBlankPages)
            {
                pdfDoc.InsertPage(data.InsertIndex, (float)data.PageWidth, (float)data.PageHeight, "");
                RefreshThumbnail();
                PageEditListBox.SelectedItems.Clear();
                PageEditListBox.SelectedIndex = data.InsertIndex;
            }
            else
            {
                DoAddFromOtherPdf(data.FilePath, data.PageRange, data.InsertIndex, data.Password);
            }
            UpdateAllPageNum();
            CPDFViewer pdfViewer= GetPDFViewer();
            if(pdfViewer!=null)
            {
                pdfViewer?.UndoManager?.ClearHistory();
                pdfViewer.UpdateVirtualNodes();
            }
           
            PageEditListBox.ScrollIntoView(PageEditListBox.SelectedItem as ListBoxItem);
            ItemsInViewHitTest(); 
        }

        public void DoReplace()
        {
            if (PageEditListBox.SelectedItems.Count < 1)
            {
                ShowAlertWithTimeout(AlertType.EmptyPageAlert);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "(*.pdf)|*.pdf";
            if ((bool)dialog.ShowDialog())
            { 
                CPDFDocument doc = CPDFDocument.InitWithFilePath(dialog.FileName); 
                int pagecount = doc.PageCount;

                int insertindex = PageEditListBox.Items.IndexOf(PageEditListBox.SelectedItem);
                if (insertindex == -1)
                    insertindex = 0;
                CPDFDocument pdfDoc = GetPDFDocument();
                if(pdfDoc!=null)
                {
                    pdfDoc.ImportPagesAtIndex(doc, "1-" + pagecount, insertindex);
                }
                doc.Release(); 
                for (int i = 0; i < pagecount; i++)
                {
                    var item = GetNewItem(insertindex + i);
                    bindPageList.Insert(insertindex + i, item);
                    if (visiblePageIndexes.Contains(insertindex + i))
                        visiblePageIndexes.Remove(insertindex + i); 
                }
                CPDFViewer pdfViewer=GetPDFViewer();
                if(pdfViewer != null)
                {
                    pdfViewer.UndoManager?.ClearHistory();
                }

                var list = GetListFromSelectedItems();
                for (int i = insertindex; i < (insertindex + pagecount); i++)
                {
                    if (i < PageEditListBox.Items.Count)
                    {
                        ListBoxItem t = PageEditListBox.Items[i] as ListBoxItem;
                        PageEditListBox.SelectedItems.Add(t);
                    }
                }
                DoDelete(list, false);
                UpdateAllPageNum(); 
                RefreshBookMarkList();
                ItemsInViewHitTest(); 
            }
        }

        private void DoRotate(double angleStep)
        {
            if (viewControl == null) return;
            if (PageEditListBox.SelectedItems.Count < 1)
            {
                ShowAlertWithTimeout(AlertType.EmptyPageAlert);
                return;
            }
            List<ListBoxItem> pages = new List<ListBoxItem>();
            CPDFViewer pdfViewer = GetPDFViewer();
            for (int i = 0; i < PageEditListBox.SelectedItems.Count; i++)
            {
                var image = GetImageElement(PageEditListBox.SelectedItems[i] as ListBoxItem);

                pages.Add(PageEditListBox.SelectedItems[i] as ListBoxItem);

                double angle = (double)image.LayoutTransform.GetValue(RotateTransform.AngleProperty);
                angle += angleStep;
                angle = angle % 360;
                image.LayoutTransform = new RotateTransform(angle, image.ActualWidth / 2, image.ActualHeight / 2);
                var index = PageEditListBox.Items.IndexOf(PageEditListBox.SelectedItems[i]);
                CPDFDocument pdfDoc = GetPDFDocument();
                if(pdfDoc!=null)
                {
                    pdfDoc.RotatePage(index, (int)angleStep / 90);
                    pdfDoc.ReleasePages(index);
                }

                pdfViewer?.UndoManager?.ClearHistory();

                if (visiblePageIndexes.Contains(index))
                    visiblePageIndexes.Remove(index);
            }
            pdfViewer?.UpdateVirtualNodes();
            ItemsInViewHitTest();
            CanSave = true;
        }
         
        public static string CreateFilePath(string path)
        {
            int i = 1;
            string oldDestName = path;
            do
            {
                if (File.Exists(path))
                {
                    int lastDot = oldDestName.LastIndexOf('.');

                    string fileExtension = string.Empty;

                    string fileName = oldDestName;

                    if (lastDot > 0)
                    {
                        fileExtension = fileName.Substring(lastDot);

                        fileName = fileName.Substring(0, lastDot);
                    }

                    path = fileName + string.Format(@"({0})", i) + fileExtension;
                }
                ++i;

            } while (File.Exists(path));
            return path;
        }

        public void DoExtract(EventArgs e)
        {
            var data = e as ExtractEventClass;
            CPDFDocument pdfDoc= GetPDFDocument();
            if(pdfDoc== null)
            {
                return;
            }
            int pagecount = pdfDoc.PageCount;
            string pageName = ""; 
            List<int> pagenums = new List<int>();
            pagenums.Clear();
            switch (data.PageMode)
            {
                case 1: 
                    for (int i = 0; i < pagecount; i++)
                    {
                        pagenums.Add(i + 1);
                    }
                    pageName = "1-" + pagecount;
                    break;
                case 2: 
                    int count = (pagecount + 1) / 2;
                    for (int i = 0; i < count; i++)
                    {
                        pagenums.Add(i * 2 + 1);
                    }
                    pageName = "OddPages";
                    break;
                case 3: 
                    if (pagecount == 1) 
                        return;
                    count = pagecount / 2;
                    for (int i = 0; i < count; i++)
                    {
                        pagenums.Add(i * 2 + 2);
                    }
                    pageName = "EvenPages";
                    break;
                case 4: 
                    pagenums = data.PageParm;
                    pageName = data.PageName;
                    break;
                default:
                    break;
            }

            Winform.FolderBrowserDialog dialog = new Winform.FolderBrowserDialog();
            if (dialog.ShowDialog() == Winform.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return;
                }
            }
            else
                return; 
            string selectedfile = ""; 
            if (data.ExtractToSingleFile)
            {
                for (int i = 0; i < pagenums.Count; i++)
                {
                    string filename = pdfDoc.FileName + " " + (pagenums[i]) + ".pdf";
                    string path = System.IO.Path.Combine(dialog.SelectedPath, filename);
                    path = CreateFilePath(path);
                    selectedfile = path;
                    CPDFDocument savedoc = CPDFDocument.CreateDocument();
                    bool result = savedoc.ImportPages(pdfDoc, (pagenums[i]).ToString());
                    if (!result)
                    { 
                        savedoc.Release();
                        continue;
                    }
                    result = savedoc.WriteToFilePath(path);
                    if (!result)
                    {
                        savedoc.Release();
                        continue; 
                    }
                    savedoc.Release(); 
                }
            }
            else 
            {
                string filename = pdfDoc.FileName + " " + pageName + ".pdf";
                string path = System.IO.Path.Combine(dialog.SelectedPath, filename);
                path = CreateFilePath(path);
                selectedfile = path;
                CPDFDocument savedoc = CPDFDocument.CreateDocument();
                string range = (data.PageMode == 1 || data.PageMode == 4) ? pageName : String.Join(",", pagenums);
                bool result = savedoc.ImportPages(pdfDoc, range);
                if (!result)
                {
                    return; 
                }
                result = savedoc.WriteToFilePath(path);
                if (!result)
                { 
                } 
                savedoc.Release(); 
            }

            if (data.DeleteAfterExtract) 
            {
                List<ListBoxItem> items = new List<ListBoxItem>();
                for (int i = 0; i < pagenums.Count; i++)
                {
                    ListBoxItem item = PageEditListBox.Items[pagenums[i]-1] as ListBoxItem;
                    items.Add(item);
                }

                DoDelete(items, false);
                CPDFViewer pdfViewer= GetPDFViewer();
                if(pdfViewer != null)
                {
                    pdfViewer.UndoManager?.ClearHistory();
                }
               
                ItemsInViewHitTest();
            }
            Process.Start(@"explorer.exe", "/select,\"" + selectedfile + "\"");

        }

        private void DoPaste()
        {
            if (viewControl == null) return;
            if (ViewportHelper.CopyDoc == null)
            {
                return;
            }
            int insertIndex = 0; 
            for (int k = 0; k < PageEditListBox.SelectedItems.Count; k++)
            {
                if (insertIndex < PageEditListBox.Items.IndexOf(PageEditListBox.SelectedItems[k] as ListBoxItem))
                    insertIndex = PageEditListBox.Items.IndexOf(PageEditListBox.SelectedItems[k] as ListBoxItem);
            }
            insertIndex++;
            int pagecount = ViewportHelper.CopyDoc.PageCount;
            CPDFDocument pdfDoc = GetPDFDocument();
            if (pdfDoc == null)
            {
                return;
            }
            bool result = pdfDoc.ImportPagesAtIndex(ViewportHelper.CopyDoc, "1-" + pagecount, insertIndex);
            if (!result)
            {
                return;
            }
            CPDFViewer pdfViewer=GetPDFViewer();
            pdfViewer?.UpdateVirtualNodes();
            AddBlankPages(pagecount, insertIndex);
            ItemsInViewHitTest();
            pdfViewer?.UndoManager?.ClearHistory();
            RefreshBookMarkList();

            List<ListBoxItem> pageList = new List<ListBoxItem>();
            PageEditListBox.SelectedItems.Clear();
            for (int i = 0; i < pagecount; i++)
            {
                pageList.Add(PageEditListBox.Items[insertIndex + i] as ListBoxItem);
                PageEditListBox.SelectedItems.Add(PageEditListBox.Items[insertIndex + i] as ListBoxItem);
            }
            CanSave = true;
            UpdateAllPageNum();
        }

        private void DoCopy()
        {
            if (viewControl == null) return;
            if (PageEditListBox.SelectedItems.Count < 1)
            {
                ShowAlertWithTimeout(AlertType.EmptyPageAlert);
                return;
            }
            ViewportHelper.CopyDoc = null;
            ViewportHelper.CopyDoc = CPDFDocument.CreateDocument();
            CPDFDocument pdfDoc=GetPDFDocument();
            if(pdfDoc != null)
            {
                string pageParam = GetPageParam();
                ViewportHelper.CopyDoc.ImportPages(pdfDoc, pageParam);
            }
        }

        private void DoExchange(List<ListBoxItem> pageLists)
        {
            if (pageLists.Count <= 1)
            {
                return;
            }
            List<int> pages = new List<int>();
            for (int i = 0; i < pageLists.Count; i++)
            {
                int pageIndex = PageEditListBox.Items.IndexOf(pageLists[i] as ListBoxItem);
                pages.Add(pageIndex);
            }
            pages.Sort();

            for (int i = 0; i < pages.Count; i++)
            {
                int preindex = pages[i];
                int laterIndex = pages[pages.Count - i - 1]; 
                if (laterIndex <= preindex) 
                    return;
                var sourceitem = PageEditListBox.Items[preindex] as ListBoxItem;
                var targetitem = PageEditListBox.Items[laterIndex] as ListBoxItem;

                bindPageList.Remove(sourceitem);
                bindPageList.Remove(targetitem);
                bindPageList.Insert(preindex, targetitem);
                bindPageList.Insert(laterIndex, sourceitem);

                CPDFDocument pdfDoc= GetPDFDocument();
                pdfDoc?.ExchangePage(preindex, laterIndex);
                CPDFViewer pdfViewer=GetPDFViewer();
                pdfViewer?.UndoManager?.ClearHistory();

                PageEditListBox.SelectedItems.Add(targetitem);
                PageEditListBox.SelectedItems.Add(sourceitem);
            }
        }
         
        private void DoDelete(List<ListBoxItem> pageLists, bool tip)
        {
            if (pageLists.Count == 0)
            {
                ShowAlertWithTimeout(AlertType.EmptyPageAlert);
                return;
            }
            if (pageLists.Count == PageEditListBox.Items.Count)
            {
                ShowAlertWithTimeout(AlertType.AllPageAlert);
                return;
            }
            Winform.DialogResult result = Winform.DialogResult.OK;
            if (tip)
            {
                result = Winform.MessageBox.Show(Helper.LanguageHelper.DocEditorManager.GetString("Warn_Delete")
                    , Helper.LanguageHelper.CommonManager.GetString("Caption_Warning")
                    , Winform.MessageBoxButtons.OKCancel, Winform.MessageBoxIcon.Warning);
            }
            if (result == Winform.DialogResult.OK || !tip)
            {
                List<int> pages = new List<int>();
                for (int i = 0; i < pageLists.Count; i++)
                {
                    var index = PageEditListBox.Items.IndexOf(pageLists[i]);
                    pages.Add(index); 
                }
                pages.Sort();

                if (pages.Count == 0) return;
                for (int i = pages.Count - 1; i >= 0; i--)
                {
                    bindPageList.RemoveAt(pages[i]);
                } 
                CPDFDocument pdfDoc=GetPDFDocument();
                if(pdfDoc==null)
                {
                    return;
                }
                var r = pdfDoc.RemovePages(pages.ToArray());
                if (!r)
                {
                    return;
                }
                CPDFViewer pdfViewer= GetPDFViewer();
                pdfViewer?.UndoManager?.ClearHistory();

                UpdateAllPageNum();
                pdfViewer?.UpdateVirtualNodes();
                CanSave = true;
            }
        }

        private List<ListBoxItem> GetListFromSelectedItems()
        {
            List<ListBoxItem> itemlists = new List<ListBoxItem>();
            for (int i = 0; i < PageEditListBox.SelectedItems.Count; i++)
            {
                itemlists.Add(PageEditListBox.SelectedItems[i] as ListBoxItem);
            }
            return itemlists;
        }
         
        public static bool GetPagesInRange(ref List<int> pageList, string pageRange, int count, char[] enumerationSeparator, char[] rangeSeparator, bool inittag = false)
        {
            string[] rangeSplit = pageRange.Split(enumerationSeparator);

            pageList.Clear();

            foreach (string range in rangeSplit)
            {
                int starttag = 1;
                if (inittag)
                {
                    starttag = 0;
                }
                if (range.Contains("-"))
                {
                    try
                    {
                        string[] limits = range.Split(rangeSeparator);
                        if (limits.Length >= 2 && !string.IsNullOrWhiteSpace(limits[0]) && !string.IsNullOrWhiteSpace(limits[1]))
                        {
                            int start = int.Parse(limits[0]);
                            int end = int.Parse(limits[1]);

                            if ((start < starttag) || (end > count) || (start > end))
                            { 
                                return false;
                            }

                            for (int i = start; i <= end; ++i)
                            {
                                if (pageList.Contains(i))
                                {
                                    return false;
                                }

                                pageList.Add(i - 1);
                            }
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                int pageNr;
                try
                { 
                    pageNr = int.Parse(range); 
                }
                catch (Exception) 
                {
                    return false;
                }
                if (pageNr < starttag || pageNr > count)
                {
                    return false; 
                }
                if (pageList.Contains(pageNr))
                {
                    return false; 
                }
                pageList.Add(pageNr - 1);
            }
            return true;
        }
         
        private void DoAddFromOtherPdf(string filepath, string pagerange, int insertindex, string password)
        {
            CPDFDocument doc = CPDFDocument.InitWithFilePath(filepath);
            if(doc.IsLocked && password != string.Empty)
            {
               if(!doc.UnlockWithPassword(password))
                {
                    return;
                }
            }
            int pagecount = 0;
            string insertRange = "";
            if (pagerange == "AllPages")
            {
                pagecount = doc.PageCount;
                insertRange = "1-" + pagecount;
            }
            else if (pagerange == "OddPages")
            {
                pagecount = (doc.PageCount + 1) / 2;
                int[] page = new int[(doc.PageCount + 1) / 2];
                for (int i = 0; i < page.Length; i++)
                {
                    page[i] = i * 2 + 1;
                }
                insertRange = string.Join(",", page);
            }
            else if (pagerange == "EvenPages")
            {
                if (doc.PageCount == 1)
                {
                    insertRange = "1";
                    pagecount = 1;
                }
                else
                {
                    pagecount = doc.PageCount / 2;
                    int[] page = new int[doc.PageCount / 2];
                    for (int i = 0; i < page.Length; i++)
                    {
                        page[i] = i * 2 + 2;
                    }
                    insertRange = string.Join(",", page);
                }
            }
            else
            {
                List<int> page = new List<int>();
                GetPagesInRange(ref page, pagerange, doc.PageCount, new char[] { ',' }, new char[] { '-' });
                insertRange = pagerange;
                pagecount = page.Count;
            }
            CPDFDocument pdfDoc = GetPDFDocument();
            if(pdfDoc == null)
            {
                return;
            }
            var result = pdfDoc.ImportPagesAtIndex(doc, insertRange, insertindex);
            if (!result)
            {
                return;
            }
            doc.Release(); 
            RefreshThumbnail();
            PageEditListBox.SelectedItems.Clear();
            for (int i = insertindex; i < insertindex + pagecount; i++)
            {
                PageEditListBox.SelectedItems.Add(PageEditListBox.Items[i] as ListBoxItem);
            }
        }

        private string GetPageParam()
        {
            string pageParam = "";
            List<int> pagesList = new List<int>();

            for (int i = 0; i < PageEditListBox.SelectedItems.Count; i++)
            {
                var item = PageEditListBox.SelectedItems[i] as ListBoxItem;
                var page = GetPageNumTextBlock(item);
                if (page != null)
                {
                    pagesList.Add(int.Parse(page.Text));
                }
            }

            if (pagesList.Count != 0)
            {
                pagesList.Sort(); 

                for (int i = 0; i < pagesList.Count; i++)
                {
                    if (i == 0)
                    {
                        pageParam += pagesList[0].ToString();
                    }
                    else
                    {
                        if (pagesList[i] == pagesList[i - 1] + 1) 
                        {
                            if (i >= 2)
                            {
                                if (pagesList[i - 1] != pagesList[i - 2] + 1)
                                    pageParam += "-";
                            }
                            else
                                pageParam += "-";

                            if (i == pagesList.Count - 1)
                            {
                                pageParam += pagesList[i].ToString();
                            }
                        }
                        else 
                        {
                            if (i >= 2)
                            {
                                if (pagesList[i - 1] == pagesList[i - 2] + 1)
                                    pageParam += pagesList[i - 1].ToString();
                            }
                            pageParam += "," + pagesList[i].ToString();
                        }
                    }
                }
            }
            return pageParam;
        }
         
        public static BitmapSource ToBitmapSource(string path)
        {
            System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(path);
            System.Drawing.Bitmap bitmap = ico.ToBitmap();
            BitmapSource returnSource;
            try
            {
                returnSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                returnSource = null;
            }
            return returnSource;
        }
         
        private Tuple<int, int, int> GetRoughViewportRange(ListBox view, Size itemSize, Thickness itemMargin)
        {
            var scrollViewer = GetScrollHost(view);
            CPDFDocument pdfDoc = GetPDFDocument();
            if (pdfDoc == null)
            {
                return new Tuple<int, int, int>(0, 0, 0);
            }
            if (viewControl == null || pdfDoc == null || scrollViewer == null || scrollViewer.ActualHeight == 0 || scrollViewer.ActualWidth == 0)
                return new Tuple<int, int, int>(0, 0, 0);
            try
            {
                var currentHeight = scrollViewer.ActualHeight - view.Padding.Top;
                var currentWidth = scrollViewer.ActualWidth; 
                var columnCount = (int)(currentWidth / (itemSize.Width + itemMargin.Left));
                var rowCount = (int)Math.Ceiling(currentHeight / (itemSize.Height + itemMargin.Bottom));

                var preItemCount = (int)((scrollViewer.VerticalOffset / scrollViewer.ExtentHeight) * ((pdfDoc.PageCount + columnCount - 1) / columnCount));
                preItemCount = preItemCount * columnCount;
                var preEnd = (int)(((scrollViewer.VerticalOffset + scrollViewer.ActualHeight) / scrollViewer.ExtentHeight) * ((pdfDoc.PageCount + columnCount - 1) / columnCount));
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

        private void ShowAlertWithTimeout(AlertType alertType)
        { 
            AlertBorder.Visibility = Visibility.Visible;
            AlertBorder.Opacity = 1.0;
             
            if (alertType == AlertType.EmptyPageAlert)
            {
                AlertTextBlock.Text = Helper.LanguageHelper.DocEditorManager.GetString("Tip_NoPage");
            }
            else if (alertType == AlertType.SinglePageALert)
            {
                AlertTextBlock.Text = "Please select above two pages.";
            }
            else if (alertType == AlertType.AllPageAlert)
            {
                AlertTextBlock.Text = Helper.LanguageHelper.DocEditorManager.GetString("Tip_AllPage");
            }
             
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 1.0;
            animation.To = 0.0;
            animation.Duration = TimeSpan.FromSeconds(3); 
            AlertBorder.BeginAnimation(OpacityProperty, animation);
        }
        #endregion

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            MidLane.Visibility = Visibility.Collapsed;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ItemsInViewHitTest(); 
        }

        private CPDFDocument GetPDFDocument()
        {
            CPDFViewer pdfViewer = GetPDFViewer();
            if (pdfViewer!=null)
            {
                return pdfViewer.GetDocument();
            }
            return null;
        }

        private CPDFViewer GetPDFViewer()
        {
            if (viewControl != null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
                if (pdfViewer != null)
                {
                    return pdfViewer;
                }
            }
            return null;
        }

        public async Task GetThumbnail(int pageIndex, int imageWidth, int imageHeight)
        {
            if (imageWidth <= 0 || imageHeight <= 0)
            {
                return;
            }
            if (viewControl == null || viewControl.PDFViewTool == null)
            {
                return;
            }

            CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
            CPDFDocument pdfdoc = pdfViewer?.GetDocument();
            if (pdfdoc == null)
            {
                return;
            }

            CPDFPage page = pdfdoc.PageAtIndex(pageIndex);
            byte[] bmpData = new byte[imageWidth * imageHeight * 4];

            await Task.Run(() => page.RenderPageBitmap(0, 0, imageWidth, imageHeight, 0xFFFFFFFF, bmpData, 1, true));
            if (OnThumbnailGenerated != null)
            {
                OnThumbnailGenerated(pageIndex, bmpData, imageWidth, imageHeight);
            }
        }
    }

    public class InsertEventClass : EventArgs
    {
        public InsertType InsertType = InsertType.BlankPages;
        public string FilePath = string.Empty;
        public double PageWidth = 210 * 2.54;
        public double PageHeight = 297 * 2.54;
        public string PageRange = "0";
        public int InsertIndex = 0;
        public string Password = string.Empty;
    }

    public enum InsertType
    {
        BlankPages,
        CustomBlankPages,
        FromOtherPDF
    }

    /// <summary>
    /// The parameter class used for extraction
    /// </summary>
    public class ExtractEventClass : EventArgs
    {
        /// <summary>
        /// 1- All pages 2 - Odd pages 3 - Even pages 4 - Custom ranges
        /// </summary>
        public int PageMode;
        /// <summary>
        ///  PageMode = 1,2,3 PageParm = null;
        /// </summary>
        public List<int> PageParm;
        /// <summary>
        /// In custom mode, the text content is passed over
        /// </summary>
        public string PageName = "";
        /// <summary>
        /// Whether to split into a single file
        /// </summary>
        public bool ExtractToSingleFile;
        /// <summary>
        /// Delete the page after extraction
        /// </summary>
        public bool DeleteAfterExtract;
    }

    public enum AlertType
    {
        EmptyPageAlert,
        SinglePageALert,
        AllPageAlert,
    }
}
