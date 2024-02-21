using Compdfkit_Tools.Data;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.Common
{
    public partial class CPDFArrowControl : UserControl
    {
        public event EventHandler ArrowChanged;

        private LineType _lineType;
        public LineType LineType
        {
            get
            {
                return new LineType()
                {
                    HeadLineType = CPDFAnnotationDictionary.GetLineTypeFromIndex[CPDFHeadArrowUI.SelectedIndex],
                    TailLineType = CPDFAnnotationDictionary.GetLineTypeFromIndex[CPDFTailArrowUI.SelectedIndex]
                };
            }
            set
            {
                _lineType = value;
                CPDFHeadArrowUI.SelectedIndex = (int)_lineType.HeadLineType;
                CPDFTailArrowUI.SelectedIndex = (int)_lineType.TailLineType;
            }
        }

        public CPDFArrowControl()
        {
            InitializeComponent();
            CPDFHeadArrowUI.ArrowChanged += CPDFHeadArrowUI_ArrowChanged;
            CPDFTailArrowUI.ArrowChanged += CPDFTailArrowUI_ArrowChanged;
            Loaded += CPDFArrowControl_Loaded;
        }

        private void CPDFArrowControl_Loaded(object sender, RoutedEventArgs e)
        {
            CPDFTailArrowUI.RotateContent(180);
        }

        private void CPDFTailArrowUI_ArrowChanged(object sender, EventArgs e)
        {
            ArrowChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CPDFHeadArrowUI_ArrowChanged(object sender, EventArgs e)
        {
            ArrowChanged?.Invoke((object)this, EventArgs.Empty);
        }
    }
}
