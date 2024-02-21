using Compdfkit_Tools.Data;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFFreehandUI : UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        public event EventHandler<bool> EraseClickHandler;
        public event EventHandler<double> EraseChangeHandler;

        private AnnotAttribEvent annotAttribEvent;
        public CPDFFreehandUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged += CPDFThicknessControl_ThicknessChanged;
            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());
            EraseThickness.ThicknessChanged += EraseThickness_ThicknessChanged;
        }

        private void EraseThickness_ThicknessChanged(object sender, EventArgs e)
        {
            EraseChangeHandler?.Invoke(this, EraseThickness.Thickness);
            EraseCircle.Width = EraseThickness.Thickness * 6;
            EraseCircle.Height = EraseThickness.Thickness * 6;
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Transparency, CPDFOpacityControl.OpacityValue / 100.0);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());

        }

        private void CPDFThicknessControl_ThicknessChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Thickness, CPDFThicknessControl.Thickness);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());

        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());

            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Color, ((SolidColorBrush)ColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());

        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.NoteText, NoteTextBox.Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        public void SetPresentAnnotAttrib(AnnotAttribEvent annotAttribEvent)
        {
            this.annotAttribEvent = null;
            ColorPickerControl.Brush = new SolidColorBrush((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);
            CPDFOpacityControl.OpacityValue = (int)((double)annotAttribEvent.Attribs[AnnotAttrib.Transparency] * 100);
            CPDFThicknessControl.Thickness =Convert.ToInt16(annotAttribEvent.Attribs[AnnotAttrib.Thickness]);
            if(annotAttribEvent.Attribs.ContainsKey(AnnotAttrib.Color))
            {
                ColorPickerControl.SetCheckedForColor((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);
            }
            NoteTextBox.Text = (string)annotAttribEvent.Attribs[AnnotAttrib.NoteText];
            this.annotAttribEvent = annotAttribEvent;
        }

        public CPDFFreehandData GetFreehandData()
        {
            CPDFFreehandData pdfFreehandData = new CPDFFreehandData();
            pdfFreehandData.AnnotationType = CPDFAnnotationType.Freehand;
            pdfFreehandData.BorderColor = ((SolidColorBrush)ColorPickerControl.Brush).Color;
            pdfFreehandData.Opacity = CPDFOpacityControl.OpacityValue / 100.0;
            pdfFreehandData.Thickness = CPDFThicknessControl.Thickness;
            pdfFreehandData.Note = NoteTextBox.Text;
            return pdfFreehandData;
        }

        public void SetEraseCheck(bool isCheck)
        {
            if(isCheck)
            {
                FreehandBtn.IsChecked = false;
                EraseBtn.IsChecked = true;
                FreehandPanel.Visibility = Visibility.Collapsed;
                ErasePanel.Visibility = Visibility.Visible;
                CPDFAnnotationPreviewerControl.Visibility = Visibility.Collapsed;
                EraseCirclePanel.Visibility = Visibility.Visible;
            }
            else
            {
                FreehandBtn.IsChecked = true;
                EraseBtn.IsChecked = false;
                FreehandPanel.Visibility = Visibility.Visible;
                ErasePanel.Visibility = Visibility.Collapsed;
                CPDFAnnotationPreviewerControl.Visibility = Visibility.Visible;
                EraseCirclePanel.Visibility=Visibility.Collapsed;
            }
        }

        internal void ClearAnnotAttribEvent()
        {
            annotAttribEvent = null;
        }

        internal int GetEraseThickness()
        {
            return EraseThickness.Thickness;
        }

        private void FreehandBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEraseCheck(false);
            EraseClickHandler?.Invoke(this, false);
        }

        private void EraseBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEraseCheck(true);
            EraseClickHandler?.Invoke(this, true);
        }
    }
}
