using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer;
using ComPDFKitViewer.Annot;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Widget;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation.Form;
using System.Windows.Controls;
using System.Windows.Media;
using static ComPDFKit.PDFAnnotation.CTextAttribute.CFontNameHelper;
using ComPDFKitViewer.Helper;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Measure;
using System.Dynamic;

namespace ComPDFKit.Tool
{
    public class MeasureEventArgs : EventArgs
    {
        public CPDFMeasureType Type { get; set; }
        public double RulerBase { get; set; } = 1;
        public string RulerBaseUnit { get; set; } = CPDFMeasure.CPDF_CM;
        public double RulerTranslate { get; set; } = 1;
        public string RulerTranslateUnit { get; set; } = CPDFMeasure.CPDF_CM;
        public double Precision { get; set; } = 0.01;
        public double Angle { get; set; }
        public Point MousePos { get; set; }
        public string Round { get; set; }
        public string Distance { get; set; }
        public string Area { get; set; }
    }

    public partial class CPDFViewerTool
    {
        Border textBorder;
        TextBox textui;
        BaseAnnot caheMoveAnnot;
        BaseAnnot cacheHitTestAnnot;
        bool isCacheRedaction = false;
        int selectedPageIndex = -1;
        int selectedAnnotIndex = -1;
        bool canSave = true;
        bool isHitTestLink = false;
        bool isHitTestRedact = false;

        public event EventHandler<MeasureEventArgs> MeasureChanged;
        internal int CreateAnnotTag { get; private set; } = -1;

        public void InvokeMeasureChangeEvent(object sender, MeasureEventArgs e)
        {
            MeasureChanged?.Invoke(sender, e);
        }

        public bool GetIsHitTestLink()
        {
            return isHitTestLink;
        }

        public void SetIsHitTestLink(bool canHitTestLink)
        {
            isHitTestLink = canHitTestLink;
        }

        public void SetIsOnlyHitTestRedact(bool canHitTestRedact)
        {
            isHitTestRedact = canHitTestRedact;
        }

        public bool IsCanSave()
        {
            return canSave;
        }

        public void SetIsCanSave(bool save)
        {
            canSave = save;
        }

        public BaseAnnot GetCacheHitTestAnnot()
        {
            return cacheHitTestAnnot;
        }

        protected bool AnnotMoveHitTest()
        {
            BaseAnnot baseAnnot = PDFViewer.AnnotHitTest();
            if (baseAnnot != null)
            {
                if (baseAnnot.CurrentType == C_ANNOTATION_TYPE.C_ANNOTATION_REDACT)
                {
                    if (isCacheRedaction)
                    {
                        (caheMoveAnnot as RedactionAnnot).SetIsMouseHover(false);
                        (caheMoveAnnot as RedactionAnnot).Draw();
                    }
                    isCacheRedaction = true;
                }
                else
                {
                    if (isCacheRedaction)
                    {
                        (caheMoveAnnot as RedactionAnnot).SetIsMouseHover(false);
                        (caheMoveAnnot as RedactionAnnot).Draw();
                    }
                    isCacheRedaction = false;
                }
                caheMoveAnnot = baseAnnot;
                return true;
            }
            return false;
        }

        protected bool AnnotHitTest()
        {
            BaseAnnot baseAnnot = PDFViewer.AnnotHitTest(true);
            if (baseAnnot != null)
            {
                if ((baseAnnot as BaseWidget) != null)
                {
                    cacheHitTestAnnot = null;
                    return false;
                }

                cacheHitTestAnnot = baseAnnot;
                return true;
            }
            else
            {
                cacheHitTestAnnot = null;
                return false;
            }
        }

        public void SelectedAnnotForIndex(int pageIndex, int annotIndex)
        {
            CleanSelectedRect();
            CleanSelectedMultiRect();
            CleanEditAnnot();
            cacheHitTestAnnot = null;
            selectedPageIndex = pageIndex;
            selectedAnnotIndex = annotIndex;
        }

        private bool UnCheckAnnotViewerModel()
        {
            if (currentModel == ToolType.CreateAnnot || currentModel == ToolType.WidgetEdit)
            {
                return false;
            }
            return true;
        }

