using ComPDFKit.PDFPage;
using ComPDFKit.PDFWatermark;
using ComPDFKit.Controls.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ComPDFKit.Controls.PDFControl
{
    public class WatermarkPreviewControl : PreviewControl
    {
        public List<int> MarkedPageList = new List<int>();

        private CPDFWatermark watermark;

        private WatermarkData _watermarkData;
        public WatermarkData WatermarkData
        {
            get => _watermarkData;
            set
            {
                _watermarkData = value;

                if (watermark != null)
                { 
                    watermark?.ClearWatermark();
                    watermark?.Release();
                    lock (queueLock)
                    {
                        Document.ReleasePages();
                    }
                }
                watermark = Document.InitWatermark(WatermarkData.Type);
                if (IsLoaded)
                {
                    OnWatermarkChanged();
                }
            }
        }

        public event EventHandler WatermarkChanged;

        public WatermarkPreviewControl()
        {
            InitializeComponent();
            
            Loaded -= WatermarkPreviewControl_Loaded;
            Loaded += WatermarkPreviewControl_Loaded;
        }

        private void WatermarkPreviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentIndexChanged += WatermarkPreviewControl_CurrentIndexChanged;
        }

        private void WatermarkPreviewControl_CurrentIndexChanged(object sender, EventArgs e)
        {
            DeleteDisplayWatermark();
        }

        private void OnWatermarkChanged()
        {
            WatermarkChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateByWatermarkData()
        {
            if (WatermarkData == null || !MarkedPageList.Contains(CurrentIndex))
            {
                return;
            }
            if (WatermarkData.Type == ComPDFKit.PDFWatermark.C_Watermark_Type.WATERMARK_TYPE_TEXT)
            {
                UpdateByTextWatermarkData();
            }
            else if (WatermarkData.Type == ComPDFKit.PDFWatermark.C_Watermark_Type.WATERMARK_TYPE_IMG && !string.IsNullOrEmpty(WatermarkData.ImagePath))
            {
                UpdateByImageWatermarkData();
            }
            else
            {
                return;
            }
        }

        private void UpdateByImageWatermarkData()
        {
            Bitmap bitmap = new Bitmap(WatermarkData.ImagePath);
            bitmap = CommonHelper.ConvertTo32bppArgb(bitmap);
            byte[] byteArray = CommonHelper.ConvertBitmapToByteArray(bitmap);
            watermark.SetImage(byteArray, bitmap.Width, bitmap.Height);
            watermark.SetScale((float)(WatermarkData.ImageScale / 100.0));
            UpdateByCommonWatermarkData();
        }

        private void UpdateByTextWatermarkData()
        {
            watermark.SetText(WatermarkData.Text);
            watermark.SetFontName(WatermarkData.FontName);
            watermark.SetTextRGBColor(WatermarkData.Color);
            watermark.SetFontSize(WatermarkData.FontSize);
            watermark.SetScale(1);
            UpdateByCommonWatermarkData();
        }

        private void UpdateByCommonWatermarkData()
        {
            watermark.SetPages((PageRangeList[CurrentIndex - 1] - 1).ToString());
            watermark.SetRotation((float)(WatermarkData.Rotation * Math.PI / 180.0));
            watermark.SetOpacity(WatermarkData.Opacity);
            watermark.SetVertalign((C_Watermark_Vertalign)(WatermarkData.Align / 3));
            watermark.SetHorizalign((C_Watermark_Horizalign)(WatermarkData.Align % 3));
            watermark.SetVertOffset(WatermarkData.VertOffset);
            watermark.SetHorizOffset(WatermarkData.HorizOffset);
            watermark.SetFront(WatermarkData.IsFront);
            watermark.SetFullScreen(WatermarkData.IsFullScreen);
            watermark.SetVerticalSpacing(WatermarkData.VerticalSpacing);
            watermark.SetHorizontalSpacing(WatermarkData.HorizontalSpacing);
            watermark.CreateWatermark();
            watermark.UpdateWatermark();
        }

        public void DeleteDisplayWatermark()
        {
            if (watermark != null)
            {
                Document.ReleasePages();
                watermark?.ClearWatermark();
            }
        }

        override public void BeginLoadImageThread(int pageIndex)
        {
            UpdateByWatermarkData();

            base.BeginLoadImageThread(pageIndex);
        }

        override public void AttachLoaded()
        {
            WatermarkChanged += WatermarkPreviewControl_WatermarkChanged;
            OnWatermarkChanged();
        }

        private void WatermarkPreviewControl_WatermarkChanged(object sender, EventArgs e)
        {
            BeginLoadImageThread(PageRangeList[CurrentIndex - 1] - 1);
        }
    }
}
