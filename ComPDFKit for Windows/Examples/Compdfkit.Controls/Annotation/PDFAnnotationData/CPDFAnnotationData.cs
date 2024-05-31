using ComPDFKit.PDFAnnotation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComPDFKit.Controls.Data
{
    public enum CPDFAnnotationType
    {
        Unknown = 0,
        Highlight,
        Underline,
        Strikeout,
        Squiggly,
        FreeText,
        Freehand,
        Note,
        Circle,
        Square,
        Arrow,
        Line,
        Stamp,
        Signature,
        Link,
        Audio,
        Image
    }

    public enum SignatureType
    {
        TextType,
        Drawing,
        ImageType
    }

    public class LineType : INotifyPropertyChanged
    {
        private C_LINE_TYPE _headLineType;
        public C_LINE_TYPE HeadLineType
        {
            get { return _headLineType; }
            set { _headLineType = value; OnPropertyChanged(nameof(HeadLineType)); }
        }

        private C_LINE_TYPE _tailLineType;
        public C_LINE_TYPE TailLineType
        {
            get { return _tailLineType; }
            set { _tailLineType = value; OnPropertyChanged(nameof(TailLineType)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class CPDFAnnotationDictionary
    {
        public static Dictionary<string, CPDFAnnotationType> GetAnnotationFromTag = new Dictionary<string, CPDFAnnotationType>() {
            { "Highlight", CPDFAnnotationType.Highlight },
            { "Underline", CPDFAnnotationType.Underline },
            { "Strikeout", CPDFAnnotationType.Strikeout },
            { "Squiggly", CPDFAnnotationType.Squiggly },
            { "Square", CPDFAnnotationType.Square },
            { "Circle", CPDFAnnotationType.Circle },
            { "Line", CPDFAnnotationType.Line },
            { "Arrow", CPDFAnnotationType.Arrow },
            { "Freehand", CPDFAnnotationType.Freehand },
            { "FreeText", CPDFAnnotationType.FreeText },
            { "Note", CPDFAnnotationType.Note },
            { "Stamp", CPDFAnnotationType.Stamp },
            { "Signature", CPDFAnnotationType.Signature },
            { "Link", CPDFAnnotationType.Link },
            {"Audio", CPDFAnnotationType.Audio },
            {"Image", CPDFAnnotationType.Image }
        };

        public static Dictionary<int, C_LINE_TYPE> GetLineTypeFromIndex = new Dictionary<int, C_LINE_TYPE>()
        {
            { 0, C_LINE_TYPE.LINETYPE_NONE },
            { 1, C_LINE_TYPE.LINETYPE_ARROW },
            { 2, C_LINE_TYPE.LINETYPE_CLOSEDARROW },
            { 3, C_LINE_TYPE.LINETYPE_SQUARE },
            { 4, C_LINE_TYPE.LINETYPE_CIRCLE },
            { 5, C_LINE_TYPE.LINETYPE_DIAMOND },
            { 6, C_LINE_TYPE.LINETYPE_BUTT },
            { 7, C_LINE_TYPE.LINETYPE_ROPENARROW },
            { 8, C_LINE_TYPE.LINETYPE_RCLOSEDARROW },
            { 9, C_LINE_TYPE.LINETYPE_SLASH }
        };
    }

    /// <summary>
    /// 用于换算的dash
    /// </summary>
    public class CPDFDashData
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string PropertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        private bool _isSolid = true;
        public bool IsSolid
        {
            get => _isSolid;
            set
            {
                _isSolid = value;
                RaisePropertyChanged("IsSolid");
            }
        }

        private int _dashSpacing = 1;
        public int DashSpacing {
            get => _dashSpacing;
            set
            {
                _dashSpacing = value;
                RaisePropertyChanged("IsSolid");
            }
        }
    }

    public class CPDFFontData
    {
        public string FontFamily = "Helvetica";
        public int FontSize = 20;
        public bool IsBold = false;
        public bool IsItalic = false;
        public TextAlignment TextAlignment = TextAlignment.Left;
    }

    public abstract class CPDFAnnotationData
    {
        public CPDFAnnotationType AnnotationType;
        public string Note = string.Empty;
        public static string Author;
        public bool IsLocked = false;
    }

    public class CPDFMarkupData : CPDFAnnotationData
    {
        public double Opacity = 1;
        public Color Color = Color.FromRgb(255, 0, 0);
    }

    public class CPDFShapeData : CPDFAnnotationData
    {
        public Color BorderColor = Color.FromRgb(255, 0, 0);
        public Color FillColor = Color.FromRgb(255, 255, 255);
        public double Opacity = 1;
        public int Thickness = 1;
        public DashStyle DashStyle = DashStyles.Solid;
    }

    public class CPDFLineShapeData : CPDFAnnotationData
    {
        public Color BorderColor = Color.FromRgb(255, 0, 0);
        public double Opacity = 1;
        public int Thickness = 1;
        public DashStyle DashStyle = DashStyles.Solid;
        public LineType LineType = new LineType() { HeadLineType = C_LINE_TYPE.LINETYPE_NONE, TailLineType = C_LINE_TYPE.LINETYPE_NONE };
    }

    public class CPDFFreeTextData : CPDFAnnotationData
    {
        public Color BorderColor = Color.FromRgb(255, 0, 0);
        public double Opacity = 1;
        public string FontFamily = "Helvetica";
        public int FontSize = 20;
        public bool IsBold = false;
        public bool IsItalic = false;
        public TextAlignment TextAlignment = TextAlignment.Left;
    }

    public class CPDFNoteData : CPDFAnnotationData
    {
        public Color BorderColor = Color.FromRgb(255, 0, 0);
    }


    public class CPDFFreehandData : CPDFAnnotationData
    {
        public Color BorderColor = Color.FromRgb(255, 0, 0);
        public double Opacity = 1;
        public double Thickness = 1;
    }

    public class CPDFStampData : CPDFAnnotationData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string PropertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        private string stampText;

        public string StampText
        {
            get { return stampText; }
            set
            {
                stampText = value;
                RaisePropertyChanged("StampText");
            }
        }

        private string sourcePath;

        public string SourcePath
        {
            get { return sourcePath; }
            set
            {
                sourcePath = value;
                RaisePropertyChanged("SourcePath");
            }
        }


        private int maxWidth;

        public int MaxWidth
        {
            get { return maxWidth; }
            set
            {
                maxWidth = value;
                RaisePropertyChanged("MaxWidth");
            }
        }

        private int maxHeight;

        public int MaxHeight
        {
            get { return maxHeight; }
            set
            {
                maxHeight = value;
                RaisePropertyChanged("MaxHeight");
            }
        }


        private C_STAMP_TYPE type = C_STAMP_TYPE.UNKNOWN_STAMP;

        public C_STAMP_TYPE Type
        {
            get { return type; }
            set
            {
                type = value;
                RaisePropertyChanged("Type");
            }
        }

        public string TypeText
        {
            get
            {
                if(Type == C_STAMP_TYPE.TEXT_STAMP)
                {
                    return "Text Stamp";
                }

                if (Type == C_STAMP_TYPE.IMAGE_STAMP)
                {
                    return "Image Stamp";
                }

                return type.ToString();
            }
        }

        private double opacity;

        public double Opacity
        {
            get { return opacity; }
            set
            {
                opacity = value;
                RaisePropertyChanged("Opacity");
            }
        }
        private BitmapSource imageSource;

        public BitmapSource ImageSource
        {
            get { return imageSource; }
            set
            {
                imageSource = value;
                RaisePropertyChanged("ImageSource");
            }
        }

        public C_TEXTSTAMP_COLOR TextColor = C_TEXTSTAMP_COLOR.TEXTSTAMP_WHITE;
        public string StampTextDate = "";
        public C_TEXTSTAMP_SHAPE TextSharp = C_TEXTSTAMP_SHAPE.TEXTSTAMP_NONE;
        public bool IsCheckedDate = false;
        public bool IsCheckedTime = false;
    }

    public class CustomStampList : List<CPDFStampData>
    {

    }

    public class CPDFSignatureData : CPDFAnnotationData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string PropertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
        private string sourcePath;

        public string SourcePath
        {
            get { return sourcePath; }
            set
            {
                sourcePath = value;
                RaisePropertyChanged("SourcePath");
            }
        }
        private string drawingPath;

        public string DrawingPath
        {
            get { return drawingPath; }
            set
            {
                drawingPath = value;
                RaisePropertyChanged("DrawingPath");
            }
        }

        public SignatureType Type { get; set; }
        public double inkThickness { get; set; }
        public Color inkColor { get; set; }
    }

    public class SignatureList : List<CPDFSignatureData>
    {

    }
}
