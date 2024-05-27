using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFShapeUI : UserControl
    {
        private CPDFAnnotationType currentAnnotationType;

        private AnnotParam annotParam;
        private CPDFAnnotation annotCore;
        private PDFViewControl viewControl;
        public event EventHandler<CPDFAnnotationData> PropertyChanged;

        public CPDFShapeUI()
        {
            InitializeComponent();
            BorderColorPickerControl.ColorChanged -= BorderColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged -= CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged -= CPDFThicknessControl_ThicknessChanged;
            CPDFLineStyleControl.LineStyleChanged -= CPDFLineStyleControl_LineStyleChanged;

            BorderColorPickerControl.ColorChanged += BorderColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged += CPDFThicknessControl_ThicknessChanged;
            CPDFLineStyleControl.LineStyleChanged += CPDFLineStyleControl_LineStyleChanged;

            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private AnnotHistory GetHistory()
        {
            if (annotCore != null && annotCore.IsValid())
            {
                switch (annotCore.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                        return new SquareAnnotHistory();
                    case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                        return new CircleAnnotHistory();
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                        return new LineAnnotHistory();
                }
            }
            return new AnnotHistory();
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData()); 
            }
            else
            {
                SolidColorBrush borderBrush=BorderColorPickerControl.Brush as SolidColorBrush;
                if(annotCore!=null && annotCore.IsValid() && borderBrush!=null)
                {
                    byte[] color = new byte[3]
                    {
                        borderBrush.Color.R,
                        borderBrush.Color.G,
                        borderBrush.Color.B
                    };
                    if (viewControl != null)
                    {
                        AnnotHistory history = GetHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE)
                        {
                            CPDFSquareAnnotation squareAnnot = annotCore as CPDFSquareAnnotation;
                            if (squareAnnot == null || squareAnnot.LineColor.SequenceEqual(color)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                            squareAnnot?.SetLineColor(color);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE)
                        {
                            CPDFCircleAnnotation circleAnnot = annotCore as CPDFCircleAnnotation;
                            if (circleAnnot == null || circleAnnot.LineColor.SequenceEqual(color)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                            circleAnnot?.SetLineColor(color);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                        {
                            CPDFLineAnnotation lineAnnot = annotCore as CPDFLineAnnotation;
                            if (lineAnnot == null || lineAnnot.LineColor.SequenceEqual(color)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                            lineAnnot?.SetLineColor(color);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                        }

                        annotCore.UpdateAp();
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                        viewControl.UpdateAnnotFrame();
                    }
                } 
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void FillColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                SolidColorBrush fillBrush = FillColorPickerControl.Brush as SolidColorBrush;
                if (annotCore != null && annotCore.IsValid() && fillBrush != null)
                {
                    byte[] color = new byte[3]
                    {
                        fillBrush.Color.R,
                        fillBrush.Color.G,
                        fillBrush.Color.B
                    };
                    if (viewControl != null && viewControl.PDFViewTool != null)
                    {
                        AnnotHistory history = GetHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE)
                        {
                            CPDFSquareAnnotation squareAnnot = annotCore as CPDFSquareAnnotation;
                            if (squareAnnot == null || squareAnnot.BgColor.SequenceEqual(color)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                            squareAnnot?.SetBgColor(color);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE)
                        {
                            CPDFCircleAnnotation circleAnnot = annotCore as CPDFCircleAnnotation;
                            if (circleAnnot == null || circleAnnot.BgColor.SequenceEqual(color)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                            circleAnnot?.SetBgColor(color);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                        {
                            CPDFLineAnnotation lineAnnot = annotCore as CPDFLineAnnotation;
                            if (lineAnnot == null || lineAnnot.BgColor.SequenceEqual(color)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                            lineAnnot?.SetBgColor(color);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                        }

                        annotCore.UpdateAp();
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                        viewControl.UpdateAnnotFrame();
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFThicknessControl_ThicknessChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    if (viewControl != null && Math.Abs(CPDFThicknessControl.Thickness - annotCore.GetBorderWidth()) > 0.01)
                    {
                        AnnotHistory history = GetHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE)
                        {
                            CPDFSquareAnnotation squareAnnot = annotCore as CPDFSquareAnnotation;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                            squareAnnot?.SetLineWidth(CPDFThicknessControl.Thickness);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE)
                        {
                            CPDFCircleAnnotation circleAnnot = annotCore as CPDFCircleAnnotation;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                            circleAnnot?.SetLineWidth(CPDFThicknessControl.Thickness);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);

                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                        {
                            CPDFLineAnnotation lineAnnot = annotCore as CPDFLineAnnotation;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                            lineAnnot?.SetLineWidth(CPDFThicknessControl.Thickness);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                        }

                        annotCore.UpdateAp();
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                        viewControl.UpdateAnnotFrame();
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                double opacity = CPDFOpacityControl.OpacityValue / 100.0;
                if (opacity > 0 && opacity <= 1)
                {
                    opacity = opacity * 255;
                }
                if (annotCore != null && annotCore.IsValid())
                {
                    if (viewControl != null && Math.Abs(opacity - annotCore.GetTransparency()) > 0.01)
                    {
                        AnnotHistory history = GetHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE)
                        {
                            CPDFSquareAnnotation squareAnnot = annotCore as CPDFSquareAnnotation;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                            squareAnnot?.SetTransparency((byte)opacity);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE)
                        {
                            CPDFCircleAnnotation circleAnnot = annotCore as CPDFCircleAnnotation;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                            circleAnnot?.SetTransparency((byte)opacity);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                        {
                            CPDFLineAnnotation lineAnnot = annotCore as CPDFLineAnnotation;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                            lineAnnot?.SetTransparency((byte)opacity);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                        }

                        annotCore.UpdateAp();
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                        viewControl.UpdateAnnotFrame();
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFLineStyleControl_LineStyleChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    float[] dashArray = null;
                    C_BORDER_STYLE borderStyle;
                    if (CPDFLineStyleControl.DashStyle == DashStyles.Solid || CPDFLineStyleControl.DashStyle == null)
                    {
                        dashArray = new float[0];
                        borderStyle = C_BORDER_STYLE.BS_SOLID;
                    }
                    else
                    {
                        List<float> floatArray = new List<float>();
                        foreach (double num in CPDFLineStyleControl.DashStyle.Dashes)
                        {
                            floatArray.Add((float)num);
                        }
                        dashArray = floatArray.ToArray();
                        borderStyle = C_BORDER_STYLE.BS_DASHDED;
                    }

                    if (viewControl != null && viewControl.PDFViewTool != null)
                    {
                        AnnotHistory history = GetHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        
                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE)
                        {
                            CPDFSquareAnnotation squareAnnot = annotCore as CPDFSquareAnnotation;
                            if(squareAnnot == null || squareAnnot.Dash.SequenceEqual(dashArray))return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                            squareAnnot.SetBorderStyle(borderStyle, dashArray);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, squareAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE)
                        {
                            CPDFCircleAnnotation circleAnnot = annotCore as CPDFCircleAnnotation;
                            if (circleAnnot == null || circleAnnot.Dash.SequenceEqual(dashArray)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                            circleAnnot.SetBorderStyle(borderStyle, dashArray);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, circleAnnot);
                        }

                        if (annotCore.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                        {
                            CPDFLineAnnotation lineAnnot = annotCore as CPDFLineAnnotation;
                            if (lineAnnot == null || lineAnnot.Dash.SequenceEqual(dashArray)) return;
                            history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                            lineAnnot.SetBorderStyle(borderStyle, dashArray);
                            history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                        }

                        annotCore.UpdateAp();
                        viewControl.UpdateAnnotFrame();
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void CPDFArrowControl_ArrowChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(PropertyChanged, GetShapeData());
            }
            else
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    if(annotCore.Type==C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                    {
                        CPDFLineAnnotation lineAnnot= annotCore as CPDFLineAnnotation;
                        if(lineAnnot!=null && viewControl != null)
                        {
                            if(lineAnnot.HeadLineType != CPDFArrowControl.LineType.HeadLineType || lineAnnot.TailLineType != CPDFArrowControl.LineType.TailLineType)
                            {
                                LineAnnotHistory history = new LineAnnotHistory();
                                history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                                history.Action = HistoryAction.Update;
                                history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                                lineAnnot.SetLineType(CPDFArrowControl.LineType.HeadLineType, CPDFArrowControl.LineType.TailLineType);
                                lineAnnot.UpdateAp();
                                history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, lineAnnot);
                                viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                                viewControl.UpdateAnnotFrame();
                            }
                        }
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetShapeData());
            }
            else
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    if (viewControl != null && annotCore.GetContent()!=NoteTextBox.Text)
                    {
                        AnnotHistory history = GetHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, annotCore);
                        annotCore.SetContent(NoteTextBox.Text);
                        viewControl.UpdateAnnotFrame();
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, annotCore);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    }
                }
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

        public void SetPresentAnnotAttrib(AnnotParam param,CPDFAnnotation annot,PDFViewControl view)
        {
            annotParam = null;
            annotCore = null;
            viewControl = null;

            if(param != null)
            {
                if(param is SquareParam)
                {
                    SquareParam squareParam= (SquareParam)param;
                    CPDFThicknessControl.Thickness = (int)squareParam.LineWidth;

                    if (squareParam.LineColor != null)
                    {
                        BorderColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                            squareParam.LineColor[0],
                            squareParam.LineColor[1],
                            squareParam.LineColor[2]));
                        BorderColorPickerControl.SetCheckedForColor(Color.FromRgb(
                        squareParam.LineColor[0],
                        squareParam.LineColor[1],
                        squareParam.LineColor[2]));
                    }

                    if (squareParam.BgColor!=null)
                    {
                        FillColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                           squareParam.BgColor[0],
                           squareParam.BgColor[1],
                           squareParam.BgColor[2]));
                        FillColorPickerControl.SetCheckedForColor(Color.FromRgb(
                           squareParam.BgColor[0],
                           squareParam.BgColor[1],
                           squareParam.BgColor[2]));
                    }
                    if (squareParam.BorderStyle == C_BORDER_STYLE.BS_SOLID)
                    {
                        CPDFLineStyleControl.DashStyle = DashStyles.Solid;
                    }
                    else
                    {
                        List<double> dashArray = new List<double>();
                        foreach (double num in squareParam.LineDash)
                        {
                            dashArray.Add(num);
                        }
                        CPDFLineStyleControl.DashStyle = new DashStyle(dashArray, 0);
                    }
                }

                if(param is CircleParam)
                {
                    CircleParam circleParam= (CircleParam)param;
                    CPDFThicknessControl.Thickness = (int)circleParam.LineWidth;
                    if (circleParam.LineColor != null)
                    {
                        BorderColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                           circleParam.LineColor[0],
                           circleParam.LineColor[1],
                           circleParam.LineColor[2]));
                        BorderColorPickerControl.SetCheckedForColor(Color.FromRgb(
                           circleParam.LineColor[0],
                           circleParam.LineColor[1],
                           circleParam.LineColor[2]));
                    }
                    if (circleParam.BgColor!=null)
                    {
                        FillColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                           circleParam.BgColor[0],
                           circleParam.BgColor[1],
                           circleParam.BgColor[2]));
                        FillColorPickerControl.SetCheckedForColor(Color.FromRgb(
                           circleParam.BgColor[0],
                           circleParam.BgColor[1],
                           circleParam.BgColor[2]));
                    }
                    if (circleParam.BorderStyle == C_BORDER_STYLE.BS_SOLID)
                    {
                        CPDFLineStyleControl.DashStyle = DashStyles.Solid;
                    }
                    else
                    {
                        List<double> dashArray = new List<double>();
                        foreach (double num in circleParam.LineDash)
                        {
                            dashArray.Add(num);
                        }
                        CPDFLineStyleControl.DashStyle = new DashStyle(dashArray, 0);
                    }
                }

                if(param is LineParam)
                {
                    LineParam lineParam= (LineParam)param;
                    CPDFThicknessControl.Thickness = (int)lineParam.LineWidth;

                    if (lineParam.LineColor != null)
                    {
                        BorderColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                          lineParam.LineColor[0],
                          lineParam.LineColor[1],
                          lineParam.LineColor[2]));
                        BorderColorPickerControl.SetCheckedForColor(Color.FromRgb(
                       lineParam.LineColor[0],
                       lineParam.LineColor[1],
                       lineParam.LineColor[2]));
                    }
                    if (lineParam.BgColor != null)
                    {
                        FillColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                            lineParam.BgColor[0],
                            lineParam.BgColor[1],
                            lineParam.BgColor[2]));
                        FillColorPickerControl.SetCheckedForColor(Color.FromRgb(
                       lineParam.BgColor[0],
                       lineParam.BgColor[1],
                       lineParam.BgColor[2]));
                    }
                    if(lineParam.BorderStyle == C_BORDER_STYLE.BS_SOLID)
                    {
                        CPDFLineStyleControl.DashStyle = DashStyles.Solid;
                    }
                    else
                    {
                        List<double> dashArray = new List<double>();
                        foreach (double num in lineParam.LineDash)
                        {
                            dashArray.Add(num);
                        }
                        CPDFLineStyleControl.DashStyle = new DashStyle(dashArray, 0);
                    }

                    LineType lineType = new LineType()
                    {
                        HeadLineType = lineParam.HeadLineType,
                        TailLineType = lineParam.TailLineType
                    };
                    CPDFArrowControl.LineType = lineType;
                }

                CPDFOpacityControl.OpacityValue = (int)Math.Ceiling(param.Transparency * 100 / 255.0);
                NoteTextBox.Text = param.Content;
            }

            annotParam = param;
            annotCore = annot;
            viewControl = view;
            CPDFAnnotationPreviewerControl.DrawShapePreview(GetShapeData());
        }

        public void InitWhenRectAndRound()
        {
            FillColorStackPanel.Visibility = Visibility.Visible;
            ArrowStackPanel.Visibility = Visibility.Collapsed;

            FillColorPickerControl.ColorChanged -= FillColorPickerControl_ColorChanged;
            FillColorPickerControl.ColorChanged += FillColorPickerControl_ColorChanged;
            CPDFArrowControl.ArrowChanged -= CPDFArrowControl_ArrowChanged;
        }

        public void InitWhenArrowAndLine()
        {
            FillColorStackPanel.Visibility = Visibility.Collapsed;
            ArrowStackPanel.Visibility = Visibility.Visible;

            CPDFArrowControl.ArrowChanged -= CPDFArrowControl_ArrowChanged;
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
