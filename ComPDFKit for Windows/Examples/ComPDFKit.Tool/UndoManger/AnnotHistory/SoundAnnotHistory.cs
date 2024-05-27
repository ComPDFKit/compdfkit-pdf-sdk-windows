using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.SettingParam;
using ComPDFKitViewer.Annot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ComPDFKit.Tool.UndoManger
{
    public class SoundAnnotHistory : AnnotHistory
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
            SoundParam currentParam = CurrentParam as SoundParam;

            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }

            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            CPDFSoundAnnotation soundAnnot = pdfPage?.CreateAnnot(C_ANNOTATION_TYPE.C_ANNOTATION_SOUND) as CPDFSoundAnnotation;
            if (soundAnnot != null)
            {
                int annotIndex = pdfPage.GetAnnotCount() - 1;
                soundAnnot.SetTransparency((byte)currentParam.Transparency);
                soundAnnot.SetRect(currentParam.ClientRect);

                byte[] imageData = null;
                int imageWidth = 0;
                int imageHeight = 0;
                PDFHelp.ImageStreamToByte(currentParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);
                if (imageData != null && imageWidth > 0 && imageHeight > 0)
                {
                    if (!string.IsNullOrEmpty(currentParam.SoundFilePath))
                    {
                        soundAnnot.SetSoundPath(imageData, imageWidth, imageHeight, currentParam.SoundFilePath);
                    }
                }

                if (!string.IsNullOrEmpty(currentParam.Author))
                {
                    soundAnnot.SetAuthor(currentParam.Author);
                }

                if (!string.IsNullOrEmpty(currentParam.Content))
                {
                    soundAnnot.SetContent(currentParam.Content);
                }
                soundAnnot.SetIsLocked(currentParam.Locked);
                soundAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                soundAnnot.UpdateAp();
                soundAnnot.ReleaseAnnot();

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
            if (CurrentParam as SoundParam == null || PreviousParam as SoundParam == null)
            {
                return false;
            }

            if (MakeAnnotValid(CurrentParam))
            {
                CPDFSoundAnnotation circleAnnot = Annot as CPDFSoundAnnotation;
                if (circleAnnot == null || !circleAnnot.IsValid())
                {
                    return false;
                }

                SoundParam updateParam = (isUndo ? PreviousParam : CurrentParam) as SoundParam;
                SoundParam checkParam = (isUndo ? CurrentParam : PreviousParam) as SoundParam;

                if (updateParam.Transparency != checkParam.Transparency)
                {
                    circleAnnot.SetTransparency((byte)updateParam.Transparency);
                }

                if (!updateParam.ClientRect.Equals(checkParam.ClientRect))
                {
                    circleAnnot.SetRect(updateParam.ClientRect);
                }

                if (updateParam.Author != checkParam.Author)
                {
                    circleAnnot.SetAuthor(updateParam.Author);
                }

                if (updateParam.Content != checkParam.Content)
                {
                    circleAnnot.SetContent(updateParam.Content);
                }

                if (updateParam.Locked != checkParam.Locked)
                {
                    circleAnnot.SetIsLocked(updateParam.Locked);
                }

                circleAnnot.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                //circleAnnot.UpdateAp();

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
