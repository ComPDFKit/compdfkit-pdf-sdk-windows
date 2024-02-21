using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFPageEditBarControl : UserControl, INotifyPropertyChanged
    {
        #region Data

        private string insert = LanguageHelper.ToolBarManager.GetString("Button_Insert");
        private string replace = LanguageHelper.ToolBarManager.GetString("Button_Replace");
        private string extract = LanguageHelper.ToolBarManager.GetString("Button_Extract");
        private string reverse = LanguageHelper.ToolBarManager.GetString("Button_Reverse");
        private string copy = LanguageHelper.ToolBarManager.GetString("Button_Copy");
        private string rotate = LanguageHelper.ToolBarManager.GetString("Button_Rotate");
        private string delete = LanguageHelper.ToolBarManager.GetString("Button_Delete");
        
        Dictionary<string, string> ButtonDict = new Dictionary<string, string>
        {
            {"Insert","M0.25 0.25H11.3107L15.75 4.68934V10H14.25V5.311L10.689 1.75H1.75V16.25H8V17.75H0.25V0.25ZM12.75 17H11.25V14.75H9V13.25H11.25V11H12.75V13.25H15V14.75H12.75V17Z"},
            {"Replace", "M0 0H11.0607L15.5 4.43934V9.75H14V5.061L10.439 1.5H1.5V16H7.75V17.5H0V0ZM12.1366 11.7474L13.1726 10.75L16.75 13.75H8.75V12.3857H12.8974L12.1366 11.7474ZM12.3274 18.25L13.3634 17.2526L12.6026 16.6143H16.75V15.25H8.75L12.3274 18.25Z"},
            {"Extract","M0.25 0.25H11.3107L15.75 4.68934V10H14.25V5.311L10.689 1.75H1.75V16.25H8V17.75H0.25V0.25ZM14.0174 15.25L13.1768 16.091L14.2374 17.1516L16.8891 14.5L14.2374 11.8483L13.1768 12.909L14.0174 13.75H9V15.25H14.0174Z"},
            {"Separator", "90" },
           // {"Reverse","M9.90193 0.75H0.25V17.25H13.75V4.95882L9.90193 0.75ZM9.24 2.25L12.25 5.542V15.75H1.75V2.25H9.24ZM18.624 5.83397L15.25 0.772918V17.25H16.75V5.729L17.376 6.66603L18.624 5.83397Z"},
            {"Copy","M3.75 0.75H13.3866L17.25 4.69386V16.25H14.25V19.25H0.75V3.75H3.75V0.75ZM3.75 16.25H12.75V17.75H2.25V5.25H3.75V16.25ZM15.75 5.306L12.756 2.25H5.25V14.75H15.75V5.306ZM13.3428 9.77832C13.165 11.1685 12.0986 12.1333 10.3911 12.1333C8.37891 12.1333 7.14746 10.7749 7.14746 8.52148C7.14746 6.30615 8.37256 4.9541 10.3848 4.9541C12.1177 4.9541 13.165 5.96338 13.3428 7.34717H12.0161C11.8574 6.67432 11.2988 6.17285 10.3848 6.17285C9.24219 6.17285 8.54395 7.04883 8.54395 8.52148C8.54395 10.0195 9.24854 10.9146 10.3848 10.9146C11.248 10.9146 11.8384 10.521 12.0161 9.77832H13.3428Z"},
            {"Rotate","M9 8.75V0.75H0.5V8.75V9.5V17.25H17V8.75H9ZM7.5 8.75V2.25H2V8.75H7.5ZM15.5 10.25V15.75H1.999V10.25H15.5ZM17.4623 4.46967L18.523 5.53033L15.8713 8.18198L13.2197 5.53033L14.2803 4.46967L15.1944 5.38355C15.1061 4.03216 14.1724 2.82648 12.7916 2.4565C12.0174 2.24905 11.214 2.33465 10.5138 2.68044L10.3255 2.78117L9.57546 1.48213C10.6588 0.856654 11.9471 0.677284 13.1798 1.00761C15.1834 1.54445 16.5429 3.28512 16.6886 5.24287L17.4623 4.46967Z"},
            {"Delete", "M12 1.75V0.25H6V1.75H12ZM18 4.75V3.25H15.75H2.25H0V4.75H2.25V17.75H15.75V4.75H18ZM3.75 16.25V4.75H14.25V16.25H3.75ZM7.75 7.5V13.5H6.25V7.5H7.75ZM11.75 7.5V13.5H10.25V7.5H11.75Z"},
        };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private CPDFViewer pdfViewer;

        public event EventHandler<string> PageEditEvent;
        #endregion

        #region Create Dafault UI
        public CPDFPageEditBarControl()
        {
            InitializeComponent();
            CreateButton();
        }

        private void CreateButton()
        {
            foreach (KeyValuePair<string, string> data in ButtonDict)
            {
                string Path = data.Value;
                string name = data.Key;

                if (string.Equals(name, "Separator"))
                {
                    CreateSeparator(Convert.ToDouble(Path));
                }
                else
                {
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
                textBlock.Text = GetToolString(name);
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
            button.ToolTip = GetToolString(name);
            button.Style = style;
            button.Content = stackPanel;
            button.Click += PageEditBtn_Click;
            FormGrid.Children.Add(button);
        }
        
        private string GetToolString(string name)
        {
            string tooltip = string.Empty;
            switch (name)
            {
                case "Insert":
                    tooltip = insert;
                    break;
                case "Replace":
                    tooltip = replace;
                    break;
                case "Extract":
                    tooltip = extract;
                    break;
                case "Reverse":
                    tooltip = reverse;
                    break;
                case "Copy":
                    tooltip = copy;
                    break;
                case "Rotate":
                    tooltip = rotate;
                    break;
                case "Delete":
                    tooltip = delete;
                    break;
                default:
                    break;
            }
            return tooltip;
        }

        private void PageEditBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button != null)
            {
                PageEditEvent?.Invoke(sender, button.Tag.ToString());
            }
            ClearToolState();
        }

        private void CreateSeparator(double rotate)
        {
            RotateTransform rotateTransform = new RotateTransform();
            rotateTransform.Angle = rotate;

            Separator separator = new Separator();
            separator.Height = 12;
            separator.Width = 20;
            separator.HorizontalAlignment = HorizontalAlignment.Center;
            separator.VerticalAlignment = VerticalAlignment.Center;
            separator.LayoutTransform = rotateTransform;
            FormGrid.Children.Add(separator);
        }

        private void ClearToolState()
        {
            foreach (UIElement child in FormGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        #endregion

    }
}
