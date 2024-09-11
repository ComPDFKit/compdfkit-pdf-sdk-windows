using ComPDFKit.PDFDocument;
using ComPDFKit.Tool.UndoManger;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Common
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

        private string _postScriptName = string.Empty;
        public string PostScriptName
        {
            get
            {
                var psName = string.Empty;
                CPDFFont.GetPostScriptName(FontFamilyValue, FontStyleValue, ref psName);
                return psName;
            }
            set
            {
                var _fontFamilyName = string.Empty;
                var _fontStyleName = string.Empty;
                _postScriptName = value;
                CPDFFont.GetFamilyStyleName(_postScriptName, ref _fontFamilyName, ref _fontStyleName);
                FontFamilyValue = _fontFamilyName;
                FontStyleValue = _fontStyleName;
            }
        }

        public string FontFamilyValue
        {
            get => CPDFFontUI.FamilyName;
            set
            {
                CPDFFontUI.FamilyName = value;
            }
        }

        public string FontStyleValue
        {
            get => CPDFFontUI.StyleName;
            set => CPDFFontUI.StyleName = value;
            
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
            CPDFFontUI.FontFamilyChanged -= CPDFFontUI_FontFamilyChanged;
            CPDFFontUI.FontStyleChanged -= CPDFFontUI_FontStyleChanged;
            CPDFFontUI.FontSizeChanged -= CPDFFontUI_FontSizeChanged;
            CPDFFontUI.FontAlignChanged -= CPDFFontUI_FontAlignChanged;

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
