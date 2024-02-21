using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using Compdfkit_Tools.PDFControl;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Compdfkit_Tools.Measure
{
    /// <summary>
    /// MeasureSettingPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MeasureSettingPanel : UserControl
    {
        public event EventHandler CancelEvent;
        public event EventHandler DoneEvent;

        public PDFViewControl PdfViewControl {  get; set; } 

        public bool ReturnToInfoPanel { get; set; }

        public List<AnnotHandlerEventArgs> UpdateArgsList { get; set; } = new List<AnnotHandlerEventArgs>();
        public MeasureSettingPanel()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateArgsList != null && UpdateArgsList.Count > 0)
            {
                ReturnToInfoPanel = false;
            }
            UpdateArgsList?.Clear();
            CancelEvent?.Invoke(this, e);
            ReturnToInfoPanel = false;
        }

        public void ShowAreaAndLength(Visibility visibility)
        {
            if (visibility == Visibility.Visible)
            {
                SettingPanel.Height = 350;
            }
            else
            {
                SettingPanel.Height = 290;
            }
            AreaAndLength.Visibility = visibility;
        }

        private void ButtonDone_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RulerBaseText.Text) || string.IsNullOrEmpty(RulerTranslateText.Text))
            {
                MessageBox.Show("Scale is not greater than zero");
                return;
            }
            if (double.TryParse(RulerBaseText.Text, out double ruleBase))
            {
                if (ruleBase <= 0)
                {
                    MessageBox.Show("Scale is not greater than zero");
                    return;
                }
            }

            if (double.TryParse(RulerTranslateText.Text, out double ruletranBase))
            {
                if (ruletranBase <= 0)
                {
                    MessageBox.Show("Scale is not greater than zero");
                    return;
                }
            }

            if (UpdateArgsList != null && UpdateArgsList.Count == 0)
            {
                SaveMeasureSetting();
            }

            if (UpdateArgsList != null && UpdateArgsList.Count > 0)
            {
                List<AnnotArgsType> allowTypeList = new List<AnnotArgsType>()
                {
                    AnnotArgsType.LineMeasure,
                    AnnotArgsType.PolyLineMeasure,
                    AnnotArgsType.PolygonMeasure
                };

                foreach (AnnotHandlerEventArgs args in UpdateArgsList)
                {
                    if (allowTypeList.Contains(args.EventType))
                    {
                        CPDFAnnotation pdfAnnot = args.GetPDFAnnot();
                        switch (pdfAnnot.Type)
                        {
                            case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                                {
                                    CPDFLineAnnotation lineAnnot = (CPDFLineAnnotation)pdfAnnot;
                                    if (lineAnnot.IsMersured())
                                    {
                                        CPDFDistanceMeasure lineMeasure = lineAnnot.GetDistanceMeasure();
                                        CPDFMeasureInfo measureInfo = lineMeasure.MeasureInfo;
                                        if (PrecisionBox.SelectedValue != null)
                                        {
                                            ComboBoxItem checkItem = PrecisionBox.SelectedValue as ComboBoxItem;
                                            if (double.TryParse(checkItem.Content.ToString(), out double precision))
                                            {
                                                measureInfo.Precision = GetMeasureSavePrecision(precision);
                                            }
                                        }
                                        if (double.TryParse(RulerBaseText.Text, out double ruleBasedata))
                                        {
                                            measureInfo.RulerBase = (float)ruleBasedata;
                                        }
                                        if (RulerBaseUnitCombo.SelectedItem != null)
                                        {
                                            ComboBoxItem RulerBaseUnitcheckItem = RulerBaseUnitCombo.SelectedItem as ComboBoxItem;
                                            measureInfo.RulerBaseUnit = RulerBaseUnitcheckItem.Content.ToString();
                                        }
                                        if (double.TryParse(RulerTranslateText.Text, out double ruletranBasedata))
                                        {
                                            measureInfo.RulerTranslate = (float)ruletranBasedata;
                                        }
                                        if (RulerTranslateCombo.SelectedItem != null)
                                        {
                                            ComboBoxItem RulerTranslatecheckItem = RulerTranslateCombo.SelectedItem as ComboBoxItem;
                                            measureInfo.RulerTranslateUnit = RulerTranslatecheckItem.Content.ToString();
                                        }
                                        lineMeasure.SetMeasureInfo(measureInfo);
                                        lineMeasure.SetMeasureScale(
                                            measureInfo.RulerBase,
                                            measureInfo.RulerBaseUnit,
                                            measureInfo.RulerTranslate,
                                            measureInfo.RulerTranslateUnit);
                                        lineMeasure.UpdateAnnotMeasure();
                                        lineAnnot.UpdateAp();
                                        args.Draw();
                                        if (PdfViewControl != null && PdfViewControl.PDFView != null)
                                        {
                                            CPDFViewer viewer = PdfViewControl.PDFView;
                                            if (viewer != null && viewer.UndoManager != null)
                                            {
                                                viewer.UndoManager.CanSave = true;
                                            }
                                        }
                                    }
                                }
                                break;
                            case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                                {
                                    CPDFPolylineAnnotation polylineAnnot = (CPDFPolylineAnnotation)pdfAnnot;
                                    if (polylineAnnot.IsMersured())
                                    {
                                        CPDFPerimeterMeasure polylineMeasure = polylineAnnot.GetPerimeterMeasure();
                                        CPDFMeasureInfo measureInfo = polylineMeasure.MeasureInfo;
                                        if (PrecisionBox.SelectedValue != null)
                                        {
                                            ComboBoxItem checkItem = PrecisionBox.SelectedValue as ComboBoxItem;
                                            if (double.TryParse(checkItem.Content.ToString(), out double precision))
                                            {
                                                measureInfo.Precision = GetMeasureSavePrecision(precision);
                                            }
                                        }
                                        if (double.TryParse(RulerBaseText.Text, out double ruleBasedata))
                                        {
                                            measureInfo.RulerBase = (float)ruleBasedata;
                                        }
                                        if (RulerBaseUnitCombo.SelectedItem != null)
                                        {
                                            ComboBoxItem RulerBaseUnitcheckItem = RulerBaseUnitCombo.SelectedItem as ComboBoxItem;
                                            measureInfo.RulerBaseUnit = RulerBaseUnitcheckItem.Content.ToString();
                                        }
                                        if (double.TryParse(RulerTranslateText.Text, out double ruletranBasedata))
                                        {
                                            measureInfo.RulerTranslate = (float)ruletranBasedata;
                                        }
                                        if (RulerTranslateCombo.SelectedItem != null)
                                        {
                                            ComboBoxItem RulerTranslatecheckItem = RulerTranslateCombo.SelectedItem as ComboBoxItem;
                                            measureInfo.RulerTranslateUnit = RulerTranslatecheckItem.Content.ToString();
                                        }
                                        polylineMeasure.SetMeasureInfo(measureInfo);
                                        polylineMeasure.SetMeasureScale(
                                            measureInfo.RulerBase,
                                            measureInfo.RulerBaseUnit,
                                            measureInfo.RulerTranslate,
                                            measureInfo.RulerTranslateUnit);
                                        polylineMeasure.UpdateAnnotMeasure();
                                        polylineAnnot.UpdateAp();
                                        args.Draw();
                                        if (PdfViewControl != null && PdfViewControl.PDFView != null)
                                        {
                                            CPDFViewer viewer = PdfViewControl.PDFView;
                                            if (viewer != null && viewer.UndoManager != null)
                                            {
                                                viewer.UndoManager.CanSave = true;
                                            }
                                        }
                                    }
                                }
                                break;
                            case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                                {
                                    CPDFPolygonAnnotation areaAnnot = (CPDFPolygonAnnotation)pdfAnnot;
                                    if (areaAnnot.IsMersured())
                                    {
                                        CPDFAreaMeasure areaMeasure = areaAnnot.GetAreaMeasure();
                                        CPDFMeasureInfo measureInfo = areaMeasure.MeasureInfo;
                                        if (PrecisionBox.SelectedValue != null)
                                        {
                                            ComboBoxItem checkItem = PrecisionBox.SelectedValue as ComboBoxItem;
                                            if (double.TryParse(checkItem.Content.ToString(), out double precision))
                                            {
                                                measureInfo.Precision = GetMeasureSavePrecision(precision);
                                            }
                                        }
                                        if (double.TryParse(RulerBaseText.Text, out double ruleBasedata))
                                        {
                                            measureInfo.RulerBase = (float)ruleBasedata;
                                        }
                                        if (RulerBaseUnitCombo.SelectedItem != null)
                                        {
                                            ComboBoxItem RulerBaseUnitcheckItem = RulerBaseUnitCombo.SelectedItem as ComboBoxItem;
                                            measureInfo.RulerBaseUnit = RulerBaseUnitcheckItem.Content.ToString();
                                        }
                                        if (double.TryParse(RulerTranslateText.Text, out double ruletranBasedata))
                                        {
                                            measureInfo.RulerTranslate = (float)ruletranBasedata;
                                        }
                                        if (RulerTranslateCombo.SelectedItem != null)
                                        {
                                            ComboBoxItem RulerTranslatecheckItem = RulerTranslateCombo.SelectedItem as ComboBoxItem;
                                            measureInfo.RulerTranslateUnit = RulerTranslatecheckItem.Content.ToString();
                                        }

                                        measureInfo.CaptionType = CPDFCaptionType.CPDF_CAPTION_NONE;
                                        if ((bool)AreaCheckBox.IsChecked)
                                        {
                                            measureInfo.CaptionType |= CPDFCaptionType.CPDF_CAPTION_AREA;
                                        }
                                        if ((bool)LengthCheckBox.IsChecked)
                                        {
                                            measureInfo.CaptionType |= CPDFCaptionType.CPDF_CAPTION_LENGTH;
                                        }
                                        areaMeasure.SetMeasureInfo(measureInfo);
                                        areaMeasure.SetMeasureScale(
                                            measureInfo.RulerBase,
                                            measureInfo.RulerBaseUnit,
                                            measureInfo.RulerTranslate,
                                            measureInfo.RulerTranslateUnit);
                                        areaMeasure.UpdateAnnotMeasure();
                                        areaAnnot.UpdateAp();
                                        args.Draw();
                                        if(PdfViewControl!=null && PdfViewControl.PDFView!=null)
                                        {
                                            CPDFViewer viewer = PdfViewControl.PDFView;
                                            if(viewer!=null && viewer.UndoManager!=null)
                                            {
                                                viewer.UndoManager.CanSave = true;
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                ReturnToInfoPanel = false;
            }
            UpdateArgsList?.Clear();
            DoneEvent?.Invoke(this, e);
            ReturnToInfoPanel = false;
        }

        private int GetMeasureSavePrecision(double Precision)
        {
            if (Precision == 1)
            {
                return CPDFMeasure.PRECISION_VALUE_ZERO;
            }
            if (Precision == 0.1)
            {
                return CPDFMeasure.PRECISION_VALUE_ONE;
            }
            if (Precision == 0.01)
            {
                return CPDFMeasure.PRECISION_VALUE_TWO;
            }
            if (Precision == 0.001)
            {
                return CPDFMeasure.PRECISION_VALUE_THREE;
            }
            if (Precision == 0.0001)
            {
                return CPDFMeasure.PRECISION_VALUE_FOUR;
            }
            return 0;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            List<Key> allowKeys = new List<Key>()
            {
                Key.Delete, Key.Back, Key.Enter, Key.NumPad0,  Key.NumPad1, Key.NumPad2, Key.NumPad3,
                Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.D0,
                Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.Left, Key.Right,
                Key.OemPeriod,Key.Decimal
            };

            if (allowKeys.Contains(e.Key) == false)
            {
                e.Handled = true;
            }
        }

        private void TextBox_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (e.Command == ApplicationCommands.Paste && Clipboard.ContainsText())
                {
                    string checkText = Clipboard.GetText();
                    if (int.TryParse(checkText, out int value))
                    {
                        e.CanExecute = true;
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void BindMeasureSetting()
        {
            if (UpdateArgsList != null && UpdateArgsList.Count>0)
            {
                return;
            }
            RulerBaseText.Text = MeasureSetting.RulerBase.ToString();
            RulerTranslateText.Text = MeasureSetting.RulerTranslate.ToString();
            RulerTranslateCombo.SelectedIndex = -1;
            RulerBaseUnitCombo.SelectedIndex = -1;
            PrecisionBox.SelectedIndex = -1;
            if (MeasureSetting.RulerBaseUnit == "in")
            {
                RulerBaseUnitCombo.SelectedIndex = 0;
            }
            if (MeasureSetting.RulerBaseUnit == "cm")
            {
                RulerBaseUnitCombo.SelectedIndex = 1;
            }
            if (MeasureSetting.RulerBaseUnit == "mm")
            {
                RulerBaseUnitCombo.SelectedIndex = 2;
            }

            for (int i = 0; i < RulerTranslateCombo.Items.Count; i++)
            {
                ComboBoxItem checkItem = RulerTranslateCombo.Items[i] as ComboBoxItem;
                if (checkItem != null && checkItem.Tag.ToString() == MeasureSetting.RulerTranslateUnit.ToString())
                {
                    RulerTranslateCombo.SelectedIndex = i;
                }
            }

            for (int i = 0; i < PrecisionBox.Items.Count; i++)
            {
                ComboBoxItem checkItem = PrecisionBox.Items[i] as ComboBoxItem;
                if (checkItem != null && checkItem.Content.ToString() == ((decimal)MeasureSetting.Precision).ToString())
                {
                    PrecisionBox.SelectedIndex = i;
                }
            }
        }
        public void BindMeasureSetting(MeasureEventArgs measureEventArgs)
        {
            if (measureEventArgs != null)
            {

                RulerBaseText.Text = measureEventArgs.RulerBase.ToString();
                RulerTranslateText.Text = measureEventArgs.RulerTranslate.ToString();
                RulerTranslateCombo.SelectedIndex = -1;
                RulerBaseUnitCombo.SelectedIndex = -1;
                PrecisionBox.SelectedIndex = -1;
                if (measureEventArgs.RulerBaseUnit == "in")
                {
                    RulerBaseUnitCombo.SelectedIndex = 0;
                }
                if (measureEventArgs.RulerBaseUnit == "cm")
                {
                    RulerBaseUnitCombo.SelectedIndex = 1;
                }
                if (measureEventArgs.RulerBaseUnit == "mm")
                {
                    RulerBaseUnitCombo.SelectedIndex = 2;
                }

                for (int i = 0; i < RulerTranslateCombo.Items.Count; i++)
                {
                    ComboBoxItem checkItem = RulerTranslateCombo.Items[i] as ComboBoxItem;
                    if (checkItem != null && checkItem.Tag.ToString() == measureEventArgs.RulerTranslateUnit.ToString())
                    {
                        RulerTranslateCombo.SelectedIndex = i;
                    }
                }

                for (int i = 0; i < PrecisionBox.Items.Count; i++)
                {
                    ComboBoxItem checkItem = PrecisionBox.Items[i] as ComboBoxItem;
                    if (checkItem != null && checkItem.Content.ToString() == ((decimal)measureEventArgs.Precision).ToString())
                    {
                        PrecisionBox.SelectedIndex = i;
                    }
                }
            }
        }

        private void SaveMeasureSetting()
        {
            if (double.TryParse(RulerBaseText.Text, out double ruleBase))
            {
                MeasureSetting.RulerBase = ruleBase;
            }

            if (RulerBaseUnitCombo.SelectedItem != null)
            {
                ComboBoxItem checkItem = RulerBaseUnitCombo.SelectedItem as ComboBoxItem;
                MeasureSetting.RulerBaseUnit = checkItem.Content.ToString();
            }

            if (double.TryParse(RulerTranslateText.Text, out double ruletranBase))
            {
                MeasureSetting.RulerTranslate = ruletranBase;
            }

            if (RulerTranslateCombo.SelectedItem != null)
            {
                ComboBoxItem checkItem = RulerTranslateCombo.SelectedItem as ComboBoxItem;
                MeasureSetting.RulerTranslateUnit = checkItem.Content.ToString();
            }

            if (PrecisionBox.SelectedValue != null)
            {
                ComboBoxItem checkItem = PrecisionBox.SelectedValue as ComboBoxItem;
                if (double.TryParse(checkItem.Content.ToString(), out double precision))
                {
                    MeasureSetting.Precision = precision;
                }
            }

            MeasureSetting.IsShowArea = (bool)AreaCheckBox.IsChecked;
            MeasureSetting.IsShowLength = (bool)LengthCheckBox.IsChecked;
        }

        public void ChangedCheckBoxIsChecked(bool Area, bool Lenght)
        {
            AreaCheckBox.IsChecked = Area;
            LengthCheckBox.IsChecked = Lenght;
        }
    }
}

