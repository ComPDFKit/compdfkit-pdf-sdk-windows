using Compdfkit_Tools.Common;
using Compdfkit_Tools.Data;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFFreeTextUI : UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        private AnnotAttribEvent annotAttribEvent;

        public CPDFFreeTextUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
            CPDFFontControl.FontFamilyChanged += CPDFFontControl_FontFamilyChanged;
            CPDFFontControl.FontStyleChanged += CPDFFontControl_FontStyleChanged;
            CPDFFontControl.FontAlignChanged += CPDFFontControl_FontAlignChanged;
            CPDFFontControl.FontSizeChanged += CPDFFontControl_FontSizeChanged;
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFFontControl_FontSizeChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontSize, CPDFFontControl.FontSizeValue);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFFontControl_FontAlignChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.TextAlign, CPDFFontControl.TextAlignment);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFFontControl_FontStyleChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsBold, CPDFFontControl.IsBold);
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsItalic, CPDFFontControl.IsItalic);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFFontControl_FontFamilyChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontName, CPDFFontControl.FontFamilyValue);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Transparency, CPDFOpacityControl.OpacityValue / 100.0);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FontColor, ((SolidColorBrush)ColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
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
            ColorPickerControl.Brush = new SolidColorBrush((Color)annotAttribEvent.Attribs[AnnotAttrib.FontColor]);
            CPDFOpacityControl.OpacityValue = (int)((double)annotAttribEvent.Attribs[AnnotAttrib.Transparency] * 100);
            CPDFFontControl.FontFamilyValue = (string)annotAttribEvent.Attribs[AnnotAttrib.FontName];
            CPDFFontControl.FontSizeValue = Convert.ToInt16(annotAttribEvent.Attribs[AnnotAttrib.FontSize]);
            CPDFFontControl.IsBold = (bool)annotAttribEvent.Attribs[AnnotAttrib.IsBold];
            CPDFFontControl.IsItalic = (bool)annotAttribEvent.Attribs[AnnotAttrib.IsItalic];
            CPDFFontControl.TextAlignment = (TextAlignment)annotAttribEvent.Attribs[AnnotAttrib.TextAlign];
            NoteTextBox.Text = (string)annotAttribEvent.Attribs[AnnotAttrib.NoteText];
            this.annotAttribEvent = annotAttribEvent;


            if (annotAttribEvent.Attribs != null && annotAttribEvent.Attribs.ContainsKey(AnnotAttrib.FontColor))
            {
                ColorPickerControl.SetCheckedForColor((Color)annotAttribEvent.Attribs[AnnotAttrib.FontColor]);
            }
        }

        public CPDFFreeTextData GetFreeTextData()
        {
            CPDFFreeTextData pdfFreeTextData = new CPDFFreeTextData();
            pdfFreeTextData.AnnotationType = CPDFAnnotationType.FreeText;
            pdfFreeTextData.BorderColor = ((SolidColorBrush)ColorPickerControl.Brush).Color;
            pdfFreeTextData.Opacity = CPDFOpacityControl.OpacityValue / 100.0;
            pdfFreeTextData.FontFamily = CPDFFontControl.FontFamilyValue;
            pdfFreeTextData.FontSize = CPDFFontControl.FontSizeValue;
            pdfFreeTextData.IsBold = CPDFFontControl.IsBold;
            pdfFreeTextData.IsItalic = CPDFFontControl.IsItalic;
            pdfFreeTextData.TextAlignment = CPDFFontControl.TextAlignment;
            pdfFreeTextData.Note = NoteTextBox.Text;
            return pdfFreeTextData;
        }
    }
}
