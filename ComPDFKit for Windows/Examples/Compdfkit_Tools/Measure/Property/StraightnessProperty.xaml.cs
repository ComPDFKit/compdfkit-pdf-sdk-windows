using ComPDFKit.PDFAnnotation;
using Compdfkit_Tools.Common;
using Compdfkit_Tools.Data;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace Compdfkit_Tools.Measure.Property
{
    /// <summary>
    /// StraightnessProperty.xaml 的交互逻辑
    /// </summary>
    public partial class StraightnessProperty : UserControl
    {
        private AnnotAttribEvent LineEvent { get; set; }
        public ObservableCollection<int> SizeList { get; set; } = new ObservableCollection<int>
        {
            6,8,9,10,12,14,18,20,24,26,28,32,30,32,48,72
        };

        bool IsLoadedData = false;

        public StraightnessProperty()
        {
            InitializeComponent();
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LineEvent?.UpdateAttrib(AnnotAttrib.NoteText, NoteTextBox.Text);
            LineEvent?.UpdateAnnot();
        }

        private void FontStyleCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            LineEvent?.UpdateAttrib(AnnotAttrib.IsBold, isBold);
            LineEvent?.UpdateAttrib(AnnotAttrib.IsItalic, isItalic);
            LineEvent?.UpdateAnnot();
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                LineEvent?.UpdateAttrib(AnnotAttrib.FontSize, (sender as ComboBox).SelectedItem);
                LineEvent?.UpdateAnnot();
            }
        }

        private void FontCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectItem = FontCombox.SelectedItem as ComboBoxItem;
            if (selectItem != null && selectItem.Content != null)
            {
                LineEvent?.UpdateAttrib(AnnotAttrib.FontName, selectItem.Content.ToString());
                LineEvent?.UpdateAnnot();
            }
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            SolidColorBrush checkBrush = BorderColorPickerControl.GetBrush() as SolidColorBrush;
            if (checkBrush != null)
            {
                LineEvent?.UpdateAttrib(AnnotAttrib.Color, checkBrush.Color);
                LineEvent?.UpdateAnnot();
            }
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            LineEvent?.UpdateAttrib(AnnotAttrib.Transparency, CPDFOpacityControl.OpacityValue/100D);
            LineEvent?.UpdateAnnot();
        }

        public void SetAnnotEventData(AnnotAttribEvent annotEvent)
        {
            LineEvent = null;
            if(annotEvent!=null)
            {
                foreach(AnnotAttrib attrib in annotEvent.Attribs.Keys)
                {
                    switch(attrib)
                    {
                        case AnnotAttrib.Color:
                            BorderColorPickerControl.SetCheckedForColor((Color)annotEvent.Attribs[attrib]);
                            break;
                        case AnnotAttrib.Transparency:
                            double transparennt= Convert.ToDouble(annotEvent.Attribs[attrib]);
                            if(transparennt>1)
                            {
                                transparennt =(transparennt / 255D);
                            }
                            CPDFOpacityControl.OpacityValue = (int)(transparennt*100);
                            break;
                        case AnnotAttrib.Thickness:
                            CPDFThicknessControl.Thickness = Convert.ToInt16(annotEvent.Attribs[attrib]);
                            break;
                        case AnnotAttrib.LineStyle:
                            CPDFLineStyleControl.DashStyle= (DashStyle)(annotEvent.Attribs[attrib]);
                            break;
                        case AnnotAttrib.FontColor:
                            FontColorPickerControl.SetCheckedForColor((Color)annotEvent.Attribs[attrib]);
                            break;
                        case AnnotAttrib.FontName:
                            {
                                string fontName= (string)annotEvent.Attribs[AnnotAttrib.FontName];
                                if (fontName.Contains("Courier"))
                                {
                                    FontCombox.SelectedIndex = 1;
                                }
                                else if (fontName == "Arial" || fontName.Contains("Helvetica"))
                                {
                                    FontCombox.SelectedIndex = 0;

                                }
                                else if (fontName.Contains("Times"))
                                {
                                    FontCombox.SelectedIndex = 2;
                                }
                                else
                                {
                                    FontCombox.SelectedIndex = -1;
                                }
                            }
                            break;
                        case AnnotAttrib.FontSize:
                            SetFontSize(Convert.ToDouble(annotEvent.Attribs[attrib]));
                            break;
                        case AnnotAttrib.NoteText:
                            NoteTextBox.Text= annotEvent.Attribs[attrib].ToString();
                            break;
                        default:
                            break;
                    }
                }

                C_LINE_TYPE headLineType = C_LINE_TYPE.LINETYPE_NONE;
                C_LINE_TYPE tailLineType = C_LINE_TYPE.LINETYPE_NONE;

                if(annotEvent.Attribs.ContainsKey(AnnotAttrib.LineStart))
                {
                    headLineType = (C_LINE_TYPE)annotEvent.Attribs[AnnotAttrib.LineStart];
                }
                if (annotEvent.Attribs.ContainsKey(AnnotAttrib.LineEnd))
                {
                    tailLineType = (C_LINE_TYPE)annotEvent.Attribs[AnnotAttrib.LineEnd];
                }
                LineType lineType = new LineType()
                {
                    HeadLineType = headLineType,
                    TailLineType = tailLineType
                };
                CPDFArrowControl.LineType = lineType;

                bool isBold=false; 
                bool isItalic=false;
                if (annotEvent.Attribs.ContainsKey(AnnotAttrib.IsBold))
                {
                    isBold = (bool)annotEvent.Attribs[AnnotAttrib.IsBold];
                }
                if (annotEvent.Attribs.ContainsKey(AnnotAttrib.IsItalic))
                {
                    isItalic = (bool)annotEvent.Attribs[AnnotAttrib.IsItalic];
                }

                SetFontStyle(isBold, isItalic);
            }

            LineEvent =annotEvent;
        }

        public void SetAnnotArgsData(LineMeasureArgs annotArgs)
        {
            Dictionary<AnnotAttrib, object> attribDict = new Dictionary<AnnotAttrib, object>();
            attribDict[AnnotAttrib.Color] = annotArgs.LineColor;
            attribDict[AnnotAttrib.Transparency] = annotArgs.Transparency;
            attribDict[AnnotAttrib.Thickness] = annotArgs.LineWidth;
            attribDict[AnnotAttrib.LineStyle] = annotArgs.LineDash;
            attribDict[AnnotAttrib.LineStart] = annotArgs.HeadLineType;
            attribDict[AnnotAttrib.LineEnd] = annotArgs.TailLineType;
            attribDict[AnnotAttrib.FontColor] = annotArgs.FontColor;
            attribDict[AnnotAttrib.FontName] = annotArgs.FontName;
            attribDict[AnnotAttrib.IsBold] = annotArgs.IsBold;
            attribDict[AnnotAttrib.IsItalic] = annotArgs.IsItalic;
            attribDict[AnnotAttrib.FontSize] = annotArgs.FontSize;
            attribDict[AnnotAttrib.NoteText] = annotArgs.Content;

            AnnotAttribEvent annotEvent = AnnotAttribEvent.GetAnnotAttribEvent(annotArgs, attribDict);
            SetAnnotEventData(annotEvent);
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

        private void CPDFThicknessControl_ThicknessChanged(object sender, EventArgs e)
        {
            LineEvent?.UpdateAttrib(AnnotAttrib.Thickness, CPDFThicknessControl.Thickness);
            LineEvent?.UpdateAnnot();
        }

        private void CPDFLineStyleControl_LineStyleChanged(object sender, EventArgs e)
        {
            LineEvent?.UpdateAttrib(AnnotAttrib.LineStyle, CPDFLineStyleControl.DashStyle);
            LineEvent?.UpdateAnnot();
        }

        private void CPDFArrowControl_ArrowChanged(object sender, EventArgs e)
        {
            LineEvent?.UpdateAttrib(AnnotAttrib.LineStart, CPDFArrowControl.LineType.HeadLineType);
            LineEvent?.UpdateAttrib(AnnotAttrib.LineEnd, CPDFArrowControl.LineType.TailLineType);
            LineEvent?.UpdateAnnot();
        }

        private void FontColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            SolidColorBrush checkBrush = FontColorPickerControl.GetBrush() as SolidColorBrush;
            if (checkBrush != null)
            {
                LineEvent?.UpdateAttrib(AnnotAttrib.FontColor, checkBrush.Color);
                LineEvent?.UpdateAnnot();
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
    }
}
