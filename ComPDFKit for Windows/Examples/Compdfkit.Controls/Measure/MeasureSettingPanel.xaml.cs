using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Controls.PDFControl;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComPDFKit.Tool.SettingParam;

namespace ComPDFKit.Controls.Measure
{
    public partial class MeasureSettingPanel : UserControl
    {
        public event EventHandler CancelEvent;
        public event EventHandler DoneEvent;

        public PDFViewControl PdfViewControl {  get; set; } 

        public bool ReturnToInfoPanel { get; set; }

        public MeasureSettingPanel()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
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

        public void BindMeasureSetting(CPDFMeasureInfo measureInfo)
        {
            if(measureInfo == null)
            {
                return;
            }
            RulerBaseText.Text = measureInfo.RulerBase.ToString();
            RulerTranslateText.Text = measureInfo.RulerTranslate.ToString();
            RulerTranslateCombo.SelectedIndex = -1;
            RulerBaseUnitCombo.SelectedIndex = -1;
            PrecisionBox.SelectedIndex = -1;
            if (measureInfo.RulerBaseUnit == "in")
            {
                RulerBaseUnitCombo.SelectedIndex = 0;
            }
            if (measureInfo.RulerBaseUnit == "cm")
            {
                RulerBaseUnitCombo.SelectedIndex = 1;
            }
            if (measureInfo.RulerBaseUnit == "mm")
            {
                RulerBaseUnitCombo.SelectedIndex = 2;
            }

            for (int i = 0; i < RulerTranslateCombo.Items.Count; i++)
            {
                ComboBoxItem checkItem = RulerTranslateCombo.Items[i] as ComboBoxItem;
                if (checkItem != null && checkItem.Tag.ToString() == measureInfo.RulerTranslateUnit.ToString())
                {
                    RulerTranslateCombo.SelectedIndex = i;
                }
            }

            string precision = GetMeasureShowPrecision(measureInfo.Precision).ToString();
            for (int i = 0; i < PrecisionBox.Items.Count; i++)
            {
                ComboBoxItem checkItem = PrecisionBox.Items[i] as ComboBoxItem;
                if (checkItem != null && checkItem.Content.ToString() == precision)
                {
                    PrecisionBox.SelectedIndex = i;
                }
            }
        }

        public void SaveMeasureSetting(CPDFAnnotation annot)
        {
            MeasureSetting measureSetting = new MeasureSetting();
            if (double.TryParse(RulerBaseText.Text, out double ruleBase))
            {
                measureSetting.RulerBase = ruleBase;
            }

            if (RulerBaseUnitCombo.SelectedItem != null)
            {
                ComboBoxItem checkItem = RulerBaseUnitCombo.SelectedItem as ComboBoxItem;
                measureSetting.RulerBaseUnit = checkItem.Tag.ToString();
            }

            if (double.TryParse(RulerTranslateText.Text, out double ruletranBase))
            {
                measureSetting.RulerTranslate = ruletranBase;
            }

            if (RulerTranslateCombo.SelectedItem != null)
            {
                ComboBoxItem checkItem = RulerTranslateCombo.SelectedItem as ComboBoxItem;
                measureSetting.RulerTranslateUnit = checkItem.Tag.ToString();
            }

            if (PrecisionBox.SelectedValue != null)
            {
                ComboBoxItem checkItem = PrecisionBox.SelectedValue as ComboBoxItem;
                if (double.TryParse(checkItem.Content.ToString(), out double precision))
                {
                    measureSetting.Precision = precision;
                }
            }

            measureSetting.IsShowArea = (bool)AreaCheckBox?.IsChecked;
            measureSetting.IsShowLength = (bool)LengthCheckBox?.IsChecked;
            UpdateAnnotWithSetting(annot, measureSetting);
        }

