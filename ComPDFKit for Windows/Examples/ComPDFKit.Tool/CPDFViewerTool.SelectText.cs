using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer;
using ComPDFKit.Tool.Help;
using System.Windows.Media;
using System.Xml.Linq;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        int selectTextTag = -1;
        private void InsertSelectTextView()
        {
            SelectText createAnnotTool = new SelectText();
            int SelectTextindex = PDFViewer.GetMaxViewIndex();
            PDFViewer.InsertView(SelectTextindex, createAnnotTool);
            selectTextTag = createAnnotTool.GetResTag();
        }

        public bool IsText()
        {
            PDFViewer.GetMousePointToPage(out int pageindex, out Point pagepoint);
            return PDFHelp.IsTextAtPos(PDFViewer.GetDocument(), pageindex, new Point(DpiHelper.StandardNumToPDFNum(pagepoint.X / PDFViewer.CurrentRenderFrame.ZoomFactor), DpiHelper.StandardNumToPDFNum(pagepoint.Y / PDFViewer.CurrentRenderFrame.ZoomFactor)));
        }

        public void DrawStartSelectText()
        {
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            PDFViewer.GetMousePointToPage(out int pageindex, out Point pagepoint);
            (baseLayer as SelectText).StartDraw(new Point(DpiHelper.StandardNumToPDFNum(pagepoint.X / PDFViewer.CurrentRenderFrame.ZoomFactor), DpiHelper.StandardNumToPDFNum(pagepoint.Y / PDFViewer.CurrentRenderFrame.ZoomFactor)), pageindex);
        }

        public void DrawMoveSelectText(bool DoubleClick)
        {
            if (PDFViewer.CurrentRenderFrame == null)
            {
                return;
            }
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            PDFViewer.GetMousePointToPage(out int pageindex, out Point pagepoint);
            (baseLayer as SelectText).MoveDraw(new Point(DpiHelper.StandardNumToPDFNum(pagepoint.X / PDFViewer.CurrentRenderFrame.ZoomFactor), DpiHelper.StandardNumToPDFNum(pagepoint.Y / PDFViewer.CurrentRenderFrame.ZoomFactor)), pageindex, PDFViewer, new Point(10, 10), DoubleClick);
        }

        public void DrawEndSelectText()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            (baseLayer as SelectText).EndDraw();
        }

        public bool GetMousePointToTextSelectInfo()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            TextSelectInfo selectTextInfo = (baseLayer as SelectText).GetTextSelectInfo();
            PDFViewer.GetMousePointToPage(out int pageindex, out Point pagepoint);
            bool PressOnSelectedText = false;
            if (selectTextInfo.PageSelectTextRectList.Count <= 0)
            {
                PressOnSelectedText = false;
                return PressOnSelectedText;
            }
            if (selectTextInfo.PageSelectTextRectList.ContainsKey(pageindex))
            {
                foreach (TextDrawRect textRect in selectTextInfo.PageSelectTextRectList[pageindex])
                {
                    Rect RawPaintRect = new Rect(
                        DpiHelper.PDFNumToStandardNum(textRect.DrawRect.Left) * currentZoom,
                           DpiHelper.PDFNumToStandardNum(textRect.DrawRect.Top) * currentZoom,
                             DpiHelper.PDFNumToStandardNum(textRect.DrawRect.Width) * currentZoom,
                             DpiHelper.PDFNumToStandardNum(textRect.DrawRect.Height) * currentZoom);

                    if (RawPaintRect.Contains(pagepoint))
                    {
                        PressOnSelectedText = true;
                        break;
                    }
                }
            }
            return PressOnSelectedText;
        }

        public TextSelectInfo GetTextSelectInfo()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            return (baseLayer as SelectText).GetTextSelectInfo();
        }

        public void ReDrawSelectText()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            if ((baseLayer as SelectText).HasSelectTextInfo())
            {
                (baseLayer as SelectText).Draw(PDFViewer);
            }
            else
            {
                (baseLayer as SelectText).CleanDraw(PDFViewer);
            }
        }

        public void ReDrawSearchText()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            if ((baseLayer as SelectText).HasSearchInfo())
            {
                (baseLayer as SelectText).Draw(PDFViewer);
            }
            else
            {
                (baseLayer as SelectText).CleanDraw(PDFViewer);
            }
        }

        public void RemoveSelectTextData()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            if ((baseLayer as SelectText).HasSelectTextInfo())
            {
                (baseLayer as SelectText).RemoveSelectDataInfo();
                (baseLayer as SelectText).Draw(PDFViewer);
            }
        }

        /// <summary>
        /// Set all search results
        /// </summary>
        /// <param name="searchTexts"></param>
        public void SetPageSelectText(List<TextSearchItem> searchTexts)
        {
            if (searchTexts.Count > 0)
            {
                TextSelectInfo searchInfo = new TextSelectInfo();
                searchInfo.StartPage = searchTexts.Min(x => x.PageIndex);
                searchInfo.EndPage = searchTexts.Max(x => x.PageIndex);
                searchInfo.PageRotate = searchTexts[0].PageRotate;
                searchInfo.RotateRecord = true;
                List<int> pageIndexList = (from u in searchTexts select u.PageIndex).Distinct().ToList();
                foreach (int pageIndex in pageIndexList)
                {
                    List<TextSearchItem> pageTexts = searchTexts.Where(x => x.PageIndex == pageIndex).ToList();
                    foreach (TextSearchItem textItem in pageTexts)
                    {
                        if (!searchInfo.PageSelectTextRectList.ContainsKey(pageIndex))
                        {
                            searchInfo.PageSelectTextRectList[pageIndex] = new List<TextDrawRect>();
                        }

                        searchInfo.PageSelectTextRectList[pageIndex].Add(new TextDrawRect()
                        {
                            Text = textItem.TextContent,
                            DrawRect = textItem.TextRect,
                            PaintBrush = textItem.PaintBrush,
                            DrawActiveSearch = false,
                            SearchInfo = textItem
                        });
                    }
                }

                BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
                (baseLayer as SelectText).SetSearchInfo(searchInfo);
            }
        }

        /// <summary>
        /// Clear the previously cached search results
        /// </summary>
        public void CleanSearchInfo()
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            (baseLayer as SelectText).CleanSearchInfo();
        }

        public void HighLightSearchText(List<TextSearchItem> selectTexts)
        {
            BaseLayer baseLayer = PDFViewer.GetViewForTag(selectTextTag);
            if ((baseLayer as SelectText).HasSearchInfo())
            {
                TextSelectInfo searchTextInfo = (baseLayer as SelectText).GetSearchInfo();
                if (searchTextInfo.PageSelectTextRectList != null)
                {
                    foreach (int pageIndex in searchTextInfo.PageSelectTextRectList.Keys)
                    {
                        List<TextDrawRect> drawSearchList = searchTextInfo.PageSelectTextRectList[pageIndex];
                        foreach (TextDrawRect drawRect in drawSearchList)
                        {
                            drawRect.DrawActiveSearch = selectTexts.Contains(drawRect.SearchInfo);
                            if (drawRect.DrawActiveSearch)
                            {
                                drawRect.PaintBrush = drawRect.SearchInfo.PaintBrush;
                            }
                            else
                            {
                                drawRect.PaintBrush = drawRect.SearchInfo.BorderBrush;
                            }
                        }
                    }
                }
            } 
        }
    }
}
