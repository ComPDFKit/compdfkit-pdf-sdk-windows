using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool.DrawTool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Annot;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using ComPDFKitViewer.BaseObject;
using ComPDFKitViewer.Helper;
using ComPDFKitViewer.Widget;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComPDFKit.Tool
{
    partial class CPDFViewerTool
    {
        public class CommandData
        {
            public ExecutedRoutedEventArgs uIEventArgs { get; set; }

            public bool StartCommand { get; set; }

            public CPDFEditType PDFEditType { get; set; }
             
            public AnnotParam CurrentParam { get; set; } = null;
        }


        /// <summary>
        ///  System provided related command time notification
        /// </summary>
        public event EventHandler<CommandData> CommandExecutedHandler;

        public AnnotParam PasteParam { get; set; }

        internal class PDFEditCommandData
        {
            public string TextContent { get; set; }

            /// <summary>
            /// Original rectangle in PDF (72DPI)
            /// </summary>
            public Rect PDFRect { get; set; }
            public bool EditAreaCopied { get; set; }
            public CPDFEditType EditType { get; set; }
            public CPDFCopyEditArea CopyArea { get; set; }
            public CPDFEditPage EditPage { get; set; }
        }

        internal Point rightPressPoint = new Point(-1, -1);
        internal static List<PDFEditCommandData> lastPDFEditArgsList { get; private set; } = new List<PDFEditCommandData>();
        internal static List<AnnotParam> lastAnnotList { get; private set; } = new List<AnnotParam>();
        internal void BindCommand()
        {
            CommandBinding copyBind = new CommandBinding(ApplicationCommands.Copy);
            copyBind.CanExecute += CommandCanExecute;
            copyBind.Executed += CommandBind_Executed;
            CommandBindings.Add(copyBind);

            CommandBinding cutBind = new CommandBinding(ApplicationCommands.Cut);
            cutBind.CanExecute += CommandCanExecute;
            cutBind.Executed += CommandBind_Executed;
            CommandBindings.Add(cutBind);

            CommandBinding pasteBind = new CommandBinding(ApplicationCommands.Paste);
            pasteBind.CanExecute += CommandCanExecute;
            pasteBind.Executed += CommandBind_Executed;
            CommandBindings.Add(pasteBind);

            CommandBinding deleteBind = new CommandBinding(ApplicationCommands.Delete);
            deleteBind.CanExecute += CommandCanExecute;
            deleteBind.Executed += CommandBind_Executed;
            CommandBindings.Add(deleteBind);

            CommandBinding playBind = new CommandBinding(MediaCommands.Play);
            playBind.CanExecute += PlayBind_CanExecute;
            playBind.Executed += PlayBind_Executed;
            CommandBindings.Add(playBind);

            CommandBinding pastWithoutStyle = new CommandBinding(CustomCommands.PasteWithoutStyle);
            pastWithoutStyle.CanExecute += CommandCanExecute;
            pastWithoutStyle.Executed += CommandBind_Executed;
            CommandBindings.Add(pastWithoutStyle);

            CommandBinding selectAllBind = new CommandBinding(ApplicationCommands.SelectAll);
            selectAllBind.CanExecute += CommandCanExecute;
            selectAllBind.Executed += CommandBind_Executed;
            CommandBindings.Add(selectAllBind);
        }

        private void PlayBind_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BaseAnnot cmdArgs = e.Parameter as BaseAnnot;
            if (cmdArgs != null && cmdArgs.GetAnnotData() != null)
            {
                if (cmdArgs is SoundAnnot)
                {
                    (cmdArgs as SoundAnnot).Play();
                }

                if (cmdArgs is MovieAnnot)
                {
                    (cmdArgs as MovieAnnot).Play();
                }
            }
            e.Handled = true;
        }

        private void PlayBind_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            BaseAnnot cmdArgs = e.Parameter as BaseAnnot;
            if (cmdArgs != null && cmdArgs.GetAnnotData() != null)
            {
                e.CanExecute = true;
            }
            e.Handled = true;
            e.ContinueRouting = false;
        }

        public void ReBindCommand(List<CommandBinding> commandBindings)
        {
            CommandBindings.Clear();

            foreach (CommandBinding binding in commandBindings)
            {
                CommandBindings.Add(binding);
            }
        }

        private bool IsCanDoCommand = true;
        public void CanDoCommand()
        {
            IsCanDoCommand = true;
        }

        public void NotDoCommand()
        {
            IsCanDoCommand = false;
        }

        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command is RoutedUICommand uiCommand)
            {
                switch (currentModel)
                {
                    case ToolType.Viewer:
                        CheckViewerCommandStatus(uiCommand, e);
                        break;
                    case ToolType.CreateAnnot:
                    case ToolType.WidgetEdit:
                    case ToolType.Pan:
                        CheckAnnotCommandStatus(uiCommand, e);
                        break;
                    case ToolType.Customize:
                        break;
                    case ToolType.ContentEdit:
                        {
                            CheckPDFEditCommandStatus(uiCommand, e);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void CommandBind_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedUICommand uiCommand = e.Command as RoutedUICommand;
            if (uiCommand != null)
            {
                PasteParam = null;
                CommandData commandData = new CommandData();
                commandData.uIEventArgs = e;
                commandData.StartCommand = true;
                CPDFEditType editType = CPDFEditType.None;
                CommandExecutedHandler?.Invoke(this, commandData);
                if (e.Handled)
                {
                    return;
                }
                if (!IsCanDoCommand)
                {
                    CanDoCommand();
                    return;
                }
                switch (currentModel)
                {
                    case ToolType.Viewer:
                        ExecuteViewerCommand(uiCommand);
                        break;
                    case ToolType.CreateAnnot:
                    case ToolType.WidgetEdit:
                    case ToolType.Pan:
                        ExecuteAnnotCommand(uiCommand);
                        break;
                    case ToolType.ContentEdit:
                        {
                            ExecutePDFEditCommand(uiCommand, out editType);
                        }
                        break;
                    case ToolType.Customize:
                        break;
                    default:
                        break;
                }
                commandData.StartCommand = false;
                commandData.PDFEditType = editType;
                commandData.CurrentParam = PasteParam;
                CommandExecutedHandler?.Invoke(this, commandData);
            }
        }

        public void SetPastePoint(Point point)
        {
            rightPressPoint = point;
        }

        #region ContentEdit

        private void CheckPDFEditCommandStatus(RoutedUICommand uiCommand, CanExecuteRoutedEventArgs e)
        {
            switch (uiCommand.Name)
            {
                case "Copy":
                case "Cut":
                case "Delete":
                case "SelectAll":
                    {
                        if (currentEditAreaObject != null)
                        {
                            e.CanExecute = true;
                        }
                        break;
                    }
                case "Paste":
                case "PasteWithoutStyle":
                    foreach (PDFEditCommandData checkItem in lastPDFEditArgsList)
                    {
                        if (checkItem.EditType == CPDFEditType.EditText && string.IsNullOrEmpty(checkItem.TextContent) == false)
                        {
                            e.CanExecute = true;
                            break;
                        }

                        if (checkItem.EditType == CPDFEditType.EditImage && checkItem.EditAreaCopied)
                        {
                            e.CanExecute = true;
                            break;
                        }
                    }
                    if (Clipboard.ContainsText())
                    {
                        if (!string.IsNullOrEmpty(Clipboard.GetText()))
                        {
                            e.CanExecute = true;
                        }
                    }

                    if (Clipboard.ContainsImage())
                    {
                        if (Clipboard.GetImage() != null)
                        {
                            e.CanExecute = true;
                        }
                    }
                    break;
            }
        }

        private void ExecutePDFEditCommand(RoutedUICommand uiCommand, out CPDFEditType editType)
        {
            editType = CPDFEditType.None;
            switch (uiCommand.Name)
            {
                case "Copy":
                    CopyPDFEditData(out editType);
                    break;

                case "Cut":
                    CopyPDFEditData(out editType);
                    DelPDFEditData(out editType);
                    break;

                case "Delete":
                    DelPDFEditData(out editType);
                    break;

                case "Paste":
                    SetEditCopyData();
                    if (lastPDFEditArgsList.Count > 0)
                    {
                        PastePDFEditData(out editType);
                        PDFViewer.UpdateRenderFrame();
                    }
                    break;
                case "PasteWithoutStyle":
                    SetEditCopyData();
                    if (lastPDFEditArgsList.Count > 0)
                    {
                        PastePDFEditData(out editType, false);
                        PDFViewer.UpdateRenderFrame();
                    }
                    break;
                case "SelectAll":
                    SelectAllPDFEditData(out editType);
                    break;
            }
        }

        private void SetEditCopyData()
        {
            if (Clipboard.ContainsText())
            {
                string copyText = Clipboard.GetText();
                bool findCopy = false;
                if (lastPDFEditArgsList != null && lastPDFEditArgsList.Count > 0)
                {
                    foreach (PDFEditCommandData checkItem in lastPDFEditArgsList)
                    {
                        if (checkItem.EditType == CPDFEditType.EditText && copyText == checkItem.TextContent)
                        {
                            findCopy = true;
                        }
                    }
                }
                if (findCopy == false)
                {
                    lastPDFEditArgsList?.Clear();
                    if (string.IsNullOrEmpty(copyText) == false)
                    {
                        PDFEditCommandData commandData = new PDFEditCommandData();
                        commandData.EditType = CPDFEditType.EditText;
                        commandData.TextContent = copyText;
                        int PageIndex = PDFViewer.CurrentRenderFrame.PageIndex;
                        RenderData render = PDFViewer.GetCurrentRenderPageForIndex(PageIndex);
                        Rect rect = render.PaintRect;

                        Point centerPoint = new Point(
                            rect.Width / PDFViewer.CurrentRenderFrame.ZoomFactor / 2,
                            rect.Height / PDFViewer.CurrentRenderFrame.ZoomFactor / 2);
                        commandData.PDFRect = DpiHelper.StandardRectToPDFRect(new Rect(centerPoint.X, centerPoint.Y, 0, 0));
                        lastPDFEditArgsList.Add(commandData);
                    }
                }
            }
            else if (Clipboard.ContainsImage())
            {
                if (Clipboard.GetImage() != null)
                {
                    BitmapSource bitmapSource = Clipboard.GetImage();
                    PDFEditCommandData commandData = new PDFEditCommandData();
                    commandData.EditType = CPDFEditType.EditImage;
                    int PageIndex = PDFViewer.CurrentRenderFrame.PageIndex;
                    RenderData render = PDFViewer.GetCurrentRenderPageForIndex(PageIndex);
                    Rect rect = render.PaintRect;

                    Point centerPoint = new Point(
                        rect.Width / PDFViewer.CurrentRenderFrame.ZoomFactor / 2,
                        rect.Height / PDFViewer.CurrentRenderFrame.ZoomFactor / 2);
                    commandData.PDFRect = DpiHelper.StandardRectToPDFRect(new Rect(centerPoint.X, centerPoint.Y, bitmapSource.PixelWidth, bitmapSource.PixelHeight));
                    lastPDFEditArgsList.Clear();
                    lastPDFEditArgsList.Add(commandData);
                }
            }
        }

        private void CopyPDFEditData(out CPDFEditType editType)
        {
            editType = CPDFEditType.None;
            if (CPDFEditPage.CopyPage != null)
            {
                CPDFEditPage.CopyPage.ReleaseCopyEditAreaList();
            }

            lastPDFEditArgsList.Clear();
            Clipboard.Clear();

            try
            {
                if (currentEditAreaObject != null)
                {
                    PDFEditCommandData commandData = new PDFEditCommandData();
                    if (currentEditAreaObject.cPDFEditArea.Type == CPDFEditType.EditText)
                    {
                        editType = CPDFEditType.EditText;
                        CPDFEditTextArea editTextArea = currentEditAreaObject.cPDFEditArea as CPDFEditTextArea;
                        commandData.TextContent = editTextArea.SelectText;
                        if (selectAllCharsForLine || string.IsNullOrEmpty(commandData.TextContent))
                        {
                            CPDFEditPage editPage = currentEditAreaObject.cPDFEditPage;
                            commandData.EditType = CPDFEditType.EditText;
                            commandData.EditAreaCopied = editPage.CopyEditArea(currentEditAreaObject.cPDFEditArea);
                            if (commandData.EditAreaCopied)
                            {
                                List<CPDFCopyEditArea> copyList = editPage.GetCopyEditAreaList();
                                CPDFCopyEditArea CopyArea = copyList[copyList.Count - 1];
                                commandData.TextContent = CopyArea.GetCopyTextAreaContent();
                                commandData.CopyArea = CopyArea;
                                commandData.EditPage = editPage;
                            }
                        }
                        try
                        {
                            Clipboard.Clear();
                            Clipboard.SetText(commandData.TextContent);
                            Clipboard.Flush();
                        }
                        catch (Exception)
                        {

                        }
                        commandData.PDFRect = DataConversionForWPF.CRectConversionForRect(currentEditAreaObject.cPDFEditArea.GetFrame());
                        lastPDFEditArgsList.Add(commandData);
                    }
                    if (currentEditAreaObject.cPDFEditArea.Type == CPDFEditType.EditImage)
                    {
                        editType = CPDFEditType.EditImage;
                        CPDFEditPage editPage = currentEditAreaObject.cPDFEditPage;
                        commandData.PDFRect = DataConversionForWPF.CRectConversionForRect(currentEditAreaObject.cPDFEditArea.GetFrame());
                        commandData.EditAreaCopied = editPage.CopyEditArea(currentEditAreaObject.cPDFEditArea);
                        commandData.EditType = CPDFEditType.EditImage;
                        if (commandData.EditAreaCopied)
                        {
                            List<CPDFCopyEditArea> copyList = editPage.GetCopyEditAreaList();
                            commandData.CopyArea = copyList[copyList.Count - 1];
                            commandData.EditPage = editPage;
                            lastPDFEditArgsList.Add(commandData);
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void DelPDFEditData(out CPDFEditType editType)
        {
            editType = CPDFEditType.None;
            if (currentEditAreaObject != null)
            {
                GroupHistory groupHistory = new GroupHistory();
                PDFEditHistory editHistory = new PDFEditHistory();
                editHistory.EditPage = currentEditAreaObject.cPDFEditPage;
                editHistory.PageIndex = currentEditAreaObject.PageIndex;
                groupHistory.Histories.Add(editHistory);
                if (currentEditAreaObject.cPDFEditArea.Type == CPDFEditType.EditText)
                {
                    editType = CPDFEditType.EditText;
                    CPDFEditTextArea editTextArea = currentEditAreaObject.cPDFEditArea as CPDFEditTextArea;
                    string selectContent = editTextArea.SelectText;
                    if (string.IsNullOrEmpty(selectContent) == false && !selectAllCharsForLine)
                    {
                        DeleteChars();
                        EndEdit();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(selectContent))
                        {
                            editTextArea.SelectAllChars();
                        }
                        RemoveTextBlock();
                    }
                    PDFEditHistory deleteHistory = new PDFEditHistory();
                    deleteHistory.EditPage = currentEditAreaObject.cPDFEditPage;
                    deleteHistory.PageIndex = currentEditAreaObject.PageIndex;
                    groupHistory.Histories.Add(deleteHistory);
                }
                if (currentEditAreaObject.cPDFEditArea.Type == CPDFEditType.EditImage)
                {
                    editType = CPDFEditType.EditImage;
                    RemoveImageBlock();
                }
                //移除后需要重新获取数据
                PDFViewer.UpdateRenderFrame();
                if (PDFViewer != null && PDFViewer.UndoManager != null)
                {
                    PDFViewer.UndoManager.AddHistory(groupHistory);
                }
            }
        }

        private void PastePDFEditData(out CPDFEditType editType, bool pasteMatchStyle = true)
        {
            editType = CPDFEditType.None;
            double left = 0;
            double right = 0;
            double top = 0;
            double bottom = 0;
            bool initial = false;

            #region 计算最大矩形

            foreach (PDFEditCommandData commandData in lastPDFEditArgsList)
            {
                if (initial == false)
                {
                    left = commandData.PDFRect.Left;
                    right = commandData.PDFRect.Right;
                    top = commandData.PDFRect.Top;
                    bottom = commandData.PDFRect.Bottom;
                    initial = true;
                    continue;
                }
                left = Math.Min(left, commandData.PDFRect.Left);
                right = Math.Max(right, commandData.PDFRect.Right);
                top = Math.Min(top, commandData.PDFRect.Top);
                bottom = Math.Max(bottom, commandData.PDFRect.Bottom);
            }
            left = DpiHelper.PDFNumToStandardNum(left);
            right = DpiHelper.PDFNumToStandardNum(right);
            top = DpiHelper.PDFNumToStandardNum(top);
            bottom = DpiHelper.PDFNumToStandardNum(bottom);
            int offsetX = 25;
            int offsetY = 25;

            Point hoverPoint = rightPressPoint;
            rightPressPoint = new Point(-1, -1);

            int pageIndex = PDFViewer.CurrentRenderFrame.PageIndex;
            //判断右键的坐标是否在页面上
            PDFViewer.GetePointToPage(hoverPoint, out RenderData renderData, out Point pagePoint);
            if (renderData != null)
            {
                //计算与左上角点偏移
                offsetX = (int)(pagePoint.X / currentZoom - left);
                offsetY = (int)(pagePoint.Y / currentZoom - top);
                if (left + offsetX < 0)
                {
                    offsetX = (int)-left;
                }
                if (right + offsetX > renderData.PageBound.Width / currentZoom)
                {
                    offsetX = (int)(renderData.PageBound.Width / currentZoom - right);
                }
                if (top + offsetY < 0)
                {
                    offsetY = (int)-top;
                }
                if (bottom + offsetY > renderData.PageBound.Height / currentZoom)
                {
                    offsetY = (int)(renderData.PageBound.Height / currentZoom - bottom);
                }
                pageIndex = renderData.PageIndex;
            }
            else
            {
                RenderData render = PDFViewer.GetCurrentRenderPageForIndex(pageIndex);

                //最大矩形（标准DPI）
                Rect maxRect = new Rect((int)left + render.PaintRect.Left / currentZoom, (int)top + render.PaintRect.Top / currentZoom, (int)(right - left), (int)(bottom - top));

                //可视范围的中心点
                Point centerPoint = new Point(
                     render.PaintRect.Left / currentZoom + render.PaintRect.Width / currentZoom / 2,
                         render.PaintRect.Top / currentZoom + render.PaintRect.Height / currentZoom / 2);

                //可视范围
                Rect checkRect = new Rect(
                    render.PaintRect.Left / currentZoom,
                    render.PaintRect.Top / currentZoom,
                    render.PaintRect.Width / currentZoom,
                    render.PaintRect.Height / currentZoom);


                if (!checkRect.IntersectsWith(maxRect))
                {
                    offsetX = (int)(left - centerPoint.X);
                    offsetY = (int)(top - centerPoint.Y);
                }
                if (left + offsetX < 0)
                {
                    offsetX = (int)-left;
                }
                if (right + offsetX > render.RenderRect.Width / currentZoom)
                {
                    offsetX = (int)(render.RenderRect.Width / currentZoom / 2 - right);
                }
                if (top + offsetY < 0)
                {
                    offsetY = (int)-top;
                }
                if (bottom + offsetY > render.RenderRect.Height / currentZoom)
                {
                    offsetY = (int)(render.RenderRect.Height / currentZoom / 2 - bottom);
                }
            }

            #endregion

            foreach (PDFEditCommandData commandData in lastPDFEditArgsList)
            {
                GroupHistory groupHistory = new GroupHistory();
                CPDFPage docPage = PDFViewer.GetDocument().PageAtIndex(pageIndex);
                CPDFEditPage editPage = docPage.GetEditPage();

                RenderData render = PDFViewer.GetCurrentRenderPageForIndex(pageIndex);
                Rect offsetRect = AddPasteOffset(commandData.PDFRect, (int)DpiHelper.StandardNumToPDFNum(offsetX), (int)DpiHelper.StandardNumToPDFNum(offsetY), new Size(DpiHelper.StandardNumToPDFNum(render.PageBound.Width / currentZoom), DpiHelper.StandardNumToPDFNum(render.PageBound.Height / currentZoom)));
                commandData.PDFRect = offsetRect;

                if (commandData.EditType == CPDFEditType.EditText)
                {
                    editType = CPDFEditType.EditText;
                    if (pasteMatchStyle && commandData.EditAreaCopied)
                    {
                        commandData.CopyArea?.PasteEditArea(editPage, new CPoint((float)offsetRect.Left, (float)offsetRect.Top));
                        if (editPage.CanUndo())
                        {
                            PDFEditHistory editHistory = new PDFEditHistory();
                            editHistory.EditPage = editPage;
                            editHistory.PageIndex = pageIndex;
                            groupHistory.Histories.Add(editHistory);

                            CPDFViewer pdfViewer = GetCPDFViewer();
                            if (pdfViewer != null && pdfViewer.UndoManager != null)
                            {
                                pdfViewer.UndoManager.AddHistory(groupHistory);
                            }
                        }

                        SelectedEditAreaForIndex(pageIndex, editPage.GetEditAreaList().Count - 1, false);
                    }
                    else
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        if (!string.IsNullOrEmpty(commandData.TextContent))
                        {
                            EditAreaObject editAreaObject = GetHitTestAreaObject(hoverPoint);
                            if (editAreaObject != null && editAreaObject.cPDFEditArea is CPDFEditTextArea)
                            {
                                CPDFEditTextArea TextArea = editAreaObject.cPDFEditArea as CPDFEditTextArea;
                                if (TextArea.SelectLineRects.Count > 0)
                                {
                                    TextArea.DeleteChars();
                                    TextArea.ClearSelectChars();
                                    PDFEditHistory deleteHistory = new PDFEditHistory();
                                    deleteHistory.EditPage = editPage;
                                    deleteHistory.PageIndex = pageIndex;
                                    groupHistory.Histories.Add(deleteHistory);
                                }
                                TextArea.InsertText(commandData.TextContent);
                                editHistory.EditPage = editPage;
                                editHistory.PageIndex = pageIndex;
                                groupHistory.Histories.Add(editHistory);
                                CPDFViewer pdfViewer = GetCPDFViewer();
                                if (pdfViewer != null && pdfViewer.UndoManager != null)
                                {
                                    pdfViewer.UndoManager.AddHistory(groupHistory);
                                }
                            }
                            else
                            {
                                PDFEditHistory createHistory = new PDFEditHistory();
                                CPDFEditTextArea editArea = editPage.CreateNewTextArea(DataConversionForWPF.RectConversionForCRect(offsetRect), "Helvetica", 14, new byte[3] { 0, 0, 0 });

                                createHistory.EditPage = editPage;
                                createHistory.PageIndex = pageIndex;
                                groupHistory.Histories.Add(createHistory);
                                if (editArea != null)
                                {
                                    if (editArea.InsertText(commandData.TextContent))
                                    {
                                        editHistory.EditPage = editPage;
                                        editHistory.PageIndex = pageIndex;
                                        groupHistory.Histories.Add(editHistory);
                                    }
                                    SelectedEditAreaForIndex(pageIndex, editPage.GetEditAreaList().Count - 1, false);
                                }

                                CPDFViewer pdfViewer = GetCPDFViewer();
                                if (pdfViewer != null && pdfViewer.UndoManager != null)
                                {
                                    pdfViewer.UndoManager.AddHistory(groupHistory);
                                }
                            }
                        }
                    }
                }
                if (commandData.EditType == CPDFEditType.EditImage)
                {
                    editType = CPDFEditType.EditImage;
                    PDFEditHistory editHistory = new PDFEditHistory();
                    if (commandData.EditAreaCopied)
                    {
                        commandData.CopyArea?.PasteEditArea(editPage, new CPoint((float)offsetRect.Left, (float)offsetRect.Top));

                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pageIndex;
                        CPDFViewer pdfViewer = GetCPDFViewer();
                        if (pdfViewer != null && pdfViewer.UndoManager != null)
                        {
                            pdfViewer.UndoManager.AddHistory(editHistory);
                        }

                        SelectedEditAreaForIndex(pageIndex, editPage.GetEditAreaList().Count - 1, false);
                    }
                    else
                    {
                        if (Clipboard.GetImage() != null)
                        {
                            PDFEditHistory createHistory = new PDFEditHistory();
                            BitmapSource bitmapSource = BinaryStructConverter.ImageFromClipboardDib();

                            byte[] imageData = new byte[bitmapSource.PixelWidth * bitmapSource.PixelHeight * 4];
                            if (bitmapSource.Format != PixelFormats.Bgra32)
                            {
                                FormatConvertedBitmap covert = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, bitmapSource.Palette, 0);
                                covert.CopyPixels(imageData, bitmapSource.PixelWidth * 4, 0);
                            }
                            else
                            {
                                bitmapSource.CopyPixels(imageData, bitmapSource.PixelWidth * 4, 0);
                            }

                            CPDFEditImageArea editArea = editPage.CreateNewImageArea(DataConversionForWPF.RectConversionForCRect(offsetRect), imageData, bitmapSource.PixelWidth, bitmapSource.PixelHeight);

                            createHistory.EditPage = editPage;
                            createHistory.PageIndex = pageIndex;
                            groupHistory.Histories.Add(createHistory);
                            if (editArea != null)
                            {
                                editHistory.EditPage = editPage;
                                editHistory.PageIndex = pageIndex;
                                SelectedEditAreaForIndex(pageIndex, editPage.GetEditAreaList().Count - 1, false);
                            }
                            CPDFViewer pdfViewer = GetCPDFViewer();
                            if (pdfViewer != null && pdfViewer.UndoManager != null)
                            {
                                pdfViewer.UndoManager.AddHistory(groupHistory);
                            }
                        }
                    }
                }

                editPage.EndEdit();
            }
        }

        private void SelectAllPDFEditData(out CPDFEditType editType)
        {
            editType = CPDFEditType.None;
            if (currentEditAreaObject != null && currentEditAreaObject.cPDFEditPage != null)
            {
                CPDFEditTextArea textArea = currentEditAreaObject.cPDFEditArea as CPDFEditTextArea;
                textArea.SelectAllChars();

                editType = CPDFEditType.EditText;
                PDFViewer.UpdateRenderFrame();
            }
        }
        private Rect AddPasteOffset(Rect clientRect, int offsetX, int offsetY, Size pageSize)
        {
            clientRect.X += offsetX;
            clientRect.Y += offsetY;
            if (clientRect.Left < 0)
            {
                clientRect.X = 0;
            }
            if (clientRect.Top < 0)
            {
                clientRect.Y = 0;
            }
            if (clientRect.Right > pageSize.Width)
            {
                clientRect.X = pageSize.Width - Math.Min(clientRect.Width, pageSize.Width);
            }
            if (clientRect.Bottom > pageSize.Height)
            {
                clientRect.Y = pageSize.Height - Math.Min(clientRect.Height, pageSize.Height);
            }

            return clientRect;
        }

        #endregion


        #region Annot

        private bool CheckCacheHitTestAnnot(string cmdName = "")
        {
            if (cacheHitTestAnnot == null)
            {
                return false;
            }
            AnnotData hitData = cacheHitTestAnnot.GetAnnotData();
            if (hitData == null)
            {
                return false;
            }
            if (hitData.Annot == null)
            {
                return false;
            }

            if (cmdName == "Delete" && hitData.Annot is CPDFSignatureWidget)
            {
                CPDFSignatureWidget signAnnot = hitData.Annot as CPDFSignatureWidget;
                if (signAnnot != null && signAnnot.IsSigned())
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckAnnotCanDoCopyCut(bool isCut = false)
        {
            if (!CheckCacheHitTestAnnot(isCut ? "Delete" : ""))
            {
                return false;
            }
            if (cacheHitTestAnnot.GetAnnotData().Annot.GetIsLocked() && isCut)
            {
                return false;
            }
            switch (cacheHitTestAnnot.CurrentType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_HIGHLIGHT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_SQUIGGLY:
                case C_ANNOTATION_TYPE.C_ANNOTATION_UNDERLINE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_STRIKEOUT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_REDACT:
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA:
                    return false;
                default:
                    break;
            }
            return true;
        }

        private void CheckViewerCommandStatus(RoutedUICommand uiCommand, CanExecuteRoutedEventArgs e)
        {
            switch (uiCommand.Name)
            {
                case "Copy":
                    TextSelectInfo textSelectInfo = GetTextSelectInfo();
                    if (!e.CanExecute && textSelectInfo != null)
                    {
                        foreach (int key in textSelectInfo.PageSelectText.Keys)
                        {
                            if (textSelectInfo.PageSelectText[key] != string.Empty)
                            {
                                e.CanExecute = true;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        private void CheckAnnotCommandStatus(RoutedUICommand uiCommand, CanExecuteRoutedEventArgs e)
        {
            switch (uiCommand.Name)
            {
                case "Copy":
                    e.CanExecute = CheckAnnotCanDoCopyCut();
                    TextSelectInfo textSelectInfo = GetTextSelectInfo();
                    if (!e.CanExecute && textSelectInfo != null)
                    {
                        foreach (int key in textSelectInfo.PageSelectText.Keys)
                        {
                            if (textSelectInfo.PageSelectText[key] != string.Empty)
                            {
                                e.CanExecute = true;
                                break;
                            }
                        }
                    }
                    break;
                case "Cut":
                    e.CanExecute = CheckAnnotCanDoCopyCut(true);
                    break;
                case "Delete":
                    e.CanExecute = CheckCacheHitTestAnnot("Delete");
                    break;
                case "Paste":
                    if (lastAnnotList.Count > 0)
                    {
                        e.CanExecute = true;
                    }
                    break;
            }
        }

        private void ExecuteViewerCommand(RoutedUICommand uiCommand)
        {
            switch (uiCommand.Name)
            {
                case "Copy":
                    TextSelectInfo textSelectInfo = GetTextSelectInfo();
                    if (textSelectInfo != null)
                    {
                        StringBuilder copyTextBuilder = new StringBuilder();
                        foreach (int key in textSelectInfo.PageSelectText.Keys)
                        {
                            if (textSelectInfo.PageSelectText[key] != string.Empty)
                            {
                                copyTextBuilder.Append(textSelectInfo.PageSelectText[key]);
                            }
                        }
                        if (copyTextBuilder.Length > 0)
                        {
                            try
                            {
                                Clipboard.Clear();
                                Clipboard.SetText(copyTextBuilder.ToString());
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        private void ExecuteAnnotCommand(RoutedUICommand uiCommand)
        {
            switch (uiCommand.Name)
            {
                case "Copy":
                    if (cacheHitTestAnnot != null)
                    {
                        lastAnnotList.Clear();
                        AnnotParam annotParam = ParamConverter.AnnotConverter(GetCPDFViewer().GetDocument(), cacheHitTestAnnot.GetAnnotData().Annot);
                        lastAnnotList.Add(annotParam);
                    }
                    if (cacheMoveWidget != null)
                    {
                        lastAnnotList.Clear();
                        AnnotParam annotParam = ParamConverter.WidgetConverter(GetCPDFViewer().GetDocument(), cacheMoveWidget.GetAnnotData().Annot);
                        lastAnnotList.Add(annotParam);
                    }
                    else
                    {
                        TextSelectInfo textSelectInfo = GetTextSelectInfo();
                        if (textSelectInfo != null)
                        {
                            StringBuilder copyTextBuilder = new StringBuilder();
                            foreach (int key in textSelectInfo.PageSelectText.Keys)
                            {
                                if (textSelectInfo.PageSelectText[key] != string.Empty)
                                {
                                    copyTextBuilder.Append(textSelectInfo.PageSelectText[key]);
                                }
                            }
                            if (copyTextBuilder.Length > 0)
                            {
                                try
                                {
                                    Clipboard.Clear();
                                    Clipboard.SetText(copyTextBuilder.ToString());
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    break;

                case "Cut":
                    if (cacheHitTestAnnot != null)
                    {
                        lastAnnotList.Clear();
                        AnnotParam annotParam;
                        if (cacheHitTestAnnot.CurrentType == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                        {
                            annotParam = ParamConverter.WidgetConverter(GetCPDFViewer().GetDocument(), cacheHitTestAnnot.GetAnnotData().Annot);
                        }
                        else
                        {
                            annotParam = ParamConverter.AnnotConverter(GetCPDFViewer().GetDocument(), cacheHitTestAnnot.GetAnnotData().Annot);
                        }
                        lastAnnotList.Add(annotParam);
                    }
                    if (cacheMoveWidget != null)
                    {
                        lastAnnotList.Clear();
                        AnnotParam annotParam = ParamConverter.WidgetConverter(GetCPDFViewer().GetDocument(), cacheMoveWidget.GetAnnotData().Annot);
                        lastAnnotList.Add(annotParam);
                    }
                    DeleteAnnotData();
                    PDFViewer.UpdateAnnotFrame();
                    break;

                case "Delete":
                    DeleteAnnotData();
                    PDFViewer.UpdateAnnotFrame();
                    break;

                case "Paste":
                    PasteAnnotData();
                    PDFViewer.UpdateAnnotFrame();
                    break;
            }
        }
        private void PasteAnnotData(bool pasteMatchStyle = false)
        {
            AnnotHistory annotHistory = null;
            foreach (AnnotParam item in lastAnnotList)
            {
                if (item == null)
                {
                    continue;
                }
                Point point = rightPressPoint;
                rightPressPoint = new Point(-1, -1);

                switch (item.CurrentType)
                {
                    case C_ANNOTATION_TYPE.C_ANNOTATION_TEXT:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new StickyNoteAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);
                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(item.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, item.CurrentType, item);
                            cPDFAnnotation.SetRect(setRect);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as StickyNoteAnnotHistory).CurrentParam = (StickyNoteParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINK:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new LinkAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);
                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(item.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, item.CurrentType, item);
                            cPDFAnnotation.SetRect(setRect);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as LinkAnnotHistory).CurrentParam = (LinkParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_FREETEXT:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new FreeTextAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);
                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(item.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, item.CurrentType, item);
                            cPDFAnnotation.SetRect(setRect);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as FreeTextAnnotHistory).CurrentParam = (FreeTextParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new LineAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);

                            LineParam newLineParam = new LineParam();
                            item.CopyTo(newLineParam);
                            {
                                float offsetX = setRect.left - item.ClientRect.left;
                                float offsetY = setRect.top - item.ClientRect.top;
                                newLineParam.HeadPoint = new CPoint(newLineParam.HeadPoint.x + offsetX, newLineParam.HeadPoint.y + offsetY);
                                newLineParam.TailPoint = new CPoint(newLineParam.TailPoint.x + offsetX, newLineParam.TailPoint.y + offsetY);
                            }

                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(item.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, item.CurrentType, item);
                            (cPDFAnnotation as CPDFLineAnnotation)?.SetLinePoints(newLineParam.HeadPoint, newLineParam.TailPoint);
                            cPDFAnnotation.SetRect(setRect);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as LineAnnotHistory).CurrentParam = (LineParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_SQUARE:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new SquareAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);
                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(item.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, item.CurrentType, item);
                            cPDFAnnotation.SetRect(setRect);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as SquareAnnotHistory).CurrentParam = (SquareParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_CIRCLE:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new CircleAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);
                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(item.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, item.CurrentType, item);
                            cPDFAnnotation.SetRect(setRect);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as CircleAnnotHistory).CurrentParam = (CircleParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_STAMP:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new StampAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);
                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(item.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, item.CurrentType, item);
                            cPDFAnnotation.SetRect(setRect);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as StampAnnotHistory).CurrentParam = (StampParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_CARET:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_INK:
                        {
                            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
                            Rect rect;
                            annotHistory = new InkAnnotHistory();
                            annotHistory.PDFDoc = cPDFDocument;
                            int index = -1;
                            if (point.Equals(new Point(-1, -1)))
                            {
                                index = item.PageIndex;
                                rect = new Rect(
                                    (item.ClientRect.left + 25),
                                    (item.ClientRect.top + 25),
                                    item.ClientRect.width(),
                                    item.ClientRect.height()
                                );
                            }
                            else
                            {
                                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                                CRect cRect = item.ClientRect;
                                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                                rect = new Rect(
                                    (pdfPoint.X - cRect.width() / 2),
                                    (pdfPoint.Y - cRect.height() / 2),
                                    cRect.width(),
                                    cRect.height()
                                );
                            }
                            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);

                            InkParam newInkParam = new InkParam();
                            item.CopyTo(newInkParam);

                            if (newInkParam.InkPath != null && newInkParam.InkPath.Count > 0)
                            {
                                float offsetX = setRect.left - item.ClientRect.left;
                                float offsetY = setRect.top - item.ClientRect.top;
                                List<List<CPoint>> arrangeList = new List<List<CPoint>>();
                                foreach (List<CPoint> inkNode in newInkParam.InkPath)
                                {
                                    List<CPoint> inkPath = new List<CPoint>();
                                    arrangeList.Add(inkPath);
                                    foreach (CPoint addPoint in inkNode)
                                    {
                                        inkPath.Add(new CPoint(addPoint.x + offsetX, addPoint.y + offsetY));
                                    }
                                }
                                newInkParam.InkPath = arrangeList;
                                newInkParam.ClientRect = setRect;
                            }

                            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
                            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateAnnot(newInkParam.CurrentType);
                            CreateDefaultAnnot(cPDFAnnotation, newInkParam.CurrentType, newInkParam);
                            cPDFAnnotation.UpdateAp();
                            AnnotParam annotParam = ParamConverter.AnnotConverter(cPDFDocument, cPDFAnnotation);
                            (annotHistory as InkAnnotHistory).CurrentParam = (InkParam)annotParam;
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_POPUP:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_FILEATTACHMENT:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                        {
                            rightPressPoint = point;
                            PasteWidgetData(ref annotHistory, item as WidgetParm);
                        }
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_SCREEN:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_PRINTERMARK:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_TRAPNET:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_WATERMARK:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_3D:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA:
                        break;
                    case C_ANNOTATION_TYPE.C_ANNOTATION_INTERCHANGE:
                        break;
                    default:
                        break;
                }
                PasteParam= annotHistory.CurrentParam;
                PDFViewer.UndoManager.AddHistory(annotHistory);
            }
        }

        private void DeleteAnnotData()
        {
            dynamic notifyData = null;

            if (cacheHitTestAnnot != null)
            {
                AnnotHistory annotHistory = ParamConverter.CreateHistory(cacheHitTestAnnot.GetAnnotData().Annot);
                if (annotHistory == null)
                {
                    return;
                }
                AnnotParam annotParam = null;

                if (cacheHitTestAnnot is BaseWidget)
                {
                    annotParam = ParamConverter.WidgetConverter(PDFViewer.GetDocument(), cacheHitTestAnnot.GetAnnotData().Annot);
                }
                else
                {
                    annotParam = ParamConverter.AnnotConverter(PDFViewer.GetDocument(), cacheHitTestAnnot.GetAnnotData().Annot);
                }
                if (annotParam != null)
                {
                    notifyData = new ExpandoObject();
                    notifyData.Action = HistoryAction.Remove;
                    notifyData.PageIndex = annotParam.PageIndex;
                    notifyData.AnnotIndex = annotParam.AnnotIndex;
                    notifyData.AnnotType = annotParam.CurrentType;
                    notifyData.CurrentParam = annotParam;
                    if (annotParam is WidgetParm)
                    {
                        notifyData.WidgetType = (annotParam as WidgetParm).WidgetType;
                    }

                    annotHistory.CurrentParam = annotParam;
                    annotHistory.PDFDoc = PDFViewer.GetDocument();
                    annotHistory.Action = HistoryAction.Remove;
                    PDFViewer.UndoManager.AddHistory(annotHistory);

                    List<C_ANNOTATION_TYPE> checkEditType = new List<C_ANNOTATION_TYPE>()
                    {
                        C_ANNOTATION_TYPE.C_ANNOTATION_LINE,
                        C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE,
                        C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON
                    };

                    if (checkEditType.Contains(cacheHitTestAnnot.CurrentType))
                    {
                        //需要清除测量选中编辑框
                        CleanEditAnnot();
                    }
                    cacheHitTestAnnot.RemoveClean(true);
                    cacheHitTestAnnot.GetAnnotData().Annot.RemoveAnnot();
                    cacheHitTestAnnot = null;
                }
                CleanSelectedRect();
            }
            if (cacheMoveWidget != null)
            {
                AnnotHistory annotHistory = ParamConverter.CreateHistory(cacheMoveWidget.GetAnnotData().Annot);
                if (annotHistory == null)
                {
                    return;
                }
                AnnotParam annotParam = ParamConverter.WidgetConverter(PDFViewer.GetDocument(), cacheMoveWidget.GetAnnotData().Annot);
                if (annotParam != null)
                {
                    notifyData = new ExpandoObject();
                    notifyData.Action = HistoryAction.Remove;
                    notifyData.PageIndex = annotParam.PageIndex;
                    notifyData.AnnotIndex = annotParam.AnnotIndex;
                    notifyData.AnnotType = annotParam.CurrentType;
                    notifyData.WidgetType = (annotParam as WidgetParm).WidgetType;
                    notifyData.CurrentParam = annotParam;

                    annotHistory.CurrentParam = annotParam;
                    annotHistory.PDFDoc = PDFViewer.GetDocument();
                    annotHistory.Action = HistoryAction.Remove;
                    PDFViewer.UndoManager.AddHistory(annotHistory);
                    cacheMoveWidget.GetAnnotData().Annot.RemoveAnnot();
                    cacheMoveWidget = null;

                }
                CleanSelectedRect();
            }
            if (notifyData != null)
            {
                AnnotChanged?.Invoke(this, notifyData);
            }
        }

        private void PasteWidgetData(ref AnnotHistory annotHistory, WidgetParm item)
        {
            CPDFDocument cPDFDocument = PDFViewer.GetDocument();
            Point point = rightPressPoint;
            rightPressPoint = new Point(-1, -1);

            Rect rect;
            int index = -1;
            if (point.Equals(new Point(-1, -1)))
            {
                index = item.PageIndex;
                rect = new Rect(
                    (item.ClientRect.left + 25),
                    (item.ClientRect.top + 25),
                    item.ClientRect.width(),
                    item.ClientRect.height()
                );
            }
            else
            {
                PDFViewer.GetPointPageInfo(point, out index, out Rect paintRect, out var pageBound);
                CRect cRect = item.ClientRect;
                Point zoomPoint = new Point((point.X - pageBound.X) / currentZoom, (point.Y - pageBound.Y) / currentZoom);
                Point pdfPoint = DpiHelper.StandardPointToPDFPoint(zoomPoint);
                rect = new Rect(
                    (pdfPoint.X - cRect.width() / 2),
                    (pdfPoint.Y - cRect.height() / 2),
                    cRect.width(),
                    cRect.height()
                );
            }

            CPDFPage cPDFPage = cPDFDocument.PageAtIndex(index);
            CPDFAnnotation cPDFAnnotation = cPDFPage.CreateWidget(item.WidgetType);
            CreateDefaultWidget(cPDFAnnotation, item.WidgetType, item);

            CRect setRect = DataConversionForWPF.RectConversionForCRect(rect);
            cPDFAnnotation.SetRect(setRect);
            (cPDFAnnotation as CPDFWidget).UpdateFormAp();
            AnnotParam annotParam = ParamConverter.WidgetConverter(cPDFDocument, cPDFAnnotation);
            switch (item.WidgetType)
            {
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_NONE:
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_PUSHBUTTON:
                    annotHistory = new PushButtonHistory();
                    (annotHistory as PushButtonHistory).CurrentParam = (PushButtonParam)annotParam;
                    annotHistory.PDFDoc = cPDFDocument;
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_CHECKBOX:
                    annotHistory = new CheckBoxHistory();
                    (annotHistory as CheckBoxHistory).CurrentParam = (CheckBoxParam)annotParam;
                    annotHistory.PDFDoc = cPDFDocument;
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_RADIOBUTTON:
                    annotHistory = new RadioButtonHistory();
                    (annotHistory as RadioButtonHistory).CurrentParam = (RadioButtonParam)annotParam;
                    annotHistory.PDFDoc = cPDFDocument;
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_TEXTFIELD:
                    annotHistory = new TextBoxHistory();
                    (annotHistory as TextBoxHistory).CurrentParam = (TextBoxParam)annotParam;
                    annotHistory.PDFDoc = cPDFDocument;
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_COMBOBOX:
                    annotHistory = new ComboBoxHistory();
                    (annotHistory as ComboBoxHistory).CurrentParam = (ComboBoxParam)annotParam;
                    annotHistory.PDFDoc = cPDFDocument;
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_LISTBOX:
                    annotHistory = new ListBoxHistory();
                    (annotHistory as ListBoxHistory).CurrentParam = (ListBoxParam)annotParam;
                    annotHistory.PDFDoc = cPDFDocument;
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS:
                    annotHistory = new SignatureHistory();
                    (annotHistory as SignatureHistory).CurrentParam = (SignatureParam)annotParam;
                    annotHistory.PDFDoc = cPDFDocument;
                    break;
                case PDFAnnotation.Form.C_WIDGET_TYPE.WIDGET_UNKNOWN:
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
