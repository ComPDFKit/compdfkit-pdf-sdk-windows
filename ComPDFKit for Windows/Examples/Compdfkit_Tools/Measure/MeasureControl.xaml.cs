using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace Compdfkit_Tools.Measure
{
    /// <summary>
    /// MeasureControl.xaml 的交互逻辑
    /// </summary>
    public partial class MeasureControl : UserControl
    {
        public MeasurePropertyControl measurePropertyControl = new MeasurePropertyControl();
        private CPDFDisplaySettingsControl displaySettingsControl;

        private PDFViewControl PdfViewControl = new PDFViewControl();

        private PanelState panelState = PanelState.GetInstance();

        public event EventHandler ExpandEvent;

        public MeasureControl()
        {
            InitializeComponent();
            MeasureSetting.MeasureChanged += MeasureSetting_MeasureChanged;
        }

        private void MeasureSetting_MeasureChanged(object sender, MeasureEventArgs e)
        {
            InfoPanel.SetMeasureType(e.Type);
            InfoPanel.SetMeasureInfo(e);

            switch (e.Type)
            {
                case CPDFMeasureType.CPDF_DISTANCE_MEASURE:
                case CPDFMeasureType.CPDF_PERIMETER_MEASURE:
                    SettingPanel.ShowAreaAndLength(Visibility.Collapsed);
                    break;
                case CPDFMeasureType.CPDF_AREA_MEASURE:
                    SettingPanel.ShowAreaAndLength(Visibility.Visible);
                    break;
                default:
                    break;
            }

            SettingPanel.BindMeasureSetting(e);
        }
        #region Init PDFViewer

        public void InitWithPDFViewer(PDFViewControl pdfViewControl, CPDFViewer pdfViewer)
        {
            PdfViewControl = pdfViewControl;
            PdfViewControl.PDFView = pdfViewer;
            PDFMeasureTool.InitWithPDFViewer(pdfViewer, measurePropertyControl, this);
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            PDFGrid.Child = PdfViewControl;

            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
            pdfViewControl.PDFView.AnnotActiveHandler -= PDFView_AnnotActiveHandler;
            pdfViewControl.PDFView.AnnotActiveHandler += PDFView_AnnotActiveHandler;
            pdfViewControl.PDFView.AnnotCommandHandler -= PDFView_AnnotCommandHandler;
            pdfViewControl.PDFView.AnnotCommandHandler += PDFView_AnnotCommandHandler;
            SetInfoPanelVisble(false, false);
            SettingPanel.PdfViewControl= pdfViewControl;
        }

        private void PDFView_AnnotCommandHandler(object sender, AnnotCommandArgs e)
        {
            switch (e.CommandType)
            {
                case CommandType.Context:
                    if (e.CommandTarget == TargetType.Annot && e.PressOnAnnot)
                    {
                        e.Handle = true;
                        e.PopupMenu = new ContextMenu();
                        e.PopupMenu.Items.Add(new MenuItem() { Header = "Delete", Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                        MenuItem menuItem = new MenuItem();
                        menuItem.Header = "Measurement Settings";
                        menuItem.Click += (item, param) =>
                        {
                            SettingPanel.UpdateArgsList = e.AnnotEventArgsList;
                            SettingPanel.BindMeasureSetting();
                            SetInfoPanelVisble(false, true);
                        };

                        MenuItem propertyItem = new MenuItem();
                        propertyItem.Header = "Properties";
                        propertyItem.Click += (item, param) =>
                        {
                            ExpandEvent?.Invoke(this, new EventArgs());
                        };
                        e.PopupMenu.Items.Add(menuItem);
                        e.PopupMenu.Items.Add(propertyItem);
                    }
                    break;

                case CommandType.Delete:
                    e.DoCommand();
                    break;
                default:
                    break;
            }
        }



        private void PDFView_AnnotActiveHandler(object sender, AnnotAttribEvent e)
        {
            if (e == null || e.IsAnnotCreateReset)
            {
                if (e == null)
                {
                    measurePropertyControl?.ClearMeasurePanel();
                    if(PDFMeasureTool.ToolChecked()==false)
                    {
                        SetInfoPanelVisble(false, false);
                    }
                }
                return;
            }
            else
            {
                switch (e.GetAnnotTypes())
                {
                    case AnnotArgsType.LineMeasure:
                        LineMeasureArgs LineArgs = e.GetAnnotHandlerEventArgs(AnnotArgsType.LineMeasure).First() as LineMeasureArgs;
                        panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                        measurePropertyControl.SetPropertyForMeasureCreate(LineArgs, e);
                        SetInfoPanelVisble(true, false);
                        SetMeasureInfoPanel(LineArgs.GetPDFAnnot(), LineArgs);
                        break;
                    case AnnotArgsType.PolygonMeasure:
                        PolygonMeasureArgs polygonArgs = e.GetAnnotHandlerEventArgs(AnnotArgsType.PolygonMeasure).First() as PolygonMeasureArgs;
                        panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                        measurePropertyControl.SetPropertyForMeasureCreate(polygonArgs, e);
                        SetInfoPanelVisble(true, false);
                        SetMeasureInfoPanel(polygonArgs.GetPDFAnnot(),polygonArgs);
                        break;

                    case AnnotArgsType.PolyLineMeasure:
                        PolyLineMeasureArgs polyLineArgs = e.GetAnnotHandlerEventArgs(AnnotArgsType.PolyLineMeasure).First() as PolyLineMeasureArgs;
                        panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
                        measurePropertyControl.SetPropertyForMeasureCreate(polyLineArgs, e);
                        SetInfoPanelVisble(true, false);
                        SetMeasureInfoPanel(polyLineArgs.GetPDFAnnot(),polyLineArgs);
                        break;
                }
            }
        }

        private void SetMeasureInfoPanel(CPDFAnnotation rawAnnot,AnnotHandlerEventArgs annotArgs=null)
        {
            if (rawAnnot == null)
            {
                return;
            }
            try
            {
                if (rawAnnot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                {
                    CPDFLineAnnotation lineAnnot = rawAnnot as CPDFLineAnnotation;
                    if (lineAnnot.IsMersured() && lineAnnot.Points != null && lineAnnot.Points.Count() == 2)
                    {
                        CPDFDistanceMeasure lineMeasure = lineAnnot.GetDistanceMeasure();
                        CPDFMeasureInfo measureInfo = lineMeasure.MeasureInfo;
                        Vector standVector = new Vector(1, 0);
                        Point startPoint = new Point(lineAnnot.Points[0].x, lineAnnot.Points[0].y);
                        Point endPoint = new Point(lineAnnot.Points[1].x, lineAnnot.Points[1].y);
                        Vector movevector = endPoint - startPoint;

                        double showLenght=lineMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_LENGTH);
                        MeasureEventArgs measureEvent = new MeasureEventArgs();
                        measureEvent.Angle = (int)Math.Abs(Vector.AngleBetween(movevector, standVector));
                        measureEvent.RulerTranslateUnit = measureInfo.RulerTranslateUnit;
                        measureEvent.RulerTranslate = measureInfo.RulerTranslate;
                        measureEvent.RulerBase = measureInfo.RulerBase;
                        measureEvent.RulerBaseUnit = measureInfo.RulerBaseUnit;
                        measureEvent.MousePos = new Point(
                           (int)Math.Abs(movevector.X),
                           (int)Math.Abs(movevector.Y));
                        measureEvent.Type = CPDFMeasureType.CPDF_DISTANCE_MEASURE;
                        NumberFormatInfo formatInfo = new NumberFormatInfo();
                        formatInfo.NumberDecimalDigits = Math.Abs(measureInfo.Precision).ToString().Length - 1;
                        measureEvent.Distance = showLenght.ToString("N", formatInfo) + " " + measureInfo.RulerTranslateUnit;
                        measureEvent.Precision = GetMeasureShowPrecision(measureInfo.Precision);

                        MeasureSetting.InvokeMeasureChangeEvent(this, measureEvent);
                        if(annotArgs!=null)
                        {
                            SettingPanel.UpdateArgsList =new List<AnnotHandlerEventArgs> { annotArgs};
                        }
                    }
                }

                if (rawAnnot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE)
                {
                    CPDFPolylineAnnotation polylineAnnot = rawAnnot as CPDFPolylineAnnotation;
                    if (polylineAnnot.IsMersured() && polylineAnnot.Points != null && polylineAnnot.Points.Count() >= 2)
                    {
                        double totalInch = 0;
                        for (int i = 0; i < polylineAnnot.Points.Count - 1; i++)
                        {
                            Point endLinePoint = new Point(
                                polylineAnnot.Points[i + 1].x,
                                polylineAnnot.Points[i + 1].y
                                );
                            Point startLinePoint = new Point(
                                polylineAnnot.Points[i].x,
                                polylineAnnot.Points[i].y
                                );
                            Vector subVector = endLinePoint - startLinePoint;
                            totalInch += subVector.Length;
                        }
                        totalInch = totalInch / 72D;
                        CPDFPerimeterMeasure lineMeasure = polylineAnnot.GetPerimeterMeasure();
                        CPDFMeasureInfo measureInfo = lineMeasure.MeasureInfo;
                        double showLenght = lineMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_LENGTH);

                        MeasureEventArgs measureEvent = new MeasureEventArgs();
                        measureEvent.Angle = 0;
                        measureEvent.RulerTranslateUnit = measureInfo.RulerTranslateUnit;
                        measureEvent.RulerTranslate = measureInfo.RulerTranslate;
                        measureEvent.RulerBase = measureInfo.RulerBase;
                        measureEvent.RulerBaseUnit = measureInfo.RulerBaseUnit;
                        measureEvent.Precision = GetMeasureShowPrecision(measureInfo.Precision);
                        measureEvent.Type = CPDFMeasureType.CPDF_PERIMETER_MEASURE;
                        NumberFormatInfo formatInfo = new NumberFormatInfo();
                        formatInfo.NumberDecimalDigits = Math.Abs(measureInfo.Precision).ToString().Length - 1;
                        measureEvent.Distance = showLenght.ToString("N", formatInfo) +" "+ measureInfo.RulerTranslateUnit;
                        MeasureSetting.InvokeMeasureChangeEvent(this, measureEvent);
                        if (annotArgs != null)
                        {
                            SettingPanel.UpdateArgsList = new List<AnnotHandlerEventArgs> { annotArgs };
                        }
                    }
                }

                if(rawAnnot.Type== C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON)
                {
                    CPDFPolygonAnnotation Annot = rawAnnot as CPDFPolygonAnnotation;
                    CPDFAreaMeasure polygonMeasure = Annot.GetAreaMeasure();
                    CPDFMeasureInfo measureInfo = polygonMeasure.MeasureInfo;
                    CPDFCaptionType CaptionType = measureInfo.CaptionType;
                    bool IsArea = false;
                    bool IsLength = false;
                    if ((CaptionType& CPDFCaptionType.CPDF_CAPTION_AREA)== CPDFCaptionType.CPDF_CAPTION_AREA)
                    {
                        IsArea = true;
                    }
                    if ((CaptionType & CPDFCaptionType.CPDF_CAPTION_LENGTH) == CPDFCaptionType.CPDF_CAPTION_LENGTH)
                    {
                        IsLength = true;
                    }
                    SettingPanel.ChangedCheckBoxIsChecked(IsArea, IsLength);

                    double inch = polygonMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_AREA);
                    double currentInch = polygonMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_LENGTH);

                    MeasureEventArgs measureEvent = new MeasureEventArgs();
                    measureEvent.RulerTranslateUnit = measureInfo.RulerTranslateUnit;
                    measureEvent.RulerTranslate = measureInfo.RulerTranslate;
                    measureEvent.RulerBase = measureInfo.RulerBase;
                    measureEvent.RulerBaseUnit = measureInfo.RulerBaseUnit;
                    measureEvent.Precision = GetMeasureShowPrecision(measureInfo.Precision);
                    measureEvent.Type = CPDFMeasureType.CPDF_AREA_MEASURE;


                    NumberFormatInfo formatInfo = new NumberFormatInfo();
                    formatInfo.NumberDecimalDigits = Math.Abs(measureInfo.Precision).ToString().Length - 1;

                    measureEvent.Distance = currentInch.ToString("N", formatInfo) + " " + measureInfo.RulerTranslateUnit;
                    measureEvent.Area = inch.ToString("N", formatInfo) + " sq " + measureInfo.RulerTranslateUnit;

                    MeasureSetting.InvokeMeasureChangeEvent(this, measureEvent);
                    if (annotArgs != null)
                    {
                        SettingPanel.UpdateArgsList = new List<AnnotHandlerEventArgs> { annotArgs };
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private double GetMeasureRatio(string baseUnit)
        {
            if (baseUnit == CPDFMeasure.CPDF_PT)
            {
                return 1 / 72;
            }
            if (baseUnit == CPDFMeasure.CPDF_IN)
            {
                return 1;
            }
            if (baseUnit == CPDFMeasure.CPDF_MM)
            {
                return 1 / 25.4;
            }
            if (baseUnit == CPDFMeasure.CPDF_CM)
            {
                return 1 / 2.54;
            }
            if (baseUnit == CPDFMeasure.CPDF_M)
            {
                return 1 / 0.0254;
            }
            if (baseUnit == CPDFMeasure.CPDFO_KM)
            {
                return 1 / 0.0254 / 1000;
            }

            if (baseUnit == CPDFMeasure.CPDF_FT)
            {
                return 12;
            }
            if (baseUnit == CPDFMeasure.CPDF_YD)
            {
                return 36;
            }
            if (baseUnit == CPDFMeasure.CPDF_MI)
            {
                return 63360;
            }
            return 0;
        }

        private double GetMeasureShowPrecision(int precision)
        {
            if (precision == CPDFMeasure.PRECISION_VALUE_ZERO)
            {
                return 1;
            }
            if (CPDFMeasure.PRECISION_VALUE_ONE == precision)
            {
                return 0.1;
            }
            if (CPDFMeasure.PRECISION_VALUE_TWO == precision)
            {
                return 0.01;
            }
            if (CPDFMeasure.PRECISION_VALUE_THREE == precision)
            {
                return 0.001;
            }
            if (CPDFMeasure.PRECISION_VALUE_FOUR == precision)
            {
                return 0.0001;
            }
            return 0;
        }

        public void SetSettingsControl(CPDFDisplaySettingsControl cPDFDisplaySettingsControl)
        {
            displaySettingsControl = cPDFDisplaySettingsControl;
        }

        public void ClearAllToolState()
        {
            PDFMeasureTool.ClearAllToolState();
        }
        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
        }

        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PanelState.IsLeftPanelExpand))
            {
                ExpandLeftPanel(panelState.IsLeftPanelExpand);
            }
            else if (e.PropertyName == nameof(PanelState.RightPanel))
            {
                if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
                {
                    ExpandRightPropertyPanel(measurePropertyControl, Visibility.Visible);
                }
                else if (panelState.RightPanel == PanelState.RightPanelState.ViewSettings)
                {
                    ExpandRightPropertyPanel(displaySettingsControl, Visibility.Visible);
                }
                else
                {
                    ExpandRightPropertyPanel(null, Visibility.Collapsed);
                }
            }
        }

        #endregion

        #region Expand and collapse Panel

        public void ExpandRightPropertyPanel(Visibility visible)
        {
            ExpandRightPropertyPanel(measurePropertyControl, visible);
        }

        public void ExpandNullRightPropertyPanel(Visibility visible)
        {
            ExpandRightPropertyPanel(null, visible);
        }

        public void ExpandViewSettings(Visibility visible)
        {
            SetViewSettings(displaySettingsControl, visible);
        }

        private void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible;
        }

        private void SetViewSettings(CPDFDisplaySettingsControl displaySettingsControl, Visibility visibility)
        {
            PropertyContainer.Child = displaySettingsControl;
            PropertyContainer.Visibility = visibility;
        }

        public void ExpandLeftPanel(bool isExpand)
        {
            BotaContainer.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            Splitter.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            if (isExpand)
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(320);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(15);
            }
            else
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(0);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MeasureInfoPanel_SettingClick(object sender, EventArgs e)
        {
            if (sender is MeasureInfoPanel)
            {
                switch ((sender as MeasureInfoPanel).MeasureType)
                {
                    case CPDFMeasureType.CPDF_DISTANCE_MEASURE:
                    case CPDFMeasureType.CPDF_PERIMETER_MEASURE:
                        SettingPanel.ShowAreaAndLength(Visibility.Collapsed);
                        break;
                    case CPDFMeasureType.CPDF_AREA_MEASURE:
                        SettingPanel.ShowAreaAndLength(Visibility.Visible);
                        break;
                    default:
                        break;
                }
            }
            SettingPanel.ReturnToInfoPanel = true;
            SetInfoPanelVisble(false, true);
            SettingPanel.BindMeasureSetting();
        }

        private void SettingPanel_CancelEvent(object sender, EventArgs e)
        {
            SetInfoPanelVisble(SettingPanel.ReturnToInfoPanel, false);
        }

        private void SettingPanel_DoneEvent(object sender, EventArgs e)
        {
            SetInfoPanelVisble(SettingPanel.ReturnToInfoPanel, false);
        }

        public void SetInfoPanelVisble(bool measureInfo, bool measureSetting)
        {
            if (measureInfo)
            {
                InfoPanel.ClearMeasureInfo();
            }
            InfoPanel.Visibility = measureInfo ? Visibility.Visible : Visibility.Collapsed;
            SettingPanel.Visibility = measureSetting ? Visibility.Visible : Visibility.Collapsed;

        }

        public void SetMeasureInfoType(CPDFMeasureType measureType)
        {
            InfoPanel?.SetMeasureType(measureType);
        }

        public void SetMeasureScale(CPDFMeasureType measureType, string scale)
        {
            InfoPanel?.SetMeasureScale(measureType,scale);
        }
    }
}

