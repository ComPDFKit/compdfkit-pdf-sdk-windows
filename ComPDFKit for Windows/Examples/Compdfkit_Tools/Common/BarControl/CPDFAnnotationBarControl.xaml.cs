using Compdfkit_Tools.Data;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFAnnotationBarControl : UserControl
    {
        private int annotationCounter = 0;

        public event EventHandler<CPDFAnnotationType> AnnotationPropertyChanged;
        public event EventHandler AnnotationCancel;

        public string CurrentMode = string.Empty;

        public CPDFAnnotationBarControl()
        {
            InitializeComponent();
        }

        private void CreateAnnotationButton(ToggleButton toggleButton)
        {
            Style style = (Style)FindResource("RoundMarginToggleButtonStyle");
            toggleButton.BorderThickness = new Thickness(0);
            toggleButton.Height = 50;
            toggleButton.Style = style;
            toggleButton.VerticalAlignment = VerticalAlignment.Center;

            Geometry annotationGeometry = Geometry.Parse("");
            Canvas canvas = new Canvas();
            if (toggleButton.Tag.ToString() == CPDFAnnotationType.Highlight.ToString())
            {
                annotationGeometry = Geometry.Parse("M15.6078 15.5L10.6964 3.22144H9.30364L4.39223 15.5H3V16.5H7V15.5H6.00776L7.10772 12.75H12.8923L13.9922 15.5H13V16.5H17V15.5H15.6078ZM12.4923 11.75L10 5.51898L7.5077 11.75H12.4923Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                annotationGeometry = Geometry.Parse("M2 2H20L18 18H0L2 2Z");
                Path path2 = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF87C"))
                };
                canvas.Children.Add(path2);
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Underline.ToString())
            {
                annotationGeometry = Geometry.Parse("M13 14H13.9922L12.8923 11.25H7.10772L6.00776 14H7V15H3V14H4.39223L9.30364 1.72144H10.6964L15.6078 14H17V15H13V14ZM7.5077 10.25H12.4923L10 4.01898L7.5077 10.25Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                Rectangle rectangle = new Rectangle
                {
                    Width = 20,
                    Height = 1.5,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000")),
                    Margin = new Thickness(0, 17, 0, 0)
                };
                canvas.Children.Add(rectangle);
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Strikeout.ToString())
            {
                annotationGeometry = Geometry.Parse("M14.3494 15.3928H13.2139V16.3928H17.4996V15.3928H15.9649L10.6964 2.22144H9.30364L4.03508 15.3928H2.5V16.3928H6.78571V15.3928H5.65062L10 4.51898L14.3494 15.3928Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                Rectangle rectangle = new Rectangle
                {
                    Width = 20,
                    Height = 1.5,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000")),
                    Margin = new Thickness(0, 10.5, 0, 0)
                };
                canvas.Children.Add(rectangle);
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Squiggly.ToString())
            {
                annotationGeometry = Geometry.Parse("M13 14H13.9922L12.8923 11.25H7.10772L6.00776 14H7V15H3V14H4.39223L9.30364 1.72144H10.6964L15.6078 14H17V15H13V14ZM7.5077 10.25H12.4923L10 4.01898L7.5077 10.25Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                annotationGeometry = Geometry.Parse("M5.08299 17.9329L5.22219 18.02C6.28806 18.7528 7.65967 18.7913 8.75772 18.1357L8.93809 18.02C9.52914 17.6136 10.2863 17.5846 10.9011 17.9329L11.0403 18.02C12.1062 18.7528 13.4778 18.7913 14.5759 18.1357L14.7562 18.02C15.3473 17.6136 16.1045 17.5846 16.7193 17.9329L16.8585 18.02C17.0638 18.1611 17.2824 18.2778 17.5107 18.3687C17.8854 18.5178 18.3064 18.3254 18.451 17.939C18.5956 17.5525 18.4091 17.1184 18.0344 16.9693C17.9483 16.935 17.8647 16.8943 17.784 16.8472L17.6653 16.7719C16.5994 16.0391 15.2278 16.0005 14.1298 16.6562L13.9494 16.7719C13.3584 17.1782 12.6011 17.2073 11.9864 16.859L11.8472 16.7719C10.7813 16.0391 9.40969 16.0005 8.31164 16.6562L8.13126 16.7719C7.54022 17.1782 6.78301 17.2073 6.16823 16.859L6.02902 16.7719C4.96316 16.0391 3.59155 16.0005 2.4935 16.6562L2.31312 16.7719C2.23868 16.8231 2.16125 16.8685 2.08133 16.9081L1.95963 16.963C1.5862 17.1155 1.40339 17.5514 1.55131 17.9365C1.69922 18.3216 2.12186 18.5101 2.49529 18.3576C2.64087 18.2981 2.78242 18.2281 2.91898 18.1479L3.11995 18.02C3.711 17.6136 4.46821 17.5846 5.08299 17.9329Z");
                Path path2 = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"))
                };
                canvas.Children.Add(path2);
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.FreeText.ToString())
            {
                annotationGeometry = Geometry.Parse("M16.75 2.75V6H15.25V4.25H10.75V16.25H12.3337V17.75H7.66699V16.25H9.25V4.25H4.75V6H3.25V2.75H16.75Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Note.ToString())
            {
                annotationGeometry = Geometry.Parse("M18 3H2V15H5V18L10 15H18V3ZM5 6H11V7.5H5V6ZM5 9.5H15V11H5V9.5Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Square.ToString())
            {
                annotationGeometry = Geometry.Parse("M18.75 3.25H1.25V16.75H18.75V3.25ZM17.25 4.75V15.25H2.75V4.75H17.25Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Circle.ToString())
            {
                annotationGeometry = Geometry.Parse("M10 1.75C5.44365 1.75 1.75 5.44365 1.75 10C1.75 14.5563 5.44365 18.25 10 18.25C14.5563 18.25 18.25 14.5563 18.25 10C18.25 5.44365 14.5563 1.75 10 1.75ZM10 3.25C13.7279 3.25 16.75 6.27208 16.75 10C16.75 13.7279 13.7279 16.75 10 16.75C6.27208 16.75 3.25 13.7279 3.25 10C3.25 6.27208 6.27208 3.25 10 3.25Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Arrow.ToString())
            {
                annotationGeometry = Geometry.Parse("M16.7501 10V3.25H10.0001V4.75H14.1894L1.46973 17.4697L2.53039 18.5303L15.2501 5.81066V10H16.7501Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Line.ToString())
            {
                annotationGeometry = Geometry.Parse("M16.4697 3.46973L17.5304 4.53039L3.53039 18.5304L2.46973 17.4697L16.4697 3.46973Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Freehand.ToString())
            {
                annotationGeometry = Geometry.Parse("M1.57675 19.1757L1 17.7787L2.23629 17.3267L3.39858 16.9142L4.48804 16.5416L5.17445 16.3153L5.82936 16.1069L6.4531 15.9162L7.04603 15.7435L7.60849 15.5886L8.14082 15.4517L8.64337 15.3328C8.72467 15.3145 8.80474 15.297 8.88359 15.2802L9.34212 15.1884L9.77173 15.1147C11.0868 14.91 11.972 14.9785 12.4769 15.3262C12.7932 15.544 13.0243 15.7942 13.1655 16.0808C13.4151 16.5876 13.3721 16.9889 13.1388 17.5664L13.0118 17.8607L13.1178 17.8549C13.3318 17.8376 13.6024 17.7943 13.926 17.7239L14.1265 17.6785L14.3516 17.6235L14.8394 17.4928C14.9249 17.4688 15.0125 17.4435 15.1022 17.4171L15.6652 17.245L16.2781 17.0452L16.9408 16.8178L17.653 16.5629L18.4146 16.2803L19 17.6741L17.9462 18.0639L17.2946 18.2959L16.6828 18.5054L16.1097 18.6922C15.925 18.7507 15.7466 18.8054 15.5744 18.8563L15.076 18.9974C14.9159 19.0406 14.7618 19.0799 14.6135 19.1154L14.186 19.2101C13.0235 19.4455 12.271 19.399 11.8322 19.0549C11.4563 18.7602 11.3001 18.3582 11.3527 17.9256C11.3673 17.8051 11.3855 17.7161 11.4219 17.6072L11.4922 17.4224L11.6818 16.9751L11.7255 16.8448C11.7439 16.7759 11.7424 16.7393 11.7305 16.715C11.7102 16.674 11.6563 16.6156 11.5457 16.5395C11.436 16.4639 11.2053 16.4374 10.8597 16.4606L10.6123 16.4834C10.5683 16.4885 10.5228 16.4941 10.476 16.5004L10.1783 16.5455L9.84745 16.6054L9.48395 16.6803L9.08816 16.7701C9.01952 16.7863 8.94956 16.8031 8.87827 16.8206L8.43479 16.933L7.95999 17.0604L7.45428 17.203L6.63861 17.4454L5.75561 17.7221L4.80659 18.0333L3.79289 18.3792L3.08181 18.6292L2.34296 18.8946L1.57675 19.1757Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64BC38"))
                };
                annotationGeometry = Geometry.Parse("M12.7275 1L16.9702 5.24264L14.8385 7.37437L10.5958 3.13173L12.7275 1ZM9.8887 3.83883L3.53515 10.1924L2.12094 15.8492L7.77779 14.435L14.1313 8.08148L9.8887 3.83883Z");
                Path path2 = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path2);
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Image.ToString())
            {

                annotationGeometry = Geometry.Parse("M18.75,3.25 L18.75,16.75 L1.25,16.75 L1.25,3.25 L18.75,3.25 Z M17.25,4.75 L2.75,4.75 L2.75,13.209 L5.79495643,10 L10.0642824,13.5 L12.3718478,11.0677556 L17.144,15.25 L17.25,15.25 L17.25,4.75 Z M14.5,6 C15.3284271,6 16,6.67157288 16,7.5 C16,8.32842712 15.3284271,9 14.5,9 C13.6715729,9 13,8.32842712 13,7.5 C13,6.67157288 13.6715729,6 14.5,6 Z");
                Path imagePath = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(imagePath);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Stamp.ToString())
            {
                annotationGeometry = Geometry.Parse("M11.982 9.97521C13.1875 9.28619 14 7.98798 14 6.5C14 4.29086 12.2091 2.5 10 2.5C7.79086 2.5 6 4.29086 6 6.5C6 7.98798 6.81248 9.2862 8.018 9.97522C8.10745 10.1032 8.5 10.6962 8.5 11.25C8.5 11.875 8 12.5 8 12.5H2V14H18V12.5H12C12 12.5 11.5 11.875 11.5 11.25C11.5 10.6962 11.8926 10.1032 11.982 9.97521ZM18 17.5V16H2V17.5H18Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Signature.ToString())
            {
                annotationGeometry = Geometry.Parse("M17.092 5.65678L12.1422 0.707031L8.96023 5.30323L5.07114 6.36389C5.07114 6.36389 4.87159 8.08671 4.36403 9.89942C4.0046 11.1831 2.91313 13.6628 2.30666 15.0009C1.93743 14.8313 1.48617 14.8986 1.18205 15.2027C0.791529 15.5932 0.791529 16.2264 1.18205 16.6169C1.57258 17.0075 2.20574 17.0075 2.59627 16.6169C2.90681 16.3064 2.97041 15.8424 2.78706 15.469C3.97379 14.9024 6.12199 13.9327 7.89957 13.435C10.5063 12.7051 11.4351 12.7278 11.4351 12.7278L12.4958 8.83876L17.092 5.65678ZM15.7544 15.3443C15.4496 14.8379 14.8798 14.6444 14.0655 14.6631C12.7327 14.6939 10.2695 15.3124 6.2244 16.6694L4.96295 17.0988L5.45212 18.5168L5.87 18.3732C9.99614 16.9622 12.5477 16.2788 13.8551 16.1757L14.1001 16.1627L14.2221 16.1623C14.2977 16.1636 14.3621 16.168 14.4154 16.1747L14.4251 16.1759L14.3479 16.5492C14.2867 16.8553 14.2743 17.0166 14.3107 17.2491C14.3866 17.7345 14.7168 18.0933 15.2254 18.2317C15.8846 18.4112 16.9866 18.246 18.8476 17.6997L19.429 17.5243L18.9861 16.0912L18.7157 16.1738C17.3039 16.5995 16.3793 16.792 15.898 16.8063L15.8261 16.8059L15.8979 16.4572C15.9814 15.9965 15.9657 15.6954 15.7544 15.3443ZM9.66733 9.54587L8.25312 8.13165L6.1318 11.6672L9.66733 9.54587Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Link.ToString())
            {
                annotationGeometry = Geometry.Parse("M18.8388 7.17154L14.331 11.6793L13.2703 10.6187L16.7175 7.17154L12.8284 3.28245L9.38125 6.7296L8.32059 5.66894L12.8284 1.16113L18.8388 7.17154ZM6.7296 9.38125L3.28245 12.8284L7.17154 16.7175L10.6187 13.2703L11.6793 14.331L7.17154 18.8388L1.16113 12.8284L5.66894 8.32059L6.7296 9.38125ZM9.11608 11.9445L11.9445 9.11608L10.8839 8.05542L8.05542 10.8839L9.11608 11.9445Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            else if (toggleButton.Tag.ToString() == CPDFAnnotationType.Audio.ToString())
            {
                annotationGeometry = Geometry.Parse("M11.1897 2V3.29894V15.9565V17.2555L10.0647 16.6061L5.85337 14.175H2.75H2V13.425V5.83046V5.08046H2.75H5.85337L10.0647 2.6494L11.1897 2ZM9.68967 4.59788L6.42926 6.48L6.25524 6.58046H6.05431H3.5V12.675H6.05431H6.25524L6.42926 12.7755L9.68967 14.6576V4.59788ZM17.2074 9.62774C17.2074 7.23851 15.2922 5.31471 12.9456 5.31471V3.81471C16.1349 3.81471 18.7074 6.42449 18.7074 9.62774C18.7074 12.831 16.1349 15.4408 12.9456 15.4408V13.9408C15.2922 13.9408 17.2074 12.017 17.2074 9.62774ZM12.9456 9.112C13.2168 9.112 13.4491 9.33604 13.4491 9.62776C13.4491 9.91947 13.2168 10.1435 12.9456 10.1435V11.6435C14.0589 11.6435 14.9491 10.7342 14.9491 9.62776C14.9491 8.52136 14.0589 7.612 12.9456 7.612V9.112Z");
                Path path = new Path
                {
                    Width = 20,
                    Height = 20,
                    Data = annotationGeometry,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"))
                };
                canvas.Children.Add(path);
            }

            if (canvas.Children.Count != 0)
            {
                canvas.Height = 20;
                canvas.Width = 20;
                canvas.HorizontalAlignment = HorizontalAlignment.Center;
                canvas.VerticalAlignment = VerticalAlignment.Center;
                toggleButton.Content = canvas;
                toggleButton.Click += ToggleButton_Click;
                Grid.SetColumn(toggleButton, annotationCounter++);
                AnnotationGrid.Children.Add(toggleButton);
            }
        }

        public void ClearAllToolState()
        {
            foreach (UIElement child in AnnotationGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        private void ClearToolState(UIElement sender)
        {
            foreach (UIElement child in AnnotationGrid.Children)
            {
                if (child is ToggleButton toggle && (child as ToggleButton) != (sender as ToggleButton))
                {
                    toggle.IsChecked = false;
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ClearToolState(sender as ToggleButton);

            if ((bool)(sender as ToggleButton).IsChecked)
            {
                AnnotationPropertyChanged?.Invoke(sender, CPDFAnnotationDictionary.GetAnnotationFromTag[(sender as ToggleButton).Tag.ToString()]);
                CurrentMode = (sender as ToggleButton).Tag.ToString();
            }
            else
            {
                AnnotationCancel?.Invoke(sender, EventArgs.Empty);
                CurrentMode = string.Empty;
            }
        }

        private ToggleButton FindToggleButtonByTag(DependencyObject parent, string tag)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is ToggleButton toggleButton && toggleButton.Tag != null && toggleButton.Tag.ToString() == tag)
                {
                    return toggleButton;
                }
                else
                {
                    var result = FindToggleButtonByTag(child, tag);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public void SetAnnotationType(CPDFAnnotationType annotationType)
        { 
            ToggleButton toggleButton = FindToggleButtonByTag(this, annotationType.ToString());
            if (toggleButton != null)
            {
                toggleButton.IsChecked = true;
                ClearToolState(toggleButton);
                AnnotationPropertyChanged?.Invoke(toggleButton, CPDFAnnotationDictionary.GetAnnotationFromTag[toggleButton.Tag.ToString()]);
                CurrentMode = toggleButton.Tag.ToString();
            } 
        }

        public void InitAnnotationBar(CPDFAnnotationType[] annotationProperties)
        {
            for (int i = 0; i < annotationProperties.Length; i++)
            {
                AnnotationGrid.ColumnDefinitions.Add(new ColumnDefinition());
                CPDFAnnotationType annotation = annotationProperties[i];
                ToggleButton toggleButton = new ToggleButton
                {
                    Tag = annotation.ToString(),
                    ToolTip = LanguageHelper.ToolBarManager.GetString("Tooltip_" + annotation)
                };
                CreateAnnotationButton(toggleButton);
                AnnotationGrid.Width += 50;
            }
        }
    }
}
