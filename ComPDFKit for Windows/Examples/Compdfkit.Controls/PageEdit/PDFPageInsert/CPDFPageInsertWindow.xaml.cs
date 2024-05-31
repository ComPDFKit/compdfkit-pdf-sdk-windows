using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControlUI;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFPageInsertWindow : Window
    {
        public InsertEventClass Result = new InsertEventClass();

        public delegate void DialogCloseEventHandler(object sender, InsertDialogCloseEventArgs e);
        public event DialogCloseEventHandler DialogClosed;

        public CPDFPageInsertWindow()
        {
            InitializeComponent();
            Title = LanguageHelper.DocEditorManager.GetString("Title_Insert");
        }

        public void InitPageInsertWindow(int index, int maxPage)
        {
            if (index == -1)
            {
                CPDFPageInsertUI.PageInsertLocation = PageInsertLocation.HomePage;
            }
            else
            {
                CPDFPageInsertUI.PageInsertLocation = PageInsertLocation.CustomPage;
                CPDFPageInsertUI.CustomPageIndex = index + 1;
                Result.InsertIndex = index + 1;
            }

            CPDFPageInsertUI.MaxIndex = maxPage;
        }

        private void CPDFPageInsertUI_Loaded(object sender, RoutedEventArgs e)
        {
            CPDFPageInsertUI.InsertTypeChanged -= CPDFPageInsertUI_InsertTypeChanged;
            CPDFPageInsertUI.SelectedFileChanged -= CPDFPageInsertUI_SelectedFileChanged;
            CPDFPageInsertUI.InsertIndexChanged -= CPDFPageInsertUI_InsertIndexChanged;
            CPDFPageInsertUI.TextChanged -= CPDFPageInsertUI_TextChanged;
            CPDFPageInsertUI.CancelEvent -= CPDFPageInsertUI_CancelEvent;
            CPDFPageInsertUI.InsertEvent -= CPDFPageInsertUI_InsertEvent;
            CPDFPageInsertUI.PageSizeChanged -= CPDFPageInsertUI_PageSizeChanged;
            CPDFPageInsertUI.PasswordChanged -= CPDFPageInsertUI_PasswordChanged;

            CPDFPageInsertUI.InsertTypeChanged += CPDFPageInsertUI_InsertTypeChanged;
            CPDFPageInsertUI.SelectedFileChanged += CPDFPageInsertUI_SelectedFileChanged;
            CPDFPageInsertUI.InsertIndexChanged += CPDFPageInsertUI_InsertIndexChanged;
            CPDFPageInsertUI.TextChanged += CPDFPageInsertUI_TextChanged;
            CPDFPageInsertUI.CancelEvent += CPDFPageInsertUI_CancelEvent;
            CPDFPageInsertUI.InsertEvent += CPDFPageInsertUI_InsertEvent;
            CPDFPageInsertUI.PageSizeChanged += CPDFPageInsertUI_PageSizeChanged;
            CPDFPageInsertUI.PasswordChanged += CPDFPageInsertUI_PasswordChanged;       
        }



        private void CPDFPageInsertUI_Unloaded(object sender, RoutedEventArgs e)
        {
            CPDFPageInsertUI.InsertTypeChanged -= CPDFPageInsertUI_InsertTypeChanged;
            CPDFPageInsertUI.SelectedFileChanged -= CPDFPageInsertUI_SelectedFileChanged;
            CPDFPageInsertUI.InsertIndexChanged -= CPDFPageInsertUI_InsertIndexChanged;
            CPDFPageInsertUI.TextChanged -= CPDFPageInsertUI_TextChanged;
            CPDFPageInsertUI.CancelEvent -= CPDFPageInsertUI_CancelEvent;
            CPDFPageInsertUI.InsertEvent -= CPDFPageInsertUI_InsertEvent;
            CPDFPageInsertUI.PageSizeChanged -= CPDFPageInsertUI_PageSizeChanged;
            CPDFPageInsertUI.PasswordChanged -= CPDFPageInsertUI_PasswordChanged;


        }

        private void CPDFPageInsertUI_PasswordChanged(object sender, string e)
        {
            Result.Password = e;
        }

        private void CPDFPageInsertUI_InsertEvent(object sender, EventArgs e)
        {
            if (Result.InsertType == InsertType.FromOtherPDF && Result.FilePath == string.Empty)
            {
                MessageBox.Show(LanguageHelper.DocEditorManager.GetString("Warn_NoFile"),LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<int> pageRangeList = new List<int>();
            if (Result.InsertType == InsertType.FromOtherPDF && !CommonHelper.GetPagesInRange(ref pageRangeList, Result.PageRange, CPDFPageInsertUI.MaxIndex, new char[] { ',' }, new char[] { '-' }))
            {
                MessageBox.Show(LanguageHelper.DocEditorManager.GetString("Warn_PageRange"), LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CloseWindow(Result);
        }

        private void CPDFPageInsertUI_CancelEvent(object sender, EventArgs e)
        {
            CloseWindow(null);
        }

        private void CPDFPageInsertUI_TextChanged(object sender, string e)
        {
            Result.PageRange = e;
        }

        private void CPDFPageInsertUI_SelectedFileChanged(object sender, string e)
        {
            Result.FilePath = e;
        }

        private void CPDFPageInsertUI_InsertTypeChanged(object sender, string e)
        {
            if (e == "BlankPages")
            {
                Result.InsertType = InsertType.BlankPages;
            }
            else if (e == "CustomBlankPages")
            {
                Result.InsertType = InsertType.CustomBlankPages;
            }
            else
            {
                Result.InsertType = InsertType.FromOtherPDF;
            }
        }

        private void CPDFPageInsertUI_InsertIndexChanged(object sender, int e)
        {
            Result.InsertIndex = e;
        }

        private void CloseWindow(InsertEventClass dialogResult)
        {
            DialogClosed?.Invoke(this, new InsertDialogCloseEventArgs(dialogResult));

            Close();
        }

        private void CPDFPageInsertUI_PageSizeChanged(object sender, double[] e)
        {
            Result.PageWidth = e[0];
            Result.PageHeight = e[1];
        }
    }
     
    public class InsertDialogCloseEventArgs : EventArgs
    {
        public InsertEventClass DialogResult { get; set; }

        public InsertDialogCloseEventArgs(InsertEventClass dialogResult)
        {
            DialogResult = dialogResult;
        }
    }
}
