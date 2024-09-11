using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        int selectImageTag = -1;
        private void InsertSelectImageView()
        {
            SelectImage createAnnotTool = new SelectImage();
            int SelectTextindex = PDFViewer.GetMaxViewIndex();
            PDFViewer.InsertView(SelectTextindex, createAnnotTool);
            selectImageTag = createAnnotTool.GetResTag();
        }
        public bool DrawMoveSelectImage()
        {
            bool isSelect = false;
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return isSelect;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectImageTag);
            PDFViewer.GetMousePointToPage(out int pageindex, out Point pagepoint);
            isSelect=(baseLayer as SelectImage).ProcessMouseMoveForSelectImage(
                new Point(DpiHelper.StandardNumToPDFNum(pagepoint.X / PDFViewer.CurrentRenderFrame.ZoomFactor),
                DpiHelper.StandardNumToPDFNum(pagepoint.Y / PDFViewer.CurrentRenderFrame.ZoomFactor)),
                pageindex, PDFViewer);
            (baseLayer as SelectImage).Draw(PDFViewer); 
            return isSelect;
        }

        public bool DrawDownSelectImage(bool isNeedClear)
        {
            bool isSelect = false;
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return isSelect;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectImageTag);
            PDFViewer.GetMousePointToPage(out int pageindex, out Point pagepoint);
            isSelect=(baseLayer as SelectImage).ProcessMouseDownForSelectImage(
                new Point(DpiHelper.StandardNumToPDFNum(pagepoint.X / PDFViewer.CurrentRenderFrame.ZoomFactor),
                DpiHelper.StandardNumToPDFNum(pagepoint.Y / PDFViewer.CurrentRenderFrame.ZoomFactor)),
                pageindex, PDFViewer, isNeedClear);
            (baseLayer as SelectImage).Draw(PDFViewer);
            return isSelect;
        }


        public void ReDrawSelectImage()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectImageTag);
            (baseLayer as SelectImage).Draw(PDFViewer);
        }

        public void CleanDrawSelectImage()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectImageTag);
            (baseLayer as SelectImage).CleanDraw(PDFViewer);
        }

        //public PageImageItem GetSelectImage()
        //{
        //    BaseLayer baseLayer = PDFViewer.GetViewForTag(selectImageTag);
        //    return (baseLayer as SelectImage).GetHoverImageItem();
        //}

        public Dictionary<int, List<PageImageItem>> GetSelectImageItems()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectImageTag);
            return (baseLayer as SelectImage).GetSelectImageItems();
        }
    }
}
