using System;
using System.Windows.Controls;

namespace Compdfkit_Tools.Common
{
    public partial class CPDFOpacityControl : UserControl
    {
        public event EventHandler OpacityChanged;

        public int OpacityValue
        {
            get=> CPDFOpacityUI.OpacityValue;
            set => CPDFOpacityUI.OpacityValue = value;
        }

        public CPDFOpacityControl()
        {
            InitializeComponent();
            CPDFOpacityUI.OpacityChanged += CPDFOpacityUI_OpacityChanged;
        }

        private void CPDFOpacityUI_OpacityChanged(object sender, EventArgs e)
        {
            OpacityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
