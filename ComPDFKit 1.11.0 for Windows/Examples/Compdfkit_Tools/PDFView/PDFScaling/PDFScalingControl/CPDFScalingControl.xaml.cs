using Compdfkit_Tools.Helper;
using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFScalingControl : UserControl
    {
        public CPDFViewer pdfViewer;

        public CPDFScalingControl()
        {
            InitializeComponent();
        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            this.pdfViewer = pdfViewer;
        }

        private void PDFScalingControl_Loaded(object sender, RoutedEventArgs e)
        {
            CPDFScalingUI.SetScaleEvent += PDFScalingControl_SetScaleEvent;
            CPDFScalingUI.ScaleIncreaseEvent += PDFScalingControl_ScaleIncreaseEvent;
            CPDFScalingUI.ScaleDecreaseEvent += PDFScalingControl_ScaleDecreaseEvent;
            CPDFScalingUI.SetPresetScaleEvent += CPDFScalingUI_SetPresetScaleEvent;
        }

        private void CPDFScalingUI_Unloaded(object sender, RoutedEventArgs e)
        {
            CPDFScalingUI.SetScaleEvent -= PDFScalingControl_SetScaleEvent;
            CPDFScalingUI.ScaleIncreaseEvent -= PDFScalingControl_ScaleIncreaseEvent;
            CPDFScalingUI.ScaleDecreaseEvent -= PDFScalingControl_ScaleDecreaseEvent;
            CPDFScalingUI.SetPresetScaleEvent -= CPDFScalingUI_SetPresetScaleEvent;
        }

        private void PDFScalingControl_ScaleDecreaseEvent(object sender, EventArgs e)
        {
            if (pdfViewer == null || pdfViewer.Document == null)
            {
                return;
            }
            if (pdfViewer.ZoomFactor < 3)
            {
                pdfViewer.Zoom(pdfViewer.ZoomFactor - 0.1);
            }
            else if (pdfViewer.ZoomFactor < 6)
            {
                pdfViewer.Zoom(pdfViewer.ZoomFactor - 0.2);
            }
            else if (pdfViewer.ZoomFactor <= 10)
            {
                pdfViewer.Zoom(pdfViewer.ZoomFactor - 0.3);
            }
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.ZoomFactor * 100)));
        }

        private void PDFScalingControl_ScaleIncreaseEvent(object sender, EventArgs e)
        {
            if (pdfViewer == null || pdfViewer.Document == null)
            {
                return;
            }
            if (pdfViewer.ZoomFactor < 3)
            {
                pdfViewer.Zoom(pdfViewer.ZoomFactor + 0.1);
            }
            else if (pdfViewer.ZoomFactor < 6)
            {
                pdfViewer.Zoom(pdfViewer.ZoomFactor + 0.2);
            }
            else if (pdfViewer.ZoomFactor <= 10)
            {
                pdfViewer.Zoom(pdfViewer.ZoomFactor + 0.3);
            }
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.ZoomFactor * 100)));
        }

        private void PDFScalingControl_SetScaleEvent(object sender, string e)
        {
            if (pdfViewer == null || pdfViewer.Document == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(e))
            {
                pdfViewer.Zoom(double.Parse(e) / 100);
                SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.ZoomFactor * 100)));
            }
        }

        private void CPDFScalingUI_SetPresetScaleEvent(object sender, string e)
        {
            if (pdfViewer == null || pdfViewer.Document == null)
            {
                return;
            }
            if (e == LanguageHelper.CommonManager.GetString("Zoom_Real"))
            {
                pdfViewer.ChangeFitMode(FitMode.FitSize);
            }
            else if (e == LanguageHelper.CommonManager.GetString("Zoom_FitWidth"))
            {
                pdfViewer.ChangeFitMode(FitMode.FitWidth);
            }
            else if (e == LanguageHelper.CommonManager.GetString("Zoom_FitPage"))
            {
                pdfViewer.ChangeFitMode(FitMode.FitHeight);
            }
            else
            {
                pdfViewer.Zoom(double.Parse(e) / 100);
            }
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.ZoomFactor * 100)));
        }

        public void SetZoomTextBoxText(string value)
        {
            CPDFScalingUI.SetZoomTextBoxText(value);
        }
         
        private void CPDFPageScalingControl_LostFocus(object sender, RoutedEventArgs e)
        {
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.ZoomFactor * 100)));
        }
    }
}