        private void UpdateAnnotWithSetting(CPDFAnnotation annot, MeasureSetting measureSetting)
        {
            if(annot is CPDFLineAnnotation lineAnnot)
            {
                if (lineAnnot.IsMersured())
                {
                    CPDFDistanceMeasure lineMeasure = lineAnnot.GetDistanceMeasure();
                    CPDFMeasureInfo info = lineMeasure.MeasureInfo;
                    
                    info.RulerBaseUnit = measureSetting.RulerBaseUnit;
                    info.RulerBase = (float)measureSetting.RulerBase;
                    info.RulerTranslateUnit = measureSetting.RulerTranslateUnit;
                    info.RulerTranslate = (float)measureSetting.RulerTranslate;
                    
                    info.Precision = GetMeasureSavePrecision(measureSetting.Precision);
                    lineAnnot.GetDistanceMeasure().SetMeasureInfo(info);
                    lineAnnot.GetDistanceMeasure().SetMeasureScale(info.RulerBase, info.RulerBaseUnit, info.RulerTranslate, info.RulerTranslateUnit);
                    
                    lineAnnot.GetDistanceMeasure().UpdateAnnotMeasure();
                    lineAnnot.UpdateAp();
                    PdfViewControl.UpdateAnnotFrame();
                }
            }
            else if(annot is CPDFPolylineAnnotation polylineAnnot)
            {
                if (polylineAnnot.IsMersured())
                {
                    CPDFPerimeterMeasure polylineMeasure = polylineAnnot.GetPerimeterMeasure();
                    CPDFMeasureInfo info = polylineMeasure.MeasureInfo;
                    info.RulerBaseUnit = measureSetting.RulerBaseUnit;
                    info.RulerBase = (float)measureSetting.RulerBase;
                    info.RulerTranslateUnit = measureSetting.RulerTranslateUnit;
                    info.RulerTranslate = (float)measureSetting.RulerTranslate;
                    info.Precision = GetMeasureSavePrecision(measureSetting.Precision);
                    polylineAnnot.GetPerimeterMeasure().SetMeasureInfo(info);
                    polylineAnnot.GetPerimeterMeasure().SetMeasureScale(info.RulerBase, info.RulerBaseUnit, info.RulerTranslate, info.RulerTranslateUnit);
                    polylineAnnot.GetPerimeterMeasure().UpdateAnnotMeasure();
                    polylineAnnot.UpdateAp();
                    PdfViewControl.UpdateAnnotFrame();
                }
            }
            else if(annot is CPDFPolygonAnnotation areaAnnot)
            {
                if (areaAnnot.IsMersured())
                {
                    CPDFAreaMeasure areaMeasure = areaAnnot.GetAreaMeasure();
                    CPDFMeasureInfo info = areaMeasure.MeasureInfo;
                    info.RulerBaseUnit = measureSetting.RulerBaseUnit;
                    info.RulerBase = (float)measureSetting.RulerBase;
                    info.RulerTranslateUnit = measureSetting.RulerTranslateUnit;
                    info.RulerTranslate = (float)measureSetting.RulerTranslate;
                    info.Precision = GetMeasureSavePrecision(measureSetting.Precision);
                    info.CaptionType = CPDFCaptionType.CPDF_CAPTION_NONE;
                    if (measureSetting.IsShowArea)
                    {
                        info.CaptionType |= CPDFCaptionType.CPDF_CAPTION_AREA;
                    }
                    if (measureSetting.IsShowLength)
                    {
                        info.CaptionType |= CPDFCaptionType.CPDF_CAPTION_LENGTH;
                    }
                    areaAnnot.GetAreaMeasure().SetMeasureInfo(info);
                    areaAnnot.GetAreaMeasure().SetMeasureScale(info.RulerBase, info.RulerBaseUnit, info.RulerTranslate, info.RulerTranslateUnit);
                    areaAnnot.GetAreaMeasure().UpdateAnnotMeasure();
                    areaAnnot.UpdateAp();
                    PdfViewControl.UpdateAnnotFrame();
                }
            }
            else
            {
                MeasureSetting setting = PdfViewControl.PDFViewTool.GetMeasureSetting();
                setting.RulerBase = measureSetting.RulerBase;
                setting.RulerBaseUnit = measureSetting.RulerBaseUnit;
                setting.RulerTranslate = measureSetting.RulerTranslate;
                setting.RulerTranslateUnit = measureSetting.RulerTranslateUnit;
                setting.Precision = measureSetting.Precision;
                setting.IsShowArea = measureSetting.IsShowArea;
                setting.IsShowLength = measureSetting.IsShowLength;
            }
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

        public void ChangedCheckBoxIsChecked(bool Area, bool Lenght)
        {
            AreaCheckBox.IsChecked = Area;
            LengthCheckBox.IsChecked = Lenght;
        }
    }
}

