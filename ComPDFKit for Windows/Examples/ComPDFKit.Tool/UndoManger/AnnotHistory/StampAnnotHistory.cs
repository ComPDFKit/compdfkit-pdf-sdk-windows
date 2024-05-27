using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComPDFKit.Tool.UndoManger
{
    public class StampAnnotHistory:AnnotHistory
	{
        public override int GetAnnotIndex()
        {
            if (CurrentParam != null)
            {
                return CurrentParam.AnnotIndex;
            }
            return base.GetAnnotIndex();
        }

        public override int GetPageIndex()
        {
            if (CurrentParam != null)
            {
                return CurrentParam.PageIndex;
            }
            return base.GetPageIndex();
        }

        public override void SetAnnotIndex(int newIndex)
        {
            if (CurrentParam != null)
            {
                CurrentParam.AnnotIndex = newIndex;
            }

            if (PreviousParam != null)
            {
                PreviousParam.AnnotIndex = newIndex;
            }
        }

        internal override bool Add()
		{
            StampParam currentParam = CurrentParam as StampParam;

            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
			{
				return false;
			}

			if(currentParam.StampType==C_STAMP_TYPE.UNKNOWN_STAMP)
			{
				return false;
			}

			if(currentParam.StampType==C_STAMP_TYPE.IMAGE_STAMP && currentParam.ImageStream==null)
			{
				return false;
			}

			CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
			CPDFStampAnnotation stampAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
           
            if (stampAnnot != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;
				switch(currentParam.StampType)
				{
					case C_STAMP_TYPE.STANDARD_STAMP:
						{
							string stampText = currentParam.StampText;
							if (stampText == null)
							{
								stampText = string.Empty;
							}
							stampAnnot.SetStandardStamp(stampText, pdfPage.Rotation);
                            stampAnnot.SetRect(currentParam.ClientRect);
                        }
						break;
					case C_STAMP_TYPE.TEXT_STAMP:
						{
							string dateText = currentParam.DateText;
							string stampText = currentParam.StampText;
							if (dateText == null)
							{
								dateText = string.Empty;
							}
							if (stampText == null)
							{
								stampText = string.Empty;
							}
							stampAnnot.SetTextStamp(
								stampText,
								dateText,
								currentParam.TextStampShape,
								currentParam.TextStampColor,
								pdfPage.Rotation);
                            stampAnnot.SetRect(currentParam.ClientRect);
                        }
						break;
					case C_STAMP_TYPE.IMAGE_STAMP:
						{
                            byte[] imageData = null;
                            int imageWidth = 0;
                            int imageHeight = 0;
                            PDFHelp.ImageStreamToByte(currentParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);
							if (imageData != null && imageWidth > 0 && imageHeight > 0)
							{
                                stampAnnot.SetRect(currentParam.ClientRect);
                                stampAnnot.SetImageStamp(
                                    imageData,
                                    imageWidth,
                                    imageHeight,
                                    pdfPage.Rotation);
                            }
						}
						break;

					default:
						break;
				}

				stampAnnot.SetTransparency((byte)currentParam.Transparency);
				

				if (!string.IsNullOrEmpty(currentParam.Author))
				{
					stampAnnot.SetAuthor(currentParam.Author);
				}

				if (!string.IsNullOrEmpty(currentParam.Content))
				{
					stampAnnot.SetContent(currentParam.Content);
				}
				stampAnnot.SetIsLocked(currentParam.Locked);
				stampAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
				stampAnnot.UpdateAp();
				stampAnnot.ReleaseAnnot();

				if (currentParam != null)
                {
                    currentParam.AnnotIndex = annotIndex;
				}
				if (PreviousParam != null)
				{
					PreviousParam.AnnotIndex = annotIndex;
				}

				return true;
			}

			return false;
		}

		internal override bool Update(bool isUndo)
		{
			if (CurrentParam as StampParam == null || PreviousParam as StampParam == null)
			{
				return false;
			}

			if (MakeAnnotValid(CurrentParam))
			{
				CPDFStampAnnotation stampAnnot = Annot as CPDFStampAnnotation;
				if (stampAnnot == null || !stampAnnot.IsValid())
				{
					return false;
				}

				StampParam updateParam = (isUndo ? PreviousParam : CurrentParam) as StampParam;
				StampParam checkParam = (isUndo ? CurrentParam : PreviousParam) as StampParam;

				if (updateParam.Transparency != checkParam.Transparency)
				{
					stampAnnot.SetTransparency((byte)updateParam.Transparency);
				}

				if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    stampAnnot.SetRect(updateParam.ClientRect);
                }

				if (updateParam.Author != checkParam.Author)
				{
					stampAnnot.SetAuthor(updateParam.Author);
				}

				if (updateParam.Content != checkParam.Content)
				{
					stampAnnot.SetContent(updateParam.Content);
				}

				if (updateParam.Locked != checkParam.Locked)
				{
					stampAnnot.SetIsLocked(updateParam.Locked);
				}

				stampAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
				stampAnnot.UpdateAp();

				return true;
			}
			return false;
		}

		internal override bool Remove()
		{
			if (MakeAnnotValid(CurrentParam))
			{
				Annot.RemoveAnnot();
				return true;
			}
			return false;
		}
	}
}
