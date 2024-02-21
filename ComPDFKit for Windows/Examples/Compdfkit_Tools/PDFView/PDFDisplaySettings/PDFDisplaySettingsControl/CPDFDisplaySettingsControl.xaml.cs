using Compdfkit_Tools.PDFControlUI;
using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFDisplaySettingsControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private CPDFViewer pdfView;

        public CPDFDisplaySettingsControl()
        {
            InitializeComponent();
            Loaded += CPDFDisplaySettingsControl_Loaded;
        }

        private void CPDFDisplaySettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModeUI.ViewModeChanged += ViewModeUI_ViewModeChanged;
            ViewModeUI.SplitModeChanged += ViewModeUI_SplitModeChanged;
            ViewModeUI.CropModeChanged += ViewModeUI_CropModeChanged;
            DrawModeUI.DrawModeChanged += DrawModeUI_DrawModeChanged;
        }

        private void ViewModeUI_CropModeChanged(object sender, bool e)
        {
            pdfView?.SetCropMode(e);
        }

        private void DrawModeUI_DrawModeChanged(object sender, DrawModeData e)
        {
            if (e.DrawMode != DrawModes.Draw_Mode_Custom)
            {
                pdfView?.SetDrawMode(e.DrawMode);
                pdfView?.ReloadDocument();
            }
            else
            {
                pdfView?.SetDrawMode(e.DrawMode,e.CustomColor);
                pdfView?.ReloadDocument();
            }
        }

        private void ViewModeUI_SplitModeChanged(object sender, SplitMode e)
        {
            pdfView?.SetSplitMode(e);
        }

        private void ViewModeUI_ViewModeChanged(object sender, ViewMode e)
        {
           pdfView?.ChangeViewMode(e);
        }

        public void InitWithPDFViewer(CPDFViewer newPDFView)
        {
            pdfView = newPDFView;
            if(pdfView != null)
            {
                pdfView.InfoChanged -= PdfView_InfoChanged;
                pdfView.InfoChanged += PdfView_InfoChanged;

                ViewModeUI.SetSplitModeUI(pdfView.Mode);
                ViewModeUI.SetViewModeUI(pdfView.ModeView);
                ViewModeUI.SetCropUI(pdfView.IsCropMode());
            } 
        }

        public void SetVisibilityWhenContentEdit(Visibility visible)
        {
            ViewModeUI?.SetSplitContainerVisibility(visible);
            ViewModeUI?.SetCropContainerVisibility(visible);
        }

        private void PdfView_InfoChanged(object sender, KeyValuePair<string, object> e)
        {
            if (e.Key == "ActiveViewIndex")
            {
                if (pdfView != null)
                {
                    ViewModeUI.SetSplitModeUI(pdfView.Mode);
                    ViewModeUI.SetViewModeUI(pdfView.ModeView);
                    ViewModeUI.SetCropUI(pdfView.IsCropMode());
                }
            }
        }
    }
}