        private void InsertAnnotView()
        {
            CreateAnnotTool createAnnotTool = new CreateAnnotTool(GetMeasureSetting(), GetDefaultDrawParam(), GetDefaultSettingParam());
            int annotViewindex = PDFViewer.GetMaxViewIndex();
            PDFViewer.InsertView(annotViewindex, createAnnotTool);
            CreateAnnotTag = createAnnotTool.GetResTag();
            createAnnotTool.UpdateAnnotHandler += CreateAnnotTool_UpdateAnnotHandler;
            createAnnotTool.CreateFreetextCanceled += CreateAnnotTool_CreateFreetextCanceled;
            createAnnotTool.MeasureChanged += CreateAnnotTool_MeasureChanged;
        }

        private void CreateAnnotTool_CreateFreetextCanceled(object sender, AnnotParam e)
        {
            dynamic notifyData = null;
            notifyData = new ExpandoObject();
            notifyData.Action = HistoryAction.Remove;
            notifyData.PageIndex = e.PageIndex;
            notifyData.AnnotIndex = e.AnnotIndex;
            notifyData.AnnotType = e.GetType();
            notifyData.CurrentParam = e;
            AnnotChanged?.Invoke(this, notifyData);
        }

        private void CreateAnnotTool_MeasureChanged(object sender, MeasureEventArgs e)
        {
            InvokeMeasureChangeEvent(sender, e);
        }

        private void CreateAnnotTool_UpdateAnnotHandler(object sender, bool e)
        {
            PDFViewer.EnableZoom(e);
            PDFViewer.CanHorizontallyScroll = e;
            PDFViewer.CanVerticallyScroll = e;
            if (e)
            {
                PDFViewer.UpdateAnnotFrame();
            }
        }

