using ComPDFKit.Tool.Help;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer;
using ComPDFKitViewer.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool.DrawTool
{
    public class TextDrawRect
    {
        public string Text { get; set; }

        /// <summary>
        /// Current text rectangle (PDF DPI)
        /// </summary>
        public Rect DrawRect { get; set; }
        public SolidColorBrush PaintBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x46, 0x46, 0x82, 0xB4));

        #region Properties Used for Searching

        internal bool DrawActiveSearch { get; set; } = false;

        internal TextSearchItem SearchInfo { get; set; }

        #endregion
    }

    public class TextSelectInfo
    {
        public int StartPage = -1;
        public int EndPage = -1;
        /// <summary>
        /// Original page coordinates
        /// </summary>
        public Point StartPoint = new Point();
        /// <summary>
        /// Original page coordinates
        /// </summary>
        public Point EndPoint = new Point();
        public int PageRotate;
        public bool RotateRecord;
        public Dictionary<int, string> PageSelectText = new Dictionary<int, string>();
        public Dictionary<int, List<TextDrawRect>> PageSelectTextRectList = new Dictionary<int, List<TextDrawRect>>();
        public Dictionary<int, KeyValuePair<Point, Point>> PageSelectPointList = new Dictionary<int, KeyValuePair<Point, Point>>();
        internal Dictionary<int, List<Rect>> ConvertToSelectRectDict()
        {
            Dictionary<int, List<Rect>> TextRectDict = new Dictionary<int, List<Rect>>();

            if (PageSelectTextRectList != null)
            {
                foreach (int key in PageSelectTextRectList.Keys)
                {
                    List<TextDrawRect> textDrawRects = PageSelectTextRectList[key];
                    List<Rect> rectList = new List<Rect>();

                    foreach (TextDrawRect drawItem in textDrawRects)
                    {
                        rectList.Add(drawItem.DrawRect);
                    }
                    TextRectDict[key] = rectList;
                }
            }

            return TextRectDict;

        }
    }

    internal class SelectText : CustomizeLayer
    {
        protected DrawingContext drawDC { get; set; }

        private TextSelectInfo textInfo { get; set; } = new TextSelectInfo();
        private TextSelectInfo searchInfo { get; set; } = new TextSelectInfo();

        /// <summary>
        ///Identify whether the text selection effect is being drawn
        /// </summary>
        protected bool isDrawSelectText { get; set; }

        private TextSelectInfo SortTextSelectInfo(TextSelectInfo textInfo)
        {
            if (textInfo == null || textInfo.StartPage <= textInfo.EndPage)
            {
                return textInfo;
            }

            TextSelectInfo SortItem = new TextSelectInfo();
            SortItem.StartPage = textInfo.EndPage;
            SortItem.StartPoint = textInfo.EndPoint;
            SortItem.EndPage = textInfo.StartPage;
            SortItem.EndPoint = textInfo.StartPoint;
            SortItem.PageSelectPointList = textInfo.PageSelectPointList;
            SortItem.PageSelectTextRectList = textInfo.PageSelectTextRectList;
            SortItem.PageSelectText = textInfo.PageSelectText;
            SortItem.PageRotate = textInfo.PageRotate;
            SortItem.RotateRecord = textInfo.RotateRecord;
            return SortItem;
        }

        private void SetTextSelectRange(TextSelectInfo SelectTextItem, CPDFViewer cPDFViewer, Point tolerance, bool DoubleClick = false)
        {
            if (SelectTextItem == null)
            {
                return;
            }

            // Remove data that does not need to be displayed
            for (int i = 0; i < SelectTextItem.PageSelectText.Count; i++)
            {
                var item = SelectTextItem.PageSelectText.ElementAt(i);
                if (item.Key < SelectTextItem.StartPage || item.Key > SelectTextItem.EndPage)
                {
                    SelectTextItem.PageSelectText.Remove(item.Key);
                }
            }

            for (int i = 0; i < SelectTextItem.PageSelectPointList.Count; i++)
            {
                var item = SelectTextItem.PageSelectPointList.ElementAt(i);
                if (item.Key < SelectTextItem.StartPage || item.Key > SelectTextItem.EndPage)
                {
                    SelectTextItem.PageSelectPointList.Remove(item.Key);
                }
            }

            for (int i = 0; i < SelectTextItem.PageSelectTextRectList.Count; i++)
            {
                var item = SelectTextItem.PageSelectTextRectList.ElementAt(i);
                if (item.Key < SelectTextItem.StartPage || item.Key > SelectTextItem.EndPage)
                {
                    SelectTextItem.PageSelectTextRectList.Remove(item.Key);
                } 
            }

            // Add or update data
            for (int i = SelectTextItem.StartPage; i <= SelectTextItem.EndPage; i++)
            {
                PageViewData RenderPage = cPDFViewer.GetPageNodeByPageIndex(i);
                if (RenderPage == null)
                {
                    continue;
                }
                Point StartPoint = new Point(0, 0);
                Point EndPoint = new Point(RenderPage.RawSize.Width, RenderPage.RawSize.Height);
                if (i == SelectTextItem.StartPage)
                {
                    StartPoint = SelectTextItem.StartPoint;
                }
                if (i == SelectTextItem.EndPage)
                {
                    EndPoint = SelectTextItem.EndPoint;
                }
                bool ReSelect = true;
                if (SelectTextItem.PageSelectPointList.ContainsKey(i))
                {
                    KeyValuePair<Point, Point> PrevPointRange = SelectTextItem.PageSelectPointList[i];
                    if (PrevPointRange.Key == StartPoint && PrevPointRange.Value == EndPoint)
                    {
                        ReSelect = false;
                    }
                }
                if (ReSelect)
                {
                    if (DoubleClick == true)
                    {
                        Rect uiRect = Rect.Empty;
                        SelectTextItem.PageSelectText[i] = PDFHelp.GetDoubleClickText(cPDFViewer.GetDocument(), i, StartPoint, ref uiRect);
                        SelectTextItem.PageSelectPointList[i] = new KeyValuePair<Point, Point>(StartPoint, EndPoint);
                        SelectTextItem.PageSelectTextRectList[i] = new List<TextDrawRect>() { new TextDrawRect() { DrawRect = uiRect, Text = SelectTextItem.PageSelectText[i] } };
                    }
                    else
                    {
                        SelectTextItem.PageSelectText[i] = PDFHelp.GetSelectText(cPDFViewer.GetDocument(), i, StartPoint, EndPoint, tolerance);
                        SelectTextItem.PageSelectPointList[i] = new KeyValuePair<Point, Point>(StartPoint, EndPoint);
                        SelectTextItem.PageSelectTextRectList[i] = PDFHelp.GetSelectTextRect(cPDFViewer.GetDocument(), i, StartPoint, EndPoint, tolerance);
                    }
                }
            }
        }

        public SelectText()
        {

        }

        public void StartDraw(Point pagePoint, int pageIndex)
        {
            isDrawSelectText = true;
            textInfo = new TextSelectInfo()
            {
                StartPage = pageIndex,
                EndPage = pageIndex,
                StartPoint = pagePoint,
                EndPoint = pagePoint
            };
        }

        public void MoveDraw(Point downPoint, int pageIndex, CPDFViewer cPDFViewer, Point tolerance, bool DoubleClick)
        {
            if (isDrawSelectText)
            {
                textInfo.EndPage = pageIndex;
                textInfo.EndPoint = downPoint;

                SetTextSelectRange(SortTextSelectInfo(textInfo), cPDFViewer, tolerance, DoubleClick);

                Draw(cPDFViewer);
            }
        }

        public TextSelectInfo GetTextSelectInfo()
        {
            return textInfo;
        }

        public TextSelectInfo GetSearchInfo()
        {
            return searchInfo;
        }
        public void SetSearchInfo(TextSelectInfo searchInfo)
        {
            this.searchInfo = searchInfo;
        }

        public void CleanSearchInfo()
        {
            searchInfo = new TextSelectInfo();
        }

        public void RemoveSelectDataInfo()
        {
            textInfo = new TextSelectInfo();
        }

        public bool HasSelectTextInfo()
        {
            if (textInfo?.PageSelectTextRectList.Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool HasSearchInfo()
        {
            if (searchInfo?.PageSelectTextRectList.Count > 0)
            {
                return true;
            }
            return false;
        }

        public void EndDraw()
        {
            isDrawSelectText = false;
        }

        public void Draw(CPDFViewer cPDFViewer)
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = Open();
                DrawSelectRange(drawDC, textInfo, cPDFViewer);
                DrawSelectRange(drawDC, searchInfo, cPDFViewer);
                Present();
            });
        }

        public void CleanDraw(CPDFViewer cPDFViewer)
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = Open();
                DrawSelectRange(drawDC, searchInfo, cPDFViewer);
                Present();
            });
        }

        public override void Draw()
        {

        }

        private void DrawSelectRange(DrawingContext dc, TextSelectInfo SelectTextItem, CPDFViewer cPDFViewer)
        {
            List<RenderData> renderDatas=new List<RenderData>();
            if (cPDFViewer.CurrentRenderFrame!=null)
            {
                renderDatas = cPDFViewer.CurrentRenderFrame.GetRenderDatas();
            }

            foreach (RenderData PaintRange in renderDatas)
            {
                if (SelectTextItem.PageSelectTextRectList.ContainsKey(PaintRange.PageIndex))
                {
                    List<TextDrawRect> PaintRectList = SelectTextItem.PageSelectTextRectList[PaintRange.PageIndex];
                    foreach (TextDrawRect SelectRect in PaintRectList)
                    {
                        Rect drawRect = SelectRect.DrawRect;
                        Rect textRect = drawRect;
                        if (cPDFViewer.GetDocument() != null && SelectTextItem.RotateRecord)
                        {
                            var rawPage = cPDFViewer.GetDocument().PageAtIndex(PaintRange.PageIndex);
                            if (rawPage != null)
                            {
                                int rotation = rawPage.Rotation - SelectTextItem.PageRotate;
                                if (rotation != 0)
                                {
                                    Size rawSize = new Size(PaintRange.PageBound.Width, PaintRange.PageBound.Height);
                                    Matrix matrix = new Matrix();
                                    matrix.RotateAt(-rotation * 90, rawSize.Width / 2, rawSize.Height / 2);
                                    Rect checkRect = new Rect(0, 0, rawSize.Width, rawSize.Height);
                                    checkRect.Transform(matrix);
                                    matrix = new Matrix();
                                    matrix.RotateAt(rotation * 90, checkRect.Width / 96D * 72D / 2, checkRect.Height / 96D * 72D / 2);
                                    checkRect = new Rect(0, 0, checkRect.Width / 96D * 72D, checkRect.Height / 96D * 72D);
                                    textRect.Transform(matrix);
                                    checkRect.Transform(matrix);
                                    drawRect = new Rect(textRect.Left - checkRect.Left,
                                        textRect.Top - checkRect.Top,
                                        textRect.Width, textRect.Height);
                                }
                            }
                        }
                        Rect BorderRect = PaintRange.PageBound;
                        Rect RawPaintRect =DpiHelper.PDFRectToStandardRect( 
                            new Rect(
                            drawRect.Left * cPDFViewer.CurrentRenderFrame.ZoomFactor - DpiHelper.StandardNumToPDFNum(PaintRange.CropLeft) * cPDFViewer.CurrentRenderFrame.ZoomFactor,
                            drawRect.Top * cPDFViewer.CurrentRenderFrame.ZoomFactor - DpiHelper.StandardNumToPDFNum(PaintRange.CropTop) * cPDFViewer.CurrentRenderFrame.ZoomFactor,
                            drawRect.Width * cPDFViewer.CurrentRenderFrame.ZoomFactor,
                            drawRect.Height * cPDFViewer.CurrentRenderFrame.ZoomFactor));

                        RawPaintRect.X += BorderRect.X;
                        RawPaintRect.Y += BorderRect.Y;

                        RectangleGeometry clipGeometry = new RectangleGeometry();
                        clipGeometry.Rect = BorderRect;
                        dc.PushClip(clipGeometry);
                        Rect paintRect = RawPaintRect;
                        dc.DrawRectangle(SelectRect.PaintBrush, null, paintRect);

                        TextSearchItem searchInfo = SelectRect.SearchInfo;
                        if (SelectRect.DrawActiveSearch && searchInfo.BorderThickness > 0 && searchInfo.BorderBrush != Brushes.Transparent)
                        {
                            Rect outRect = new Rect(paintRect.Left - searchInfo.BorderThickness / 2 - searchInfo.Padding.Left,
                                paintRect.Top - searchInfo.BorderThickness / 2 - searchInfo.Padding.Top,
                                paintRect.Width + searchInfo.BorderThickness + searchInfo.Padding.Left + searchInfo.Padding.Right,
                                paintRect.Height + searchInfo.BorderThickness + searchInfo.Padding.Top + searchInfo.Padding.Bottom);

                            Pen borderPen = new Pen(searchInfo.BorderBrush, searchInfo.BorderThickness);
                            dc.DrawRectangle(null, borderPen, outRect);
                        }
                        dc.Pop();
                    }
                }
            }
        }
    }
}
