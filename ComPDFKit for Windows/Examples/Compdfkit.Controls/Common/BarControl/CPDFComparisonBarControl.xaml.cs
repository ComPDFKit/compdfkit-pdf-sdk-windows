using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.Common
{
    public partial class CPDFComparisonBarControl : UserControl
    {
        public event EventHandler<ComparisonAction> ComparisonActionChanged;

        public enum ComparisonAction
        {
            ContentComparison,
            OverlayComparison
        }

        private string content = LanguageHelper.ToolBarManager.GetString("Button_ContentComparison");
        private string overlay = LanguageHelper.ToolBarManager.GetString("Button_OverlayComparison");
        private bool isFirstLoad = true;
        private int counter = 0;
        Dictionary<string, string> ButtonDict;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
        private void ClearToolState(UIElement sender)
        {
            foreach (UIElement child in ComparisonBarGrid.Children)
            {
                if (child is ToggleButton toggle && ((child as ToggleButton) != sender))
                {
                    toggle.IsChecked = false;
                }
            }
        }
        
        public void ClearAllToolState()
        {
            foreach (UIElement child in ComparisonBarGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    toggle.IsChecked = false;
                }
            }
        }
        
        public CPDFComparisonBarControl()
        {
            ButtonDict = new Dictionary<string, string>
            {
                {content,"M6.88663 0.25H0.25V15.75H9.25H10.75H19.75V4.19386L15.8866 0.25H9.25V2.66261L6.88663 0.25ZM10.75 14.25H18.25V4.806L15.256 1.75H10.75V4.19386V14.25ZM7.32812 6.25L5.83984 10.25H4.43359L3.01562 6.25H4.33594L5.03125 8.71484C5.10938 8.99349 5.15495 9.23047 5.16797 9.42578H5.18359C5.20182 9.24089 5.25 9.01172 5.32812 8.73828L6.03906 6.25H7.32812ZM12.9802 10.1445V9.14453C13.1833 9.26693 13.3851 9.35807 13.5856 9.41797C13.7887 9.47786 13.9802 9.50781 14.1598 9.50781C14.3786 9.50781 14.5505 9.47786 14.6755 9.41797C14.8031 9.35807 14.8669 9.26693 14.8669 9.14453C14.8669 9.06641 14.8382 9.0013 14.7809 8.94922C14.7236 8.89714 14.6494 8.85156 14.5583 8.8125C14.4697 8.77344 14.3721 8.73828 14.2653 8.70703C14.1585 8.67578 14.0557 8.64062 13.9567 8.60156C13.7979 8.54167 13.6572 8.47786 13.5348 8.41016C13.415 8.33984 13.3135 8.25911 13.2302 8.16797C13.1494 8.07682 13.0869 7.97135 13.0427 7.85156C13.001 7.73177 12.9802 7.58984 12.9802 7.42578C12.9802 7.20182 13.0283 7.00911 13.1247 6.84766C13.2236 6.6862 13.3538 6.55469 13.5153 6.45312C13.6794 6.34896 13.8656 6.27344 14.0739 6.22656C14.2848 6.17708 14.5036 6.15234 14.7302 6.15234C14.9072 6.15234 15.0869 6.16667 15.2692 6.19531C15.4515 6.22135 15.6312 6.26042 15.8083 6.3125V7.26562C15.652 7.17448 15.4841 7.10677 15.3044 7.0625C15.1273 7.01562 14.9528 6.99219 14.7809 6.99219C14.7002 6.99219 14.6234 7 14.5505 7.01562C14.4802 7.02865 14.4177 7.04948 14.363 7.07812C14.3083 7.10417 14.2653 7.13932 14.2341 7.18359C14.2028 7.22526 14.1872 7.27344 14.1872 7.32812C14.1872 7.40104 14.2106 7.46354 14.2575 7.51562C14.3044 7.56771 14.3656 7.61328 14.4411 7.65234C14.5166 7.6888 14.5999 7.72266 14.6911 7.75391C14.7848 7.78255 14.8773 7.8125 14.9684 7.84375C15.1325 7.90104 15.2809 7.96354 15.4137 8.03125C15.5466 8.09896 15.6598 8.17839 15.7536 8.26953C15.8499 8.36068 15.9229 8.46745 15.9723 8.58984C16.0244 8.71224 16.0505 8.85807 16.0505 9.02734C16.0505 9.26432 15.9984 9.46745 15.8942 9.63672C15.7927 9.80339 15.6559 9.9401 15.4841 10.0469C15.3148 10.151 15.1182 10.2266 14.8942 10.2734C14.6729 10.3229 14.4424 10.3477 14.2028 10.3477C13.7627 10.3477 13.3552 10.2799 12.9802 10.1445Z"},
                {overlay, "M3.75 0.75H13.3866L17.25 4.69386V16.25H14.25V19.25H0.75V3.75H3.75V0.75ZM3.75 16.25H12.75V17.75H2.25V5.25H3.75V16.25ZM15.75 5.306L12.756 2.25H5.25V14.75H15.75V5.306ZM9.83984 11L11.3281 7H10.0391L9.32812 9.48828C9.25 9.76172 9.20182 9.99089 9.18359 10.1758H9.16797C9.15495 9.98047 9.10938 9.74349 9.03125 9.46484L8.33594 7H7.01562L8.43359 11H9.83984ZM11.5938 9.89453V10.8945C11.9688 11.0299 12.3763 11.0977 12.8164 11.0977C13.056 11.0977 13.2865 11.0729 13.5078 11.0234C13.7318 10.9766 13.9284 10.901 14.0977 10.7969C14.2695 10.6901 14.4062 10.5534 14.5078 10.3867C14.612 10.2174 14.6641 10.0143 14.6641 9.77734C14.6641 9.60807 14.638 9.46224 14.5859 9.33984C14.5365 9.21745 14.4635 9.11068 14.3672 9.01953C14.2734 8.92839 14.1602 8.84896 14.0273 8.78125C13.8945 8.71354 13.7461 8.65104 13.582 8.59375C13.4909 8.5625 13.3984 8.53255 13.3047 8.50391C13.2135 8.47266 13.1302 8.4388 13.0547 8.40234C12.9792 8.36328 12.918 8.31771 12.8711 8.26562C12.8242 8.21354 12.8008 8.15104 12.8008 8.07812C12.8008 8.02344 12.8164 7.97526 12.8477 7.93359C12.8789 7.88932 12.9219 7.85417 12.9766 7.82812C13.0312 7.79948 13.0938 7.77865 13.1641 7.76562C13.237 7.75 13.3138 7.74219 13.3945 7.74219C13.5664 7.74219 13.7409 7.76562 13.918 7.8125C14.0977 7.85677 14.2656 7.92448 14.4219 8.01562V7.0625C14.2448 7.01042 14.0651 6.97135 13.8828 6.94531C13.7005 6.91667 13.5208 6.90234 13.3438 6.90234C13.1172 6.90234 12.8984 6.92708 12.6875 6.97656C12.4792 7.02344 12.293 7.09896 12.1289 7.20312C11.9674 7.30469 11.8372 7.4362 11.7383 7.59766C11.6419 7.75911 11.5938 7.95182 11.5938 8.17578C11.5938 8.33984 11.6146 8.48177 11.6562 8.60156C11.7005 8.72135 11.763 8.82682 11.8438 8.91797C11.9271 9.00911 12.0286 9.08984 12.1484 9.16016C12.2708 9.22786 12.4115 9.29167 12.5703 9.35156C12.6693 9.39062 12.7721 9.42578 12.8789 9.45703C12.9857 9.48828 13.0833 9.52344 13.1719 9.5625C13.263 9.60156 13.3372 9.64714 13.3945 9.69922C13.4518 9.7513 13.4805 9.81641 13.4805 9.89453C13.4805 10.0169 13.4167 10.1081 13.2891 10.168C13.1641 10.2279 12.9922 10.2578 12.7734 10.2578C12.5938 10.2578 12.4023 10.2279 12.1992 10.168C11.9987 10.1081 11.7969 10.0169 11.5938 9.89453Z"},
            };
            InitializeComponent();
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstLoad)
            {
                foreach (KeyValuePair<string, string> data in ButtonDict)
                {
                    string Path = data.Value;
                    string name = data.Key;

                    Geometry annotationGeometry = Geometry.Parse(Path);
                    Path path = new Path
                    {
                        Width = 20,
                        Height = 20,
                        Data = annotationGeometry,
                        Fill = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D))
                    };
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
            if (name == content)
            {
                button.Click -= ContentComparisonBtn_Click;
                button.Click += ContentComparisonBtn_Click;
            }
            else if (name == overlay)
            {
                button.Click -= OverlayComparisonBtn_Click;
                button.Click += OverlayComparisonBtn_Click;
            }
            ComparisonBarGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ComparisonBarGrid.Width += 180;
            Grid.SetColumn(button, counter++);
            ComparisonBarGrid.Children.Add(button);
        }

        private void ContentComparisonBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearToolState(null);
            ComparisonActionChanged?.Invoke(this, ComparisonAction.ContentComparison);
        }
        
        private void OverlayComparisonBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearToolState(null);
            ComparisonActionChanged?.Invoke(this, ComparisonAction.OverlayComparison);
        }
    }
}