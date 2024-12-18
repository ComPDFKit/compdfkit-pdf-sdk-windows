using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Widget;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKitViewer.BaseObject;
using ComPDFKit.Tool.SettingParam;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Viewer.Helper;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool.Help;
using ComPDFKit.Measure;
using System.Dynamic;
using System.Globalization;
using ComPDFKitViewer.Layer;
using ComPDFKitViewer;
using ComPDFKitViewer.Annot;

namespace ComPDFKit.Tool
{
    public class CPDFToolManager
    {
        private CPDFViewerTool viewerTool;

        public event EventHandler<MouseEventObject> MouseLeftButtonDownHandler;
        public event EventHandler<MouseEventObject> MouseLeftButtonUpHandler;
        public event EventHandler<MouseEventObject> MouseMoveHandler;
        public event EventHandler<MouseEventObject> MouseRightButtonDownHandler;
        public event EventHandler<SelectedAnnotData> AnnotDefaultEditedHandler;
        public event EventHandler<MultiSelectedData> MulitDefaultEditedHandler;

        /// <summary>
        /// Current tool type, pay attention to the use of if writing, reserve the subsequent or operation mode switch
        /// </summary>
        private ToolType currentToolType = ToolType.None;

        /// <summary>
        /// Identify whether it is a selected text state or a drag box state for content editing
        /// </summary>
        private bool editSelected = true;
        private bool clickEditSelected = false;

        /// <summary>
        /// Current creation annotation type
        /// </summary>
        private C_ANNOTATION_TYPE createAnnotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE;

        /// <summary>
        /// Current creation Widget type
        /// </summary>
        private C_WIDGET_TYPE createWidgetType = C_WIDGET_TYPE.WIDGET_NONE;

        /// <summary>
        /// Current creation content edit type
        /// </summary>
        private CPDFEditType createContentEditType = CPDFEditType.None;

        /// <summary>
        /// Current creation data layer annotation object (effective after Down, invalid after Up, if you need to modify the creation attributes, you need to do this)
        /// </summary>
        private CPDFAnnotation cPDFAnnotation;

        /// <summary>
        /// Image path cache used for image creation (subject to possible adjustments later)
        /// </summary>
        private string createImagePath = string.Empty;

        private bool isActiveCropping = true;

        /// <summary>
        ///  add TextEdit Cursor
        /// </summary>
        private Cursor addTextEditCursor = Cursors.IBeam;

        /// <summary>
        /// add ImageEdit Cursor 
        /// </summary>
        private Cursor addImageEditCursor = Cursors.Arrow;

        public bool SaveEmptyStickyAnnot { get; set; } = true;
        public void SetActiveCropping(bool isActiveCropping)
        {
            this.isActiveCropping = isActiveCropping;
        }

        public CPDFToolManager(CPDFViewerTool cPDFTool) : base()
        {
            viewerTool = cPDFTool;
            viewerTool.MouseLeftButtonDownHandler += ViewerTool_MouseLeftButtonDownHandler;
            viewerTool.MouseMoveHandler += ViewerTool_MouseMoveHandler;
            viewerTool.MouseLeftButtonUpHandler += ViewerTool_MouseLeftButtonUpHandler;
            viewerTool.MouseRightButtonDownHandler += ViewerTool_MouseRightButtonDownHandler;
            viewerTool.SelectedDataChanged += ViewerTool_SelectedDataChanged;
            viewerTool.SelectedDataChanging += ViewerTool_SelectedDataChanging;
            viewerTool.MultiDataChanged += ViewerTool_MulitDataChanged;
            viewerTool.MultiDataChanging += ViewerTool_MulitDataChanging;
            viewerTool.DeleteChanged += ViewerTool_DeleteChanged;
            viewerTool.AnnotEditDataChanged += ViewerTool_AnnotEditDataChanged;
            viewerTool.AnnotEditDataChanging += ViewerTool_AnnotEditDataChanging;
        }

        public void Remove()
        {
            viewerTool.MouseLeftButtonDownHandler -= ViewerTool_MouseLeftButtonDownHandler;
            viewerTool.MouseMoveHandler -= ViewerTool_MouseMoveHandler;
            viewerTool.MouseLeftButtonUpHandler -= ViewerTool_MouseLeftButtonUpHandler;
            viewerTool.SelectedDataChanged -= ViewerTool_SelectedDataChanged;
            viewerTool.SelectedDataChanging -= ViewerTool_SelectedDataChanging;
            viewerTool.MultiDataChanged -= ViewerTool_MulitDataChanged;
            viewerTool.MultiDataChanging -= ViewerTool_MulitDataChanging;
            viewerTool.DeleteChanged -= ViewerTool_DeleteChanged;
            viewerTool.AnnotEditDataChanged -= ViewerTool_AnnotEditDataChanged;
            viewerTool.AnnotEditDataChanging -= ViewerTool_AnnotEditDataChanging;
            viewerTool = null;
        }

        public BaseAnnot GetCacheHitTestAnnot()
        {
            return viewerTool.GetCacheHitTestAnnot();
        }

        public BaseWidget GetCacheHitTestWidget()
        {
            return viewerTool.GetCacheHitTestWidget();
        }

        public CPDFViewerTool GetCPDFViewerTool()
        {
            return viewerTool;
        }

        public CPDFDocument GetDocument()
        {
            return viewerTool.GetCPDFViewer().GetDocument();
        }

        public void SetCreateImagePath(string path)
        {
            createImagePath = path;
        }

        public ToolType GetToolType()
        {
            return currentToolType;
        }

