using ComPDFKit.PDFAnnotation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComPDFKit.Tool.SettingParam
{
    public class SoundParam : AnnotParam
    {
        public  SoundParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_SOUND;
        }


        public string SoundFilePath { get; set; } = string.Empty;
        public MemoryStream ImageStream { get; set; }

        public override bool CopyTo(AnnotParam transfer)
        {
            SoundParam soundTransfer = transfer as SoundParam;
            if (soundTransfer == null)
            {
                return false;
            }

            if (!base.CopyTo(soundTransfer))
            {
                return false;
            }

            soundTransfer.SoundFilePath = SoundFilePath;
            soundTransfer.ImageStream = ImageStream;
            return true;
        }
    }
}
