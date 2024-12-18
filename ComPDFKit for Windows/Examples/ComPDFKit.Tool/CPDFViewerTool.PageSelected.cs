using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer.Layer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComPDFKit.Tool
{
    public class PageSelectedData
    {
        public int PageIndex { get; set; }
        public Rect DrawRect { get; set; }
        public Rect RawRect { get; set; }
        public Rect SelectRect { get; set; }
    }
    public partial class CPDFViewerTool
    {
        int pageSelectedRectViewTag = -1;
        int pageIndex = -1;
        public event EventHandler<PageSelectedData> PageSelectedChanging;
        public event EventHandler<PageSelectedData> PageSelectedChanged;
        private void InsertPageSelectedRectView()
        {
            int selectedRectViewIndex = PDFViewer.GetMaxViewIndex();
            CustomizeLayer customizeLayer = new CustomizeLayer();
            PageSelectedRect pageSelectedRect = new PageSelectedRect(GetDefaultDrawParam());
            pageSelectedRect.SetDrawMoveType(DrawMoveType.kReferenceLine);
            customizeLayer.Children.Add(pageSelectedRect);
            pageSelectedRect.DataChanged += PageSelectedRect_DataChanged;
            pageSelectedRect.DataChanging += PageSelectedRect_DataChanging; ;
            PDFViewer.InsertView(selectedRectViewIndex, customizeLayer);
            pageSelectedRectViewTag = customizeLayer.GetResTag();
        }

        private void PageSelectedRect_DataChanging(object sender, Rect e)
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));

            if (pageSelectedRect != null)
            {
                PageSelectedData pageSelectedData = new PageSelectedData();
                pageSelectedData.PageIndex = pageIndex;
                pageSelectedData.DrawRect = e;
                PageSelectedChanged?.Invoke(this, pageSelectedData);
            }
        }

        private void PageSelectedRect_DataChanged(object sender, Rect e)
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));

            if (pageSelectedRect != null)
            {
                Canvas canvas = CommonHelper.FindVisualChild<Canvas>(pageSelectedRect);
                if (canvas != null)
                {
                    UserControl userControl = CommonHelper.FindVisualChild<UserControl>(canvas);
                    if (userControl != null)
                    {
                        userControl.Visibility = Visibility.Collapsed;
                        if (e.Width > 0 && e.Height > 0)
                        {
                            userControl.Visibility = Visibility.Visible;
                            userControl.Measure(pageSelectedRect.GetMaxRect().Size);
                            Size desireSize = userControl.DesiredSize;
                            if (e.Bottom + desireSize.Height + 3 > Math.Min(pageSelectedRect.GetMaxRect().Bottom, PDFViewer.ViewportHeight))
                            {
                                userControl.SetValue(Canvas.LeftProperty, e.Right - desireSize.Width);
                                double topPos = (int)Math.Max(e.Top - desireSize.Height - 3, 0);

                                userControl.SetValue(Canvas.TopProperty, topPos);
                            }
                            else
                            {
                                userControl.SetValue(Canvas.LeftProperty, e.Right - desireSize.Width);
                                userControl.SetValue(Canvas.TopProperty, e.Bottom + 3);
                            }
                            if (e.Right - desireSize.Width < 0)
                            {
                                userControl.SetValue(Canvas.LeftProperty, 3D);
                            }
                        }
                    }
                    PageSelectedData pageSelectedData = new PageSelectedData();
                    pageSelectedData.PageIndex = pageIndex;
                    pageSelectedData.DrawRect = e;
                    pageSelectedData.RawRect = Rect.Empty;
                    pageSelectedData.SelectRect = Rect.Empty;
                    try
                    {
                        Rect maxRect = pageSelectedRect.GetMaxRect();
                        Rect pageRect = new Rect(e.X - maxRect.X, e.Y - maxRect.Y, e.Width, e.Height);
                        pageSelectedData.SelectRect = pageRect;
                        double zoom = PDFViewer.GetZoom();
                        pageRect = new Rect(
                            pageRect.X / zoom / 96D * 72D, 
                            pageRect.Y / zoom / 96D * 72D, 
                            pageRect.Width / zoom / 96D * 72D,
                            pageRect.Height / zoom / 96D * 72D);

                        pageSelectedData.RawRect = pageRect;
                    }
                    catch (Exception ex)
                    {

                    }

                    PageSelectedChanged?.Invoke(this, pageSelectedData);
                }
            }
        }

        public void DrawStartPageSelectedRect()
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));

            if (pageSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                pageSelectedRect.Draw();
                pageSelectedRect.OnMouseLeftButtonDown(point);
                PDFViewer.CanHorizontallyScroll = false;
                PDFViewer.CanVerticallyScroll = false;
                PDFViewer.EnableZoom(false);
            }
        }

        public void DrawMovePageSelectedRect()
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));

            if (pageSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                pageSelectedRect.OnMouseMove(point, out bool Tag, PDFViewer.ActualWidth, PDFViewer.ActualHeight);
            }
        }

        public void DrawEndPageSelectedRect()
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));

            if (pageSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                pageSelectedRect.OnMouseLeftButtonUp(point);
            }
        }

        public void SelectedPageSelectedRect(Rect selectedRects, Rect MaxRect)
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));
            if (pageSelectedRect != null)
            {
                pageSelectedRect.SetRect(selectedRects);
                pageSelectedRect.SetMaxRect(MaxRect);
                pageSelectedRect.SetPDFViewerActualSize(PDFViewer.ActualWidth, PDFViewer.ActualHeight);
                pageSelectedRect.Draw();
            }
        }

        public bool HitTestPageSelectedRect()
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));
            if (pageSelectedRect != null)
            {
                if (pageSelectedRect.GetHitControlIndex(Mouse.GetPosition(this)) != PointControlType.None)
                {
                    return true;
                }
            }
            return false;
        }

        public void CleanPageSelectedRect()
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));
            if (pageSelectedRect != null)
            {
                pageSelectedRect.ClearDraw(); 
                Canvas canvas = CommonHelper.FindVisualChild<Canvas>(pageSelectedRect);
                if (canvas != null)
                {
                    UserControl userControl = CommonHelper.FindVisualChild<UserControl>(canvas);
                    if (userControl != null)
                    {
                        userControl.Visibility=Visibility.Collapsed;
                    }
                }
                pageIndex = -1;
                PDFViewer.CanHorizontallyScroll = true;
                PDFViewer.CanVerticallyScroll = true;
                PDFViewer.EnableZoom(true);
            }
        }
            
        public void CreatePageSelectdRect()
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));
            if (pageSelectedRect != null)
            {
                Point point = Mouse.GetPosition(this);
                BaseLayer baseLayer = PDFViewer.GetViewForTag(CreateAnnotTag);
                PDFViewer.GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
                if (index < 0)
                {
                    return;
                }
                pageIndex = index;
                CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);

                Point cropPoint = new Point();
                if (PDFViewer.GetIsCrop())
                {
                    CRect cRect = cPDFPage.GetCropBounds();
                    cropPoint.X = DpiHelper.PDFNumToStandardNum(cRect.left);
                    cropPoint.Y = DpiHelper.PDFNumToStandardNum(cRect.top);
                }
                pageSelectedRect.CreateRect(point, cropPoint, pageBound, PDFViewer.ActualWidth, PDFViewer.ActualHeight);
                pageSelectedRect.Draw();
                PDFViewer.CanHorizontallyScroll = false;
                PDFViewer.CanVerticallyScroll = false;
                PDFViewer.EnableZoom(false);
            }
        }

        public void SetPageSelectdUserControl(UserControl control)
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));
            if (pageSelectedRect != null)
            {
                pageSelectedRect.Children.Clear();
                Canvas popCanvas = new Canvas();
                pageSelectedRect.Children.Add(popCanvas);
                popCanvas.Children.Add(control);
                control.Visibility = Visibility.Collapsed;
                pageSelectedRect.Arrange();
            }
        }

        public void RemovePageSelectdUserControl()
        {
            PageSelectedRect pageSelectedRect = CommonHelper.FindVisualChild<PageSelectedRect>(PDFViewer.GetViewForTag(pageSelectedRectViewTag));
            if (pageSelectedRect != null)
            {
                pageSelectedRect.Children.Clear();
                pageIndex = -1;
            }
        }
    }
}
