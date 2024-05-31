using ComPDFKit.Controls.Helper;
using ComPDFKitViewer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFScalingControl : UserControl
    {
        public PDFViewControl ViewControl;

        public CPDFScalingControl()
        {
            InitializeComponent();
        }

        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            ViewControl = viewControl;
        }

        private void PDFControl_MouseWheelZoomHandler(object sender, ComPDFKitViewer.MouseWheelZoomArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                CPDFViewer pdfViewer = ViewControl.GetCPDFViewer();
                SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.GetZoom() * 100)));
            }
        }

        private void PDFScalingControl_Loaded(object sender, RoutedEventArgs e)
        {
            CPDFScalingUI.SetScaleEvent -= PDFScalingControl_SetScaleEvent;
            CPDFScalingUI.ScaleIncreaseEvent -= PDFScalingControl_ScaleIncreaseEvent;
            CPDFScalingUI.ScaleDecreaseEvent -= PDFScalingControl_ScaleDecreaseEvent;
            CPDFScalingUI.SetPresetScaleEvent -= CPDFScalingUI_SetPresetScaleEvent;

            CPDFScalingUI.SetScaleEvent += PDFScalingControl_SetScaleEvent;
            CPDFScalingUI.ScaleIncreaseEvent += PDFScalingControl_ScaleIncreaseEvent;
            CPDFScalingUI.ScaleDecreaseEvent += PDFScalingControl_ScaleDecreaseEvent;
            CPDFScalingUI.SetPresetScaleEvent += CPDFScalingUI_SetPresetScaleEvent;
            
            if (ViewControl != null)
            {
                ViewControl.MouseWheelZoomHandler -= PDFControl_MouseWheelZoomHandler;
                ViewControl.FocusPDFViewToolChanged -= ViewControl_FocusPDFViewToolChanged;
                ViewControl.MouseWheelZoomHandler += PDFControl_MouseWheelZoomHandler;
                ViewControl.FocusPDFViewToolChanged += ViewControl_FocusPDFViewToolChanged;
            }
        }

        private void ViewControl_FocusPDFViewToolChanged(object sender, EventArgs e)
        {
            CPDFViewer pdfViewer = ViewControl.GetCPDFViewer();
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.GetZoom() * 100)));
        }

        private void CPDFScalingUI_Unloaded(object sender, RoutedEventArgs e)
        {
            CPDFScalingUI.SetScaleEvent -= PDFScalingControl_SetScaleEvent;
            CPDFScalingUI.ScaleIncreaseEvent -= PDFScalingControl_ScaleIncreaseEvent;
            CPDFScalingUI.ScaleDecreaseEvent -= PDFScalingControl_ScaleDecreaseEvent;
            CPDFScalingUI.SetPresetScaleEvent -= CPDFScalingUI_SetPresetScaleEvent;
            ViewControl.MouseWheelZoomHandler -= PDFControl_MouseWheelZoomHandler;
        }

        private void PDFScalingControl_ScaleDecreaseEvent(object sender, EventArgs e)
        {
            if (ViewControl == null || ViewControl.PDFViewTool == null)
            {
                return;
            }
            CPDFViewer pdfViewer = ViewControl.GetCPDFViewer();
            if (pdfViewer == null)
            {
                return;
            }
         
            if (pdfViewer.GetZoom() < 3)
            {
                double newZoom = Math.Max(0.01, pdfViewer.GetZoom() - 0.1);
                pdfViewer.SetZoom(newZoom);
                pdfViewer.UpdateRenderFrame();
            }
            else if (pdfViewer.GetZoom() < 6)
            {
                pdfViewer.SetZoom(pdfViewer.GetZoom() - 0.2);
                pdfViewer.UpdateRenderFrame();
            }
            else if (pdfViewer.GetZoom() >6)
            {
                pdfViewer.SetZoom(pdfViewer.GetZoom() - 0.3);
                pdfViewer.UpdateRenderFrame();
            }
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.GetZoom() * 100)));
        }

        private void PDFScalingControl_ScaleIncreaseEvent(object sender, EventArgs e)
        {
            if (ViewControl == null || ViewControl.PDFViewTool == null)
            {
                return;
            }
            CPDFViewer pdfViewer = ViewControl.GetCPDFViewer();
            if (pdfViewer == null)
            {
                return;
            }

            if (pdfViewer.GetZoom() < 3)
            {
                pdfViewer.SetZoom(pdfViewer.GetZoom() + 0.1);
                pdfViewer.UpdateRenderFrame();
            }
            else if (pdfViewer.GetZoom() < 6)
            {
                pdfViewer.SetZoom(pdfViewer.GetZoom() + 0.2);
                pdfViewer.UpdateRenderFrame();
            }
            else if (pdfViewer.GetZoom() <= 10)
            {
                double newZoom = Math.Max(10, pdfViewer.GetZoom() + 0.3);
                pdfViewer.SetZoom(newZoom);
                pdfViewer.UpdateRenderFrame();
            }
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.GetZoom()* 100)));
        }

        private void PDFScalingControl_SetScaleEvent(object sender, string e)
        {
            if (ViewControl == null || ViewControl.PDFViewTool == null)
            {
                return;
            }
            CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
            if (pdfViewer == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(e))
            {
                pdfViewer.SetZoom(double.Parse(e) / 100);
                pdfViewer.UpdateRenderFrame();
                SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.GetZoom() * 100)));
            }
        }

        private void CPDFScalingUI_SetPresetScaleEvent(object sender, string e)
        {
            if (ViewControl == null || ViewControl.PDFViewTool == null)
            {
                return;
            }
            CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
            if (pdfViewer == null)
            {
                return;
            }
            if (e == LanguageHelper.CommonManager.GetString("Zoom_Real"))
            {
                pdfViewer.SetFitMode(FitMode.FitOriginal);
            }
            else if (e == LanguageHelper.CommonManager.GetString("Zoom_FitWidth"))
            {
                pdfViewer.SetFitMode(FitMode.FitWidth);
            }
            else if (e == LanguageHelper.CommonManager.GetString("Zoom_FitPage"))
            {
                pdfViewer.SetFitMode(FitMode.FitHeight);
            }
            else
            {
                pdfViewer.SetZoom(double.Parse(e) / 100);
            }
            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.GetZoom() * 100)));
            pdfViewer.UpdateRenderFrame();
        }

        public void SetZoomTextBoxText(string value)
        {
            CPDFScalingUI.SetZoomTextBoxText(value);
        }
         
        private void CPDFPageScalingControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ViewControl == null || ViewControl.PDFViewTool == null)
            {
                return;
            }
            CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
            if (pdfViewer == null)
            {
                return;
            }

            SetZoomTextBoxText(string.Format("{0}", (int)(pdfViewer.GetZoom() * 100)));
        }
    }
}
