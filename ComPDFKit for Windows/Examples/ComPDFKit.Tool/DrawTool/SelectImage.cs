using ComPDFKit.Import;
using ComPDFKit.PDFPage;
using ComPDFKit.Viewer.Helper;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ComPDFKit.Tool.SettingParam;

namespace ComPDFKit.Tool.DrawTool
{
    public class PageImageItem
    {
        public int PageIndex;
        /// <summary>
        /// PDF DPI(72)
        /// </summary>
        public Rect PaintRect;
        public int PageRotate;
        public int ImageIndex;

        public PageImageItem Clone()
        {
            PageImageItem cloneItem = new PageImageItem();

            cloneItem.PageIndex = PageIndex;
            cloneItem.PaintRect = PaintRect;
            cloneItem.PageRotate = PageRotate;
            cloneItem.ImageIndex = ImageIndex;

            return cloneItem;
        }
    }
    internal class SelectImage : CustomizeLayer
    {
        protected DrawingContext drawDC { get; set; }

        protected DefaultDrawParam drawParam = new DefaultDrawParam();

        private Dictionary<int, List<PageImageItem>> pageImageDict = new Dictionary<int, List<PageImageItem>>();

        private PageImageItem hoverImageItem = null;

        /// <summary>
        /// Identify whether the image selection effect is being drawn
        /// </summary>
        protected bool isDrawSelectImage { get; set; }

        public override void Draw()
        {

        }