        public bool SetCreateAnnotType(C_ANNOTATION_TYPE annotType)
        {
            viewerTool.SetIsHitTestLink(false);
            viewerTool.SetIsOnlyHitTestRedact(false);
            if (currentToolType != ToolType.CreateAnnot)
            {
                return false;
            }
            //MouseEventObject e= new MouseEventObject();
            //if (viewerTool.IsCanSave())
            //{
            //    SaveCreateAnnotation(ref cPDFAnnotation, ref e);
            //    viewerTool.PDFViewer.EnableZoom(true);
            //    viewerTool.PDFViewer.CanHorizontallyScroll = true;
            //    viewerTool.PDFViewer.CanVerticallyScroll = true;
            //}

            if (createAnnotType != annotType)
            {
                SaveCurrentAnnot();
                createAnnotType = annotType;
            }

            if (createAnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_LINK)
            {
                viewerTool.SetIsHitTestLink(true);
            }

            if (createAnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_REDACT)
            {
                viewerTool.SetIsOnlyHitTestRedact(true);
            }
            switch (annotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                    TextSelectInfo textSelectInfo = viewerTool.GetTextSelectInfo();
                    if (textSelectInfo.PageSelectPointList.Count > 0)
                    {
                        GroupHistory historyData = null;
                        viewerTool.CreateAnnotForSelectText(textSelectInfo, createAnnotType, out historyData);
                        viewerTool.RemoveSelectTextData();

                        if (historyData != null && historyData.Histories.Count > 0)
                        {
                            List<object> dataList = new List<object>();
                            if (historyData != null && historyData.Histories.Count > 0)
                            {
                                foreach (IHistory historyItem in historyData.Histories)
                                {
                                    AnnotHistory checkHistory = historyItem as AnnotHistory;
                                    if (checkHistory == null || checkHistory.CurrentParam == null)
                                    {
                                        continue;
                                    }
                                    dynamic expandData = new ExpandoObject();
                                    expandData.AnnotIndex = checkHistory.CurrentParam.AnnotIndex;
                                    expandData.PageIndex = checkHistory.CurrentParam.PageIndex;
                                    expandData.AnnotParam = checkHistory.CurrentParam;
                                    dataList.Add(expandData);
                                }
                            }

                            if (dataList.Count > 0)
                            {
                                MouseEventObject eventObject = new MouseEventObject();
                                eventObject.annotType = createAnnotType;
                                eventObject.IsCreate = true;
                                eventObject.Data = dataList.Count > 1 ? dataList : dataList[0];
                                MouseLeftButtonUpHandler?.Invoke(this, eventObject);
                            }

                        }
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public C_ANNOTATION_TYPE GetAnnotType()
        {
            return createAnnotType;
        }

        public bool SetCreateWidgetType(C_WIDGET_TYPE widgetType)
        {
            if (currentToolType != ToolType.WidgetEdit)
            {
                return false;
            }

            if (createWidgetType != widgetType)
            {
                SaveCurrentAnnot();
                createWidgetType = widgetType;
            }

            viewerTool.SetDrawWidgetType(createWidgetType);
            return true;
        }

        public C_WIDGET_TYPE GetCreateWidgetType()
        {
            return createWidgetType;
        }

        public bool SetCreateContentEditType(CPDFEditType editType)
        {
            createContentEditType = editType;
            return true;
        }

        public CPDFEditType GetCreateContentEditType()
        {
            return createContentEditType;
        }

        public void ClearSelect()
        {
            /// Clear some UI effects of other modules
            viewerTool.HideWidgetHitPop();
            viewerTool.CleanSelectedRect();
            viewerTool.CleanEditAnnot();
            viewerTool.CleanDrawSelectImage();
            viewerTool.ClearDrawWidget();
            viewerTool.CleanEditView();
            viewerTool.CleanCustomizeTool();
            viewerTool.GetCPDFViewer().SetCacheEditPage(false);
        }

        public void SetToolType(ToolType toolType)
        {
            if (currentToolType == toolType)
            {
                return;
            }

            viewerTool.RemovePopTextUI();
            /// Clear some UI effects of other modules
            viewerTool.HideWidgetHitPop();
            viewerTool.CleanSelectedRect();
            viewerTool.CleanEditAnnot();
            viewerTool.ClearDrawWidget();
            viewerTool.CleanEditView();
            viewerTool.CleanCustomizeTool();
            viewerTool.CleanDrawSelectImage();
            viewerTool.GetCPDFViewer().SetCacheEditPage(false);
            viewerTool.ClearDrawAnnot();
            viewerTool.CleanPageSelectedRect();
            viewerTool.SelectedAnnotForIndex(-1, -1);
            ToolType oldToolType = currentToolType;
            currentToolType = toolType;
            switch (toolType)
            {
                case ToolType.None:
                    break;
                case ToolType.Viewer:
                    viewerTool.SetToolType(ToolType.Viewer);
                    break;
                case ToolType.CreateAnnot:
                    viewerTool.SetToolType(ToolType.CreateAnnot);
                    break;
                case ToolType.Pan:
                    viewerTool.SetToolType(ToolType.Pan);
                    break;
                case ToolType.WidgetEdit:
                    viewerTool.SetToolType(ToolType.WidgetEdit);
                    viewerTool.RemoveSelectTextData();
                    break;
                case ToolType.ContentEdit:
                    viewerTool.SetToolType(ToolType.ContentEdit);
                    viewerTool.GetCPDFViewer().SetCacheEditPage(true);
                    viewerTool.RemoveSelectTextData();
                    break;
                case ToolType.SelectedPage:
                    viewerTool.SetToolType(ToolType.SelectedPage);
                    viewerTool.RemoveSelectTextData();
                    break;
                case ToolType.Customize:
                    viewerTool.SetToolType(ToolType.Customize);
                    viewerTool.RemoveSelectTextData();
                    break;

                default:
                    break;
            }

            if (oldToolType == ToolType.ContentEdit || currentToolType == ToolType.ContentEdit)
            {
                viewerTool.GetCPDFViewer().GetDocument()?.ReleasePages();
                //Undo delete logic
                viewerTool.GetCPDFViewer().UndoManager.ClearHistory();
                viewerTool.GetCPDFViewer().UpdateVirtualNodes();
                viewerTool.GetCPDFViewer().UpdateRenderFrame();
            }
        }

        public CPDFEditArea GetSelectedEditAreaObject(ref int pageIndex)
        {
            EditAreaObject editAreaObject = viewerTool.GetEditAreaObjectForRect(viewerTool.GetLastSelectedRect());
            if (editAreaObject == null)
            {
                pageIndex = -1;
            }
            else
            {
                pageIndex = editAreaObject.PageIndex;
            }
            return editAreaObject?.cPDFEditArea;
        }

        /// <summary>
        /// Get the index of the multi choice comment list
        /// </summary>
        /// <param name="pageIndexs"></param>
        /// <returns></returns>
        public List<CPDFEditArea> GetSelectedEditAreaListObject(ref List<int> pageIndexs)
        {
            List<CPDFEditArea> editAreaObjectlist = new List<CPDFEditArea>();
            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(viewerTool.PDFViewer.GetViewForTag(viewerTool.MultiSelectedRectViewTag));
            if (multiSelectedRect != null && multiSelectedRect.Children.Count > 0)
            {
                foreach (SelectedRect selectedRect in multiSelectedRect.Children)
                {
                    EditAreaObject editAreaObject = viewerTool.GetEditAreaObjectListForRect(selectedRect);
                    if (editAreaObject == null)
                    {
                        //pageIndexs.Add(-1);
                    }
                    else
                    {
                        if (!editAreaObjectlist.Contains(editAreaObject?.cPDFEditArea))
                        {
                            pageIndexs.Add(editAreaObject.PageIndex);
                            editAreaObjectlist.Add(editAreaObject?.cPDFEditArea);
                        }
                    }
                }
            }
            return editAreaObjectlist;
        }

        public CPDFAnnotation GetCPDFAnnotation()
        {
            return cPDFAnnotation;
        }

        public void SetPageSelectText(List<TextSearchItem> pageTextList)
        {
            viewerTool.Dispatcher.Invoke(() =>
            {
                viewerTool.SetPageSelectText(pageTextList);
                viewerTool.ReDrawSelectText();
            });
        }

        public void HighLightSearchText(List<TextSearchItem> pageTextList)
        {
            if (pageTextList.Count < 0)
            {
                return;
            }
            viewerTool.Dispatcher.Invoke(() =>
            {
                viewerTool.HighLightSearchText(pageTextList);
                viewerTool.ReDrawSearchText();
                viewerTool.GetCPDFViewer().GoToPage(pageTextList[0].PageIndex, new Point(pageTextList[0].TextRect.X, pageTextList[0].TextRect.Y));
            });
        }

        private void ViewerTool_DeleteChanged(object sender, List<AnnotParam> e)
        {
            viewerTool.GetCPDFViewer().UpdateAnnotFrame();
            AnnotDefaultEditedHandler?.Invoke(this, null);
        }

        private void ViewerTool_SelectedDataChanging(object sender, DrawTool.SelectedAnnotData e)
        {

        }

        private void ViewerTool_SelectedDataChanged(object sender, DrawTool.SelectedAnnotData e)
        {
            if (e.annotData == null)
            {
                return;
            }
            AnnotHistory annotHistory = ParamConverter.CreateHistory(e.annotData.Annot);
            if (annotHistory == null)
            {
                return;
            }
            AnnotParam previousParam;
            AnnotParam currentParam;
            switch (e.annotData.AnnotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    previousParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                    break;
                default:
                    previousParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                    break;
            }

            /// Change operation
            if (e.annotData.AnnotType != C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
            {
                Rect rect1 = new Rect(
                    (e.Square.Left - e.annotData.PaintOffset.X + (e.annotData.CropLeft * e.annotData.CurrentZoom)) / e.annotData.CurrentZoom,
                    (e.Square.Top - e.annotData.PaintOffset.Y + (e.annotData.CropTop * e.annotData.CurrentZoom)) / e.annotData.CurrentZoom,
                    e.Square.Width / e.annotData.CurrentZoom,
                    e.Square.Height / e.annotData.CurrentZoom
                    );

                Rect rect = DpiHelper.StandardRectToPDFRect(rect1);
                CRect cRect = new CRect((float)rect.Left, (float)rect.Bottom, (float)rect.Right, (float)rect.Top);
                if (e.annotData.AnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_STAMP)
                {
                    CPDFStampAnnotation stampAnnot = e.annotData.Annot as CPDFStampAnnotation;
                    stampAnnot.SetSourceRect(cRect);
                    stampAnnot.AnnotationRotator.SetRotation(-e.rotationAngle);
                }
                else
                {
                    e.annotData.Annot.SetRect(cRect);
                }
            }

            switch (e.annotData.AnnotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    {
                        currentParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                        (e.annotData.Annot as CPDFWidget).UpdateFormAp();
                        break;
                    }

                default:
                    {
                        currentParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                        if (e.annotData.AnnotType != C_ANNOTATION_TYPE.C_ANNOTATION_SOUND)
                        {
                            e.annotData.Annot.UpdateAp();
                            if (e.annotData.Annot is CPDFTextAnnotation)
                            {
                                CommonHelper.UpdateStickyAP(e.annotData.Annot as CPDFTextAnnotation);
                            }
                        }
                        break;
                    }
            }

            annotHistory.PreviousParam = previousParam;
            annotHistory.PDFDoc = viewerTool.GetCPDFViewer().GetDocument();
            annotHistory.CurrentParam = currentParam;
            annotHistory.Action = HistoryAction.Update;
            viewerTool.GetCPDFViewer().UndoManager.AddHistory(annotHistory);
            viewerTool.GetCPDFViewer().UpdateAnnotFrame();
            AnnotDefaultEditedHandler?.Invoke(this, e);
        }

        private void ViewerTool_MulitDataChanging(object sender, MultiSelectedData e)
        {

        }

        private void ViewerTool_MulitDataChanged(object sender, MultiSelectedData e)
        {
            switch (e.ObjectType)
            {
                case SelectedType.None:
                    break;
                case SelectedType.Annot:
                    break;
                case SelectedType.PDFEdit:
                    SaveToMulitChanged(e);
                    break;
                default:
                    break;
            }
            MulitDefaultEditedHandler?.Invoke(this, e);
        }

        private void SaveToMulitChanged(MultiSelectedData e)
        {
            GroupHistory groupHistory = new GroupHistory();
            CPDFDocument cPDFDocument = viewerTool.GetCPDFViewer().GetDocument();
            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(e.PageIndex);
            CPDFEditPage cPDFEditPage = cPDFPage.GetEditPage();
            List<CPDFEditArea> cPDFEditAreas = cPDFEditPage.GetEditAreaList(false);
            float zoom = (float)viewerTool.PDFViewer.GetZoom();
            for (int i = 0; i < e.MultiObjectIndex.Count; i++)
            {
                if (e.MultiObjectIndex[i] < cPDFEditAreas.Count)
                {
                    PDFEditHistory pDFEditHistory = new PDFEditHistory();
                    pDFEditHistory.PageIndex = e.PageIndex;
                    pDFEditHistory.EditPage = cPDFEditPage;
                    CRect cRect = cPDFEditAreas[e.MultiObjectIndex[i]].GetFrame();
                    if (cPDFEditAreas[e.MultiObjectIndex[i]].Type == CPDFEditType.EditImage)
                    {
                        cRect = (cPDFEditAreas[e.MultiObjectIndex[i]] as CPDFEditImageArea).GetClipRect();
                    }
                    Point point = DpiHelper.StandardPointToPDFPoint(e.MoveOffset);
                    point.X = point.X / zoom;
                    point.Y = point.Y / zoom;
                    if (point.X != 0 && point.Y != 0 && e.ZoomX == 1 && e.ZoomY == 1)
                    {
                        cRect.left += (float)point.X;
                        cRect.right += (float)point.X;
                        cRect.top += (float)point.Y;
                        cRect.bottom += (float)point.Y;
                    }
                    else
                    {
                        //Mobile scaling ratio logic
                        if ((float)point.X == 0)
                        {
                            cRect.left += (float)point.X;
                            cRect.right = cRect.left + (cRect.right - cRect.left) + (float)DpiHelper.StandardNumToPDFNum(e.ChangeX) / zoom;
                        }
                        else
                        {
                            cRect.left += (float)point.X;
                            if (e.ZoomX == 1)
                            {
                                cRect.right += (float)point.X;
                            }
                        }
                        if ((float)point.Y == 0)
                        {
                            cRect.top += (float)point.Y;
                            cRect.bottom = cRect.top + (cRect.bottom - cRect.top) + (float)DpiHelper.StandardNumToPDFNum(e.ChangeY) / zoom;
                        }
                        else
                        {

                            cRect.top += (float)point.Y;
                            if (e.ZoomY == 1)
                            {
                                cRect.bottom += (float)point.Y;
                            }
                        }
                    }
                    //Original Logic
                    //cRect.left += (float)point.X;
                    //cRect.right += (float)point.X;
                    //cRect.top += (float)point.Y;
                    //cRect.bottom += (float)point.Y;

                    //cRect.right = cRect.right* e.ZoomX;
                    //cRect.bottom = cRect.bottom * e.ZoomY;
                    cPDFEditAreas[e.MultiObjectIndex[i]].SetFrame(cRect);
                    groupHistory.Histories.Add(pDFEditHistory);
                }
            }
            //Add end edit
            cPDFEditPage.EndEdit();
            viewerTool.GetCPDFViewer().UndoManager.AddHistory(groupHistory);
            viewerTool.GetCPDFViewer().UpdateRenderFrame();
        }

        private void ViewerTool_AnnotEditDataChanging(object sender, SelectedAnnotData e)
        {

        }

        private void ViewerTool_AnnotEditDataChanged(object sender, SelectedAnnotData e)
        {
            if (e.annotData == null)
            {
                return;
            }
            AnnotHistory annotHistory = ParamConverter.CreateHistory(e.annotData.Annot);
            if (annotHistory == null)
            {
                return;
            }
            AnnotParam previousParam;
            AnnotParam currentParam;
            switch (e.annotData.AnnotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    previousParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                    break;
                default:
                    previousParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                    break;
            }

            switch (e.annotData.AnnotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    if ((e.annotData.Annot as CPDFLineAnnotation).IsMeasured())
                    {
                        List<Point> cPoints = new List<Point>();
                        for (int i = 0; i < e.Points.Count; i++)
                        {
                            Point cPoint = new Point((float)((e.Points[i].X - e.annotData.PaintOffset.X) / e.annotData.CurrentZoom),
                            (float)((e.Points[i].Y - e.annotData.PaintOffset.Y) / e.annotData.CurrentZoom));
                            cPoints.Add(cPoint);
                        }

                        Vector lineVector = cPoints[0] - cPoints[1];
                        Point lineCenterPoint = new Point(
                            (cPoints[0].X + cPoints[2].X) / 2,
                            (cPoints[0].Y + cPoints[2].Y) / 2
                            );
                        Point crossCenterPoint = new Point(
                            (cPoints[4].X + cPoints[5].X) / 2,
                            (cPoints[4].Y + cPoints[5].Y) / 2
                            );

                        Rect unionRect = new Rect(cPoints[0], cPoints[2]);
                        unionRect.Union(cPoints[1]);
                        unionRect.Union(cPoints[3]);
                        CPDFLineAnnotation annotLine = (e.annotData.Annot as CPDFLineAnnotation);

                        annotLine.SetLinePoints(
                            new CPoint(
                                 (float)(cPoints[0].X / 96D * 72D),
                                 (float)(cPoints[0].Y / 96D * 72D)),
                            new CPoint(
                                 (float)(cPoints[2].X / 96D * 72D),
                                 (float)(cPoints[2].Y / 96D * 72D)
                                                ));

                        annotLine.SetRect(new CRect(
                                  (float)(unionRect.Left / 96D * 72D),
                                  (float)(unionRect.Bottom / 96D * 72D),
                                  (float)(unionRect.Right / 96D * 72D),
                                  (float)(unionRect.Top / 96D * 72D)
                                  ));

                        CPDFDistanceMeasure lineMeasure = annotLine.GetDistanceMeasure();
                        double saveLength = lineVector.Length / 96D * 72D;
                        if (lineCenterPoint.Y < crossCenterPoint.Y)
                        {
                            lineMeasure.SetLeadLength(-(float)saveLength);
                        }

                        if (lineCenterPoint.Y > crossCenterPoint.Y)
                        {
                            lineMeasure.SetLeadLength((float)saveLength);
                        }

                        if (lineCenterPoint.Y == crossCenterPoint.Y)
                        {
                            if (lineCenterPoint.X > crossCenterPoint.X)
                            {
                                lineMeasure.SetLeadLength(-(float)saveLength);
                            }

                            if (lineCenterPoint.X < crossCenterPoint.X)
                            {
                                lineMeasure.SetLeadLength((float)saveLength);
                            }
                        }

                        annotLine.UpdateAp();
                        lineMeasure.UpdateAnnotMeasure();
                        PostMeasureInfo(this, annotLine);
                    }
                    else
                    {
                        Point startPoint = new Point(
                            (float)((e.Points.First().X - e.annotData.PaintOffset.X) / e.annotData.CurrentZoom),
                            (float)((e.Points.First().Y - e.annotData.PaintOffset.Y) / e.annotData.CurrentZoom)
                            );

                        Point endPoint = new Point(
                            (float)((e.Points.Last().X - e.annotData.PaintOffset.X) / e.annotData.CurrentZoom),
                            (float)((e.Points.Last().Y - e.annotData.PaintOffset.Y) / e.annotData.CurrentZoom)
                            );
                        CPoint cstartPoint = new CPoint((float)DpiHelper.StandardPointToPDFPoint(startPoint).X, (float)DpiHelper.StandardPointToPDFPoint(startPoint).Y);
                        CPoint cendPoint = new CPoint((float)DpiHelper.StandardPointToPDFPoint(endPoint).X, (float)DpiHelper.StandardPointToPDFPoint(endPoint).Y);
                        DpiHelper.StandardPointToPDFPoint(endPoint);
                        CPDFLineAnnotation annotLine = (e.annotData.Annot as CPDFLineAnnotation);

                        annotLine.SetLinePoints(cstartPoint, cendPoint);
                        annotLine.UpdateAp();
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    {
                        List<CPoint> cPoints = new List<CPoint>();
                        for (int i = 0; i < e.Points.Count; i++)
                        {
                            Point cPoint = new Point((float)((e.Points[i].X - e.annotData.PaintOffset.X) / e.annotData.CurrentZoom),
                            (float)((e.Points[i].Y - e.annotData.PaintOffset.Y) / e.annotData.CurrentZoom));
                            cPoints.Add(DataConversionForWPF.PointConversionForCPoint(DpiHelper.StandardPointToPDFPoint(cPoint)));
                        }

                        CPDFPolygonAnnotation polygonAnnot = (e.annotData.Annot as CPDFPolygonAnnotation);
                        polygonAnnot.SetPoints(cPoints);
                        double left = cPoints.AsEnumerable().Select(x => x.x).Min();
                        double right = cPoints.AsEnumerable().Select(x => x.x).Max();
                        double top = cPoints.AsEnumerable().Select(x => x.y).Min();
                        double bottom = cPoints.AsEnumerable().Select(x => x.y).Max();
                        polygonAnnot.SetRect(new CRect(
                            (float)left,
                            (float)bottom,
                            (float)right,
                            (float)top));

                        polygonAnnot.UpdateAp();
                        if (polygonAnnot.IsMeasured())
                        {
                            polygonAnnot.GetAreaMeasure().UpdateAnnotMeasure();
                            PostMeasureInfo(this, polygonAnnot);
                        }
                    }
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    {
                        List<CPoint> cPoints = new List<CPoint>();
                        for (int i = 0; i < e.Points.Count; i++)
                        {
                            Point cPoint = new Point((float)((e.Points[i].X - e.annotData.PaintOffset.X) / e.annotData.CurrentZoom),
                            (float)((e.Points[i].Y - e.annotData.PaintOffset.Y) / e.annotData.CurrentZoom));
                            cPoints.Add(DataConversionForWPF.PointConversionForCPoint(DpiHelper.StandardPointToPDFPoint(cPoint)));
                        }

                        CPDFPolylineAnnotation polylineAnnot = (e.annotData.Annot as CPDFPolylineAnnotation);
                        polylineAnnot.SetPoints(cPoints);
                        double left = cPoints.AsEnumerable().Select(x => x.x).Min();
                        double right = cPoints.AsEnumerable().Select(x => x.x).Max();
                        double top = cPoints.AsEnumerable().Select(x => x.y).Min();
                        double bottom = cPoints.AsEnumerable().Select(x => x.y).Max();
                        polylineAnnot.SetRect(new CRect(
                            (float)left,
                            (float)bottom,
                            (float)right,
                            (float)top));

                        polylineAnnot.UpdateAp();
                        if (polylineAnnot.IsMeasured())
                        {
                            polylineAnnot.GetPerimeterMeasure().UpdateAnnotMeasure();
                            PostMeasureInfo(this, polylineAnnot);
                        }
                    }
                    break;
                default:
                    break;
            }

            switch (e.annotData.AnnotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    {
                        currentParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                        (e.annotData.Annot as CPDFWidget).UpdateFormAp();
                        break;
                    }

                default:
                    {
                        currentParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), e.annotData.Annot);
                        if (e.annotData.AnnotType != C_ANNOTATION_TYPE.C_ANNOTATION_SOUND)
                        {
                            e.annotData.Annot.UpdateAp();
                        }
                        break;
                    }
            }

            annotHistory.PreviousParam = previousParam;
            annotHistory.PDFDoc = viewerTool.GetCPDFViewer().GetDocument();
            annotHistory.CurrentParam = currentParam;
            annotHistory.Action = HistoryAction.Update;
            viewerTool.GetCPDFViewer().UndoManager.AddHistory(annotHistory);
            viewerTool.GetCPDFViewer().UpdateAnnotFrame();
        }

        private void ViewerTool_MouseLeftButtonUpHandler(object sender, MouseEventObject e)
        {
            if (viewerTool == null)
                return;

            viewerTool.DrawEndSelectedMultiRect();
            viewerTool.DrawEndPageSelectedRect();
            if (currentToolType != ToolType.SelectedPage &&
                viewerTool.IsCanSave() &&
                cPDFAnnotation?.Type != C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT)
            {
                viewerTool.PDFViewer.EnableZoom(true);
                viewerTool.PDFViewer.CanHorizontallyScroll = true;
                viewerTool.PDFViewer.CanVerticallyScroll = true;
            }

            //To be optimized 
            if (currentToolType != ToolType.WidgetEdit && currentToolType != ToolType.ContentEdit && viewerTool.IsText())
            {
                viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.IBeam;
            }
            else if (currentToolType == ToolType.ContentEdit)
            {
                if (createContentEditType == CPDFEditType.EditText)
                    viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.IBeam;
                else
                    viewerTool.PDFViewer.Cursor = viewerTool.Cursor = viewerTool.DrawMoveTest(viewerTool.GetLastSelectedRect());
            }
            else
            {
                viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.Arrow;
            }

            if (currentToolType == ToolType.Customize)
            {
                viewerTool.CleanCustomizeTool();
            }

            if (e.hitTestType == MouseHitTestType.SelectRect)
            {
                List<C_ANNOTATION_TYPE> list = new List<C_ANNOTATION_TYPE>()
                {
                    C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                    C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                    C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON,
                };
                if (list.Contains(e.annotType))
                {
                    viewerTool.DrawEndEditAnnot();
                }
                else
                {
                    viewerTool.DrawEndSelectedRect();
                }
            }

            switch (currentToolType)
            {
                case ToolType.CreateAnnot:
                    CreateAnnotTypeMouseLeftUp(ref e);
                    break;

                case ToolType.WidgetEdit:
                    {
                        if (cPDFAnnotation != null)
                        {
                            Rect rect = viewerTool.EndDrawWidget();
                            CRect cRect = new CRect((float)rect.Left, (float)rect.Bottom, (float)rect.Right, (float)rect.Top);
                            cPDFAnnotation.SetRect(cRect);
                            (cPDFAnnotation as CPDFWidget).UpdateFormAp();
                            CPDFDocument cPDFDocument = viewerTool.GetCPDFViewer().GetDocument();
                            AnnotHistory annotHistory = ParamConverter.CreateHistory(cPDFAnnotation);
                            if (annotHistory == null)
                            {
                                return;
                            }
                            WidgetParm annotParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), cPDFAnnotation) as WidgetParm;
                            annotHistory.CurrentParam = annotParam;
                            annotHistory.PDFDoc = cPDFDocument;
                            viewerTool.GetCPDFViewer().UndoManager.AddHistory(annotHistory);
                            viewerTool.GetCPDFViewer().UpdateAnnotFrame();
                            e.annotType = cPDFAnnotation.Type;
                            e.IsCreate = true;
                            dynamic expandData = new ExpandoObject();
                            expandData.AnnotIndex = annotParam.AnnotIndex;
                            expandData.PageIndex = annotParam.PageIndex;
                            expandData.AnnotParam = annotParam;
                            e.Data = expandData;
                        }
                    }
                    break;

                case ToolType.ContentEdit:
                    {
                        if (viewerTool.GetLastSelectedRect() != null)
                        {
                            //Crop Save Processing
                            if (!viewerTool.GetIsCropMode())
                            {
                                viewerTool.DrawEndTextEdit(viewerTool.GetLastSelectedRect());
                            }
                            else
                            {
                                if (isActiveCropping)
                                {
                                    CropSelectRect();
                                    viewerTool.SetClipThickness();
                                }
                                //Originally saved cropping logic
                            }

                            editSelected = false;
                        }
                        else
                        {
                            if (createContentEditType == CPDFEditType.EditImage && viewerTool.PDFViewer.GetIsShowStampMouse())
                            {
                                Rect stampRect = viewerTool.PDFViewer.GetStampRect();

                                Point mousePoint = new Point(
                                    stampRect.X - stampRect.Width / 2 * viewerTool.GetCPDFViewer().GetZoom(),
                                    stampRect.Y - stampRect.Height / 2 * viewerTool.GetCPDFViewer().GetZoom()
                                    );
                                viewerTool.GetCPDFViewer().GetPointPageInfo(mousePoint, out int pageindex, out Rect paintRect, out Rect pageBound);
                                if (pageindex >= 0)
                                {
                                    Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect((mousePoint.X - pageBound.X) / viewerTool.GetCPDFViewer().GetZoom(), (mousePoint.Y - pageBound.Y) / viewerTool.GetCPDFViewer().GetZoom(), stampRect.Width, stampRect.Height));
                                    CRect SaveRect = new CRect((float)PDFRect.Left, (float)PDFRect.Bottom, (float)PDFRect.Right, (float)PDFRect.Top);

                                    CPDFPage docPage = viewerTool.PDFViewer.GetDocument().PageAtIndex(pageindex);
                                    CPDFEditPage EditPage = docPage.GetEditPage();
                                    CPDFEditImageArea cPDFEditImageArea = EditPage.CreateNewImageArea(SaveRect, createImagePath, string.Empty);
                                    if (cPDFEditImageArea == null)
                                    {
                                        byte[] imageData = null;
                                        int imageWidth = 0;
                                        int imageHeight = 0;
                                        PDFHelp.ImagePathToByte(createImagePath, ref imageData, ref imageWidth, ref imageHeight);
                                        if (imageData != null && imageWidth > 0 && imageHeight > 0)
                                        {
                                            cPDFEditImageArea = EditPage.CreateNewImageArea(SaveRect, imageData, imageWidth, imageHeight);
                                        }
                                    }

                                    viewerTool.PDFViewer.UpdateRenderFrame();
                                    PDFEditHistory editHistory = new PDFEditHistory();
                                    editHistory.EditPage = EditPage;
                                    editHistory.PageIndex = pageindex;
                                    EditPage.EndEdit();
                                    viewerTool.PDFViewer.UndoManager.AddHistory(editHistory);
                                    e.IsCreate = true;
                                }
                            }
                            else if (createContentEditType == CPDFEditType.EditText)
                            {
                                if (viewerTool.CanAddTextEdit)
                                {
                                    e.IsCreate = viewerTool.DrawEndTest(); 
                                }
                            }
                            else
                            {
                                //Draw a box to select multiple boxes
                                Rect rectFrameSelect = viewerTool.DrawEndFrameSelect();
                                viewerTool.FrameSelectAddRect(rectFrameSelect);

                                e.IsCreate = true;
                            }
                        }
                    }
                    break;

                case ToolType.Customize:
                    viewerTool.DrawEndCustomizeTool();
                    break;

                default:
                    break;
            }

            viewerTool.DrawEndSelectText();
            MouseLeftButtonUpHandler?.Invoke(this, e);
            if (viewerTool.IsCanSave())
            {
                cPDFAnnotation = null;
            }
        }

