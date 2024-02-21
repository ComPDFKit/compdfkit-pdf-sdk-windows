using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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

namespace Compdfkit_Tools.PDFControlUI
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
                DrawMode = DrawModes.Draw_Mode_Normal
            }) ;
        }

        private void DrawModeSoft_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawModes.Draw_Mode_Soft
            });
        }

        private void DrawModeDark_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawModes.Draw_Mode_Dark
            });
        }

        private void DrawModeGreen_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawModes.Draw_Mode_Green
            });
        }

        private void DrawModeOrange_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawModes.Draw_Mode_Custom,
                CustomColor= 0xFFFFE390
            });
        }

        private void DrawModeLightBlue_Click(object sender, RoutedEventArgs e)
        {
            DrawModeChanged?.Invoke(sender, new DrawModeData()
            {
                DrawMode = DrawModes.Draw_Mode_Custom,
                CustomColor= 0xFFC1E6FF
            });
        }
    }

    public class DrawModeData
    {
        public DrawModes DrawMode { get; set; }
        public uint CustomColor { get; set; }
    }
}
