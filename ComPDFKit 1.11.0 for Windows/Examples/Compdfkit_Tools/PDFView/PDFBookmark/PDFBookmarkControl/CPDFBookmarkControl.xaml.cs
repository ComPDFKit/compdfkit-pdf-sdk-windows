using ComPDFKit.PDFDocument;
using Compdfkit_Tools.PDFControlUI;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFBookmarkControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private CPDFViewer pdfView;

        public CPDFBookmarkControl()
        {
            InitializeComponent();
            Loaded += CPDFBookmarkControl_Loaded;
            Unloaded += CPDFBookmarkControl_Unloaded;
        }

        private void CPDFBookmarkControl_Loaded(object sender, RoutedEventArgs e)
        {
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
            if(pdfView!=null)
            {
                BookmarkAddUI.SetBookmarkChangeData(new BookmarkChangeData()
                {
                    PageIndex=pdfView.CurrentIndex
                });
            }
        }

        private void BookmarkResultUI_BookmarkClicked(object sender, int e)
        {
            GotoBookmarkPage(e);
        }

        private void BookmarkResultUI_BookmarkDelete(object sender, BookmarkChangeData e)
        {
            if (pdfView == null || pdfView.Document == null)
            {
                return;
            }

            pdfView.Document.RemoveBookmark(e.PageIndex);
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
            if (pdfView == null || pdfView.Document == null || newData==null)
            {
                return;
            }

            if (newData.PageIndex >= 0 && newData.PageIndex < pdfView.Document.PageCount)
            {
                if (string.IsNullOrEmpty(newData.NewTitle) == false && string.IsNullOrEmpty(newData.BookmarkTitle))
                {
                    bool addState = pdfView.Document.AddBookmark(new CPDFBookmark()
                    {
                        PageIndex = newData.PageIndex,
                        Title = newData.NewTitle
                    });
                    if (addState)
                    {
                        LoadBookmark();
                        pdfView.UndoManager.CanSave = true;
                    }
                    else
                    {
                        MessageBox.Show("Bookmark existed, add failed");
                    }
                    return;
                }

                if (!string.IsNullOrEmpty(newData.NewTitle) && !string.IsNullOrEmpty(newData.BookmarkTitle))
                {
                    pdfView.Document.EditBookmark(newData.PageIndex, newData.NewTitle);
                    BookmarkBindData bindUiData = newData.BindData as BookmarkBindData;
                    if (bindUiData != null)
                    {
                        bindUiData.BindProperty.BookmarkTitle = newData.NewTitle;
                        pdfView.UndoManager.CanSave = true;
                    }
                }
            }
        }

        private void GotoBookmarkPage(int pageIndex)
        {
            if (pdfView == null || pdfView.Document == null)
            {
                return;
            }

            if (pageIndex >= 0)
            {
                pdfView.GoToPage(pageIndex);
            }
        }

        public void InitWithPDFViewer(CPDFViewer newPDFView)
        {
            pdfView = newPDFView;
        }

        public void LoadBookmark()
        {
            if (pdfView == null || pdfView.Document == null)
            {
                return;
            }

            List<CPDFBookmark> bookmarkList = pdfView.Document.GetBookmarkList();
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
