using Compdfkit_Tools.Data;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using DashStyle = System.Windows.Media.DashStyle;

namespace Compdfkit_Tools.Common
{
    public partial class CPDFLineStyleControl : UserControl
    {
        public event EventHandler LineStyleChanged;

        private DashStyle _dashStyle;
        public DashStyle DashStyle
        {
            get
            {
                _dashStyle = CalculateDashStyle(CPDFLineStyleUI.DashStyle);
                return _dashStyle;
            }
            set
            {
                _dashStyle = value;
                if(_dashStyle.Dashes.ToArray().Length == 0)
                {
                    CPDFLineStyleUI.SolidRadioButton.IsChecked = true;
                    CPDFLineStyleUI.IsSolid = true;
                    CPDFLineStyleUI.DashSpacing = 1;
                }
                else
                {
                    CPDFLineStyleUI.DashRadioButton.IsChecked = true;
                    CPDFLineStyleUI.IsSolid = false;
                    CPDFLineStyleUI.DashSpacing = (int)(_dashStyle.Dashes.ToArray().First());
                }
            }
        }

        public CPDFLineStyleControl()
        {
            InitializeComponent();
            CPDFLineStyleUI.LineStyleChanged += CPDFLineStyleUI_LineStyleChanged;
        }

        public DashStyle CalculateDashStyle(CPDFDashData pdfDash)
        {
            DashStyle dashStyle = new DashStyle();
            if (pdfDash.IsSolid)
            {
                dashStyle = DashStyles.Solid;
                return dashStyle;
            }
            else
            {
                dashStyle.Dashes.Add(pdfDash.DashSpacing);
                dashStyle.Dashes.Add(pdfDash.DashSpacing);
                return dashStyle;
            }
        }

        private void CPDFLineStyleUI_LineStyleChanged(object sender, EventArgs e)
        {
            LineStyleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
