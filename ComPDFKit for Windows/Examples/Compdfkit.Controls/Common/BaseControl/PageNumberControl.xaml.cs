using ComPDFKit.Tool;
using ComPDFKit.Controls.PDFControl;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComPDFKit.Controls.Common
{
    public partial class PageNumberControl : UserControl
    {
        private PDFViewControl pdfView;

        public PageNumberControl()
        {
            InitializeComponent();
        }

        public void InitWithPDFViewer(PDFViewControl newPDFView)
        {
            if (pdfView != newPDFView)
            {
                if (pdfView != null)
                {
                    pdfView.DrawChanged -= PdfView_InfoChanged;
                }
                pdfView = newPDFView;
                if (pdfView != null)
                {
                    pdfView.DrawChanged -= PdfView_InfoChanged;
                    pdfView.FocusPDFViewToolChanged -= PdfView_FocusPDFViewToolChanged;
                    pdfView.DrawChanged += PdfView_InfoChanged;
                    pdfView.FocusPDFViewToolChanged += PdfView_FocusPDFViewToolChanged;
                }
            }
        }

        private void PdfView_FocusPDFViewToolChanged(object sender, EventArgs e)
        {
            if (pdfView.GetCPDFViewer().CurrentRenderFrame != null)
            {
                PageRangeText.Text = $"{pdfView.GetCPDFViewer().CurrentRenderFrame.PageIndex + 1}/{pdfView.GetCPDFViewer().GetDocument().PageCount}"; 
            }
        }

        private void PdfView_InfoChanged(object sender, EventArgs e)
        {
            if (sender is CPDFViewerTool)
            {
                PageRangeText.Text = string.Format("{0}/{1}", (sender as CPDFViewerTool).GetCPDFViewer().CurrentRenderFrame.PageIndex+1, (sender as CPDFViewerTool).GetCPDFViewer().GetDocument().PageCount);
            }
        }


        private void NextPageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetPageRangeVisible();
            if (pdfView.GetCPDFViewer().GetViewMode() == ComPDFKitViewer.ViewMode.Single || pdfView.GetCPDFViewer().GetViewMode() == ComPDFKitViewer.ViewMode.SingleContinuous)
            {
                pdfView.GetCPDFViewer().GoToPage(pdfView.GetCPDFViewer().CurrentRenderFrame.PageIndex + 1,new Point(0,0));
            }
            else
            {
                pdfView.GetCPDFViewer().GoToPage(pdfView.GetCPDFViewer().CurrentRenderFrame.PageIndex + 2, new Point(0, 0));
            }
        }

        private void PrevPageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetPageRangeVisible();
            if (pdfView.GetCPDFViewer().GetViewMode() == ComPDFKitViewer.ViewMode.Single || pdfView.GetCPDFViewer().GetViewMode() == ComPDFKitViewer.ViewMode.SingleContinuous)
            {
                pdfView.GetCPDFViewer().GoToPage(pdfView.GetCPDFViewer().CurrentRenderFrame.PageIndex - 1, new Point(0, 0));
            }
            else
            {
                pdfView.GetCPDFViewer().GoToPage(pdfView.GetCPDFViewer().CurrentRenderFrame.PageIndex - 2, new Point(0, 0));
            }
        }

        private void PageRangeText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PageRangeText.Visibility = Visibility.Collapsed;
            if(pdfView!=null && pdfView.GetCPDFViewer().GetDocument() != null)
            {
                PageInputText.Text =string.Format("{0}", pdfView.GetCPDFViewer().CurrentRenderFrame.PageIndex + 1);
            }
            PageInputText.Visibility = Visibility.Visible;
            PageInputText.Focus();
        }

        private void PageInputText_LostFocus(object sender, RoutedEventArgs e)
        {
            SetInputPage();
            SetPageRangeVisible();
        }

        private void PageInputText_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key== Key.Escape)
            {
                SetInputPage();
                SetPageRangeVisible();
            }
        }

        private void SetPageRangeVisible()
        {
            PageRangeText.Visibility = Visibility.Visible;
            PageInputText.Visibility = Visibility.Collapsed;
        }

        private void SetInputPage()
        {
            if (pdfView != null && pdfView.GetCPDFViewer().GetDocument() != null)
            {

                if (int.TryParse(PageInputText.Text, out int newPageNum))
                {
                    newPageNum = Math.Min(pdfView.GetCPDFViewer().GetDocument().PageCount, newPageNum);
                    newPageNum = Math.Max(1, newPageNum);
                    pdfView.GetCPDFViewer().GoToPage(newPageNum-1,new Point(0,0));
                }
            }
        }

        private void PageInputText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            List<Key> allowKeys = new List<Key>()
            { 
                Key.Delete, Key.Back, Key.Enter, Key.NumPad0,  Key.NumPad1, Key.NumPad2, Key.NumPad3,
                Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.D0,
                Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.Left, Key.Right
            };

            if(allowKeys.Contains(e.Key)==false)
            {
                e.Handled = true;
            }
        }

        private void PageInputText_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (e.Command == ApplicationCommands.Paste && Clipboard.ContainsText())
                {
                    string checkText=Clipboard.GetText();
                    if (int.TryParse(checkText, out int value))
                    {
                        e.CanExecute = true;
                    }
                    e.Handled=true;
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
