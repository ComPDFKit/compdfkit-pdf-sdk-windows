using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Compdfkit_Tools.Helper;
using static Compdfkit_Tools.PDFControlUI.CPDFAnnoationListUI;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFAnnotationListControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private CPDFViewer pdfViewer;

        public CPDFAnnotationListControl()
        {
            InitializeComponent();
            Loaded += CPDFAnnotationListControl_Loaded;
        }

        private void CPDFAnnotationListControl_Loaded(object sender, RoutedEventArgs e)
        {
            AnnotationList.DeleteItemHandler -= AnnotationList_DeleteItemHandler;
            AnnotationList.DeleteItemHandler += AnnotationList_DeleteItemHandler;
        }

        private void AnnotationList_DeleteItemHandler(object sender, Dictionary<int, List<int>> e)
        {
           if(pdfViewer!=null)
            {
                pdfViewer.ClearSelectAnnots();
                pdfViewer.RemovePageAnnot(e);
                LoadAnnotationList();
            }
        }

        public void InitWithPDFViewer(CPDFViewer newPDFView)
        {
            pdfViewer = newPDFView;
            pdfViewer.AnnotActiveHandler -= PdfViewer_AnnotActiveHandler;
            pdfViewer.AnnotActiveHandler += PdfViewer_AnnotActiveHandler;

            
        }

        private void PdfViewer_AnnotActiveHandler(object sender, AnnotAttribEvent e)
        {
            if (e != null)
            {
                //TODO SomeThing Need Change
              int a =  e.GetAnnotHandlerEventArgs(e.GetAnnotTypes())[0].AnnotIndex;
              int b =  e.GetAnnotHandlerEventArgs(e.GetAnnotTypes())[0].PageIndex;
                AnnotationList.SelectAnnotationChanged(b,a);
            }
            else
            {
                AnnotationList.SelectAnnotationChanged(-1);
            }
        }

        public void LoadAnnotationList()
        {
            if (pdfViewer == null || pdfViewer.Document == null)
            {
                return;
            }

            if (pdfViewer.Document.IsLocked)
            {
                return;
            }
            int pageCount = pdfViewer.Document.PageCount;

            List<BindAnnotationResult> bindAnnotationResults = new List<BindAnnotationResult>();

            for (int i = 0; i < pageCount; i++)
            {
                List<AnnotHandlerEventArgs> annotList = pdfViewer.GetAnnotCommentList(i, pdfViewer.Document);
                if (annotList != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (AnnotHandlerEventArgs annot in annotList)
                        {
                            bindAnnotationResults.Add(new BindAnnotationResult
                            {
                                PageIndex = i,
                                annotationData = annot
                            });
                        }
                    });
                }
            }
            AnnotationList.SetAnnotationList(bindAnnotationResults);
        }

        private void AnnotationList_Loaded(object sender, RoutedEventArgs e)
        {
            AnnotationList.AnnotationSelectionChanged += AnnotationList_AnnotationSelectionChanged;
        }

        private void AnnotationList_Unloaded(object sender, RoutedEventArgs e)
        {
            AnnotationList.AnnotationSelectionChanged -= AnnotationList_AnnotationSelectionChanged;
        }

        private void AnnotationList_AnnotationSelectionChanged(object sender, object e)
        {
            var bindAnnotationResult = e as BindAnnotationResult;
            pdfViewer.SelectAnnotation(bindAnnotationResult.PageIndex, bindAnnotationResult.AnnotIndex);
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = CommonHelper.GetExistedPathOrEmpty("XFDF Files (*.xfdf)|*.xfdf");
            if (string.IsNullOrEmpty(selectedPath)) return;
            var tempPath = Path.Combine(Path.GetDirectoryName(selectedPath), "temp");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            pdfViewer.Document.ImportAnnotationFromXFDFPath(selectedPath,tempPath);
                
            LoadAnnotationList();
            pdfViewer.ReloadDocument();
            pdfViewer.UndoManager.CanSave = true;
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = CommonHelper.GetGeneratePathOrEmpty("XFDF Files (*.xfdf)|*.xfdf",pdfViewer.Document.FileName);
            if (string.IsNullOrEmpty(selectedPath)) return;
            var tempPath = Path.Combine(Path.GetDirectoryName(selectedPath), "temp");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            if (pdfViewer.Document.ExportAnnotationToXFDFPath(selectedPath, tempPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + selectedPath);
            }
        }
    }
}
