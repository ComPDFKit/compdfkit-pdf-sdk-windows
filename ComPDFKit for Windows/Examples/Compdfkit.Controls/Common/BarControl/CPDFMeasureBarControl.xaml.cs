using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.Measure;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using static ComPDFKit.Controls.Helper.PanelState;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFMeasureBarControl : UserControl
    {
        #region Data

        private bool isFirstLoad = true;
        
        private static string line = LanguageHelper.ToolBarManager.GetString("Button_Line");
        private static string multiline = LanguageHelper.ToolBarManager.GetString("Button_Multiline");
        private static string polygonal = LanguageHelper.ToolBarManager.GetString("Button_Polygonal");
        private static string rectangles = LanguageHelper.ToolBarManager.GetString("Button_Rectangle");

        readonly Dictionary<string, string> buttonDict = new Dictionary<string, string>
        {
            {line,"M18.2782 5.75646L14.7427 2.22093L13.5052 3.45837L15.9802 5.93333L14.9195 6.99399L12.4446 4.51903L10.6768 6.28679L13.1518 8.76175L12.0911 9.82241L9.61615 7.34745L7.84838 9.11522L10.3233 11.5902L9.26268 12.6508L6.78772 10.1759L5.01996 11.9436L7.49492 14.4186L6.43426 15.4793L3.9593 13.0043L2.72186 14.2417L6.25739 17.7773L18.2782 5.75646ZM15.8033 1.16027L14.7427 0.0996094L13.682 1.16027L1.6612 13.1811L0.600539 14.2417L1.6612 15.3024L5.19673 18.8379L6.25739 19.8986L7.31805 18.8379L19.3389 6.81712L20.3995 5.75646L19.3389 4.6958L15.8033 1.16027Z"},
            {multiline, "M1 3C1 1.89543 1.89543 1 3 1C3.83934 1 4.55793 1.51704 4.85462 2.25H16.1454C16.4421 1.51704 17.1607 1 18 1C19.1046 1 20 1.89543 20 3C20 3.83934 19.483 4.55793 18.75 4.85462V17V17.75H18H14.2011L15.0167 18.5765L14.0138 19.5928L11.9986 17.5505L11.4972 17.0423L11.9986 16.5342L14.0138 14.4918L15.0167 15.5082L14.2847 16.25H17.25V4.85462C16.7487 4.65168 16.3483 4.25135 16.1454 3.75H4.85462C4.65168 4.25135 4.25135 4.65168 3.75 4.85462V16.25H6.75V15H8.25V19H6.75V17.75H3H2.25V17V4.85462C1.51704 4.55793 1 3.83934 1 3Z"},
            {polygonal,"M1.87111 7.6963L10.5 1.42705L19.1289 7.6963L15.8329 17.8402H5.16705L1.87111 7.6963Z"},
            {rectangles,"M20.25 2.25H0.75V17.75H20.25V2.25ZM18.75 10.75V16.25H2.25V10.75H18.75ZM18.75 9.25V3.75H2.25V9.25H18.75ZM6.5 5.5H3.5V7.5H6.5V5.5ZM3.5 12.5H6.5V14.5H3.5V12.5Z"}
        };


        private PDFViewControl pdfViewer;

        private MeasurePropertyControl measurePropertyControl = null;
        private MeasureControl measureControl = null;

        private enum MeasureType
        {
            UnKnown = -1,
            Line,
            Multiline,
            Polygonal,
            Rectangles
        }

        private PanelState panelState = PanelState.GetInstance();

        private MeasureType StringToType(string type)
        {
            if (type == line)
                return MeasureType.Line;
            if (type == multiline)
                return MeasureType.Multiline;
            if (type == polygonal)
                return MeasureType.Polygonal;
            if (type == rectangles)
                return MeasureType.Rectangles;

            return MeasureType.UnKnown;
        }


        #endregion

        #region Create Default UI

        public CPDFMeasureBarControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstLoad)
            {
                foreach (KeyValuePair<string, string> data in buttonDict)
                {
                    string Path = data.Value;
                    string name = data.Key;

                    Geometry annotationGeometry = Geometry.Parse(Path);
                    Path path = new Path
                    {
                        Width = 20,
                        Height = 20,
                        Data = annotationGeometry,
                        Fill = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D)),
                    };
                    if (name == LanguageHelper.ToolBarManager.GetString("Button_Polygonal"))
                    {
                        path.Fill = null;
                        path.Stroke = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D));
                        path.StrokeThickness = 1.5;
                    }
                    else if (name == LanguageHelper.ToolBarManager.GetString("Button_Rectangle"))
                    {
                        path.Fill = null;
                        path.Data = new RectangleGeometry(new Rect(3.25, 2.75, 14.5, 14.5));
                        path.Stroke = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D));
                        path.StrokeThickness = 1.5;
                    }
                    CreateButtonForPath(path, name);
                }
                isFirstLoad = false;
            }
        }

        private void CreateButtonForPath(Path path, String name)
        {
            StackPanel stackPanel = new StackPanel();
            TextBlock textBlock = new TextBlock();
            if (path != null)
            {
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                stackPanel.Children.Add(path);
            }
            if (!string.IsNullOrEmpty(name))
            {
                textBlock.Text = name;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Margin = new Thickness(8, 0, 0, 0);
                textBlock.FontSize = 12;

                stackPanel.Children.Add(textBlock);
            }

            Style style = (Style)FindResource("RoundMarginToggleButtonStyle");
            ToggleButton button = new ToggleButton();
            button.BorderThickness = new Thickness(0);
            button.Padding = new Thickness(10, 5, 10, 5);
            button.Tag = name;
            button.ToolTip = name;
            button.Style = style;
            button.Content = stackPanel;
            button.Click += MeasureBtn_Click;
            MeasureGrid.Children.Add(button);
        }

        #endregion

        #region Even Process

        public void InitWithPDFViewer(PDFViewControl pdfViewer, MeasurePropertyControl FromProperty, MeasureControl parentControl)
        {
            this.pdfViewer = pdfViewer;
            measurePropertyControl = FromProperty;
            measureControl = parentControl;
        }

        public void ClearAllToolState()
        {
            foreach (UIElement child in MeasureGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        public bool ToolChecked()
        {
            foreach (UIElement child in MeasureGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    if (toggle.IsChecked==true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ClearToolState(UIElement sender)
        {
            foreach (UIElement child in MeasureGrid.Children)
            {
                if (child is ToggleButton toggle && (child as ToggleButton) != (sender as ToggleButton))
                {
                    toggle.IsChecked = false;
                }
            }
        }

        private void MeasureBtn_Click(object sender, RoutedEventArgs e)
        {
            AnnotParam annotParam = null;
            ClearToolState(sender as ToggleButton);
            if ((bool)(sender as ToggleButton).IsChecked)
            {
                switch (StringToType((sender as ToggleButton).Tag.ToString()))
                {
                    case MeasureType.UnKnown:
                        break;
                    case MeasureType.Line:
                        annotParam = CreateLine();
                        break;
                    case MeasureType.Multiline:
                        annotParam = CreateMultiline();
                        break;
                    case MeasureType.Polygonal:
                        annotParam = CreatePolygonal();
                        break;
                    case MeasureType.Rectangles:
                        annotParam = CreateRectangles();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                pdfViewer.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_NONE);
                pdfViewer.SetToolType(ToolType.Pan);
                measureControl.SetInfoPanelVisble(false, false);
            }
            if (annotParam != null)
            {
                panelState.RightPanel = RightPanelState.PropertyPanel;
            }
            measurePropertyControl.SetPropertyForMeasureCreate(annotParam, null, pdfViewer);
        }

        #endregion

        #region Create Form

        private AnnotParam CreateLine()
        {
            pdfViewer.SetToolType(ToolType.CreateAnnot);
            pdfViewer.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_LINE);
            LineMeasureParam lineMeasureParam = new LineMeasureParam();
            lineMeasureParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_LINE;
            lineMeasureParam.LineColor = new byte[] { 255, 0, 0, };
            lineMeasureParam.LineWidth = 2;
            lineMeasureParam.Transparency = 255;
            lineMeasureParam.FontColor = new byte[] { 255, 0, 0, };
            lineMeasureParam.FontName = "Arial";
            lineMeasureParam.FontSize = 14;
            lineMeasureParam.HeadLineType = C_LINE_TYPE.LINETYPE_ARROW;
            lineMeasureParam.TailLineType = C_LINE_TYPE.LINETYPE_ARROW;
            lineMeasureParam.measureInfo = new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH,
            };
            pdfViewer.SetAnnotParam(lineMeasureParam);
            //pdfViewer?.ClearSelectAnnots();
            //pdfViewer?.SetMouseMode(MouseModes.AnnotCreate);
            //pdfViewer?.SetToolParam(lineMeasureArgs);
            measureControl.SetMeasureInfoType(CPDFMeasureType.CPDF_DISTANCE_MEASURE);
            measureControl.SetInfoPanelVisble(true, false);
            var measureSetting = pdfViewer.PDFViewTool.GetMeasureSetting();
            measureControl.SetMeasureScale(CPDFMeasureType.CPDF_DISTANCE_MEASURE,
               string.Format("{0} {1} = {2} {3}",
                   measureSetting.RulerBase,
                   measureSetting.RulerBaseUnit,
                   measureSetting.RulerTranslate,
                   measureSetting.RulerTranslateUnit));
            return lineMeasureParam;
        }

        private AnnotParam CreateMultiline()
        {
            pdfViewer.SetToolType(ToolType.CreateAnnot);
            pdfViewer.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE);
            PolyLineMeasureParam polyLineMeasureParam = new PolyLineMeasureParam();
            polyLineMeasureParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE;
            polyLineMeasureParam.LineColor = new byte[] { 255, 0, 0, };
            polyLineMeasureParam.LineWidth = 2;
            polyLineMeasureParam.Transparency = 255;
            polyLineMeasureParam.FontColor = new byte[] { 255, 0, 0, };
            polyLineMeasureParam.FontName = "Arial";
            polyLineMeasureParam.FontSize = 14;
            polyLineMeasureParam.measureInfo = new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH,
            };
            pdfViewer.SetAnnotParam(polyLineMeasureParam);
            //PolyLineMeasureArgs polyLineMeasureArgs = new PolyLineMeasureArgs();
            //polyLineMeasureArgs.LineColor = Colors.Red;
            //polyLineMeasureArgs.LineWidth = 2;
            //polyLineMeasureArgs.Transparency = 1;
            //polyLineMeasureArgs.FontColor = Colors.Red;
            //polyLineMeasureArgs.FontName = "Arial";
            //polyLineMeasureArgs.FontSize = 14;
            // pdfViewer?.ClearSelectAnnots();
            // pdfViewer?.SetMouseMode(MouseModes.AnnotCreate);
            // pdfViewer?.SetToolParam(polyLineMeasureArgs);
            measureControl.SetMeasureInfoType(CPDFMeasureType.CPDF_PERIMETER_MEASURE);
            measureControl.SetInfoPanelVisble(true, false);
            var measureSetting = pdfViewer.PDFViewTool.GetMeasureSetting();
            measureControl.SetMeasureScale(CPDFMeasureType.CPDF_PERIMETER_MEASURE,
               string.Format("{0} {1} = {2} {3}",
                   measureSetting.RulerBase,
                   measureSetting.RulerBaseUnit,
                   measureSetting.RulerTranslate,
                   measureSetting.RulerTranslateUnit));
            return polyLineMeasureParam;
        }

        private AnnotParam CreatePolygonal()
        {
            pdfViewer.PDFViewTool.GetDefaultSettingParam().IsCreateSquarePolygonMeasure = false;
            pdfViewer.SetToolType(ToolType.CreateAnnot);
            pdfViewer.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON);
            PolygonMeasureParam polygonMeasureParam = new PolygonMeasureParam();
            polygonMeasureParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON;
            polygonMeasureParam.LineColor = new byte[] { 255, 0, 0, };
            polygonMeasureParam.LineWidth = 2;
            polygonMeasureParam.Transparency = 255;
            polygonMeasureParam.FontColor = new byte[] { 255, 0, 0, };
            polygonMeasureParam.FontName = "Arial";
            polygonMeasureParam.FontSize = 14;
            polygonMeasureParam.measureInfo = new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH | CPDFCaptionType.CPDF_CAPTION_AREA,
            };
            pdfViewer.SetAnnotParam(polygonMeasureParam);
            //    PolygonMeasureArgs polygonMeasureArgs = new PolygonMeasureArgs();
            //    polygonMeasureArgs.LineColor = Colors.Red;
            //    polygonMeasureArgs.LineWidth = 2;
            //    polygonMeasureArgs.Transparency = 1;
            //    polygonMeasureArgs.FontColor = Colors.Red;
            //    polygonMeasureArgs.FillColor = Colors.Transparent;
            //    polygonMeasureArgs.FontName = "Arial";
            //    polygonMeasureArgs.FontSize = 14;
            //    pdfViewer?.ClearSelectAnnots();
            //    pdfViewer?.SetMouseMode(MouseModes.AnnotCreate);
            //    pdfViewer?.SetToolParam(polygonMeasureArgs);
            measureControl.SetMeasureInfoType(CPDFMeasureType.CPDF_AREA_MEASURE);
            measureControl.SetInfoPanelVisble(true, false);
            var measureSetting = pdfViewer.PDFViewTool.GetMeasureSetting();
            measureControl.SetMeasureScale(CPDFMeasureType.CPDF_AREA_MEASURE,
                string.Format("{0} {1} = {2} {3}",
                    measureSetting.RulerBase,
                    measureSetting.RulerBaseUnit,
                    measureSetting.RulerTranslate,
                    measureSetting.RulerTranslateUnit));
            //    return polygonMeasureArgs;
            return polygonMeasureParam;
        }

        private AnnotParam CreateRectangles()
        {
            pdfViewer.PDFViewTool.GetDefaultSettingParam().IsCreateSquarePolygonMeasure = true;
            pdfViewer.SetToolType(ToolType.CreateAnnot);
            pdfViewer.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON);
            PolygonMeasureParam polygonMeasureParam = new PolygonMeasureParam();
            polygonMeasureParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON;
            polygonMeasureParam.LineColor = new byte[] { 255, 0, 0, };
            polygonMeasureParam.LineWidth = 2;
            polygonMeasureParam.Transparency = 255;
            polygonMeasureParam.FontColor = new byte[] { 255, 0, 0, };
            polygonMeasureParam.FontName = "Arial";
            polygonMeasureParam.FontSize = 14;
            polygonMeasureParam.measureInfo = new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH | CPDFCaptionType.CPDF_CAPTION_AREA,
            };
            pdfViewer.SetAnnotParam(polygonMeasureParam);
            measureControl.SetMeasureInfoType(CPDFMeasureType.CPDF_AREA_MEASURE);
            //    PolygonMeasureArgs rectPolygonMeasureArgs = new PolygonMeasureArgs();
            //    rectPolygonMeasureArgs.LineColor = Colors.Red;
            //    rectPolygonMeasureArgs.IsOnlyDrawRect = true;
            //    rectPolygonMeasureArgs.LineWidth = 2;
            //    rectPolygonMeasureArgs.Transparency = 1;
            //    rectPolygonMeasureArgs.FontColor = Colors.Red;
            //    rectPolygonMeasureArgs.FillColor = Colors.Transparent;
            //    rectPolygonMeasureArgs.FontName = "Arial";
            //    rectPolygonMeasureArgs.FontSize = 14;
            //    pdfViewer?.ClearSelectAnnots();
            //    pdfViewer?.SetMouseMode(MouseModes.AnnotCreate);
            //    pdfViewer?.SetToolParam(rectPolygonMeasureArgs);
            //    measureControl.SetMeasureInfoType(CPDFMeasureType.CPDF_AREA_MEASURE);
            //    measureControl.SetInfoPanelVisble(true, false);
            //    measureControl.SetMeasureScale(CPDFMeasureType.CPDF_AREA_MEASURE,
            //     string.Format("{0} {1} = {2} {3}",
            //                 MeasureSetting.RulerBase,
            //                 MeasureSetting.RulerBaseUnit,
            //                 MeasureSetting.RulerTranslate,
            //                 MeasureSetting.RulerTranslateUnit));
            //    return rectPolygonMeasureArgs;
            return polygonMeasureParam;
        }

#endregion
    }
}