        /// <summary>
        /// Create cropping logic
        /// </summary>
        public void CropSelectRect()
        {
            if (viewerTool.GetLastSelectedRect() != null)
            {
                if (viewerTool.GetIsCropMode())
                {
                    viewerTool.DrawEndTextEdit(viewerTool.GetLastSelectedRect());
                }
            }
            viewerTool.DrawEndSelectText();
            if (viewerTool.IsCanSave())
            {
                cPDFAnnotation = null;
            }
        }

        private void SaveCurrentAnnot()
        {
            viewerTool.SetIsCanSave(true);
            MouseEventObject e = new MouseEventObject
            {
                mouseButtonEventArgs = null,
                hitTestType = MouseHitTestType.Unknown,
                annotType = C_ANNOTATION_TYPE.C_ANNOTATION_NONE,
                IsCreate = false
            };
            switch (currentToolType)
            {
                case ToolType.CreateAnnot:
                    CreateAnnotTypeMouseLeftUp(ref e);
                    break;

                case ToolType.WidgetEdit:
                    {
                        if (cPDFAnnotation != null)
                        {
                            Rect rect = viewerTool.EndDrawWidget();
                            CRect cRect = new CRect((float)rect.Left, (float)rect.Bottom, (float)rect.Right, (float)rect.Top);
                            cPDFAnnotation.SetRect(cRect);
                            (cPDFAnnotation as CPDFWidget).UpdateFormAp();
                            CPDFDocument cPDFDocument = viewerTool.GetCPDFViewer().GetDocument();
                            AnnotHistory annotHistory = ParamConverter.CreateHistory(cPDFAnnotation);
                            if (annotHistory == null)
                            {
                                return;
                            }
                            WidgetParm annotParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), cPDFAnnotation) as WidgetParm;
                            annotHistory.CurrentParam = annotParam;
                            annotHistory.PDFDoc = cPDFDocument;
                            viewerTool.GetCPDFViewer().UndoManager.AddHistory(annotHistory);
                            viewerTool.GetCPDFViewer().UpdateAnnotFrame();
                            e.annotType = cPDFAnnotation.Type;
                            e.IsCreate = true;
                            dynamic expandData = new ExpandoObject();
                            expandData.AnnotIndex = annotParam.AnnotIndex;
                            expandData.PageIndex = annotParam.PageIndex;
                            expandData.AnnotParam = annotParam;
                            e.Data = expandData;
                        }
                    }
                    break;

