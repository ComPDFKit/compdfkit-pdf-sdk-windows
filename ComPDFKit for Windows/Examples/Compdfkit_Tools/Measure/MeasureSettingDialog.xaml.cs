using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Compdfkit_Tools.Measure
{
    /// <summary>
    /// MeasureSettingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class MeasureSettingDialog : Window
    {
        public MeasureSettingDialog()
        {
            InitializeComponent();
        }

        public event EventHandler CancelEvent;
        public event EventHandler DoneEvent;

        public bool ReturnToInfoPanel { get; set; }

        public List<AnnotHandlerEventArgs> UpdateArgsList { get; set; } = new List<AnnotHandlerEventArgs>();

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateArgsList != null && UpdateArgsList.Count > 0)
            {
                ReturnToInfoPanel = false;
            }
            UpdateArgsList?.Clear();
            CancelEvent?.Invoke(this, e);
            ReturnToInfoPanel = false;
            Close();
        }

        private void ButtonDone_Click(object sender, RoutedEventArgs e)
        {
            SaveMeasureSetting();
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
                                        measureInfo.Precision = MeasureSetting.GetMeasureSavePrecision();
                                        measureInfo.RulerBase = (float)MeasureSetting.RulerBase;
                                        measureInfo.RulerBaseUnit = MeasureSetting.RulerBaseUnit;
                                        measureInfo.RulerTranslate = (float)MeasureSetting.RulerTranslate;
                                        measureInfo.RulerTranslateUnit = MeasureSetting.RulerTranslateUnit;
                                        lineMeasure.SetMeasureInfo(measureInfo);
                                        lineMeasure.SetMeasureScale(
                                            measureInfo.RulerBase,
                                            measureInfo.RulerBaseUnit,
                                            measureInfo.RulerTranslate,
                                            measureInfo.RulerTranslateUnit);
                                        lineMeasure.UpdateAnnotMeasure();
                                        lineAnnot.UpdateAp();
                                        args.Draw();
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
                                        measureInfo.Precision = MeasureSetting.GetMeasureSavePrecision();
                                        measureInfo.RulerBase = (float)MeasureSetting.RulerBase;
                                        measureInfo.RulerBaseUnit = MeasureSetting.RulerBaseUnit;
                                        measureInfo.RulerTranslate = (float)MeasureSetting.RulerTranslate;
                                        measureInfo.RulerTranslateUnit = MeasureSetting.RulerTranslateUnit;
                                        polylineMeasure.SetMeasureInfo(measureInfo);
                                        polylineMeasure.SetMeasureScale(
                                            measureInfo.RulerBase,
                                            measureInfo.RulerBaseUnit,
                                            measureInfo.RulerTranslate,
                                            measureInfo.RulerTranslateUnit);
                                        polylineMeasure.UpdateAnnotMeasure();
                                        polylineAnnot.UpdateAp();
                                        args.Draw();
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
                                        measureInfo.Precision = MeasureSetting.GetMeasureSavePrecision();
                                        measureInfo.RulerBase = (float)MeasureSetting.RulerBase;
                                        measureInfo.RulerBaseUnit = MeasureSetting.RulerBaseUnit;
                                        measureInfo.RulerTranslate = (float)MeasureSetting.RulerTranslate;
                                        measureInfo.RulerTranslateUnit = MeasureSetting.RulerTranslateUnit;

                                        measureInfo.CaptionType = CPDFCaptionType.CPDF_CAPTION_NONE;
                                        if (MeasureSetting.IsShowArea)
                                        {
                                            measureInfo.CaptionType |= CPDFCaptionType.CPDF_CAPTION_AREA;
                                        }
                                        if (MeasureSetting.IsShowLength)
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
            Close();
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
                if (checkItem != null && checkItem.Content.ToString() == MeasureSetting.RulerTranslateUnit.ToString())
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
