using ComPDFKit.Controls.PDFControlUI;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFDisplaySettingsControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private PDFViewControl ViewControl;
        public event EventHandler<CPDFViewModeUI.SplitMode> SplitModeChanged;
        public CPDFDisplaySettingsControl()
        {
            InitializeComponent();
            Loaded += CPDFDisplaySettingsControl_Loaded;
        }

        private void CPDFDisplaySettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void ViewModeUI_SplitModeChanged(object sender, CPDFViewModeUI.SplitMode e)
        {
            SplitModeChanged?.Invoke(this, e);
        }

        private void ViewModeUI_CropModeChanged(object sender, bool e)
        {
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                ViewControl?.SetCropMode(e);
            }
        }

        private void DrawModeUI_DrawModeChanged(object sender, DrawModeData e)
        {
            if(ViewControl!=null && ViewControl.PDFViewTool!=null)
            {
                ViewControl.SetDrawModes(e);
            }
        }

        private void ViewModeUI_ViewModeChanged(object sender, ViewMode e)
        {
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.FocusPDFViewTool.GetCPDFViewer();
                if (pdfViewer != null)
                {
                    pdfViewer?.SetViewMode(e);
                }
            }
        }

        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            ViewControl = viewControl;
            ViewControl.FocusPDFViewToolChanged -= PDFViewControl_FocusPDFViewToolChanged;
            ViewControl.FocusPDFViewToolChanged += PDFViewControl_FocusPDFViewToolChanged;
            if(ViewControl != null && ViewControl.PDFViewTool!=null)
            {
               CPDFViewer pdfViewer=  viewControl.PDFViewTool.GetCPDFViewer();
                if(pdfViewer != null)
                {
                    ViewModeUI.SetViewModeUI(pdfViewer.GetViewMode());
                }
            }
            ViewModeUI.ViewModeChanged -= ViewModeUI_ViewModeChanged;
            ViewModeUI.SplitModeChanged -= ViewModeUI_SplitModeChanged;
            ViewModeUI.CropModeChanged -= ViewModeUI_CropModeChanged;
            DrawModeUI.DrawModeChanged -= DrawModeUI_DrawModeChanged;

            ViewModeUI.ViewModeChanged += ViewModeUI_ViewModeChanged;
            ViewModeUI.SplitModeChanged += ViewModeUI_SplitModeChanged;
            ViewModeUI.CropModeChanged += ViewModeUI_CropModeChanged;
            DrawModeUI.DrawModeChanged += DrawModeUI_DrawModeChanged;
        }

        private void PDFViewControl_FocusPDFViewToolChanged(object sender,EventArgs e)
        {
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.FocusPDFViewTool.GetCPDFViewer();
                if (pdfViewer != null)
                {
                    ViewModeUI.SetViewModeUI(pdfViewer.GetViewMode()); 
                }
            }
        }

        public void SetVisibilityWhenContentEdit(Visibility visible)
        {
            ViewModeUI?.SetSplitContainerVisibility(visible);
            ViewModeUI?.SetCropContainerVisibility(visible);
        }
    }
}
