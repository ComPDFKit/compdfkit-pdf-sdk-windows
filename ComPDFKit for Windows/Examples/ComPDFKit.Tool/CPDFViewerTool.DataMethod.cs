using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.PDFDocument;
using ComPDFKit.Tool.SettingParam;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using static ComPDFKit.PDFAnnotation.CTextAttribute;
using ComPDFKit.Tool.Help;
using static ComPDFKit.Tool.Help.ParamConverter;
using ComPDFKit.Import;
using ComPDFKit.Measure;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        public CPDFViewer GetCPDFViewer()
        {
            return PDFViewer;
        }

        #region DefaultAnnot

        public void CreateDefaultAnnot(CPDFAnnotation cPDFAnnotation, C_ANNOTATION_TYPE annotType, AnnotParam annotParam)
        {
            switch (annotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_NONE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNKOWN:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                    DefaultTextAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                    DefaultLinkAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                    DefaultFreeTextAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    if (annotParam != null)
                    {
                        if (annotParam is LineMeasureParam)
                        {
                            DefaultLineMeasureAnnot(cPDFAnnotation, annotParam);
                        }
                        else
                        {
                            DefaultLineAnnot(cPDFAnnotation, annotParam);
                        }
                    }
                    else
                    {
                        DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                        if (defaultSettingParam.IsOpenMeasure)
                        {
                            DefaultLineMeasureAnnot(cPDFAnnotation, annotParam);
                        }
                        else
                        {
                            DefaultLineAnnot(cPDFAnnotation, annotParam);
                        }

                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                    DefaultSquareAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                    DefaultCircleAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    DefaultPolygonMeasureAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    DefaultPolyLineMeasureAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                    DefaultHighlightAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                    DefaultUnderlineAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                    DefaultSquigglyAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                    DefaultStrikeoutAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                    DefaultStampAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CARET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                    DefaultInkAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POPUP:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_FILEATTACHMENT:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                    DefaultSoundAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
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
                    DefaultRedactAnnot(cPDFAnnotation, annotParam);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_INTERCHANGE:
                    break;
                default:
                    break;
            }
        }

        private void DefaultAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            if (cPDFAnnotation == null || annotParam == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(annotParam.Content))
            {
                cPDFAnnotation.SetContent(annotParam.Content);
            }

            if (!string.IsNullOrEmpty(annotParam.Author))
            {
                cPDFAnnotation.SetAuthor(annotParam.Author);
            }

            if (!string.IsNullOrEmpty(annotParam.CreateTime))
            {
                cPDFAnnotation.SetCreationDate(annotParam.CreateTime);
            }

            if (!string.IsNullOrEmpty(annotParam.UpdateTime))
            {
                cPDFAnnotation.SetModifyDate(annotParam.UpdateTime);
            }

            cPDFAnnotation.SetIsLocked(annotParam.Locked);
            cPDFAnnotation.SetTransparency(annotParam.Transparency);
        }

        private void DefaultTextAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFTextAnnotation TextAnnotation = (cPDFAnnotation as CPDFTextAnnotation);
            if (TextAnnotation == null)
            {
                return;
            }
            StickyNoteParam StickyNoteParamDef;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                StickyNoteParamDef = defaultSettingParam.StickyNoteParamDef;
            }
            else
            {
                StickyNoteParamDef = annotParam as StickyNoteParam;
            }

            TextAnnotation.SetColor(StickyNoteParamDef.StickyNoteColor);
            DefaultAnnot(cPDFAnnotation, StickyNoteParamDef);
        }

        private void DefaultLinkAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFLinkAnnotation linkAnnotation = (cPDFAnnotation as CPDFLinkAnnotation);
            if (linkAnnotation == null)
            {
                return;
            }
            LinkParam linkParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                linkParam = defaultSettingParam.LinkParamDef;
            }
            else
            {
                linkParam = annotParam as LinkParam;
            }
            switch (linkParam.Action)
            {
                case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                    CPDFGoToAction gotoAction = new CPDFGoToAction();
                    CPDFDestination destination = new CPDFDestination();
                    destination.Position_X = linkParam.DestinationPosition.x;
                    destination.Position_Y = linkParam.DestinationPosition.y;
                    destination.PageIndex = linkParam.DestinationPageIndex;
                    gotoAction.SetDestination(PDFViewer.GetDocument(), destination);
                    linkAnnotation.SetLinkAction(gotoAction);
                    break;
                case C_ACTION_TYPE.ACTION_TYPE_URI:
                    CPDFUriAction uriAction = new CPDFUriAction();
                    if (!string.IsNullOrEmpty(linkParam.Uri))
                    {
                        uriAction.SetUri(linkParam.Uri);
                    }
                    linkAnnotation.SetLinkAction(uriAction);
                    break;
                default:
                    break;
            }

            DefaultAnnot(cPDFAnnotation, linkParam);
        }

        private void DefaultFreeTextAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFFreeTextAnnotation freeTextAnnotation = (cPDFAnnotation as CPDFFreeTextAnnotation);
            if (freeTextAnnotation == null)
            {
                return;
            }
            FreeTextParam FreeTextParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                FreeTextParam = defaultSettingParam.FreeTextParamDef;
            }
            else
            {
                FreeTextParam = annotParam as FreeTextParam;
            }

            freeTextAnnotation.SetFreetextAlignment((C_TEXT_ALIGNMENT)(int)FreeTextParam.Alignment);
            if (FreeTextParam.LineColor != null)
            {
                freeTextAnnotation.SetLineColor(FreeTextParam.LineColor);
            }
            freeTextAnnotation.SetBorderWidth(1);
            freeTextAnnotation.SetTransparency(FreeTextParam.Transparency);
            freeTextAnnotation.SetLineWidth((float)FreeTextParam.LineWidth);

            if (FreeTextParam.HasBgColor && freeTextAnnotation.BgColor.Length == 3)
            {
                freeTextAnnotation.SetBgColor(FreeTextParam.BgColor);
            }
            CTextAttribute textAttr = new CTextAttribute();
            textAttr.FontColor = FreeTextParam.FontColor;
            textAttr.FontSize = (float)FreeTextParam.FontSize;
            textAttr.FontName = FreeTextParam.FontName;
            freeTextAnnotation.SetFreetextDa(textAttr);

            DefaultAnnot(cPDFAnnotation, FreeTextParam);
        }

        private void DefaultLineAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFLineAnnotation LineAnnotation = (cPDFAnnotation as CPDFLineAnnotation);
            if (LineAnnotation == null)
            {
                return;
            }
            LineParam lineParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                lineParam = defaultSettingParam.LineParamDef;
            }
            else
            {
                lineParam = annotParam as LineParam;
            }
            if (lineParam.LineColor != null)
            {
                LineAnnotation.SetLineColor(lineParam.LineColor);
            }
            if (lineParam.HasBgColor)
            {
                if (lineParam.BgColor != null)
                {
                    LineAnnotation.SetBgColor(lineParam.BgColor);
                }
            }
            else
            {
                LineAnnotation.ClearBgColor();
            }

            LineAnnotation.SetLineWidth((float)lineParam.LineWidth);

            LineAnnotation.SetTransparency(lineParam.Transparency);

            if (lineParam.LineDash != null)
            {
                LineAnnotation.SetBorderStyle(lineParam.BorderStyle, lineParam.LineDash);
            }

            LineAnnotation.SetLineType(lineParam.HeadLineType, lineParam.TailLineType);

            DefaultAnnot(cPDFAnnotation, lineParam);
        }

        private void DefaultSquareAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFSquareAnnotation SquareAnnotation = (cPDFAnnotation as CPDFSquareAnnotation);
            if (SquareAnnotation == null)
            {
                return;
            }

            SquareParam squareParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                squareParam = defaultSettingParam.SquareParamDef;
            }
            else
            {
                squareParam = annotParam as SquareParam;
            }
            if (squareParam.LineColor != null)
            {
                SquareAnnotation.SetLineColor(squareParam.LineColor);
            }
            if (squareParam.HasBgColor)
            {
                if (squareParam.BgColor != null)
                {
                    SquareAnnotation.SetBgColor(squareParam.BgColor);
                }
            }
            else
            {
                SquareAnnotation.ClearBgColor();
            }

            SquareAnnotation.SetLineWidth((float)squareParam.LineWidth);

            SquareAnnotation.SetTransparency(squareParam.Transparency);

            if (squareParam.LineDash != null)
            {
                SquareAnnotation.SetBorderStyle(squareParam.BorderStyle, squareParam.LineDash);
            }

            DefaultAnnot(cPDFAnnotation, squareParam);
        }

        private void DefaultCircleAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFCircleAnnotation circleAnnotation = (cPDFAnnotation as CPDFCircleAnnotation);
            if (circleAnnotation == null)
            {
                return;
            }
            CircleParam circleParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                circleParam = defaultSettingParam.CircleParamDef;
            }
            else
            {
                circleParam = annotParam as CircleParam;
            }

            if (circleParam.LineColor != null)
            {
                circleAnnotation.SetLineColor(circleParam.LineColor);
            }
            if (circleParam.HasBgColor)
            {
                if (circleParam.BgColor != null)
                {
                    circleAnnotation.SetBgColor(circleParam.BgColor);
                }
            }
            else
            {
                circleAnnotation.ClearBgColor();
            }

            circleAnnotation.SetLineWidth((float)circleParam.LineWidth);

            circleAnnotation.SetTransparency(circleParam.Transparency);

            if (circleParam.LineDash != null)
            {
                circleAnnotation.SetBorderStyle(circleParam.BorderStyle, circleParam.LineDash);
            }

            DefaultAnnot(cPDFAnnotation, circleParam);
        }

        private void DefaultHighlightAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFHighlightAnnotation highlightAnnotation = (cPDFAnnotation as CPDFHighlightAnnotation);
            if (highlightAnnotation == null)
            {
                return;
            }

            HighlightParam highlightParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                highlightParam = defaultSettingParam.HighlightParamDef;
            }
            else
            {
                highlightParam = annotParam as HighlightParam;
            }

            if (highlightParam.HighlightColor != null)
            {
                highlightAnnotation.SetColor(highlightParam.HighlightColor);
            }

            DefaultAnnot(cPDFAnnotation, highlightParam);
        }

        private void DefaultUnderlineAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFUnderlineAnnotation highlightAnnotation = (cPDFAnnotation as CPDFUnderlineAnnotation);
            if (highlightAnnotation == null)
            {
                return;
            }
            UnderlineParam underlineParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                underlineParam = defaultSettingParam.UnderlineParamDef;
            }
            else
            {
                underlineParam = annotParam as UnderlineParam;
            }

            if (underlineParam.UnderlineColor != null)
            {
                highlightAnnotation.SetColor(underlineParam.UnderlineColor);
            }

            DefaultAnnot(cPDFAnnotation, underlineParam);
        }

        private void DefaultSquigglyAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFSquigglyAnnotation squigglyAnnotation = (cPDFAnnotation as CPDFSquigglyAnnotation);
            if (squigglyAnnotation == null)
            {
                return;
            }
            SquigglyParam squigglyParam;
            if (annotParam == null)
            {

                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                squigglyParam = defaultSettingParam.SquigglyParamDef;
            }
            else
            {
                squigglyParam = annotParam as SquigglyParam;
            }

            if (squigglyParam.SquigglyColor != null)
            {
                squigglyAnnotation.SetColor(squigglyParam.SquigglyColor);
            }

            DefaultAnnot(cPDFAnnotation, squigglyParam);
        }

        private void DefaultStrikeoutAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFStrikeoutAnnotation strikeoutAnnotation = (cPDFAnnotation as CPDFStrikeoutAnnotation);
            if (strikeoutAnnotation == null)
            {
                return;
            }

            StrikeoutParam strikeoutParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                strikeoutParam = defaultSettingParam.StrikeoutParamDef;
            }
            else
            {
                strikeoutParam = annotParam as StrikeoutParam;
            }

            if (strikeoutParam.StrikeoutColor != null)
            {
                strikeoutAnnotation.SetColor(strikeoutParam.StrikeoutColor);
            }

            DefaultAnnot(cPDFAnnotation, strikeoutParam);
        }

        private void DefaultStampAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFStampAnnotation strikeoutAnnotation = (cPDFAnnotation as CPDFStampAnnotation);
            if (strikeoutAnnotation == null)
            {
                return;
            }

            StampParam stampParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                stampParam = defaultSettingParam.StampParamDef;
            }
            else
            {
                stampParam = annotParam as StampParam;
            }

            switch (stampParam.StampType)
            {
                case C_STAMP_TYPE.STANDARD_STAMP:
                    {
                        string stampText = stampParam.StampText;
                        if (stampText == null)
                        {
                            stampText = string.Empty;
                        }
                        strikeoutAnnotation.SetStandardStamp(stampText, stampParam.Rotation);
                    }
                    break;
                case C_STAMP_TYPE.IMAGE_STAMP:
                    {
                        byte[] imageData = null;
                        int imageWidth = 0;
                        int imageHeight = 0;
                        PDFHelp.ImageStreamToByte(stampParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);

                        if (imageData != null && imageWidth > 0 && imageHeight > 0)
                        {
                            strikeoutAnnotation.SetRect(new CRect(0, imageHeight, imageWidth, 0));
                            strikeoutAnnotation.SetImageStamp(
                                imageData,
                                imageWidth,
                                imageHeight,
                                stampParam.Rotation);
                        }
                    }
                    break;
                case C_STAMP_TYPE.TEXT_STAMP:
                    {
                        string dateText = stampParam.DateText;
                        string stampText = stampParam.StampText;
                        if (dateText == null)
                        {
                            dateText = string.Empty;
                        }
                        if (stampText == null)
                        {
                            stampText = string.Empty;
                        }
                        strikeoutAnnotation.SetTextStamp(
                            stampText,
                            dateText,
                            stampParam.TextStampShape,
                            stampParam.TextStampColor,
                            stampParam.Rotation);
                    }
                    break;
                default:
                    break;
            }

            DefaultAnnot(cPDFAnnotation, stampParam);
        }

        private void DefaultInkAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFInkAnnotation InkAnnotation = (cPDFAnnotation as CPDFInkAnnotation);
            if (InkAnnotation == null)
            {
                return;
            }

            InkParam inkParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                inkParam = defaultSettingParam.InkParamDef;
            }
            else
            {
                inkParam = annotParam as InkParam;
            }

            if (inkParam.InkColor != null)
            {
                InkAnnotation.SetInkColor(inkParam.InkColor);
            }

            InkAnnotation.SetThickness((float)inkParam.Thickness);
            (cPDFAnnotation as CPDFInkAnnotation).SetInkPath(inkParam.InkPath);
            DefaultAnnot(cPDFAnnotation, inkParam);
        }

        private void DefaultSoundAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFSoundAnnotation SoundAnnotation = (cPDFAnnotation as CPDFSoundAnnotation);
            if (SoundAnnotation == null)
            {
                return;
            }

            SoundParam soundParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                soundParam = defaultSettingParam.SoundParamDef;
            }
            else
            {
                soundParam = annotParam as SoundParam;
            }

            byte[] imageData = null;
            int imageWidth = 0;
            int imageHeight = 0;
            PDFHelp.ImageStreamToByte(soundParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);
            if (imageData != null && imageWidth > 0 && imageHeight > 0)
            {
                SoundAnnotation.SetRect(new Import.CRect(0, imageHeight, imageWidth, 0));
                SoundAnnotation.SetSoundPath(
                    imageData,
                    imageWidth,
                    imageHeight,
                   soundParam.SoundFilePath);
            }
            DefaultAnnot(cPDFAnnotation, soundParam);
        }

        private void DefaultRedactAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFRedactAnnotation redactAnnotation = (cPDFAnnotation as CPDFRedactAnnotation);
            if (redactAnnotation == null)
            {
                return;
            }
            RedactParam redactParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                redactParam = defaultSettingParam.RedactParamDef;
            }
            else
            {
                redactParam = annotParam as RedactParam;
            }

            if (redactParam.LineColor != null)
            {
                redactAnnotation.SetOutlineColor(redactParam.LineColor);
            }

            if (redactParam.BgColor != null)
            {
                redactAnnotation.SetFillColor(redactParam.BgColor);
            }

            redactAnnotation.SetTextAlignment(redactParam.Alignment);
            if (!string.IsNullOrEmpty(redactParam.OverlayText))
            {
                redactAnnotation.SetOverlayText(redactParam.OverlayText);
            }

            CTextAttribute textAttr = new CTextAttribute();
            byte[] fontColor = new byte[3];

            if (redactParam.FontColor != null && redactParam.FontColor.Length == 3)
            {
                fontColor = redactParam.FontColor;
            }
            textAttr.FontColor = fontColor;
            textAttr.FontSize = (float)redactParam.FontSize;
            textAttr.FontName = redactParam.FontName;
            redactAnnotation.SetTextAttribute(textAttr);

            DefaultAnnot(cPDFAnnotation, redactParam);
        }

        private void DefaultLineMeasureAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFLineAnnotation LineAnnotation = (cPDFAnnotation as CPDFLineAnnotation);
            if (LineAnnotation == null)
            {
                return;
            }
            if (!LineAnnotation.IsMeasured() && annotParam != null)
            {
                return;
            }
            LineMeasureParam lineMeasureParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                lineMeasureParam = defaultSettingParam.LineMeasureParamDef;
            }
            else
            {
                lineMeasureParam = annotParam as LineMeasureParam;
            }
            if (lineMeasureParam.LineColor != null)
            {
                LineAnnotation.SetLineColor(lineMeasureParam.LineColor);
            }
            LineAnnotation.SetLineWidth((float)lineMeasureParam.LineWidth);

            if (lineMeasureParam.LineDash != null)
            {
                if (lineMeasureParam.LineDash.Length == 0)
                {
                    LineAnnotation.SetBorderStyle(C_BORDER_STYLE.BS_SOLID, new float[0]);
                }
                else
                {
                    LineAnnotation.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, lineMeasureParam.LineDash);
                }
            }

            LineAnnotation.SetLineType(lineMeasureParam.HeadLineType, lineMeasureParam.TailLineType);

            CTextAttribute textAttribute = new CTextAttribute();
            textAttribute.FontColor = lineMeasureParam.FontColor;
            textAttribute.FontSize = (float)lineMeasureParam.FontSize;
            textAttribute.FontName = CFontNameHelper.ObtainFontName(CFontNameHelper.GetFontType(lineMeasureParam.FontName),
                        lineMeasureParam.IsBold,
                        lineMeasureParam.IsItalic);
            LineAnnotation.SetTextAttribute(textAttribute);
            if (lineMeasureParam.measureInfo != null)
            {
                CPDFDistanceMeasure polygonMeasure = LineAnnotation.GetDistanceMeasure();
                if (polygonMeasure != null)
                {
                    polygonMeasure.SetMeasureInfo(lineMeasureParam.measureInfo);
                    polygonMeasure.SetMeasureScale(lineMeasureParam.measureInfo.RulerBase, lineMeasureParam.measureInfo.RulerBaseUnit,
                                                   lineMeasureParam.measureInfo.RulerTranslate, lineMeasureParam.measureInfo.RulerTranslateUnit);
                    var x = polygonMeasure.UpdateAnnotMeasure();
                }
            }
            DefaultAnnot(cPDFAnnotation, lineMeasureParam);
        }

        private void DefaultPolygonMeasureAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFPolygonAnnotation PolyAnnotation = (cPDFAnnotation as CPDFPolygonAnnotation);
            if (PolyAnnotation == null)
            {
                return;
            }
            if (!PolyAnnotation.IsMeasured() && annotParam != null)
            {
                return;
            }
            PolygonMeasureParam MeasureParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                MeasureParam = defaultSettingParam.PolygonMeasureParamDef;
            }
            else
            {
                MeasureParam = annotParam as PolygonMeasureParam;
            }

            if (MeasureParam.LineColor != null)
            {
                PolyAnnotation.SetLineColor(MeasureParam.LineColor);
            }
            PolyAnnotation.SetLineWidth((float)MeasureParam.LineWidth);

            if (MeasureParam.LineDash != null)
            {
                if (MeasureParam.LineDash.Length == 0)
                {
                    PolyAnnotation.SetBorderStyle(C_BORDER_STYLE.BS_SOLID, new float[0]);
                }
                else
                {
                    PolyAnnotation.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, MeasureParam.LineDash);
                }
            }

            if (MeasureParam.HasFillColor && MeasureParam.FillColor != null && MeasureParam.FillColor.Length == 3)
            {
                PolyAnnotation.SetBgColor(MeasureParam.FillColor);
            }

            CTextAttribute textAttribute = new CTextAttribute();
            textAttribute.FontColor = MeasureParam.FontColor;
            textAttribute.FontSize = (float)MeasureParam.FontSize;
            textAttribute.FontName = CFontNameHelper.ObtainFontName(CFontNameHelper.GetFontType(MeasureParam.FontName),
                        MeasureParam.IsBold,
                        MeasureParam.IsItalic);
            PolyAnnotation.SetTextAttribute(textAttribute);
            if (MeasureParam.measureInfo != null)
            {
                CPDFAreaMeasure polygonMeasure = PolyAnnotation.GetAreaMeasure();
                if (polygonMeasure != null)
                {
                    polygonMeasure.SetMeasureInfo(MeasureParam.measureInfo);
                    polygonMeasure.SetMeasureScale(MeasureParam.measureInfo.RulerBase, MeasureParam.measureInfo.RulerBaseUnit,
                                                   MeasureParam.measureInfo.RulerTranslate, MeasureParam.measureInfo.RulerTranslateUnit);
                    polygonMeasure.UpdateAnnotMeasure();
                }
            }
            DefaultAnnot(cPDFAnnotation, MeasureParam);
        }

        private void DefaultPolyLineMeasureAnnot(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFPolylineAnnotation PolyAnnotation = (cPDFAnnotation as CPDFPolylineAnnotation);
            if (PolyAnnotation == null)
            {
                return;
            }
            if (!PolyAnnotation.IsMeasured() && annotParam != null)
            {
                return;
            }
            PolyLineMeasureParam lineMeasureParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                lineMeasureParam = defaultSettingParam.PolyLineMeasureParamDef;
            }
            else
            {
                lineMeasureParam = annotParam as PolyLineMeasureParam;
            }
            if (lineMeasureParam.LineColor != null)
            {
                PolyAnnotation.SetLineColor(lineMeasureParam.LineColor);
            }
            PolyAnnotation.SetLineWidth((float)lineMeasureParam.LineWidth);

            if (lineMeasureParam.LineDash != null)
            {
                if (lineMeasureParam.LineDash.Length == 0)
                {
                    PolyAnnotation.SetBorderStyle(C_BORDER_STYLE.BS_SOLID, new float[0]);
                }
                else
                {
                    PolyAnnotation.SetBorderStyle(C_BORDER_STYLE.BS_DASHDED, lineMeasureParam.LineDash);
                }
            }

            CTextAttribute textAttribute = new CTextAttribute();
            textAttribute.FontColor = lineMeasureParam.FontColor;
            textAttribute.FontSize = (float)lineMeasureParam.FontSize;
            textAttribute.FontName = CFontNameHelper.ObtainFontName(CFontNameHelper.GetFontType(lineMeasureParam.FontName),
                        lineMeasureParam.IsBold,
                        lineMeasureParam.IsItalic);
            PolyAnnotation.SetTextAttribute(textAttribute);
            if (lineMeasureParam.measureInfo != null)
            {
                CPDFPerimeterMeasure polygonMeasure = PolyAnnotation.GetPerimeterMeasure();
                if (polygonMeasure != null)
                {
                    polygonMeasure.SetMeasureInfo(lineMeasureParam.measureInfo);
                    polygonMeasure.SetMeasureScale(lineMeasureParam.measureInfo.RulerBase, lineMeasureParam.measureInfo.RulerBaseUnit,
                                                   lineMeasureParam.measureInfo.RulerTranslate, lineMeasureParam.measureInfo.RulerTranslateUnit);
                    polygonMeasure.UpdateAnnotMeasure();
                }
            }
            DefaultAnnot(cPDFAnnotation, lineMeasureParam);
        }

        #endregion

        #region DefaultWidget
        public void CreateDefaultWidget(CPDFAnnotation cPDFAnnotation, C_WIDGET_TYPE annotType, AnnotParam annotParam)
        {
            switch (annotType)
            {
                case C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                    DefaultPushButton(cPDFAnnotation, annotParam);
                    break;
                case C_WIDGET_TYPE.WIDGET_CHECKBOX:
                    DefaultCheckBox(cPDFAnnotation, annotParam);
                    break;
                case C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                    DefaultRadioButton(cPDFAnnotation, annotParam);
                    break;
                case C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                    DefaultTextField(cPDFAnnotation, annotParam);
                    break;
                case C_WIDGET_TYPE.WIDGET_COMBOBOX:
                    DefaultComBoBox(cPDFAnnotation, annotParam);
                    break;
                case C_WIDGET_TYPE.WIDGET_LISTBOX:
                    DefaultListBox(cPDFAnnotation, annotParam);
                    break;
                case C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                    DefaultSignatureFields(cPDFAnnotation, annotParam);
                    break;
                case C_WIDGET_TYPE.WIDGET_UNKNOWN:
                    break;
                default:
                    break;
            }
        }

        private void DefaultWidget(CPDFWidget cPDFWidget, WidgetParm widgetParm)
        {
            if (cPDFWidget == null || widgetParm == null)
            {
                return;
            }
            cPDFWidget.SetWidgetBorderStyle(widgetParm.BorderStyle);
            if (widgetParm.HasLineColor)
            {
                if (widgetParm.LineColor != null)
                {
                    cPDFWidget.SetWidgetBorderRGBColor(widgetParm.LineColor);
                }
            }
            else
            {
                cPDFWidget.ClearWidgetBorderRGBColor();
            }

            if (widgetParm.HasBgColor)
            {
                if (widgetParm.BgColor != null)
                {
                    cPDFWidget.SetWidgetBgRGBColor(widgetParm.BgColor);
                }
            }
            else
            {
                cPDFWidget.ClearWidgetBgRGBColor();
            }

            cPDFWidget.SetBorderWidth((float)widgetParm.LineWidth);
            cPDFWidget.SetFlags(GetFormFlags(FormField.Visible, cPDFWidget));
            cPDFWidget.SetIsReadOnly(widgetParm.IsReadOnly);
            cPDFWidget.SetIsHidden(widgetParm.IsHidden);
            DefaultAnnot(cPDFWidget, widgetParm);
        }

        private void DefaultPushButton(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFPushButtonWidget widget = (cPDFAnnotation as CPDFPushButtonWidget);
            if (widget == null)
            {
                return;
            }
            PushButtonParam pushButtonParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                pushButtonParam = defaultSettingParam.PushButtonParamDef;
            }
            else
            {
                pushButtonParam = annotParam as PushButtonParam;
            }

            CTextAttribute textAttr = new CTextAttribute();
            textAttr.FontColor = pushButtonParam.FontColor;
            textAttr.FontSize = (float)pushButtonParam.FontSize;
            FontType checkFontType = CFontNameHelper.GetFontType(pushButtonParam.FontName);
            textAttr.FontName = CFontNameHelper.ObtainFontName(
                checkFontType == FontType.Unknown ? FontType.Helvetica : checkFontType,
                pushButtonParam.IsBold,
                 pushButtonParam.IsItalic);
            widget.SetTextAttribute(textAttr);

            if (!string.IsNullOrEmpty(pushButtonParam.FieldName))
            {
                widget.SetFieldName(pushButtonParam.FieldName);
            }
            else
            {
                widget.SetFieldName(string.Format("PushButton{0}", widget.Page.GetAnnotCount()));
            }
            if (!string.IsNullOrEmpty(pushButtonParam.Text))
            {
                widget.SetButtonTitle(pushButtonParam.Text);
            }

            switch (pushButtonParam.Action)
            {
                case C_ACTION_TYPE.ACTION_TYPE_GOTO:
                    CPDFGoToAction gotoAction = new CPDFGoToAction();
                    CPDFDestination destination = new CPDFDestination();
                    destination.Position_X = pushButtonParam.DestinationPosition.x;
                    destination.Position_Y = pushButtonParam.DestinationPosition.y;
                    destination.PageIndex = pushButtonParam.DestinationPageIndex;
                    gotoAction.SetDestination(PDFViewer.GetDocument(), destination);
                    widget.SetButtonAction(gotoAction);
                    break;
                case C_ACTION_TYPE.ACTION_TYPE_URI:
                    CPDFUriAction uriAction = new CPDFUriAction();
                    if (!string.IsNullOrEmpty(pushButtonParam.Uri))
                    {
                        uriAction.SetUri(pushButtonParam.Uri);
                    }
                    widget.SetButtonAction(uriAction);
                    break;
                default:
                    break;
            }

            DefaultWidget(widget, pushButtonParam);
        }

        private void DefaultCheckBox(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFCheckBoxWidget widget = (cPDFAnnotation as CPDFCheckBoxWidget);
            if (widget == null)
            {
                return;
            }
            CheckBoxParam checkBoxParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                checkBoxParam = defaultSettingParam.CheckBoxParamDef;
            }
            else
            {
                checkBoxParam = annotParam as CheckBoxParam;
            }

            CTextAttribute textAttr = widget.GetTextAttribute();
            textAttr.FontColor = checkBoxParam.FontColor;
            textAttr.FontSize = (float)checkBoxParam.FontSize;
            bool isBold = IsBold(textAttr.FontName);
            bool isItalic = IsItalic(textAttr.FontName);
            FontType checkFontType = CFontNameHelper.GetFontType(checkBoxParam.FontName);
            textAttr.FontName = CFontNameHelper.ObtainFontName(
                checkFontType == FontType.Unknown ? FontType.Helvetica : checkFontType,
                isBold,
                 isItalic);
            widget.SetTextAttribute(textAttr);

            if (!string.IsNullOrEmpty(checkBoxParam.FieldName))
            {
                widget.SetFieldName(checkBoxParam.FieldName);
            }
            else
            {
                widget.SetFieldName(string.Format("CheckBox{0}", widget.Page.GetAnnotCount()));
            }
            widget.SetWidgetCheckStyle(checkBoxParam.CheckStyle);
            widget.SetChecked(checkBoxParam.IsChecked);

            DefaultWidget(widget, checkBoxParam);
        }

        private void DefaultRadioButton(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFRadioButtonWidget widget = (cPDFAnnotation as CPDFRadioButtonWidget);
            if (widget == null)
            {
                return;
            }
            RadioButtonParam radioButtonParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                radioButtonParam = defaultSettingParam.RadioButtonParamDef;
            }
            else
            {
                radioButtonParam = annotParam as RadioButtonParam;
            }

            CTextAttribute textAttr = widget.GetTextAttribute();
            textAttr.FontColor = radioButtonParam.FontColor;
            textAttr.FontSize = (float)radioButtonParam.FontSize;
            bool isBold = IsBold(textAttr.FontName);
            bool isItalic = IsItalic(textAttr.FontName);
            FontType checkFontType = CFontNameHelper.GetFontType(radioButtonParam.FontName);
            textAttr.FontName = CFontNameHelper.ObtainFontName(
                checkFontType == FontType.Unknown ? FontType.Helvetica : checkFontType,
                isBold,
                 isItalic);
            widget.SetTextAttribute(textAttr);

            if (!string.IsNullOrEmpty(radioButtonParam.FieldName))
            {
                widget.SetFieldName(radioButtonParam.FieldName);
            }
            else
            {
                widget.SetFieldName("RadioGroup1");
            }
            widget.SetWidgetCheckStyle(radioButtonParam.CheckStyle);
            widget.SetChecked(radioButtonParam.IsChecked);

            DefaultWidget(widget, radioButtonParam);
        }

        private void DefaultTextField(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFTextWidget widget = (cPDFAnnotation as CPDFTextWidget);
            if (widget == null)
            {
                return;
            }
            TextBoxParam textBoxParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                textBoxParam = defaultSettingParam.TextBoxParamDef;
            }
            else
            {
                textBoxParam = annotParam as TextBoxParam;
            }

            CTextAttribute textAttr = new CTextAttribute();
            textAttr.FontColor = textBoxParam.FontColor;
            textAttr.FontSize = (float)textBoxParam.FontSize;
            FontType checkFontType = CFontNameHelper.GetFontType(textBoxParam.FontName);
            textAttr.FontName = CFontNameHelper.ObtainFontName(
                checkFontType == FontType.Unknown ? FontType.Helvetica : checkFontType,
                textBoxParam.IsBold,
                 textBoxParam.IsItalic);
            widget.SetTextAttribute(textAttr);

            if (!string.IsNullOrEmpty(textBoxParam.FieldName))
            {
                widget.SetFieldName(textBoxParam.FieldName);
            }
            else
            {
                widget.SetFieldName(string.Format("TextField{0}", widget.Page.GetAnnotCount()));
            }

            widget.SetMultiLine(textBoxParam.IsMultiLine);
            widget.SetJustification(textBoxParam.Alignment);

            if (string.IsNullOrEmpty(textBoxParam.Text) == false)
            {
                widget.SetText(textBoxParam.Text);
            }

            DefaultWidget(widget, textBoxParam);
        }

        private void DefaultComBoBox(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFComboBoxWidget widget = (cPDFAnnotation as CPDFComboBoxWidget);
            if (widget == null)
            {
                return;
            }
            ComboBoxParam comboBoxParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                comboBoxParam = defaultSettingParam.ComboBoxParamDef;
            }
            else
            {
                comboBoxParam = annotParam as ComboBoxParam;
            }

            CTextAttribute textAttr = new CTextAttribute();
            textAttr.FontColor = comboBoxParam.FontColor;
            textAttr.FontSize = (float)comboBoxParam.FontSize;
            FontType checkFontType = CFontNameHelper.GetFontType(comboBoxParam.FontName);
            textAttr.FontName = CFontNameHelper.ObtainFontName(
                checkFontType == FontType.Unknown ? FontType.Helvetica : checkFontType,
                comboBoxParam.IsBold,
                 comboBoxParam.IsItalic);
            widget.SetTextAttribute(textAttr);

            if (!string.IsNullOrEmpty(comboBoxParam.FieldName))
            {
                widget.SetFieldName(comboBoxParam.FieldName);
            }
            else
            {
                widget.SetFieldName(string.Format("ComBoBox{0}", widget.Page.GetAnnotCount()));
            }
            if (comboBoxParam.OptionItems != null && comboBoxParam.OptionItems.Count > 0)
            {
                int addIndex = 0;
                foreach (string key in comboBoxParam.OptionItems.Keys)
                {
                    widget.AddOptionItem(addIndex, comboBoxParam.OptionItems[key], key);
                    addIndex++;
                }
            }

            if (comboBoxParam.SelectItemsIndex != null && comboBoxParam.SelectItemsIndex.Count > 0)
            {
                widget.SelectItem(comboBoxParam.SelectItemsIndex[0]);
            }

            DefaultWidget(widget, comboBoxParam);
        }

        private void DefaultListBox(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFListBoxWidget widget = (cPDFAnnotation as CPDFListBoxWidget);
            if (widget == null)
            {
                return;
            }
            ListBoxParam listBoxParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                listBoxParam = defaultSettingParam.ListBoxParamDef;
            }
            else
            {
                listBoxParam = annotParam as ListBoxParam;
            }

            CTextAttribute textAttr = new CTextAttribute();
            textAttr.FontColor = listBoxParam.FontColor;
            textAttr.FontSize = (float)listBoxParam.FontSize;
            FontType checkFontType = CFontNameHelper.GetFontType(listBoxParam.FontName);
            textAttr.FontName = CFontNameHelper.ObtainFontName(
                checkFontType == FontType.Unknown ? FontType.Helvetica : checkFontType,
                listBoxParam.IsBold,
                 listBoxParam.IsItalic);
            widget.SetTextAttribute(textAttr);

            if (!string.IsNullOrEmpty(listBoxParam.FieldName))
            {
                widget.SetFieldName(listBoxParam.FieldName);
            }
            else
            {
                widget.SetFieldName(string.Format("ComBoBox{0}", widget.Page.GetAnnotCount()));
            }
            if (listBoxParam.OptionItems != null && listBoxParam.OptionItems.Count > 0)
            {
                int addIndex = 0;
                foreach (string key in listBoxParam.OptionItems.Keys)
                {
                    widget.AddOptionItem(addIndex, listBoxParam.OptionItems[key], key);
                    addIndex++;
                }
            }

            if (listBoxParam.SelectItemsIndex != null && listBoxParam.SelectItemsIndex.Count > 0)
            {
                widget.SelectItem(listBoxParam.SelectItemsIndex[0]);
            }

            DefaultWidget(widget, listBoxParam);
        }

        private void DefaultSignatureFields(CPDFAnnotation cPDFAnnotation, AnnotParam annotParam)
        {
            CPDFSignatureWidget widget = (cPDFAnnotation as CPDFSignatureWidget);
            if (widget == null)
            {
                return;
            }
            SignatureParam signatureParam;
            if (annotParam == null)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();

                signatureParam = defaultSettingParam.SignatureParamDef;
            }
            else
            {
                signatureParam = annotParam as SignatureParam;
            }
            CTextAttribute textAttr = widget.GetTextAttribute();
            textAttr.FontColor = signatureParam.FontColor;
            textAttr.FontSize = (float)signatureParam.FontSize;
            bool isBold = IsBold(textAttr.FontName);
            bool isItalic = IsItalic(textAttr.FontName);
            FontType checkFontType = CFontNameHelper.GetFontType(signatureParam.FontName);
            textAttr.FontName = CFontNameHelper.ObtainFontName(
                checkFontType == FontType.Unknown ? FontType.Helvetica : checkFontType,
                isBold,
                 isItalic);
            widget.SetTextAttribute(textAttr);

            if (!string.IsNullOrEmpty(signatureParam.FieldName))
            {
                widget.SetFieldName(signatureParam.FieldName);
            }
            else
            {
                widget.SetFieldName(string.Format("ComBoBox{0}", widget.Page.GetAnnotCount()));
            }

            DefaultWidget(widget, signatureParam);
        }

        #endregion

    }
}
