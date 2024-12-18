using ComPDFKit.Tool.Help;
using ComPDFKit.Viewer.Layer;
using ComPDFKitViewer;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        private int PDFCompareViewID { get; set; } = -1;
        private int PDFComparePageIndex {  get; set; } = -1;
        private List<Rect> PDFCompareRectList {  get; set; }
        private SolidColorBrush PDFCompareBrush { get; set; }
        private Pen PDFComparePen { get; set; }
        private CustomizeLayer GetPDFCompareDrawView()
        {
            if(PDFViewer==null)
            {
                return null;
            }
            if (PDFCompareViewID != -1)
            {
                return PDFViewer.GetViewForTag(PDFCompareViewID) as CustomizeLayer;
            }
            int maxId = PDFViewer.GetMaxViewIndex();
            CustomizeLayer customizeLayer = new CustomizeLayer();
            PDFViewer.InsertView(maxId, customizeLayer);
            PDFCompareViewID = customizeLayer.GetResTag();
            PDFViewer.DrawChanged -= DrawChangedNotify;
            PDFViewer.DrawChanged += DrawChangedNotify;
            return customizeLayer;
        }

        private void DrawPDFCompare()
        {
            if (PDFCompareViewID==-1)
            {
                return;
            }
            CustomizeLayer drawLayer = GetPDFCompareDrawView();
            if (drawLayer == null)
            {
                return;
            }

            if (PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }

            RenderFrame frame = PDFViewer.CurrentRenderFrame;
            List<RenderData> drawDataList = frame.GetRenderDatas();
            if (drawDataList == null || drawDataList.Count == 0)
            {
                return;
            }

            RenderData drawData = null;
            foreach (RenderData data in drawDataList)
            {
                if (data.PageIndex == PDFComparePageIndex)
                {
                    drawData = data;
                    break;
                }
            }

          DrawingContext drawDC=  drawLayer.RenderOpen();

            if (drawData != null && PDFCompareRectList != null)
            {
                foreach(Rect drawRect in PDFCompareRectList)
                {
                    Rect standRect = DpiHelper.PDFRectToStandardRect(drawRect);

                    Rect paintRect = new Rect(
                        standRect.Left * frame.ZoomFactor,
                        standRect.Top * frame.ZoomFactor,
                        standRect.Width * frame.ZoomFactor,
                        standRect.Height * frame.ZoomFactor);

                    Rect offsetRect = new Rect(
                        paintRect.Left + drawData.PageBound.Left,
                        paintRect.Top + drawData.PageBound.Top,
                        paintRect.Width,
                        paintRect.Height);

                    drawDC.DrawRectangle(PDFCompareBrush,PDFComparePen,offsetRect);
                }
            }

            drawDC.Close();
        }

        private void DrawChangedNotify(object sender, EventArgs e)
        {
            DrawPDFCompare();
        }

        public void SetPDFCompareView(SolidColorBrush FillBrush,Pen BorderPen,int PageIndex,List<Rect> PDFRectList)
        {
            PDFCompareRectList=PDFRectList;
            PDFComparePageIndex = PageIndex;
            PDFCompareBrush = FillBrush;
            PDFComparePen = BorderPen;
            if(PDFCompareViewID==-1)
            {
                GetPDFCompareDrawView();
            }
            DrawPDFCompare();
        }

        public void ClearPDFCompareView()
        {
            CustomizeLayer drawLayer = GetPDFCompareDrawView();
            if (drawLayer != null && PDFViewer != null)
            {
                PDFViewer.RemoveView(drawLayer);
                PDFCompareViewID = -1;
            }
        }
    }
}
