using ComPDFKit.PDFAnnotation;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class FreeTextParam:AnnotParam
    {
        public  FreeTextParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT;
        }

        public byte[] LineColor { get; set; }
        public byte[] BgColor { get; set; }
        public bool HasBgColor {  get; set; }
        public byte[] FontColor { get; set; }
        public double LineWidth {  get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public string FontName { get; set; } = string.Empty;
        public double FontSize { get; set; }
        public C_TEXT_ALIGNMENT Alignment {  get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            
            FreeTextParam freetextTransfer = transfer as FreeTextParam;
            if (freetextTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(freetextTransfer))
            {
                return false;
            }

            if(LineColor != null)
            {
                freetextTransfer.LineColor = (byte[])LineColor.Clone();
            }
           
            if(BgColor != null)
            {
                freetextTransfer.BgColor = (byte[])BgColor.Clone();
            }
          
            if(FontColor != null)
            {
                freetextTransfer.FontColor = (byte[])FontColor.Clone();
            }

            freetextTransfer.HasBgColor = HasBgColor;
            freetextTransfer.LineWidth = LineWidth;
            freetextTransfer.IsBold = IsBold;
            freetextTransfer.IsItalic = IsItalic;
            freetextTransfer.FontName = FontName;
            freetextTransfer.FontSize = FontSize;
            freetextTransfer.Alignment = Alignment;

            return true;
        }
    }
}
