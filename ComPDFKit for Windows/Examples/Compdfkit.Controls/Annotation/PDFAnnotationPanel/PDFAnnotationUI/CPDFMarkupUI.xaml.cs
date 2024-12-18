using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControl;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFMarkupUI : UserControl
    {
        private CPDFAnnotationType currentAnnotationType;
        private AnnotParam annotParam;
        private CPDFMarkupAnnotation markupAnnot;
        private PDFViewControl viewControl;
        public event EventHandler<CPDFAnnotationData> PropertyChanged;

        public CPDFMarkupUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged -= ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged -= CPDFOpacityControl_OpacityChanged;

            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
        }
        
        private AnnotHistory GetAnnotHistory()
        {
            if (markupAnnot != null && markupAnnot.IsValid())
            {
                switch (markupAnnot.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                        return new HighlightAnnotHistory();
                    case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                        return new UnderlineAnnotHistory();
                    case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                        return new StrikeoutAnnotHistory();
                    case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                        return new SquigglyAnnotHistory();
                }
            }
            return new AnnotHistory();
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetMarkupData());
            }
            else
            {
                if(markupAnnot!=null && markupAnnot.IsValid())
                {
                    byte opacity = (byte)(CPDFOpacityControl.OpacityValue / 100.0 * 255);
                    if (viewControl != null && viewControl.PDFViewTool != null)
                    {
                        AnnotHistory history = GetAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, markupAnnot.Page.PageIndex, markupAnnot);
                        
                        markupAnnot.SetTransparency(opacity);
                        markupAnnot.UpdateAp();
                        viewControl.UpdateAnnotFrame();
                        
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, markupAnnot.Page.PageIndex, markupAnnot);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawMarkUpPreview(GetMarkupData());
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetMarkupData());
            }
            else
            {
                SolidColorBrush colorBrush = ColorPickerControl.Brush as SolidColorBrush;
                if (markupAnnot != null && markupAnnot.IsValid() && colorBrush!=null)
                {
                    byte[] color = new byte[3]
                    {
                        colorBrush.Color.R,
                        colorBrush.Color.G,
                        colorBrush.Color.B
                    };
                    if (viewControl != null && !markupAnnot.Color.SequenceEqual(color))
                    {
                        AnnotHistory history = GetAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, markupAnnot.Page.PageIndex, markupAnnot);
                        
                        markupAnnot.SetColor(color);
                        markupAnnot.UpdateAp();
                        viewControl.UpdateAnnotFrame();
                        
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, markupAnnot.Page.PageIndex, markupAnnot);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawMarkUpPreview(GetMarkupData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetMarkupData());
            }
            else
            {
                if (markupAnnot != null && markupAnnot.IsValid())
                {
                    if (viewControl != null && markupAnnot.GetContent() != NoteTextBox.Text)
                    {
                        AnnotHistory history = GetAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, markupAnnot.Page.PageIndex, markupAnnot);
                        
                        markupAnnot.SetContent(NoteTextBox.Text);
                        viewControl.UpdateAnnotFrame();
                        
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, markupAnnot.Page.PageIndex, markupAnnot);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    }
                }
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

        public void SetPresentAnnotAttrib(AnnotParam param,CPDFMarkupAnnotation markup,PDFViewControl view)
        {
            this.annotParam = null;
            this.markupAnnot = null;
            this.viewControl = null;

            if(param!=null)
            {
                if(param is HighlightParam)
                {
                    HighlightParam highlightParam = (HighlightParam)param;
                    Color newColor = Color.FromRgb(
                        highlightParam.HighlightColor[0],
                        highlightParam.HighlightColor[1],
                        highlightParam.HighlightColor[2]);

                    ColorPickerControl.Brush = new SolidColorBrush(newColor);
                    ColorPickerControl.SetCheckedForColor(newColor);
                }
               
                if(param is UnderlineParam)
                {
                    UnderlineParam underlineParam = (UnderlineParam)param;
                    Color newColor = Color.FromRgb(
                        underlineParam.UnderlineColor[0],
                        underlineParam.UnderlineColor[1],
                        underlineParam.UnderlineColor[2]);

                    ColorPickerControl.Brush = new SolidColorBrush(newColor);
                    ColorPickerControl.SetCheckedForColor(newColor);
                }

                if (param is StrikeoutParam)
                {
                    StrikeoutParam strikeoutParam = (StrikeoutParam)param;
                    Color newColor = Color.FromRgb(
                       strikeoutParam.StrikeoutColor[0],
                       strikeoutParam.StrikeoutColor[1],
                       strikeoutParam.StrikeoutColor[2]);

                    ColorPickerControl.Brush = new SolidColorBrush(newColor);
                    ColorPickerControl.SetCheckedForColor(newColor);
                }

                if(param is SquigglyParam)
                {
                    SquigglyParam squigglyParam= (SquigglyParam)param;
                    Color newColor = Color.FromRgb(
                      squigglyParam.SquigglyColor[0],
                      squigglyParam.SquigglyColor[1],
                      squigglyParam.SquigglyColor[2]);

                    ColorPickerControl.Brush = new SolidColorBrush(newColor);
                    ColorPickerControl.SetCheckedForColor(newColor); 
                }
                CPDFOpacityControl.OpacityValue = (int)(param.Transparency / 255D * 100);
                NoteTextBox.Text = param.Content;
            }
          
            this.annotParam = param;
            this.markupAnnot = markup;
            this.viewControl = view;
          
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
