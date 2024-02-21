using Compdfkit_Tools.Data;
using Compdfkit_Tools.PDFControlUI;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI;
using System.IO;
using System.Windows.Ink;
using Compdfkit_Tools.Annotation.PDFAnnotationUI;
using Microsoft.Win32;
using System.Windows.Media;
using Compdfkit_Tools.Properties;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFAnnotationControl : UserControl
    {

        private bool isTempPanel = false;

        private CPDFAnnotationType currentAnnotationType = CPDFAnnotationType.Unknown;

        private CPDFViewer pdfViewer;

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
        private AnnotHandlerEventArgs annotArgs;
        private EraseArgs eraseArgs;
        private bool disableClean;
        public event EventHandler ClearAnnotationBar;

        public CPDFAnnotationControl()
        {
            InitializeComponent();
        }

        public void SetPDFViewer(CPDFViewer pdfViewer)
        {
            if (this.pdfViewer != null)
            {
                UnLoadPDFViewHandler();
            }
            this.pdfViewer = pdfViewer;
            LoadPDFViewHandler();
        }

        public void LoadPDFViewHandler()
        {
            if (this.pdfViewer != null)
            {
                this.pdfViewer.AnnotActiveHandler -= PDFViewer_AnnotActiveHandler;
                this.pdfViewer.AnnotActiveHandler += PDFViewer_AnnotActiveHandler;
                this.pdfViewer.AnnotEditHandler -= PdfViewer_AnnotEditHandler;
                this.pdfViewer.AnnotEditHandler += PdfViewer_AnnotEditHandler;
            }
        }

        private void PdfViewer_AnnotEditHandler(object sender, List<AnnotEditEvent> e)
        {
            if (e != null && e.Count > 0)
            {
                if (e[0].EditAction == ActionType.Del)
                {
                    if (pdfViewer.MouseMode == MouseModes.AnnotCreate && pdfViewer.ToolManager.CurrentAnnotArgs is EraseArgs)
                    {
                        return;
                    }
                    SetAnnotEventData(null);
                }

                if (e[0].EditAction == ActionType.Modify && e[0].EditAnnotArgs != null && e[0].EditAnnotArgs.EventType == AnnotArgsType.AnnotSticky)
                {
                    CPDFNoteUI tempUI = annotationPanel as CPDFNoteUI;
                    if (tempUI == null || tempUI.annotAttribEvent == null)
                    {
                        tempUI = tempAnnotationPanel as CPDFNoteUI;
                    }
                    if (tempUI == null || tempUI.annotAttribEvent == null)
                    {
                        tempUI = pdfNoteUI;
                    }
                    if (tempUI != null && tempUI.annotAttribEvent != null)
                    {
                        AnnotAttribEvent oldEvent = tempUI.annotAttribEvent;
                        oldEvent.Attribs[AnnotAttrib.NoteText] = e[0].EditAnnotArgs.Content;
                        tempUI.SetPresentAnnotAttrib(oldEvent);
                    }
                }
                if (e[0].EditAction == ActionType.Add && e[0].EditAnnotArgs.EventType == AnnotArgsType.AnnotStamp)
                {
                    pdfViewer.SetMouseMode(MouseModes.PanTool);
                }
            }
        }

        public void UnLoadPDFViewHandler()
        {
            if (this.pdfViewer != null)
            {
                this.pdfViewer.AnnotActiveHandler -= PDFViewer_AnnotActiveHandler;

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
                pdfViewer.SetMouseMode(MouseModes.PanTool);
                return;
            }

            CPDFAnnotationType annotationType = pdfAnnotationData.AnnotationType;
            AnnotHandlerEventArgs annotHandlerEventArgs = null;
            Dictionary<AnnotAttrib, object> annotAttribsList = new Dictionary<AnnotAttrib, object>();
            switch (annotationType)
            {
                case CPDFAnnotationType.Highlight:
                    CPDFMarkupData highlightData = pdfAnnotationData as CPDFMarkupData;
                    annotHandlerEventArgs = new TextHighlightAnnotArgs();
                    (annotHandlerEventArgs as TextHighlightAnnotArgs).Color = highlightData.Color;
                    (annotHandlerEventArgs as TextHighlightAnnotArgs).Transparency = highlightData.Opacity;
                    (annotHandlerEventArgs as TextHighlightAnnotArgs).Content = highlightData.Note;
                    (annotHandlerEventArgs as TextHighlightAnnotArgs).Author = CPDFAnnotationData.Author;
                    (annotHandlerEventArgs as TextHighlightAnnotArgs).Locked = highlightData.IsLocked;
                    break;

                case CPDFAnnotationType.Underline:
                    CPDFMarkupData underlineData = pdfAnnotationData as CPDFMarkupData;
                    annotHandlerEventArgs = new TextUnderlineAnnotArgs();
                    (annotHandlerEventArgs as TextUnderlineAnnotArgs).Color = underlineData.Color;
                    (annotHandlerEventArgs as TextUnderlineAnnotArgs).Transparency = underlineData.Opacity;
                    (annotHandlerEventArgs as TextUnderlineAnnotArgs).Author = CPDFAnnotationData.Author;
                    (annotHandlerEventArgs as TextUnderlineAnnotArgs).Content = underlineData.Note;
                    (annotHandlerEventArgs as TextUnderlineAnnotArgs).Locked = underlineData.IsLocked;
                    break;

                case CPDFAnnotationType.Strikeout:
                    CPDFMarkupData strikeoutData = pdfAnnotationData as CPDFMarkupData;
                    annotHandlerEventArgs = new TextStrikeoutAnnotArgs();
                    (annotHandlerEventArgs as TextStrikeoutAnnotArgs).Color = strikeoutData.Color;
                    (annotHandlerEventArgs as TextStrikeoutAnnotArgs).Transparency = strikeoutData.Opacity;
                    (annotHandlerEventArgs as TextStrikeoutAnnotArgs).Locked = strikeoutData.IsLocked;
                    (annotHandlerEventArgs as TextStrikeoutAnnotArgs).Author = CPDFAnnotationData.Author;
                    (annotHandlerEventArgs as TextStrikeoutAnnotArgs).Content = strikeoutData.Note;
                    break;

                case CPDFAnnotationType.Squiggly:
                    CPDFMarkupData squigglyData = pdfAnnotationData as CPDFMarkupData;
                    annotHandlerEventArgs = new TextSquigglyAnnotArgs();
                    (annotHandlerEventArgs as TextSquigglyAnnotArgs).Color = squigglyData.Color;
                    (annotHandlerEventArgs as TextSquigglyAnnotArgs).Transparency = squigglyData.Opacity;
                    (annotHandlerEventArgs as TextSquigglyAnnotArgs).Locked = squigglyData.IsLocked;
                    (annotHandlerEventArgs as TextSquigglyAnnotArgs).Author = CPDFAnnotationData.Author;
                    (annotHandlerEventArgs as TextSquigglyAnnotArgs).Content = squigglyData.Note;
                    break;

                case CPDFAnnotationType.Square:
                    CPDFShapeData squareData = pdfAnnotationData as CPDFShapeData;
                    annotHandlerEventArgs = new SquareAnnotArgs();
                    (annotHandlerEventArgs as SquareAnnotArgs).LineColor = squareData.BorderColor;
                    (annotHandlerEventArgs as SquareAnnotArgs).BgColor = squareData.FillColor;
                    (annotHandlerEventArgs as SquareAnnotArgs).LineWidth = squareData.Thickness;
                    (annotHandlerEventArgs as SquareAnnotArgs).Transparency = squareData.Opacity;
                    (annotHandlerEventArgs as SquareAnnotArgs).LineDash = squareData.DashStyle;
                    (annotHandlerEventArgs as SquareAnnotArgs).Author = CPDFAnnotationData.Author;
                    (annotHandlerEventArgs as SquareAnnotArgs).Content = squareData.Note;
                    break;

                case CPDFAnnotationType.Circle:
                    CPDFShapeData cicleData = pdfAnnotationData as CPDFShapeData;
                    annotHandlerEventArgs = new CircleAnnotArgs();
                    (annotHandlerEventArgs as CircleAnnotArgs).LineColor = cicleData.BorderColor;
                    (annotHandlerEventArgs as CircleAnnotArgs).BgColor = cicleData.FillColor;
                    (annotHandlerEventArgs as CircleAnnotArgs).LineWidth = cicleData.Thickness;
                    (annotHandlerEventArgs as CircleAnnotArgs).Transparency = cicleData.Opacity;
                    (annotHandlerEventArgs as CircleAnnotArgs).LineDash = cicleData.DashStyle;
                    (annotHandlerEventArgs as CircleAnnotArgs).Author = CPDFAnnotationData.Author;
                    (annotHandlerEventArgs as CircleAnnotArgs).Content = cicleData.Note;
                    break;

                case CPDFAnnotationType.Arrow:
                case CPDFAnnotationType.Line:
                    CPDFLineShapeData lineData = pdfAnnotationData as CPDFLineShapeData;
                    annotHandlerEventArgs = new LineAnnotArgs();
                    (annotHandlerEventArgs as LineAnnotArgs).LineColor = lineData.BorderColor;
                    (annotHandlerEventArgs as LineAnnotArgs).LineWidth = lineData.Thickness;
                    (annotHandlerEventArgs as LineAnnotArgs).Transparency = lineData.Opacity;
                    (annotHandlerEventArgs as LineAnnotArgs).LineDash = lineData.DashStyle;
                    (annotHandlerEventArgs as LineAnnotArgs).HeadLineType = lineData.LineType.HeadLineType;
                    (annotHandlerEventArgs as LineAnnotArgs).TailLineType = lineData.LineType.TailLineType;
                    (annotHandlerEventArgs as LineAnnotArgs).Author = CPDFAnnotationData.Author;
                    (annotHandlerEventArgs as LineAnnotArgs).Content = lineData.Note;
                    break;

                case CPDFAnnotationType.Note:
                    CPDFNoteData noteData = pdfAnnotationData as CPDFNoteData;
                    annotHandlerEventArgs = new StickyAnnotArgs();
                    (annotHandlerEventArgs as StickyAnnotArgs).Color = noteData.BorderColor;
                    (annotHandlerEventArgs as StickyAnnotArgs).StickyNote = noteData.Note;
                    (annotHandlerEventArgs as StickyAnnotArgs).Transparency = 1;
                    (annotHandlerEventArgs as StickyAnnotArgs).Author = CPDFAnnotationData.Author;
                    break;

                case CPDFAnnotationType.Freehand:
                    CPDFFreehandData freehandData = pdfAnnotationData as CPDFFreehandData;
                    annotHandlerEventArgs = new FreehandAnnotArgs();
                    (annotHandlerEventArgs as FreehandAnnotArgs).InkColor = freehandData.BorderColor;
                    (annotHandlerEventArgs as FreehandAnnotArgs).LineWidth = freehandData.Thickness;
                    (annotHandlerEventArgs as FreehandAnnotArgs).Transparency = freehandData.Opacity;
                    (annotHandlerEventArgs as FreehandAnnotArgs).Content = freehandData.Note;
                    (annotHandlerEventArgs as FreehandAnnotArgs).Author = CPDFAnnotationData.Author;
                    break;

                case CPDFAnnotationType.FreeText:
                    CPDFFreeTextData freeTextData = pdfAnnotationData as CPDFFreeTextData;
                    annotHandlerEventArgs = new FreeTextAnnotArgs();
                    (annotHandlerEventArgs as FreeTextAnnotArgs).Transparency = freeTextData.Opacity;
                    (annotHandlerEventArgs as FreeTextAnnotArgs).FontName = freeTextData.FontFamily.ToString();
                    (annotHandlerEventArgs as FreeTextAnnotArgs).FontColor = freeTextData.BorderColor;
                    (annotHandlerEventArgs as FreeTextAnnotArgs).IsBold = freeTextData.IsBold;
                    (annotHandlerEventArgs as FreeTextAnnotArgs).IsItalic = freeTextData.IsItalic;
                    (annotHandlerEventArgs as FreeTextAnnotArgs).FontSize = freeTextData.FontSize;
                    (annotHandlerEventArgs as FreeTextAnnotArgs).Align = freeTextData.TextAlignment;
                    (annotHandlerEventArgs as FreeTextAnnotArgs).Content = freeTextData.Note;
                    (annotHandlerEventArgs as FreeTextAnnotArgs).Author = CPDFAnnotationData.Author;
                    break;

                case CPDFAnnotationType.Stamp:
                    annotHandlerEventArgs = new StampAnnotArgs();
                    StampAnnotArgs stampAnnot = annotHandlerEventArgs as StampAnnotArgs;
                    CPDFStampData stampData = pdfAnnotationData as CPDFStampData;
                    SetStamp(ref stampAnnot, stampData);
                    break;
                case CPDFAnnotationType.Signature:
                    annotHandlerEventArgs = new StampAnnotArgs();
                    StampAnnotArgs SignatureAnnot = annotHandlerEventArgs as StampAnnotArgs;
                    CPDFSignatureData SignatureData = pdfAnnotationData as CPDFSignatureData;
                    SetSignature(ref SignatureAnnot, SignatureData);
                    break;
                case CPDFAnnotationType.Link:
                    if (annotHandlerEventArgs != null)
                    {
                        pdfViewer.SetToolParam(annotHandlerEventArgs);
                    }
                    break;
                case CPDFAnnotationType.Unknown:
                    pdfViewer.SetMouseMode(MouseModes.PanTool);
                    return;
                default:
                    break;
            }
            this.pdfViewer.SetMouseMode(MouseModes.AnnotCreate);
            pdfViewer.SetToolParam(annotHandlerEventArgs);
        }

        public void SetSignature(ref StampAnnotArgs Args, CPDFSignatureData stamp)
        {
            switch (stamp.Type)
            {
                case SignatureType.TextType:
                case SignatureType.ImageType:
                    {
                        Args.Opacity = 1;
                        Args.Type = StampType.IMAGE_STAMP;
                        Args.ImagePath = stamp.SourcePath;
                    }
                    break;
                case SignatureType.Drawing:
                    {
                        Args.SetInkData(GetPoints(stamp.DrawingPath), stamp.inkThickness, stamp.inkColor);
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

        private void SetStamp(ref StampAnnotArgs Args, CPDFStampData stamp)
        {
            Args.StampText = stamp.StampText;
            Args.Author = CPDFAnnotationData.Author;
            Args.Opacity = stamp.Opacity;
            if (stamp.Type == StampType.IMAGE_STAMP)
            {

                Args.ImageWidth = stamp.MaxWidth;
                Args.ImageHeight = stamp.MaxHeight;
            }
            else
            {
                Args.MaxWidth = stamp.MaxWidth;
                Args.MaxHeight = stamp.MaxHeight;
            }
            Args.StampTextDate = stamp.StampTextDate;
            Args.TextColor = stamp.TextColor;
            Args.TextSharp = stamp.TextSharp;
            if (!string.IsNullOrEmpty(stamp.SourcePath))
            {
                BitmapImage image = new BitmapImage(new Uri(stamp.SourcePath));
                Args.ImageArray = new byte[image.PixelWidth * image.PixelHeight * 4];
                image.CopyPixels(Args.ImageArray, image.PixelWidth * 4, 0);
                Args.ImageHeight = image.PixelHeight;
                Args.ImageWidth = image.PixelWidth;
            }
            else
            {
                try
                {
                    Args.ImageArray = new byte[stamp.ImageSource.PixelWidth * stamp.ImageSource.PixelHeight * 4];
                    stamp.ImageSource.CopyPixels(Args.ImageArray, stamp.ImageSource.PixelWidth * 4, 0);
                    Args.ImageHeight = stamp.ImageSource.PixelHeight;
                    Args.ImageWidth = stamp.ImageSource.PixelWidth;
                }
                catch
                {
                    var i = pdfStampUI.CustomStampList.IndexOf(stamp);
                    Settings.Default.CustomStampList.RemoveAt(i);
                    Settings.Default.Save();
                    pdfStampUI.LoadSettings();
                    return;
                }

            }
            Args.Type = stamp.Type;
        }

        public void AnnotationCancel()
        {
            this.pdfViewer.SetMouseMode(MouseModes.PanTool);
            ClearPanel();
        }

        private void CPDFAnnotationControl_PropertyChanged(object sender, CPDFAnnotationData e)
        {
            SetAnnotationProperty(e);
            if (pdfViewer != null && pdfViewer.MouseMode == MouseModes.AnnotCreate && pdfViewer.ToolManager.CurrentAnnotArgs is FreehandAnnotArgs)
            {
                FreehandAnnotArgs freehandArgs = pdfViewer.ToolManager.CurrentAnnotArgs as FreehandAnnotArgs;
                CPDFFreehandData freehandData = e as CPDFFreehandData;
                if (freehandData != null)
                {
                    freehandArgs.InkColor = freehandData.BorderColor;
                    freehandArgs.LineWidth = freehandData.Thickness;
                    freehandArgs.Transparency = freehandData.Opacity;
                    freehandArgs.Content = freehandData.Note;
                    freehandArgs.Author = CPDFAnnotationData.Author;
                }
            }
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
                    LinkAnnotArgs linkAnnotArgs = new LinkAnnotArgs();
                    if (this.pdfViewer != null && this.pdfViewer.Document != null)
                    {
                        this.pdfViewer.SetMouseMode(MouseModes.AnnotCreate);
                        pdfViewer.SetToolParam(linkAnnotArgs);
                        pdfLinkUI.InitLinkAnnotArgs(linkAnnotArgs, pdfViewer.Document.PageCount);
                    }
                    annotationPanel = pdfLinkUI;
                    break;
                case CPDFAnnotationType.Audio:
                    SoundAnnotArgs soundArgs = new SoundAnnotArgs();
                    OpenFileDialog openAudioDialog = new OpenFileDialog();
                    openAudioDialog.Filter = "Wave Files(*.wav)|*.wav|All Files(*.*;)|*.*;";
                    if (openAudioDialog.ShowDialog() == true)
                    {
                        soundArgs.SoundFilePath = openAudioDialog.FileName;
                        this.pdfViewer.SetMouseMode(MouseModes.AnnotCreate);
                        pdfViewer.SetToolParam(soundArgs);
                    }
                    else
                    {
                        this.pdfViewer.SetMouseMode(MouseModes.PanTool);
                        ClearAnnotationBar?.Invoke(this, EventArgs.Empty);
                    }
                    ClearPanel();
                    break;
                case CPDFAnnotationType.Image:
                    StampAnnotArgs stampArgs = new StampAnnotArgs();
                    stampArgs.Opacity = 1;
                    stampArgs.Type = StampType.IMAGE_STAMP;
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        stampArgs.ImagePath = openFileDialog.FileName;

                        this.pdfViewer?.SetMouseMode(MouseModes.AnnotCreate);
                        this.pdfViewer?.SetToolParam(stampArgs);
                    }
                    else
                    {
                        this.pdfViewer.SetMouseMode(MouseModes.PanTool);
                    }
                    ClearAnnotationBar?.Invoke(this, EventArgs.Empty);
                    ClearPanel();
                    break;
                default:
                    break;
            }
        }

        private void PdfFreehandUI_EraseChangeHandler(object sender, double e)
        {
            if (pdfViewer != null && eraseArgs != null)
            {
                eraseArgs.Thickness = e;
            }
        }

        private void PdfFreehandUI_EraseClickHandler(object sender, bool e)
        {
            if (pdfViewer != null)
            {
                CPDFFreehandUI freehandUI = sender as CPDFFreehandUI;
                if (e)
                {
                    annotArgs = pdfViewer.ToolManager.CurrentAnnotArgs;
                    eraseArgs = new EraseArgs();
                    eraseArgs.UIBorderColor = Color.FromArgb(0x1A, 0x00, 0x00, 0x00);
                    eraseArgs.UIFillColor = Color.FromArgb(0x1A, 0x00, 0x00, 0x00);

                    if (freehandUI != null)
                    {
                        eraseArgs.Thickness = freehandUI.GetEraseThickness();
                    }
                    else
                    {
                        eraseArgs.Thickness = 1;
                    }

                    disableClean = true;
                    pdfViewer.SetMouseMode(MouseModes.AnnotCreate);
                    pdfViewer.SetToolParam(eraseArgs);
                    disableClean = false;
                    EmptyMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pdfViewer.SetMouseMode(MouseModes.AnnotCreate);
                    FreehandAnnotArgs freehandAnnotArgs = annotArgs as FreehandAnnotArgs;
                    if (freehandAnnotArgs == null)
                    {
                        freehandAnnotArgs = new FreehandAnnotArgs();

                        freehandAnnotArgs.InkColor = Colors.Red;
                        freehandAnnotArgs.Transparency = 1;
                        freehandAnnotArgs.LineWidth = 1;
                        annotArgs = freehandAnnotArgs;
                    }

                    if (freehandUI != null)
                    {
                        freehandUI.PropertyChanged -= CPDFAnnotationControl_PropertyChanged;
                        Dictionary<AnnotAttrib, object> attribDict = new Dictionary<AnnotAttrib, object>();
                        attribDict[AnnotAttrib.Color] = freehandAnnotArgs.InkColor;
                        attribDict[AnnotAttrib.Transparency] = freehandAnnotArgs.Transparency;
                        attribDict[AnnotAttrib.Thickness] = freehandAnnotArgs.LineWidth;
                        attribDict[AnnotAttrib.NoteText] = freehandAnnotArgs.Content;

                        AnnotAttribEvent annotEvent = AnnotAttribEvent.GetAnnotAttribEvent(freehandAnnotArgs, attribDict);
                        freehandUI.SetPresentAnnotAttrib(annotEvent);
                        freehandUI.PropertyChanged += CPDFAnnotationControl_PropertyChanged;
                        freehandUI.ClearAnnotAttribEvent();
                    }
                    pdfViewer.SetToolParam(freehandAnnotArgs);
                }
            }
        }

        public void CreatTempAnnotationPanel(AnnotAttribEvent annotAttribEvent)
        {
            AnnotArgsType annotArgsType = annotAttribEvent.GetAnnotTypes();
            switch (annotArgsType)
            {
                case AnnotArgsType.AnnotHighlight:
                case AnnotArgsType.AnnotUnderline:
                case AnnotArgsType.AnnotStrikeout:
                case AnnotArgsType.AnnotSquiggly:
                    tempAnnotationPanel = new CPDFMarkupUI();
                    (tempAnnotationPanel as CPDFMarkupUI).InitWithAnnotationType(CPDFAnnotationDictionary.GetAnnotArgsTypeFromAnnotationType[annotArgsType]);
                    (tempAnnotationPanel as CPDFMarkupUI).SetPresentAnnotAttrib(annotAttribEvent);
                    break;

                case AnnotArgsType.AnnotSquare:
                case AnnotArgsType.AnnotCircle:
                case AnnotArgsType.AnnotLine:
                    tempAnnotationPanel = new CPDFShapeUI();
                    (tempAnnotationPanel as CPDFShapeUI).InitWithAnnotationType(CPDFAnnotationDictionary.GetAnnotArgsTypeFromAnnotationType[annotArgsType]);
                    (tempAnnotationPanel as CPDFShapeUI).SetPresentAnnotAttrib(annotAttribEvent);
                    break;

                case AnnotArgsType.AnnotFreehand:
                    CPDFFreehandUI tempFreehandPanel = new CPDFFreehandUI();
                    tempFreehandPanel.SetPresentAnnotAttrib(annotAttribEvent);
                    tempFreehandPanel.EraseClickHandler += PdfFreehandUI_EraseClickHandler;
                    tempFreehandPanel.EraseChangeHandler += PdfFreehandUI_EraseChangeHandler;
                    tempAnnotationPanel = tempFreehandPanel;
                    break;

                case AnnotArgsType.AnnotFreeText:
                    tempAnnotationPanel = new CPDFFreeTextUI();
                    (tempAnnotationPanel as CPDFFreeTextUI).SetPresentAnnotAttrib(annotAttribEvent);
                    break;

                case AnnotArgsType.AnnotSticky:
                    tempAnnotationPanel = new CPDFNoteUI();
                    (tempAnnotationPanel as CPDFNoteUI).SetPresentAnnotAttrib(annotAttribEvent);
                    break;

                case AnnotArgsType.AnnotStamp:
                    tempAnnotationPanel = new CPDFTempStampUI();
                    (tempAnnotationPanel as CPDFTempStampUI).SetPresentAnnotAttrib(annotAttribEvent);
                    break;
                case AnnotArgsType.AnnotLink:
                    tempAnnotationPanel = new CPDFLinkUI();
                    (tempAnnotationPanel as CPDFLinkUI).SetPresentAnnotAttrib(annotAttribEvent, pdfViewer.Document.PageCount);
                    break;
                case AnnotArgsType.AnnotSound:
                    tempAnnotationPanel = null;
                    break;
                default:
                    break;
            }
        }

        public void LoadAnnotationPanel(CPDFAnnotationType annotationType)
        {
            this.pdfViewer.SetMouseMode(MouseModes.PanTool);
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
                    (annotationPanel as CPDFFreehandUI)?.SetEraseCheck(false);
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
                if (pdfViewer != null && pdfViewer.ToolManager.CurrentAnnotArgs is EraseArgs)
                {
                    EmptyMessage.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowTempAnnotPanel(AnnotAttribEvent annotAttribEvent)
        {
            if (annotAttribEvent != null)
            {
                CreatTempAnnotationPanel(annotAttribEvent);
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

        private void PDFViewer_AnnotActiveHandler(object sender, AnnotAttribEvent e)
        {
            SetAnnotEventData(e);
        }

        public void ClearPanel()
        {
            annotationPanel = null;
            SetAnnotationPanel(annotationPanel);
            EmptyMessage.Visibility = Visibility.Visible;
        }

        public void SetAnnotEventData(AnnotAttribEvent newData)
        {
            if (newData != null)
            {
                if (newData.IsAnnotCreateReset && isTempPanel)
                {
                    ShowCurrentAnnotPanel();
                    isTempPanel = false;
                }
                else if (!newData.IsAnnotCreateReset)
                {
                    AnnotArgsType annotArgsType = newData.GetAnnotTypes();
                    ShowTempAnnotPanel(newData);
                    isTempPanel = true;
                }
            }
            else
            {
                if (pdfViewer != null && pdfViewer.MouseMode == MouseModes.AnnotCreate)
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
