using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Viewer.Helper;
using ComPDFKit.Controls.PDFControlUI;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFThumbnailControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private PDFViewControl ViewControl;

        /// <summary>
        /// Whether the thumbnail has been loaded
        /// </summary>
        public bool ThumbLoaded { get; set; }

        /// <summary>
        /// A collection of scale factors
        /// </summary>
        private int[] thumbnailSize = { 50, 100, 150, 200, 300, 500 };

        /// <summary>
        /// Scale factor
        /// </summary>
        private int zoomLevel = 2;

        /// <summary>
        /// A list of thumbnail data
        /// </summary>
        private List<ThumbnailItem> thumbnailItemList { get; set; } = new List<ThumbnailItem>();

        private List<int> cachePageList = new List<int>();
        private delegate void OnThumbnailGeneratedEventHandler(int pageIndex, byte[] thumb, int w, int h);
        private OnThumbnailGeneratedEventHandler OnThumbnailGenerated;
        public CPDFThumbnailControl()
        {
            InitializeComponent();
            Loaded += PdfThumbnail_Loaded;
        }

        /// <summary>
        /// Load completion event
        /// </summary>
        private void PdfThumbnail_Loaded(object sender, RoutedEventArgs e)
        {
            ThumbControl.ViewChanged -= ThumbControl_ViewChanged;
            ThumbControl.SelectionChanged -= ThumbControl_SelectionChanged;

            ThumbControl.ViewChanged += ThumbControl_ViewChanged;
            ThumbControl.SelectionChanged += ThumbControl_SelectionChanged;
        }

        /// <summary>
        /// The thumbnail list selects the change event
        /// </summary>
        private void ThumbControl_SelectionChanged(object sender, int e)
        {
            if(ViewControl!=null &&ViewControl.PDFViewTool!=null)
            {
                CPDFViewer pdfViewer=ViewControl.PDFViewTool.GetCPDFViewer();
                pdfViewer?.GoToPage(e, new Point(0, 0));
            }
        }

        /// <summary>
        /// Thumbnail content scrolling events
        /// </summary>
        private void ThumbControl_ViewChanged(object sender, ScrollChangedEventArgs e)
        {
            LoadVisibleThumbs();
        }

        /// <summary>
        /// Load thumbnails
        /// </summary>
        public void LoadThumb()
        {
            if (ViewControl == null || ViewControl.PDFViewTool == null || ThumbLoaded)
            {
                return;
            }

            CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
            CPDFDocument document= pdfViewer.GetDocument();
            if (document==null || document.IsLocked)
            {
                return;
            };
            pdfViewer.UpdateVirtualNodes();
            pdfViewer.UpdateRenderFrame();
            cachePageList.Clear();
            OnThumbnailGenerated -= ThumbnailGenerated;
            OnThumbnailGenerated += ThumbnailGenerated;

            pdfViewer.DrawChanged-= PdfView_DrawChanged;
            pdfViewer.DrawChanged += PdfView_DrawChanged;

            PopulateThumbnailList();
            LoadVisibleThumbs();
        }

        private void PdfView_DrawChanged(object sender, EventArgs e)
        {
            if (ViewControl != null && ViewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                if (pdfViewer != null && pdfViewer.CurrentRenderFrame != null)
                {
                    SelectThumbItemWithoutGoTo(pdfViewer.CurrentRenderFrame.PageIndex);
                }
            }
        }

        /// <summary>
        /// Set up PDFViewer
        /// </summary>
        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            ViewControl = viewControl;
        }

        /// <summary>
        /// Set the selected thumbnail
        /// </summary>
        public void SelectThumbItem(int newIndex)
        {
            ThumbControl?.SelectItem(newIndex);
        }


        public void SelectThumbItemWithoutGoTo(int newIndex)
        {
            ThumbControl?.SelectItemWithoutGoTo(newIndex);
        }

        private void PopulateThumbnailList()
        {
            int thumbnailWidth = thumbnailSize[zoomLevel];
            thumbnailItemList.Clear();
            if (ViewControl == null || ViewControl.PDFViewTool == null)
            {
                return;
            }

            CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
            CPDFDocument pdfdoc= pdfViewer?.GetDocument();
            if (pdfdoc == null)
            {
                return;
            }
            for (int i = 0; i < pdfdoc.PageCount; i++)
            {
                Size pageSize = DataConversionForWPF.CSizeConversionForSize(pdfdoc.GetPageSize(i));

                int imageWidth = 0; 
                int imageHeight = 0; 

                if(pageSize.Width>0 && pageSize.Height>0)
                {
                    imageWidth = pageSize.Width > pageSize.Height ? thumbnailWidth * 2 : (int)(pageSize.Width / pageSize.Height * thumbnailWidth * 2);
                    imageHeight = pageSize.Height > pageSize.Width ? thumbnailWidth * 2 : (int)(pageSize.Height / pageSize.Width * thumbnailWidth * 2);
                    Image img = new Image()
                    {
                        Margin = new Thickness(0, 0, 5, 0),

                        Width = imageWidth,
                        Height = imageHeight,
                        Stretch = Stretch.Uniform,
                    };

                    ThumbnailItem addItem = new ThumbnailItem();
                    addItem.ImageHeight = imageHeight;
                    addItem.ImageWidth = imageWidth;
                    addItem.ThumbnailHeight = thumbnailWidth;
                    addItem.ThumbnailWidth = thumbnailWidth;
                    addItem.PageIndex = i;
                    addItem.ImageData = img;
                    thumbnailItemList.Add(addItem);
                }
            }

            ThumbControl.SetThumbResult(thumbnailItemList);
        }

        private async void LoadVisibleThumbs()
        {
            try
            {
                foreach (ThumbnailItem item in thumbnailItemList)
                {
                    if (ThumbControl.IsItemVisible(item) == false)
                    {
                        continue;
                    }

                    if (item.ImageData == null || item.ImageData.Source == null)
                    {
                        if (cachePageList.Contains(item.PageIndex) == false)
                        {
                            cachePageList.Add(item.PageIndex);
                            await GetThumbnail(item.PageIndex, item.ImageWidth, item.ImageHeight);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ThumbnailGenerated(int pageIndex, byte[] thumb, int w, int h)
        {
            try
            {
                if (thumbnailItemList != null && thumbnailItemList.Count > pageIndex)
                {
                    PixelFormat fmt = PixelFormats.Bgra32;
                    BitmapSource bps = BitmapSource.Create(w, h, 96.0, 96.0, fmt, null, thumb, (w * fmt.BitsPerPixel + 7) / 8);
                    ThumbnailItem thumbItem = thumbnailItemList[pageIndex];
                    thumbItem.ImageData.Source = bps;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ThumbControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LoadVisibleThumbs();
        }

        private void ThumbControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;

                if (e.Delta < 0)
                {
                    zoomLevel = Math.Max(0, --zoomLevel);
                }
                else
                {
                    zoomLevel = Math.Min(thumbnailSize.Length - 1, ++zoomLevel);
                }

                LoadThumb();
            }
        }

        public async Task GetThumbnail(int pageIndex, int imageWidth, int imageHeight)
        {
            if (imageWidth <= 0 || imageHeight <= 0)
            {
                return;
            }
            if (ViewControl == null || ViewControl.PDFViewTool == null)
            {
                return;
            }

            CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
            CPDFDocument pdfdoc = pdfViewer?.GetDocument();
            if (pdfdoc == null)
            {
                return;
            }

            CPDFPage page = pdfdoc.PageAtIndex(pageIndex);
            byte[] bmpData = new byte[imageWidth * imageHeight * 4];

            await Task.Run(() => page.RenderPageBitmap(0, 0, imageWidth, imageHeight, 0xFFFFFFFF, bmpData, 1, true));
            if (OnThumbnailGenerated != null)
            {
                OnThumbnailGenerated(pageIndex, bmpData, imageWidth, imageHeight);
            }
        }
    }
}
