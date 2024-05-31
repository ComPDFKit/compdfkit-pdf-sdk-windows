using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
	public class WidgetParm:AnnotParam
	{
		public C_WIDGET_TYPE WidgetType {  get; set; }
		public C_BORDER_STYLE BorderStyle { get; set; }
		public byte[] LineColor { get; set; }
		public bool HasLineColor {  get; set; }
		public byte[] BgColor { get; set; }
		public bool HasBgColor {  get; set; }
		public byte[] FontColor { get; set; }
        public string FontName { get; set; } = string.Empty; 
        public double FontSize { get; set; }
        public double LineWidth { get; set; }
		public string FieldName { get; set; } = string.Empty;

        /// <summary> 
		/// Note the use of ParamConverter.GetFormFlags for obtaining the result
        /// </summary>
        public int Flags {  get; set; }
		public bool IsReadOnly {  get; set; }
		public bool IsHidden {  get; set; }
		public override bool CopyTo(AnnotParam transfer)
		{
			WidgetParm formTransfer = transfer as WidgetParm;
			if (formTransfer == null)
			{
				return false;
			}
			if (!base.CopyTo(formTransfer))
			{
				return false;
			}

			formTransfer.WidgetType = WidgetType;
			formTransfer.BorderStyle = BorderStyle;
			if(LineColor!=null)
			{
				formTransfer.LineColor = (byte[])LineColor.Clone();
			}
			
			if(BgColor!=null)
			{
				formTransfer.BgColor = (byte[])BgColor.Clone();
			}
			
			if(FontColor!=null)
			{
				formTransfer.FontColor = (byte[])FontColor.Clone();
			}

			formTransfer.HasBgColor = HasBgColor;
			formTransfer.HasLineColor = HasLineColor;
			formTransfer.LineWidth = LineWidth;
			formTransfer.FieldName = FieldName;
			formTransfer.FontName = FontName;
			formTransfer.FontSize = FontSize;
			formTransfer.Flags = Flags;
			formTransfer.IsReadOnly = IsReadOnly;
			formTransfer.IsHidden = IsHidden;

			return true;
		}
	}
}
