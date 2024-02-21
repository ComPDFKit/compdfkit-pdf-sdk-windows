using Compdfkit_Tools.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Compdfkit_Tools.Helper.CommonHelper;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFAnnotationPreviewerControl : UserControl
    {
        public CPDFAnnotationPreviewerControl()
        {
            InitializeComponent();
        }

        public void CollapsedAll()
        {
            MarkupGrid.Visibility = Visibility.Collapsed;
            HighlightPath.Visibility = Visibility.Collapsed;
            UnderlinePath.Visibility = Visibility.Collapsed;
            StrikeoutPath.Visibility = Visibility.Collapsed;
            SquigglyPath.Visibility = Visibility.Collapsed;
            FreehandGrid.Visibility = Visibility.Collapsed;
            FreeTextGrid.Visibility = Visibility.Collapsed;
            ShapeGrid.Visibility = Visibility.Collapsed;
            Ellipse.Visibility = Visibility.Collapsed;
            Rectangle.Visibility = Visibility.Collapsed;
            Line.Visibility = Visibility.Collapsed;
            NoteGrid.Visibility = Visibility.Collapsed;
            StampGrid.Visibility = Visibility.Collapsed;
        }

        public void DrawMarkUpPreview(CPDFAnnotationData annotationData)
        {
            CPDFMarkupData markupData = annotationData as CPDFMarkupData;

            CollapsedAll();
            CPDFAnnotationType annotationType = annotationData.AnnotationType;
            switch (annotationType)
            {
                case CPDFAnnotationType.Highlight:
                    MarkupGrid.Visibility = Visibility.Visible;
                    HighlightPath.Visibility = Visibility.Visible;
                    HighlightPath.Stroke = new SolidColorBrush(markupData.Color);
                    HighlightPath.Opacity = markupData.Opacity;
                    break;

                case CPDFAnnotationType.Underline:
                    MarkupGrid.Visibility = Visibility.Visible;
                    UnderlinePath.Visibility = Visibility.Visible;
                    UnderlinePath.Stroke = new SolidColorBrush(markupData.Color);
                    UnderlinePath.Opacity = markupData.Opacity;
                    break;

                case CPDFAnnotationType.Squiggly:
                    MarkupGrid.Visibility = Visibility.Visible;
                    SquigglyPath.Visibility = Visibility.Visible;
                    SquigglyPath.Stroke = new SolidColorBrush(markupData.Color);
                    SquigglyPath.Opacity = markupData.Opacity;
                    break;

                case CPDFAnnotationType.Strikeout:
                    MarkupGrid.Visibility = Visibility.Visible;
                    StrikeoutPath.Visibility = Visibility.Visible;
                    StrikeoutPath.Stroke = new SolidColorBrush(markupData.Color);
                    StrikeoutPath.Opacity = markupData.Opacity;
                    break;
            }
        }

        public void DrawShapePreview(CPDFAnnotationData annotationData)
        {
            CPDFAnnotationType annotationType = annotationData.AnnotationType;
            switch (annotationType)
            {
                case CPDFAnnotationType.Circle:
                    CPDFShapeData circleData = annotationData as CPDFShapeData;
                    ShapeGrid.Visibility = Visibility.Visible;
                    Ellipse.Visibility = Visibility.Visible;

                    Ellipse.StrokeThickness = circleData.Thickness;
                    Ellipse.Opacity = circleData.Opacity;
                    Ellipse.Fill = new SolidColorBrush(circleData.FillColor);
                    Ellipse.Stroke = new SolidColorBrush(circleData.BorderColor);
                    DashStyle circleDash = new DashStyle();
                    if (circleData.DashStyle.Dashes.Count == 2)
                    {
                        circleDash.Dashes.Add(circleData.DashStyle.Dashes[0] / Math.Max(Ellipse.StrokeThickness, 1));
                        circleDash.Dashes.Add(circleData.DashStyle.Dashes[1] / Math.Max(Ellipse.StrokeThickness, 1));
                    }
                    Ellipse.StrokeDashArray = circleDash.Dashes;

                    break;

                case CPDFAnnotationType.Square:
                    CPDFShapeData squareData = annotationData as CPDFShapeData;
                    ShapeGrid.Visibility = Visibility.Visible;
                    Rectangle.Visibility = Visibility.Visible;

                    Rectangle.StrokeThickness = squareData.Thickness;
                    Rectangle.Opacity = squareData.Opacity;
                    Rectangle.Fill = new SolidColorBrush(squareData.FillColor);
                    Rectangle.Stroke = new SolidColorBrush(squareData.BorderColor);
                    DashStyle squareDash = new DashStyle();
                    if (squareData.DashStyle.Dashes.Count == 2)
                    {
                        squareDash.Dashes.Add(squareData.DashStyle.Dashes[0] / Math.Max(Rectangle.StrokeThickness, 1));
                        squareDash.Dashes.Add(squareData.DashStyle.Dashes[1] / Math.Max(Rectangle.StrokeThickness, 1));
                    }
                    Rectangle.StrokeDashArray = squareDash.Dashes;
                    break;

                case CPDFAnnotationType.Line:
                case CPDFAnnotationType.Arrow:

                    CPDFLineShapeData lineShapeData = annotationData as CPDFLineShapeData;
                    ShapeGrid.Visibility = Visibility.Visible;
                    Line.Visibility = Visibility.Visible;
                    ArrowHelper arrowLine = new ArrowHelper();
                    arrowLine.ArrowLength = 8;
                    arrowLine.LineStart = new Point(20, 50);
                    arrowLine.LineEnd = new Point(50, 20);
                    arrowLine.StartSharp = lineShapeData.LineType.HeadLineType;
                    arrowLine.EndSharp = lineShapeData.LineType.TailLineType;
                    Line.Stroke = new SolidColorBrush(lineShapeData.BorderColor);
                    Line.StrokeThickness = lineShapeData.Thickness;
                    Line.Opacity = lineShapeData.Opacity;
                    Line.Data = arrowLine.BuildArrowBody();
                    DashStyle lineDash = new DashStyle();
                    if (lineShapeData.DashStyle.Dashes.Count == 2)
                    {
                        lineDash.Dashes.Add(lineShapeData.DashStyle.Dashes[0] / Math.Max(Line.StrokeThickness, 1));
                        lineDash.Dashes.Add(lineShapeData.DashStyle.Dashes[1] / Math.Max(Line.StrokeThickness, 1));
                    }
                    Line.StrokeDashArray = lineDash.Dashes;
                    break;
            }
        }

        public void DrawFreehandPreview(CPDFAnnotationData annotationData)
        {
            CPDFFreehandData freehandData = annotationData as CPDFFreehandData;

            CollapsedAll();
            FreehandGrid.Visibility = Visibility.Visible;
            SharpPath.Stroke = new SolidColorBrush(freehandData.BorderColor);
            SharpPath.Opacity = freehandData.Opacity;
            SharpPath.StrokeThickness = freehandData.Thickness;
        }

        public void DrawNotePreview(CPDFAnnotationData annotationData)
        {
            CollapsedAll();
            NoteGrid.Visibility = Visibility.Visible;
            CPDFNoteData noteData = annotationData as CPDFNoteData;
            NotePath.Fill = new SolidColorBrush(noteData.BorderColor);
        }

        public void DrawStampPreview(WriteableBitmap writeableBitmap)
        {
            CollapsedAll();
            StampGrid.Visibility = Visibility.Visible;
            StampImage.Source = writeableBitmap;
        }
        
        public void DrawFreeTextPreview(CPDFFreeTextData freeTextData)
        {
            CollapsedAll();
            FreeTextGrid.Visibility = Visibility.Visible;
            
            if (freeTextData.FontFamily == "Helvetica")
            {
                FreeText.FontFamily = new FontFamily("Arial");
            }
            else if (freeTextData.FontFamily == "Times")
            {
                FreeText.FontFamily = new FontFamily("Times New Roman");
            }
            else
            {
                FreeText.FontFamily = new FontFamily("Courier New");
            }

            FreeText.FontSize = freeTextData.FontSize/1.2;
            FreeText.Foreground = new SolidColorBrush(freeTextData.BorderColor);
            FreeText.Opacity = freeTextData.Opacity;
            if (freeTextData.IsBold)
            {
                FreeText.FontWeight = FontWeights.Bold;
            }
            else
            {
                FreeText.FontWeight = FontWeights.Medium;
            }

            if (freeTextData.IsItalic)
            {
                FreeText.FontStyle = FontStyles.Italic;
            }
            else
            {
                FreeText.FontStyle = FontStyles.Normal;
            }
        }
    }
}