        public void Draw(CPDFViewer cPDFViewer)
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = Open();
                List<RenderData> renderDatas = cPDFViewer.CurrentRenderFrame.GetRenderDatas();
                foreach (RenderData item in renderDatas)
                {
                    if (pageImageDict.ContainsKey(item.PageIndex))
                    {
                        List<PageImageItem> PaintImageList = pageImageDict[item.PageIndex];
                        foreach (PageImageItem SelectItem in PaintImageList)
                        {
                            DrawItem(SelectItem, item, cPDFViewer, drawDC, false);
                        }
                    }
                    if (hoverImageItem != null && hoverImageItem.PageIndex == item.PageIndex)
                    {
                        DrawItem(hoverImageItem, item, cPDFViewer, drawDC, true);
                    }
                }
                Present();
            });
        }

        public void CleanDraw(CPDFViewer cPDFViewer)
        {
            Dispatcher.Invoke(() =>
            {
                drawDC = Open();
                Present();
                ClearImageItem();
            });
        }

        private void DrawItem(PageImageItem SelectItem, RenderData renderData, CPDFViewer cPDFViewer, DrawingContext dc, bool isHover)
        {
            Rect drawRect = SelectItem.PaintRect;
            if (cPDFViewer.GetDocument() != null)
            {
                CPDFPage rawPage = cPDFViewer.GetDocument().PageAtIndex(renderData.PageIndex);
                if (rawPage != null)
                {
                    int rotation = rawPage.Rotation - SelectItem.PageRotate;
                    if (rotation != 0)
                    {
                        Size rawSize = renderData.RenderRect.Size;
                        Matrix matrix = new Matrix();
                        matrix.RotateAt(-rotation * 90, rawSize.Width / 2, rawSize.Height / 2);
                        Rect checkRect = new Rect(0, 0, rawSize.Width, rawSize.Height);
                        checkRect.Transform(matrix);
                        matrix = new Matrix();
                        matrix.RotateAt(rotation * 90, checkRect.Width / 96D * 72D / 2, checkRect.Height / 96D * 72D / 2);
                        checkRect = new Rect(0, 0, checkRect.Width / 96D * 72D, checkRect.Height / 96D * 72D);

                        drawRect.Transform(matrix);
                        checkRect.Transform(matrix);

                        drawRect = new Rect(drawRect.Left - checkRect.Left,
                            drawRect.Top - checkRect.Top,
                            drawRect.Width, drawRect.Height);
                    }
                }
            }
            Rect BorderRect = renderData.PageBound;
            Rect RawPaintRect = new Rect(drawRect.Left / 72 * cPDFViewer.GetZoom() * 96 - renderData.CropLeft * cPDFViewer.GetZoom(),
                drawRect.Top / 72 * cPDFViewer.GetZoom() * 96 - renderData.CropTop * cPDFViewer.GetZoom(),
                drawRect.Width / 72 * cPDFViewer.GetZoom() * 96,
                drawRect.Height / 72 * cPDFViewer.GetZoom() * 96);

            RawPaintRect.X += BorderRect.X;
            RawPaintRect.Y += BorderRect.Y;

            RectangleGeometry clipGeometry = new RectangleGeometry();
            clipGeometry.Rect = renderData.PageBound;
            dc.PushClip(clipGeometry);
            if (isHover)
            {
                dc.DrawRectangle(null, drawParam.ViewerImagePen, RawPaintRect);
            }
            else
            {
                dc.DrawRectangle(drawParam.ViewerImageBackgroundBrush, null, RawPaintRect);
            }
            dc.Pop();
        }

        public bool ProcessMouseDownForSelectImage(Point pdfPoint, int pageIndex, CPDFViewer cPDFViewer, bool isNeedClear)
        {
            return ProcessSelectImageAtPos(cPDFViewer, pdfPoint, pageIndex, false, isNeedClear);
        }

        public bool ProcessMouseMoveForSelectImage(Point pdfPoint, int pageIndex, CPDFViewer cPDFViewer)
        {
            return ProcessSelectImageAtPos(cPDFViewer, pdfPoint, pageIndex, true, true);
        }

        private bool ProcessSelectImageAtPos(CPDFViewer cPDFViewer, Point pdfPoint, int pageIndex, bool isHover, bool isNeedClear)
        {
            bool result = false;
            if (cPDFViewer == null || cPDFViewer.GetDocument() == null || pageIndex < 0)
            {
                return result;
            }
            CPDFPage rawPage = cPDFViewer.GetDocument().PageAtIndex(pageIndex);
            CPDFImgSelection imageRanges = rawPage.GetImgSelection();
            List<CRect> checkList = imageRanges.GetImageRects();

            bool findItem = false;
            for (int i = 0; i < checkList.Count; i++)
            {
                Rect checkRect = DataConversionForWPF.CRectConversionForRect(checkList[i]);
                if (checkRect.Contains(pdfPoint))
                {
                    findItem = true;
                    if (isHover)
                    {
                        if (hoverImageItem == null || hoverImageItem.PageIndex != pageIndex || hoverImageItem.ImageIndex != i)
                        {
                            hoverImageItem = new PageImageItem()
                            {
                                PageIndex = pageIndex,
                                PaintRect = checkRect,
                                PageRotate = rawPage.Rotation,
                                ImageIndex = i
                            };
                            result = true;
                        }
                    }
                    else
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            if (ContainImageItem(pageIndex, i))
                            {
                                result = RemoveImageItem(pageIndex, i);
                            }
                            else
                            {
                                if (pageImageDict != null && pageImageDict.Count > 0 && pageImageDict.Keys.Contains(pageIndex) == false)
                                {
                                    ClearImageItem();
                                }
                                result = AddPageImageItem(pageIndex, checkRect, rawPage.Rotation, i);
                            }
                        }
                        else
                        {
                            if (!ContainImageItem(pageIndex, i) || isNeedClear)
                            {
                                ClearImageItem();
                                result = AddPageImageItem(pageIndex, checkRect, rawPage.Rotation, i);
                            }
                            else
                            {
                                result = true;
                            }
                        }

                        hoverImageItem = null;
                    }
                    break;
                }
            }
            if (findItem == false && hoverImageItem != null)
            {
                hoverImageItem = null;
                result = false;
            }
            if (findItem == false && !isHover && pageImageDict != null && pageImageDict.Count > 0 && isNeedClear)
            {
                ClearImageItem();
                result = true;
            }

            return result;
        }

        private bool AddPageImageItem(int pageIndex, Rect paintRect, int pageRotate, int ImageIndex)
        {
            bool result = false;
            if (pageImageDict.ContainsKey(pageIndex) && pageImageDict[pageIndex] != null)
            {
                List<PageImageItem> imageItems = pageImageDict[pageIndex];
                if (ContainImageItem(pageIndex, ImageIndex) == false)
                {
                    imageItems.Add(new PageImageItem()
                    {
                        PageIndex = pageIndex,
                        PaintRect = paintRect,
                        PageRotate = pageRotate,
                        ImageIndex = ImageIndex
                    });
                    result = true;
                }
            }
            else
            {
                List<PageImageItem> imageItems = new List<PageImageItem>();
                imageItems.Add(new PageImageItem()
                {
                    PageIndex = pageIndex,
                    PaintRect = paintRect,
                    PageRotate = pageRotate,
                    ImageIndex = ImageIndex
                });

                pageImageDict.Add(pageIndex, imageItems);
                result = true;
            }
            return result;
        }

        private bool ContainImageItem(int pageIndex, int ImageIndex)
        {
            bool result = false;
            if (pageImageDict != null && pageImageDict.ContainsKey(pageIndex))
            {
                List<PageImageItem> imageItems = pageImageDict[pageIndex];
                if (imageItems != null && imageItems.Count > 0 && imageItems.Where(x => x.ImageIndex == ImageIndex).Count() > 0)
                {
                    result = true;
                }
            }
            return result;
        }
        private bool RemoveImageItem(int pageIndex, int ImageIndex)
        {
            bool result = false;
            if (pageImageDict.ContainsKey(pageIndex) && pageImageDict[pageIndex] != null)
            {
                List<PageImageItem> imageItems = pageImageDict[pageIndex];

                if (imageItems.Count > 0)
                {
                    List<PageImageItem> delItems = imageItems.Where(x => x.ImageIndex == ImageIndex).ToList();
                    if (delItems.Count > 0)
                    {
                        result = true;
                    }
                    foreach (PageImageItem delItem in delItems)
                    {
                        imageItems.Remove(delItem);
                    }
                }
            }
            return result;
        }
        public bool ClearImageItem()
        {
            bool result = false;
            if (pageImageDict.Count > 0)
            {
                pageImageDict.Clear();
                result = true;
            }
            hoverImageItem = null;
            return result;
        }

        public PageImageItem GetHoverImageItem()
        {
            return hoverImageItem.Clone();
        }

        public Dictionary<int, List<PageImageItem>> GetSelectImageItems()
        {
            return pageImageDict;
        }
    }
}
