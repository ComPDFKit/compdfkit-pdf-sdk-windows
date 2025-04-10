using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Controls.Properties;
using ComPDFKit.PDFDocument;
using UserControl = System.Windows.Controls.UserControl;

namespace ComPDFKit.Controls.Comparison
{
    public partial class CompareOverwriteResultControl : UserControl
    {
        private double[] zoomLevel = { 1.00f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        private CPDFDocument CompareDoc { get; set; }
        public CPDFDocument LeftDoc { get; set; }
        public CPDFDocument RightDoc { get; set; }
        public event EventHandler ExitCompareEvent;
        public PDFViewControl pdfViewerCtrl { get; set; }
        private bool HasSaved { get; set; }
        public CompareOverwriteResultControl()
        {
            InitializeComponent();
        }
        
        private void CloseLeave()
        {
            if (pdfViewerCtrl != null)
            {
                ExitCompareEvent?.Invoke(null,null);
            }
        }
        private void Close_MouseLeftDown(object sender, RoutedEventArgs e)
        {
            CloseConfirmGrid.Visibility = Visibility.Visible;
        }
        public void LoadComparePdf(CPDFDocument leftDoc)
        {
            CompareDoc = leftDoc;
            OverwriteViewer.InitDocument(leftDoc);
            ScaleControl.InitWithPDFViewer(OverwriteViewer);
        }
        private void SaveCompareData()
        {
            CoverGrid.Visibility = Visibility.Visible;
            SaveConfirmBorder.Visibility = Visibility.Collapsed;
            if (CompareDoc != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files(*.pdf)|*.pdf;";
                saveFileDialog.FileName = /*App.MainPageLoader.GetString*/("EditPDF_ExportName");
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    CompareDoc.WriteFlattenToFilePath(saveFileDialog.FileName);
                    Process.Start(@"explorer.exe", "/select,\"" + saveFileDialog.FileName + "\"");
                }
                HasSaved = true;
            }
            CoverGrid.Visibility = Visibility.Collapsed;
        }
        private void Save_MouseLeftDown(object sender, RoutedEventArgs e)
        {
            SaveCompareData();
            //CloseLeave();
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveCompareData();
            CloseLeave();
        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseLeave();
        }
        
        private void ConfirmExitBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseConfirmGrid.Visibility = Visibility.Collapsed;
            CloseLeave();
        }

        private void CancelCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseConfirmGrid.Visibility = Visibility.Collapsed;
        }
        public void SetCompareColor(Brush Nbrush, Brush Obrush)
        {
            NewDocumentRect.Fill = Nbrush;
            OldDocumentRect.Fill = Obrush;
        }
    }
}