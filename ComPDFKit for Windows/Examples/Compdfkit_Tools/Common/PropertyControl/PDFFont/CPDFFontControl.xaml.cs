using System;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.Common
{
    public partial class CPDFFontControl : UserControl
    {
        public bool IsReset = false;

        private int _fontSizeValue = 20;
        public int FontSizeValue
        {
            get => CPDFFontUI.FontSizeValue;
            set => CPDFFontUI.FontSizeValue = value;
        }

        public string FontFamilyValue
        {
            get => CPDFFontUI.FontFamilyValue;
            set => CPDFFontUI.FontFamilyValue = value;
        }

        public bool IsBold
        {
            get => CPDFFontUI.IsBold;
            set => CPDFFontUI.IsBold = value;
        }

        public bool IsItalic
        {
            get => CPDFFontUI.IsItalic;
            set => CPDFFontUI.IsItalic = value;
        }

        public TextAlignment TextAlignment
        {
            get => CPDFFontUI.TextAlignment;
            set => CPDFFontUI.TextAlignment = value;
        }

        public event EventHandler FontFamilyChanged;
        public event EventHandler FontStyleChanged;
        public event EventHandler FontSizeChanged;
        public event EventHandler FontAlignChanged;

        public CPDFFontControl()
        {
            InitializeComponent();
            CPDFFontUI.FontFamilyChanged += CPDFFontUI_FontFamilyChanged;
            CPDFFontUI.FontStyleChanged += CPDFFontUI_FontStyleChanged;
            CPDFFontUI.FontSizeChanged += CPDFFontUI_FontSizeChanged;
            CPDFFontUI.FontAlignChanged += CPDFFontUI_FontAlignChanged;
        }

        private void CPDFFontUI_FontAlignChanged(object sender, EventArgs e)
        {
            FontAlignChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CPDFFontUI_FontSizeChanged(object sender, EventArgs e)
        {
            FontSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CPDFFontUI_FontStyleChanged(object sender, EventArgs e)
        {
            FontStyleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CPDFFontUI_FontFamilyChanged(object sender, EventArgs e)
        {
            FontFamilyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