                case ToolType.Customize:
                    viewerTool.DrawEndCustomizeTool();
                    break;

                default:
                    break;
            }
            if (viewerTool.IsCanSave())
            {
                cPDFAnnotation = null;
            }
        }

        #region MouseLeftButtonUpCreateAnnot 
        private void CreateAnnotTypeMouseLeftUp(ref MouseEventObject e)
        {
            //Mersured
            if (cPDFAnnotation != null)
            {
                switch (cPDFAnnotation.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                        {
                            if ((cPDFAnnotation as CPDFLineAnnotation).IsMeasured())
                            {
                                MeasureSetting measureSetting = viewerTool.GetMeasureSetting();
                                if (viewerTool.GetMoveLength() > measureSetting.MoveDetectionLength)
                                {
                                    viewerTool.SetIsCanSave(true);
                                }
                            }
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                        // if ((cPDFAnnotation as CPDFPolygonAnnotation).IsMeasured())
                        {
                            DefaultSettingParam defSetting = viewerTool.GetDefaultSettingParam();
                            if (defSetting.IsCreateSquarePolygonMeasure)
                            {
                                MeasureSetting measureSetting = viewerTool.GetMeasureSetting();
                                if (viewerTool.GetMoveLength() > measureSetting.MoveDetectionLength)
                                {
                                    viewerTool.SetIsCanSave(true);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            if (viewerTool.IsCanSave())
            {
                SaveCreateAnnotation(ref cPDFAnnotation, ref e);
                viewerTool.PDFViewer.EnableZoom(true);
                viewerTool.PDFViewer.CanHorizontallyScroll = true;
                viewerTool.PDFViewer.CanVerticallyScroll = true;
            }
        }

        private void SaveCreateAnnotation(ref CPDFAnnotation annotation, ref MouseEventObject e)
        {

            if (annotation == null)
            {
                return;
            }

            if (!annotation.IsValid())
            {
                annotation.RemoveAnnot();
                annotation = null;

                return;
            }

            Point StartPoint = viewerTool.GetStartPoint();
            Point EndPoint = viewerTool.GetEndPoint();
            List<Point> points = viewerTool.GetInkDrawPoints();
            List<Point> measurepoints = viewerTool.GetMeasureDrawPoints();
            annotation.SetCreationDate(PDFHelp.GetCurrentPdfTime());
            annotation.SetModifyDate(PDFHelp.GetCurrentPdfTime());

            Rect rect = viewerTool.EndDrawAnnot();
 

            if (annotation != null)
            {
                switch (annotation.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                        SaveInkCreateAnnotation(ref annotation, points);
                        e.IsCreate = true;
                        e.annotType = C_ANNOTATION_TYPE.C_ANNOTATION_INK;
                        e.Data = GetAnnotExpandObject(annotation);
                        if (e.mouseButtonEventArgs != null)
                        {
                            MouseLeftButtonUpHandler?.Invoke(this, e);
                        }
                        annotation = null;
                        return;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                        e.IsCreate = true;
                        e.annotType = C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT;
                        e.Data = GetAnnotExpandObject(annotation);
                        if (e.mouseButtonEventArgs != null)
                        {
                            MouseLeftButtonUpHandler?.Invoke(this, e);
                        }
                        return;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                        {
                            if ((annotation as CPDFLineAnnotation).IsMeasured())
                            {
                                if (measurepoints.Count > 1)
                                {
                                    (annotation as CPDFLineAnnotation).SetLinePoints(new CPoint(
                                        (float)DpiHelper.StandardNumToPDFNum(measurepoints[0].X),
                                        (float)DpiHelper.StandardNumToPDFNum(measurepoints[0].Y)),
                                        new CPoint((float)DpiHelper.StandardNumToPDFNum(measurepoints[1].X),
                                        (float)DpiHelper.StandardNumToPDFNum(measurepoints[1].Y)));
                                    (annotation as CPDFLineAnnotation).GetDistanceMeasure().UpdateAnnotMeasure();
                                    PostMeasureInfo(this, annotation);
                                }
                                else
                                {
                                    annotation.ReleaseAnnot();
                                    if (e.mouseButtonEventArgs != null)
                                    {
                                        MouseLeftButtonUpHandler?.Invoke(this, e);
                                    }
                                    annotation = null;
                                    return;
                                }
                            }
                            else
                            {
                                if (EndPoint != new Point(-1, -1))
                                {
                                    (annotation as CPDFLineAnnotation).SetLinePoints(new CPoint((float)StartPoint.X, (float)StartPoint.Y), new CPoint((float)EndPoint.X, (float)EndPoint.Y));
                                }
                                else
                                {
                                    annotation.ReleaseAnnot();
                                    if (e.mouseButtonEventArgs != null)
                                    {
                                        MouseLeftButtonUpHandler?.Invoke(this, e);
                                    }
                                    annotation = null;
                                    return;
                                }
                            }
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                        {
                            List<CPoint> cPoints = new List<CPoint>();
                            foreach (Point item in measurepoints)
                            {
                                cPoints.Add(DataConversionForWPF.PointConversionForCPoint(DpiHelper.StandardPointToPDFPoint(item)));
                            }
                            (annotation as CPDFPolygonAnnotation).SetPoints(cPoints);
                            if ((annotation as CPDFPolygonAnnotation).IsMeasured())
                            {
                                (annotation as CPDFPolygonAnnotation).GetAreaMeasure().UpdateAnnotMeasure();
                                PostMeasureInfo(this, annotation);
                            }
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                        {
                            if ((annotation as CPDFPolylineAnnotation).IsMeasured())
                            {
                                List<CPoint> cPoints = new List<CPoint>();
                                foreach (Point item in measurepoints)
                                {
                                    cPoints.Add(DataConversionForWPF.PointConversionForCPoint(DpiHelper.StandardPointToPDFPoint(item)));
                                }
                                (annotation as CPDFPolylineAnnotation).SetPoints(cPoints);
                                (annotation as CPDFPolylineAnnotation).GetPerimeterMeasure().UpdateAnnotMeasure();
                                PostMeasureInfo(this, annotation);
                            }
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                        if (rect.Width > 0 && rect.Height > 0)
                        {
                            List<CRect> coreRectList = new List<CRect>
                            {
                                new CRect((float)rect.Left, (float)rect.Bottom, (float)rect.Right, (float)rect.Top)
                            };
                            (annotation as CPDFRedactAnnotation).SetQuardRects(coreRectList);
                        }
                        break;
                    default:
                        break;
                }
                if (rect.Width <= 0 && rect.Height <= 0)
                {
                    switch (createAnnotType)
                    {
                        case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                        case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                        case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                        case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                        case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                        case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                            {
                                annotation.RemoveAnnot();
                                viewerTool.DrawEndSelectText();
                                TextSelectInfo textSelectInfo = viewerTool.GetTextSelectInfo();
                                if (textSelectInfo.PageSelectPointList.Count > 0)
                                {
                                    viewerTool.DrawEndSelectText();
                                    viewerTool.RemoveSelectTextData();
                                    GroupHistory historyData = null;
                                    viewerTool.CreateAnnotForSelectText(textSelectInfo, createAnnotType, out historyData);
                                    List<object> dataList = new List<object>();

                                    if (historyData != null && historyData.Histories.Count > 0)
                                    {
                                        foreach (IHistory historyItem in historyData.Histories)
                                        {
                                            AnnotHistory checkHistory = historyItem as AnnotHistory;
                                            if (checkHistory == null || checkHistory.CurrentParam == null)
                                            {
                                                continue;
                                            }
                                            dynamic expandData = new ExpandoObject();
                                            expandData.AnnotIndex = checkHistory.CurrentParam.AnnotIndex;
                                            expandData.PageIndex = checkHistory.CurrentParam.PageIndex;
                                            expandData.AnnotParam = checkHistory.CurrentParam;
                                            dataList.Add(expandData);
                                        }
                                    }
                                    e.annotType = createAnnotType;
                                    e.IsCreate = true;
                                    if (dataList.Count > 0)
                                    {
                                        e.Data = dataList.Count > 1 ? dataList : dataList[0];
                                    }
                                }

                                if (createAnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_REDACT && textSelectInfo.PageSelectPointList.Count == 0)
                                {
                                    viewerTool.GetCPDFViewer().UpdateAnnotFrame();
                                }
                                else if (createAnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT && textSelectInfo.PageSelectPointList.Count == 0)
                                {
                                    bool isA = annotation.RemoveAnnot();
                                    annotation = null;
                                    return;
                                }
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                            {
                                if (!viewerTool.PDFViewer.GetIsShowStampMouse())
                                {
                                    break;
                                }
                                Rect stampRect = viewerTool.PDFViewer.GetStampRect();

                                Point mousePoint = new Point(
                                    stampRect.X - stampRect.Width / 2 * viewerTool.GetCPDFViewer().GetZoom(),
                                    stampRect.Y - stampRect.Height / 2 * viewerTool.GetCPDFViewer().GetZoom()
                                    );
                                viewerTool.GetCPDFViewer().GetPointPageInfo(mousePoint, out int pageindex, out Rect paintRect, out Rect pageBound);
                                if (pageindex < 0)
                                {
                                    annotation.RemoveAnnot();
                                    annotation = null;
                                    return;
                                }
                                else
                                {
                                    CPDFDocument cPDFDocument = viewerTool.GetCPDFViewer().GetDocument();
                                    CPDFPage cPDFPage = cPDFDocument.PageAtIndex(pageindex);
                                    Point cropPoint = new Point();
                                    if (viewerTool.GetCPDFViewer().GetIsCrop())
                                    {
                                        CRect cropRect = cPDFPage.GetCropBounds();
                                        cropPoint.X = DpiHelper.PDFNumToStandardNum(cropRect.left);
                                        cropPoint.Y = DpiHelper.PDFNumToStandardNum(cropRect.top);
                                    }
                                    Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect(
                                        (mousePoint.X - pageBound.X + (cropPoint.X * viewerTool.GetCPDFViewer().GetZoom())) / viewerTool.GetCPDFViewer().GetZoom(),
                                        (mousePoint.Y - pageBound.Y + (cropPoint.Y * viewerTool.GetCPDFViewer().GetZoom())) / viewerTool.GetCPDFViewer().GetZoom(),
                                        stampRect.Width, stampRect.Height)
                                        );

                                    if (annotation.Page.Rotation % 2 != 0)
                                    {
                                        PDFRect = RotateRect90(PDFRect);
                                    }

                                    CRect cStampRect = new CRect((float)PDFRect.Left, (float)PDFRect.Bottom, (float)PDFRect.Right, (float)PDFRect.Top);
                                    annotation.SetSourceRect(cStampRect);
                                    (annotation as CPDFStampAnnotation).AnnotationRotator.SetRotation(annotation.Page.Rotation * 90);
                                    annotation.UpdateAp();
                                    e.IsCreate = true;
                                    e.annotType = C_ANNOTATION_TYPE.C_ANNOTATION_STAMP;
                                    e.Data = GetAnnotExpandObject(annotation);
                                    StampAnnotHistory stampAnnotHistory = new StampAnnotHistory();
                                    StampParam annotParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), annotation) as StampParam;
                                    if (annotParam.StampType == C_STAMP_TYPE.IMAGE_STAMP)
                                    {
                                        annotParam.CopyImageAnnot = CPDFAnnotation.CopyAnnot(annotation);
                                    }

                                    annotParam.AnnotIndex = annotation.Page.GetAnnotCount() - 1;
                                    stampAnnotHistory.CurrentParam = annotParam;
                                    stampAnnotHistory.PDFDoc = viewerTool.GetCPDFViewer().GetDocument();
                                    viewerTool.GetCPDFViewer().UndoManager.AddHistory(stampAnnotHistory);
                                }

                                viewerTool.GetCPDFViewer().UpdateAnnotFrame();
                            }
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                            {
                                Point point = Mouse.GetPosition(viewerTool.PDFViewer);
                                viewerTool.GetCPDFViewer().GetPointPageInfo(point, out int pageindex, out Rect paintRect, out Rect pageBound);
                                if (pageindex < 0)
                                {
                                    annotation.RemoveAnnot();
                                    annotation = null;
                                    return;
                                }
                                else
                                {
                                    CPDFDocument cPDFDocument = viewerTool.GetCPDFViewer().GetDocument();
                                    CPDFPage cPDFPage = cPDFDocument.PageAtIndex(pageindex);
                                    Point cropPoint = new Point();
                                    if (viewerTool.GetCPDFViewer().GetIsCrop())
                                    {
                                        CRect cropRect = cPDFPage.GetCropBounds();
                                        cropPoint.X = DpiHelper.PDFNumToStandardNum(cropRect.left);
                                        cropPoint.Y = DpiHelper.PDFNumToStandardNum(cropRect.top);
                                    }
                                    CRect x = annotation.GetRect();
                                    Rect PDFRect = DpiHelper.StandardRectToPDFRect(new Rect(
                                        (point.X - pageBound.X + (cropPoint.X * viewerTool.GetCPDFViewer().GetZoom())) / viewerTool.GetCPDFViewer().GetZoom(),
                                        (point.Y - pageBound.Y + (cropPoint.Y * viewerTool.GetCPDFViewer().GetZoom())) / viewerTool.GetCPDFViewer().GetZoom(),
                                       x.width(), x.height())
                                        );
                                    PDFRect.X = PDFRect.X - x.width() / 2;
                                    PDFRect.Y = PDFRect.Y - x.height() / 2;
                                    CRect cStampRect = new CRect((float)PDFRect.Left, (float)PDFRect.Bottom, (float)PDFRect.Right, (float)PDFRect.Top);
                                    annotation.SetRect(cStampRect);

                                    viewerTool.GetCPDFViewer().UpdateAnnotFrame();
                                    e.IsCreate = true;
                                    e.annotType = createAnnotType;
                                    e.Data = GetAnnotExpandObject(annotation);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    //annotation.ReleaseAnnot();
                    if (e.mouseButtonEventArgs != null)
                    {
                        MouseLeftButtonUpHandler?.Invoke(this, e);
                    }
                    annotation = null;
                    return;
                }

                CRect cRect = new CRect(
                    (float)rect.Left,
                    (float)rect.Bottom,
                    (float)rect.Right,
                    (float)rect.Top);

                annotation.SetRect(cRect);
                SaveSharpAnnotBoundText(annotation);

                if (annotation.Type != C_ANNOTATION_TYPE.C_ANNOTATION_TEXT)
                {
                    annotation.UpdateAp();
                }
                else
                {
                    CommonHelper.UpdateStickyAP(annotation as CPDFTextAnnotation);
                }

                AnnotHistory annotHistory = ParamConverter.CreateHistory(annotation);
                if (annotHistory == null)
                {
                    return;
                }

                AnnotParam currentParam;
                switch (annotation.Type)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                        currentParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), annotation);
                        break;
                    default:
                        currentParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), annotation);
                        break;
                }
                annotHistory.CurrentParam = currentParam;
                annotHistory.Action = HistoryAction.Add;
                annotHistory.PDFDoc = viewerTool.PDFViewer.GetDocument();

                viewerTool.ClearDrawAnnot();
                viewerTool.GetCPDFViewer().UpdateAnnotFrame();
                viewerTool.GetCPDFViewer().UndoManager.AddHistory(annotHistory);

                if (annotation.Type == C_ANNOTATION_TYPE.C_ANNOTATION_TEXT && SaveEmptyStickyAnnot == false)
                {
                    BaseLayer baseLayer1 = viewerTool.GetCPDFViewer().GetViewForTag(viewerTool.GetCPDFViewer().GetAnnotViewTag());
                    int checkPageIndex = currentParam.PageIndex;
                    int checkAnnotIndex = currentParam.AnnotIndex;
                    BaseAnnot selectAnnot = (baseLayer1 as AnnotLayer).GetSelectedAnnot(ref checkPageIndex, ref checkAnnotIndex);
                    if (selectAnnot != null)
                    {
                        StickyNoteAnnot stickyAnnot = selectAnnot as StickyNoteAnnot;
                        StickyNoteAnnot.StickyPopupClosed -= StickyAnnot_StickyPopupClosed;
                        StickyNoteAnnot.StickyPopupClosed += StickyAnnot_StickyPopupClosed;
                        stickyAnnot.PopStickyNote();
                    }
                }

                {
                    e.annotType = annotation.Type;
                    e.IsCreate = true;
                    dynamic expandData = new ExpandoObject();
                    expandData.AnnotIndex = currentParam.AnnotIndex;
                    expandData.PageIndex = currentParam.PageIndex;
                    expandData.AnnotParam = currentParam;
                    e.Data = expandData;
                }

            }
        }

        private void SaveSharpAnnotBoundText(CPDFAnnotation boundAnnot)
        {
            if (boundAnnot == null || boundAnnot.Page == null || boundAnnot.Page.IsValid() == false)
            {
                return;
            }

            if (boundAnnot.Type != C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE && boundAnnot.Type != C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE)
            {
                return;
            }

            CPDFTextPage textPage = boundAnnot.Page.GetTextPage();
            if (textPage == null || textPage.IsValid() == false)
            {
                return;
            }

            string boundText = textPage.GetBoundedText(boundAnnot.GetRect());
            if (string.IsNullOrEmpty(boundText) == false)
            {
                boundAnnot.SetContent(boundText);
            }
        }

        private void StickyAnnot_StickyPopupClosed(object sender, EventArgs e)
        {
            StickyNoteAnnot.StickyPopupClosed -= StickyAnnot_StickyPopupClosed;
            StickyNoteAnnot stickyAnnot = sender as StickyNoteAnnot;
            if (stickyAnnot == null)
            {
                return;
            }
            AnnotData annotData = stickyAnnot.GetAnnotData();
            AnnotParam currentParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), annotData.Annot);
            AnnotHistory annotHistory = ParamConverter.CreateHistory(annotData.Annot);
            string content = annotData.Annot.GetContent();
            if (string.IsNullOrEmpty(content))
            {
                if (annotData.Annot.RemoveAnnot())
                {
                    viewerTool.ClearDrawAnnot();
                    viewerTool.GetCPDFViewer().UpdateAnnotFrame();
                    viewerTool.SelectedAnnotForIndex(-1, -1);
                    annotHistory.CurrentParam = currentParam;
                    annotHistory.Action = HistoryAction.Remove;
                    annotHistory.PDFDoc = viewerTool.PDFViewer.GetDocument();
                    viewerTool.GetCPDFViewer().UndoManager.AddHistory(annotHistory);
                }

                return;
            }
            AnnotParam previousParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), annotData.Annot);
            previousParam.Content = string.Empty;
            annotHistory.PreviousParam = previousParam;
            annotHistory.CurrentParam = currentParam;
            annotHistory.Action = HistoryAction.Update;
            annotHistory.PDFDoc = viewerTool.PDFViewer.GetDocument();
            viewerTool.GetCPDFViewer().UndoManager.AddHistory(annotHistory);
        }

        internal void PostMeasureInfo(object sender, CPDFAnnotation rawAnnot)
        {
            if (rawAnnot == null)
            {
                return;
            }
            try
            {
                if (rawAnnot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                {
                    CPDFLineAnnotation lineAnnot = rawAnnot as CPDFLineAnnotation;
                    if (lineAnnot.IsMeasured() && lineAnnot.Points != null && lineAnnot.Points.Count() == 2)
                    {
                        CPDFDistanceMeasure lineMeasure = lineAnnot.GetDistanceMeasure();
                        CPDFMeasureInfo measureInfo = lineMeasure.MeasureInfo;
                        Vector standVector = new Vector(1, 0);

                        Point startPoint = new Point(lineAnnot.Points[0].x, lineAnnot.Points[0].y);
                        Point endPoint = new Point(lineAnnot.Points[1].x, lineAnnot.Points[1].y);
                        Vector movevector = endPoint - startPoint;
                        double showLenght = lineMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_LENGTH);

                        MeasureEventArgs measureEvent = new MeasureEventArgs();
                        measureEvent.Angle = (int)Math.Abs(Vector.AngleBetween(movevector, standVector));
                        measureEvent.RulerTranslateUnit = measureInfo.RulerTranslateUnit;
                        measureEvent.RulerTranslate = measureInfo.RulerTranslate;
                        measureEvent.RulerBase = measureInfo.RulerBase;
                        measureEvent.RulerBaseUnit = measureInfo.RulerBaseUnit;
                        measureEvent.MousePos = new Point(
                            (int)Math.Abs(movevector.X),
                            (int)Math.Abs(movevector.Y));
                        measureEvent.Type = CPDFMeasureType.CPDF_DISTANCE_MEASURE;
                        NumberFormatInfo formatInfo = new NumberFormatInfo();
                        formatInfo.NumberDecimalDigits = Math.Abs(measureInfo.Precision).ToString().Length - 1;
                        measureEvent.Distance = showLenght.ToString("N", formatInfo) + " " + measureInfo.RulerTranslateUnit;
                        measureEvent.Precision = GetMeasureShowPrecision(measureInfo.Precision);

                        viewerTool?.InvokeMeasureChangeEvent(sender, measureEvent);
                    }
                }

                if (rawAnnot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE)
                {
                    CPDFPolylineAnnotation polylineAnnot = rawAnnot as CPDFPolylineAnnotation;
                    if (polylineAnnot.IsMeasured() && polylineAnnot.Points != null && polylineAnnot.Points.Count() >= 2)
                    {
                        double totalInch = 0;
                        for (int i = 0; i < polylineAnnot.Points.Count - 1; i++)
                        {
                            Point endLinePoint = new Point(
                                polylineAnnot.Points[i + 1].x,
                                polylineAnnot.Points[i + 1].y
                                );
                            Point startLinePoint = new Point(
                                polylineAnnot.Points[i].x,
                                polylineAnnot.Points[i].y
                                );
                            Vector subVector = endLinePoint - startLinePoint;
                            totalInch += subVector.Length;
                        }
                        totalInch = totalInch / 72D;
                        CPDFPerimeterMeasure lineMeasure = polylineAnnot.GetPerimeterMeasure();
                        CPDFMeasureInfo measureInfo = lineMeasure.MeasureInfo;
                        double showLenght = lineMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_LENGTH);

                        MeasureEventArgs measureEvent = new MeasureEventArgs();
                        measureEvent.Angle = 0;

                        measureEvent.RulerTranslateUnit = measureInfo.RulerTranslateUnit;
                        measureEvent.RulerTranslate = measureInfo.RulerTranslate;
                        measureEvent.RulerBase = measureInfo.RulerBase;
                        measureEvent.RulerBaseUnit = measureInfo.RulerBaseUnit;
                        measureEvent.Precision = GetMeasureShowPrecision(measureInfo.Precision);
                        measureEvent.Type = CPDFMeasureType.CPDF_PERIMETER_MEASURE;
                        NumberFormatInfo formatInfo = new NumberFormatInfo();
                        formatInfo.NumberDecimalDigits = Math.Abs(measureInfo.Precision).ToString().Length - 1;
                        measureEvent.Distance = showLenght.ToString("N", formatInfo) + " " + measureInfo.RulerTranslateUnit;
                        viewerTool?.InvokeMeasureChangeEvent(sender, measureEvent);
                    }
                }

                if (rawAnnot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON)
                {
                    CPDFPolygonAnnotation polygonAnnot = rawAnnot as CPDFPolygonAnnotation;
                    if (polygonAnnot.IsMeasured() && polygonAnnot.Points != null && polygonAnnot.Points.Count() >= 2)
                    {
                        double totalInch = 0;
                        for (int i = 0; i < polygonAnnot.Points.Count - 1; i++)
                        {
                            Point endLinePoint = new Point(
                                polygonAnnot.Points[i + 1].x,
                                polygonAnnot.Points[i + 1].y
                                );
                            Point startLinePoint = new Point(
                                polygonAnnot.Points[i].x,
                                polygonAnnot.Points[i].y
                                );
                            Vector subVector = endLinePoint - startLinePoint;
                            totalInch += subVector.Length;
                        }
                        totalInch = totalInch / 72D;
                        CPDFAreaMeasure areaMeasure = polygonAnnot.GetAreaMeasure();
                        CPDFMeasureInfo measureInfo = areaMeasure.MeasureInfo;
                        double showLenght = areaMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_LENGTH);

                        MeasureEventArgs measureEvent = new MeasureEventArgs();
                        measureEvent.Angle = 0;

                        measureEvent.RulerTranslateUnit = measureInfo.RulerTranslateUnit;
                        measureEvent.RulerTranslate = measureInfo.RulerTranslate;
                        measureEvent.RulerBase = measureInfo.RulerBase;
                        measureEvent.RulerBaseUnit = measureInfo.RulerBaseUnit;
                        measureEvent.Precision = GetMeasureShowPrecision(measureInfo.Precision);
                        measureEvent.Type = CPDFMeasureType.CPDF_AREA_MEASURE;
                        NumberFormatInfo formatInfo = new NumberFormatInfo();
                        formatInfo.NumberDecimalDigits = Math.Abs(measureInfo.Precision).ToString().Length - 1;
                        measureEvent.Distance = showLenght.ToString("N", formatInfo) + " " + measureInfo.RulerTranslateUnit;
                        double area = areaMeasure.GetMeasurementResults(CPDFCaptionType.CPDF_CAPTION_AREA);
                        measureEvent.Area = string.Format("{0} sq {1}", GetPrecisionData(area, measureEvent.Precision), measureEvent.RulerTranslateUnit);

                        viewerTool?.InvokeMeasureChangeEvent(sender, measureEvent);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public string GetPrecisionData(double number, double precision)
        {
            NumberFormatInfo formatInfo = new NumberFormatInfo();
            formatInfo.NumberDecimalDigits = 2;
            if (precision == 1)
            {
                formatInfo.NumberDecimalDigits = 0;
            }
            if (precision == 0.1)
            {
                formatInfo.NumberDecimalDigits = 1;
            }
            if (precision == 0.01)
            {
                formatInfo.NumberDecimalDigits = 2;
            }
            if (precision == 0.001)
            {
                formatInfo.NumberDecimalDigits = 3;
            }
            if (precision == 0.0001)
            {
                formatInfo.NumberDecimalDigits = 4;
            }
            if (precision == 0.00001)
            {
                formatInfo.NumberDecimalDigits = 5;
            }

            return number.ToString("N", formatInfo);
        }

        internal double GetMeasureShowPrecision(int precision)
        {
            if (precision == CPDFMeasure.PRECISION_VALUE_ZERO)
            {
                return 1;
            }
            if (CPDFMeasure.PRECISION_VALUE_ONE == precision)
            {
                return 0.1;
            }
            if (CPDFMeasure.PRECISION_VALUE_TWO == precision)
            {
                return 0.01;
            }
            if (CPDFMeasure.PRECISION_VALUE_THREE == precision)
            {
                return 0.001;
            }
            if (CPDFMeasure.PRECISION_VALUE_FOUR == precision)
            {
                return 0.0001;
            }
            return 0;
        }

        private object GetAnnotExpandObject(CPDFAnnotation annot)
        {
            if (annot != null && annot.IsValid())
            {
                try
                {
                    AnnotParam annotParam = null;
                    if (annot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                    {
                        annotParam = ParamConverter.WidgetConverter(viewerTool.GetCPDFViewer().GetDocument(), annot);
                    }
                    else
                    {
                        annotParam = ParamConverter.AnnotConverter(viewerTool.GetCPDFViewer().GetDocument(), annot);
                    }
                    if (annotParam != null)
                    {
                        annotParam.AnnotIndex = annot.Page.GetAnnotCount() - 1;
                        dynamic expandData = new ExpandoObject();
                        expandData.AnnotIndex = annotParam.AnnotIndex;
                        expandData.PageIndex = annot.Page.PageIndex;
                        expandData.AnnotParam = annotParam;
                        return expandData;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return null;
        }

        private void SaveInkCreateAnnotation(ref CPDFAnnotation annotation, List<Point> points)
        {
            // Creation of non-custom stamps
            if (!viewerTool.PDFViewer.GetIsShowStampMouse())
            {
                if (points.Count < 5)
                {
                    annotation.RemoveAnnot();
                    annotation = null;
                    return;
                }

                List<List<CPoint>> inkPathList = new List<List<CPoint>>();
                List<CPoint> inkPath = new List<CPoint>();
                foreach (Point inkNode in points)
                {
                    inkPath.Add(new CPoint((float)DpiHelper.StandardNumToPDFNum(inkNode.X), (float)DpiHelper.StandardNumToPDFNum(inkNode.Y)));
                }
                inkPathList.Add(inkPath);
                (annotation as CPDFInkAnnotation).SetInkPath(inkPathList);
                (annotation as CPDFInkAnnotation).UpdateAp();
            }
            else
            {
                Rect stampRect = viewerTool.PDFViewer.GetStampRect();

                Point mousePoint = new Point(
                    stampRect.X - stampRect.Width / 2 * viewerTool.GetCPDFViewer().GetZoom(),
                    stampRect.Y - stampRect.Height / 2 * viewerTool.GetCPDFViewer().GetZoom()
                    );
                viewerTool.GetCPDFViewer().GetPointPageInfo(mousePoint, out int pageindex, out Rect paintRect, out Rect pageBound);
                if (pageindex < 0)
                {
                    annotation.RemoveAnnot();
                    annotation = null;
                    return;
                }
                else
                {
                    CPDFPage cPDFPage = viewerTool.GetCPDFViewer().GetDocument().PageAtIndex(pageindex);
                    Point cropPoint = new Point();
                    if (viewerTool.GetCPDFViewer().GetIsCrop())
                    {
                        CRect cropRect = cPDFPage.GetCropBounds();
                        cropPoint.X = DpiHelper.PDFNumToStandardNum(cropRect.left);
                        cropPoint.Y = DpiHelper.PDFNumToStandardNum(cropRect.top);
                    }
                    // Move the point
                    CPoint cPoint = new CPoint(
                        (float)((mousePoint.X - pageBound.X + (cropPoint.X * viewerTool.GetCPDFViewer().GetZoom())) / viewerTool.GetCPDFViewer().GetZoom()),
                        (float)((mousePoint.Y - pageBound.Y + (cropPoint.Y * viewerTool.GetCPDFViewer().GetZoom())) / viewerTool.GetCPDFViewer().GetZoom()));
                    List<List<CPoint>> cPoints = (annotation as CPDFInkAnnotation).InkPath;
                    List<List<CPoint>> savePointList = new List<List<CPoint>>();
                    foreach (List<CPoint> inkNode in cPoints)
                    {
                        List<CPoint> savePoints = new List<CPoint>();
                        foreach (CPoint addPoint in inkNode)
                        {
                            savePoints.Add(
                                new CPoint(
                                (addPoint.x + (float)DpiHelper.StandardNumToPDFNum(cPoint.x)),
                             (addPoint.y + (float)DpiHelper.StandardNumToPDFNum(cPoint.y))
                              ));
                        }
                        if (savePoints.Count > 0)
                        {
                            savePointList.Add(savePoints);
                        }
                    }

                    (annotation as CPDFInkAnnotation).SetInkPath(savePointList);
                    (annotation as CPDFInkAnnotation).UpdateAp();
                }
            }
            viewerTool.ClearDrawAnnot();
            viewerTool.GetCPDFViewer().UpdateAnnotFrame();
            InkAnnotHistory inkAnnotHistory = new InkAnnotHistory();
            AnnotParam annotParam = ParamConverter.AnnotConverter(viewerTool.PDFViewer.GetDocument(), cPDFAnnotation);
            annotParam.AnnotIndex = cPDFAnnotation.Page.GetAnnotCount() - 1;
            inkAnnotHistory.CurrentParam = (InkParam)annotParam;
            inkAnnotHistory.PDFDoc = viewerTool.PDFViewer.GetDocument();
            viewerTool.GetCPDFViewer().UndoManager.AddHistory(inkAnnotHistory);
            return;
        }

        #endregion

        /// <summary>
        /// Set mouse pattern when creating content editing
        /// </summary>
        /// <param name="AddTextEditCursor">add text</param>
        /// <param name="AddImageEditCursor">add image</param>
        public void SetAddContentEditCursor(Cursor AddTextEditCursor, Cursor AddImageEditCursor)
        {
            if (AddTextEditCursor == null)
            {
                AddTextEditCursor = Cursors.IBeam;
            }
            if (AddImageEditCursor == null)
            {
                AddImageEditCursor = Cursors.Arrow;
            }
            this.addTextEditCursor = AddTextEditCursor;
            this.addImageEditCursor = AddImageEditCursor;

        }

        private void ViewerTool_MouseMoveHandler(object sender, MouseEventObject e)
        {
            if (viewerTool == null)
                return;

            viewerTool.DrawMoveSelectedMultiRect();
            viewerTool.DrawMovePageSelectedRect();
            if (currentToolType != ToolType.ContentEdit)
            {
                if (e.hitTestType == MouseHitTestType.SelectRect)
                {
                    List<C_ANNOTATION_TYPE> list = new List<C_ANNOTATION_TYPE>()
                    {
                        C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                        C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                        C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON,
                    };

                    Cursor oldCursor = viewerTool.Cursor;
                    Cursor newCursor = viewerTool.GetMoveSelectedRectCursor();
                    if (oldCursor != newCursor)
                    {
                        viewerTool.PDFViewer.Cursor = viewerTool.Cursor = newCursor;
                    }
                    //viewerTool.PDFViewer.Cursor = viewerTool.Cursor= viewerTool.GetMoveSelectedRectCursor();
                    if (list.Contains(e.annotType))
                    {
                        viewerTool.DrawMoveEditAnnot();
                    }
                    else
                    {
                        if (e.annotType != C_ANNOTATION_TYPE.C_ANNOTATION_LINK)
                        {
                            bool tag = viewerTool.DrawMoveSelectedRect();
                            if (currentToolType == ToolType.WidgetEdit)
                            {
                                BaseWidget hitWidget = viewerTool?.GetCPDFViewer()?.AnnotHitTest() as BaseWidget;
                                if (hitWidget == null)
                                {
                                    viewerTool.MoveDrawWidget(tag);
                                }
                                else
                                {
                                    viewerTool.MoveDrawWidget(true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (currentToolType == ToolType.CreateAnnot)
                    {
                        if (!viewerTool.PDFViewer.GetIsShowStampMouse())
                        {
                            // Annotation drawing only occurs if the mouse is not set to stamp/data application mode.// Annotation drawing only occurs if the mouse is not set to stamp/data application mode.
                            viewerTool.MoveDrawAnnot();
                        }
                        switch (createAnnotType)
                        {
                            case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                            case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                            case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                            case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                            case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                                viewerTool.DrawMoveSelectText(false);
                                break;
                            default:
                                break;
                        }
                    }
                    else if (currentToolType == ToolType.WidgetEdit)
                    {
                        BaseWidget hitWidget = viewerTool?.GetCPDFViewer()?.AnnotHitTest() as BaseWidget;
                        if (hitWidget == null)
                        {
                            viewerTool.MoveDrawWidget(false);
                        }
                        else
                        {
                            viewerTool.MoveDrawWidget(true);
                        }
                    }
                    else if (e.hitTestType == MouseHitTestType.Unknown && currentToolType != ToolType.WidgetEdit)
                    {
                        if (currentToolType == ToolType.Customize)
                        {
                            viewerTool.DrawMoveCustomizeTool();
                        }
                        else
                        {
                            TextSelectInfo textInfo = viewerTool.GetTextSelectInfo();
                            if ((textInfo.StartPage != -1 && e.mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed) || viewerTool.IsText())
                            {
                                viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.IBeam;
                            }
                            else
                            {
                                viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.Arrow;
                            }
                        }
                    }
                }

                if (e.annotType == C_ANNOTATION_TYPE.C_ANNOTATION_NONE)
                {
                    viewerTool.DrawMoveSelectText(false);
                }
            }
            else
            {
                if (createContentEditType != CPDFEditType.EditImage)
                {
                    Cursor cursor = Cursors.Arrow;
                    MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(viewerTool.PDFViewer.GetViewForTag(viewerTool.MultiSelectedRectViewTag));

                    if (viewerTool.GetLastSelectedRect() != null)
                    {
                        if (editSelected)
                        {
                            viewerTool.DrawMoveTextEdit(viewerTool.GetLastSelectedRect(), clickEditSelected);
                            if (clickEditSelected)
                                cursor = Cursors.IBeam;
                            else
                                cursor = viewerTool.DrawMoveTest(viewerTool.GetLastSelectedRect());
                        }
                        else
                        {
                            cursor = viewerTool.DrawMoveTest(viewerTool.GetLastSelectedRect());
                        }

                    }
                    else
                    {

                        cursor = viewerTool.DrawMoveTest(viewerTool.GetLastSelectedRect());

                        if (multiSelectedRect == null || multiSelectedRect.Children.Count == 0)
                        {
                            //Selection of mobile drawing logic
                            if (e.mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed && createContentEditType == CPDFEditType.None)
                            {
                                viewerTool.DrawMoveFrameSelect();
                            }
                        }
                    }

                    if (cursor == Cursors.Arrow && createContentEditType == CPDFEditType.EditText)
                    {
                        cursor = addTextEditCursor;
                    }
                    viewerTool.Cursor = cursor;
                    viewerTool.PDFViewer.Cursor = cursor;

                }
                else
                {
                    Cursor cursor = Cursors.Arrow;
                    MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(viewerTool.PDFViewer.GetViewForTag(viewerTool.MultiSelectedRectViewTag));
                    if (viewerTool.GetLastSelectedRect() != null)
                    {
                        if (editSelected)
                        {
                            viewerTool.DrawMoveTextEdit(viewerTool.GetLastSelectedRect(), clickEditSelected);
                            if (clickEditSelected)
                                cursor = Cursors.IBeam;
                            else
                                cursor = viewerTool.DrawMoveTest(viewerTool.GetLastSelectedRect());
                        }
                        else
                        {
                            cursor = viewerTool.DrawMoveTest(viewerTool.GetLastSelectedRect());
                        }
                    }
                    else
                    {
                        cursor = viewerTool.DrawMoveTest(viewerTool.GetLastSelectedRect());
                    }

                    if (cursor == Cursors.Arrow && createContentEditType == CPDFEditType.EditText)
                    {
                        cursor = Cursors.IBeam;
                    }
                    if (cursor == Cursors.Arrow && createContentEditType == CPDFEditType.EditImage)
                    {
                        cursor = addImageEditCursor;
                    }
                    viewerTool.Cursor = cursor;
                    viewerTool.PDFViewer.Cursor = cursor;
                    //viewerTool.Cursor = Cursors.None;
                    //viewerTool.PDFViewer.Cursor = Cursors.None;
                }
            }

            MouseMoveHandler?.Invoke(this, e);
        }

        private void ViewerTool_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            viewerTool.RemoveSelectTextData();
            viewerTool.ReDrawSelectText();
            viewerTool.HideWidgetHitPop();
            switch (e.hitTestType)
            {
                case MouseHitTestType.Unknown:
                    {
                        if (currentToolType == ToolType.CreateAnnot)
                        {
                            switch ((e.mouseButtonEventArgs as MouseButtonEventArgs).ClickCount)
                            {
                                case 1:
                                    if (viewerTool.IsCanSave())
                                    {
                                        cPDFAnnotation = viewerTool.StartDrawAnnot(createAnnotType);
                                        viewerTool.CreateDefaultAnnot(cPDFAnnotation, createAnnotType, null);
                                        if (cPDFAnnotation != null && createAnnotType == C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT)
                                        {
                                            viewerTool.CreateTextBox();
                                        }
                                        switch (createAnnotType)
                                        {
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                                                {
                                                    viewerTool.DrawStartSelectText();
                                                    viewerTool.EndDrawAnnot();
                                                }
                                                break;
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                                                if (viewerTool.IsText())
                                                {
                                                    viewerTool.DrawStartSelectText();
                                                    viewerTool.EndDrawAnnot();
                                                }
                                                break;

                                            case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                                                {
                                                    bool cansave = true;
                                                    CPDFLineAnnotation LineAnnotation = (cPDFAnnotation as CPDFLineAnnotation);
                                                    if (LineAnnotation != null)
                                                    {
                                                        if (LineAnnotation.IsMeasured())
                                                        {
                                                            cansave = false;
                                                        }
                                                    }
                                                    viewerTool.SetIsCanSave(cansave);
                                                }
                                                break;
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                                                {
                                                    bool cansave = true;
                                                    CPDFPolygonAnnotation PolyAnnotation = (cPDFAnnotation as CPDFPolygonAnnotation);
                                                    if (PolyAnnotation != null)
                                                    {
                                                        cansave = false;
                                                    }
                                                    viewerTool.SetIsCanSave(cansave);
                                                }
                                                break;
                                            case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                                                {
                                                    bool cansave = true;
                                                    CPDFPolylineAnnotation PolyAnnotation = (cPDFAnnotation as CPDFPolylineAnnotation);
                                                    PolyAnnotation?.IsMeasured();
                                                    if (PolyAnnotation != null)
                                                    {
                                                        if (PolyAnnotation.IsMeasured())
                                                        {
                                                            cansave = false;
                                                        }
                                                    }
                                                    viewerTool.SetIsCanSave(cansave);
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        viewerTool.MultipleClick();
                                        e.IsDrawing = true;
                                    }
                                    break;
                                case 2:
                                    if (currentToolType != ToolType.Customize)
                                    {
                                        if (!viewerTool.IsCanSave())
                                        {
                                            BaseLayer baseLayer = viewerTool.PDFViewer.GetViewForTag(viewerTool.CreateAnnotTag);
                                            bool canSave = (baseLayer as CreateAnnotTool).IsCanSave();
                                            viewerTool.SetIsCanSave(canSave);
                                        }
                                    }
                                    break;
                            }
                        }
                        else if (currentToolType == ToolType.WidgetEdit)
                        {
                            cPDFAnnotation = viewerTool.StartDrawWidget(createWidgetType);
                            viewerTool.CreateDefaultWidget(cPDFAnnotation, createWidgetType, null);
                            viewerTool?.InvokeWidgetCreated(cPDFAnnotation);
                        }
                        else if (currentToolType == ToolType.Pan || currentToolType == ToolType.Viewer)
                        {
                            if (viewerTool.IsText())
                            {
                                viewerTool.DrawStartSelectText();
                                e.hitTestType = MouseHitTestType.Text;
                            }
                        }
                        else if (currentToolType == ToolType.Customize)
                        {
                            viewerTool.DrawStartCustomizeTool(CustomizeToolType.kErase);
                        }

                        viewerTool.CleanSelectedRect();
                        viewerTool.CleanEditAnnot();
                    }
                    break;
                case MouseHitTestType.Widget:
                    {
                        viewerTool.CleanSelectedRect();
                        viewerTool.CleanEditAnnot();

                        BaseWidget hitWidget = viewerTool?.GetCPDFViewer()?.AnnotHitTest() as BaseWidget;
                        if (hitWidget != null)
                        {
                            viewerTool.ShowFormHitPop(hitWidget);
                        }
                    }
                    break;
                case MouseHitTestType.MultiTextEdit:
                    viewerTool.DrawStartSelectedMultiRect();
                    break;
                case MouseHitTestType.SelectedPageRect:
                    viewerTool.DrawStartPageSelectedRect();
                    break;
                default:
                    {
                        List<C_ANNOTATION_TYPE> list = new List<C_ANNOTATION_TYPE>()
                        {
                            C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON,
                            C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                        };
                        if (list.Contains(e.annotType))
                        {
                            viewerTool.CleanSelectedRect();
                            if (!e.IsMersured || !list.Contains(createAnnotType))
                            {
                                viewerTool.StartDrawEditAnnot();
                            }
                        }
                        else
                        {
                            if (currentToolType != ToolType.Viewer)
                            {
                                viewerTool.CleanEditAnnot();
                                viewerTool.DrawStartSelectedRect();
                                if (currentToolType == ToolType.WidgetEdit)
                                {
                                    viewerTool.MoveDrawWidget(true);
                                }
                            }
                        }
                    }
                    break;
            }
            switch (currentToolType)
            {
                case ToolType.Pan:
                    {
                        if (e.annotType == C_ANNOTATION_TYPE.C_ANNOTATION_NONE)
                        {
                            switch ((e.mouseButtonEventArgs as MouseButtonEventArgs).ClickCount)
                            {
                                case 2:
                                    viewerTool.DrawMoveSelectText(true);
                                    break;
                            }
                        }
                    }
                    break;
                case ToolType.ContentEdit:
                    {
                        editSelected = true;
                        clickEditSelected = true;
                        switch ((e.mouseButtonEventArgs as MouseButtonEventArgs).ClickCount)
                        {
                            case 1:
                                if (viewerTool.GetIsCropMode())
                                {
                                    //Preconditions for determining crop acquisition points
                                    viewerTool.HandleTextSelectClick(viewerTool.GetLastSelectedRect(), true);
                                    clickEditSelected = false;
                                    MouseLeftButtonDownHandler?.Invoke(this, e);
                                }
                                break;
                            case 2:
                                viewerTool.HandleTextSelectClick(viewerTool.GetLastSelectedRect(), true);
                                clickEditSelected = false;
                                MouseLeftButtonDownHandler?.Invoke(this, e);
                                return;
                            case 3:
                                viewerTool.HandleTextSelectClick(viewerTool.GetLastSelectedRect(), false);
                                clickEditSelected = false;
                                MouseLeftButtonDownHandler?.Invoke(this, e);
                                return;
                        }
                        if (createContentEditType != CPDFEditType.EditImage)
                        {
                            viewerTool.DrawTextEditDownEvent(true);
                        }
                        viewerTool.HideDrawSelectedMultiRect();
                        if (viewerTool.GetLastSelectedRect() != null)
                        {
                            viewerTool.DrawEndFrameSelect();
                            Point point = Mouse.GetPosition(viewerTool);
                            PointControlType pointControlType = PointControlType.None;
                            if (viewerTool.GetIsCropMode())
                            {
                                //Crop acquisition point judgment
                                pointControlType = viewerTool.GetLastSelectedRect().GetHitCropControlIndex(point);
                            }
                            else
                            {
                                pointControlType = viewerTool.GetLastSelectedRect().GetHitControlIndex(point);
                            }

                            EditAreaObject editAreaObject = viewerTool.GetEditAreaObjectForRect(viewerTool.GetLastSelectedRect());
                            if (pointControlType != PointControlType.None &&
                                (editAreaObject.cPDFEditArea.Type != CPDFEditType.EditText || pointControlType != PointControlType.Body))
                            {
                                switch (pointControlType)
                                {
                                    case PointControlType.LeftTop:
                                    case PointControlType.RightBottom:
                                        viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.SizeNWSE;
                                        break;
                                    case PointControlType.LeftMiddle:
                                    case PointControlType.RightMiddle:
                                        viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.SizeWE;
                                        break;
                                    case PointControlType.LeftBottom:
                                    case PointControlType.RightTop:
                                        viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.SizeNESW;
                                        break;
                                    case PointControlType.MiddleBottom:
                                    case PointControlType.MiddleTop:
                                        viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.SizeNS;
                                        break;
                                    case PointControlType.Line:
                                    case PointControlType.Body:
                                        viewerTool.PDFViewer.Cursor = viewerTool.Cursor = Cursors.SizeAll;
                                        break;
                                    default:
                                        break;
                                }
                                if (e.mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
                                {
                                    viewerTool.DrawStartTextEdit(viewerTool.GetLastSelectedRect(), editAreaObject);
                                }

                                clickEditSelected = false;
                            }
                        }
                        else
                        {
                            //If it is multiple selection, do not create a new input box
                            MultiSelectedRect multiSelectedRect = CommonHelper.FindVisualChild<MultiSelectedRect>(viewerTool.PDFViewer.GetViewForTag(viewerTool.MultiSelectedRectViewTag));
                            if (multiSelectedRect != null && multiSelectedRect.Children.Count > 0)
                            {
                                return;
                            }
                            Point point = Mouse.GetPosition(viewerTool);
                            viewerTool.GetCPDFViewer().GetPointPageInfo(point, out int index, out Rect paintRect, out Rect pageBound);
                            if (index < 0)
                            {
                                MouseLeftButtonDownHandler?.Invoke(this, e);
                                return;
                            }
                            if (createContentEditType == CPDFEditType.EditText)
                            {
                                if (viewerTool.CanAddTextEdit)
                                {
                                    viewerTool.DrawTest(pageBound, index);
                                }
                            }
                            if (createContentEditType == CPDFEditType.None)
                            {
                                viewerTool.DrawFrameSelect();
                            }
                            else
                            {
                                viewerTool.DrawEndFrameSelect();
                            }
                            clickEditSelected = false;
                        }
                    }
                    break;
                default:
                    break;
            }

            MouseLeftButtonDownHandler?.Invoke(this, e);
        }

        private void ViewerTool_MouseRightButtonDownHandler(object sender, MouseEventObject e)
        {
            if (e.mouseButtonEventArgs != null)
            {
                viewerTool?.SetPastePoint(e.mouseButtonEventArgs.GetPosition(viewerTool));
            }

            MouseRightButtonDownHandler?.Invoke(sender, e);
        }

        private Rect RotateRect90(Rect rect)
        {
            double centerX = rect.Left + rect.Width / 2;
            double centerY = rect.Top + rect.Height / 2;

            double newWidth = rect.Height;
            double newHeight = rect.Width;

            double newLeft = centerX - newWidth / 2.0;
            double newTop = centerY - newHeight / 2.0;

            return new Rect(newLeft, newTop, newWidth, newHeight);
        }
    }
}
