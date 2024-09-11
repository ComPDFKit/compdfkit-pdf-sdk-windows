using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ComPDFKit.Controls.Helper;
using System.Collections.Specialized;
using ComPDFKit.Controls.PDFControl;
using static ComPDFKit.Controls.PDFControlUI.CPDFAnnotationListUI;
using ComPDFKit.Controls.Data;

namespace ComPDFKit.Controls.PDFControlUI
{
    public class ShowReplyListCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is AnnotationBindData data)
            {
                data.IsReplyListVisible = !data.IsReplyListVisible;
            }
        }
    }

    public class ShowReplyInputCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is AnnotationBindData data)
            {
                data.IsReplyInputVisible = !data.IsReplyInputVisible;
                if(data.IsReplyInputVisible)
                {
                    data.IsReplyListVisible = true;
                }
            }
        }
    }

    internal class AnnotationBindData : INotifyPropertyChanged
    {
        public PDFViewControl pdfViewer { get; set; }

        public AnnotParam annotationData { get; set; }

        public string ReplyCount
        {
            get => ReplyList.Count.ToString();
        }

        private bool _isReplyListVisible = true;
        public bool IsReplyListVisible
        {
            get { return _isReplyListVisible; }
            set
            {
                _isReplyListVisible = value;
                OnPropertyChanged(nameof(IsReplyListVisible));
            }
        }

        private bool _isReplyInputVisible;
        public bool IsReplyInputVisible
        {
            get { return _isReplyInputVisible; }
            set
            {
                _isReplyInputVisible = value;
                OnPropertyChanged(nameof(IsReplyInputVisible));
            }
        }

        public int PageIndex { get; set; }

        public int AnnotIndex { get => annotationData.AnnotIndex; }

        public string Author
        {
            get => annotationData.Author;
        }

        public string Note
        {
            get => annotationData.Content;
        }


        private CPDFAnnotation _annotation;
        public CPDFAnnotation Annotation
        {
            get
            {
                List<CPDFAnnotation> annotCoreList = pdfViewer?.GetCPDFViewer()?.GetDocument()?.PageAtIndex(annotationData.PageIndex, false)?.GetAnnotations();
                return annotCoreList[annotationData.AnnotIndex];
            }
        }

        private ObservableCollection<ReplyData> _replyList = new ObservableCollection<ReplyData>();

        public ObservableCollection<ReplyData> ReplyList
        {
            get => _replyList;
            set
            {
                if (_replyList != value)
                {
                    if (_replyList != null)
                    {
                        // Unsubscribe from the previous collection
                        _replyList.CollectionChanged -= ReplyList_CollectionChanged;
                    }

                    _replyList = value;

                    if (_replyList != null)
                    {
                        // Subscribe to the new collection
                        _replyList.CollectionChanged += ReplyList_CollectionChanged;
                    }

                    OnPropertyChanged(nameof(ReplyList));
                }
            }
        }

        private ObservableCollection<AnnotationBindData> annotationList = new ObservableCollection<AnnotationBindData>();

        private void ReplyList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Notify that the ReplyCount has changed when the collection changes
            OnPropertyChanged(nameof(ReplyCount));
        }

        public BindAnnotationResult BindProperty { get; set; }
        public AnnotationBindData()
        {
            BindProperty = new BindAnnotationResult();
        }
        public int ShowPageIndex { get { return BindProperty.PageIndex + 1; } set { BindProperty.PageIndex = value; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class CPDFAnnotationListUI : UserControl
    {
        public event EventHandler<CPDFAnnotationState> ReplyStatusChanged;

        public class ReplyData
        {
            public CPDFAnnotation ReplyAnnotation { get; set; }

            public string Author
            {
                get => ReplyAnnotation.GetAuthor();
                set => ReplyAnnotation.SetAuthor(value);
            }

            public string Date
            {
                get
                {
                    try
                    {
                        if (Regex.IsMatch(ReplyAnnotation.GetCreationDate(), "(?<=D\\:)[0-9]+(?=[\\+\\-])"))
                        {
                            string dateStr = Regex.Match(ReplyAnnotation.GetCreationDate(), "(?<=D\\:)[0-9]+(?=[\\+\\-])").Value;
                            return (dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6, 2) + ", " + dateStr.Substring(8, 2) + ":" +
                                    dateStr.Substring(10, 2));
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }

                }
                set => ReplyAnnotation.SetCreationDate(value);
            }

            public string Content
            {
                get => ReplyAnnotation.GetContent();
                set => ReplyAnnotation.SetContent(value);
            }
        }
         
        public class BindAnnotationResult : INotifyPropertyChanged
        {
            private ObservableCollection<ReplyData> _replyList = new ObservableCollection<ReplyData>();

            public ObservableCollection<ReplyData> ReplyList
            {
                get => _replyList;
                set
                {
                    if (_replyList != value)
                    {
                        if (_replyList != null)
                        {
                            // Unsubscribe from the previous collection
                            _replyList.CollectionChanged -= ReplyList_CollectionChanged;
                        }

                        _replyList = value;

                        if (_replyList != null)
                        {
                            // Subscribe to the new collection
                            _replyList.CollectionChanged += ReplyList_CollectionChanged; ;
                        }

                        OnPropertyChanged(nameof(ReplyList));
                    }
                }
            }

            private void ReplyList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {         
                OnPropertyChanged(nameof(ReplyCount));
            }

            private Visibility _isAnnotListVisible = Visibility.Visible;
            public Visibility IsAnnotListVisible
            {
                get { return _isAnnotListVisible; }
                set
                {
                    _isAnnotListVisible = value;
                    OnPropertyChanged(nameof(IsAnnotListVisible));
                }
            }

            public int PageIndex { get; set; }

            public int AnnotIndex { get => annotationData.AnnotIndex; }

            public string Author
            {
                get => annotationData.Author;
            }

            public string ReplyCount
            {
                get => ReplyList.Count.ToString();
            }

            public string CreateDate
            {
                get
                {
                    if (Regex.IsMatch(annotationData.CreateTime, "(?<=D\\:)[0-9]+(?=[\\+\\-])"))
                    {
                        string dateStr = Regex.Match(annotationData.CreateTime, "(?<=D\\:)[0-9]+(?=[\\+\\-])").Value;
                        return (dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6, 2) + ", " + dateStr.Substring(8, 2) + ":" +
                            dateStr.Substring(10, 2));
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            public string Note
            {
                get => annotationData.Content;
            }

            public CPDFAnnotationType CurrentAnnotationType
            {
                get
                {
                    switch(Annotation.Type)
                    {
                        case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                            return CPDFAnnotationType.Circle;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                            return CPDFAnnotationType.Square;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                            {
                                if(Annotation.IsMeasured())
                                    return CPDFAnnotationType.LineMeasure;
                                else
                                    return CPDFAnnotationType.Line;
                            }

                        case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                             return CPDFAnnotationType.FreeText;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                             return CPDFAnnotationType.Highlight;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                            return CPDFAnnotationType.Squiggly;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                            return CPDFAnnotationType.Strikeout;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                            return CPDFAnnotationType.Underline;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                            return CPDFAnnotationType.Freehand;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                            return CPDFAnnotationType.Note;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                            return CPDFAnnotationType.Stamp;

                        case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                            {
                                if (Annotation.IsMeasured())
                                    return CPDFAnnotationType.PolyLineMeasure;
                                else
                                    return CPDFAnnotationType.PolyLine;
                            }

                        case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                            {
                                if (Annotation.IsMeasured())
                                    return CPDFAnnotationType.PolygonMeasure;
                                else
                                    return CPDFAnnotationType.Polygon;
                            }

                        default:
                            return CPDFAnnotationType.Note;

                    }

                }

            }

            public AnnotParam annotationData { get; set; }

            private CPDFAnnotation _annotation;
            public CPDFAnnotation Annotation
            {
                get
                {
                    List<CPDFAnnotation> annotCoreList = pdfViewer?.GetCPDFViewer()?.GetDocument()?.PageAtIndex(annotationData.PageIndex, false)?.GetAnnotations();
                    return annotCoreList[annotationData.AnnotIndex];
                }
            }

            public PDFViewControl pdfViewer { get; set; }

            public CPDFAnnotationState ReplyState { get; set; }

            public bool IsMarkState { get; set; }
            public BindAnnotationResult()
            {
                ReplyList.CollectionChanged += ReplyList_CollectionChanged;
            }
             
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            } 
        }

        private ObservableCollection<AnnotationBindData> annotationList = new ObservableCollection<AnnotationBindData>();

        public event EventHandler<object> AnnotationSelectionChanged;

        public event EventHandler<Dictionary<int, List<int>>> DeleteItemHandler;

        private ContextMenu popContextMenu;
        private bool enableSelectEvent = true;

        public CPDFAnnotationListUI()
        {
            InitializeComponent();
            ICollectionView groupView = CollectionViewSource.GetDefaultView(annotationList);
            groupView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(AnnotationBindData.ShowPageIndex)));
            AnnotationList.PreviewMouseRightButtonUp += ((sender, args) => { SetContextMenu(); });
        }

        private void SetContextMenu()
        {
            if (AnnotationList.SelectedIndex == -1)
            {
                return;
            }
            popContextMenu = new ContextMenu();
            AnnotationBindData data = annotationList[AnnotationList.SelectedIndex];

            MenuItem deleteMenu = new MenuItem();
            deleteMenu.Header = LanguageHelper.BotaManager.GetString("Menu_Delete");
            deleteMenu.Click -= DeleteMenu_Click;
            deleteMenu.Click += DeleteMenu_Click;
            popContextMenu.Items.Add(deleteMenu);

            MenuItem deleteAllMenu = new MenuItem();
            deleteAllMenu.Header = LanguageHelper.BotaManager.GetString("Menu_DeleteAll");
            deleteAllMenu.Click -= DeleteAllMenu_Click;
            deleteAllMenu.Click += DeleteAllMenu_Click;
            popContextMenu.Items.Add(deleteAllMenu);

            MenuItem replyMenu = new MenuItem();
            replyMenu.Header = LanguageHelper.BotaManager.GetString("Menu_AddReply");
            replyMenu.Click += (sender, e) =>
            {
                if (AnnotationList != null && AnnotationList.SelectedIndex >= 0)
                {
                    data.IsReplyInputVisible = true;
                }
            };
            popContextMenu.Items.Add(replyMenu);

            MenuItem showReplyMenu = new MenuItem();

            if (data.IsReplyListVisible)
            {
                showReplyMenu.Header = LanguageHelper.BotaManager.GetString("Menu_FoldReply");
            }
            else
            {
                showReplyMenu.Header = LanguageHelper.BotaManager.GetString("Menu_ExpandReply");
            }
            showReplyMenu.Click += (sender, e) =>
            {
                if (AnnotationList != null && AnnotationList.SelectedIndex >= 0)
                {
                    data.IsReplyListVisible = !data.IsReplyListVisible;
                }
            };
            popContextMenu.Items.Add(showReplyMenu);

            AnnotationList.ContextMenu = popContextMenu;
        }

        public void DeleteAllAnnot()
        {
            try
            {
                Dictionary<int, List<int>> delDict = new Dictionary<int, List<int>>();

                foreach (AnnotationBindData bindData in annotationList)
                {
                    if (delDict.ContainsKey(bindData.BindProperty.PageIndex) == false)
                    {
                        delDict[bindData.BindProperty.PageIndex] = new List<int>();
                    }
                    delDict[bindData.BindProperty.PageIndex].Add(bindData.BindProperty.AnnotIndex);
                }

                if (delDict.Count > 0)
                {
                    DeleteItemHandler?.Invoke(this, delDict);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void DeleteAllReply()
        {
            try
            {
                foreach (var data in annotationList)
                {
                    foreach (var replyData in data.BindProperty.ReplyList)
                    {
                        replyData.ReplyAnnotation.RemoveAnnot();
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void ExpandAllReply(bool isExpand)
        {
            foreach (AnnotationBindData data in annotationList)
            {
                data.IsReplyListVisible = isExpand;
            }
        }

        public void ExpandAnnotList(bool isExpand)
        {
            foreach (AnnotationBindData data in annotationList)
            {
                if (isExpand)
                    data.BindProperty.IsAnnotListVisible = Visibility.Visible;
                else
                    data.BindProperty.IsAnnotListVisible = Visibility.Collapsed;
            }
        }

        private void DeleteAllMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<int, List<int>> delDict = new Dictionary<int, List<int>>();

                foreach (AnnotationBindData bindData in annotationList)
                {
                    if (delDict.ContainsKey(bindData.BindProperty.PageIndex) == false)
                    {
                        delDict[bindData.BindProperty.PageIndex] = new List<int>();
                    }
                    delDict[bindData.BindProperty.PageIndex].Add(bindData.BindProperty.AnnotIndex);
                }

                if (delDict.Count > 0)
                {
                    DeleteItemHandler?.Invoke(this, delDict);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AnnotationList != null && AnnotationList.SelectedIndex >= 0)
                {
                    AnnotationBindData bindData = annotationList[AnnotationList.SelectedIndex];

                    Dictionary<int, List<int>> delDict = new Dictionary<int, List<int>>();
                    delDict[bindData.BindProperty.PageIndex] = new List<int>()
                    {
                        bindData.BindProperty.AnnotIndex
                    };
                    DeleteItemHandler?.Invoke(this, delDict);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SetAnnotationList(List<BindAnnotationResult> results)
        {
            annotationList.Clear();
            AnnotationList.ContextMenu = null;
            if (results == null || results.Count == 0)
            {
                AnnotationList.ItemsSource = null;
                NoContentText.Visibility = Visibility.Visible;
                return;
            }

            foreach (BindAnnotationResult item in results)
            {
                annotationList.Add(new AnnotationBindData()
                {
                    BindProperty = item
                });
            }
            AnnotationList.ItemsSource = annotationList;
            if (annotationList.Count > 0)
            {
                AnnotationList.ContextMenu = popContextMenu;
            }
            AnnotationList.Visibility = Visibility.Visible;
            NoContentText.Visibility = Visibility.Collapsed;
        }

        private void AnnotationListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (enableSelectEvent)
            {
                try
                {
                    if (e.AddedItems[0] is AnnotationBindData annotationBindData)
                    {
                        AnnotationSelectionChanged?.Invoke(this, (annotationBindData).BindProperty);
                    }
                }
                catch { }
            }
        }

        public void CancelSelected()
        {
            AnnotationList.SelectedIndex = -1;
        }

        public void SelectAnnotationChanged(int annotationIndex = -1)
        {

            AnnotationList.SelectedIndex = annotationIndex;
        }

        public void SelectAnnotationChanged(int pageIIndex, int annotIndex)
        {
            if (annotationList != null && annotationList.Count > 0)
            {
                for (int i = 0; i < annotationList.Count; i++)
                {
                    AnnotationBindData data = annotationList[i];
                    if (data.BindProperty.PageIndex == pageIIndex && data.BindProperty.AnnotIndex == annotIndex)
                    {
                        enableSelectEvent = false;
                        AnnotationList.SelectedIndex = i;
                        enableSelectEvent = true;
                        break;
                    }
                }
            }
        }

        private void AnnotationList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (popContextMenu.Items[0] is MenuItem checkMenu)
                {
                    if (checkMenu != null)
                    {
                        checkMenu.IsEnabled = true;
                    }
                    if (AnnotationList != null && AnnotationList.SelectedIndex == -1)
                    {
                        checkMenu.IsEnabled = false;
                    }
                }  
            }
            catch (Exception ex)
            {

            }
        }

        private void AnnotationList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CancelSelected();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ReplyStatusChanged?.Invoke(sender, CPDFAnnotationState.C_ANNOTATION_MARKED);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ReplyStatusChanged?.Invoke(sender, CPDFAnnotationState.C_ANNOTATION_UNMARKED);
        }

        private void StatusControl_ReplyStatusChanged(object sender, CPDFAnnotationState e)
        {
            ReplyStatusChanged?.Invoke(sender, e);
        }
    }
}
