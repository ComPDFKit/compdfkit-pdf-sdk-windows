using Compdfkit_Tools.Common;
using Compdfkit_Tools.Data;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFMarkupUI : UserControl
    {
        private CPDFAnnotationType currentAnnotationType;
        private AnnotAttribEvent annotAttribEvent;
        public event EventHandler<CPDFAnnotationData> PropertyChanged;

        public CPDFMarkupUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetMarkupData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Transparency, CPDFOpacityControl.OpacityValue / 100.0);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawMarkUpPreview(GetMarkupData());
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetMarkupData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Color, ((SolidColorBrush)ColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawMarkUpPreview(GetMarkupData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetMarkupData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.NoteText, NoteTextBox.Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        public CPDFMarkupData GetMarkupData()
        {
            CPDFMarkupData pdfMarkupData = new CPDFMarkupData();
            pdfMarkupData.AnnotationType = currentAnnotationType;
            pdfMarkupData.Color = ((SolidColorBrush)ColorPickerControl.Brush).Color;
            pdfMarkupData.Opacity = CPDFOpacityControl.OpacityValue / 100.0;
            pdfMarkupData.Note = NoteTextBox.Text;
            return pdfMarkupData;
        }

        public void SetPresentAnnotAttrib(AnnotAttribEvent annotAttribEvent)
        {
            this.annotAttribEvent = null;
            ColorPickerControl.Brush = new SolidColorBrush((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);
            ColorPickerControl.SetCheckedForColor((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);
            CPDFOpacityControl.OpacityValue = (int)((double)annotAttribEvent.Attribs[AnnotAttrib.Transparency] * 100);
            NoteTextBox.Text = (string)annotAttribEvent.Attribs[AnnotAttrib.NoteText];
            this.annotAttribEvent = annotAttribEvent;
            CPDFAnnotationPreviewerControl.DrawMarkUpPreview(GetMarkupData());
        }

        public void InitWithAnnotationType(CPDFAnnotationType annotationType)
        {
            currentAnnotationType = annotationType;
            switch (annotationType)
            {
                case CPDFAnnotationType.Highlight:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Highlight");
                    break;
                case CPDFAnnotationType.Underline:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Underline");
                    break;
                case CPDFAnnotationType.Strikeout:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Strikeout");
                    break;
                case CPDFAnnotationType.Squiggly:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Squiggly");
                    break;
                default:
                    throw new ArgumentException("Not Excepted Argument");
            }
            CPDFAnnotationPreviewerControl.DrawMarkUpPreview(GetMarkupData());
        }
    }
}
