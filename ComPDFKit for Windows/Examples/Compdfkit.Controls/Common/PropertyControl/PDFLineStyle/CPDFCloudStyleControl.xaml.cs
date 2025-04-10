using ComPDFKit.PDFAnnotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace ComPDFKit.Controls.Common
{
    /// <summary>
    /// Interaction logic for CPDFCloudStyleControl.xaml
    /// </summary>
    public partial class CPDFCloudStyleControl : UserControl
    {
        private const string streat = "Streat";
        private const string cloud1 = "Cloud1";
        private const string cloud2 = "Cloud2";

        private bool isChecked = false;

        public event EventHandler<CPDFBorderEffector> LineShapeChanged;

        private CPDFBorderEffector _borderEffector = new CPDFBorderEffector(C_BORDER_TYPE.C_BORDER_TYPE_CLOUD, C_BORDER_INTENSITY.C_INTENSITY_TWO);
        public CPDFBorderEffector BorderEffector
        {
            get
            {
                return _borderEffector;
            }
            set
            {
                _borderEffector = value;
                  
                rdoStreat.Checked -= rdoShape_Checked;
                rdoCloud1.Checked -= rdoShape_Checked;
                rdoCloud2.Checked -= rdoShape_Checked;

                if (_borderEffector == null)
                {
                    rdoStreat.IsChecked = true;
                }
                else if (_borderEffector.BorderType == C_BORDER_TYPE.C_BORDER_TYPE_CLOUD)
                {
                    if (_borderEffector.BorderIntensity == C_BORDER_INTENSITY.C_INTENSITY_TWO)
                    {
                        rdoCloud1.IsChecked = true;
                    }
                    else if (_borderEffector.BorderIntensity == C_BORDER_INTENSITY.C_INTENSITY_ONE)
                    {
                        rdoCloud2.IsChecked = true;
                    }
                    else
                    {
                        rdoStreat.IsChecked = true;
                    }
                }
                else
                {
                    rdoStreat.IsChecked = true;
                }

                // 恢复事件处理程序
                rdoStreat.Checked += rdoShape_Checked;
                rdoCloud1.Checked += rdoShape_Checked;
                rdoCloud2.Checked += rdoShape_Checked;
            }
        }

        public CPDFCloudStyleControl()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void rdoShape_Checked(object sender, RoutedEventArgs e)
        {
            switch ((sender as RadioButton).Tag)
            {
                case streat:
                    BorderEffector = new CPDFBorderEffector(C_BORDER_TYPE.C_BORDER_TYPE_STRAIGHT, C_BORDER_INTENSITY.C_INTENSITY_ZERO);
                    break;
                case cloud1:
                    BorderEffector = new CPDFBorderEffector(C_BORDER_TYPE.C_BORDER_TYPE_CLOUD, C_BORDER_INTENSITY.C_INTENSITY_TWO);
                    break;
                case cloud2:
                    BorderEffector = new CPDFBorderEffector(C_BORDER_TYPE.C_BORDER_TYPE_CLOUD, C_BORDER_INTENSITY.C_INTENSITY_ONE);
                    break;
            }
            LineShapeChanged?.Invoke(this, BorderEffector);
        }
    }
}
