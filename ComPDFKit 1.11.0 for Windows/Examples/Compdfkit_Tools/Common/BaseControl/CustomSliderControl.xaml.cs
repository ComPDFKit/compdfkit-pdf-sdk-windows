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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Compdfkit_Tools.Common
{
    public partial class CustomSliderControl : UserControl
    {
        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty SliderWidthProperty =
            DependencyProperty.Register("SliderWidth", typeof(double), typeof(CustomSliderControl), new UIPropertyMetadata());
        public double SliderWidth
        {
            get { return (double)GetValue(SliderWidthProperty); }
            set { SetValue(SliderWidthProperty, value); }
        }

        public static readonly DependencyProperty SliderValueProperty =
            DependencyProperty.Register("SliderValue", typeof(int), typeof(CustomSliderControl), new UIPropertyMetadata());
        public int SliderValue
        {
            get { return (int)GetValue(SliderValueProperty); }
            set { SetValue(SliderValueProperty, value); }
        }

        // The dependency property which will be accessible on the UserControl
        public static readonly DependencyProperty MaxiumProperty =
            DependencyProperty.Register("Maxium", typeof(int), typeof(CustomSliderControl), new UIPropertyMetadata());
        public int Maxium
        {
            get { return (int)GetValue(MaxiumProperty); }
            set { SetValue(MaxiumProperty, value); }
        }
        public CustomSliderControl()
        {
            InitializeComponent();
        }
    }
}
