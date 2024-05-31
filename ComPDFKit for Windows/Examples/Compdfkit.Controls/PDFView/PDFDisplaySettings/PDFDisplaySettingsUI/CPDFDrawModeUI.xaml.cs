using ComPDFKitViewer;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFDrawModeUI : UserControl
    {
        public event EventHandler<string> SetDrawModeEvent;
        public event EventHandler<string> SetCustomDrawModeEvent;

        public event EventHandler<DrawModeData> DrawModeChanged;
        public CPDFDrawModeUI()
        {
            InitializeComponent();
        }

        private void DrawMode_Click(object sender, RoutedEventArgs e)
        {
            var drawModeRadioButton = sender as RadioButton;
            SetDrawModeEvent?.Invoke(sender, drawModeRadioButton.Tag as string);
        }

        private void DrawModeNormal_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawMode.Normal
            }) ;
        }

        private void DrawModeSoft_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawMode.Soft
            });
        }

        private void DrawModeDark_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawMode.Dark
            });
        }

        private void DrawModeGreen_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawMode.Green
            });
        }

        private void DrawModeOrange_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawMode.Custom,
                CustomColor= 0xFFFFE390
            });
        }

        private void DrawModeLightBlue_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawMode.Custom,
                CustomColor= 0xFFC1E6FF
            });
        }
    }

    public class DrawModeData
    {
        public DrawMode DrawMode { get; set; }
        public uint CustomColor { get; set; }
    }
}
