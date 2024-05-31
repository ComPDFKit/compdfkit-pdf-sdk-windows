using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ComPDFKit.Tool.SettingParam
{
    public class DefaultSettingParam
    {
        public bool IsShowLink = false;

        public bool IsShowWidget = false;

        #region Measure

        public bool IsOpenMeasure = false;

        public bool IsCreateSquarePolygonMeasure = false;

        private LineMeasureParam lineMeasureParamDef;
        public LineMeasureParam LineMeasureParamDef
        {
            get
            {
                LineMeasureParam Param = new LineMeasureParam();
                if (lineMeasureParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(lineMeasureParamDef);
            }
        }

        private PolyLineMeasureParam polyLineMeasureParamDef;
        public PolyLineMeasureParam PolyLineMeasureParamDef
        {
            get
            {
                PolyLineMeasureParam Param = new PolyLineMeasureParam();
                if (polyLineMeasureParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(polyLineMeasureParamDef);
            }
        }

        private PolygonMeasureParam polygonMeasureParamDef;
        public PolygonMeasureParam PolygonMeasureParamDef
        {
            get
            {
                PolygonMeasureParam Param = new PolygonMeasureParam();
                if (polygonMeasureParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(polygonMeasureParamDef);
            }
        }

        private void InitLineMeasure()
        {
            lineMeasureParamDef = new LineMeasureParam();
            lineMeasureParamDef.measureInfo = new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH,
            };
        }

        private void InitPolyLineMeasure()
        {
            polyLineMeasureParamDef = new PolyLineMeasureParam();
            polyLineMeasureParamDef.measureInfo = new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH,
            };
        }

        private void InitPolygonMeasure()
        {
            polygonMeasureParamDef = new PolygonMeasureParam();
            polygonMeasureParamDef.measureInfo = new CPDFMeasureInfo
            {
                Unit = CPDFMeasure.CPDF_CM,
                Precision = CPDFMeasure.PRECISION_VALUE_TWO,
                RulerBase = 1,
                RulerBaseUnit = CPDFMeasure.CPDF_CM,
                RulerTranslate = 1,
                RulerTranslateUnit = CPDFMeasure.CPDF_CM,
                CaptionType = CPDFCaptionType.CPDF_CAPTION_LENGTH | CPDFCaptionType.CPDF_CAPTION_AREA,
            };
        }

        #endregion

        #region Annot

        private CircleParam circleParamDef;
        public CircleParam CircleParamDef
        {
            get
            {
                CircleParam Param = new CircleParam();
                if (circleParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(circleParamDef);
            }
        }


        private FreeTextParam freeTextParamDef;
        public FreeTextParam FreeTextParamDef
        {
            get
            {
                FreeTextParam Param = new FreeTextParam();
                if (freeTextParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(freeTextParamDef);
            }
        }


        private HighlightParam highlightParamDef;
        public HighlightParam HighlightParamDef
        {
            get
            {
                HighlightParam Param = new HighlightParam();
                if (highlightParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(highlightParamDef);
            }
        }


        private InkParam inkParamDef;
        public InkParam InkParamDef
        {
            get
            {
                InkParam Param = new InkParam();
                if (inkParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(inkParamDef);
            }
        }


        private LineParam lineParamDef;
        public LineParam LineParamDef
        {
            get
            {
                LineParam Param = new LineParam();
                if (lineParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(lineParamDef);
            }
        }


        private LinkParam linkParamDef;
        public LinkParam LinkParamDef
        {
            get
            {
                LinkParam Param = new LinkParam();
                if (linkParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(linkParamDef);
            }
        }


        private RedactParam redactParamDef;
        public RedactParam RedactParamDef
        {
            get
            {
                RedactParam Param = new RedactParam();
                if (redactParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(redactParamDef);
            }
        }


        private SquareParam squareParamDef;
        public SquareParam SquareParamDef
        {
            get
            {
                SquareParam Param = new SquareParam();
                if (squareParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(squareParamDef);
            }
        }


        private SquigglyParam squigglyParamDef;
        public SquigglyParam SquigglyParamDef
        {
            get
            {
                SquigglyParam Param = new SquigglyParam();
                if (squigglyParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(squigglyParamDef);
            }
        }


        private StampParam stampParamDef;
        public StampParam StampParamDef
        {
            get
            {
                StampParam Param = new StampParam();
                if (stampParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(stampParamDef);
            }
        }


        private StickyNoteParam stickyNoteParamDef;
        public StickyNoteParam StickyNoteParamDef
        {
            get
            {
                StickyNoteParam Param = new StickyNoteParam();
                if (stickyNoteParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(stickyNoteParamDef);
            }
        }


        private StrikeoutParam strikeoutParamDef;
        public StrikeoutParam StrikeoutParamDef
        {
            get
            {
                StrikeoutParam Param = new StrikeoutParam();
                if (strikeoutParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(strikeoutParamDef);
            }
        }


        private UnderlineParam underlineParamDef;
        public UnderlineParam UnderlineParamDef
        {
            get
            {
                UnderlineParam Param = new UnderlineParam();
                if (underlineParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(underlineParamDef);
            }
        }


        private SoundParam soundParamDef;
        public SoundParam SoundParamDef
        {
            get
            {
                SoundParam Param = new SoundParam();
                if (soundParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(soundParamDef);
            }
        }


        private void InitUnderline()
        {
            underlineParamDef = new UnderlineParam();
        }

        private void InitStrikeout()
        {
            strikeoutParamDef = new StrikeoutParam();
        }

        private void InitStamp()
        {
            stampParamDef = new StampParam();
        }

        private void InitSquiggly()
        {
            squigglyParamDef = new SquigglyParam();
        }

        private void InitSquare()
        {
            squareParamDef = new SquareParam();
        }

        private void InitRedact()
        {
            redactParamDef = new RedactParam();
        }

        private void InitLink()
        {
            linkParamDef = new LinkParam();
        }

        private void InitLine()
        {
            lineParamDef = new LineParam();
        }

        private void InitInk()
        {
            inkParamDef = new InkParam();
        }

        private void Inithighlight()
        {
            highlightParamDef = new HighlightParam();
        }

        private void InitFreeText()
        {
            freeTextParamDef = new FreeTextParam();
            freeTextParamDef.Alignment = C_TEXT_ALIGNMENT.ALIGNMENT_LEFT;
            freeTextParamDef.Transparency = 255;
            byte[] lineColor = { 255, 0, 0 };
            freeTextParamDef.LineColor = lineColor;
            freeTextParamDef.LineWidth = 1;
            freeTextParamDef.FontName = "Arial";

            byte[] fontColor = { 255, 0, 0 };
            freeTextParamDef.FontColor = fontColor;
            freeTextParamDef.FontSize = 14;

        }

        private void InitCircle()
        {
            circleParamDef = new CircleParam();
        }

        private void InitStickyNote()
        {
            stickyNoteParamDef = new StickyNoteParam();
            stickyNoteParamDef.Transparency = 255;
            stickyNoteParamDef.StickyNoteColor = new byte[3] { 255, 0, 0 };
        }

        private void InitSound()
        {
            soundParamDef = new SoundParam();
        }

        #endregion

        #region Widget

        private CheckBoxParam checkBoxParamDef;
        public CheckBoxParam CheckBoxParamDef
        {
            get
            {
                CheckBoxParam Param = new CheckBoxParam();
                if (checkBoxParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(checkBoxParamDef);
            }
        }


        private ComboBoxParam comboBoxParamDef;
        public ComboBoxParam ComboBoxParamDef
        {
            get
            {
                ComboBoxParam Param = new ComboBoxParam();
                if (comboBoxParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(comboBoxParamDef);
            }
        }


        private ListBoxParam listBoxParamDef;
        public ListBoxParam ListBoxParamDef
        {
            get
            {
                ListBoxParam Param = new ListBoxParam();
                if (listBoxParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(listBoxParamDef);
            }
        }


        private PushButtonParam pushButtonParamDef;
        public PushButtonParam PushButtonParamDef
        {
            get
            {
                PushButtonParam Param = new PushButtonParam();
                if (pushButtonParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(pushButtonParamDef);
            }
        }


        private RadioButtonParam radioButtonParamDef;
        public RadioButtonParam RadioButtonParamDef
        {
            get
            {
                RadioButtonParam Param = new RadioButtonParam();
                if (radioButtonParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(radioButtonParamDef);
            }
        }

        private SignatureParam signatureParamDef;
        public SignatureParam SignatureParamDef
        {
            get
            {
                SignatureParam Param = new SignatureParam();
                if (signatureParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(signatureParamDef);
            }
        }


        private TextBoxParam textBoxParamDef;
        public TextBoxParam TextBoxParamDef
        {
            get
            {
                TextBoxParam Param = new TextBoxParam();
                if (textBoxParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(textBoxParamDef);
            }
        }


        private void InitCheckBox()
        {
            checkBoxParamDef = new CheckBoxParam();
        }

        private void InitComboBox()
        {
            comboBoxParamDef = new ComboBoxParam();
        }

        private void InitListBox()
        {
            listBoxParamDef = new ListBoxParam();
        }

        private void InitPushButton()
        {
            pushButtonParamDef = new PushButtonParam();
        }
        private void InitRadioButton()
        {
            radioButtonParamDef = new RadioButtonParam();
        }

        private void InitSignature()
        {
            signatureParamDef = new SignatureParam();
        }

        private void InitTextBox()
        {
            textBoxParamDef = new TextBoxParam();
        }

        #endregion

        #region PDFEdit

        private ImageEditParam imageEditParamDef;
        public ImageEditParam ImageEditParamDef
        {
            get
            {
                ImageEditParam Param = new ImageEditParam();
                if (imageEditParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(imageEditParamDef);
            }
        }

        private TextEditParam textEditParamDef;
        public TextEditParam TextEditParamDef
        {
            get
            {
                TextEditParam Param = new TextEditParam();
                if (textEditParamDef.CopyTo(Param))
                {
                    return Param;
                }
                return null;
            }
            private set
            {
                value.CopyTo(textEditParamDef);
            }
        }

        private void InitTextEdit()
        {
            textEditParamDef = new TextEditParam();
        }
        private void InitImageEdit()
        {
            imageEditParamDef = new ImageEditParam();
        }
        #endregion

        public DefaultSettingParam()
        {
            #region Measure

            InitLineMeasure();
            InitPolyLineMeasure();
            InitPolygonMeasure();

            #endregion

            #region Annot

            InitUnderline();
            InitStrikeout();
            InitStamp();
            InitSquiggly();
            InitSquare();
            InitRedact();
            InitLink();
            InitLine();
            InitInk();
            Inithighlight();
            InitFreeText();
            InitCircle();
            InitStickyNote();
            InitSound();

            #endregion

            #region Widget

            InitCheckBox();
            InitComboBox();
            InitListBox();
            InitPushButton();
            InitRadioButton();
            InitSignature();
            InitTextBox();

            #endregion

            #region PDFEdit

            InitTextEdit();
            InitImageEdit();

            #endregion
        }

        public bool SetAnnotParam(AnnotParam annotParam)
        {
            bool IsOK = false;
            if (annotParam == null)
            {
                return IsOK;
            }
            switch (annotParam.CurrentType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_NONE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNKOWN:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                    if (annotParam is StickyNoteParam)
                    {
                        StickyNoteParamDef = annotParam as StickyNoteParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                    if (annotParam is LinkParam)
                    {
                        LinkParamDef = annotParam as LinkParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                    if (annotParam is FreeTextParam)
                    {
                        FreeTextParamDef = annotParam as FreeTextParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    if (annotParam is LineParam)
                    {
                        LineParamDef = annotParam as LineParam;
                        IsOK = true;
                    }
                    else if (annotParam is LineMeasureParam)
                    {
                        LineMeasureParamDef = annotParam as LineMeasureParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                    if (annotParam is SquareParam)
                    {
                        SquareParamDef = annotParam as SquareParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                    if (annotParam is CircleParam)
                    {
                        CircleParamDef = annotParam as CircleParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    if (annotParam is PolygonMeasureParam)
                    {
                        PolygonMeasureParamDef = annotParam as PolygonMeasureParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    if (annotParam is PolyLineMeasureParam)
                    {
                        PolyLineMeasureParamDef = annotParam as PolyLineMeasureParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                    if (annotParam is HighlightParam)
                    {
                        HighlightParamDef = annotParam as HighlightParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                    if (annotParam is UnderlineParam)
                    {
                        UnderlineParamDef = annotParam as UnderlineParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                    if (annotParam is SquigglyParam)
                    {
                        SquigglyParamDef = annotParam as SquigglyParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                    if (annotParam is StrikeoutParam)
                    {
                        StrikeoutParamDef = annotParam as StrikeoutParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                    if (annotParam is StampParam)
                    {
                        StampParamDef = annotParam as StampParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CARET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                    if (annotParam is InkParam)
                    {
                        InkParamDef = annotParam as InkParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POPUP:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FILEATTACHMENT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                    if (annotParam is SoundParam)
                    {
                        SoundParamDef = annotParam as SoundParam;
                        IsOK = true;
                    }
                    break;
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    if (annotParam is WidgetParm)
                    {
                        IsOK = SetWidgetParam(annotParam as WidgetParm);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SCREEN:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_PRINTERMARK:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_TRAPNET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_WATERMARK:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_3D:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                    if (annotParam is RedactParam)
                    {
                        RedactParamDef = annotParam as RedactParam;
                        IsOK = true;
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INTERCHANGE:
                    break;
                default:
                    break;
            }
            return IsOK;
        }

        private bool SetWidgetParam(WidgetParm formParm)
        {
            bool IsOK = false;
            if (formParm == null)
            {
                return IsOK;
            }
            switch (formParm.WidgetType)
            {
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_NONE:
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                    if (formParm is PushButtonParam)
                    {
                        PushButtonParamDef = formParm as PushButtonParam;
                        IsOK = true;
                    }
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_CHECKBOX:
                    if (formParm is CheckBoxParam)
                    {
                        CheckBoxParamDef = formParm as CheckBoxParam;
                        IsOK = true;
                    }
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                    if (formParm is RadioButtonParam)
                    {
                        RadioButtonParamDef = formParm as RadioButtonParam;
                        IsOK = true;
                    }
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                    if (formParm is TextBoxParam)
                    {
                        TextBoxParamDef = formParm as TextBoxParam;
                        IsOK = true;
                    }
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_COMBOBOX:
                    if (formParm is ComboBoxParam)
                    {
                        ComboBoxParamDef = formParm as ComboBoxParam;
                        IsOK = true;
                    }
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_LISTBOX:
                    if (formParm is ListBoxParam)
                    {
                        ListBoxParamDef = formParm as ListBoxParam;
                        IsOK = true;
                    }
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                    if (formParm is SignatureParam)
                    {
                        SignatureParamDef = formParm as SignatureParam;
                        IsOK = true;
                    }
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_UNKNOWN:
                    break;
                default:
                    break;
            }
            return IsOK;
        }

        public bool SetPDFEditParamm(PDFEditParam editParam)
        {
            switch (editParam.EditType)
            {
                case PDFPage.CPDFEditType.EditText:
                    if (editParam is TextEditParam)
                    {
                        TextEditParamDef = editParam as TextEditParam;
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }
    }

    public class DefaultDrawParam
    {
        #region ShowAnnot

        private SolidColorBrush showLinkBrush;

        public SolidColorBrush ShowLinkBrush
        {
            get { return showLinkBrush.Clone(); }
            set { showLinkBrush = value; }
        }

        private Pen showLinkePen;

        public Pen ShowLinkePen
        {
            get { return showLinkePen.Clone(); }
            set { showLinkePen = value; }
        }
        #endregion

        #region AnnotSelectedRect

        private SolidColorBrush annotMoveBrush;

        public SolidColorBrush AnnotMoveBrush
        {
            get { return annotMoveBrush.Clone(); }
            set { annotMoveBrush = value; }
        }

        private Pen annotMovePen;

        public Pen AnnotMovePen
        {
            get { return annotMovePen.Clone(); }
            set { annotMovePen = value; }
        }

        private SolidColorBrush annotRectFillBrush;

        public SolidColorBrush AnnotRectFillBrush
        {
            get { return annotRectFillBrush.Clone(); }
            set { annotRectFillBrush = value; }
        }

        private Pen annotRectLinePen;

        public Pen AnnotRectLinePen
        {
            get { return annotRectLinePen.Clone(); }
            set { annotRectLinePen = value; }
        }


        private Pen annotRectHoverPen;

        public Pen AnnotRectHoverPen
        {
            get { return annotRectHoverPen.Clone(); }
            set { annotRectHoverPen = value; }
        }

        private Pen annotPointPen;

        public Pen AnnotPointPen
        {
            get { return annotPointPen.Clone(); }
            set { annotPointPen = value; }
        }

        private SolidColorBrush annotPointBorderBrush;

        public SolidColorBrush AnnotPointBorderBrush
        {
            get { return annotPointBorderBrush.Clone(); }
            set { annotPointBorderBrush = value; }
        }

        #endregion

        #region PDFEdit

        private SolidColorBrush caretBrush;

        public SolidColorBrush CaretBrush
        {
            get { return caretBrush.Clone(); }
            set { caretBrush = value; }
        }

        private Pen caretPen;

        public Pen CaretPen
        {
            get { return caretPen.Clone(); }
            set { caretPen = value; }
        }

        private SolidColorBrush pDFEditMoveBrush;

        public SolidColorBrush PDFEditMoveBrush
        {
            get { return pDFEditMoveBrush.Clone(); }
            set { pDFEditMoveBrush = value; }
        }

        private Pen pDFEditMovePen;

        public Pen PDFEditMovePen
        {
            get { return pDFEditMovePen.Clone(); }
            set { pDFEditMovePen = value; }
        }

        #region Not selected state

        private SolidColorBrush pDFEditRectFillBrush;

        public SolidColorBrush PDFEditRectFillBrush
        {
            get { return pDFEditRectFillBrush.Clone(); }
            set { pDFEditRectFillBrush = value; }
        }

        private Pen pDFEditRectLinePen;

        public Pen PDFEditRectLinePen
        {
            get { return pDFEditRectLinePen.Clone(); }
            set { pDFEditRectLinePen = value; }
        }

        private Pen pDFEditPointPen;

        public Pen PDFEditPointPen
        {
            get { return pDFEditPointPen.Clone(); }
            set { pDFEditPointPen = value; }
        }

        private SolidColorBrush pDFEditPointBorderBrush;

        public SolidColorBrush PDFEditPointBorderBrush
        {
            get { return pDFEditPointBorderBrush.Clone(); }
            set { pDFEditPointBorderBrush = value; }
        }

        #endregion

        #region Selected state


        private SolidColorBrush sPDFEditRectFillBrush;

        public SolidColorBrush SPDFEditRectFillBrush
        {
            get { return sPDFEditRectFillBrush.Clone(); }
            set { sPDFEditRectFillBrush = value; }
        }

        private Pen sPDFEditRectLinePen;

        public Pen SPDFEditRectLinePen
        {
            get { return sPDFEditRectLinePen.Clone(); }
            set { sPDFEditRectLinePen = value; }
        }

        private Pen sPDFEditPointPen;

        public Pen SPDFEditPointPen
        {
            get { return sPDFEditPointPen.Clone(); }
            set { sPDFEditPointPen = value; }
        }

        private SolidColorBrush sPDFEditPointBorderBrush;

        public SolidColorBrush SPDFEditPointBorderBrush
        {
            get { return sPDFEditPointBorderBrush.Clone(); }
            set { sPDFEditPointBorderBrush = value; }
        }

        private SolidColorBrush sPDFEditCropBorderBrush;
        public SolidColorBrush SPDFEditCropBorderBrush
        {
            get { return sPDFEditCropBorderBrush.Clone(); }
            set { sPDFEditCropBorderBrush = value; }
        }

        #endregion

        #region Hover state
        private SolidColorBrush pDFEditRectFillHoverBrush;

        public SolidColorBrush PDFEditRectFillHoverBrush
        {
            get { return pDFEditRectFillHoverBrush.Clone(); }
            set { pDFEditRectFillHoverBrush = value; }
        }

        private Pen pDFEditRectLineHoverPen;

        public Pen PDFEditRectLineHoverPen
        {
            get { return pDFEditRectLineHoverPen.Clone(); }
            set { pDFEditRectLineHoverPen = value; }
        }

        private Pen pDFEditPointHoverPen;

        public Pen PDFEditPointHoverPen
        {
            get { return pDFEditPointHoverPen.Clone(); }
            set { pDFEditPointHoverPen = value; }
        }

        private SolidColorBrush pDFEditPointBorderHoverBrush;

        public SolidColorBrush PDFEditPointBorderHoverBrush
        {
            get { return pDFEditPointBorderHoverBrush.Clone(); }
            set { pDFEditPointBorderHoverBrush = value; }
        }

        #endregion

        #endregion

        #region AnnotEdit

        private SolidColorBrush editControlLineBrush;

        public SolidColorBrush EditControlLineBrush
        {
            get { return editControlLineBrush.Clone(); }
            set { editControlLineBrush = value; }
        }

        private Pen editControlLinePen;

        public Pen EditControlLinePen
        {
            get { return editControlLinePen.Clone(); }
            set { editControlLinePen = value; }
        }

        private Pen editLinePen;

        public Pen EditLinePen
        {
            get { return editLinePen.Clone(); }
            set { editLinePen = value; }
        }

        #endregion

        #region AnnotCreate


        private Pen redactPen;
        public Pen RedactPen
        {
            get { return redactPen.Clone(); }
            set { redactPen = value; }
        }

        private Pen linkPen;
        public Pen LinkPen
        {
            get { return linkPen.Clone(); }
            set { linkPen = value; }
        }

        private SolidColorBrush linkBrush;
        public SolidColorBrush LinkBrush
        {
            get { return linkBrush.Clone(); }
            set { linkBrush = value; }
        }

        #endregion

        #region WidgetCreate

        private Pen createWidgetPen;
        public Pen CreateWidgetPen
        {
            get { return createWidgetPen.Clone(); }
            set { createWidgetPen = value; }
        }

        #endregion

        #region PDFViewer

        private Pen viewerImagePen;
        public Pen ViewerImagePen
        {
            get { return viewerImagePen.Clone(); }
            set { viewerImagePen = value; }
        }

        private Brush viewerImageBackgroundBrush;
        public Brush ViewerImageBackgroundBrush
        {
            get { return viewerImageBackgroundBrush.Clone(); }
            set { viewerImageBackgroundBrush = value; }
        }

        #endregion

        #region PDFEditMultiSelected

        private SolidColorBrush pDFEditMultiMoveBrush;

        public SolidColorBrush PDFEditMultiMoveBrush
        {
            get { return pDFEditMultiMoveBrush.Clone(); }
            set { pDFEditMultiMoveBrush = value; }
        }

        private Pen pDFEditMultiMovePen;

        public Pen PDFEditMultiMovePen
        {
            get { return pDFEditMultiMovePen.Clone(); }
            set { pDFEditMultiMovePen = value; }
        }

        #region Not selected state

        private SolidColorBrush pDFEditMultiRectFillBrush;

        public SolidColorBrush PDFEditMultiRectFillBrush
        {
            get { return pDFEditMultiRectFillBrush.Clone(); }
            set { pDFEditMultiRectFillBrush = value; }
        }

        private Pen pDFEditMultiRectLinePen;

        public Pen PDFEditMultiRectLinePen
        {
            get { return pDFEditMultiRectLinePen.Clone(); }
            set { pDFEditMultiRectLinePen = value; }
        }

        private Pen pDFEditMultiPointPen;

        public Pen PDFEditMultiPointPen
        {
            get { return pDFEditMultiPointPen.Clone(); }
            set { pDFEditMultiPointPen = value; }
        }

        private SolidColorBrush pDFEditMultiPointBorderBrush;

        public SolidColorBrush PDFEditMultiPointBorderBrush
        {
            get { return pDFEditMultiPointBorderBrush.Clone(); }
            set { pDFEditMultiPointBorderBrush = value; }
        }

        #endregion

        #region Selected state


        private SolidColorBrush sPDFEditMultiRectFillBrush;

        public SolidColorBrush SPDFEditMultiRectFillBrush
        {
            get { return sPDFEditMultiRectFillBrush.Clone(); }
            set { sPDFEditMultiRectFillBrush = value; }
        }

        private Pen sPDFEditMultiRectLinePen;

        public Pen SPDFEditMultiRectLinePen
        {
            get { return sPDFEditMultiRectLinePen.Clone(); }
            set { sPDFEditMultiRectLinePen = value; }
        }

        private Pen sPDFEditMultiPointPen;

        public Pen SPDFEditMultiPointPen
        {
            get { return sPDFEditMultiPointPen.Clone(); }
            set { sPDFEditMultiPointPen = value; }
        }

        private SolidColorBrush sPDFEditMultiPointBorderBrush;

        public SolidColorBrush SPDFEditMultiPointBorderBrush
        {
            get { return sPDFEditMultiPointBorderBrush.Clone(); }
            set { sPDFEditMultiPointBorderBrush = value; }
        }

        private SolidColorBrush sPDFEditMultiCropBorderBrush;
        public SolidColorBrush SPDFEditMultiCropBorderBrush
        {
            get { return sPDFEditMultiCropBorderBrush.Clone(); }
            set { sPDFEditMultiCropBorderBrush = value; }
        }

        #endregion

        #region Hover state
        private SolidColorBrush pDFEditMultiRectFillHoverBrush;

        public SolidColorBrush PDFEditMultiRectFillHoverBrush
        {
            get { return pDFEditMultiRectFillHoverBrush.Clone(); }
            set { pDFEditMultiRectFillHoverBrush = value; }
        }

        private Pen pDFEditMultiRectLineHoverPen;

        public Pen PDFEditMultiRectLineHoverPen
        {
            get { return pDFEditMultiRectLineHoverPen.Clone(); }
            set { pDFEditMultiRectLineHoverPen = value; }
        }

        private Pen pDFEditMultiPointHoverPen;

        public Pen PDFEditMultiPointHoverPen
        {
            get { return pDFEditMultiPointHoverPen.Clone(); }
            set { pDFEditMultiPointHoverPen = value; }
        }

        private SolidColorBrush pDFEditMultiPointBorderHoverBrush;

        public SolidColorBrush PDFEditMultiPointBorderHoverBrush
        {
            get { return pDFEditMultiPointBorderHoverBrush.Clone(); }
            set { pDFEditMultiPointBorderHoverBrush = value; }
        }

        #endregion

        #endregion

        #region PageSelected

        private SolidColorBrush pageSelectedBgPen;

        public SolidColorBrush PageSelectedBgPen
        {
            get { return pageSelectedBgPen.Clone(); }
            set { pageSelectedBgPen = value; }
        }

        private SolidColorBrush pageSelectedBgBrush;

        public SolidColorBrush PageSelectedBgBrush
        {
            get { return pageSelectedBgBrush.Clone(); }
            set { pageSelectedBgBrush = value; }
        }

        private SolidColorBrush pageSelectedMoveBrush;

        public SolidColorBrush PageSelectedMoveBrush
        {
            get { return pageSelectedMoveBrush.Clone(); }
            set { pageSelectedMoveBrush = value; }
        }

        private Pen pageSelectedMovePen;

        public Pen PageSelectedMovePen
        {
            get { return pageSelectedMovePen.Clone(); }
            set { pageSelectedMovePen = value; }
        }

        #region Not selected state

        private SolidColorBrush pageSelectedRectFillBrush;

        public SolidColorBrush PageSelectedRectFillBrush
        {
            get { return pageSelectedRectFillBrush.Clone(); }
            set { pageSelectedRectFillBrush = value; }
        }

        private Pen pageSelectedRectLinePen;

        public Pen PageSelectedRectLinePen
        {
            get { return pageSelectedRectLinePen.Clone(); }
            set { pageSelectedRectLinePen = value; }
        }

        private Pen pageSelectedPointPen;

        public Pen PageSelectedPointPen
        {
            get { return pageSelectedPointPen.Clone(); }
            set { pageSelectedPointPen = value; }
        }

        private SolidColorBrush pageSelectedPointBorderBrush;

        public SolidColorBrush PageSelectedPointBorderBrush
        {
            get { return pageSelectedPointBorderBrush.Clone(); }
            set { pageSelectedPointBorderBrush = value; }
        }

        #endregion

        #region Selected state


        private SolidColorBrush sPageSelectedRectFillBrush;

        public SolidColorBrush SPageSelectedRectFillBrush
        {
            get { return sPageSelectedRectFillBrush.Clone(); }
            set { sPageSelectedRectFillBrush = value; }
        }

        private Pen sPageSelectedRectLinePen;

        public Pen SPageSelectedRectLinePen
        {
            get { return sPageSelectedRectLinePen.Clone(); }
            set { sPageSelectedRectLinePen = value; }
        }

        private Pen sPageSelectedPointPen;

        public Pen SPageSelectedPointPen
        {
            get { return sPageSelectedPointPen.Clone(); }
            set { sPageSelectedPointPen = value; }
        }

        private SolidColorBrush sPageSelectedPointBorderBrush;

        public SolidColorBrush SPageSelectedPointBorderBrush
        {
            get { return sPageSelectedPointBorderBrush.Clone(); }
            set { sPageSelectedPointBorderBrush = value; }
        }

        private SolidColorBrush sPageSelectedCropBorderBrush;
        public SolidColorBrush SPageSelectedCropBorderBrush
        {
            get { return sPageSelectedCropBorderBrush.Clone(); }
            set { sPageSelectedCropBorderBrush = value; }
        }

        #endregion

        #region Horver state
        private SolidColorBrush pageSelectedRectFillHoverBrush;

        public SolidColorBrush PageSelectedRectFillHoverBrush
        {
            get { return pageSelectedRectFillHoverBrush.Clone(); }
            set { pageSelectedRectFillHoverBrush = value; }
        }

        private Pen pageSelectedRectLineHoverPen;

        public Pen PageSelectedRectLineHoverPen
        {
            get { return pageSelectedRectLineHoverPen.Clone(); }
            set { pageSelectedRectLineHoverPen = value; }
        }

        private Pen pageSelectedPointHoverPen;

        public Pen PageSelectedPointHoverPen
        {
            get { return pageSelectedPointHoverPen.Clone(); }
            set { pageSelectedPointHoverPen = value; }
        }

        private SolidColorBrush pageSelectedPointBorderHoverBrush;

        public SolidColorBrush PageSelectedPointBorderHoverBrush
        {
            get { return pageSelectedPointBorderHoverBrush.Clone(); }
            set { pageSelectedPointBorderHoverBrush = value; }
        }

        #endregion


        #endregion

        public DefaultDrawParam()
        {
            //ShowAnnot
            ShowLinkBrush = new SolidColorBrush(Color.FromRgb(0x78, 0xB4, 0xDB));
            ShowLinkePen = null;

            // AnnotSelectedRect
            AnnotMoveBrush = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));
            AnnotMovePen = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)), 2);

            AnnotRectFillBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            AnnotRectLinePen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            AnnotRectHoverPen = new Pen(Brushes.Orange, 2) { DashStyle = DashStyles.Dash };
            AnnotPointPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            AnnotPointBorderBrush = Brushes.White;

            //PDFEdit
            CaretBrush = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));
            CaretPen = new Pen(Brushes.Black, 1);
            PDFEditMovePen = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)), 2);
            PDFEditMoveBrush = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));


            PDFEditRectFillHoverBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            PDFEditRectLineHoverPen = new Pen(Brushes.Orange, 2) { DashStyle = DashStyles.Dash };
            PDFEditPointHoverPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            PDFEditPointBorderHoverBrush = Brushes.White;

            PDFEditRectFillBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            PDFEditRectLinePen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2) { DashStyle = DashStyles.Dash };
            PDFEditPointPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            PDFEditPointBorderBrush = Brushes.White;

            SPDFEditRectFillBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            SPDFEditRectLinePen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            SPDFEditPointPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            SPDFEditPointBorderBrush = Brushes.White;
            SPDFEditCropBorderBrush = new SolidColorBrush(Color.FromRgb(71, 126, 222));

            //AnnotEdit 
            EditControlLineBrush = new SolidColorBrush(Color.FromRgb(0x78, 0xB4, 0xDB));
            EditControlLinePen = new Pen(new SolidColorBrush(Color.FromRgb(0x78, 0xB4, 0xDB)), 1);
            EditLinePen = new Pen(new SolidColorBrush(Color.FromRgb(0x78, 0xB4, 0xDB)), 1);

            //AnnotCreate
            RedactPen = new Pen(Brushes.Red, 1);
            LinkPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 1);
            LinkBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xDD, 0xEA));

            //WidgetCreate
            CreateWidgetPen = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)), 2);

            //PDFViewer
            ViewerImagePen = new Pen(new SolidColorBrush(Color.FromRgb(0x46, 0x82, 0xB4)), 1);
            ViewerImageBackgroundBrush = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));

            //PDFEditMultiSelected
            PDFEditMultiMovePen = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)), 2);
            PDFEditMultiMoveBrush = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));

            PDFEditMultiRectFillHoverBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            PDFEditMultiRectLineHoverPen = new Pen(Brushes.Orange, 2) { DashStyle = DashStyles.Dash };
            PDFEditMultiPointHoverPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            PDFEditMultiPointBorderHoverBrush = Brushes.White;

            PDFEditMultiRectFillBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            PDFEditMultiRectLinePen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2) { DashStyle = DashStyles.Solid };
            PDFEditMultiPointPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            PDFEditMultiPointBorderBrush = Brushes.White;

            SPDFEditMultiRectFillBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            SPDFEditMultiRectLinePen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            SPDFEditMultiPointPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            SPDFEditMultiPointBorderBrush = Brushes.White;
            SPDFEditMultiCropBorderBrush = new SolidColorBrush(Color.FromRgb(71, 126, 222));

            //PageSelected
            PageSelectedBgPen = null;
            PageSelectedBgBrush = new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x00, 0x00));

            PageSelectedMovePen = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00)), 2);
            PageSelectedMoveBrush = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));

            PageSelectedRectFillHoverBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            PageSelectedRectLineHoverPen = new Pen(Brushes.Orange, 2) { DashStyle = DashStyles.Dash };
            PageSelectedPointHoverPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            PageSelectedPointBorderHoverBrush = Brushes.White;

            PageSelectedRectFillBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            PageSelectedRectLinePen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2) { DashStyle = DashStyles.Dash };
            PageSelectedPointPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            PageSelectedPointBorderBrush = Brushes.White;

            SPageSelectedRectFillBrush = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00));
            SPageSelectedRectLinePen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            SPageSelectedPointPen = new Pen(new SolidColorBrush(Color.FromRgb(71, 126, 222)), 2);
            SPageSelectedPointBorderBrush = Brushes.White;
            SPageSelectedCropBorderBrush = new SolidColorBrush(Color.FromRgb(71, 126, 222));

        }
    }

    public class MeasureSetting
    {
        public double RulerBase { get; set; } = 1;
        public string RulerBaseUnit { get; set; } = CPDFMeasure.CPDF_CM;
        public double RulerTranslate { get; set; } = 1;
        public string RulerTranslateUnit { get; set; } = CPDFMeasure.CPDF_CM;
        public double Precision { get; set; } = 0.01;
        public double MoveDetectionLength { get; set; } = 10;
        public bool IsShowArea { get; set; } = true;
        public bool IsShowLength { get; set; } = true;

        /// <summary>
        /// Return inch
        /// </summary>
        internal static double GetPointLength(Point startPoint, Point endPoint, double zoom)
        {
            if (zoom > 0)
            {
                return (endPoint - startPoint).Length / zoom / 96D;
            }
            return 0;
        }

        internal double GetMeasureRatio(string baseUnit)
        {
            if (baseUnit == CPDFMeasure.CPDF_PT)
            {
                return 1 / 72;
            }
            if (baseUnit == CPDFMeasure.CPDF_IN)
            {
                return 1;
            }
            if (baseUnit == CPDFMeasure.CPDF_MM)
            {
                return 1 / 25.4;
            }
            if (baseUnit == CPDFMeasure.CPDF_CM)
            {
                return 1 / 2.54;
            }
            if (baseUnit == CPDFMeasure.CPDF_M)
            {
                return 1 / 0.0254;
            }
            if (baseUnit == CPDFMeasure.CPDFO_KM)
            {
                return 1 / 0.0254 / 1000;
            }

            if (baseUnit == CPDFMeasure.CPDF_FT)
            {
                return 12;
            }
            if (baseUnit == CPDFMeasure.CPDF_YD)
            {
                return 36;
            }
            if (baseUnit == CPDFMeasure.CPDF_MI)
            {
                return 63360;
            }
            return 0;
        }

        internal double GetMeasureAreaRatio()
        {
            if (RulerBaseUnit == CPDFMeasure.CPDF_PT)
            {
                return CPDFMeasure.pt;
            }
            if (RulerBaseUnit == CPDFMeasure.CPDF_IN)
            {
                return CPDFMeasure.pt_in;
            }
            if (RulerBaseUnit == CPDFMeasure.CPDF_MM)
            {
                return CPDFMeasure.pt_mm;
            }
            if (RulerBaseUnit == CPDFMeasure.CPDF_CM)
            {
                return CPDFMeasure.pt_cm;
            }
            if (RulerBaseUnit == CPDFMeasure.CPDF_M)
            {
                return CPDFMeasure.pt_m;
            }
            if (RulerBaseUnit == CPDFMeasure.CPDFO_KM)
            {
                return CPDFMeasure.pt_km;
            }

            if (RulerBaseUnit == CPDFMeasure.CPDF_FT)
            {
                return CPDFMeasure.pt_ft;
            }
            if (RulerBaseUnit == CPDFMeasure.CPDF_YD)
            {
                return CPDFMeasure.pt_yd;
            }
            if (RulerBaseUnit == CPDFMeasure.CPDF_MI)
            {
                return CPDFMeasure.pt_mi;
            }
            return 0;
        }

        public int GetMeasureSavePrecision()
        {
            if (Precision == 1)
            {
                return CPDFMeasure.PRECISION_VALUE_ZERO;
            }
            if (Precision == 0.1)
            {
                return CPDFMeasure.PRECISION_VALUE_ONE;
            }
            if (Precision == 0.01)
            {
                return CPDFMeasure.PRECISION_VALUE_TWO;
            }
            if (Precision == 0.001)
            {
                return CPDFMeasure.PRECISION_VALUE_THREE;
            }
            if (Precision == 0.0001)
            {
                return CPDFMeasure.PRECISION_VALUE_FOUR;
            }
            return 0;
        }

        internal double GetMeasureShowPrecision(int precision)
        {
            if (precision == CPDFMeasure.PRECISION_VALUE_ZERO)
            {
                return 1;
            }
            if (CPDFMeasure.PRECISION_VALUE_ONE == precision)
            {
                return 0.1;
            }
            if (CPDFMeasure.PRECISION_VALUE_TWO == precision)
            {
                return 0.01;
            }
            if (CPDFMeasure.PRECISION_VALUE_THREE == precision)
            {
                return 0.001;
            }
            if (CPDFMeasure.PRECISION_VALUE_FOUR == precision)
            {
                return 0.0001;
            }
            return 0;
        }

        internal double GetMeasureLength(Point startPoint, Point endPoint, double zoom)
        {
            try
            {
                double inch = GetPointLength(startPoint, endPoint, zoom);
                double ratio = GetMeasureRatio(RulerBaseUnit);
                double rate = RulerTranslate / RulerBase;
                return inch * rate / ratio;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public double GetMeasurePDFLength(double lenght)
        {
            try
            {
                double inch = lenght / 72D;
                double ratio = GetMeasureRatio(RulerBaseUnit);
                double rate = RulerTranslate / RulerBase;
                return inch * rate / ratio;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public double GetMeasurePDFArea(double area)
        {
            try
            {
                double inch = area;
                double ratio = GetMeasureAreaRatio();
                double rate = RulerTranslate / RulerBase;
                return inch * ratio * ratio * rate * rate;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public string GetPrecisionData(double number)
        {
            NumberFormatInfo formatInfo = new NumberFormatInfo();
            formatInfo.NumberDecimalDigits = 2;
            if (Precision == 1)
            {
                formatInfo.NumberDecimalDigits = 0;
            }
            if (Precision == 0.1)
            {
                formatInfo.NumberDecimalDigits = 1;
            }
            if (Precision == 0.01)
            {
                formatInfo.NumberDecimalDigits = 2;
            }
            if (Precision == 0.001)
            {
                formatInfo.NumberDecimalDigits = 3;
            }
            if (Precision == 0.0001)
            {
                formatInfo.NumberDecimalDigits = 4;
            }
            if (Precision == 0.00001)
            {
                formatInfo.NumberDecimalDigits = 5;
            }

            return number.ToString("N", formatInfo);
        }

        internal double ComputePolygonArea(List<Point> points)
        {
            int point_num = points.Count;
            if (point_num < 3)
                return 0.0;
            double s = points[0].Y * (points[point_num - 1].X - points[1].X);
            for (int i = 1; i < point_num; ++i)
                s += points[i].Y * (points[i - 1].X - points[(i + 1) % point_num].X);
            return Math.Abs(s / 2.0);
        }
    }

}
