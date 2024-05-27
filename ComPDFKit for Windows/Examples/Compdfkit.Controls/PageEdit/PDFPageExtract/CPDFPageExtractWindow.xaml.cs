using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControlUI;
using ComPDFKit.Controls.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFPageExtractWindow : Window
    {
        private string pageRangeString = string.Empty;
        private List<int> pageRangeList = new List<int>();


        public ExtractEventClass extractEventClass = new ExtractEventClass();

        public delegate void DialogCloseEventHandler(object sender, ExtractDialogCloseEventArgs e);
        public event DialogCloseEventHandler DialogClosed;

        public CPDFPageExtractWindow()
        {
            InitializeComponent();
            Title = LanguageHelper.DocEditorManager.GetString("Title_Extract");
        }
          
        public void InitPageExtractWindow(string customPageRange, int maxPage)
        {
            if (customPageRange == string.Empty)
            {
                CPDFPageExtractUI.PageRange = PageRange.AllPages;
                extractEventClass.PageMode = 1;
            }
            else
            {
                CPDFPageExtractUI.PageRange = PageRange.CustomPages;
                extractEventClass.PageMode = 4;
                pageRangeString = customPageRange;
                CommonHelper.GetPagesInRange(ref pageRangeList, pageRangeString, CPDFPageExtractUI.MaxIndex, new char[] { ',' }, new char[] { '-' });
                for (int i = 0; i < pageRangeList.Count; i++)
                {
                    pageRangeList[i]++;
                }
            }
            CPDFPageExtractUI.MaxIndex = maxPage;
            CPDFPageExtractUI.CurrentPageRange = customPageRange;
        }

        private void CPDFPageExtractUI_Loaded(object sender, RoutedEventArgs e)
        {
            CPDFPageExtractUI.PageRangeChanged -= CPDFPageExtractUI_PageRangeChanged;
            CPDFPageExtractUI.CustomPageRangeChanged -= CPDFPageExtractUI_CustomPageRangeChanged;
            CPDFPageExtractUI.SeparateChanged -= CPDFPageExtractUI_SeparateChanged;
            CPDFPageExtractUI.DeleteChanged -= CPDFPageExtractUI_DeleteChanged;
            CPDFPageExtractUI.ExtractEvent -= CPDFPageExtractUI_ExtractEvent;
            CPDFPageExtractUI.CancelEvent -= CPDFPageExtractUI_CancelEvent;

            CPDFPageExtractUI.PageRangeChanged += CPDFPageExtractUI_PageRangeChanged;
            CPDFPageExtractUI.CustomPageRangeChanged += CPDFPageExtractUI_CustomPageRangeChanged;
            CPDFPageExtractUI.SeparateChanged += CPDFPageExtractUI_SeparateChanged;
            CPDFPageExtractUI.DeleteChanged += CPDFPageExtractUI_DeleteChanged;
            CPDFPageExtractUI.ExtractEvent += CPDFPageExtractUI_ExtractEvent;
            CPDFPageExtractUI.CancelEvent += CPDFPageExtractUI_CancelEvent;
        }

        private void CPDFPageExtractUI_CancelEvent(object sender, EventArgs e)
        {
            CloseWindow(null);
        }

        private void CPDFPageExtractUI_ExtractEvent(object sender, EventArgs e)
        {
            if (extractEventClass.PageMode != 4)
            {
                if (extractEventClass.PageMode == 1 && extractEventClass.DeleteAfterExtract)
                {
                    MessageBox.Show(" Please keep at least one page.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (extractEventClass.PageMode == 3 && CPDFPageExtractUI.MaxIndex == 1)
                {
                    MessageBox.Show(" No page will be extracted.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                CloseWindow(extractEventClass);
            }
            else if(extractEventClass.PageMode == 4 && CommonHelper.GetPagesInRange(ref pageRangeList, pageRangeString, CPDFPageExtractUI.MaxIndex, new char[] { ',' }, new char[] { '-' }))
            {
                if (pageRangeList.Count == CPDFPageExtractUI.MaxIndex && extractEventClass.DeleteAfterExtract)
                {
                    MessageBox.Show(" Please keep at least one page.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                extractEventClass.PageName = pageRangeString;
                for (int i = 0; i < pageRangeList.Count; i++)
                {
                    pageRangeList[i]++;
                }
                extractEventClass.PageParm = pageRangeList;
                CloseWindow(extractEventClass);
            }
            else
            {
                MessageBox.Show(LanguageHelper.DocEditorManager.GetString("Warn_PageRange"), 
                    LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CPDFPageExtractUI_CustomPageRangeChanged(object sender, string e)
        {
            pageRangeString = e;
        }

        private void CPDFPageExtractUI_DeleteChanged(object sender, bool e)
        {
            extractEventClass.DeleteAfterExtract = e;
        }

        private void CPDFPageExtractUI_SeparateChanged(object sender, bool e)
        {
            extractEventClass.ExtractToSingleFile = e;
        }

        private void CPDFPageExtractUI_PageRangeChanged(object sender, PageRange e)
        {
            extractEventClass.PageMode = (int)e;
        }

        private void CPDFPageExtractUI_Unloaded(object sender, RoutedEventArgs e)
        {
            CPDFPageExtractUI.PageRangeChanged -= CPDFPageExtractUI_PageRangeChanged;
            CPDFPageExtractUI.CustomPageRangeChanged -= CPDFPageExtractUI_CustomPageRangeChanged;
            CPDFPageExtractUI.SeparateChanged -= CPDFPageExtractUI_SeparateChanged;
            CPDFPageExtractUI.DeleteChanged -= CPDFPageExtractUI_DeleteChanged;
            CPDFPageExtractUI.ExtractEvent -= CPDFPageExtractUI_ExtractEvent;
            CPDFPageExtractUI.CancelEvent -= CPDFPageExtractUI_CancelEvent;
        }

        private void CloseWindow(ExtractEventClass dialogResult)
        {
            DialogClosed?.Invoke(this, new ExtractDialogCloseEventArgs(dialogResult));

            Close();
        }
    }

    public class ExtractDialogCloseEventArgs : EventArgs
    {
        public ExtractEventClass DialogResult { get; set; }

        public ExtractDialogCloseEventArgs(ExtractEventClass dialogResult)
        {
            DialogResult = dialogResult;
        }
    }
}
