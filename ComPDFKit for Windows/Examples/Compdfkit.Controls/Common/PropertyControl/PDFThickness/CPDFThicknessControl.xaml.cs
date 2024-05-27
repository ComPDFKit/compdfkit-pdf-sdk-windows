using System;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Common
{
    public partial class CPDFThicknessControl : UserControl
    {


        public event EventHandler ThicknessChanged;

        public int Thickness
        {
            get => CPDFThicknessUI.Thickness;
            set => CPDFThicknessUI.Thickness = value;
        }

        public CPDFThicknessControl()
        {
            InitializeComponent();
            CPDFThicknessUI.ThicknessChanged -= CPDFThicknessUI_ThicknessChanged;
            CPDFThicknessUI.ThicknessChanged += CPDFThicknessUI_ThicknessChanged;
        }

        private void CPDFThicknessUI_ThicknessChanged(object sender, EventArgs e)
        {
            ThicknessChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
