using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControl;
using ComPDFKitViewer;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using ComPDFKit.PDFDocument;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFFreeTextUI : System.Windows.Controls.UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        private CPDFFreeTextAnnotation textAnnot;
        private FreeTextParam textParam;
        private PDFViewControl viewControl;
        public CPDFFreeTextUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged -= ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged -= CPDFOpacityControl_OpacityChanged;
            CPDFFontControl.FontFamilyChanged -= CPDFFontControl_FontFamilyChanged;
            CPDFFontControl.FontStyleChanged -= CPDFFontControl_FontStyleChanged;
            CPDFFontControl.FontAlignChanged -= CPDFFontControl_FontAlignChanged;
            CPDFFontControl.FontSizeChanged -= CPDFFontControl_FontSizeChanged;

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
            if (textAnnot == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                if(textAnnot!=null &&textAnnot.IsValid())
                {
                    CTextAttribute textAttr = textAnnot.FreeTextDa;
                    textAttr.FontSize = CPDFFontControl.FontSizeValue;
                    textAnnot.SetFreetextDa(textAttr);
                    textAnnot.UpdateAp();
                    if (viewControl != null && viewControl.PDFViewTool != null)
                    {
                        viewControl.UpdateAnnotFrame();
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFFontControl_FontAlignChanged(object sender, EventArgs e)
        {
            if (textAnnot == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                if (textAnnot != null && textAnnot.IsValid())
                {
                    FreeTextAnnotHistory history = new FreeTextAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    
                   switch(CPDFFontControl.TextAlignment)
                    {
                        case TextAlignment.Left:
                            textAnnot.SetFreetextAlignment(C_TEXT_ALIGNMENT.ALIGNMENT_LEFT);
                            break;
                        case TextAlignment.Center:
                            textAnnot.SetFreetextAlignment(C_TEXT_ALIGNMENT.ALIGNMENT_CENTER);
                            break;
                        case TextAlignment.Right:
                            textAnnot.SetFreetextAlignment(C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT);
                            break;
                        default:
                            break;
                    }
                    textAnnot.UpdateAp();
                    if (viewControl != null && viewControl.PDFViewTool != null)
                    {
                        viewControl.UpdateAnnotFrame();
                    }
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFFontControl_FontStyleChanged(object sender, EventArgs e)
        {
            if (textAnnot == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                if (textAnnot != null && textAnnot.IsValid())
                {
                    FreeTextAnnotHistory history = new FreeTextAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    
                    CTextAttribute textAttr = textAnnot.FreeTextDa;
                    string psName=String.Empty;
                    CPDFFont.GetPostScriptName(CPDFFontControl.FontFamilyValue, CPDFFontControl.FontStyleValue, ref psName);
                    textAttr.FontName = psName;
                    textAnnot.SetFreetextDa(textAttr);
                    textAnnot.UpdateAp();
                    if (viewControl != null && viewControl.PDFViewTool != null)
                    {
                        viewControl.UpdateAnnotFrame();
                    }
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFFontControl_FontFamilyChanged(object sender, EventArgs e)
        {
            if (textAnnot == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                FreeTextAnnotHistory history = new FreeTextAnnotHistory();
                history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                history.Action = HistoryAction.Update;
                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                
                CTextAttribute textAttr = textAnnot.FreeTextDa;
                bool isBold = IsBold(textAttr.FontName);
                bool isItalic = IsItalic(textAttr.FontName);
                string psName=String.Empty;
                CPDFFont.GetPostScriptName(CPDFFontControl.FontFamilyValue, CPDFFontControl.FontStyleValue, ref psName);
                textAttr.FontName = psName;
                textAnnot.SetFreetextDa(textAttr);
                textAnnot.UpdateAp();
                if (viewControl != null && viewControl.PDFViewTool != null)
                {
                    viewControl.UpdateAnnotFrame() ;
                }
                
                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (textAnnot == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                if (viewControl != null && textParam.Transparency != CPDFOpacityControl.OpacityValue)
                {
                    FreeTextAnnotHistory history = new FreeTextAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    textAnnot.SetTransparency((byte)(CPDFOpacityControl.OpacityValue / 100.0*255));
                    textAnnot.UpdateAp();
                    viewControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (textAnnot == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                Color fontColor = ((SolidColorBrush)ColorPickerControl.Brush).Color;
                CTextAttribute textAttr = new CTextAttribute
                {
                    FontName = textAnnot.FreeTextDa.FontName,
                    FontSize = textAnnot.FreeTextDa.FontSize,
                    FontColor = new byte[3]
                    {
                        fontColor.R,
                        fontColor.G, 
                        fontColor.B,
                    }
                };

                if (viewControl != null && !textAnnot.FreeTextDa.FontColor.SequenceEqual(textAttr.FontColor))
                {
                    FreeTextAnnotHistory history = new FreeTextAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    
                    textAnnot.SetFreetextDa(textAttr);
                    textAnnot.UpdateAp();
                    viewControl.UpdateAnnotFrame() ;
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
            CPDFAnnotationPreviewerControl.DrawFreeTextPreview(GetFreeTextData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textAnnot == null)
            {
                PropertyChanged?.Invoke(this, GetFreeTextData());
            }
            else
            {
                if (viewControl != null && textParam.Content != NoteTextBox.Text)
                {
                    FreeTextAnnotHistory history = new FreeTextAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    
                    viewControl.UpdateAnnotFrame();
                    textAnnot.SetContent(NoteTextBox.Text);
                    textAnnot.UpdateAp();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        public void SetPresentAnnotAttrib(FreeTextParam textParam,CPDFFreeTextAnnotation annot,PDFViewControl view)
        {
            this.textAnnot = null;
            if(textParam!=null)
            {
                ColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                    textParam.FontColor[0],
                    textParam.FontColor[1],
                    textParam.FontColor[2]));
                CPDFOpacityControl.OpacityValue = (int)(textParam.Transparency / 255D * 100);
                CPDFFontControl.PostScriptName = textParam.FontName;
                CPDFFontControl.FontSizeValue = (int)textParam.FontSize;
                switch(textParam.Alignment)
                {
                    case C_TEXT_ALIGNMENT.ALIGNMENT_LEFT:
                        CPDFFontControl.TextAlignment =TextAlignment.Left;
                        break;
                    case C_TEXT_ALIGNMENT.ALIGNMENT_CENTER:
                        CPDFFontControl.TextAlignment=TextAlignment.Center;
                        break;
                    case C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT:
                        CPDFFontControl.TextAlignment = TextAlignment.Right;
                        break;
                    default:
                        break;
                }
                NoteTextBox.Text = textParam.Content;
                ColorPickerControl.SetCheckedForColor(Color.FromRgb(
                    textParam.FontColor[0],
                    textParam.FontColor[1],
                    textParam.FontColor[2]));
            }
           
            this.textAnnot = annot;
            this.textParam = textParam;
            this.viewControl = view;
        }

        public CPDFFreeTextData GetFreeTextData()
        {
            CPDFFreeTextData pdfFreeTextData = new CPDFFreeTextData();
             pdfFreeTextData.AnnotationType = CPDFAnnotationType.FreeText;
            pdfFreeTextData.BorderColor = ((SolidColorBrush)ColorPickerControl.Brush).Color;
            pdfFreeTextData.Opacity = CPDFOpacityControl.OpacityValue / 100.0;
            pdfFreeTextData.FontFamily = CPDFFontControl.PostScriptName;
            pdfFreeTextData.FontSize = CPDFFontControl.FontSizeValue;
            pdfFreeTextData.TextAlignment = CPDFFontControl.TextAlignment;
            pdfFreeTextData.Note = NoteTextBox.Text;
            return pdfFreeTextData;
        }
    }
}
