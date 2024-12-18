using ComPDFKit.Controls.Common;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using CFontNameHelper = ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using System.Runtime.CompilerServices;

namespace ComPDFKit.Controls.Measure.Property
{
    public partial class PolygonalProperty : UserControl
    {
        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,32,30,32,48,72
        };

        bool IsLoadedData = false;

        private PolygonMeasureParam polygonMeasureParam;

        public CPDFPolygonAnnotation Annotation { get; set; }

        public PDFViewControl ViewControl { get; set; }

        //private AnnotAttribEvent PolygonalEvent { get; set; }
        public event EventHandler<PolygonMeasureParam> PolygonMeasureParamChanged;


        public PolygonalProperty()
        {
            InitializeComponent();
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (Annotation == null)
                {
                    polygonMeasureParam.Content = NoteTextBox.Text;
                    PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
                }
                if (Annotation != null && ViewControl != null)
                {
                    Annotation.SetContent(NoteTextBox.Text);
                    Annotation.UpdateAp();
                    ViewControl?.UpdateAnnotFrame();
                }
            }
        }

        private void FontStyleCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoadedData) return;
            int selectIndex = Math.Max(0, FontStyleCombox.SelectedIndex);
            bool isBold = false;
            bool isItalic = false;

            switch (selectIndex)
            {
                case 0:
                    isBold = false;
                    isItalic = false;
                    break;
                case 1:
                    isBold = true;
                    isItalic = false;
                    break;
                case 2:
                    isBold = false;
                    isItalic = true;
                    break;
                case 3:
                    isBold = true;
                    isItalic = true;
                    break;
                default:
                    break;
            }

            if (Annotation == null)
            {
                polygonMeasureParam.IsBold = isBold;
                polygonMeasureParam.IsItalic = isItalic;
                PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
            }

            if (Annotation != null)
            {
                CTextAttribute textAttribute = Annotation.GetTextAttribute();
                var fontType = CFontNameHelper.GetFontType((FontCombox.SelectedItem as ComboBoxItem).Content.ToString());
                var newName = CFontNameHelper.ObtainFontName(fontType, isBold, isItalic);
                textAttribute.FontName = newName;
                Annotation.SetTextAttribute(textAttribute);
                Annotation.UpdateAp();
                ViewControl?.UpdateAnnotFrame();
            }
        }

        private void FontSizeCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                if (FontSizeComboBox.SelectedItem != null)
                {
                    if (Annotation == null)
                    {
                        polygonMeasureParam.FontSize = (float)Convert.ToDouble(FontSizeComboBox.SelectedItem);
                        PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
                    }
                    if (Annotation != null && ViewControl != null)
                    {
                        CTextAttribute textAttribute = Annotation.GetTextAttribute();
                        textAttribute.FontSize = (float)Convert.ToDouble(FontSizeComboBox.SelectedItem);
                        Annotation.SetTextAttribute(textAttribute);
                        Annotation.UpdateAp();
                        ViewControl?.UpdateAnnotFrame();
                    }
                }
            }
        }

        private void FontCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                ComboBoxItem selectItem = FontCombox.SelectedItem as ComboBoxItem;
                if (Annotation == null)
                {
                    var fontType = CFontNameHelper.GetFontType(selectItem.Content.ToString());
                    string FontName = CFontNameHelper.ObtainFontName(fontType, polygonMeasureParam.IsBold, polygonMeasureParam.IsItalic);
                    polygonMeasureParam.FontName = FontName;
                    PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
                }
                if (Annotation != null && ViewControl != null)
                {
                    if (selectItem != null && selectItem.Content != null)
                    {
                        CTextAttribute textAttr = Annotation.GetTextAttribute();
                        bool isBold = CFontNameHelper.IsBold(textAttr.FontName);
                        bool isItalic = CFontNameHelper.IsItalic(textAttr.FontName);
                        var fontType = CFontNameHelper.GetFontType(selectItem.Content.ToString());
                        textAttr.FontName = CFontNameHelper.ObtainFontName(fontType, isBold, isItalic);
                        Annotation.SetTextAttribute(textAttr);
                        Annotation.UpdateAp();
                        ViewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Binding SizeListbinding = new Binding();
            SizeListbinding.Source = this;
            SizeListbinding.Path = new System.Windows.PropertyPath("SizeList");
            FontSizeComboBox.SetBinding(ComboBox.ItemsSourceProperty, SizeListbinding);
            IsLoadedData = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                if (BorderColorPickerControl.GetBrush() is SolidColorBrush checkBrush)
                {
                    byte[] color = { checkBrush.Color.R, checkBrush.Color.G, checkBrush.Color.B };

                    if (Annotation == null)
                    {
                        polygonMeasureParam.LineColor = color;
                        PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
                    }
                    if (Annotation != null && ViewControl != null)
                    {
                        Annotation.SetLineColor(color);
                        Annotation.UpdateAp();
                        ViewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        private void FillColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                SolidColorBrush checkBrush = FillColorPickerControl.GetBrush() as SolidColorBrush;
                if (checkBrush != null)
                {
                    byte[] color = { checkBrush.Color.R, checkBrush.Color.G, checkBrush.Color.B };
                    if (Annotation == null)
                    {
                        polygonMeasureParam.FillColor = color;
                        if (checkBrush.Color != Colors.Transparent)
                        {
                            polygonMeasureParam.HasFillColor = true;
                        }
                        else
                        {
                            polygonMeasureParam.HasFillColor = false;
                        }
                        PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
                    }
                    if (Annotation != null && ViewControl != null)
                    {
                        if (checkBrush.Color == Colors.Transparent)
                        {
                            Annotation.ClearBgColor();
                        }
                        else
                        {
                            Annotation.SetBgColor(color);
                        }
                        Annotation.UpdateAp();
                        ViewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                double opacity = CPDFOpacityControl.OpacityValue / 100.0;
                if (opacity > 0 && opacity <= 1)
                {
                    opacity = opacity * 255;
                }

                if (Annotation == null)
                {
                    polygonMeasureParam.Transparency = (byte)opacity;
                    PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
                }
                if (Annotation != null && ViewControl != null)
                {
                    if (Math.Abs(opacity - Annotation.GetTransparency()) > 0.01)
                    {
                        Annotation.SetTransparency((byte)opacity);
                        Annotation.UpdateAp();
                        ViewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        private void CPDFLineStyleControl_LineStyleChanged(object sender, EventArgs e)
        {
            if (!IsLoadedData) return;
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
            if (Annotation == null)
            {
                polygonMeasureParam.BorderStyle = borderStyle;
                polygonMeasureParam.LineDash = dashArray;

                PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
            }
            if (Annotation != null && ViewControl != null)
            {

                Annotation.SetBorderStyle(borderStyle, dashArray);
                Annotation.UpdateAp();
                ViewControl.UpdateAnnotFrame();
            }
        }

        private void FontColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (!IsLoadedData) return;
            SolidColorBrush checkBrush = FontColorPickerControl.GetBrush() as SolidColorBrush;
            if (checkBrush != null)
            {
                byte[] color = { checkBrush.Color.R, checkBrush.Color.G, checkBrush.Color.B };
                if (Annotation == null)
                {
                    polygonMeasureParam.FontColor = color;
                    PolygonMeasureParamChanged?.Invoke(sender, polygonMeasureParam);
                }

                if (Annotation != null && ViewControl != null)
                {

                    if (Annotation != null)
                    {
                        CTextAttribute textAttribute = Annotation.GetTextAttribute();
                        textAttribute.FontColor = color;
                        Annotation.SetTextAttribute(textAttribute);
                        Annotation.UpdateAp();
                        ViewControl.UpdateAnnotFrame();
                    }
                }
            }
        }

        public void SetFontStyle(bool isBold, bool isItalic)
        {
            if (isBold == false && isItalic == false)
            {
                FontStyleCombox.SelectedIndex = 0;
                return;
            }

            if (isBold && isItalic == false)
            {
                FontStyleCombox.SelectedIndex = 1;
                return;
            }

            if (isBold == false && isItalic)
            {
                FontStyleCombox.SelectedIndex = 2;
                return;
            }

            if (isBold && isItalic)
            {
                FontStyleCombox.SelectedIndex = 3;
            }
        }

        private void SetFontSize(double size)
        {
            int index = SizeList.IndexOf((int)size);
            FontSizeComboBox.SelectedIndex = index;
        }

        public void SetFontName(string fontName)
        {
            foreach (ComboBoxItem item in FontCombox.Items)
            {
                if (item.Content.ToString() == fontName)
                {
                    FontCombox.SelectedItem = item;
                    break;
                }
            }
        }

        public void SetAnnotParam(PolygonMeasureParam param, CPDFAnnotation annot, PDFViewControl viewControl)
        {
            Annotation = annot as CPDFPolygonAnnotation;
            ViewControl = viewControl;
            polygonMeasureParam = param;
            if (param == null)
            {
                return;
            }

            Color lineColor = Color.FromRgb(param.LineColor[0], param.LineColor[1], param.LineColor[2]);
            BorderColorPickerControl.SetCheckedForColor(lineColor);
            if(Annotation != null)
            {
                if (!Annotation.HasBgColor)
                {
                    FillColorPickerControl.SetCheckedForColor(Colors.Transparent);
                }
                else
                {
                    Color fillColor = Color.FromRgb(param.FillColor[0], param.FillColor[1], param.FillColor[2]);
                    FillColorPickerControl.SetCheckedForColor(fillColor);
                }
            }
            else
            {
                if (!param.HasFillColor)
                {
                    FillColorPickerControl.SetCheckedForColor(Colors.Transparent);
                }
                else
                {
                    Color fillColor = Color.FromRgb(param.FillColor[0], param.FillColor[1], param.FillColor[2]);
                    FillColorPickerControl.SetCheckedForColor(fillColor);
                }
            }
            Color FontColor = Color.FromRgb(param.FontColor[0], param.FontColor[1], param.FontColor[2]);    
            FontColorPickerControl.SetCheckedForColor(FontColor);

            if (polygonMeasureParam.BorderStyle == C_BORDER_STYLE.BS_SOLID)
            {
                CPDFLineStyleControl.DashStyle = DashStyles.Solid;
            }
            else
            {
                List<double> dashArray = new List<double>();
                foreach (double num in param.LineDash)
                {
                    dashArray.Add(num);
                }
                CPDFLineStyleControl.DashStyle = new DashStyle(dashArray, 0);
            }
            double opacity = param.Transparency / 255.0 * 100.0;
            CPDFOpacityControl.OpacityValue = (int)Math.Ceiling(opacity);
            NoteTextBox.Text = param.Content;
            SetFontSize(param.FontSize);
            SetFontStyle(param.IsBold, param.IsItalic);
            SetFontName(param.FontName);
        }
    }
}
