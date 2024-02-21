using ComPDFKit.Measure;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
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
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.Measure
{
    /// <summary>
    /// MeasureInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class MeasureInfoPanel : UserControl
    {
        public event EventHandler SettingClick;
        public CPDFMeasureType MeasureType { get; private set; }

        internal Dictionary<string, string> IconsDict { get; private set; } = new Dictionary<string, string>()
        {
            {
                LanguageHelper.ToolBarManager.GetString("Button_Line") ?? string.Empty,
                "M17.7782 5.75646L14.2427 2.22093L13.0052 3.45837L15.4802 5.93333L14.4195 6.99399L11.9446 4.51903L10.1768 " +
                "6.28679L12.6518 8.76175L11.5911 9.82241L9.11615 7.34745L7.34838 9.11522L9.82334 11.5902L8.76268 " +
                "12.6508L6.28772 10.1759L4.51996 11.9436L6.99492 14.4186L5.93426 15.4793L3.4593 13.0043L2.22186 " +
                "14.2417L5.75739 17.7773L17.7782 5.75646ZM15.3033 1.16027L14.2427 0.0996094L13.182 1.16027L1.1612 " +
                "13.1811L0.100539 14.2417L1.1612 15.3024L4.69673 18.8379L5.75739 19.8986L6.81805 18.8379L18.8389 " +
                "6.81712L19.8995 5.75646L18.8389 4.6958L15.3033 1.16027Z"
            },
            {
                LanguageHelper.ToolBarManager.GetString("Button_Multiline") ?? string.Empty,
                "M0.5 3C0.5 1.89543 1.39543 1 2.5 1C3.33934 1 4.05793 1.51704 4.35462 2.25H15.6454C15.9421 1.51704 16.6607 1 " +
                "17.5 1C18.6046 1 19.5 1.89543 19.5 3C19.5 3.83934 18.983 4.55793 18.25 4.85462V17V17.75H17.5H13.7011L14.5167 " +
                "18.5765L13.5138 19.5928L11.4986 17.5505L10.9972 17.0423L11.4986 16.5342L13.5138 14.4918L14.5167 15.5082L13.7847 " +
                "16.25H16.75V4.85462C16.2487 4.65168 15.8483 4.25135 15.6454 3.75H4.35462C4.15168 4.25135 3.75135 4.65168 3.25 " +
                "4.85462V16.25H6.25V15H7.75V19H6.25V17.75H2.5H1.75V17V4.85462C1.01704 4.55793 0.5 3.83934 0.5 3Z"
            },
            {
                LanguageHelper.ToolBarManager.GetString("Button_Polygonal") ?? string.Empty,
                "M1.37111 7.6963L10 1.42705L18.6289 7.6963L15.3329 17.8402H4.66705L1.37111 7.6963Z"
            },
            {
                LanguageHelper.ToolBarManager.GetString("Button_Rectangle") ?? string.Empty,
                "M20.25 2.25H0.75V17.75H20.25V2.25ZM18.75 10.75V16.25H2.25V10.75H18.75ZM18.75 9.25V3.75H2.25V9.25H18.75ZM6.5 5.5H3.5V7.5H6.5V5.5ZM3.5 12.5H6.5V14.5H3.5V12.5Z"
            }

        };

        public MeasureInfoPanel()
        {
            InitializeComponent();
        }

        internal void SetMeasureScale(CPDFMeasureType measureType, string scale)
        {
            switch (measureType)
            {
                case CPDFMeasureType.CPDF_DISTANCE_MEASURE:
                    ScaleText.Text = scale;
                    break;
                case CPDFMeasureType.CPDF_PERIMETER_MEASURE:
                    ScalePolyLineText.Text = scale;
                    break;
                case CPDFMeasureType.CPDF_AREA_MEASURE:
                    ScalePolygonText.Text = scale;
                    break;
                default:
                    break;
            }
        }

        public void SetMeasureType(CPDFMeasureType newType)
        {
            MeasureType=newType;
            int iconIndex = (int)newType;
            LinePanel.Visibility = Visibility.Collapsed;
            PolyLinePanel.Visibility = Visibility.Collapsed;
            PolygonPanel.Visibility = Visibility.Collapsed;
            if (iconIndex>=0 &&  iconIndex< IconsDict.Count)
            {
                TypeConverter typeCovert = TypeDescriptor.GetConverter(typeof(Geometry));
                MeasureIcon.Data = PathGeometry.CreateFromGeometry((Geometry)typeCovert.ConvertFrom(IconsDict.ElementAt(iconIndex).Value));
                MeasureTitelText.Text = IconsDict.ElementAt(iconIndex).Key;

                switch(newType)
                {
                    case CPDFMeasureType.CPDF_DISTANCE_MEASURE:
                        LinePanel.Visibility = Visibility.Visible;
                        MeasureIcon.Fill = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D));
                        MeasureIcon.Stroke = null;
                        break;
                    case CPDFMeasureType.CPDF_PERIMETER_MEASURE:
                        PolyLinePanel.Visibility = Visibility.Visible;
                        MeasureIcon.Fill = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D));
                        MeasureIcon.Stroke = null;
                        break;
                    case CPDFMeasureType.CPDF_AREA_MEASURE:
                        PolygonPanel.Visibility = Visibility.Visible;
                        MeasureIcon.Fill = null;
                        MeasureIcon.Stroke=new SolidColorBrush(Color.FromRgb(0x43,0x47,0x4D));
                        break;
                    default:
                        break;
                }
            }
        }

        public void ClearMeasureInfo()
        {
            DistanceText.Text = string.Empty;
            PrecisionText.Text = string.Empty;
            AngleText.Text = string.Empty;
            XText.Text = string.Empty;
            YText.Text = string.Empty;
            ScaleText.Text= string.Empty;
            DistancePolyLineText.Text = string.Empty;
            PrecisionPolyLineText.Text = string.Empty;
            AnglePolyLineText.Text = string.Empty;
            ScalePolyLineText.Text = string.Empty;
            ScalePolygonText.Text=string.Empty;
            RoundPolygonText.Text = string.Empty;
            PrecisionPolygonText.Text = string.Empty;
            AnglePolygonText.Text = string.Empty;
        }

        public void SetMeasureInfo(MeasureEventArgs info)
        {
            switch(info.Type)
            {
                case CPDFMeasureType.CPDF_DISTANCE_MEASURE:
                    {
                        DistanceText.Text=info.Distance;
                        PrecisionText.Text= ((decimal)info.Precision).ToString();
                        AngleText.Text = info.Angle.ToString()+"°";
                        XText.Text=info.MousePos.X.ToString();
                        YText.Text=info.MousePos.Y.ToString();
                        ScaleText.Text = string.Format("{0} {1} = {2} {3}",
                            info.RulerBase,
                            info.RulerBaseUnit,
                            info.RulerTranslate,
                            info.RulerTranslateUnit);
                    }
                    break;
                case CPDFMeasureType.CPDF_PERIMETER_MEASURE:
                    {
                        DistancePolyLineText.Text = info.Distance;
                        PrecisionPolyLineText.Text = ((decimal)info.Precision).ToString();
                        AnglePolyLineText.Text = info.Angle.ToString() + "°";
                        ScalePolyLineText.Text = string.Format("{0} {1} = {2} {3}",
                            info.RulerBase,
                            info.RulerBaseUnit,
                            info.RulerTranslate,
                            info.RulerTranslateUnit);
                    }
                    break;
                case CPDFMeasureType.CPDF_AREA_MEASURE:
                    {
                        RoundPolygonText.Text = info.Area;
                        PrecisionPolygonText.Text = ((decimal)info.Precision).ToString();
                        AnglePolygonText.Text = info.Angle.ToString() + "°";
                        ScalePolygonText.Text = string.Format("{0} {1} = {2} {3}",
                            info.RulerBase,
                            info.RulerBaseUnit, 
                            info.RulerTranslate,
                            info.RulerTranslateUnit);
                    }
                    break;
                default:
                    break;
            }
        }
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SettingClick?.Invoke(this, e);
        }
    }
}
