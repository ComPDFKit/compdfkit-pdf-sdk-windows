using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using ComPDFKitViewer;
using ComPDFKitViewer.BaseObject;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static ComPDFKit.Controls.PDFControlUI.CPDFAnnotationListUI;
using ComPDFKit.Controls.PDFControlUI;
using System.Windows.Input;
using System;
using System.Linq;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFAnnotationListControl : UserControl
    {
        private List<C_ANNOTATION_TYPE> OmitList = new List<C_ANNOTATION_TYPE>
        {
            C_ANNOTATION_TYPE.C_ANNOTATION_UNKOWN,
            C_ANNOTATION_TYPE.C_ANNOTATION_LINK,
            C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET,
            C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE,
            C_ANNOTATION_TYPE.C_ANNOTATION_SOUND,
            C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA
        };

        /// <summary>
        /// PDFViewer
        /// </summary>
        private PDFViewControl pdfViewer;

        public CPDFAnnotationListControl()
        {
            InitializeComponent();
            Loaded += CPDFAnnotationListControl_Loaded;
        }

        private void CPDFAnnotationListControl_Loaded(object sender, RoutedEventArgs e)
        {
            AnnotationList.DeleteItemHandler -= AnnotationList_DeleteItemHandler;
            AnnotationList.DeleteItemHandler += AnnotationList_DeleteItemHandler;
            AnnotationList.ReplyStatusChanged -= AnnotationList_ReplyStatusChanged;
            AnnotationList.ReplyStatusChanged += AnnotationList_ReplyStatusChanged;
            AnnotationReplyListControl.ReplyListChanged -= AnnotationReplyListControl_ReplyListChanged;
            AnnotationReplyListControl.ReplyListChanged += AnnotationReplyListControl_ReplyListChanged;
        }

        private void AnnotationList_ReplyStatusChanged(object sender, CPDFAnnotationState e)
        {
            if (sender is ReplyStatusControl replyStatusControl)
            {
                if (replyStatusControl.DataContext is AnnotationBindData data)
                {
                    if (pdfViewer != null)
                    {
                        CPDFAnnotation annot = data.BindProperty.Annotation;
                        if (annot != null)
                        {
                            annot.SetState(e);
                            pdfViewer.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
                            pdfViewer.PDFViewTool.IsDocumentModified = true;
                        }
                    }
                }
            }
            else if (sender is CheckBox checkBox)
            {
                if (checkBox.DataContext is AnnotationBindData data)
                {
                    if (pdfViewer != null)
                    {
                        CPDFAnnotation annot = data.BindProperty.Annotation;
                        if (annot != null)
                        {
                            annot.SetMarkedAnnotState(checkBox.IsChecked == true ? CPDFAnnotationState.C_ANNOTATION_MARKED : CPDFAnnotationState.C_ANNOTATION_UNMARKED, "");
                            pdfViewer.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
                            pdfViewer.PDFViewTool.IsDocumentModified = true;
                        }
                    }
                }
            }
        }

        private void AnnotationReplyListControl_ReplyListChanged(object sender, System.EventArgs e)
        {
            pdfViewer.PDFViewTool.IsDocumentModified = true;
            pdfViewer.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
        }

        private void AnnotationList_DeleteItemHandler(object sender, Dictionary<int, List<int>> e)
        {
            if (pdfViewer != null)
            {
                pdfViewer.PDFToolManager.ClearSelect();
                ParamConverter.RemovePageAnnot(e, pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument());
                pdfViewer.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
                LoadAnnotationList();
            }
        }

        public void InitWithPDFViewer(PDFViewControl newPDFView)
        {
            pdfViewer = newPDFView;
            pdfViewer.PDFToolManager.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            pdfViewer.PDFToolManager.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;
        }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, ComPDFKit.Tool.MouseEventObject e)
        {
            if (OmitList.Contains(e.annotType))
            {
                AnnotationList.SelectAnnotationChanged(-1);
            }
            else
            {
                BaseAnnot baseAnnot = pdfViewer.PDFToolManager.GetCacheHitTestAnnot();
                AnnotData annotData = baseAnnot?.GetAnnotData();
                if (annotData != null)
                {
                    AnnotationList.SelectAnnotationChanged(annotData.PageIndex, annotData.AnnotIndex);
                }
            }
        }

        public void LoadAnnotationList()
        {
            if (pdfViewer == null || pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() == null)
            {
                return;
            }

            if (pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument().IsLocked)
            {
                return;
            }
            int pageCount = pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument().PageCount;

            List<BindAnnotationResult> bindAnnotationResults = new List<BindAnnotationResult>();

            pdfViewer.UpdateAnnotFrame();
            for (int i = 0; i < pageCount; i++)
            {
                List<AnnotParam> annotList = GetAnnotCommentList(i, pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument());
                List<CPDFAnnotation> annotCoreList = pdfViewer?.GetCPDFViewer()?.GetDocument()?.PageAtIndex(i, false)?.GetAnnotations();
                if (annotList != null && annotList.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (AnnotParam annot in annotList)
                        {
                            CPDFAnnotation annotCore = annotCoreList[annot.AnnotIndex];
                            if (annotCore == null || annotCore.IsReplyAnnot() || annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINK)
                            {
                                continue;
                            }

                            var bindResult = new BindAnnotationResult
                            {
                                PageIndex = i,
                                annotationData = annot,
                                pdfViewer = pdfViewer,
                                ReplyState = annotCore.GetState(),
                                IsMarkState = annotCore.IsMarkedStateAnnot()
                            };

                            List<CPDFTextAnnotation> replyAnnotations = annotCore?.GetReplies();
                            if (replyAnnotations != null && replyAnnotations.Count > 0)
                            { 
                                foreach (CPDFTextAnnotation replyAnnot in replyAnnotations)
                                {
                                    if (replyAnnot == null || replyAnnot.IsMarkedStateAnnot())
                                    {
                                        continue;
                                    }

                                    bindResult.ReplyList.Add(new ReplyData
                                    {
                                        ReplyAnnotation = replyAnnot,
                                    });
                                }
                            }
                            bindAnnotationResults.Add(bindResult);
                        }
                    });
                }
            }
            AnnotationList.SetAnnotationList(bindAnnotationResults);
        }

        public List<AnnotParam> GetAnnotCommentList(int pageIndex, CPDFDocument currentDoc)
        {
            List<AnnotParam> annotList = new List<AnnotParam>();

            if (pageIndex < 0 || pdfViewer == null || currentDoc == null)
            {
                return annotList;
            }
            CPDFPage docPage = currentDoc.PageAtIndex(pageIndex, false);
            if (docPage == null)
            {
                return annotList;
            }
            List<CPDFAnnotation> docAnnots = docPage.GetAnnotations();
            if (docAnnots != null && docAnnots.Count > 0)
            {
                foreach (CPDFAnnotation annotation in docAnnots)
                {
                    if (annotation == null || OmitList.Contains(annotation.Type))
                    {
                        continue;
                    }
                    AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(currentDoc, pageIndex, annotation);
                    if (annotParam != null)
                    {
                        annotList.Add(annotParam);
                    }
                }
            }
            return annotList;
        }

        private void AnnotationList_Loaded(object sender, RoutedEventArgs e)
        {
            AnnotationList.AnnotationSelectionChanged -= AnnotationList_AnnotationSelectionChanged;
            AnnotationList.AnnotationSelectionChanged += AnnotationList_AnnotationSelectionChanged;
        }

        private void AnnotationList_Unloaded(object sender, RoutedEventArgs e)
        {
            AnnotationList.AnnotationSelectionChanged -= AnnotationList_AnnotationSelectionChanged;
        }

        private void AnnotationList_AnnotationSelectionChanged(object sender, object e)
        {
            var bindAnnotationResult = e as BindAnnotationResult;
            pdfViewer.PDFViewTool.GetCPDFViewer().GoToPage(bindAnnotationResult.PageIndex, new Point(bindAnnotationResult.annotationData.ClientRect.left, bindAnnotationResult.annotationData.ClientRect.top));
            pdfViewer.PDFViewTool.GetCPDFViewer().UpdateRenderFrame();
            pdfViewer.PDFViewTool.SelectedAnnotForIndex(bindAnnotationResult.PageIndex, bindAnnotationResult.AnnotIndex);
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = Helper.CommonHelper.GetExistedPathOrEmpty("XFDF Files (*.xfdf)|*.xfdf");
            if (string.IsNullOrEmpty(selectedPath)) return;
            var tempPath = Path.Combine(Path.GetDirectoryName(selectedPath), "temp");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            pdfViewer.PDFToolManager.GetDocument().ImportAnnotationFromXFDFPath(selectedPath, tempPath);

            LoadAnnotationList();
            pdfViewer.PDFViewTool.GetCPDFViewer().UpdateVirtualNodes();
            pdfViewer.PDFViewTool.GetCPDFViewer().UpdateRenderFrame();
            //pdfViewer.UndoManager.CanSave = true;
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = Helper.CommonHelper.GetGeneratePathOrEmpty("XFDF Files (*.xfdf)|*.xfdf", pdfViewer.PDFToolManager.GetDocument().FileName);
            if (string.IsNullOrEmpty(selectedPath)) return;
            var tempPath = Path.Combine(Path.GetDirectoryName(selectedPath), "temp");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            if (pdfViewer.PDFToolManager.GetDocument().ExportAnnotationToXFDFPath(selectedPath, tempPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + selectedPath);
            }
        }
    }

    public class ExpandAllReplyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CPDFAnnotationListControl annotationListControl)
            {
                annotationListControl.AnnotationList.ExpandAllReply(true);
            }
        }
    }

    public class FoldAllReplyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CPDFAnnotationListControl annotationListControl)
            {
                annotationListControl.AnnotationList.ExpandAllReply(false);
            }
        }
    }

    public class DeleteAllAnnotCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CPDFAnnotationListControl annotationListControl)
            {
                annotationListControl.AnnotationList.DeleteAllReply();
                annotationListControl.LoadAnnotationList();
                annotationListControl.AnnotationList.DeleteAllAnnot();
            }
        }
    }

    public class DeleteAllReplyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CPDFAnnotationListControl annotationListControl)
            {
                annotationListControl.AnnotationList.DeleteAllReply();
                annotationListControl.LoadAnnotationList();
            }
        }
    }

    public class ExpandAnnotListCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CPDFAnnotationListControl annotationListControl)
            {
                annotationListControl.AnnotationList.ExpandAnnotList(true);
            }
        }
    }

    public class FoldAnnotListCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CPDFAnnotationListControl annotationListControl)
            {
                annotationListControl.AnnotationList.ExpandAnnotList(false);
            }
        }
    }

}