        public void SetAnnotIsProportionalScaling(bool isProportionalScaling)
        {
            if (UnCheckAnnotViewerModel())
            {
                return;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            (baseLayer as CreateAnnotTool).SetIsProportionalScaling(isProportionalScaling);
        }

        public double GetMoveLength()
        {
            if (UnCheckAnnotViewerModel())
            {
                return 0;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            return (baseLayer as CreateAnnotTool).GetMoveLength();
        }

        public CPDFAnnotation StartDrawAnnot(C_ANNOTATION_TYPE annotType)
        {
            if (UnCheckAnnotViewerModel())
            {
                return null;
            }

            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            PDFViewer.GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
            if (index < 0)
            {
                return null;
            }
            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
            if (annotType == C_ANNOTATION_TYPE.C_ANNOTATION_STAMP)
            {
                DefaultSettingParam defaultSettingParam = GetDefaultSettingParam();
                StampParam stampParam = defaultSettingParam.StampParamDef;
                stampParam.PageRotation = cPDFPage.Rotation;
                defaultSettingParam.SetAnnotParam(stampParam);
            }
            Point cropPoint = new Point();
            if (PDFViewer.GetIsCrop())
            {
                CRect cRect = cPDFPage.GetCropBounds();
                cropPoint.X = DpiHelper.PDFNumToStandardNum(cRect.left);
                cropPoint.Y = DpiHelper.PDFNumToStandardNum(cRect.top);
            }
            SetScrollAndZoomTypeForAnnot(annotType);
            switch (annotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    canSave = false;
                    break;
                default:
                    break;
            }
            return (baseLayer as CreateAnnotTool).StartDraw(point, cropPoint, cPDFPage, paintRect, pageBound, annotType, PDFViewer, PDFViewer.GetZoom());
        }

        public void MultipleClick()
        {
            if (UnCheckAnnotViewerModel())
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            PDFViewer.GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
            if (index < 0)
            {
                return;
            }
            (baseLayer as CreateAnnotTool).MultipleClick(point);
        }

        public void SetScrollAndZoomTypeForAnnot(C_ANNOTATION_TYPE annotType)
        {
            bool enableScroll = false;
            bool enableZoom = false;
            switch (annotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                    enableScroll = true;
                    enableZoom = true;
                    break;
                default:
                    break;
            }
            PDFViewer.CanHorizontallyScroll = enableScroll;
            PDFViewer.CanVerticallyScroll = enableScroll;
            PDFViewer.EnableZoom(enableZoom);
        }

        public void MoveDrawAnnot()
        {
            if (UnCheckAnnotViewerModel())
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            (baseLayer as CreateAnnotTool).MoveDraw(point, PDFViewer.GetZoom());
        }

        public void CreateTextBox()
        {
            if (UnCheckAnnotViewerModel())
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            (baseLayer as CreateAnnotTool).CreateTextBox();
        }

        public Rect EndDrawAnnot()
        {
            if (UnCheckAnnotViewerModel())
            {
                return new Rect();
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            return (baseLayer as CreateAnnotTool).EndDraw();
        }

        private void SetMarkupContent(CPDFMarkupAnnotation markupAnnot, string markupContent)
        {
            if (markupAnnot == null || markupAnnot.IsValid() == false)
            {
                return;
            }

            try
            {
                DefaultSettingParam defaultParam = GetDefaultSettingParam();
                if (defaultParam != null)
                {
                    switch (markupAnnot.Type)
                    {
                        case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                            if (string.IsNullOrEmpty(defaultParam.HighlightParamDef.Content) == false)
                            {
                                markupContent = defaultParam.HighlightParamDef.Content;
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                            if (string.IsNullOrEmpty(defaultParam.UnderlineParamDef.Content) == false)
                            {
                                markupContent = defaultParam.UnderlineParamDef.Content;
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                            if (string.IsNullOrEmpty(defaultParam.SquigglyParamDef.Content) == false)
                            {
                                markupContent = defaultParam.SquigglyParamDef.Content;
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                            if (string.IsNullOrEmpty(defaultParam.StrikeoutParamDef.Content) == false)
                            {
                                markupContent = defaultParam.StrikeoutParamDef.Content;
                            }
                            break;
                        default:
                            return;
                    }
                }

                if (string.IsNullOrEmpty(markupContent) == false)
                {
                    markupAnnot.SetContent(markupContent);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public bool CreateAnnotForSelectText(TextSelectInfo textSelectInfo, C_ANNOTATION_TYPE annotType, out GroupHistory historyData)
        {
            historyData = null;
            if (UnCheckAnnotViewerModel())
            {
                return false;
            }
            Dictionary<int, List<Rect>> PagesRectList = textSelectInfo.ConvertToSelectRectDict();
            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
            GroupHistory historyGroup = new GroupHistory();
            foreach (int pageIndex in PagesRectList.Keys)
            {
                List<Rect> pageSelectRectList = PagesRectList[pageIndex];
                if (pageSelectRectList.Count > 0)
                {
                    CPDFPage docPage = cPDFDocument.PageAtIndex(pageIndex);
                    docPage.ReleaseAllAnnotations();
                    CPDFAnnotation annotCore = docPage.CreateAnnot(annotType);
                    annotCore.SetCreationDate(PDFHelp.GetCurrentPdfTime());
                    annotCore.SetModifyDate(PDFHelp.GetCurrentPdfTime());
                    if (annotCore == null || annotCore is CPDFWidget)
                    {
                        return false;
                    }
                    List<CRect> coreRectList = new List<CRect>();
                    foreach (Rect copyRect in pageSelectRectList)
                    {
                        coreRectList.Add(new CRect((float)copyRect.Left, (float)copyRect.Bottom, (float)copyRect.Right, (float)copyRect.Top));
                    }
                    CreateDefaultAnnot(annotCore, annotType, null);
                    string markupContent = textSelectInfo.PageSelectText[pageIndex];

                    switch (annotType)
                    {
                        case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                            (annotCore as CPDFHighlightAnnotation).SetQuardRects(coreRectList);
                            SetMarkupContent(annotCore as CPDFMarkupAnnotation, markupContent);
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                            (annotCore as CPDFUnderlineAnnotation).SetQuardRects(coreRectList);
                            SetMarkupContent(annotCore as CPDFMarkupAnnotation, markupContent);
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                            (annotCore as CPDFSquigglyAnnotation).SetQuardRects(coreRectList);
                            SetMarkupContent(annotCore as CPDFMarkupAnnotation, markupContent);
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                            (annotCore as CPDFStrikeoutAnnotation).SetQuardRects(coreRectList);
                            SetMarkupContent(annotCore as CPDFMarkupAnnotation, markupContent);
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                            (annotCore as CPDFRedactAnnotation).SetQuardRects(coreRectList);
                            break;
                        default:
                            break;
                    }

                    int Left = (int)pageSelectRectList.Select(x => x.Left).Min();
                    int Top = (int)pageSelectRectList.Select(x => x.Top).Min();
                    int Right = (int)pageSelectRectList.Select(x => x.Right).Max();
                    int Bottom = (int)pageSelectRectList.Select(x => x.Bottom).Max();
                    annotCore.SetRect(new CRect(Left, Bottom, Right, Top));

                    //if (annotCore.GetIsLocked() != underlineArgs.Locked)
                    //{
                    //    annotCore.SetIsLocked(underlineArgs.Locked);
                    //}
                    annotCore.UpdateAp();
                    switch (annotType)
                    {
                        case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                            {
                                HighlightAnnotHistory highlightAnnotHistory = new HighlightAnnotHistory();
                                AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, annotCore);
                                highlightAnnotHistory.CurrentParam = (HighlightParam)annotParam;
                                highlightAnnotHistory.PDFDoc = cPDFDocument;
                                highlightAnnotHistory.Action = HistoryAction.Add;
                                historyGroup.Histories.Add(highlightAnnotHistory);

                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                            {
                                UnderlineAnnotHistory underlineAnnotHistory = new UnderlineAnnotHistory();
                                AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, annotCore);
                                underlineAnnotHistory.CurrentParam = (UnderlineParam)annotParam;
                                underlineAnnotHistory.PDFDoc = cPDFDocument;
                                underlineAnnotHistory.Action = HistoryAction.Add;
                                historyGroup.Histories.Add(underlineAnnotHistory);
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                            {
                                SquigglyAnnotHistory squigglyAnnotHistory = new SquigglyAnnotHistory();
                                AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, annotCore);
                                squigglyAnnotHistory.CurrentParam = (SquigglyParam)annotParam;
                                squigglyAnnotHistory.PDFDoc = cPDFDocument;
                                squigglyAnnotHistory.Action = HistoryAction.Add;
                                historyGroup.Histories.Add(squigglyAnnotHistory);
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                            {
                                StrikeoutAnnotHistory strikeoutHistory = new StrikeoutAnnotHistory();
                                AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, annotCore);
                                strikeoutHistory.CurrentParam = (StrikeoutParam)annotParam;
                                strikeoutHistory.PDFDoc = cPDFDocument;
                                strikeoutHistory.Action = HistoryAction.Add;
                                historyGroup.Histories.Add(strikeoutHistory);
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                            {
                                RedactAnnotHistory redactHistory = new RedactAnnotHistory();
                                AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, annotCore);
                                redactHistory.Action = HistoryAction.Add;
                                redactHistory.CurrentParam = (RedactParam)annotParam;
                                redactHistory.PDFDoc = cPDFDocument;
                                redactHistory.Action = HistoryAction.Add;
                                historyGroup.Histories.Add(redactHistory);
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                            {
                                LinkAnnotHistory linkHistory = new LinkAnnotHistory();
                                AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, annotCore);
                                linkHistory.Action = HistoryAction.Add;
                                linkHistory.CurrentParam = (LinkParam)annotParam;
                                linkHistory.PDFDoc = cPDFDocument;
                                linkHistory.Action = HistoryAction.Add;
                                historyGroup.Histories.Add(linkHistory);
                            }
                            break;
                        default:
                            break;
                    }
                    annotCore.ReleaseAnnot();
                }
            }

            if (historyGroup.Histories.Count > 0)
            {
                GetCPDFViewer()?.UndoManager?.AddHistory(historyGroup);
            }
            PDFViewer.UpdateAnnotFrame();
            historyData = historyGroup;
            return true;
        }

        public Point GetStartPoint()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            return (baseLayer as CreateAnnotTool).GetStartPoint();
        }

        public Point GetEndPoint()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            return (baseLayer as CreateAnnotTool).GetEndPoint();
        }

        public List<Point> GetInkDrawPoints()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            return (baseLayer as CreateAnnotTool).GetInkDrawPoints();
        }

        public List<Point> GetMeasureDrawPoints()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            return (baseLayer as CreateAnnotTool).GetMeasureDrawPoints();
        }

        public Rect GetDrawAnnotMaxRect()
        {
            if (UnCheckAnnotViewerModel())
            {
                return new Rect();
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            return (baseLayer as CreateAnnotTool).GetMaxRect();
        }

        public void ClearDrawAnnot()
        {
            if (UnCheckAnnotViewerModel())
            {
                return;
            }
            Point point = Mouse.GetPosition(this);
            BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
            (baseLayer as CreateAnnotTool).ClearDraw();
        }

        protected void UpdateTextPop()
        {
            if (textBorder != null)
            {
                BaseAnnot currentAnnot = textui.GetValue(PopupAttachDataProperty) as BaseAnnot;
                if (currentAnnot == null)
                {
                    return;
                }

                AnnotLayer annotLayer = PDFViewer.GetViewForTag(PDFViewer.GetAnnotViewTag()) as AnnotLayer;

                bool isOk = annotLayer.GetUpdate(ref currentAnnot);
                if (!isOk)
                {
                    return;
                }
                AnnotData annotData = currentAnnot.GetAnnotData();

                if (annotData.PaintRect == Rect.Empty)
                {
                    return;
                }
                //SetFormRotateTransform(textui, annotData);
                // Set the width and height of the TextBox, rotation, and other control position information
                RotateTransform rotateTrans = new RotateTransform();
                rotateTrans.Angle = -90 * annotData.Rotation;
                rotateTrans.CenterX = annotData.PaintRect.Width / 2;
                rotateTrans.CenterY = annotData.PaintRect.Height / 2;
                Rect rotateRect = rotateTrans.TransformBounds(annotData.PaintRect);

                textBorder.Width = rotateRect.Width;
                textui.MinHeight = Math.Max(0, textui.FontSize + 8);
                textui.MaxHeight = Math.Max(0, annotData.PaintOffset.Bottom - annotData.PaintRect.Top - 8);
                textBorder.SetValue(Canvas.LeftProperty, annotData.PaintRect.Left + rotateTrans.CenterX - rotateRect.Width / 2);
                textBorder.SetValue(Canvas.TopProperty, annotData.PaintRect.Top + rotateTrans.CenterY - rotateRect.Height / 2);
            }
        }

        protected void BuildPopTextUI(BaseAnnot textAnnot)
        {
            try
            {
                if (textAnnot != null && textAnnot.CurrentType == C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT)
                {
                    AnnotData annotData = textAnnot.GetAnnotData();
                    CPDFFreeTextAnnotation textWidget = annotData.Annot as CPDFFreeTextAnnotation;
                    if (textWidget == null)
                    {
                        return;
                    }
                    textAnnot.CleanDraw();
                    CRect drawRect = textWidget.GetRect();
                    double Width = DpiHelper.PDFNumToStandardNum(drawRect.width());
                    double Height = DpiHelper.PDFNumToStandardNum(drawRect.height());
                    textui = new TextBox();
                    textui.Name = "PdfViewerTextBox";
                    textBorder = new DashedBorder();
                    textBorder.Child = textui;
                    textBorder.MinWidth = Width * PDFViewer.GetZoom();
                    textBorder.MinHeight = Height * PDFViewer.GetZoom();

                    // Calculate the maximum value
                    double PDFWidth = PDFViewer.GetCurrentRenderPageForIndex(annotData.PageIndex).PaintRect.Width;
                    double PDFHeight = PDFViewer.GetCurrentRenderPageForIndex(annotData.PageIndex).PaintRect.Height;

                    textBorder.MaxWidth = (PDFWidth - annotData.VisualRect.Left - annotData.CropLeft);
                    textBorder.MaxHeight = (PDFHeight - annotData.VisualRect.Top - annotData.CropTop);
                    CTextAttribute textAttribute = textWidget.FreeTextDa;
                    byte transparency = textWidget.GetTransparency();
                    textui.FontSize = DpiHelper.PDFNumToStandardNum(textAttribute.FontSize * annotData.CurrentZoom);
                    Color textColor = Color.FromArgb(
                        transparency,
                        textAttribute.FontColor[0],
                        textAttribute.FontColor[1],
                        textAttribute.FontColor[2]);

                    Color borderColor = Colors.Transparent;
                    Color backgroundColor = Colors.Transparent;
                    byte[] colorArray = new byte[3];
                    if (textWidget.Transparency > 0)
                    {
                        borderColor = Color.FromArgb(textWidget.Transparency, textWidget.LineColor[0], textWidget.LineColor[1], textWidget.LineColor[2]);
                    }

                    if (textWidget.HasBgColor)
                    {
                        backgroundColor = Color.FromArgb(textWidget.Transparency, textWidget.BgColor[0], textWidget.BgColor[1], textWidget.BgColor[2]);
                    }

                    textui.Foreground = new SolidColorBrush(textColor);
                    textui.Background = new SolidColorBrush(backgroundColor);

                    textBorder.Padding = new Thickness(0);
                    textBorder.BorderBrush = new SolidColorBrush(borderColor);
                    double rawWidth = textWidget.GetBorderWidth();
                    double drawWidth = DpiHelper.PDFNumToStandardNum(rawWidth * annotData.CurrentZoom);
                    textBorder.BorderThickness = new Thickness(drawWidth);
                    textui.BorderThickness = new Thickness(0);
                    textui.Text = textWidget.Content;
                    if (textWidget.BorderStyle != C_BORDER_STYLE.BS_SOLID && textWidget.Dash != null && textWidget.Dash.Length > 0)
                    {
                        //补充保存虚线样式
                        DashedBorder dashBorder = textBorder as DashedBorder;
                        DoubleCollection dashCollection = new DoubleCollection();
                        foreach (float num in textWidget.Dash)
                        {
                            dashCollection.Add(num);
                        }
                        dashBorder?.DrawDashBorder(true, drawWidth, rawWidth, dashCollection);
                    }

                    string fontName = string.Empty;
                    string fontFamily = string.Empty;
                    CPDFFont.GetFamilyStyleName(textWidget.FreeTextDa.FontName, ref fontFamily, ref fontName);
                    textui.FontFamily = new FontFamily(fontFamily);

                    textui.AcceptsReturn = true;
                    textui.TextWrapping = TextWrapping.Wrap;
                    textui.TextAlignment = TextAlignment.Left;

                    switch (textWidget.Alignment)
                    {
                        case C_TEXT_ALIGNMENT.ALIGNMENT_LEFT:
                            textui.TextAlignment = TextAlignment.Left;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT:
                            textui.TextAlignment = TextAlignment.Right;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_CENTER:
                            textui.TextAlignment = TextAlignment.Center;
                            break;
                        default:
                            break;
                    }

                    //SetFormRotateTransform(textui, annotData);
                    // Set the width and height of the TextBox, rotation, and other control position information
                    RotateTransform rotateTrans = new RotateTransform();
                    rotateTrans.Angle = -90 * annotData.Rotation;
                    rotateTrans.CenterX = annotData.PaintRect.Width / 2;
                    rotateTrans.CenterY = annotData.PaintRect.Height / 2;
                    Rect rotateRect = rotateTrans.TransformBounds(annotData.PaintRect);

                    textBorder.Width = rotateRect.Width;
                    textui.MinHeight = Math.Max(0, textui.FontSize + 8);
                    textui.MaxHeight = Math.Max(0, annotData.PaintOffset.Bottom - annotData.PaintRect.Top - 8);
                    textBorder.SetValue(Canvas.LeftProperty, annotData.PaintRect.Left + rotateTrans.CenterX - rotateRect.Width / 2);
                    textBorder.SetValue(Canvas.TopProperty, annotData.PaintRect.Top + rotateTrans.CenterY - rotateRect.Height / 2);

                    rotateTrans.Angle = 90 * annotData.Rotation;
                    rotateTrans.CenterX = rotateRect.Width / 2;
                    rotateTrans.CenterY = rotateRect.Height / 2;
                    textBorder.RenderTransform = rotateTrans;

                    textui.Loaded += (object sender, RoutedEventArgs e) =>
                    {
                        textui.Focus();
                        textui.CaretIndex = textui.Text.Length;
                        textui.SetValue(PopupAttachDataProperty, textAnnot);
                    };

                    CPDFViewer viewer = GetCPDFViewer();
                    textui.LostFocus += (object sender, RoutedEventArgs e) =>
                    {
                        BaseAnnot currentAnnot = textui.GetValue(PopupAttachDataProperty) as BaseAnnot;
                        if (currentAnnot != null)
                        {
                            AnnotData widgetData = currentAnnot.GetAnnotData();
                            CPDFFreeTextAnnotation updateFreeText = widgetData.Annot as CPDFFreeTextAnnotation;

                            if (textui.Text == string.Empty && updateFreeText.GetBorderWidth() == 0)
                            {
                                dynamic notifyData = null;
                                notifyData = new ExpandoObject();
                                notifyData.Action = HistoryAction.Remove;
                                notifyData.PageIndex = widgetData.PageIndex;
                                notifyData.AnnotIndex = widgetData.AnnotIndex;
                                notifyData.AnnotType = widgetData.AnnotType;
                                notifyData.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(viewer.GetDocument(), widgetData.PageIndex, updateFreeText);
                                updateFreeText.RemoveAnnot();
                                AnnotChanged?.Invoke(this, notifyData);
                            }
                            else
                            {
                                FreeTextAnnotHistory history = null;
                                if (updateFreeText.Content != textui.Text)
                                {
                                    history = new FreeTextAnnotHistory();
                                    history.PDFDoc = viewer.GetDocument();
                                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(viewer.GetDocument(), updateFreeText.Page.PageIndex, updateFreeText);
                                    history.Action = HistoryAction.Update;
                                    updateFreeText.SetContent(textui.Text);
                                }

                                RotateTransform rotateTranss = new RotateTransform();
                                rotateTranss.Angle = -90 * annotData.Rotation;
                                rotateTranss.CenterX = textBorder.ActualWidth / 2;
                                rotateTranss.CenterY = textBorder.ActualHeight / 2;
                                Rect textRect = rotateTranss.TransformBounds(new Rect(0, 0, textBorder.ActualWidth, textBorder.ActualHeight));

                                Rect changeRect = new Rect(
                                    annotData.ClientRect.Left,
                                    annotData.ClientRect.Top,
                                    DpiHelper.StandardNumToPDFNum(textRect.Width / annotData.CurrentZoom),
                                    DpiHelper.StandardNumToPDFNum(textRect.Height / annotData.CurrentZoom));

                                updateFreeText.SetRect(new CRect(
                                     (float)changeRect.Left,
                                      (float)changeRect.Bottom,
                                       (float)changeRect.Right,
                                        (float)changeRect.Top
                                    ));
                                updateFreeText.UpdateAp();

                                if (history != null)
                                {
                                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(viewer.GetDocument(), updateFreeText.Page.PageIndex, updateFreeText);
                                    viewer.UndoManager?.AddHistory(history);
                                    viewer.UndoManager?.InvokeHistoryChanged(this, new KeyValuePair<ComPDFKitViewer.Helper.UndoAction, IHistory>(ComPDFKitViewer.Helper.UndoAction.Custom, history));
                                }
                                viewer.UpdateAnnotFrame();
                            }
                            RemovePopTextUI();
                        }
                    };

                    BaseLayer createAnnotTool = PDFViewer?.GetView(CreateAnnotTag) as CreateAnnotTool;
                    if (createAnnotTool != null)
                    {
                        createAnnotTool.Children.Add(textBorder);
                        createAnnotTool.Arrange();
                    }

                    textui.LayoutUpdated += (object sender, EventArgs e) =>
                    {
                        createAnnotTool.Arrange();
                    };
                }
            }
            catch (Exception ex)
            {

            }

        }

        public void UpdatePopTextUI(BaseAnnot textAnnot)
        {
            try
            {
                if (textui != null && textBorder != null && textAnnot != null && textAnnot.CurrentType == C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT)
                {
                    AnnotData annotData = textAnnot.GetAnnotData();
                    CPDFFreeTextAnnotation textWidget = annotData.Annot as CPDFFreeTextAnnotation;
                    if (textWidget == null)
                    {
                        return;
                    }
                    textAnnot.CleanDraw();

                    CTextAttribute textAttribute = textWidget.FreeTextDa;
                    byte transparency = textWidget.GetTransparency();
                    textui.FontSize = DpiHelper.PDFNumToStandardNum(textAttribute.FontSize * annotData.CurrentZoom);
                    Color textColor = Color.FromArgb(
                        transparency,
                        textAttribute.FontColor[0],
                        textAttribute.FontColor[1],
                        textAttribute.FontColor[2]);

                    Color borderColor = Colors.Transparent;
                    Color backgroundColor = Colors.White;
                    byte[] colorArray = new byte[3];
                    if (textWidget.Transparency > 0)
                    {
                        borderColor = Color.FromRgb(textWidget.LineColor[0], textWidget.LineColor[1], textWidget.LineColor[2]);
                    }

                    if (textWidget.HasBgColor)
                    {
                        backgroundColor = Color.FromRgb(textWidget.BgColor[0], textWidget.BgColor[1], textWidget.BgColor[2]);
                    }

                    textui.Foreground = new SolidColorBrush(textColor);
                    textui.Background = new SolidColorBrush(backgroundColor);

                    textBorder.Padding = new Thickness(0);
                    textBorder.BorderBrush = new SolidColorBrush(borderColor);
                    textBorder.BorderThickness = new Thickness(DpiHelper.PDFNumToStandardNum(textWidget.GetBorderWidth() * annotData.CurrentZoom));
                    textui.BorderThickness = new Thickness(0);
                    textui.Text = textWidget.Content;

                    textui.FontFamily = new FontFamily(GetFontName(textAttribute.FontName));
                    textui.FontWeight = IsBold(textAttribute.FontName) ? FontWeights.Bold : FontWeights.Normal;
                    textui.FontStyle = IsItalic(textAttribute.FontName) ? FontStyles.Italic : FontStyles.Normal;

                    textui.AcceptsReturn = true;
                    textui.TextWrapping = TextWrapping.Wrap;
                    //textui.VerticalContentAlignment = VerticalAlignment.Center;
                    textui.TextAlignment = TextAlignment.Left;

                    switch (textWidget.Alignment)
                    {
                        case C_TEXT_ALIGNMENT.ALIGNMENT_LEFT:
                            textui.TextAlignment = TextAlignment.Left;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT:
                            textui.TextAlignment = TextAlignment.Right;
                            break;
                        case C_TEXT_ALIGNMENT.ALIGNMENT_CENTER:
                            textui.TextAlignment = TextAlignment.Center;
                            break;
                        default:
                            break;
                    }

                    //SetFormRotateTransform(textui, annotData);
                    // Set the width and height of the TextBox, rotation, and other control position information
                    RotateTransform rotateTrans = new RotateTransform();
                    rotateTrans.Angle = -90 * annotData.Rotation;
                    rotateTrans.CenterX = annotData.PaintRect.Width / 2;
                    rotateTrans.CenterY = annotData.PaintRect.Height / 2;
                    Rect rotateRect = rotateTrans.TransformBounds(annotData.PaintRect);

                    textBorder.Width = rotateRect.Width;
                    textui.MinHeight = Math.Max(0, textui.FontSize + 8);
                    textui.MaxHeight = Math.Max(0, annotData.PaintOffset.Bottom - annotData.PaintRect.Top - 8);
                    textBorder.SetValue(Canvas.LeftProperty, annotData.PaintRect.Left + rotateTrans.CenterX - rotateRect.Width / 2);
                    textBorder.SetValue(Canvas.TopProperty, annotData.PaintRect.Top + rotateTrans.CenterY - rotateRect.Height / 2);

                    rotateTrans.Angle = 90 * annotData.Rotation;
                    rotateTrans.CenterX = rotateRect.Width / 2;
                    rotateTrans.CenterY = rotateRect.Height / 2;
                    textBorder.RenderTransform = rotateTrans;
                }
            }
            catch
            {

            }
        }

        public void RemovePopTextUI()
        {
            if (textBorder == null)
            {
                return;
            }
            BaseLayer removeLayer = PDFViewer?.GetView(CreateAnnotTag) as CreateAnnotTool;
            removeLayer.Children.Remove(textBorder);
        }

        public bool HitTestBorder()
        {
            if (textBorder == null)
            {
                return false;
            }

            Point pagePosition = Mouse.GetPosition(textBorder);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(textBorder, pagePosition);
            if (hitTestResult != null && hitTestResult.VisualHit != null)
            {
                return true;
            }
            return false;
        }
    }
}
