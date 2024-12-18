using ComPDFKit.Import;
using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static ComPDFKit.PDFAnnotation.CTextAttribute;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;

namespace ComPDFKit.Tool.Help
{
    /// <summary>
    /// Some quick conversion tools for parameters
    /// </summary>
    public static class ParamConverter
    {
        public enum FormField
        {
            /// <summary>
            /// Visible
            /// </summary>
            Visible,
            /// <summary>
            /// Hidden
            /// </summary>
            Hidden,
            /// <summary>
            /// Visible but unprintable
            /// </summary>
            VisibleNoPrint,
            /// <summary>
            /// Hidden but printable
            /// </summary>
            HiddenPrintable
        }

        public static AnnotHistory CreateHistory(CPDFAnnotation cPDFAnnotation)
        {
            AnnotHistory annotHistory = null;
            switch (cPDFAnnotation.Type)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_NONE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNKOWN:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                    annotHistory = new StickyNoteAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                    annotHistory = new LinkAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                    annotHistory = new FreeTextAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    if ((cPDFAnnotation as CPDFLineAnnotation).IsMeasured())
                    {
                        annotHistory = new LineMeasureAnnotHistory();
                    }
                    else
                    {
                        annotHistory = new LineAnnotHistory();
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                    annotHistory = new SquareAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                    annotHistory = new CircleAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    annotHistory = new PolygonMeasureAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    annotHistory = new PolyLineMeasureAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                    annotHistory = new HighlightAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                    annotHistory = new UnderlineAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                    annotHistory = new SquigglyAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                    annotHistory = new StrikeoutAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                    annotHistory = new StampAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CARET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                    annotHistory = new InkAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POPUP:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FILEATTACHMENT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                    annotHistory = new SoundAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    switch ((cPDFAnnotation as CPDFWidget).WidgetType)
                    {
                        case C_WIDGET_TYPE.WIDGET_NONE:
                            break;
                        case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                            annotHistory = new PushButtonHistory();
                            break;
                        case C_WIDGET_TYPE.WIDGET_CHECKBOX:
                            annotHistory = new CheckBoxHistory();
                            break;
                        case C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                            annotHistory = new RadioButtonHistory();
                            break;
                        case C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                            annotHistory = new TextBoxHistory();
                            break;
                        case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                            annotHistory = new ComboBoxHistory();
                            break;
                        case C_WIDGET_TYPE.WIDGET_LISTBOX:
                            annotHistory = new ListBoxHistory();
                            break;
                        case C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                            annotHistory = new SignatureHistory();
                            break;
                        case C_WIDGET_TYPE.WIDGET_UNKNOWN:
                            break;
                        default:
                            break;
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
                    annotHistory = new RedactAnnotHistory();
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INTERCHANGE:
                    break;
                default:
                    break;
            }
            return annotHistory;
        }

        public static bool RemovePageAnnot(Dictionary<int, List<int>> removeAnnots, CPDFViewer cPDFViewer)
        {
            CPDFDocument cPDFDocument = cPDFViewer.GetDocument();
            if (cPDFDocument == null || removeAnnots.Count <= 0)
            {
                return false;
            }

            GroupHistory historyGroup = new GroupHistory();
            foreach (int pageIndex in removeAnnots.Keys)
            {
                CPDFPage pageCore = cPDFDocument.PageAtIndex(pageIndex);
                List<CPDFAnnotation> cPDFAnnotations = pageCore.GetAnnotations();
                foreach (int annotIndex in removeAnnots[pageIndex])
                {
                    CPDFAnnotation cPDFAnnotation = cPDFAnnotations.ElementAtOrDefault(annotIndex);
                    if (cPDFAnnotation != null)
                    {
                        AnnotParam annotParam = CPDFDataConverterToAnnotParam(cPDFDocument, pageIndex, cPDFAnnotation);
                        if (annotParam is StampParam stampParam)
                        {
                            if (stampParam.StampType == C_STAMP_TYPE.IMAGE_STAMP)
                            {
                                stampParam.CopyImageAnnot = CPDFAnnotation.CopyAnnot(cPDFAnnotation);
                            }
                        }

                        AnnotHistory annotHistory = CreateHistory(cPDFAnnotation);
                        annotHistory.CurrentParam = annotParam;
                        annotHistory.PDFDoc = cPDFDocument;
                        annotHistory.Action = HistoryAction.Remove;
                        historyGroup.Histories.Add(annotHistory);
                    }
                }
            }

            if (historyGroup.Histories.Count > 0)
            {
                cPDFViewer.UndoManager.AddHistory(historyGroup);
            }

            foreach (int pageIndex in removeAnnots.Keys)
            {
                CPDFPage pageCore = cPDFDocument.PageAtIndex(pageIndex);
                List<CPDFAnnotation> cPDFAnnotations = pageCore.GetAnnotations();
                foreach (int annotIndex in removeAnnots[pageIndex])
                {
                    CPDFAnnotation cPDFAnnotation = cPDFAnnotations.ElementAtOrDefault(annotIndex);
                    if (cPDFAnnotation != null)
                    {
                        cPDFAnnotation.RemoveAnnot();
                    }
                }
            }

            return true;
        }

        public static FormField ConverterWidgetFormFlags(int Flags, bool IsHidden)
        {

            int flag = Flags;
            bool invisible = (flag & (int)CPDFAnnotationFlags.CPDFAnnotationFlagInvisible) != 0;
            bool noview = (flag & (int)CPDFAnnotationFlags.CPDFAnnotationFlagNoView) != 0;
            bool print = (flag & (int)CPDFAnnotationFlags.CPDFAnnotationFlagPrint) != 0;

            if (noview && print)
            {
                return FormField.HiddenPrintable;
            }

            if (IsHidden || noview || invisible)
            {
                return FormField.Hidden;
            }

            bool visibleflag = !IsHidden && !invisible && !noview && !print;

            if (visibleflag)
            {
                return FormField.VisibleNoPrint;
            }

            return FormField.Visible;
        }

        public static int GetFormFlags(FormField field, CPDFWidget widget)
        {
            int flag = widget.GetFlags();

            switch (field)
            {
                case FormField.Hidden:
                    widget.SetIsHidden(true);
                    flag = widget.GetFlags();
                    break;
                case FormField.Visible:
                    {
                        int newflag = (int)CPDFAnnotationFlags.CPDFAnnotationFlagNoView | (int)CPDFAnnotationFlags.CPDFAnnotationFlagHidden
                            | (int)CPDFAnnotationFlags.CPDFAnnotationFlagInvisible;
                        flag &= ~newflag;
                        flag |= (int)CPDFAnnotationFlags.CPDFAnnotationFlagPrint;
                    }
                    break;
                case FormField.VisibleNoPrint:
                    {
                        int newflag = (int)CPDFAnnotationFlags.CPDFAnnotationFlagNoView | (int)CPDFAnnotationFlags.CPDFAnnotationFlagPrint |
                       (int)CPDFAnnotationFlags.CPDFAnnotationFlagHidden | (int)CPDFAnnotationFlags.CPDFAnnotationFlagInvisible;
                        flag &= ~newflag;
                    }
                    break;
                case FormField.HiddenPrintable:
                    {
                        flag = flag | (int)CPDFAnnotationFlags.CPDFAnnotationFlagNoView | (int)CPDFAnnotationFlags.CPDFAnnotationFlagPrint;
                    }
                    break;
                default:
                    break;
            }
            return flag;
        }

        public static Color ConverterByteForColor(byte[] bytes)
        {
            if (bytes != null && bytes.Length == 3)
            {
                return new Color { R = bytes[0], G = bytes[1], B = bytes[2], A = 255};
            }
            return new Color { R = 0, G = 0, B = 0, A = 0 };
        }

        public static void ParseDashStyle(DashStyle dashStyle, out float[] LineDash, out C_BORDER_STYLE BorderStyle)
        {
            if (dashStyle == DashStyles.Solid || dashStyle == null)
            {
                LineDash = new float[0];
                BorderStyle = C_BORDER_STYLE.BS_SOLID;
            }
            else
            {
                List<float> floatArray = new List<float>();
                foreach (double num in dashStyle.Dashes)
                {
                    floatArray.Add((float)num);
                }
                LineDash = floatArray.ToArray();
                BorderStyle = C_BORDER_STYLE.BS_DASHDED;
            }
        }

        public static AnnotParam CPDFDataConverterToAnnotParam(CPDFDocument cPDFDocument, int PageIndex, CPDFAnnotation cPDFAnnotation)
        {
            if (cPDFAnnotation == null && !cPDFAnnotation.IsValid() && cPDFDocument == null && !cPDFDocument.IsValid())
            {
                return null;
            }

            AnnotParam annotParam = null;
            if (cPDFAnnotation.Type == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                annotParam = WidgetConverter(cPDFDocument, cPDFAnnotation);
            }
            else
            {
                annotParam = AnnotConverter(cPDFDocument, cPDFAnnotation);
            }

            if (annotParam != null)
            {
                annotParam.PageIndex = PageIndex;
                CPDFPage cPDFPage = cPDFDocument.PageAtIndex(annotParam.PageIndex, false);
                List<CPDFAnnotation> cPDFAnnotations = cPDFPage.GetAnnotations();
                annotParam.AnnotIndex = cPDFAnnotations.IndexOf(cPDFAnnotation);
            }

            return annotParam;
        }

        public static PDFEditParam CPDFDataConverterToPDFEitParam(CPDFDocument cPDFDocument, CPDFEditArea cPDFEditArea, int pageIndex)
        {
            PDFEditParam annotParam = null;
            if (cPDFEditArea == null && !cPDFEditArea.IsValid() && cPDFDocument == null && !cPDFDocument.IsValid() && pageIndex >= 0)
            {
                return null;
            }
            switch (cPDFEditArea.Type)
            {
                case CPDFEditType.None:
                    break;
                case CPDFEditType.EditText:
                    annotParam = GetTextEditParam(cPDFDocument, cPDFEditArea as CPDFEditTextArea, pageIndex);
                    break;
                case CPDFEditType.EditImage:
                    annotParam = GetImageEditParam(cPDFDocument, cPDFEditArea as CPDFEditImageArea, pageIndex);
                    break;
                case CPDFEditType.EditPath:
                    annotParam = GetPathEditParam(cPDFDocument, cPDFEditArea as CPDFEditPathArea, pageIndex);
                    break;

                default:
                    break;
            }

            return annotParam;
        }

        #region PDFEdit

        internal static TextEditParam GetTextEditParam(CPDFDocument cPDFDocument, CPDFEditTextArea cPDFEditArea, int pageIndex)
        {
            TextEditParam textEditParam = new TextEditParam();
            string fontName = "Helvetica";
            float fontSize = 14;
            byte[] fontColor = {0,0,0};
            byte transparency = 255;
            bool isBold = false;
            bool isItalic = false;

            cPDFEditArea.GetTextStyle(ref fontName,ref fontSize, ref fontColor, ref transparency, ref isBold, ref isItalic);
            textEditParam.FontName = fontName;
            textEditParam.FontSize = fontSize;
            textEditParam.FontColor = fontColor;
            textEditParam.Transparency = transparency;
            textEditParam.TextAlign = cPDFEditArea.GetTextSectionAlign();
            textEditParam.EditType = CPDFEditType.EditText;
            CPDFPage docPage = cPDFDocument.PageAtIndex(pageIndex);
            CPDFEditPage editPage = docPage.GetEditPage();
            textEditParam.EditIndex = editPage.GetEditAreaList().IndexOf(cPDFEditArea);
            textEditParam.PageIndex = pageIndex;

            if(string.IsNullOrEmpty(cPDFEditArea.SelectText))
            {
                textEditParam.IsBold = isBold;
                textEditParam.IsItalic = isItalic;
            }
            else
            {
                textEditParam.IsBold = cPDFEditArea.IsCharsFontBold();
                textEditParam.IsItalic = cPDFEditArea.IsCharsFontItalic();
            }    

            return textEditParam;
        }

        internal static ImageEditParam GetImageEditParam(CPDFDocument cPDFDocument, CPDFEditImageArea cPDFEditArea, int pageIndex)
        {
            ImageEditParam imageEditParam = new ImageEditParam();
            imageEditParam.Transparency = cPDFEditArea.GetImageTransparency();
            imageEditParam.Rotate = cPDFEditArea.GetRotation();
            imageEditParam.ClipRect = cPDFEditArea.GetClipRect();
            imageEditParam.EditType = CPDFEditType.EditImage;

            CPDFPage docPage = cPDFDocument.PageAtIndex(pageIndex);
            CPDFEditPage editPage = docPage.GetEditPage();
            imageEditParam.EditIndex = editPage.GetEditAreaList().IndexOf(cPDFEditArea);
            imageEditParam.PageIndex = pageIndex;
            return imageEditParam;
        }

        internal static PathEditParam GetPathEditParam(CPDFDocument cPDFDocument, CPDFEditPathArea cPDFEditArea, int pageIndex)
        {
            PathEditParam pathEditParam = new PathEditParam();
            pathEditParam.Transparency = cPDFEditArea.GetTransparency();
            pathEditParam.Rotate = cPDFEditArea.GetRotation();
            pathEditParam.StrokeColor = cPDFEditArea.GetStrokeColor();
            pathEditParam.FillColor = cPDFEditArea.GetFillColor();
            pathEditParam.ClipRect = cPDFEditArea.GetClipRect();
            pathEditParam.EditType = CPDFEditType.EditPath;

            CPDFPage docPage = cPDFDocument.PageAtIndex(pageIndex);
            CPDFEditPage editPage = docPage.GetEditPage();
            pathEditParam.EditIndex = editPage.GetEditAreaList().IndexOf(cPDFEditArea);
            pathEditParam.PageIndex = pageIndex;
            return pathEditParam;
        }

        #endregion

        #region Widegt

        internal static AnnotParam WidgetConverter(CPDFDocument document, CPDFAnnotation cPDFAnnotation)
        {
            AnnotParam annotParam = null;
            if (cPDFAnnotation is CPDFWidget)
            {
                switch ((cPDFAnnotation as CPDFWidget).WidgetType)
                {
                    case C_WIDGET_TYPE.WIDGET_NONE:
                        break;
                    case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                        annotParam = GetPushButtonParam(document, cPDFAnnotation as CPDFPushButtonWidget);
                        break;
                    case C_WIDGET_TYPE.WIDGET_CHECKBOX:
                        annotParam = GetCheckBoxParam(cPDFAnnotation as CPDFCheckBoxWidget);
                        break;
                    case C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                        annotParam = GetRadioButtonParam(cPDFAnnotation as CPDFRadioButtonWidget);
                        break;
                    case C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                        annotParam = GetTextBoxParam(cPDFAnnotation as CPDFTextWidget);
                        break;
                    case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                        annotParam = GetComboBoxParam(cPDFAnnotation as CPDFComboBoxWidget);
                        break;
                    case C_WIDGET_TYPE.WIDGET_LISTBOX:
                        annotParam = GetListBoxParam(cPDFAnnotation as CPDFListBoxWidget);
                        break;
                    case C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                        annotParam = GetSignatureParam(cPDFAnnotation as CPDFSignatureWidget);
                        break;
                    case C_WIDGET_TYPE.WIDGET_UNKNOWN:
                        break;
                    default:
                        break;
                }
            }
            return annotParam;
        }

        internal static PushButtonParam GetPushButtonParam(CPDFDocument document, CPDFPushButtonWidget cPDFWidget)
        {
            PushButtonParam pushButtonParam = null;
            if (cPDFWidget != null && cPDFWidget.IsValid())
            {
                pushButtonParam = new PushButtonParam();
                CTextAttribute cTextAttribute = cPDFWidget.GetTextAttribute();
                pushButtonParam.IsBold = IsBold(cTextAttribute.FontName);
                pushButtonParam.IsItalic = IsItalic(cTextAttribute.FontName);
                pushButtonParam.Text = cPDFWidget.GetButtonTitle();
                pushButtonParam.FontColor = cTextAttribute.FontColor;
                pushButtonParam.FontName = cTextAttribute.FontName;
                pushButtonParam.FontSize = cTextAttribute.FontSize;

                CPDFAction cPDFAction = cPDFWidget.GetButtonAction();
                if (cPDFAction != null)
                {
                    switch (cPDFAction.ActionType)
                    {
                        case C_ACTION_TYPE.ACTION_TYPE_URI:
                            pushButtonParam.Uri = (cPDFAction as CPDFUriAction)?.GetUri();
                            break;
                        case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                            CPDFGoToAction gotoAction = cPDFAction as CPDFGoToAction;
                            CPDFDestination dest = gotoAction.GetDestination(document);
                            pushButtonParam.DestinationPageIndex = dest.PageIndex;
                            pushButtonParam.DestinationPosition = new CPoint(dest.Position_X, dest.Position_Y);
                            break;
                        default:
                            break;
                    }
                    pushButtonParam.Action = cPDFAction.ActionType;
                }

                pushButtonParam.WidgetType = cPDFWidget.WidgetType;
                pushButtonParam.BorderStyle = cPDFWidget.GetWidgetBorderStyle();

                byte[] LineColor = new byte[3];
                pushButtonParam.HasLineColor = cPDFWidget.GetWidgetBorderRGBColor(ref LineColor);
                pushButtonParam.LineColor = LineColor;

                byte[] BgColor = new byte[3];
                bool hasBgColor = cPDFWidget.GetWidgetBgRGBColor(ref BgColor);
                pushButtonParam.HasBgColor = hasBgColor;
                pushButtonParam.BgColor = BgColor;
                pushButtonParam.LineWidth = cPDFWidget.GetBorderWidth();
                pushButtonParam.FieldName = cPDFWidget.GetFieldName();
                pushButtonParam.Flags = cPDFWidget.GetFlags();
                pushButtonParam.IsReadOnly = cPDFWidget.GetIsReadOnly();
                pushButtonParam.IsHidden = cPDFWidget.GetIsHidden();

                GetAnnotCommonParam(cPDFWidget, pushButtonParam);
            }
            return pushButtonParam;
        }

        internal static CheckBoxParam GetCheckBoxParam(CPDFCheckBoxWidget cPDFWidget)
        {
            CheckBoxParam checkBoxParam = null;
            if (cPDFWidget != null && cPDFWidget.IsValid())
            {
                checkBoxParam = new CheckBoxParam();
                checkBoxParam.CheckStyle = cPDFWidget.GetWidgetCheckStyle();
                checkBoxParam.IsChecked = cPDFWidget.IsChecked();

                checkBoxParam.WidgetType = cPDFWidget.WidgetType;
                checkBoxParam.BorderStyle = cPDFWidget.GetWidgetBorderStyle();

                byte[] LineColor = new byte[3];
                checkBoxParam.HasLineColor = cPDFWidget.GetWidgetBorderRGBColor(ref LineColor);
                checkBoxParam.LineColor = LineColor;

                byte[] BgColor = new byte[3];
                bool hasBgColor = cPDFWidget.GetWidgetBgRGBColor(ref BgColor);
                checkBoxParam.HasBgColor = hasBgColor;
                checkBoxParam.BgColor = BgColor;

                CTextAttribute cTextAttribute = cPDFWidget.GetTextAttribute();
                checkBoxParam.FontColor = cTextAttribute.FontColor;
                checkBoxParam.FontName = cTextAttribute.FontName;
                checkBoxParam.FontSize = cTextAttribute.FontSize;

                checkBoxParam.LineWidth = cPDFWidget.GetBorderWidth();
                checkBoxParam.FieldName = cPDFWidget.GetFieldName();
                checkBoxParam.Flags = cPDFWidget.GetFlags();
                checkBoxParam.IsReadOnly = cPDFWidget.GetIsReadOnly();
                checkBoxParam.IsHidden = cPDFWidget.GetIsHidden();

                GetAnnotCommonParam(cPDFWidget, checkBoxParam);

            }
            return checkBoxParam;
        }

        internal static RadioButtonParam GetRadioButtonParam(CPDFRadioButtonWidget cPDFWidget)
        {
            RadioButtonParam radioButtonParam = null;
            if (cPDFWidget != null && cPDFWidget.IsValid())
            {
                radioButtonParam = new RadioButtonParam();
                radioButtonParam.CheckStyle = cPDFWidget.GetWidgetCheckStyle();
                radioButtonParam.IsChecked = cPDFWidget.IsChecked();

                radioButtonParam.WidgetType = cPDFWidget.WidgetType;
                radioButtonParam.BorderStyle = cPDFWidget.GetWidgetBorderStyle();

                byte[] LineColor = new byte[3];
                radioButtonParam.HasLineColor = cPDFWidget.GetWidgetBorderRGBColor(ref LineColor);
                radioButtonParam.LineColor = LineColor;

                byte[] BgColor = new byte[3];
                bool hasBgColor = cPDFWidget.GetWidgetBgRGBColor(ref BgColor);
                radioButtonParam.HasBgColor = hasBgColor;
                radioButtonParam.BgColor = BgColor;

                CTextAttribute cTextAttribute = cPDFWidget.GetTextAttribute();
                radioButtonParam.FontColor = cTextAttribute.FontColor;
                radioButtonParam.FontName = cTextAttribute.FontName;
                radioButtonParam.FontSize = cTextAttribute.FontSize;

                radioButtonParam.LineWidth = cPDFWidget.GetBorderWidth();
                radioButtonParam.FieldName = cPDFWidget.GetFieldName();
                radioButtonParam.Flags = cPDFWidget.GetFlags();
                radioButtonParam.IsReadOnly = cPDFWidget.GetIsReadOnly();
                radioButtonParam.IsHidden = cPDFWidget.GetIsHidden();

                GetAnnotCommonParam(cPDFWidget, radioButtonParam);

            }
            return radioButtonParam;
        }

        internal static TextBoxParam GetTextBoxParam(CPDFTextWidget cPDFWidget)
        {
            TextBoxParam textBoxParam = null;
            if (cPDFWidget != null && cPDFWidget.IsValid())
            {
                textBoxParam = new TextBoxParam();
                CTextAttribute cTextAttribute = cPDFWidget.GetTextAttribute();
                textBoxParam.Text = cPDFWidget.Text;
                textBoxParam.IsMultiLine = cPDFWidget.IsMultiLine;
                textBoxParam.IsPassword = cPDFWidget.IsPassword;
                textBoxParam.IsItalic = IsItalic(cTextAttribute.FontName);
                textBoxParam.IsBold = IsBold(cTextAttribute.FontName);
                textBoxParam.Alignment = cPDFWidget.Alignment;

                textBoxParam.WidgetType = cPDFWidget.WidgetType;
                textBoxParam.BorderStyle = cPDFWidget.GetWidgetBorderStyle();

                byte[] LineColor = new byte[3];
                textBoxParam.HasLineColor = cPDFWidget.GetWidgetBorderRGBColor(ref LineColor);
                textBoxParam.LineColor = LineColor;

                byte[] BgColor = new byte[3];
                bool hasBgColor = cPDFWidget.GetWidgetBgRGBColor(ref BgColor);
                textBoxParam.HasBgColor = hasBgColor;
                textBoxParam.BgColor = BgColor;

                textBoxParam.FontColor = cTextAttribute.FontColor;
                textBoxParam.FontName = cTextAttribute.FontName;
                textBoxParam.FontSize = cTextAttribute.FontSize;

                textBoxParam.LineWidth = cPDFWidget.GetBorderWidth();
                textBoxParam.FieldName = cPDFWidget.GetFieldName();
                textBoxParam.Flags = cPDFWidget.GetFlags();
                textBoxParam.IsReadOnly = cPDFWidget.GetIsReadOnly();
                textBoxParam.IsHidden = cPDFWidget.GetIsHidden();

                GetAnnotCommonParam(cPDFWidget, textBoxParam);

            }
            return textBoxParam;
        }

        internal static ComboBoxParam GetComboBoxParam(CPDFComboBoxWidget cPDFWidget)
        {
            ComboBoxParam comboBoxParam = null;
            if (cPDFWidget != null && cPDFWidget.IsValid())
            {
                comboBoxParam = new ComboBoxParam();
                CTextAttribute cTextAttribute = cPDFWidget.GetTextAttribute();
                comboBoxParam.IsItalic = IsItalic(cTextAttribute.FontName);
                comboBoxParam.IsBold = IsBold(cTextAttribute.FontName);

                //Support for multiple selections needs to be adjusted later.
                CWidgetItem[] cWidgetItem = cPDFWidget.LoadWidgetItems();
                CWidgetItem cWidgetItem1 = cPDFWidget.GetSelectedItem();

                if (cWidgetItem != null)
                {
                    for (int i = 0; i < cWidgetItem.Length; i++)
                    {
                        CWidgetItem item = cWidgetItem[i];
                        comboBoxParam.OptionItems.Add(item.Value, item.Text);
                        if (cWidgetItem1 != null && cWidgetItem1.Text == item.Text && cWidgetItem1.Value == item.Value)
                        {
                            comboBoxParam.SelectItemsIndex = new List<int> { i };
                        }
                    }
                }

                comboBoxParam.WidgetType = cPDFWidget.WidgetType;
                comboBoxParam.BorderStyle = cPDFWidget.GetWidgetBorderStyle();

                byte[] LineColor = new byte[3];
                comboBoxParam.HasLineColor = cPDFWidget.GetWidgetBorderRGBColor(ref LineColor);
                comboBoxParam.LineColor = LineColor;

                byte[] BgColor = new byte[3];
                bool hasBgColor = cPDFWidget.GetWidgetBgRGBColor(ref BgColor);
                comboBoxParam.HasBgColor = hasBgColor;
                comboBoxParam.BgColor = BgColor;

                comboBoxParam.FontColor = cTextAttribute.FontColor;
                comboBoxParam.FontName = cTextAttribute.FontName;
                comboBoxParam.FontSize = cTextAttribute.FontSize;

                comboBoxParam.LineWidth = cPDFWidget.GetBorderWidth();
                comboBoxParam.FieldName = cPDFWidget.GetFieldName();
                comboBoxParam.Flags = cPDFWidget.GetFlags();
                comboBoxParam.IsReadOnly = cPDFWidget.GetIsReadOnly();
                comboBoxParam.IsHidden = cPDFWidget.GetIsHidden();

                GetAnnotCommonParam(cPDFWidget, comboBoxParam);

            }
            return comboBoxParam;
        }

        internal static ListBoxParam GetListBoxParam(CPDFListBoxWidget cPDFWidget)
        {
            ListBoxParam listBoxParam = null;
            if (cPDFWidget != null && cPDFWidget.IsValid())
            {
                listBoxParam = new ListBoxParam();
                CTextAttribute cTextAttribute = cPDFWidget.GetTextAttribute();
                listBoxParam.IsItalic = IsItalic(cTextAttribute.FontName);
                listBoxParam.IsBold = IsBold(cTextAttribute.FontName);
                CWidgetItem[] cWidgetItem = cPDFWidget.LoadWidgetItems();

                //Support for multiple selections needs to be adjusted later.
                CWidgetItem cWidgetItem1 = cPDFWidget.GetSelectedItem();
                if (cWidgetItem != null)
                {
                    for (int i = 0; i < cWidgetItem.Length; i++)
                    {
                        CWidgetItem item = cWidgetItem[i];
                        listBoxParam.OptionItems.Add(item.Value, item.Text);
                        if (cWidgetItem1 != null && cWidgetItem1.Text == item.Text && cWidgetItem1.Value == item.Value)
                        {
                            listBoxParam.SelectItemsIndex = new List<int> { i };
                        }
                    }
                }
                listBoxParam.WidgetType = cPDFWidget.WidgetType;
                listBoxParam.BorderStyle = cPDFWidget.GetWidgetBorderStyle();

                byte[] LineColor = new byte[3];
                listBoxParam.HasLineColor = cPDFWidget.GetWidgetBorderRGBColor(ref LineColor);
                listBoxParam.LineColor = LineColor;

                byte[] BgColor = new byte[3];
                bool hasBgColor = cPDFWidget.GetWidgetBgRGBColor(ref BgColor);
                listBoxParam.HasBgColor = hasBgColor;
                listBoxParam.BgColor = BgColor;

                listBoxParam.FontColor = cTextAttribute.FontColor;
                listBoxParam.FontName = cTextAttribute.FontName;
                listBoxParam.FontSize = cTextAttribute.FontSize;

                listBoxParam.LineWidth = cPDFWidget.GetBorderWidth();
                listBoxParam.FieldName = cPDFWidget.GetFieldName();
                listBoxParam.Flags = cPDFWidget.GetFlags();
                listBoxParam.IsReadOnly = cPDFWidget.GetIsReadOnly();
                listBoxParam.IsHidden = cPDFWidget.GetIsHidden();

                GetAnnotCommonParam(cPDFWidget, listBoxParam);

            }
            return listBoxParam;
        }

        internal static SignatureParam GetSignatureParam(CPDFSignatureWidget cPDFWidget)
        {
            SignatureParam signatureParam = null;
            if (cPDFWidget != null && cPDFWidget.IsValid())
            {
                signatureParam = new SignatureParam();

                signatureParam.WidgetType = cPDFWidget.WidgetType;
                signatureParam.BorderStyle = cPDFWidget.GetWidgetBorderStyle();

                byte[] LineColor = new byte[3];
                signatureParam.HasLineColor = cPDFWidget.GetWidgetBorderRGBColor(ref LineColor);
                signatureParam.LineColor = LineColor;

                byte[] BgColor = new byte[3];
                bool hasBgColor = cPDFWidget.GetWidgetBgRGBColor(ref BgColor);
                signatureParam.HasBgColor = hasBgColor;
                signatureParam.BgColor = BgColor;

                CTextAttribute cTextAttribute = cPDFWidget.GetTextAttribute();
                signatureParam.FontColor = cTextAttribute.FontColor;
                signatureParam.FontName = cTextAttribute.FontName;
                signatureParam.FontSize = cTextAttribute.FontSize;

                signatureParam.LineWidth = cPDFWidget.GetBorderWidth();
                signatureParam.FieldName = cPDFWidget.GetFieldName();
                signatureParam.Flags = cPDFWidget.GetFlags();
                signatureParam.IsReadOnly = cPDFWidget.GetIsReadOnly();
                signatureParam.IsHidden = cPDFWidget.GetIsHidden();

                GetAnnotCommonParam(cPDFWidget, signatureParam);

            }
            return signatureParam;
        }

        #endregion

        #region Annot

        internal static AnnotParam MeasureAnnotConverter(CPDFAnnotation pdfAnnot)
        {
            if (pdfAnnot == null || pdfAnnot.IsValid() == false)
            {
                return null;
            }

            switch (pdfAnnot.Type)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                {
                    CPDFLineAnnotation lineAnnot = pdfAnnot as CPDFLineAnnotation;
                    if (lineAnnot != null)
                    {
                        return GetLineMeasureParam(lineAnnot);
                    }
                }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                {
                    CPDFPolylineAnnotation polylineAnnot = pdfAnnot as CPDFPolylineAnnotation;
                    if (polylineAnnot != null)
                    {
                        return GetPolylineParam(polylineAnnot);
                    }
                }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                {
                    CPDFPolygonAnnotation polygonAnnot = pdfAnnot as CPDFPolygonAnnotation;
                    if (polygonAnnot != null)
                    {
                        return GetPolygonParam(polygonAnnot);
                    }
                }
                    break;
            }
            return null;
        }

