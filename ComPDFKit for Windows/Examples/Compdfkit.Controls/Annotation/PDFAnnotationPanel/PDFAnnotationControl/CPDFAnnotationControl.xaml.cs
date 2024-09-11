using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControlUI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ComPDFKit.Controls.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using System.IO;
using System.Windows.Ink;
using ComPDFKit.Controls.Annotation.PDFAnnotationUI;
using Microsoft.Win32;
using System.Windows.Media;
using ComPDFKit.Controls.Properties;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Helper;
using ComPDFKit.Viewer.Helper;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFAnnotationControl : UserControl
    {

        private bool isTempPanel = false;

        private CPDFAnnotationType currentAnnotationType = CPDFAnnotationType.Unknown;

        private PDFViewControl pdfViewerControl;

        private UIElement annotationPanel = null;
        private UIElement tempAnnotationPanel = null;

        private CPDFMarkupUI pdfHighlightUI;
        private CPDFMarkupUI pdfUnderlineUI;
        private CPDFMarkupUI pdfSquigglyUI;
        private CPDFMarkupUI pdfStrikeoutUI;
        private CPDFShapeUI pdfSquareUI;
        private CPDFShapeUI pdfCircleUI;
        private CPDFShapeUI pdfLineUI;
        private CPDFShapeUI pdfArrowUI;
        private CPDFFreehandUI pdfFreehandUI;
        private CPDFFreeTextUI pdfFreeTextUI;
        private CPDFNoteUI pdfNoteUI;
        private CPDFStampUI pdfStampUI;
        private CPDFSignatureUI pdfSignatureUI;
        private CPDFLinkUI pdfLinkUI;
        private bool disableClean;
        public event EventHandler ClearAnnotationBar;

        public CPDFAnnotationControl()
        {
            InitializeComponent();
        }

        public void SetPDFViewer(PDFViewControl pdfViewer)
        {
            if (this.pdfViewerControl != null)
            {
                UnLoadPDFViewHandler();
            }
            this.pdfViewerControl = pdfViewer;
            LoadPDFViewHandler();
        }

        public void LoadPDFViewHandler()
        {
            if (pdfViewerControl != null)
            {
                pdfViewerControl.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
                pdfViewerControl.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;
                pdfViewerControl.MouseLeftButtonUpHandler -= PdfViewerControl_MouseLeftButtonUpHandler;
                pdfViewerControl.MouseLeftButtonUpHandler += PdfViewerControl_MouseLeftButtonUpHandler;
                pdfViewerControl.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
                pdfViewerControl.MouseRightButtonDownHandler += PDFViewControl_MouseRightButtonDownHandler;
            }
        }

        public void UnLoadPDFViewHandler()
        {
            if (pdfViewerControl != null)
            {
                pdfViewerControl.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
                pdfViewerControl.MouseLeftButtonUpHandler -= PdfViewerControl_MouseLeftButtonUpHandler;
                pdfViewerControl.MouseRightButtonDownHandler -= PDFViewControl_MouseRightButtonDownHandler;
            }
        }

        private void PDFViewControl_MouseRightButtonDownHandler(object sender, MouseEventObject e)
        {
            //throw new NotImplementedException();
        }

        private void PdfViewerControl_MouseLeftButtonUpHandler(object sender, MouseEventObject e)
        {
            if (e.IsCreate)
            {
                if (currentAnnotationType == CPDFAnnotationType.Image || currentAnnotationType == CPDFAnnotationType.Stamp || currentAnnotationType == CPDFAnnotationType.Signature)
                {
                    pdfViewerControl.SetToolType(ToolType.Pan);
                    pdfViewerControl.SetIsVisibleCustomMouse(false);
                    pdfViewerControl.SetIsShowStampMouse(false);
                }
            }
        }

        private void AnnotationControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPDFViewHandler();
        }

        private void AnnotationControl_Unloaded(object sender, RoutedEventArgs e)
        {
             UnLoadPDFViewHandler();
        }

        private UIElement GetAnnotationPanel()
        {
            return AnnotationPanelContainer.Child;
        }

        private void SetAnnotationPanel(UIElement newChild)
        {
            AnnotationPanelContainer.Child = newChild;
        }

        private void ExpandPanel()
        {
            AnnotationPanelContainer.Visibility = Visibility.Visible;
        }

        private void ChangeAnnotationData()
        {
            switch (currentAnnotationType)
            {
                case CPDFAnnotationType.Highlight:
                case CPDFAnnotationType.Underline:
                case CPDFAnnotationType.Squiggly:
                case CPDFAnnotationType.Strikeout:
                    (annotationPanel as CPDFMarkupUI).PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                    (annotationPanel as CPDFMarkupUI).PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                    SetAnnotationProperty((annotationPanel as CPDFMarkupUI).GetMarkupData());
                    break;
                case CPDFAnnotationType.Square:
                case CPDFAnnotationType.Circle:
                case CPDFAnnotationType.Line:
                case CPDFAnnotationType.Arrow:
                    (annotationPanel as CPDFShapeUI).PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                    (annotationPanel as CPDFShapeUI).PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                    SetAnnotationProperty((annotationPanel as CPDFShapeUI).GetShapeData());
                    break;
                case CPDFAnnotationType.Note:
                    (annotationPanel as CPDFNoteUI).PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                    (annotationPanel as CPDFNoteUI).PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                    SetAnnotationProperty((annotationPanel as CPDFNoteUI).GetNoteData());
                    break;
                case CPDFAnnotationType.Freehand:
                    (annotationPanel as CPDFFreehandUI).PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                    (annotationPanel as CPDFFreehandUI).PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                    SetAnnotationProperty((annotationPanel as CPDFFreehandUI).GetFreehandData());
                    break;
                case CPDFAnnotationType.FreeText:
                    (annotationPanel as CPDFFreeTextUI).PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                    (annotationPanel as CPDFFreeTextUI).PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                    SetAnnotationProperty((annotationPanel as CPDFFreeTextUI).GetFreeTextData());
                    break;
                case CPDFAnnotationType.Stamp:
                    (annotationPanel as CPDFStampUI).PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                    (annotationPanel as CPDFStampUI).PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                    break;
                case CPDFAnnotationType.Signature:
                    (annotationPanel as CPDFSignatureUI).PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                    (annotationPanel as CPDFSignatureUI).PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                    break;
                default:
                    break;
            }
        }

        private void SetAnnotationProperty(CPDFAnnotationData pdfAnnotationData = null)
        {
            if (pdfAnnotationData == null)
            {
                pdfViewerControl.SetToolType(ToolType.Pan);
                pdfViewerControl.SetIsShowStampMouse(false);
                pdfViewerControl.SetIsVisibleCustomMouse(false);
                return;
            }

            CPDFAnnotationType annotationType = pdfAnnotationData.AnnotationType;
            AnnotParam annotHandlerEventArgs = null;
            pdfViewerControl.SetToolType(ToolType.CreateAnnot);
            switch (annotationType)
            {
                case CPDFAnnotationType.Highlight:
                    {
                        CPDFMarkupData highlightData = pdfAnnotationData as CPDFMarkupData;
                        annotHandlerEventArgs = new HighlightParam();

                        byte[] Color = new byte[] { highlightData.Color.R, highlightData.Color.G, highlightData.Color.B };
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT;
                        (annotHandlerEventArgs as HighlightParam).HighlightColor = Color;
                        (annotHandlerEventArgs as HighlightParam).Transparency = Convert.ToByte(highlightData.Opacity * 255);
                        (annotHandlerEventArgs as HighlightParam).Content = highlightData.Note;
                        (annotHandlerEventArgs as HighlightParam).Author = CPDFMarkupData.Author;
                        (annotHandlerEventArgs as HighlightParam).Locked = highlightData.IsLocked;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT);
                    }
                    break;

                case CPDFAnnotationType.Underline:
                    {
                        CPDFMarkupData underlineData = pdfAnnotationData as CPDFMarkupData;
                        annotHandlerEventArgs = new UnderlineParam();
                        byte[] Color = new byte[] { underlineData.Color.R, underlineData.Color.G, underlineData.Color.B };
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE;
                        (annotHandlerEventArgs as UnderlineParam).UnderlineColor = Color;
                        (annotHandlerEventArgs as UnderlineParam).Transparency = Convert.ToByte(underlineData.Opacity * 255);
                        (annotHandlerEventArgs as UnderlineParam).Author = CPDFMarkupData.Author;
                        (annotHandlerEventArgs as UnderlineParam).Content = underlineData.Note;
                        (annotHandlerEventArgs as UnderlineParam).Locked = underlineData.IsLocked;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE);
                    }
                    break;

                case CPDFAnnotationType.Strikeout:
                    {
                        CPDFMarkupData strikeoutData = pdfAnnotationData as CPDFMarkupData;
                        annotHandlerEventArgs = new StrikeoutParam();
                        byte[] Color = new byte[] { strikeoutData.Color.R, strikeoutData.Color.G, strikeoutData.Color.B };
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT;
                        (annotHandlerEventArgs as StrikeoutParam).StrikeoutColor = Color;
                        (annotHandlerEventArgs as StrikeoutParam).Transparency = Convert.ToByte(strikeoutData.Opacity * 255);
                        (annotHandlerEventArgs as StrikeoutParam).Locked = strikeoutData.IsLocked;
                        (annotHandlerEventArgs as StrikeoutParam).Author = CPDFMarkupData.Author;
                        (annotHandlerEventArgs as StrikeoutParam).Content = strikeoutData.Note;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT);
                    }
                    break;

                case CPDFAnnotationType.Squiggly:
                    {
                        CPDFMarkupData squigglyData = pdfAnnotationData as CPDFMarkupData;
                        annotHandlerEventArgs = new SquigglyParam();
                        byte[] Color = new byte[] { squigglyData.Color.R, squigglyData.Color.G, squigglyData.Color.B };
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY;
                        (annotHandlerEventArgs as SquigglyParam).SquigglyColor = Color;
                        (annotHandlerEventArgs as SquigglyParam).Transparency = Convert.ToByte(squigglyData.Opacity * 255);
                        (annotHandlerEventArgs as SquigglyParam).Locked = squigglyData.IsLocked;
                        (annotHandlerEventArgs as SquigglyParam).Author = CPDFMarkupData.Author;
                        (annotHandlerEventArgs as SquigglyParam).Content = squigglyData.Note;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY);
                    }
                    break;

                case CPDFAnnotationType.Square:
                    {
                        CPDFShapeData squareData = pdfAnnotationData as CPDFShapeData;
                        annotHandlerEventArgs = new SquareParam();
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE;
                        byte[] LineColor = new byte[] { squareData.BorderColor.R, squareData.BorderColor.G, squareData.BorderColor.B };
                        (annotHandlerEventArgs as SquareParam).LineColor = LineColor;
                        if (squareData.FillColor != Colors.Transparent)
                        {
                            byte[] FillColor = new byte[] { squareData.FillColor.R, squareData.FillColor.G, squareData.FillColor.B };
                            (annotHandlerEventArgs as SquareParam).BgColor = FillColor;
                            (annotHandlerEventArgs as SquareParam).HasBgColor = true;
                        }
                        (annotHandlerEventArgs as SquareParam).LineWidth = squareData.Thickness;
                        (annotHandlerEventArgs as SquareParam).Transparency = Convert.ToByte(squareData.Opacity * 255);
                        ParamConverter.ParseDashStyle(squareData.DashStyle, out float[] LineDash, out C_BORDER_STYLE BorderStyle);
                        (annotHandlerEventArgs as SquareParam).LineDash = LineDash;
                        (annotHandlerEventArgs as SquareParam).BorderStyle = BorderStyle;
                        (annotHandlerEventArgs as SquareParam).Author = CPDFMarkupData.Author;
                        (annotHandlerEventArgs as SquareParam).Content = squareData.Note;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE);
                    }
                    break;

                case CPDFAnnotationType.Circle:
                    {
                        CPDFShapeData cicleData = pdfAnnotationData as CPDFShapeData;
                        annotHandlerEventArgs = new CircleParam();
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE;
                        byte[] LineColor = new byte[] { cicleData.BorderColor.R, cicleData.BorderColor.G, cicleData.BorderColor.B };
                        (annotHandlerEventArgs as CircleParam).LineColor = LineColor;
                        if (cicleData.FillColor != Colors.Transparent)
                        {
                            byte[] BgColor = new byte[] { cicleData.FillColor.R, cicleData.FillColor.G, cicleData.FillColor.B };
                            (annotHandlerEventArgs as CircleParam).BgColor = BgColor;
                            (annotHandlerEventArgs as CircleParam).HasBgColor = true;
                        }
                        (annotHandlerEventArgs as CircleParam).LineWidth = cicleData.Thickness;
                        (annotHandlerEventArgs as CircleParam).Transparency = Convert.ToByte(cicleData.Opacity * 255);
                        ParamConverter.ParseDashStyle(cicleData.DashStyle, out float[] LineDash, out C_BORDER_STYLE BorderStyle);
                        (annotHandlerEventArgs as CircleParam).LineDash = LineDash;
                        (annotHandlerEventArgs as CircleParam).BorderStyle = BorderStyle;
                        (annotHandlerEventArgs as CircleParam).Author = CPDFMarkupData.Author;
                        (annotHandlerEventArgs as CircleParam).Content = cicleData.Note;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE);
                    }
                    break;

                case CPDFAnnotationType.Arrow:
                case CPDFAnnotationType.Line:
                    {
                        CPDFLineShapeData lineData = pdfAnnotationData as CPDFLineShapeData;
                        annotHandlerEventArgs = new LineParam();
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_LINE;
                        byte[] LineColor = new byte[] { lineData.BorderColor.R, lineData.BorderColor.G, lineData.BorderColor.B };
                        (annotHandlerEventArgs as LineParam).LineColor = LineColor;
                        (annotHandlerEventArgs as LineParam).LineWidth = lineData.Thickness;
                        (annotHandlerEventArgs as LineParam).Transparency = Convert.ToByte(lineData.Opacity * 255);
                        ParamConverter.ParseDashStyle(lineData.DashStyle, out float[] LineDash, out C_BORDER_STYLE BorderStyle);
                        (annotHandlerEventArgs as LineParam).LineDash = LineDash;
                        (annotHandlerEventArgs as LineParam).BorderStyle = BorderStyle;
                        (annotHandlerEventArgs as LineParam).HeadLineType = lineData.LineType.HeadLineType;
                        (annotHandlerEventArgs as LineParam).TailLineType = lineData.LineType.TailLineType;
                        (annotHandlerEventArgs as LineParam).Author = CPDFMarkupData.Author;
                        (annotHandlerEventArgs as LineParam).Content = lineData.Note;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_LINE);
                    }
                    break;

                case CPDFAnnotationType.Note:
                    {
                        CPDFNoteData noteData = pdfAnnotationData as CPDFNoteData;
                        annotHandlerEventArgs = new StickyNoteParam();
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_TEXT;
                        byte[] StickyNoteColor = new byte[] { noteData.BorderColor.R, noteData.BorderColor.G, noteData.BorderColor.B };
                        (annotHandlerEventArgs as StickyNoteParam).StickyNoteColor = StickyNoteColor;
                        (annotHandlerEventArgs as StickyNoteParam).Content = noteData.Note;
                        (annotHandlerEventArgs as StickyNoteParam).Transparency = 255;
                        (annotHandlerEventArgs as StickyNoteParam).Author = CPDFMarkupData.Author;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_TEXT);
                    }
                    break;

                case CPDFAnnotationType.Freehand:
                    {
                        CPDFFreehandData freehandData = pdfAnnotationData as CPDFFreehandData;
                        annotHandlerEventArgs = new InkParam();
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_INK;
                        byte[] LineColor = new byte[] { freehandData.BorderColor.R, freehandData.BorderColor.G, freehandData.BorderColor.B };
                        (annotHandlerEventArgs as InkParam).InkColor = LineColor;
                        (annotHandlerEventArgs as InkParam).Thickness = freehandData.Thickness;
                        (annotHandlerEventArgs as InkParam).Transparency = Convert.ToByte(freehandData.Opacity * 255);
                        (annotHandlerEventArgs as InkParam).Content = freehandData.Note;
                        (annotHandlerEventArgs as InkParam).Author = CPDFMarkupData.Author;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_INK);
                    }
                    break;

                case CPDFAnnotationType.FreeText:
                    {
                        CPDFFreeTextData freeTextData = pdfAnnotationData as CPDFFreeTextData;
                        annotHandlerEventArgs = new FreeTextParam();
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT;
                        (annotHandlerEventArgs as FreeTextParam).Transparency = Convert.ToByte(freeTextData.Opacity * 255);
                        (annotHandlerEventArgs as FreeTextParam).FontName = freeTextData.FontFamily.ToString();

                        byte[] FontColor = new byte[] { freeTextData.BorderColor.R, freeTextData.BorderColor.G, freeTextData.BorderColor.B };
                        (annotHandlerEventArgs as FreeTextParam).FontColor = FontColor;
                        (annotHandlerEventArgs as FreeTextParam).FontSize = freeTextData.FontSize; 

                        string postScriptName = string.Empty;
                        CPDFFont.GetPostScriptName(pdfFreeTextUI.CPDFFontControl.FontFamilyValue, pdfFreeTextUI.CPDFFontControl.FontStyleValue, ref postScriptName);
                        (annotHandlerEventArgs as FreeTextParam).FontName = postScriptName;
                        switch (freeTextData.TextAlignment)
                        {
                            case TextAlignment.Left:
                                (annotHandlerEventArgs as FreeTextParam).Alignment = C_TEXT_ALIGNMENT.ALIGNMENT_LEFT;
                                break;
                            case TextAlignment.Right:
                                (annotHandlerEventArgs as FreeTextParam).Alignment = C_TEXT_ALIGNMENT.ALIGNMENT_RIGHT;
                                break;
                            case TextAlignment.Center:
                                (annotHandlerEventArgs as FreeTextParam).Alignment = C_TEXT_ALIGNMENT.ALIGNMENT_CENTER;
                                break;
                            default:
                                break;
                        }
                        (annotHandlerEventArgs as FreeTextParam).Content = freeTextData.Note;
                        (annotHandlerEventArgs as FreeTextParam).Author = CPDFMarkupData.Author;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT);
                    }
                    break;

                case CPDFAnnotationType.Stamp:
                    {
                        StampParam stampParam = new StampParam();
                        stampParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_STAMP;
                        CPDFStampData stampData = pdfAnnotationData as CPDFStampData;
                        SetStamp(ref stampParam, stampData);
                        annotHandlerEventArgs = stampParam;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP);

                        byte[] imageData = null;
                        int imageWidth = 0;
                        int imageHeight = 0;
                        PDFHelp.ImageStreamToByte(stampParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);
                        if (imageData != null && imageWidth > 0 && imageHeight > 0)
                        {
                            pdfViewerControl.PDFViewTool.GetCPDFViewer().SetMouseImageMaxSize(200, 300);
                            pdfViewerControl.SetStampMouseImage(imageData, imageWidth, imageHeight);
                        }

                        pdfViewerControl.SetIsVisibleCustomMouse(true);
                        pdfViewerControl.SetIsShowStampMouse(true);
                    }
                    break;
                case CPDFAnnotationType.Signature:
                    {
                        AnnotParam signatureParam = new AnnotParam();
                        CPDFSignatureData SignatureData = pdfAnnotationData as CPDFSignatureData;
                        SetSignature(ref signatureParam, SignatureData);
                        switch (signatureParam.CurrentType)
                        {
                            case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                                {
                                    StampParam stampParam = (StampParam)signatureParam;
                                    stampParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_STAMP;

                                    byte[] imageData = null;
                                    int imageWidth = 0;
                                    int imageHeight = 0;
                                    PDFHelp.ImageStreamToByte(stampParam.ImageStream, ref imageData, ref imageWidth, ref imageHeight);
                                    if (imageData != null && imageWidth > 0 && imageHeight > 0)
                                    {
                                        pdfViewerControl.PDFViewTool.GetCPDFViewer().SetMouseImageMaxSize(200, 300);
                                        pdfViewerControl.SetStampMouseImage(imageData, imageWidth, imageHeight);
                                        pdfViewerControl.SetIsVisibleCustomMouse(true);
                                        pdfViewerControl.SetIsShowStampMouse(true);
                                        annotHandlerEventArgs = signatureParam;
                                    }
                                }
                                break;
                            case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                                {
                                    WriteableBitmap writeableBitmap = CreateInkImage(signatureParam as InkParam);
                                    byte[] imageArray = new byte[writeableBitmap.PixelWidth * writeableBitmap.PixelHeight * 4];
                                    writeableBitmap.CopyPixels(imageArray, writeableBitmap.PixelWidth * 4, 0);
                                    pdfViewerControl.PDFViewTool.GetCPDFViewer().SetMouseImageMaxSize(writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
                                    pdfViewerControl.SetStampMouseImage(imageArray, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
                                    pdfViewerControl.SetIsVisibleCustomMouse(true);
                                    pdfViewerControl.SetIsShowStampMouse(true);
                                    annotHandlerEventArgs = signatureParam;
                                }
                                break;
                            default:
                                return;
                        }
                    }
                    break;
                case CPDFAnnotationType.Link:
                    if (annotHandlerEventArgs != null)
                    {
                        annotHandlerEventArgs.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_LINK;
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_LINK);
                    }
                    break;
                case CPDFAnnotationType.Unknown:
                    pdfViewerControl.SetToolType(ToolType.Pan);
                    return;
                default:
                    break;
            }
            pdfViewerControl.SetToolType(ToolType.CreateAnnot);
            if (annotationType != CPDFAnnotationType.Stamp && annotationType != CPDFAnnotationType.Signature && annotationType != CPDFAnnotationType.Image)
            {
                pdfViewerControl.SetIsShowStampMouse(false);
            }
            pdfViewerControl.SetAnnotParam(annotHandlerEventArgs);
        }

        private WriteableBitmap CreateInkImage(InkParam inkParam)
        {
            if (inkParam == null)
            {
                return null;
            }

            if (inkParam.InkPath != null && inkParam.InkPath.Count > 0)
            {
                GeometryGroup PaintGeomtry = new GeometryGroup();
                int minLeft = -1;
                int minTop = -1;
                int maxLeft = -1;
                int maxTop = -1;

                foreach (List<CPoint> Item in inkParam.InkPath)
                {
                    for (int i = 0; i < Item.Count; i++)
                    {
                        CPoint paintPoint = Item[i];

                        if (minLeft == -1)
                        {
                            minLeft = (int)paintPoint.x;
                            maxLeft = (int)paintPoint.x;
                            minTop = (int)paintPoint.y;
                            maxTop = (int)paintPoint.y;
                        }
                        else
                        {
                            minLeft = (int)Math.Min(minLeft, paintPoint.x);
                            maxLeft = (int)Math.Max(maxLeft, paintPoint.x);
                            minTop = (int)Math.Min(minTop, paintPoint.y);
                            maxTop = (int)Math.Max(maxTop, paintPoint.y);
                        }
                    }
                }

                if (minLeft >= 0 && maxLeft > minLeft && minTop >= 0 && maxTop > minTop)
                {
                    List<List<CPoint>> points = new List<List<CPoint>>();
                    foreach (List<CPoint> Item in inkParam.InkPath)
                    {
                        PathGeometry PaintPath = new PathGeometry();
                        PathFigureCollection Figures = new PathFigureCollection();
                        PathFigure AddFigure = new PathFigure();
                        Figures.Add(AddFigure);
                        PaintPath.Figures = Figures;
                        PaintGeomtry.Children.Add(PaintPath);

                        List<CPoint> changeList = new List<CPoint>();
                        for (int i = 0; i < Item.Count; i++)
                        {
                            Point paintPoint = new Point(DpiHelper.PDFNumToStandardNum(Item[i].x - minLeft), DpiHelper.PDFNumToStandardNum(Item[i].y - minTop));
                            changeList.Add(DataConversionForWPF.PointConversionForCPoint(DpiHelper.StandardPointToPDFPoint(paintPoint)));
                            if (i == 0)
                            {
                                AddFigure.StartPoint = paintPoint;
                            }
                            else
                            {
                                LineSegment AddSegment = new LineSegment();
                                AddSegment.Point = paintPoint;
                                AddFigure.Segments.Add(AddSegment);
                            }
                        }
                        if (changeList.Count > 0)
                        {
                            points.Add(changeList);
                        }
                    }

                    int drawWidth = (int)DpiHelper.PDFNumToStandardNum(maxLeft - minLeft);
                    int drawHeight = (int)DpiHelper.PDFNumToStandardNum(maxTop - minTop);

                    inkParam.InkPath = points;
                    DefaultSettingParam defaultSettingParam = pdfViewerControl.PDFViewTool.GetDefaultSettingParam();
                    defaultSettingParam.SetAnnotParam(inkParam);
                    DrawingVisual copyVisual = new DrawingVisual();
                    DrawingContext copyContext = copyVisual.RenderOpen();

                    Color color = ParamConverter.ConverterByteForColor(inkParam.InkColor);
                    color.A = inkParam.Transparency;
                    Pen drawPen = new Pen(new SolidColorBrush(color), inkParam.Thickness);
                    copyContext?.DrawGeometry(null, drawPen, PaintGeomtry);
                    copyContext.Close();
                    RenderTargetBitmap targetBitmap = new RenderTargetBitmap(drawWidth, drawHeight, 96, 96, PixelFormats.Pbgra32);
                    targetBitmap.Render(copyVisual);
                    byte[] ImageArray = new byte[targetBitmap.PixelWidth * targetBitmap.PixelHeight * 4];
                    targetBitmap.CopyPixels(new Int32Rect(0, 0, (int)targetBitmap.PixelWidth, (int)targetBitmap.PixelHeight), ImageArray, targetBitmap.PixelWidth * 4, 0);

                    WriteableBitmap writeBitmap = new WriteableBitmap(targetBitmap.PixelWidth, targetBitmap.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
                    writeBitmap.WritePixels(new Int32Rect(0, 0, targetBitmap.PixelWidth, targetBitmap.PixelHeight), ImageArray, targetBitmap.PixelWidth * 4, 0);
                    return writeBitmap;
                }
            }
            return null;
        }

        public void SetSignature(ref AnnotParam annotParam, CPDFSignatureData stamp)
        {
            switch (stamp.Type)
            {
                case SignatureType.TextType:
                case SignatureType.ImageType:
                    {
                        annotParam = new StampParam();

                        annotParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_STAMP;
                        (annotParam as StampParam).Transparency = 255;
                        (annotParam as StampParam).StampType = C_STAMP_TYPE.IMAGE_STAMP;
                        if (!string.IsNullOrEmpty(stamp.SourcePath) && File.Exists(stamp.SourcePath))
                        {
                            BitmapImage image = new BitmapImage(new Uri(stamp.SourcePath));
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));
                            MemoryStream memoryStream = new MemoryStream();
                            encoder.Save(memoryStream);
                            (annotParam as StampParam).ImageStream = memoryStream;
                        }

                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP);
                    }
                    break;
                case SignatureType.Drawing:
                    {
                        annotParam = new InkParam();
                        (annotParam as InkParam).Transparency = stamp.inkColor.A;
                        annotParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_INK;
                        List<List<Point>> RawPointList = GetPoints(stamp.DrawingPath);

                        if (RawPointList != null && RawPointList.Count > 0)
                        {
                            List<List<CPoint>> inkPath = new List<List<CPoint>>();
                            foreach (List<Point> inkPoints in RawPointList)
                            {
                                List<CPoint> ink = new List<CPoint>();
                                foreach (Point point in inkPoints)
                                {
                                    ink.Add(new CPoint((float)DpiHelper.StandardNumToPDFNum(point.X), (float)DpiHelper.StandardNumToPDFNum(point.Y)));
                                }

                                if (ink.Count > 0)
                                {
                                    inkPath.Add(ink);
                                }
                            }

                            if (inkPath.Count > 0)
                            {
                                (annotParam as InkParam).InkPath = inkPath;
                            }
                        }
                        (annotParam as InkParam).Thickness = stamp.inkThickness;

                        byte[] InkColor = new byte[] { stamp.inkColor.R, stamp.inkColor.G, stamp.inkColor.B };
                        (annotParam as InkParam).InkColor = InkColor;

                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_INK);
                    }
                    break;
                default:
                    break;
            }
        }
        private List<List<Point>> GetPoints(string Path)
        {
            StrokeCollection Strokes;
            List<List<Point>> RawPointList = new List<List<Point>>();
            using (FileStream strokeStream = File.OpenRead(Path))
            {
                Strokes = new StrokeCollection(strokeStream);
            }

            for (int kk = 0; kk < Strokes.Count; kk++)
            {
                List<Point> p = new List<Point>();
                RawPointList.Add(p);
                for (int gg = 0; gg < Strokes[kk].StylusPoints.Count; gg++)
                {
                    var point = Strokes[kk].StylusPoints[gg].ToPoint();

                    if (point.X >= 0 && point.Y >= 0)
                        RawPointList[kk].Add(point);

                }
            }
            return RawPointList;
        }

        private void SetStamp(ref StampParam stampParam, CPDFStampData stamp)
        {
            stampParam.StampText = stamp.StampText;
            stampParam.Author = CPDFMarkupData.Author;
            stampParam.Transparency = Convert.ToByte(stamp.Opacity * 255);
            stampParam.DateText = stamp.StampTextDate;
            stampParam.TextStampColor = stamp.TextColor;
            stampParam.TextStampShape = stamp.TextSharp;
            if (!string.IsNullOrEmpty(stamp.SourcePath))
            {
                BitmapImage image = new BitmapImage(new Uri(stamp.SourcePath));
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                MemoryStream memoryStream = new MemoryStream();
                encoder.Save(memoryStream);
                stampParam.ImageStream = memoryStream;
            }
            else
            {
                var i = pdfStampUI.CustomStampList.IndexOf(stamp);
                Settings.Default.CustomStampList.RemoveAt(i);
                Settings.Default.Save();
                pdfStampUI.LoadSettings();
                return;
            }
            stampParam.StampType = stamp.Type;
        }

        public void AnnotationCancel()
        {
            this.pdfViewerControl.SetToolType(ToolType.Pan);
            pdfViewerControl.SetIsShowStampMouse(false);
            pdfViewerControl.SetIsVisibleCustomMouse(false);
            ClearPanel();
        }

        private void CPDFAnnotationControl_PropertyChanged(object sender, CPDFAnnotationData e)
        {
            SetAnnotationProperty(e);
        }

        public void InitAnnotationPanel(CPDFAnnotationType annotationType)
        {
            switch (annotationType)
            {
                case CPDFAnnotationType.Highlight:
                    if (pdfHighlightUI == null)
                    {
                        pdfHighlightUI = new CPDFMarkupUI();
                    }
                    annotationPanel = pdfHighlightUI;
                    (annotationPanel as CPDFMarkupUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Underline:
                    if (pdfUnderlineUI == null)
                    {
                        pdfUnderlineUI = new CPDFMarkupUI();
                    }
                    annotationPanel = pdfUnderlineUI;
                    (annotationPanel as CPDFMarkupUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Strikeout:
                    if (pdfStrikeoutUI == null)
                    {
                        pdfStrikeoutUI = new CPDFMarkupUI();
                    }
                    annotationPanel = pdfStrikeoutUI;
                    (annotationPanel as CPDFMarkupUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Squiggly:
                    if (pdfSquigglyUI == null)
                    {
                        pdfSquigglyUI = new CPDFMarkupUI();
                    }
                    annotationPanel = pdfSquigglyUI;
                    (annotationPanel as CPDFMarkupUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Square:
                    if (pdfSquareUI == null)
                    {
                        pdfSquareUI = new CPDFShapeUI();
                    }
                    annotationPanel = pdfSquareUI;
                    (annotationPanel as CPDFShapeUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Circle:
                    if (pdfCircleUI == null)
                    {
                        pdfCircleUI = new CPDFShapeUI();
                    }
                    annotationPanel = pdfCircleUI;
                    (annotationPanel as CPDFShapeUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Arrow:
                    if (pdfArrowUI == null)
                    {
                        pdfArrowUI = new CPDFShapeUI();
                    }
                    annotationPanel = pdfArrowUI;
                    (annotationPanel as CPDFShapeUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Line:
                    if (pdfLineUI == null)
                    {
                        pdfLineUI = new CPDFShapeUI();
                    }
                    annotationPanel = pdfLineUI;
                    (annotationPanel as CPDFShapeUI).InitWithAnnotationType(annotationType);
                    break;
                case CPDFAnnotationType.Freehand:
                    if (pdfFreehandUI == null)
                    {
                        pdfFreehandUI = new CPDFFreehandUI();
                        pdfFreehandUI.EraseClickHandler -= PdfFreehandUI_EraseClickHandler;
                        pdfFreehandUI.EraseChangeHandler -= PdfFreehandUI_EraseChangeHandler;
                        pdfFreehandUI.EraseClickHandler += PdfFreehandUI_EraseClickHandler;
                        pdfFreehandUI.EraseChangeHandler += PdfFreehandUI_EraseChangeHandler;
                    }
                    annotationPanel = pdfFreehandUI;
                    break;
                case CPDFAnnotationType.FreeText:
                    if (pdfFreeTextUI == null)
                    {
                        pdfFreeTextUI = new CPDFFreeTextUI();
                    }
                    annotationPanel = pdfFreeTextUI;
                    break;
                case CPDFAnnotationType.Note:
                    if (pdfNoteUI == null)
                    {
                        pdfNoteUI = new CPDFNoteUI();
                    }
                    annotationPanel = pdfNoteUI;
                    break;
                case CPDFAnnotationType.Stamp:
                    if (pdfStampUI == null)
                    {
                        pdfStampUI = new CPDFStampUI();
                    }
                    annotationPanel = pdfStampUI;
                    break;
                case CPDFAnnotationType.Signature:
                    if (pdfSignatureUI == null)
                    {
                        pdfSignatureUI = new CPDFSignatureUI();
                    }
                    annotationPanel = pdfSignatureUI;
                    break;
                case CPDFAnnotationType.Link:
                    if (pdfLinkUI == null)
                    {
                        pdfLinkUI = new CPDFLinkUI();
                    }
                    LinkParam linkAnnotArgs = new LinkParam();
                    if (this.pdfViewerControl != null && pdfViewerControl.PDFViewTool != null && pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument() != null)
                    {
                        pdfViewerControl.SetToolType(ToolType.CreateAnnot);
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_LINK);
                        pdfLinkUI.InitLinkAnnotArgs(linkAnnotArgs, pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument().PageCount);
                    }
                    annotationPanel = pdfLinkUI;
                    break;
                case CPDFAnnotationType.Audio:
                    SoundParam soundParam = new SoundParam();
                    soundParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_SOUND;
                    OpenFileDialog openAudioDialog = new OpenFileDialog();
                    openAudioDialog.Filter = "Wave Files(*.wav)|*.wav|All Files(*.*;)|*.*;";
                    if (openAudioDialog.ShowDialog() == true)
                    {
                        BitmapImage img = new BitmapImage(new Uri("pack://application:,,,/ComPDFKit.Controls;component/Asset/Resource/SoundAnnot.png"));
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(img));
                        MemoryStream memoryStream = new MemoryStream();
                        encoder.Save(memoryStream);

                        soundParam.ImageStream = memoryStream;
                        soundParam.SoundFilePath = openAudioDialog.FileName;
                        pdfViewerControl.SetToolType(ToolType.CreateAnnot);
                        pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_SOUND);
                        pdfViewerControl.SetAnnotParam(soundParam);
                    }
                    else
                    {
                        pdfViewerControl.SetToolType(ToolType.Pan);
                        ClearAnnotationBar?.Invoke(this, EventArgs.Empty);
                    }
                    ClearPanel();
                    break;
                case CPDFAnnotationType.Image:
                    StampParam stampParam = new StampParam();
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        try
                        {
                            using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                PngBitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None));
                                MemoryStream memoryStream = new MemoryStream();
                                encoder.Save(memoryStream);
                                stampParam.ImageStream = memoryStream;
                                stampParam.Transparency = 255;
                                stampParam.CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_STAMP;
                                stampParam.StampType = C_STAMP_TYPE.IMAGE_STAMP;

                                BitmapFrame frame = null;
                                if (encoder != null && encoder.Frames.Count > 0)
                                {
                                    frame = encoder.Frames[0];
                                }
                                if (frame != null)
                                {
                                    byte[] imageArray = new byte[frame.PixelWidth * frame.PixelHeight * 4];
                                    if (frame.Format != PixelFormats.Bgra32)
                                    {
                                        FormatConvertedBitmap covert = new FormatConvertedBitmap(
                                            frame,
                                            PixelFormats.Bgra32,
                                            frame.Palette,
                                            0);
                                        covert.CopyPixels(imageArray, frame.PixelWidth * 4, 0);
                                    }
                                    else
                                    {
                                        frame.CopyPixels(imageArray, frame.PixelWidth * 4, 0);
                                    }

                                    pdfViewerControl.PDFViewTool.GetCPDFViewer().SetMouseImageMaxSize(200, 300);
                                    pdfViewerControl.SetStampMouseImage(imageArray, frame.PixelWidth, frame.PixelHeight);
                                }

                                pdfViewerControl.SetToolType(ToolType.CreateAnnot);
                                pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_STAMP);
                                pdfViewerControl.SetIsVisibleCustomMouse(true);
                                pdfViewerControl.SetIsShowStampMouse(true);
                                pdfViewerControl.SetAnnotParam(stampParam);
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        pdfViewerControl.SetToolType(ToolType.Pan);
                        ClearAnnotationBar?.Invoke(this, EventArgs.Empty);
                    }
                    ClearPanel();
                    break;
                default:
                    break;
            }
        }

        private void PdfFreehandUI_EraseChangeHandler(object sender, double e)
        {

            if (pdfViewerControl != null)
            {
                pdfViewerControl.PDFViewTool.SetEraseZoom(e);
            }
        }

        private void PdfFreehandUI_EraseClickHandler(object sender, bool e)
        {
            if (e)
            {
                //pdfViewerControl.PDFToolManager.ClearSelect();
                pdfViewerControl.PDFToolManager.SetToolType(ToolType.Customize);
            }
            else
            {
                pdfViewerControl.SetToolType(ToolType.CreateAnnot);
                pdfViewerControl.SetCreateAnnotType(C_ANNOTATION_TYPE.C_ANNOTATION_INK);
            }
        }

        public void CreatTempAnnotationPanel(BaseAnnot baseAnnot)
        {
            switch (baseAnnot.CurrentType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                    {
                        tempAnnotationPanel = new CPDFMarkupUI();
                        (tempAnnotationPanel as CPDFMarkupUI).InitWithAnnotationType(CPDFAnnotationType.Highlight);
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFMarkupUI).SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot as CPDFMarkupAnnotation, pdfViewerControl);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                    {
                        tempAnnotationPanel = new CPDFMarkupUI();
                        (tempAnnotationPanel as CPDFMarkupUI).InitWithAnnotationType(CPDFAnnotationType.Underline);
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFMarkupUI).SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot as CPDFMarkupAnnotation, pdfViewerControl);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                    {
                        tempAnnotationPanel = new CPDFMarkupUI();
                        (tempAnnotationPanel as CPDFMarkupUI).InitWithAnnotationType(CPDFAnnotationType.Strikeout);
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFMarkupUI).SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot as CPDFMarkupAnnotation, pdfViewerControl);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                    {
                        tempAnnotationPanel = new CPDFMarkupUI();
                        (tempAnnotationPanel as CPDFMarkupUI).InitWithAnnotationType(CPDFAnnotationType.Squiggly);
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFMarkupUI).SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot as CPDFMarkupAnnotation, pdfViewerControl);
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                    {
                        tempAnnotationPanel = new CPDFShapeUI();
                        (tempAnnotationPanel as CPDFShapeUI).InitWithAnnotationType(CPDFAnnotationType.Square);

                        int page = baseAnnot.GetAnnotData().PageIndex;
                        var annot = baseAnnot.GetAnnotData().Annot;
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFShapeUI).SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot, pdfViewerControl);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                    {
                        tempAnnotationPanel = new CPDFShapeUI();
                        (tempAnnotationPanel as CPDFShapeUI).InitWithAnnotationType(CPDFAnnotationType.Circle);
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFShapeUI).SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot, pdfViewerControl);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    {
                        tempAnnotationPanel = new CPDFShapeUI();
                        (tempAnnotationPanel as CPDFShapeUI).InitWithAnnotationType(CPDFAnnotationType.Line);
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFShapeUI).SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot, pdfViewerControl);
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                    {

                        CPDFFreehandUI tempFreehandPanel = new CPDFFreehandUI();
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        tempFreehandPanel.SetPresentAnnotAttrib(annotParam, baseAnnot.GetAnnotData().Annot, pdfViewerControl);
                        tempFreehandPanel.EraseClickHandler -= PdfFreehandUI_EraseClickHandler;
                        tempFreehandPanel.EraseChangeHandler -= PdfFreehandUI_EraseChangeHandler;
                        tempFreehandPanel.EraseClickHandler += PdfFreehandUI_EraseClickHandler;
                        tempFreehandPanel.EraseChangeHandler += PdfFreehandUI_EraseChangeHandler;
                        tempAnnotationPanel = tempFreehandPanel;
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                    {
                        tempAnnotationPanel = new CPDFFreeTextUI();
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFFreeTextUI).SetPresentAnnotAttrib(annotParam as FreeTextParam, baseAnnot.GetAnnotData().Annot as CPDFFreeTextAnnotation, pdfViewerControl);
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                    {
                        tempAnnotationPanel = new CPDFNoteUI();
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFNoteUI).SetPresentAnnotAttrib(annotParam as StickyNoteParam, baseAnnot.GetAnnotData().Annot as CPDFTextAnnotation, pdfViewerControl);
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                    {
                        tempAnnotationPanel = new CPDFTempStampUI();
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFTempStampUI).SetPresentAnnotAttrib(annotParam as StampParam, baseAnnot.GetAnnotData().Annot as CPDFStampAnnotation, pdfViewerControl.PDFToolManager.GetDocument(), pdfViewerControl);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                    {
                        tempAnnotationPanel = new CPDFLinkUI();
                        AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                                                               pdfViewerControl.PDFViewTool.GetCPDFViewer().GetDocument(),
                                                               baseAnnot.GetAnnotData().PageIndex,
                                                               baseAnnot.GetAnnotData().Annot);
                        (tempAnnotationPanel as CPDFLinkUI).SetPresentAnnotAttrib(annotParam as LinkParam, baseAnnot.GetAnnotData().Annot as CPDFLinkAnnotation, pdfViewerControl, pdfViewerControl.PDFToolManager.GetDocument().PageCount);
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                    tempAnnotationPanel = null;
                    break;
                default:
                    break;
            }
        }

        public void LoadAnnotationPanel(CPDFAnnotationType annotationType)
        {
            this.pdfViewerControl.SetToolType(ToolType.Pan);
            currentAnnotationType = annotationType;
            annotationPanel = GetAnnotationPanel();
            InitAnnotationPanel(annotationType);
            ShowCurrentAnnotPanel();
        }

        private void ShowCurrentAnnotPanel()
        {
            if (annotationPanel != null)
            {
                if (annotationPanel is CPDFFreehandUI)
                {
                    if(pdfViewerControl.PDFToolManager.GetToolType() == ToolType.Customize)
                    {
                        (annotationPanel as CPDFFreehandUI)?.SetEraseCheck(true);
                        return;
                    }
                    else
                    {
                        (annotationPanel as CPDFFreehandUI)?.SetEraseCheck(false);
                    }                    
                }

                SetAnnotationPanel(annotationPanel);
                ExpandPanel();
                ChangeAnnotationData();
                EmptyMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyMessage.Visibility = Visibility.Visible;
                SetAnnotationPanel(null);
                if (pdfViewerControl != null)
                {
                    EmptyMessage.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowTempAnnotPanel()
        {
            BaseAnnot baseAnnot = pdfViewerControl.GetCacheHitTestAnnot();
            if (baseAnnot != null)
            {
                CreatTempAnnotationPanel(baseAnnot);
                SetAnnotationPanel(tempAnnotationPanel);
                ExpandPanel();
                if (tempAnnotationPanel != null)
                {
                    EmptyMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyMessage.Visibility = Visibility.Visible;
                }
            }
        }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            SetAnnotEventData();
        }

        public void ClearPanel()
        {
            annotationPanel = null;
            SetAnnotationPanel(annotationPanel);
            EmptyMessage.Visibility = Visibility.Visible;
        }

        public void SetAnnotEventData()
        {
            if (pdfViewerControl.GetCacheHitTestAnnot() != null)
            {
                ShowTempAnnotPanel();
                isTempPanel = true;
            }
            else
            {
                if (pdfViewerControl != null && (pdfViewerControl.PDFToolManager.GetToolType() == ToolType.CreateAnnot || pdfViewerControl.PDFToolManager.GetToolType() == ToolType.Customize))
                {
                    ShowCurrentAnnotPanel();
                    isTempPanel = false;
                }
                else if (annotationPanel is CPDFStampUI && currentAnnotationType == CPDFAnnotationType.Stamp)
                {
                    ShowCurrentAnnotPanel();
                }
                else if (annotationPanel is CPDFSignatureUI && currentAnnotationType == CPDFAnnotationType.Signature)
                {
                    ShowCurrentAnnotPanel();
                }
                else if (disableClean == false)
                {
                    ClearPanel();
                }
            }
        }
    }
}
