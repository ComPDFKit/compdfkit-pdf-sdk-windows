using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;

namespace ComPDFKit.Tool
{
    public class TextEditParam:PDFEditParam
    {
        public TextEditParam()
        {
            EditType = CPDFEditType.EditText;
        }
        public double FontSize { get; set; }
        public byte[] FontColor { get; set; }
        public TextAlignType TextAlign { get; set; }
        public string FontName { get; set; } = string.Empty;
        public bool IsItalic { get; set; }
        public bool IsBold { get; set; }

        public override bool CopyTo(PDFEditParam transfer)
        {
            TextEditParam texteditTransfer = transfer as TextEditParam;
            if (texteditTransfer == null || !base.CopyTo(texteditTransfer))
                return false;

            texteditTransfer.FontSize = FontSize;
            if (FontColor != null)
                texteditTransfer.FontColor = (byte[])FontColor.Clone();

            texteditTransfer.TextAlign = TextAlign;
            texteditTransfer.FontName = FontName;
            texteditTransfer.IsItalic = IsItalic;
            texteditTransfer.IsBold = IsBold;
            return true;
        }
    }
}
