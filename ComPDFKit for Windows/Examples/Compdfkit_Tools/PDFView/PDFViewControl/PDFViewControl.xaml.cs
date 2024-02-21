using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFPage;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Compdfkit_Tools.PDFControl
{
    public partial class PDFViewControl : UserControl
    {
        #region Properties
        public CPDFViewer PDFView { get; set; }

        public bool CustomSignHandle { get; set; }

        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        #endregion
        public PDFViewControl()
        {
            InitializeComponent();
            PDFView = new CPDFViewer();
            Content = PDFView;
            PDFView.MouseWheelZoomHandler += PDFView_MouseWheelZoomHandler;
            PDFView.PDFActionHandler += PDFView_PDFActionHandler;
        }

        #region Private Command Methods
        private void PDFView_PDFActionHandler(object sender, CPDFAction pdfAction)
        {
            if (pdfAction != null)
            {
                switch (pdfAction.ActionType)
                {
                    case C_ACTION_TYPE.ACTION_TYPE_NAMED:
                        {
                            CPDFNamedAction namedAction = pdfAction as CPDFNamedAction;
                            string namedStr = namedAction.GetName();
                            switch (namedStr)
                            {
                                case "FirstPage":
                                    {
                                        PDFView?.GoToPage(0);
                                        break;
                                    }
                                case "LastPage":
                                    {
                                        PDFView?.GoToPage(PDFView.Document.PageCount - 1);
                                        break;
                                    }
                                case "NextPage":
                                    if (PDFView != null)
                                    {
                                        int nextIndex = PDFView.CurrentIndex + 1;
                                        if (nextIndex < PDFView.Document.PageCount)
                                        {
                                            PDFView.GoToPage(nextIndex);
                                        }
                                    }
                                    break;
                                case "PrevPage":
                                    if (PDFView != null)
                                    {
                                        int prevIndex = PDFView.CurrentIndex - 1;
                                        if (prevIndex >= 0)
                                        {
                                            PDFView.GoToPage(prevIndex);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }

                    case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                        if (PDFView != null)
                        {
                            CPDFGoToAction gotoAction = pdfAction as CPDFGoToAction;
                            CPDFDestination dest = gotoAction.GetDestination(PDFView.Document);
                            if (dest != null)
                                PDFView.GoToPage(dest.PageIndex, new Point(0, 0));
                        }
                        break;
                    case C_ACTION_TYPE.ACTION_TYPE_GOTOR:
                        if (PDFView != null)
                        {
                            CPDFGoToRAction gotorAction = pdfAction as CPDFGoToRAction;
                            CPDFDestination dest = gotorAction.GetDestination(PDFView.Document);
                            if (dest != null)
                            {
                                PDFView.GoToPage(dest.PageIndex, new Point(0, 0));
                            }
                        }
                        break;

                    case C_ACTION_TYPE.ACTION_TYPE_URI:
                        {
                            CPDFUriAction uriAction = pdfAction as CPDFUriAction;
                            string uri = uriAction.GetUri();
                            try
                            {
                                if (!string.IsNullOrEmpty(uri))
                                {
                                    Process.Start(uri);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        break;

                    default:
                        break;
                }
            }


        }

        private void PDFView_MouseWheelZoomHandler(object sender, bool e)
        {
            double newZoom = CheckZoomLevel(PDFView.ZoomFactor + (e ? 0.01 : -0.01), e);
            PDFView.Zoom(newZoom);
        }

        private double CheckZoomLevel(double zoom, bool IsGrowth)
        {
            double standardZoom = 100;
            if (zoom <= 0.01)
            {
                return 0.01;
            }
            if (zoom >= 10)
            {
                return 10;
            }

            zoom *= 100;
            for (int i = 0; i < zoomLevelList.Length - 1; i++)
            {
                if (zoom > zoomLevelList[i] && zoom <= zoomLevelList[i + 1] && IsGrowth)
                {
                    standardZoom = zoomLevelList[i + 1];
                    break;
                }
                if (zoom >= zoomLevelList[i] && zoom < zoomLevelList[i + 1] && !IsGrowth)
                {
                    standardZoom = zoomLevelList[i];
                    break;
                }
            }
            return standardZoom / 100;
        }
        #endregion

        #region Public Methods
        public bool CheckHasForm()
        {
            if (PDFView == null || PDFView.Document == null)
                return false;

            var document = PDFView.Document;
            for (int i = 0; i < document.PageCount; i++)
            {
                CPDFPage page = document.PageAtIndex(i, false);
                List<CPDFAnnotation> annotList = page.GetAnnotations();
                if (annotList == null || annotList.Count < 1)
                    continue;

                List<CPDFWidget> formList = annotList.AsEnumerable().
                    Where(x => x.Type == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                    .Cast<CPDFWidget>()
                    .ToList();
                if (formList.Count > 0)
                    return true;
            }
            return false;
        }
        #endregion
        
    }
}
