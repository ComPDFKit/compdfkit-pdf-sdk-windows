using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.PDFControlUI;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFBookmarkControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private PDFViewControl ViewControl;

        public CPDFBookmarkControl()
        {
            InitializeComponent();
            Loaded += CPDFBookmarkControl_Loaded;
            Unloaded += CPDFBookmarkControl_Unloaded;
        }

        private void CPDFBookmarkControl_Loaded(object sender, RoutedEventArgs e)
        {
            BookmarkAddUI.BookmarkAddEvent -= BookmarkAddUI_BookmarkAddEvent;
            BookmarkAddUI.BookmarkInputExpandEvent -= BookmarkAddUI_BookmarkInputExpandEvent;
            BookmarkResultUI.SelectionChanged -= BookmarkResultUI_SelectionChanged;
            BookmarkResultUI.BookmarkDelete -= BookmarkResultUI_BookmarkDelete;
            BookmarkResultUI.BookmarkClicked -= BookmarkResultUI_BookmarkClicked;
            BookmarkResultUI.BookmarkEdit -= BookmarkResultUI_BookmarkEdit;

            BookmarkAddUI.BookmarkAddEvent += BookmarkAddUI_BookmarkAddEvent;
            BookmarkAddUI.BookmarkInputExpandEvent += BookmarkAddUI_BookmarkInputExpandEvent;
            BookmarkResultUI.SelectionChanged += BookmarkResultUI_SelectionChanged;
            BookmarkResultUI.BookmarkDelete += BookmarkResultUI_BookmarkDelete;
            BookmarkResultUI.BookmarkClicked += BookmarkResultUI_BookmarkClicked;
            BookmarkResultUI.BookmarkEdit += BookmarkResultUI_BookmarkEdit;
        }
        
        private void CPDFBookmarkControl_Unloaded(object sender, RoutedEventArgs e)
        {
            BookmarkAddUI.BookmarkAddEvent -= BookmarkAddUI_BookmarkAddEvent;
            BookmarkAddUI.BookmarkInputExpandEvent -= BookmarkAddUI_BookmarkInputExpandEvent;
            BookmarkResultUI.SelectionChanged -= BookmarkResultUI_SelectionChanged;
            BookmarkResultUI.BookmarkDelete -= BookmarkResultUI_BookmarkDelete;
            BookmarkResultUI.BookmarkClicked -= BookmarkResultUI_BookmarkClicked;
            BookmarkResultUI.BookmarkEdit -= BookmarkResultUI_BookmarkEdit;
        }

        private void BookmarkResultUI_BookmarkEdit(object sender, BookmarkChangeData e)
        {
            BookmarkAddUI.HideInputUI(false);
            BookmarkAddUI.SetBookmarkChangeData(e);
        }

        private void BookmarkAddUI_BookmarkInputExpandEvent(object sender, EventArgs e)
        {
            if(ViewControl!=null && ViewControl.PDFViewTool!=null)
            {
                CPDFViewer pdfViewer=ViewControl.PDFViewTool.GetCPDFViewer();
                if(pdfViewer!=null && pdfViewer.CurrentRenderFrame!=null)
                {
                    BookmarkAddUI.SetBookmarkChangeData(new BookmarkChangeData()
                    {
                        PageIndex = pdfViewer.CurrentRenderFrame.PageIndex,
                    });
                }
            }
        }

        private void BookmarkResultUI_BookmarkClicked(object sender, int e)
        {
            GotoBookmarkPage(e);
        }

        private void BookmarkResultUI_BookmarkDelete(object sender, BookmarkChangeData e)
        {
            if(ViewControl!=null && ViewControl.PDFViewTool!=null)
            {
                CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfViewer?.GetDocument();
                if(pdfDoc!=null)
                {
                    pdfDoc.RemoveBookmark(e.PageIndex);
                }
            }
        }

        private void BookmarkResultUI_SelectionChanged(object sender, int e)
        {
            if (e >= 0)
            {
                BindBookmarkResult bindResult = BookmarkResultUI.GetItem(e);
                if (bindResult != null)
                {
                    GotoBookmarkPage(bindResult.PageIndex);
                }
            }
        }

        private void BookmarkAddUI_BookmarkAddEvent(object sender, BookmarkChangeData newData)
        {
            CPDFDocument pdfDoc = null;
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                 pdfDoc = pdfViewer?.GetDocument();
              
            }
            if (pdfDoc == null || newData == null)
            {
                return;
            }
            if (newData.PageIndex >= 0 && newData.PageIndex < pdfDoc.PageCount)
            {
                if (string.IsNullOrEmpty(newData.NewTitle) == false && string.IsNullOrEmpty(newData.BookmarkTitle))
                {
                    bool addState = pdfDoc.AddBookmark(new CPDFBookmark()
                    {
                        PageIndex = newData.PageIndex,
                        Title = newData.NewTitle
                    });
                    if (addState)
                    {
                        LoadBookmark();
                    }
                    else
                    {
                        MessageBox.Show("Bookmark existed, add failed");
                    }
                    return;
                }

                if (!string.IsNullOrEmpty(newData.NewTitle) && !string.IsNullOrEmpty(newData.BookmarkTitle))
                {
                    pdfDoc.EditBookmark(newData.PageIndex, newData.NewTitle);
                    BookmarkBindData bindUiData = newData.BindData as BookmarkBindData;
                    if (bindUiData != null)
                    {
                        bindUiData.BindProperty.BookmarkTitle = newData.NewTitle;
                    }
                }
            }
        }

        private void GotoBookmarkPage(int pageIndex)
        {
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                if (pageIndex >= 0)
                {
                    pdfViewer.GoToPage(pageIndex,new Point(0,0));
                }
            }
        }

        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            ViewControl = viewControl;
        }

        public void LoadBookmark()
        {
            CPDFDocument pdfDoc = null;
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                pdfDoc = pdfViewer?.GetDocument();

            }
            if (pdfDoc == null)
            {
                return;
            }

            List<CPDFBookmark> bookmarkList = pdfDoc.GetBookmarkList();
            List<BindBookmarkResult> bindBookmarkList = new List<BindBookmarkResult>();
            if (bookmarkList != null && bookmarkList.Count>0)
            {
               
                foreach (CPDFBookmark bookmark in bookmarkList.AsEnumerable().OrderBy(x=>x.PageIndex))
                {
                    bindBookmarkList.Add(new BindBookmarkResult()
                    {
                        PageIndex=bookmark.PageIndex,
                        BookmarkTitle=bookmark.Title
                    });
                }
               
            }
            BookmarkResultUI?.SetBookmarkResult(bindBookmarkList);
        }
    }
}
