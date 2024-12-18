using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using System;
using System.IO;
using System.Windows;
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
            if (pdfPage == null)
                return false;

            CPDFStampAnnotation stampAnnot;
            if (currentParam.StampType == C_STAMP_TYPE.IMAGE_STAMP && currentParam.CopyImageAnnot != null)
            {
                stampAnnot = CPDFAnnotation.AddCopyAnnotToPage(PDFDoc, pdfPage, currentParam.CopyImageAnnot) as CPDFStampAnnotation;
            }
            else
            {
                stampAnnot = pdfPage.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP) as CPDFStampAnnotation;
            }

            if (stampAnnot != null)
			{
				int annotIndex = pdfPage.GetAnnotCount() - 1;
                switch (currentParam.StampType)
				{
					case C_STAMP_TYPE.STANDARD_STAMP:
						{
							string stampText = currentParam.StampText;
							if (stampText == null)
							{
								stampText = string.Empty;
							}

							stampAnnot.SetStandardStamp(stampText, pdfPage.Rotation);
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
                        }
						break;

					case C_STAMP_TYPE.IMAGE_STAMP:
                        {
                            if(currentParam.CopyImageAnnot == null)
                            {
                                byte[] imageData = null;
                                int imageWidth = 0;
                                int imageHeight = 0;
                                PDFHelp.ImageStreamToByte(currentParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);
                                if (imageData != null && imageWidth > 0 && imageHeight > 0)
                                {
                                    stampAnnot.SetSourceRect(currentParam.SourceRect);
                                    stampAnnot.AnnotationRotator.SetRotation(currentParam.Rotation);
                                    stampAnnot.SetImageStamp(
                                        imageData,
                                        imageWidth,
                                        imageHeight,
                                        pdfPage.Rotation);
                                }
                            }
                        }
                        break;

					default:
						break;
				}

                if (currentParam.StampType != C_STAMP_TYPE.IMAGE_STAMP || currentParam.CopyImageAnnot != null)
                {
                    stampAnnot.SetSourceRect(currentParam.SourceRect);
                    stampAnnot.AnnotationRotator.SetRotation(currentParam.Rotation);
                }

                stampAnnot.SetTransparency(currentParam.Transparency);
                string imagePath = string.Empty;
                if (currentParam.Transparency <255 && GetAnnotOpacityImage(stampAnnot, currentParam.Transparency / 255.0, 5, out imagePath) && File.Exists(imagePath))
                {
                    stampAnnot.SetImageStamp(imagePath, string.Empty);
                    File.Delete(imagePath);
                }

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
					stampAnnot.SetTransparency(updateParam.Transparency);
                    string imagePath = string.Empty;
                    if (GetAnnotOpacityImage(stampAnnot, updateParam.Transparency/255.0, 5, out imagePath) && File.Exists(imagePath))
                    {
                        stampAnnot.SetImageStamp(imagePath, string.Empty);
                        File.Delete(imagePath);
                    }
                }

				if (!updateParam.SourceRect.Equals(checkParam.SourceRect))
                {
                    stampAnnot.SetSourceRect(updateParam.SourceRect);
                    stampAnnot.AnnotationRotator.SetRotation(updateParam.Rotation);
                }

                if (updateParam.Rotation != checkParam.Rotation)
                {
                    stampAnnot.AnnotationRotator.SetRotation(updateParam.Rotation);
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

        private bool GetAnnotOpacityImage(CPDFStampAnnotation annot, double opacity, double zoom, out string imagePath)
        {
            imagePath = string.Empty;
            if (annot == null || annot.IsValid() == false)
            {
                return false;
            }

            try
            {
                CRect rawRect = annot.GetRect();
                Rect drawRect = new Rect(0, 0, (int)(rawRect.width() * 96.0 / 72.0 * zoom), (int)(rawRect.height() * 96.0 / 72.0 * zoom));

                byte[] imageData = new byte[(int)drawRect.Width * (int)drawRect.Height * 4];
                annot.RenderAnnot((int)drawRect.Width, (int)drawRect.Height, imageData);

                int stride = ((int)drawRect.Width) * 4;
                for (int i = 0; i < (int)(drawRect.Height); i++)
                {
                    for (int j = 0; j < (int)(drawRect.Width); j++)
                    {
                        byte b = imageData[i * stride + j * 4];
                        byte g = imageData[i * stride + j * 4 + 1];
                        byte r = imageData[i * stride + j * 4 + 2];
                        byte a = imageData[i * stride + j * 4 + 3];

                        if (a == 0 && b == 255 && g == 255 && r == 255)
                        {
                            continue;
                        }

                        if (a == 0 && b == 0 && g == 0 && r == 0)
                        {
                            continue;
                        }

                        imageData[i * stride + j * 4 + 3] = (byte)(opacity * 255);
                    }
                }

                WriteableBitmap writeImage = new WriteableBitmap((int)drawRect.Width, (int)drawRect.Height, 96, 96, PixelFormats.Bgra32, null);
                writeImage.WritePixels(new Int32Rect(0, 0, (int)drawRect.Width, (int)drawRect.Height), imageData, stride, 0);
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                using (FileStream fs = File.Create(tempPath))
                {
                    pngEncoder.Frames.Add(BitmapFrame.Create(writeImage));
                    pngEncoder.Save(fs);
                }
                imagePath = tempPath;
                return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }
    }
}
