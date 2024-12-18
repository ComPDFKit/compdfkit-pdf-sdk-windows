using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace ComPDFKit.Controls.PDFControlUI
{
    /// <summary>
    /// Interaction logic for CPDFCloudUI.xaml
    /// </summary>
    public partial class CPDFCloudUI : UserControl
    {
        bool IsLoadedData = false;

        private AnnotParam annotParam;
        private CPDFAnnotation annotCore;
        private PDFViewControl viewControl;
        public event EventHandler<CPDFAnnotationData> PropertyChanged;

        public CPDFCloudUI()
        {
            InitializeComponent();

            ctlBorderColorPicker.ColorChanged -= CtlBorderColorPicker_ColorChanged;
            ctlFillColorPicker.ColorChanged -= CtlFillColorPicker_ColorChanged;
            CPDFOpacityControl.OpacityChanged -= CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged -= CPDFThicknessControl_ThicknessChanged;
            CPDFLineShapeControl.LineShapeChanged -= CPDFLineShapeControl_LineShapeChanged;
            ctlLineStyle.LineStyleChanged -= CtlLineStyle_LineStyleChanged;

            ctlBorderColorPicker.ColorChanged += CtlBorderColorPicker_ColorChanged;
            ctlFillColorPicker.ColorChanged += CtlFillColorPicker_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged += CPDFThicknessControl_ThicknessChanged;
            CPDFLineShapeControl.LineShapeChanged += CPDFLineShapeControl_LineShapeChanged;
            ctlLineStyle.LineStyleChanged += CtlLineStyle_LineStyleChanged;

            CPDFAnnotationPreviewerControl.DrawCloudPreview();
        }

        private void CtlLineStyle_LineStyleChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetPolygonData());
            }
            else
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    float[] dashArray = null;
                    C_BORDER_STYLE borderStyle;
                    if (ctlLineStyle.DashStyle == DashStyles.Solid || ctlLineStyle.DashStyle == null)
                    {
                        dashArray = new float[0];
                        borderStyle = C_BORDER_STYLE.BS_SOLID;
                    }
                    else
                    {
                        List<float> floatArray = new List<float>();
                        foreach (double num in ctlLineStyle.DashStyle.Dashes)
                        {
                            floatArray.Add((float)num);
                        }
                        dashArray = floatArray.ToArray();
                        borderStyle = C_BORDER_STYLE.BS_DASHDED;
                    }

                    if (viewControl != null && viewControl.PDFViewTool != null)
                    {
                        PolygonMeasureAnnotHistory history = new PolygonMeasureAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        CPDFPolygonAnnotation polygonAnnotation = annotCore as CPDFPolygonAnnotation;
                        if (polygonAnnotation == null || polygonAnnotation.Dash.SequenceEqual(dashArray)) return;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);
                        polygonAnnotation.SetBorderStyle(borderStyle, dashArray);
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);

                        annotCore.UpdateAp();
                        viewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        private void CPDFLineShapeControl_LineShapeChanged(object sender, CPDFBorderEffector e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetPolygonData());
            }
            if (IsLoadedData)
            {
                if (annotCore != null && annotCore.IsValid() && annotCore is CPDFPolygonAnnotation polygonAnnotation)
                {
                    PolygonMeasureAnnotHistory history = new PolygonMeasureAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);
                    polygonAnnotation.SetAnnotBorderEffector(e);
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);

                    annotCore.UpdateAp();
                    viewControl.UpdateAnnotFrame();
                }
            }
        }

        private void CPDFThicknessControl_ThicknessChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetPolygonData());
            }
            if (IsLoadedData)
            {
                if (annotCore != null && annotCore.IsValid() && annotCore is CPDFPolygonAnnotation polygonAnnotation)
                {
                    PolygonMeasureAnnotHistory history = new PolygonMeasureAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, annotCore);
                    polygonAnnotation.SetLineWidth((sender as CPDFThicknessControl).Thickness);
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, annotCore);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);

                    annotCore.UpdateAp();
                    viewControl.UpdateAnnotFrame();
                }
            }
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetPolygonData());
            }
            if (IsLoadedData)
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    double opacity = (sender as CPDFOpacityControl).OpacityValue / 100.0;
                    if (opacity > 0 && opacity <= 1)
                    {
                        opacity *= 255;
                    }

                    PolygonMeasureAnnotHistory history = new PolygonMeasureAnnotHistory();
                    history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, annotCore);
                    annotCore.SetTransparency((byte)opacity);
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, annotCore);
                    viewControl.GetCPDFViewer().UndoManager.AddHistory(history);

                    annotCore.UpdateAp();
                    viewControl.UpdateAnnotFrame();
                }
            }
        }

        private void CtlFillColorPicker_ColorChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetPolygonData());
            }
            if (IsLoadedData)
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    if (annotCore is CPDFPolygonAnnotation polygonAnnotation)
                    {
                        PolygonMeasureAnnotHistory history = new PolygonMeasureAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);

                        SolidColorBrush brush = (sender as ColorPickerControl)?.Brush as SolidColorBrush;
                        polygonAnnotation.SetBgColor(new byte[3]
                        {
                            brush.Color.R,
                            brush.Color.G,
                            brush.Color.B
                        });

                        if(brush.Color.A == 0)
                        {
                            polygonAnnotation.ClearBgColor();
                        }

                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);

                        annotCore.UpdateAp();
                        viewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        private void CtlBorderColorPicker_ColorChanged(object sender, EventArgs e)
        {
            if (annotParam == null)
            {
                PropertyChanged?.Invoke(this, GetPolygonData());
            }
            if (IsLoadedData)
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    if (annotCore is CPDFPolygonAnnotation polygonAnnotation)
                    {
                        PolygonMeasureAnnotHistory history = new PolygonMeasureAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);

                        SolidColorBrush brush = (sender as ColorPickerControl)?.Brush as SolidColorBrush;
                        polygonAnnotation.SetLineColor(new byte[3]
                        {
                            brush.Color.R,
                            brush.Color.G,
                            brush.Color.B
                        });

                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, annotCore.Page.PageIndex, polygonAnnotation);
                        
                        annotCore.UpdateAp();
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                        viewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        public void SetPresentAnnotAttrib(PolygonMeasureParam polygonParam, CPDFPolygonAnnotation annotation, PDFViewControl view, int PageCount)
        {
            annotParam = polygonParam;
            annotCore = annotation;
            viewControl = view;
            if (polygonParam == null)
            {
                return;
            }

            Color lineColor = Color.FromRgb(polygonParam.LineColor[0], polygonParam.LineColor[1], polygonParam.LineColor[2]);
            Color fillColor;
            if (annotation.HasBgColor)
            {
                fillColor = Color.FromRgb(polygonParam.FillColor[0], polygonParam.FillColor[1], polygonParam.FillColor[2]);
            }
            else
            {
                fillColor = Colors.Transparent;
            }

            ctlBorderColorPicker.SetCheckedForColor(lineColor);
            ctlFillColorPicker.SetCheckedForColor(fillColor);

            double opacity = polygonParam.Transparency / 255.0 * 100.0;
            CPDFOpacityControl.OpacityValue = (int)Math.Ceiling(opacity);

            float thickness = polygonParam.LineWidth;
            CPDFThicknessControl.Thickness = (int)Math.Ceiling(thickness);

            CPDFLineShapeControl.BorderEffector = annotation.GetAnnotBorderEffector();
            if (polygonParam.BorderStyle == C_BORDER_STYLE.BS_SOLID)
            {
                ctlLineStyle.DashStyle = DashStyles.Solid;
            }
            else
            {
                List<double> dashArray = new List<double>();
                foreach (double num in polygonParam.LineDash)
                {
                    dashArray.Add(num);
                }
                ctlLineStyle.DashStyle = new DashStyle(dashArray, 0);
            }

            NoteTextBox.Text = polygonParam.Content;
        }

        public CPDFAnnotationData GetPolygonData()
        {
            CPDFPolygonData polygonData = new CPDFPolygonData
            {
                AnnotationType = CPDFAnnotationType.Polygon,
                BorderColor = ((SolidColorBrush)ctlBorderColorPicker.Brush).Color,
                FillColor = ((SolidColorBrush)ctlFillColorPicker.Brush).Color,
                BorderEffector = CPDFLineShapeControl.BorderEffector,
                DashStyle = ctlLineStyle.DashStyle, 
                IsMeasured = false,
                Thickness = CPDFThicknessControl.Thickness,
                Opacity = CPDFOpacityControl.OpacityValue,
                Note = NoteTextBox.Text
            };

            return polygonData;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoadedData = true;
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (annotCore != null && annotCore.IsValid())
                {
                    annotCore.SetContent(NoteTextBox.Text);
                    annotCore.UpdateAp();
                    viewControl?.UpdateAnnotFrame();
                }
            }
        }
    }
}
