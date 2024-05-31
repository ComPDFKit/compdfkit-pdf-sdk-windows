using ComPDFKit.PDFAnnotation;
using System.IO;

namespace ComPDFKit.Tool
{
	public class StampParam:AnnotParam
	{
		public  StampParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_STAMP;
        }


		public string StampText { get; set; } = string.Empty;
		public string DateText {  get; set; }= string.Empty;
		public C_STAMP_TYPE StampType { get; set; }
		public C_TEXTSTAMP_SHAPE TextStampShape {  get; set; }
		public C_TEXTSTAMP_COLOR TextStampColor {  get; set; }
		public MemoryStream ImageStream { get; set; }
		public int Rotation { get; set; }	


        public override bool CopyTo(AnnotParam transfer)
		{
			StampParam stampTransfer = transfer as StampParam;
			if (stampTransfer == null)
			{
				return false;
			}

			if (!base.CopyTo(stampTransfer))
			{
				return false;
			}

		    stampTransfer.StampText = StampText;
			stampTransfer.DateText = DateText;
			stampTransfer.StampType = StampType;
			stampTransfer.TextStampColor = TextStampColor;
			stampTransfer.TextStampShape = TextStampShape;
			stampTransfer.ImageStream = ImageStream;
			stampTransfer.Rotation = Rotation;

			return true;
		}
	}
}
