using Compdfkit_Tools.Common;
using Compdfkit_Tools.Data;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Compdfkit_Tools.Helper;
using ComPDFKit.PDFAnnotation;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFShapeUI : UserControl
    {
        private CPDFAnnotationType currentAnnotationType;

        private AnnotAttribEvent annotAttribEvent;

        public event EventHandler<CPDFAnnotationData> PropertyChanged;

        public CPDFShapeUI()
        {
            InitializeComponent();
            BorderColorPickerControl.ColorChanged += BorderColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged += CPDFThicknessControl_ThicknessChanged;
            CPDFLineStyleControl.LineStyleChanged += CPDFLineStyleControl_LineStyleChanged;

            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Color, ((SolidColorBrush)BorderColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void FillColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FillColor, ((SolidColorBrush)FillColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFThicknessControl_ThicknessChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Thickness, CPDFThicknessControl.Thickness);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Transparency, CPDFOpacityControl.OpacityValue / 100.0);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFLineStyleControl_LineStyleChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.LineStyle, CPDFLineStyleControl.DashStyle);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFArrowControl_ArrowChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(PropertyChanged, GetShapeData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.LineStart, CPDFArrowControl.LineType.HeadLineType);
                annotAttribEvent.UpdateAttrib(AnnotAttrib.LineEnd, CPDFArrowControl.LineType.TailLineType);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.NoteText, NoteTextBox.Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        public CPDFAnnotationData GetShapeData()
        {
            if (currentAnnotationType == CPDFAnnotationType.Circle || currentAnnotationType == CPDFAnnotationType.Square)
            {
                CPDFShapeData pdfShapeData = new CPDFShapeData();
                pdfShapeData.AnnotationType = currentAnnotationType;
                pdfShapeData.BorderColor = ((SolidColorBrush)BorderColorPickerControl.Brush).Color;
                pdfShapeData.FillColor = ((SolidColorBrush)FillColorPickerControl.Brush).Color;
                pdfShapeData.Opacity = CPDFOpacityControl.OpacityValue / 100.0;
                pdfShapeData.Thickness = CPDFThicknessControl.Thickness;
                pdfShapeData.DashStyle = CPDFLineStyleControl.DashStyle;
                pdfShapeData.Note = NoteTextBox.Text;
                return pdfShapeData;
            }

            else
            {
                CPDFLineShapeData pdfLineShapeData = new CPDFLineShapeData();
                pdfLineShapeData.AnnotationType = currentAnnotationType;
                pdfLineShapeData.BorderColor = ((SolidColorBrush)BorderColorPickerControl.Brush).Color;
                pdfLineShapeData.Opacity = CPDFOpacityControl.OpacityValue / 100.0;
                pdfLineShapeData.LineType = CPDFArrowControl.LineType;
                pdfLineShapeData.Thickness = CPDFThicknessControl.Thickness;
                pdfLineShapeData.DashStyle = CPDFLineStyleControl.DashStyle;
                pdfLineShapeData.LineType = CPDFArrowControl.LineType;
                pdfLineShapeData.Note = NoteTextBox.Text;
                return pdfLineShapeData;
            }
        }

        public void SetPresentAnnotAttrib(AnnotAttribEvent annotAttribEvent)
        {
            this.annotAttribEvent = null;
            BorderColorPickerControl.Brush = new SolidColorBrush((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);

            CPDFOpacityControl.OpacityValue = (int)((double)annotAttribEvent.Attribs[AnnotAttrib.Transparency] * 100);
            CPDFThicknessControl.Thickness = Convert.ToInt16(annotAttribEvent.Attribs[AnnotAttrib.Thickness]);
            NoteTextBox.Text = (string)annotAttribEvent.Attribs[AnnotAttrib.NoteText];
            if(annotAttribEvent.Attribs!=null && annotAttribEvent.Attribs.ContainsKey(AnnotAttrib.Color))
            {
                BorderColorPickerControl.SetCheckedForColor((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);
            }

            if (annotAttribEvent.Attribs != null && annotAttribEvent.Attribs.ContainsKey(AnnotAttrib.FillColor))
            {
                FillColorPickerControl.SetCheckedForColor((Color)annotAttribEvent.Attribs[AnnotAttrib.FillColor]);
            }

            CPDFLineStyleControl.DashStyle = (DashStyle)(annotAttribEvent.Attribs[AnnotAttrib.LineStyle]);

            if (annotAttribEvent.GetAnnotTypes() == AnnotArgsType.AnnotSquare ||
                annotAttribEvent.GetAnnotTypes() == AnnotArgsType.AnnotCircle)
            {
                FillColorPickerControl.Brush = new SolidColorBrush((Color)annotAttribEvent.Attribs[AnnotAttrib.FillColor]);
            }
            else
            {
                LineType lineType = new LineType()
                {
                    HeadLineType = (C_LINE_TYPE)annotAttribEvent.Attribs[AnnotAttrib.LineStart],
                    TailLineType = (C_LINE_TYPE)annotAttribEvent.Attribs[AnnotAttrib.LineEnd]
                };
                CPDFArrowControl.LineType = lineType; 
            }
            this.annotAttribEvent = annotAttribEvent;
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        public void InitWhenRectAndRound()
        {
            FillColorStackPanel.Visibility = Visibility.Visible;
            ArrowStackPanel.Visibility = Visibility.Collapsed;

            FillColorPickerControl.ColorChanged += FillColorPickerControl_ColorChanged;
            CPDFArrowControl.ArrowChanged -= CPDFArrowControl_ArrowChanged;
        }

        public void InitWhenArrowAndLine()
        {
            FillColorStackPanel.Visibility = Visibility.Collapsed;
            ArrowStackPanel.Visibility = Visibility.Visible;

            CPDFArrowControl.ArrowChanged += CPDFArrowControl_ArrowChanged;
            FillColorPickerControl.ColorChanged -= FillColorPickerControl_ColorChanged;
            LineType lineType;

            if (currentAnnotationType == CPDFAnnotationType.Arrow)
            {
                lineType = new LineType()
                {
                    HeadLineType = C_LINE_TYPE.LINETYPE_NONE,
                    TailLineType = C_LINE_TYPE.LINETYPE_ARROW
                };
            }
            else
            {
                lineType = new LineType()
                {
                    HeadLineType = C_LINE_TYPE.LINETYPE_NONE,
                    TailLineType = C_LINE_TYPE.LINETYPE_NONE
                };
            }
            CPDFArrowControl.LineType = lineType;
        }


        public void InitWithAnnotationType(CPDFAnnotationType annotationType)
        {
            currentAnnotationType = annotationType;
            switch (annotationType)
            {
                case CPDFAnnotationType.Square:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Square");
                    InitWhenRectAndRound();
                    break;
                case CPDFAnnotationType.Circle:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Circle");
                    InitWhenRectAndRound();
                    break;
                case CPDFAnnotationType.Arrow:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Arrow");
                    InitWhenArrowAndLine();
                    break;
                case CPDFAnnotationType.Line:
                    TitleTextBlock.Text = LanguageHelper.PropertyPanelManager.GetString("Title_Line");
                    InitWhenArrowAndLine();
                    break;
                default:
                    throw new ArgumentException("Not Excepted Argument");
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }
    }
}