        internal static AnnotParam AnnotConverter(CPDFDocument pdfDoc, CPDFAnnotation pdfAnnot)
        {
            if (pdfAnnot == null || pdfAnnot.IsValid() == false)
            {
                return null;
            }
            switch (pdfAnnot.Type)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                    {
                        CPDFSquareAnnotation squareAnnot = pdfAnnot as CPDFSquareAnnotation;
                        if (squareAnnot != null)
                        {
                            return GetSquareParam(squareAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                    {
                        CPDFCircleAnnotation circleAnnot = pdfAnnot as CPDFCircleAnnotation;
                        if (circleAnnot != null)
                        {
                            return GetCircleParam(circleAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    {
                        CPDFLineAnnotation lineAnnot = pdfAnnot as CPDFLineAnnotation;
                        if (lineAnnot != null)
                        {
                            if(lineAnnot.IsMeasured())
                            {
                                return GetLineMeasureParam(lineAnnot);
                            }

                            return GetLineParam(lineAnnot); 
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                    {
                        CPDFInkAnnotation inkAnnot = pdfAnnot as CPDFInkAnnotation;
                        if (inkAnnot != null)
                        {
                            return GetInkParam(inkAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                    {
                        CPDFFreeTextAnnotation freetextAnnot = pdfAnnot as CPDFFreeTextAnnotation;
                        if (freetextAnnot != null)
                        {
                            return GetFreeTextParam(freetextAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                    {
                        CPDFHighlightAnnotation highlightAnnot = pdfAnnot as CPDFHighlightAnnotation;
                        if (highlightAnnot != null)
                        {
                            return GetHighlightParam(highlightAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                    {
                        CPDFUnderlineAnnotation underlineAnnot = pdfAnnot as CPDFUnderlineAnnotation;
                        if (underlineAnnot != null)
                        {
                            return GetUnderlineParam(underlineAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                    {
                        CPDFStrikeoutAnnotation strikeoutAnnot = pdfAnnot as CPDFStrikeoutAnnotation;
                        if (strikeoutAnnot != null)
                        {
                            return GetStrikeoutParam(strikeoutAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                    {
                        CPDFSquigglyAnnotation squigglyAnnot = pdfAnnot as CPDFSquigglyAnnotation;
                        if (squigglyAnnot != null)
                        {
                            return GetSquigglyParam(squigglyAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                    {
                        CPDFTextAnnotation stickyAnnot = pdfAnnot as CPDFTextAnnotation;
                        if (stickyAnnot != null)
                        {
                            return GetStickynoteParam(stickyAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                    {
                        CPDFStampAnnotation stampAnnot = pdfAnnot as CPDFStampAnnotation;
                        if (stampAnnot != null)
                        {
                            return GetStampParam(stampAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                    {
                        CPDFLinkAnnotation linkAnnot = pdfAnnot as CPDFLinkAnnotation;
                        if (linkAnnot != null)
                        {
                            return GetLinkParam(linkAnnot, pdfDoc);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                    {
                        CPDFSoundAnnotation soundAnnot = pdfAnnot as CPDFSoundAnnotation;
                        if (soundAnnot != null)
                        {
                            return GetSoundParam(soundAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                    {
                        CPDFRedactAnnotation redactAnnot= pdfAnnot as CPDFRedactAnnotation;
                        if(redactAnnot != null)
                        {
                            return GetRedactParam(redactAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    {
                        CPDFPolylineAnnotation polylineAnnot= pdfAnnot as CPDFPolylineAnnotation;
                        if (polylineAnnot!=null && polylineAnnot.IsMeasured())
                        {
                            return GetPolyLineMeasureParam(polylineAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    {
                        CPDFPolygonAnnotation polygonAnnot= pdfAnnot as CPDFPolygonAnnotation;
                        if(polygonAnnot!=null)
                        {
                            return GetPolygonMeasureParam(polygonAnnot);
                        }
                    }
                    break;
                default:
                    break;

            }
            return null;
        }

        internal static void GetAnnotCommonParam(CPDFAnnotation pdfAnnot, AnnotParam annotParam)
        {
            if (pdfAnnot == null || annotParam == null || !pdfAnnot.IsValid())
            {
                return;
            }
            annotParam.CurrentType = pdfAnnot.Type;
            annotParam.Author = pdfAnnot.GetAuthor();
            annotParam.Transparency = pdfAnnot.GetTransparency();
            annotParam.Content = pdfAnnot.GetContent();
            annotParam.UpdateTime = pdfAnnot.GetModifyDate();
            annotParam.CreateTime = pdfAnnot.GetCreationDate();
            annotParam.Locked = pdfAnnot.GetIsLocked();
            annotParam.ClientRect = pdfAnnot.GetRect();
            annotParam.AnnotIndex = pdfAnnot.Page.GetAnnotations().IndexOf(pdfAnnot);

            //Annotation object exists, but the list cannot be found, it can only be a newly created annotation object.
            if (annotParam.AnnotIndex == -1)
            {
                annotParam.AnnotIndex = pdfAnnot.Page.GetAnnotCount() - 1;
            }

            annotParam.PageIndex = pdfAnnot.Page.PageIndex;
        }

        internal static SquareParam GetSquareParam(CPDFSquareAnnotation squareAnnot)
        {
            if (squareAnnot == null || squareAnnot.IsValid() == false)
            {
                return null;
            }

            SquareParam squareParam = new SquareParam();

            if (squareAnnot.LineColor != null && squareAnnot.LineColor.Length == 3)
            {
                squareParam.LineColor = new byte[3]
                {
                    squareAnnot.LineColor[0],
                    squareAnnot.LineColor[1],
                    squareAnnot.LineColor[2]
                };
            }

            if (squareAnnot.HasBgColor && squareAnnot.BgColor != null)
            {
                if (squareAnnot.BgColor.Length == 3)
                {
                    squareParam.HasBgColor = true;
                    squareParam.BgColor = new byte[3]
                    {
                        squareAnnot.BgColor[0],
                        squareAnnot.BgColor[1],
                        squareAnnot.BgColor[2]
                    };
                }
            }

            squareParam.LineWidth = squareAnnot.LineWidth;
            squareParam.BorderStyle = squareAnnot.BorderStyle;

            if (squareAnnot.Dash != null && squareAnnot.Dash.Length > 0)
            {
                squareParam.LineDash = new float[squareAnnot.Dash.Length];
                squareAnnot.Dash.CopyTo(squareParam.LineDash, 0);
            }

            GetAnnotCommonParam(squareAnnot, squareParam);
            return squareParam;
        }

        internal static CircleParam GetCircleParam(CPDFCircleAnnotation circleAnnot)
        {
            if (circleAnnot == null || circleAnnot.IsValid() == false)
            {
                return null;
            }

            CircleParam circleParam = new CircleParam();

            if (circleAnnot.LineColor != null && circleAnnot.LineColor.Length == 3)
            {
                circleParam.LineColor = new byte[3]
                {
                    circleAnnot.LineColor[0],
                    circleAnnot.LineColor[1],
                    circleAnnot.LineColor[2]
                };
            }

            if (circleAnnot.HasBgColor && circleAnnot.BgColor != null)
            {
                if (circleAnnot.BgColor.Length == 3)
                {
                    circleParam.HasBgColor = true;
                    circleParam.BgColor = new byte[3]
                    {
                        circleAnnot.BgColor[0],
                        circleAnnot.BgColor[1],
                        circleAnnot.BgColor[2]
                    };
                }
            }

            circleParam.LineWidth = circleAnnot.LineWidth;
            circleParam.BorderStyle = circleAnnot.BorderStyle;

            if (circleAnnot.Dash != null && circleAnnot.Dash.Length > 0)
            {
                circleParam.LineDash = new float[circleAnnot.Dash.Length];
                circleAnnot.Dash.CopyTo(circleParam.LineDash, 0);
            }

            GetAnnotCommonParam(circleAnnot, circleParam);
            return circleParam;
        }

        private static AnnotParam GetPolylineParam(CPDFPolylineAnnotation polylineAnnot)
        {
            if (polylineAnnot == null || polylineAnnot.IsValid() == false)
            {
                return null;
            }

            PolyLineMeasureParam polylineParam = new PolyLineMeasureParam();

            if (polylineAnnot.LineColor != null && polylineAnnot.LineColor.Length == 3)
            {
                polylineParam.LineColor = new byte[3]
                {
                    polylineAnnot.LineColor[0],
                    polylineAnnot.LineColor[1],
                    polylineAnnot.LineColor[2]
                };
            }

            polylineParam.LineWidth = polylineAnnot.LineWidth;
            polylineParam.BorderStyle = polylineAnnot.BorderStyle;

            if (polylineAnnot.Dash != null && polylineAnnot.Dash.Length > 0)
            {
                polylineParam.LineDash = new float[polylineAnnot.Dash.Length];
                polylineAnnot.Dash.CopyTo(polylineParam.LineDash, 0);
            }
            

            if (polylineAnnot.Points != null && polylineAnnot.Points.Count > 0)
            {
                polylineParam.SavePoints = new List<CPoint>();
                foreach (CPoint point in polylineAnnot.Points)
                {
                    polylineParam.SavePoints.Add(point);
                }
            }
            
            CTextAttribute cTextAttribute = polylineAnnot.GetTextAttribute();
            polylineParam.FontName = cTextAttribute.FontName;
            polylineParam.FontSize = cTextAttribute.FontSize;
            polylineParam.FontColor = cTextAttribute.FontColor;

            GetAnnotCommonParam(polylineAnnot, polylineParam);
            return polylineParam;
        }
        
        private static AnnotParam GetPolygonParam(CPDFPolygonAnnotation polygonAnnot)
        {
            if (polygonAnnot == null || polygonAnnot.IsValid() == false)
            {
                return null;
            }

            PolygonMeasureParam polygonParam = new PolygonMeasureParam();
            if (polygonAnnot.LineColor != null && polygonAnnot.LineColor.Length == 3)
            {
                polygonParam.LineColor = new byte[3]
                {
                    polygonAnnot.LineColor[0],
                    polygonAnnot.LineColor[1],
                    polygonAnnot.LineColor[2]
                };
            }
            
            if(polygonAnnot.HasBgColor && polygonAnnot.BgColor != null)
            {
                polygonParam.HasFillColor = true;
                polygonParam.FillColor = new byte[3]
                {
                    polygonAnnot.BgColor[0],
                    polygonAnnot.BgColor[1],
                    polygonAnnot.BgColor[2]
                };
            }

            polygonParam.LineWidth = polygonAnnot.LineWidth;
            polygonParam.BorderStyle = polygonAnnot.BorderStyle;

            if (polygonAnnot.Dash != null && polygonAnnot.Dash.Length > 0)
            {
                polygonParam.LineDash = new float[polygonAnnot.Dash.Length];
                polygonAnnot.Dash.CopyTo(polygonParam.LineDash, 0);
            }
            
            if (polygonAnnot.Points != null && polygonAnnot.Points.Count > 0)
            {
                polygonParam.SavePoints = new List<CPoint>();
                foreach (CPoint point in polygonAnnot.Points)
                {
                    polygonParam.SavePoints.Add(point);
                }
            }
            
            CTextAttribute cTextAttribute = polygonAnnot.GetTextAttribute();
            polygonParam.FontName = cTextAttribute.FontName;
            polygonParam.FontSize = cTextAttribute.FontSize;
            polygonParam.FontColor = cTextAttribute.FontColor;

            GetAnnotCommonParam(polygonAnnot, polygonParam);
            return polygonParam;
        }
        
        internal static LineParam GetLineParam(CPDFLineAnnotation lineAnnot)
        {
            if (lineAnnot == null || lineAnnot.IsValid() == false)
            {
                return null;
            }

            LineParam lineParam = new LineParam();

            if (lineAnnot.LineColor != null && lineAnnot.LineColor.Length == 3)
            {
                lineParam.LineColor = new byte[3]
                {
                    lineAnnot.LineColor[0],
                    lineAnnot.LineColor[1],
                    lineAnnot.LineColor[2]
                };
            }

            if (lineAnnot.HasBgColor && lineAnnot.BgColor != null)
            {
                if (lineAnnot.BgColor.Length == 3)
                {
                    lineParam.HasBgColor = true;
                    lineParam.BgColor = new byte[3]
                    {
                        lineAnnot.BgColor[0],
                        lineAnnot.BgColor[1],
                        lineAnnot.BgColor[2]
                    };
                }
            }

            lineParam.LineWidth = lineAnnot.LineWidth;
            lineParam.BorderStyle = lineAnnot.BorderStyle;

            if (lineAnnot.Dash != null && lineAnnot.Dash.Length > 0)
            {
                lineParam.LineDash = new float[lineAnnot.Dash.Length];
                lineAnnot.Dash.CopyTo(lineParam.LineDash, 0);
            }

            lineParam.HeadLineType = lineAnnot.HeadLineType;
            lineParam.TailLineType = lineAnnot.TailLineType;
            if (lineAnnot.Points != null && lineAnnot.Points.Length == 2)
            {
                lineParam.HeadPoint = lineAnnot.Points[0];
                lineParam.TailPoint = lineAnnot.Points[1];
            }

            GetAnnotCommonParam(lineAnnot, lineParam);

            return lineParam;
        }

        internal static LineMeasureParam GetLineMeasureParam(CPDFLineAnnotation lineAnnot)
        {
            if (lineAnnot == null || lineAnnot.IsValid() == false || lineAnnot.IsMeasured()==false)
            {
                return null;
            }

            LineMeasureParam measureParam = new LineMeasureParam();

            CPDFDistanceMeasure distanceMeasure = lineAnnot.GetDistanceMeasure();
            measureParam.measureInfo = distanceMeasure.MeasureInfo;

            if (lineAnnot.LineColor != null && lineAnnot.LineColor.Length == 3)
            {
                measureParam.LineColor = new byte[3] { lineAnnot.LineColor[0], lineAnnot.LineColor[1], lineAnnot.LineColor[2] };
            }

            measureParam.BorderStyle = lineAnnot.BorderStyle;
            measureParam.LineWidth = lineAnnot.LineWidth;
            measureParam.Transparency = lineAnnot.Transparency;
            measureParam.LineDash = lineAnnot.Dash;
            CTextAttribute textAttr = lineAnnot.GetTextAttribute();
            measureParam.FontName = textAttr.FontName;
            measureParam.FontSize = textAttr.FontSize;
            if (textAttr.FontColor != null && textAttr.FontColor.Length == 3)
            {
                measureParam.FontColor = new byte[] { textAttr.FontColor[0], textAttr.FontColor[1], textAttr.FontColor[2] };
            }
            measureParam.IsBold = CFontNameHelper.IsBold(textAttr.FontName);
            measureParam.IsItalic = CFontNameHelper.IsItalic(textAttr.FontName);
            measureParam.HeadLineType=lineAnnot.HeadLineType;
            measureParam.TailLineType=lineAnnot.TailLineType;
            measureParam.HeadPoint = lineAnnot.Points[0];
            measureParam.TailPoint = lineAnnot.Points[1];
            GetAnnotCommonParam(lineAnnot, measureParam);

            return measureParam;
        }

        internal static InkParam GetInkParam(CPDFInkAnnotation inkAnnot)
        {
            if (inkAnnot == null || inkAnnot.IsValid() == false)
            {
                return null;
            }

            InkParam inkParam = new InkParam();

            inkParam.Thickness = inkAnnot.Thickness;

            if (inkAnnot.InkColor != null && inkAnnot.InkColor.Length == 3)
            {
                inkParam.InkColor = new byte[3]
                {
                    inkAnnot.InkColor[0],
                    inkAnnot.InkColor[1],
                    inkAnnot.InkColor[2]
                };
            }

            if (inkAnnot.InkPath != null && inkAnnot.InkPath.Count > 0)
            {
                List<List<CPoint>> inkPath = new List<List<CPoint>>();

                foreach (List<CPoint> copyList in inkAnnot.InkPath)
                {
                    if (copyList.Count == 0)
                    {
                        continue;
                    }

                    List<CPoint> saveList = new List<CPoint>();
                    foreach (CPoint item in copyList)
                    {
                        saveList.Add(item);
                    }
                    if (saveList.Count > 0)
                    {
                        inkPath.Add(saveList);
                    }
                }
                if (inkPath.Count > 0)
                {
                    inkParam.InkPath = inkPath;
                }
            }

            if(inkAnnot.Dash!=null && inkAnnot.Dash.Length>0)
            {
                inkParam.Dash =new float[inkAnnot.Dash.Length];
                inkAnnot.Dash.CopyTo(inkParam.Dash, 0);
            }

            GetAnnotCommonParam(inkAnnot, inkParam);
            return inkParam;
        }

        internal static FreeTextParam GetFreeTextParam(CPDFFreeTextAnnotation freetextAnnot)
        {
            if (freetextAnnot == null || freetextAnnot.IsValid() == false)
            {
                return null;
            }

            FreeTextParam freetextParam = new FreeTextParam();

            if (freetextAnnot.LineColor != null && freetextAnnot.LineColor.Length == 3)
            {
                freetextParam.LineColor = new byte[3]
                {
                    freetextAnnot.LineColor[0],
                    freetextAnnot.LineColor[1],
                    freetextAnnot.LineColor[2]
                };
            }

            if (freetextAnnot.HasBgColor && freetextAnnot.BgColor != null)
            {
                if (freetextAnnot.BgColor.Length == 3)
                {
                    freetextParam.HasBgColor = true;
                    freetextParam.BgColor = new byte[3]
                    {
                        freetextAnnot.BgColor[0],
                        freetextAnnot.BgColor[1],
                        freetextAnnot.BgColor[2]
                    };
                }
            }

            freetextParam.LineWidth = freetextAnnot.LineWidth;

            if (freetextAnnot.FreeTextDa != null)
            {
                byte[] fontColor = freetextAnnot.FreeTextDa.FontColor;
                if (fontColor != null && fontColor.Length == 3)
                {
                    freetextParam.FontColor = new byte[3]
                    {
                        fontColor[0],
                        fontColor[1],
                        fontColor[2]
                    };
                }

                string fontName = freetextAnnot.FreeTextDa.FontName;
                if (!string.IsNullOrEmpty(fontName))
                {
                    freetextParam.FontName = fontName;
                    freetextParam.IsBold = IsBold(fontName);
                    freetextParam.IsItalic = IsItalic(fontName);
                }

                freetextParam.FontSize = freetextAnnot.FreeTextDa.FontSize;
                freetextParam.Alignment = freetextAnnot.Alignment;
            }

            if (freetextAnnot.Dash != null && freetextAnnot.Dash.Length > 0)
            {
                freetextParam.Dash = new float[freetextAnnot.Dash.Length];
                freetextAnnot.Dash.CopyTo(freetextParam.Dash, 0);
            }

            GetAnnotCommonParam(freetextAnnot, freetextParam);
            return freetextParam;
        }

        internal static HighlightParam GetHighlightParam(CPDFHighlightAnnotation highlightAnnot)
        {
            if (highlightAnnot == null || highlightAnnot.IsValid() == false)
            {
                return null;
            }

            HighlightParam highlightParam = new HighlightParam();

            if (highlightAnnot.Color != null && highlightAnnot.Color.Length == 3)
            {
                highlightParam.HighlightColor = new byte[3]
                {
                    highlightAnnot.Color[0],
                    highlightAnnot.Color[1],
                    highlightAnnot.Color[2]
                };
            }

            if (highlightAnnot.QuardRects != null && highlightAnnot.QuardRects.Count > 0)
            {
                List<CRect> saveList = new List<CRect>();
                foreach (CRect saveRect in highlightAnnot.QuardRects)
                {
                    saveList.Add(saveRect);
                }
                highlightParam.QuardRects = saveList;
            }

            GetAnnotCommonParam(highlightAnnot, highlightParam);

            return highlightParam;
        }

        internal static UnderlineParam GetUnderlineParam(CPDFUnderlineAnnotation underlineAnnot)
        {
            if (underlineAnnot == null || underlineAnnot.IsValid() == false)
            {
                return null;
            }

            UnderlineParam underlineParam = new UnderlineParam();

            if (underlineAnnot.Color != null && underlineAnnot.Color.Length == 3)
            {
                underlineParam.UnderlineColor = new byte[3]
                {
                    underlineAnnot.Color[0],
                    underlineAnnot.Color[1],
                    underlineAnnot.Color[2]
                };
            }

            if (underlineAnnot.QuardRects != null && underlineAnnot.QuardRects.Count > 0)
            {
                List<CRect> saveList = new List<CRect>();
                foreach (CRect saveRect in underlineAnnot.QuardRects)
                {
                    saveList.Add(saveRect);
                }
                underlineParam.QuardRects = saveList;
            }

            GetAnnotCommonParam(underlineAnnot, underlineParam);

            return underlineParam;
        }

        internal static StrikeoutParam GetStrikeoutParam(CPDFStrikeoutAnnotation strikeoutAnnot)
        {
            if (strikeoutAnnot == null || strikeoutAnnot.IsValid() == false)
            {
                return null;
            }

            StrikeoutParam strikeoutParam = new StrikeoutParam();

            if (strikeoutAnnot.Color != null && strikeoutAnnot.Color.Length == 3)
            {
                strikeoutParam.StrikeoutColor = new byte[3]
                {
                    strikeoutAnnot.Color[0],
                    strikeoutAnnot.Color[1],
                    strikeoutAnnot.Color[2]
                };
            }

            if (strikeoutAnnot.QuardRects != null && strikeoutAnnot.QuardRects.Count > 0)
            {
                List<CRect> saveList = new List<CRect>();
                foreach (CRect saveRect in strikeoutAnnot.QuardRects)
                {
                    saveList.Add(saveRect);
                }
                strikeoutParam.QuardRects = saveList;
            }

            GetAnnotCommonParam(strikeoutAnnot, strikeoutParam);

            return strikeoutParam;
        }

        internal static SquigglyParam GetSquigglyParam(CPDFSquigglyAnnotation squigglyAnnot)
        {
            if (squigglyAnnot == null || squigglyAnnot.IsValid() == false)
            {
                return null;
            }

            SquigglyParam squigglyParam = new SquigglyParam();

            if (squigglyAnnot.Color != null && squigglyAnnot.Color.Length == 3)
            {
                squigglyParam.SquigglyColor = new byte[3]
                {
                    squigglyAnnot.Color[0],
                    squigglyAnnot.Color[1],
                    squigglyAnnot.Color[2]
                };
            }

            if (squigglyAnnot.QuardRects != null && squigglyAnnot.QuardRects.Count > 0)
            {
                List<CRect> saveList = new List<CRect>();
                foreach (CRect saveRect in squigglyAnnot.QuardRects)
                {
                    saveList.Add(saveRect);
                }
                squigglyParam.QuardRects = saveList;
            }

            GetAnnotCommonParam(squigglyAnnot, squigglyParam);

            return squigglyParam;
        }

        internal static StickyNoteParam GetStickynoteParam(CPDFTextAnnotation stickyAnnot)
        {
            if (stickyAnnot == null || stickyAnnot.IsValid() == false)
            {
                return null;
            }

            StickyNoteParam stickyParam = new StickyNoteParam();

            if (stickyAnnot.Color != null && stickyAnnot.Color.Length == 3)
            {
                stickyParam.StickyNoteColor = new byte[3]
                {
                    stickyAnnot.Color[0],
                    stickyAnnot.Color[1],
                    stickyAnnot.Color[2]
                };
            }

            stickyParam.IconName=stickyAnnot.GetIconName();
            GetAnnotCommonParam(stickyAnnot, stickyParam);

            return stickyParam;
        }

        internal static StampParam GetStampParam(CPDFStampAnnotation stampAnnot)
        {
            if (stampAnnot == null || stampAnnot.IsValid() == false)
            {
                return null;
            }

            StampParam stampParam = new StampParam();
            C_STAMP_TYPE stampType = stampAnnot.GetStampType();
            switch (stampType)
            {
                case C_STAMP_TYPE.STANDARD_STAMP:
                    {
                        stampParam.StampText = stampAnnot.GetStandardStamp();
                        stampParam.StampType = stampType;
                    }
                    break;
                case C_STAMP_TYPE.TEXT_STAMP:
                    {
                        string stampText = string.Empty;
                        string dateText = string.Empty;
                        C_TEXTSTAMP_SHAPE stampShape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_NONE;
                        C_TEXTSTAMP_COLOR stampColor = C_TEXTSTAMP_COLOR.TEXTSTAMP_WHITE;
                        stampAnnot.GetTextStamp(ref stampText,
                            ref dateText,
                            ref stampShape,
                            ref stampColor);

                        stampParam.StampText = stampText;
                        stampParam.DateText = dateText;
                        stampParam.TextStampShape = stampShape;
                        stampParam.TextStampColor = stampColor;

                        stampParam.StampType = stampType;
                    }
                    break;
                case C_STAMP_TYPE.IMAGE_STAMP:
                case C_STAMP_TYPE.UNKNOWN_STAMP:
                    {
                        stampParam.StampType = stampType;
                        CRect rawRect = stampAnnot.GetRect();
                        int width = (int)(rawRect.width() / 72D * 96D);
                        int height = (int)(rawRect.height() / 72D * 96D);
                        if (width > 0 && height > 0)
                        {
                            Rect rotateRect = new Rect(0, 0, width, height);
                            Matrix rotateMatrix = new Matrix();
                            rotateMatrix.RotateAt(-90 * stampAnnot.Page.Rotation, width / 2, height / 2);
                            rotateRect.Transform(rotateMatrix);
                            int imageWidth = (int)rotateRect.Width;
                            int imageHeight = (int)rotateRect.Height;

                            byte[] ImageArray = new byte[imageWidth * imageHeight * 4];
                            stampAnnot.RenderAnnot(imageWidth, imageHeight, ImageArray);
                            WriteableBitmap writeBitmap = new WriteableBitmap(
                                imageWidth,
                                imageHeight,
                                96,
                                96,
                                PixelFormats.Bgra32,
                                null);

                            writeBitmap.WritePixels(new Int32Rect(0, 0, imageWidth, imageHeight), ImageArray, imageWidth * 4, 0);
                            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                            pngEncoder.Frames.Add(BitmapFrame.Create(writeBitmap));
                            MemoryStream memStream = new MemoryStream();
                            pngEncoder.Save(memStream);
                            stampParam.ImageStream = memStream;
                        }
                    }
                    break;
                default:
                    return null;
            }

            stampParam.PageRotation = stampAnnot.Page.Rotation;
            stampParam.Rotation = stampAnnot.AnnotationRotator.GetRotation();
            CRect sourceRect = new CRect();
            stampAnnot.GetSourceRect(ref sourceRect);
            stampParam.SourceRect = sourceRect;

            GetAnnotCommonParam(stampAnnot, stampParam);
            return stampParam;
        }

        internal static LinkParam GetLinkParam(CPDFLinkAnnotation linkAnnot, CPDFDocument pdfDoc)
        {
            if (linkAnnot == null || linkAnnot.IsValid() == false)
            {
                return null;
            }

            LinkParam linkParam = new LinkParam();
            CPDFAction linkAction = linkAnnot.GetLinkAction();
            if (linkAction != null)
            {
                switch (linkAction.ActionType)
                {
                    case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                        {
                            CPDFGoToAction gotoAction = linkAction as CPDFGoToAction;
                            if (gotoAction != null && pdfDoc != null && pdfDoc.IsValid())
                            {
                                CPDFDestination dest = gotoAction.GetDestination(pdfDoc);
                                if (dest != null)
                                {
                                    linkParam.Action = C_ACTION_TYPE.ACTION_TYPE_GOTO;
                                    linkParam.DestinationPageIndex = dest.PageIndex;
                                    linkParam.DestinationPosition = new CPoint(dest.Position_X, dest.Position_Y);
                                }
                            }
                        }
                        break;
                    case C_ACTION_TYPE.ACTION_TYPE_URI:
                        {
                            CPDFUriAction urlAction = linkAction as CPDFUriAction;
                            if (urlAction != null)
                            {
                                linkParam.Uri = urlAction.GetUri();
                                linkParam.Action = C_ACTION_TYPE.ACTION_TYPE_URI;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            GetAnnotCommonParam(linkAnnot, linkParam);

            return linkParam;
        }

        internal static SoundParam GetSoundParam(CPDFSoundAnnotation stampAnnot)
        {
            if (stampAnnot == null || stampAnnot.IsValid() == false)
            {
                return null;
            }

            SoundParam soundParam = new SoundParam();


            GetAnnotCommonParam(stampAnnot, soundParam);

            return soundParam;
        }

        internal static RedactParam GetRedactParam(CPDFRedactAnnotation redactAnnot)
        {
            if(redactAnnot == null || redactAnnot.IsValid() == false)
            {
                return null;
            }

            RedactParam redactParam = new RedactParam();
            if (redactAnnot.OutlineColor != null && redactAnnot.OutlineColor.Length == 3)
            {
                redactParam.LineColor = new byte[3] { redactAnnot.OutlineColor[0], redactAnnot.OutlineColor[1], redactAnnot.OutlineColor[2] };
            }

            if (redactAnnot.FillColor != null && redactAnnot.FillColor.Length == 3)
            {
                redactParam.BgColor = new byte[3] { redactAnnot.FillColor[0], redactAnnot.FillColor[1], redactAnnot.FillColor[2] };
            }

            if (redactAnnot.TextDa != null)
            {
                if (redactAnnot.TextDa.FontColor != null && redactAnnot.TextDa.FontColor.Length == 3)
                {
                    redactParam.FontColor = new byte[3] { redactAnnot.TextDa.FontColor[0], redactAnnot.TextDa.FontColor[1], redactAnnot.TextDa.FontColor[2] };
                }
                
                redactParam.FontName= redactAnnot.TextDa.FontName;
                redactParam.FontSize = redactAnnot.TextDa.FontSize;
                redactParam.Alignment=redactAnnot.TextAlignment;
            }

            if(redactAnnot.QuardRects!=null)
            {
                redactParam.QuardRects = new List<CRect>(redactAnnot.QuardRects);
            }

            redactParam.OverlayText = redactAnnot.OverlayText;

            GetAnnotCommonParam(redactAnnot, redactParam);

            return redactParam;
        }

        internal static PolyLineMeasureParam GetPolyLineMeasureParam(CPDFPolylineAnnotation polylineAnnot)
        {
            if (polylineAnnot == null || polylineAnnot.IsValid() == false || polylineAnnot.IsMeasured()==false)
            {
                return null;
            }

            PolyLineMeasureParam measureParam = new PolyLineMeasureParam();
            CPDFPerimeterMeasure perimeterMeasure = polylineAnnot.GetPerimeterMeasure();
            measureParam.measureInfo= perimeterMeasure.MeasureInfo;

            if (polylineAnnot.LineColor != null && polylineAnnot.LineColor.Length == 3)
            {
                measureParam.LineColor = new byte[3] { polylineAnnot.LineColor[0], polylineAnnot.LineColor[1], polylineAnnot.LineColor[2] };
            }
            measureParam.SavePoints = polylineAnnot.Points;
            measureParam.BorderStyle = polylineAnnot.BorderStyle;
            measureParam.LineWidth=polylineAnnot.LineWidth;
            measureParam.Transparency=polylineAnnot.Transparency;
            measureParam.LineDash=polylineAnnot.Dash;
            CTextAttribute textAttr = polylineAnnot.GetTextAttribute();
            measureParam.FontName = textAttr.FontName;
            measureParam.FontSize = textAttr.FontSize;
            if(textAttr.FontColor!=null && textAttr.FontColor.Length == 3)
            {
                measureParam.FontColor = new byte[] { textAttr.FontColor[0], textAttr.FontColor[1], textAttr.FontColor[2] };
            }
            measureParam.IsBold = CFontNameHelper.IsBold(textAttr.FontName);
            measureParam.IsItalic = CFontNameHelper.IsItalic(textAttr.FontName);

            GetAnnotCommonParam(polylineAnnot, measureParam);
            return measureParam;
        }

        internal static PolygonMeasureParam GetPolygonMeasureParam(CPDFPolygonAnnotation polygonAnnot)
        {
            if (polygonAnnot == null || polygonAnnot.IsValid() == false)
            {
                return null;
            }

            PolygonMeasureParam measureParam = new PolygonMeasureParam();
            if(polygonAnnot.IsMeasured())
            {
                CPDFAreaMeasure areaMeasure = polygonAnnot.GetAreaMeasure();
                measureParam.measureInfo = areaMeasure.MeasureInfo;
                CTextAttribute textAttr = polygonAnnot.GetTextAttribute();
                measureParam.FontName = textAttr.FontName;
                measureParam.FontSize = textAttr.FontSize;
                if (textAttr.FontColor != null && textAttr.FontColor.Length == 3)
                {
                    measureParam.FontColor = new byte[] { textAttr.FontColor[0], textAttr.FontColor[1], textAttr.FontColor[2] };
                }
                measureParam.IsBold = CFontNameHelper.IsBold(textAttr.FontName);
                measureParam.IsItalic = CFontNameHelper.IsItalic(textAttr.FontName);
            }

            if (polygonAnnot.LineColor != null && polygonAnnot.LineColor.Length == 3)
            {
                measureParam.LineColor = new byte[] { polygonAnnot.LineColor[0], polygonAnnot.LineColor[1], polygonAnnot.LineColor[2] };
            }

            if(polygonAnnot.HasBgColor && polygonAnnot.BgColor!=null && polygonAnnot.BgColor.Length == 3)
            {
                measureParam.HasFillColor = true;
                measureParam.FillColor = new byte[] { polygonAnnot.BgColor[0], polygonAnnot.BgColor[1], polygonAnnot.BgColor[2] };
            }

            measureParam.SavePoints = polygonAnnot.Points;
            measureParam.BorderStyle = polygonAnnot.BorderStyle;
            measureParam.LineWidth = polygonAnnot.LineWidth;
            measureParam.Transparency = polygonAnnot.Transparency;
            measureParam.LineDash = polygonAnnot.Dash;
            measureParam.BorderEffector = polygonAnnot.GetAnnotBorderEffector();

            GetAnnotCommonParam(polygonAnnot, measureParam);
            return measureParam;
        }

        #endregion

        public static bool SetParamForPDFAnnot(CPDFDocument cPDFDocument, CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            bool successful = false;
            if (cPDFAnnotation == null && !cPDFAnnotation.IsValid() && cPDFDocument == null && !cPDFDocument.IsValid())
            {
                return successful;
            }
            if (cPDFAnnotation.Type == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
            {
                successful = SetWidgetParamForPDFAnnot(cPDFDocument, cPDFAnnotation, param);
            }
            else
            {
                successful = SetAnnotParamForPDFAnnot(cPDFDocument, cPDFAnnotation, param);
            }
            return successful;
        }


        #region SetWidegt

        internal static bool SetWidgetParamForPDFAnnot(CPDFDocument cPDFDocument, CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            bool successful = false;
            if (cPDFAnnotation is CPDFWidget)
            {
                switch ((cPDFAnnotation as CPDFWidget).WidgetType)
                {
                    case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                        successful = SetPushButtonParamForPDFAnnot(cPDFDocument, cPDFAnnotation, param);
                        break;
                    case C_WIDGET_TYPE.WIDGET_CHECKBOX:
                        successful = SetCheckBoxParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                        successful = SetRadioButtonParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                        successful = SetTextBoxParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                        successful = SetComboBoxParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_WIDGET_TYPE.WIDGET_LISTBOX:
                        successful = SetListBoxParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                        successful = SetSignatureParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    default:
                        successful = false;
                        break;
                }
            }
            return successful;
        }
        internal static bool SetPushButtonParamForPDFAnnot(CPDFDocument cPDFDocument, CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            PushButtonParam CurrentParam = param as PushButtonParam;
            CPDFPushButtonWidget pushbuttonWidget = cPDFAnnotation as CPDFPushButtonWidget;
            bool successful = false;

            if (pushbuttonWidget == null && !pushbuttonWidget.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentParam.FieldName))
                {
                    pushbuttonWidget.SetFieldName(CurrentParam.FieldName);
                }

                if (CurrentParam.HasLineColor)
                {
                    if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                    {
                        pushbuttonWidget.SetWidgetBorderRGBColor(CurrentParam.LineColor);
                    }
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        pushbuttonWidget.SetWidgetBgRGBColor(CurrentParam.BgColor);
                    }
                }

                if (!string.IsNullOrEmpty(CurrentParam.Text))
                {
                    pushbuttonWidget.SetButtonTitle(CurrentParam.Text);
                }

                pushbuttonWidget.SetBorderWidth((float)CurrentParam.LineWidth);
                pushbuttonWidget.SetWidgetBorderStyle(CurrentParam.BorderStyle);

                CTextAttribute textAttr = new CTextAttribute();
                byte[] fontColor = new byte[3];
                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    fontColor = CurrentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
                textAttr.FontSize = (float)CurrentParam.FontSize;
                textAttr.FontName = ObtainFontName(
                    GetFontType(CurrentParam.FontName),
                    CurrentParam.IsBold,
                    CurrentParam.IsItalic);

                pushbuttonWidget.SetTextAttribute(textAttr);

                switch (CurrentParam.Action)
                {
                    case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                        {
                            CPDFGoToAction gotoAction = new CPDFGoToAction();
                            CPDFDestination destination = new CPDFDestination();
                            destination.Position_X = CurrentParam.DestinationPosition.x;
                            destination.Position_Y = CurrentParam.DestinationPosition.y;
                            destination.PageIndex = CurrentParam.DestinationPageIndex;
                            gotoAction.SetDestination(cPDFDocument, destination);
                            pushbuttonWidget.SetButtonAction(gotoAction);
                        }
                        break;
                    case C_ACTION_TYPE.ACTION_TYPE_URI:
                        {
                            CPDFUriAction uriAction = new CPDFUriAction();
                            uriAction.SetUri(CurrentParam.Uri);
                            pushbuttonWidget.SetButtonAction(uriAction);
                        }
                        break;
                    default:
                        break;
                }

                pushbuttonWidget.SetRect(CurrentParam.ClientRect);
                pushbuttonWidget.SetFlags(CurrentParam.Flags);
                pushbuttonWidget.SetIsLocked(CurrentParam.Locked);
                pushbuttonWidget.SetIsReadOnly(CurrentParam.IsReadOnly);
                pushbuttonWidget.SetIsHidden(CurrentParam.IsHidden);

                pushbuttonWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                pushbuttonWidget.UpdateFormAp();
                successful = true;
                return successful;
            }
        }
        internal static bool SetCheckBoxParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            CheckBoxParam CurrentParam = param as CheckBoxParam;
            CPDFCheckBoxWidget checkboxWidget = cPDFAnnotation as CPDFCheckBoxWidget;
            bool successful = false;

            if (checkboxWidget == null && !checkboxWidget.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentParam.FieldName))
                {
                    checkboxWidget.SetFieldName(CurrentParam.FieldName);
                }

                checkboxWidget.SetWidgetCheckStyle(CurrentParam.CheckStyle);
                checkboxWidget.SetChecked(CurrentParam.IsChecked);

                if (CurrentParam.HasLineColor)
                {
                    if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                    {
                        checkboxWidget.SetWidgetBorderRGBColor(CurrentParam.LineColor);
                    }
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        checkboxWidget.SetWidgetBgRGBColor(CurrentParam.BgColor);
                    }
                }

                checkboxWidget.SetBorderWidth((float)CurrentParam.LineWidth);
                checkboxWidget.SetWidgetBorderStyle(CurrentParam.BorderStyle);

                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    CTextAttribute textAttr = checkboxWidget.GetTextAttribute();
                    textAttr.FontColor = CurrentParam.FontColor;
                    checkboxWidget.SetTextAttribute(textAttr);
                }

                checkboxWidget.SetRect(CurrentParam.ClientRect);

                checkboxWidget.SetFlags(CurrentParam.Flags);
                checkboxWidget.SetIsLocked(CurrentParam.Locked);
                checkboxWidget.SetIsReadOnly(CurrentParam.IsReadOnly);
                checkboxWidget.SetIsHidden(CurrentParam.IsHidden);

                checkboxWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                checkboxWidget.UpdateFormAp();
                successful = true;

                return successful;
            }
        }
        internal static bool SetRadioButtonParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            RadioButtonParam CurrentParam = param as RadioButtonParam;
            CPDFRadioButtonWidget radioWidget = cPDFAnnotation as CPDFRadioButtonWidget;
            bool successful = false;

            if (radioWidget == null && !radioWidget.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentParam.FieldName))
                {
                    radioWidget.SetFieldName(CurrentParam.FieldName);
                }

                radioWidget.SetWidgetCheckStyle(CurrentParam.CheckStyle);
                radioWidget.SetChecked(CurrentParam.IsChecked);

                if (CurrentParam.HasLineColor)
                {
                    if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                    {
                        radioWidget.SetWidgetBorderRGBColor(CurrentParam.LineColor);
                    }
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        radioWidget.SetWidgetBgRGBColor(CurrentParam.BgColor);
                    }
                }

                radioWidget.SetBorderWidth((float)CurrentParam.LineWidth);
                radioWidget.SetWidgetBorderStyle(CurrentParam.BorderStyle);

                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    CTextAttribute textAttr = radioWidget.GetTextAttribute();
                    textAttr.FontColor = CurrentParam.FontColor;
                    radioWidget.SetTextAttribute(textAttr);
                }

                radioWidget.SetRect(CurrentParam.ClientRect);

                radioWidget.SetFlags(CurrentParam.Flags);
                radioWidget.SetIsLocked(CurrentParam.Locked);
                radioWidget.SetIsReadOnly(CurrentParam.IsReadOnly);
                radioWidget.SetIsHidden(CurrentParam.IsHidden);

                radioWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                radioWidget.UpdateFormAp();

                successful = true;

                return successful;
            }
        }
        internal static bool SetTextBoxParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            TextBoxParam CurrentParam = param as TextBoxParam;
            CPDFTextWidget textWidget = cPDFAnnotation as CPDFTextWidget;
            bool successful = false;

            if (textWidget == null && !textWidget.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentParam.FieldName))
                {
                    textWidget.SetFieldName(CurrentParam.FieldName);
                }

                if (CurrentParam.HasLineColor)
                {
                    if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                    {
                        textWidget.SetWidgetBorderRGBColor(CurrentParam.LineColor);
                    }
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        textWidget.SetWidgetBgRGBColor(CurrentParam.BgColor);
                    }
                }

                if (!string.IsNullOrEmpty(CurrentParam.Text))
                {
                    textWidget.SetText(CurrentParam.Text);
                }

                CTextAttribute textAttr = new CTextAttribute();
                byte[] fontColor = new byte[3];
                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    fontColor = CurrentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
                textAttr.FontSize = (float)CurrentParam.FontSize;
                textAttr.FontName = ObtainFontName(
                    GetFontType(CurrentParam.FontName),
                    CurrentParam.IsBold,
                    CurrentParam.IsItalic);

                textWidget.SetTextAttribute(textAttr);
                textWidget.SetJustification(CurrentParam.Alignment);

                textWidget.SetBorderWidth((float)CurrentParam.LineWidth);
                textWidget.SetWidgetBorderStyle(CurrentParam.BorderStyle);
                textWidget.SetMultiLine(CurrentParam.IsMultiLine);

                textWidget.SetRect(CurrentParam.ClientRect);

                textWidget.SetFlags(CurrentParam.Flags);
                textWidget.SetIsLocked(CurrentParam.Locked);
                textWidget.SetIsReadOnly(CurrentParam.IsReadOnly);
                textWidget.SetIsHidden(CurrentParam.IsHidden);

                textWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                textWidget.UpdateFormAp();

                successful = true;

                return successful;
            }
        }
        internal static bool SetComboBoxParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            ComboBoxParam CurrentParam = param as ComboBoxParam;
            CPDFComboBoxWidget comboboxWidget = cPDFAnnotation as CPDFComboBoxWidget;
            bool successful = false;

            if (comboboxWidget == null && !comboboxWidget.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentParam.FieldName))
                {
                    comboboxWidget.SetFieldName(CurrentParam.FieldName);
                }

                if (CurrentParam.HasLineColor)
                {
                    if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                    {
                        comboboxWidget.SetWidgetBorderRGBColor(CurrentParam.LineColor);
                    }
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        comboboxWidget.SetWidgetBgRGBColor(CurrentParam.BgColor);
                    }
                }

                comboboxWidget.SetBorderWidth((float)CurrentParam.LineWidth);
                comboboxWidget.SetWidgetBorderStyle(CurrentParam.BorderStyle);

                CTextAttribute textAttr = new CTextAttribute();
                byte[] fontColor = new byte[3];
                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    fontColor = CurrentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
                textAttr.FontSize = (float)CurrentParam.FontSize;
                textAttr.FontName = ObtainFontName(
                    GetFontType(CurrentParam.FontName),
                    CurrentParam.IsBold,
                    CurrentParam.IsItalic);

                comboboxWidget.SetTextAttribute(textAttr);

                if (CurrentParam.OptionItems != null && CurrentParam.OptionItems.Count > 0)
                {
                    int addIndex = 0;
                    foreach (string key in CurrentParam.OptionItems.Keys)
                    {
                        comboboxWidget.AddOptionItem(addIndex, CurrentParam.OptionItems[key], key);
                        addIndex++;
                    }
                }

                if (CurrentParam.SelectItemsIndex != null && CurrentParam.SelectItemsIndex.Count > 0)
                {
                    comboboxWidget.SelectItem(CurrentParam.SelectItemsIndex[0]);
                }

                comboboxWidget.SetRect(CurrentParam.ClientRect);

                comboboxWidget.SetFlags(CurrentParam.Flags);
                comboboxWidget.SetIsLocked(CurrentParam.Locked);
                comboboxWidget.SetIsReadOnly(CurrentParam.IsReadOnly);
                comboboxWidget.SetIsHidden(CurrentParam.IsHidden);

                comboboxWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                comboboxWidget.UpdateFormAp();
                successful = true;

                return successful;
            }
        }
        internal static bool SetListBoxParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            ListBoxParam CurrentParam = param as ListBoxParam;
            CPDFListBoxWidget listboxWidget = cPDFAnnotation as CPDFListBoxWidget;
            bool successful = false;

            if (listboxWidget == null && !listboxWidget.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentParam.FieldName))
                {
                    listboxWidget.SetFieldName(CurrentParam.FieldName);
                }

                if (CurrentParam.HasLineColor)
                {
                    if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                    {
                        listboxWidget.SetWidgetBorderRGBColor(CurrentParam.LineColor);
                    }
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        listboxWidget.SetWidgetBgRGBColor(CurrentParam.BgColor);
                    }
                }

                listboxWidget.SetBorderWidth((float)CurrentParam.LineWidth);
                listboxWidget.SetWidgetBorderStyle(CurrentParam.BorderStyle);

                CTextAttribute textAttr = new CTextAttribute();
                byte[] fontColor = new byte[3];
                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    fontColor = CurrentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
                textAttr.FontSize = (float)CurrentParam.FontSize;
                textAttr.FontName = ObtainFontName(
                    GetFontType(CurrentParam.FontName),
                    CurrentParam.IsBold,
                    CurrentParam.IsItalic);

                listboxWidget.SetTextAttribute(textAttr);

                if (CurrentParam.OptionItems != null && CurrentParam.OptionItems.Count > 0)
                {
                    int addIndex = 0;
                    foreach (string key in CurrentParam.OptionItems.Keys)
                    {
                        listboxWidget.AddOptionItem(addIndex, CurrentParam.OptionItems[key], key);
                        addIndex++;
                    }
                }

                if (CurrentParam.SelectItemsIndex != null && CurrentParam.SelectItemsIndex.Count > 0)
                {
                    listboxWidget.SelectItem(CurrentParam.SelectItemsIndex[0]);
                }

                listboxWidget.SetRect(CurrentParam.ClientRect);

                listboxWidget.SetFlags(CurrentParam.Flags);
                listboxWidget.SetIsLocked(CurrentParam.Locked);
                listboxWidget.SetIsReadOnly(CurrentParam.IsReadOnly);
                listboxWidget.SetIsHidden(CurrentParam.IsHidden);

                listboxWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                listboxWidget.UpdateFormAp();

                successful = true;

                return successful;
            }
        }
        internal static bool SetSignatureParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            SignatureParam CurrentParam = param as SignatureParam;
            CPDFSignatureWidget signWidget = cPDFAnnotation as CPDFSignatureWidget;
            bool successful = false;

            if (signWidget == null && !signWidget.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentParam.FieldName))
                {
                    signWidget.SetFieldName(CurrentParam.FieldName);
                }

                if (CurrentParam.HasLineColor)
                {
                    if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                    {
                        signWidget.SetWidgetBorderRGBColor(CurrentParam.LineColor);
                    }
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        signWidget.SetWidgetBgRGBColor(CurrentParam.BgColor);
                    }
                }

                signWidget.SetBorderWidth((float)CurrentParam.LineWidth);
                signWidget.SetWidgetBorderStyle(CurrentParam.BorderStyle);

                signWidget.SetRect(CurrentParam.ClientRect);

                signWidget.SetFlags(CurrentParam.Flags);
                signWidget.SetIsLocked(CurrentParam.Locked);
                signWidget.SetIsReadOnly(CurrentParam.IsReadOnly);
                signWidget.SetIsHidden(CurrentParam.IsHidden);

                signWidget.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                signWidget.UpdateFormAp();

                successful = true;

                return successful;
            }
        }

        #endregion

        #region SetAnnot

        internal static bool SetAnnotParamForPDFAnnot(CPDFDocument cPDFDocument, CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            bool successful = false;
            if (cPDFAnnotation != null)
            {
                switch (cPDFAnnotation.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                        successful = SetSquareAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                        successful = SetCircleAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                        successful = SetLineAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                        successful = SetInkAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                        successful = SetFreeTextAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                        successful = SetHighlightAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                        successful = SetUnderlineAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                        successful = SetStrikeoutAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                        successful = SetSquigglyAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                        successful = SetTextAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                        successful = SetStampAnnotParamForPDFAnnot(cPDFAnnotation, param);
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                        successful = SetLinkAnnotParamForPDFAnnot(cPDFDocument, cPDFAnnotation, param);
                        break;
                    default:
                        break;

                }
            }
            return successful;
        }

        internal static bool SetSquareAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            SquareParam CurrentParam = param as SquareParam;
            CPDFSquareAnnotation squareAnnot = cPDFAnnotation as CPDFSquareAnnotation;
            bool successful = false;

            if (squareAnnot == null && !squareAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                {
                    squareAnnot.SetLineColor(CurrentParam.LineColor);
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        squareAnnot.SetBgColor(CurrentParam.BgColor);
                    }
                }

                squareAnnot.SetTransparency((byte)CurrentParam.Transparency);
                squareAnnot.SetLineWidth((byte)CurrentParam.LineWidth);
                squareAnnot.SetRect(CurrentParam.ClientRect);

                List<float> floatArray = new List<float>();
                if (CurrentParam.LineDash != null)
                {
                    foreach (float num in CurrentParam.LineDash)
                    {
                        floatArray.Add(num);
                    }
                }
                squareAnnot.SetBorderStyle(CurrentParam.BorderStyle, floatArray.ToArray());

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    squareAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    squareAnnot.SetContent(CurrentParam.Content);
                }
                squareAnnot.SetIsLocked(CurrentParam.Locked);
                squareAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                squareAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetCircleAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            CircleParam CurrentParam = param as CircleParam;
            CPDFCircleAnnotation circleAnnot = cPDFAnnotation as CPDFCircleAnnotation;
            bool successful = false;

            if (circleAnnot == null && !circleAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                {
                    circleAnnot.SetLineColor(CurrentParam.LineColor);
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        circleAnnot.SetBgColor(CurrentParam.BgColor);
                    }
                }

                circleAnnot.SetTransparency((byte)CurrentParam.Transparency);
                circleAnnot.SetLineWidth((byte)CurrentParam.LineWidth);
                circleAnnot.SetRect(CurrentParam.ClientRect);

                List<float> floatArray = new List<float>();
                if (CurrentParam.LineDash != null)
                {
                    foreach (float num in CurrentParam.LineDash)
                    {
                        floatArray.Add(num);
                    }
                }
                circleAnnot.SetBorderStyle(CurrentParam.BorderStyle, floatArray.ToArray());

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    circleAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    circleAnnot.SetContent(CurrentParam.Content);
                }
                circleAnnot.SetIsLocked(CurrentParam.Locked);
                circleAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                circleAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetLineAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            LineParam CurrentParam = param as LineParam;
            CPDFLineAnnotation lineAnnot = cPDFAnnotation as CPDFLineAnnotation;
            bool successful = false;

            if (lineAnnot == null && !lineAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (CurrentParam.HeadLineType != C_LINE_TYPE.LINETYPE_NONE || CurrentParam.TailLineType != C_LINE_TYPE.LINETYPE_NONE)
                {
                    lineAnnot.SetLineType(CurrentParam.HeadLineType, CurrentParam.TailLineType);
                }

                if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                {
                    lineAnnot.SetLineColor(CurrentParam.LineColor);
                }

                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        lineAnnot.SetBgColor(CurrentParam.BgColor);
                    }
                }

                lineAnnot.SetTransparency((byte)CurrentParam.Transparency);
                lineAnnot.SetLineWidth((byte)CurrentParam.LineWidth);
                lineAnnot.SetLinePoints(CurrentParam.HeadPoint, CurrentParam.TailPoint);
                lineAnnot.SetRect(CurrentParam.ClientRect);

                List<float> floatArray = new List<float>();
                if (CurrentParam.LineDash != null)
                {
                    foreach (float num in CurrentParam.LineDash)
                    {
                        floatArray.Add(num);
                    }
                }
                lineAnnot.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, floatArray.ToArray());

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    lineAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    lineAnnot.SetContent(CurrentParam.Content);
                }
                lineAnnot.SetIsLocked(CurrentParam.Locked);
                lineAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                lineAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetInkAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            InkParam CurrentParam = param as InkParam;
            CPDFInkAnnotation inkAnnot = cPDFAnnotation as CPDFInkAnnotation;
            bool successful = false;

            if (inkAnnot == null && !inkAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (CurrentParam.InkColor != null && CurrentParam.InkColor.Length == 3)
                {
                    inkAnnot.SetInkColor(CurrentParam.InkColor);
                }

                inkAnnot.SetThickness((float)CurrentParam.Thickness);
                inkAnnot.SetInkPath(CurrentParam.InkPath);
                inkAnnot.SetTransparency((byte)CurrentParam.Transparency);
                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    inkAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    inkAnnot.SetContent(CurrentParam.Content);
                }

                inkAnnot.SetIsLocked(CurrentParam.Locked);
                inkAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                inkAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetHighlightAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            HighlightParam CurrentParam = param as HighlightParam;
            CPDFHighlightAnnotation highlightAnnot = cPDFAnnotation as CPDFHighlightAnnotation;
            bool successful = false;

            if (highlightAnnot == null && !highlightAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                highlightAnnot.SetTransparency((byte)CurrentParam.Transparency);

                if (CurrentParam.QuardRects != null)
                {
                    highlightAnnot.SetQuardRects(CurrentParam.QuardRects);
                }

                if (CurrentParam.HighlightColor != null && CurrentParam.HighlightColor.Length == 3)
                {
                    highlightAnnot.SetColor(CurrentParam.HighlightColor);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    highlightAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    highlightAnnot.SetContent(CurrentParam.Content);
                }
                highlightAnnot.SetRect(CurrentParam.ClientRect);
                highlightAnnot.SetIsLocked(CurrentParam.Locked);
                highlightAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                highlightAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetUnderlineAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            UnderlineParam CurrentParam = param as UnderlineParam;
            CPDFUnderlineAnnotation underlineAnnot = cPDFAnnotation as CPDFUnderlineAnnotation;
            bool successful = false;

            if (underlineAnnot == null && !underlineAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                underlineAnnot.SetTransparency((byte)CurrentParam.Transparency);
                underlineAnnot.SetRect(CurrentParam.ClientRect);

                if (CurrentParam.QuardRects != null)
                {
                    underlineAnnot.SetQuardRects(CurrentParam.QuardRects);
                }

                if (CurrentParam.UnderlineColor != null && CurrentParam.UnderlineColor.Length == 3)
                {
                    underlineAnnot.SetColor(CurrentParam.UnderlineColor);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    underlineAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    underlineAnnot.SetContent(CurrentParam.Content);
                }
                underlineAnnot.SetIsLocked(CurrentParam.Locked);
                underlineAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                underlineAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetStrikeoutAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            StrikeoutParam CurrentParam = param as StrikeoutParam;
            CPDFStrikeoutAnnotation strikeoutAnnot = cPDFAnnotation as CPDFStrikeoutAnnotation;
            bool successful = false;

            if (strikeoutAnnot == null && !strikeoutAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                strikeoutAnnot.SetTransparency((byte)CurrentParam.Transparency);
                strikeoutAnnot.SetRect(CurrentParam.ClientRect);

                if (CurrentParam.QuardRects != null)
                {
                    strikeoutAnnot.SetQuardRects(CurrentParam.QuardRects);
                }

                if (CurrentParam.StrikeoutColor != null && CurrentParam.StrikeoutColor.Length == 3)
                {
                    strikeoutAnnot.SetColor(CurrentParam.StrikeoutColor);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    strikeoutAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    strikeoutAnnot.SetContent(CurrentParam.Content);
                }
                strikeoutAnnot.SetIsLocked(CurrentParam.Locked);
                strikeoutAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                strikeoutAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetSquigglyAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            SquigglyParam CurrentParam = param as SquigglyParam;
            CPDFSquigglyAnnotation squigglyAnnot = cPDFAnnotation as CPDFSquigglyAnnotation;
            bool successful = false;

            if (squigglyAnnot == null && !squigglyAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                squigglyAnnot.SetTransparency((byte)CurrentParam.Transparency);
                squigglyAnnot.SetRect(CurrentParam.ClientRect);

                if (CurrentParam.QuardRects != null)
                {
                    squigglyAnnot.SetQuardRects(CurrentParam.QuardRects);
                }

                if (CurrentParam.SquigglyColor != null && CurrentParam.SquigglyColor.Length == 3)
                {
                    squigglyAnnot.SetColor(CurrentParam.SquigglyColor);
                }


                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    squigglyAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    squigglyAnnot.SetContent(CurrentParam.Content);
                }
                squigglyAnnot.SetIsLocked(CurrentParam.Locked);
                squigglyAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                squigglyAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetFreeTextAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            FreeTextParam CurrentParam = param as FreeTextParam;
            CPDFFreeTextAnnotation textAnnot = cPDFAnnotation as CPDFFreeTextAnnotation;
            bool successful = false;

            if (textAnnot == null && !textAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                if (CurrentParam.LineColor != null && CurrentParam.LineColor.Length == 3)
                {
                    textAnnot.SetLineColor(CurrentParam.LineColor);
                }


                if (CurrentParam.HasBgColor)
                {
                    if (CurrentParam.BgColor != null && CurrentParam.BgColor.Length == 3)
                    {
                        textAnnot.SetBgColor(CurrentParam.BgColor);
                    }
                }

                textAnnot.SetTransparency((byte)CurrentParam.Transparency);
                textAnnot.SetLineWidth((byte)CurrentParam.LineWidth);

                textAnnot.SetFreetextAlignment(CurrentParam.Alignment);

                CTextAttribute textAttr = new CTextAttribute();
                byte[] fontColor = new byte[3];

                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    fontColor = CurrentParam.FontColor;
                }
                textAttr.FontColor = fontColor;
                textAttr.FontSize = (float)CurrentParam.FontSize;
                textAttr.FontName = ObtainFontName(
                    GetFontType(CurrentParam.FontName),
                    CurrentParam.IsBold,
                    CurrentParam.IsItalic);

                textAnnot.SetFreetextDa(textAttr);

                textAnnot.SetRect(CurrentParam.ClientRect);

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    textAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    textAnnot.SetContent(CurrentParam.Content);
                }
                textAnnot.SetIsLocked(CurrentParam.Locked);
                textAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                textAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetStampAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            StampParam CurrentParam = param as StampParam;
            CPDFStampAnnotation stampAnnot = cPDFAnnotation as CPDFStampAnnotation;
            bool successful = false;

            if (stampAnnot == null && !stampAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                switch (CurrentParam.StampType)
                {
                    case C_STAMP_TYPE.STANDARD_STAMP:
                        {
                            string stampText = CurrentParam.StampText;
                            if (stampText == null)
                            {
                                stampText = string.Empty;
                            }
                            stampAnnot.SetStandardStamp(stampText, CurrentParam.PageRotation);
                            stampAnnot.SetRect(CurrentParam.ClientRect);
                        }
                        break;
                    case C_STAMP_TYPE.TEXT_STAMP:
                        {
                            string dateText = CurrentParam.DateText;
                            string stampText = CurrentParam.StampText;
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
                                CurrentParam.TextStampShape,
                                CurrentParam.TextStampColor,
                                CurrentParam.PageRotation);
                            stampAnnot.SetRect(CurrentParam.ClientRect);
                        }
                        break;
                    case C_STAMP_TYPE.IMAGE_STAMP:
                        {
                            byte[] imageData = null;
                            int imageWidth = 0;
                            int imageHeight = 0;
                            PDFHelp.ImageStreamToByte(CurrentParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);
                            if (imageData != null && imageWidth > 0 && imageHeight > 0)
                            {
                                stampAnnot.SetRect(CurrentParam.ClientRect);
                                stampAnnot.SetImageStamp(
                                    imageData,
                                    imageWidth,
                                    imageHeight,
                                    CurrentParam.PageRotation);
                            }
                        }
                        break;
                    default:
                        break;
                }

                stampAnnot.SetTransparency((byte)CurrentParam.Transparency);


                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    stampAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    stampAnnot.SetContent(CurrentParam.Content);
                }
                stampAnnot.SetIsLocked(CurrentParam.Locked);
                stampAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                stampAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetLinkAnnotParamForPDFAnnot(CPDFDocument cPDFDocument, CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            LinkParam CurrentParam = param as LinkParam;
            CPDFLinkAnnotation linkAnnot = cPDFAnnotation as CPDFLinkAnnotation;
            bool successful = false;

            if (linkAnnot == null && !linkAnnot.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                switch (CurrentParam.Action)
                {
                    case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                        {
                            CPDFGoToAction gotoAction = new CPDFGoToAction();
                            CPDFDestination destination = new CPDFDestination();
                            destination.Position_X = CurrentParam.DestinationPosition.x;
                            destination.Position_Y = CurrentParam.DestinationPosition.y;
                            destination.PageIndex = CurrentParam.DestinationPageIndex;
                            gotoAction.SetDestination(cPDFDocument, destination);
                            linkAnnot.SetLinkAction(gotoAction);
                        }
                        break;
                    case C_ACTION_TYPE.ACTION_TYPE_URI:
                        {
                            CPDFUriAction uriAction = new CPDFUriAction();
                            if (!string.IsNullOrEmpty(CurrentParam.Uri))
                            {
                                uriAction.SetUri(CurrentParam.Uri);
                            }
                            linkAnnot.SetLinkAction(uriAction);
                        }
                        break;
                    default:
                        break;
                }

                linkAnnot.SetRect(CurrentParam.ClientRect);

                if (!string.IsNullOrEmpty(CurrentParam.Author))
                {
                    linkAnnot.SetAuthor(CurrentParam.Author);
                }

                if (!string.IsNullOrEmpty(CurrentParam.Content))
                {
                    linkAnnot.SetContent(CurrentParam.Content);
                }

                linkAnnot.SetIsLocked(CurrentParam.Locked);
                linkAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                linkAnnot.UpdateAp();
                successful = true;

                return successful;
            }
        }

        internal static bool SetTextAnnotParamForPDFAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam param)
        {
            StickyNoteParam textAnnotParam = param as StickyNoteParam;
            CPDFTextAnnotation textAnnot = cPDFAnnotation as CPDFTextAnnotation;
            if (textAnnot == null && !textAnnot.IsValid() && textAnnotParam == null)
            {
                return false;
            }
            else
            {
                if (textAnnotParam.StickyNoteColor != null && textAnnotParam.StickyNoteColor.Length == 3)
                {
                    textAnnot.SetColor(textAnnotParam.StickyNoteColor);
                }

                textAnnot.SetTransparency((byte)textAnnotParam.Transparency);
                textAnnot.SetRect(textAnnotParam.ClientRect);
                if (!string.IsNullOrEmpty(textAnnotParam.Author))
                {
                    textAnnot.SetAuthor(textAnnotParam.Author);
                }

                if (!string.IsNullOrEmpty(textAnnotParam.Content))
                {
                    textAnnot.SetContent(textAnnotParam.Content);
                }

                textAnnot.SetIsLocked(textAnnotParam.Locked);
                textAnnot.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                textAnnot.UpdateAp();
                return true;
            }
        }

        #endregion

        public static bool SetParamForPDFEdit(CPDFEditArea cPDFEditArea, PDFEditParam param)
        {
            bool successful = false;
            if (cPDFEditArea == null && !cPDFEditArea.IsValid())
            {
                return successful;
            }
            switch (cPDFEditArea.Type)
            {
                case CPDFEditType.EditText:
                    SetParamForPDFTextEdit(cPDFEditArea, param);
                    break;
                default:
                    break;
            }
            return successful;
        }

        internal static bool SetParamForPDFTextEdit(CPDFEditArea cPDFEditArea, PDFEditParam param)
        {
            TextEditParam CurrentParam = param as TextEditParam;
            CPDFEditTextArea cPDFEditTextArea = cPDFEditArea as CPDFEditTextArea;
            bool successful = false;

            if (cPDFEditTextArea == null && !cPDFEditTextArea.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                cPDFEditTextArea.SetCharsFontTransparency(param.Transparency);
                cPDFEditTextArea.SetFrame(
                    DataConversionForWPF.RectConversionForCRect(
                    new Rect(CurrentParam.ClientRect.left, CurrentParam.ClientRect.top, CurrentParam.ClientRect.width(), CurrentParam.ClientRect.height())
                    ));
                cPDFEditTextArea.SetCharsFontSize((float)CurrentParam.FontSize, true);
                if (CurrentParam.FontColor != null && CurrentParam.FontColor.Length == 3)
                {
                    cPDFEditTextArea.SetCharsFontColor(CurrentParam.FontColor[0], CurrentParam.FontColor[1], CurrentParam.FontColor[2]);
                }
                cPDFEditTextArea.SetTextAreaAlign(CurrentParam.TextAlign);
                cPDFEditTextArea.SetCharsFontName(CurrentParam.FontName);
                cPDFEditTextArea.SetCharsFontItalic(CurrentParam.IsItalic);
                cPDFEditTextArea.SetCharsFontBold(CurrentParam.IsBold);
                successful = true;
                return successful;
            }
        }

        internal static bool SetParamForPDFImageEdit(CPDFEditArea cPDFEditArea, PDFEditParam param)
        {
            ImageEditParam CurrentParam = param as ImageEditParam;
            CPDFEditImageArea cPDFEditImageArea = cPDFEditArea as CPDFEditImageArea;
            bool successful = false;

            if (cPDFEditImageArea == null && !cPDFEditImageArea.IsValid() && CurrentParam == null)
            {
                return successful;
            }
            else
            {
                cPDFEditImageArea.SetImageTransparency(param.Transparency);
                cPDFEditImageArea.SetFrame(
                     DataConversionForWPF.RectConversionForCRect(
                         new Rect(CurrentParam.ClientRect.left, CurrentParam.ClientRect.top, CurrentParam.ClientRect.width(), CurrentParam.ClientRect.height())
                         ))
                     ;
                cPDFEditImageArea.CutWithRect(
                     DataConversionForWPF.RectConversionForCRect(
                    new Rect(CurrentParam.ClipRect.left, CurrentParam.ClipRect.top, CurrentParam.ClipRect.width(), CurrentParam.ClipRect.height())
                    ));
                successful = true;
                return successful;
            }
        }
    }
}
