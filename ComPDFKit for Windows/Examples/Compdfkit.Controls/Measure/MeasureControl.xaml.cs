using ComPDFKit.Measure;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool.Help;
using ComPDFKitViewer.BaseObject;
using Path = System.IO.Path;
using ComPDFKit.Tool.DrawTool;

namespace ComPDFKit.Controls.Measure
{
    public partial class MeasureControl : UserControl
    {
        public MeasurePropertyControl measurePropertyControl = new MeasurePropertyControl();
        private CPDFDisplaySettingsControl displaySettingsControl;

        private PDFViewControl PdfViewControl = new PDFViewControl();

        private PanelState panelState = PanelState.GetInstance();
        
        private CPDFAnnotation currentAnnot = null;

        public event EventHandler ExpandEvent;
        public event EventHandler OnAnnotEditHandler;


        public MeasureControl()
        {
            InitializeComponent();
        }
        
        #region Init PDFViewer

        public void InitWithPDFViewer(PDFViewControl pdfViewControl)
        {
            PdfViewControl = pdfViewControl;
            //PdfViewControl.PDFView = pdfViewer;
            PDFMeasureTool.InitWithPDFViewer(pdfViewControl, measurePropertyControl, this);
            FloatPageTool.InitWithPDFViewer(pdfViewControl);
            PDFGrid.Child = PdfViewControl;

            panelState.PropertyChanged -= PanelState_PropertyChanged;
            PdfViewControl.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonUpHandler -= PDFToolManager_MouseLeftButtonUpHandler;
            PdfViewControl.MouseMoveHandler -= PDFToolManager_MouseMoveHandler;
            pdfViewControl.MouseRightButtonDownHandler -= PDFToolManager_MouseRightButtonDownHandler;
            PdfViewControl.PDFViewTool.MeasureChanged -= MeasureSetting_MeasureChanged;

            PdfViewControl.PDFViewTool.MeasureChanged += MeasureSetting_MeasureChanged;
            PdfViewControl.MouseLeftButtonDownHandler += PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonUpHandler += PDFToolManager_MouseLeftButtonUpHandler;
            PdfViewControl.MouseMoveHandler += PDFToolManager_MouseMoveHandler;
            pdfViewControl.MouseRightButtonDownHandler += PDFToolManager_MouseRightButtonDownHandler;
            panelState.PropertyChanged += PanelState_PropertyChanged; 
            SetInfoPanelVisble(false, false);
            SettingPanel.PdfViewControl= pdfViewControl;
        }

        private void MeasureSetting_MeasureChanged(object sender, MeasureEventArgs e)
        {
            InfoPanel.SetMeasureType(e.Type);
            InfoPanel.SetMeasureInfo(e);
        }

        private void PDFToolManager_MouseRightButtonDownHandler(object sender, MouseEventObject e)
        {
            ContextMenu ContextMenu = PdfViewControl.GetRightMenu();
            if (ContextMenu == null)
            {
                ContextMenu = new ContextMenu();
            }
            switch (e.hitTestType)
            {
                case MouseHitTestType.Annot:
                case MouseHitTestType.SelectRect:
                    CreateAnnotContextMenu(sender, ref ContextMenu, e.annotType);
                    break;
                case MouseHitTestType.Text:
                    CreateSelectTextContextMenu(sender, ref ContextMenu);
                    break;
                case MouseHitTestType.ImageSelect:
                    CreateSelectImageContextMenu(sender, ref ContextMenu);
                    break;
                default:
                    ContextMenu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    PdfViewControl.CreateViewerMenu(sender, ref ContextMenu);
                    break;
            }
            PdfViewControl.SetRightMenu(ContextMenu);
        }
        
        private void CreateAnnotContextMenu(object sender, ref ContextMenu menu, C_ANNOTATION_TYPE annotType)
        {
            switch (annotType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    CreateMeasureContextMenu(sender, ref menu);
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_SOUND:
                case C_ANNOTATION_TYPE.C_ANNOTATION_MOVIE:
                case C_ANNOTATION_TYPE.C_ANNOTATION_RICHMEDIA:
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Play"), Command = MediaCommands.Play, CommandTarget = (UIElement)sender, CommandParameter = (sender as CPDFViewerTool).GetCacheHitTestAnnot() });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    break;
                default:
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Cut"), Command = ApplicationCommands.Cut, CommandTarget = (UIElement)sender });
                    menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Paste"), Command = ApplicationCommands.Paste, CommandTarget = (UIElement)sender });
                    break;
            }
        }
        
        private void CreateMeasureContextMenu(object sender, ref ContextMenu menu)
        {
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Delete"), Command = ApplicationCommands.Delete, CommandTarget = (UIElement)sender });
            
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "Measurement Settings";
            menuItem.Click += (item, param) =>
            {
                if (currentAnnot is CPDFLineAnnotation annotation)
                {
                    SettingPanel.BindMeasureSetting(annotation.GetDistanceMeasure().MeasureInfo);
                }
                SetInfoPanelVisble(false, true);
            };
            menu.Items.Add(menuItem);
            
            MenuItem propertyItem = new MenuItem();
            propertyItem.Header = "Properties";
            propertyItem.Click += (item, param) =>
            {
                panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
            };
            menu.Items.Add(propertyItem);
        }
        
        private void CreateSelectTextContextMenu(object sender, ref ContextMenu menu)
        {
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
        }
        
        private void CreateSelectImageContextMenu(object sender, ref ContextMenu menu)
        {
            if (menu == null)
            {
                menu = new ContextMenu();
            }
            MenuItem copyImage = new MenuItem();
            copyImage.Header = "Copy Image";
            copyImage.Click += CopyImage_Click;
            menu.Items.Add(copyImage);

            MenuItem extractImage = new MenuItem();
            extractImage.Header = "Extract Image";
            extractImage.Click += ExtractImage_Click;
            menu.Items.Add(extractImage);
        }

        private void ExtractImage_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PageImageItem image = null;
                Dictionary<int, List<PageImageItem>> pageImageDict = PdfViewControl.FocusPDFViewTool.GetSelectImageItems();
                if (pageImageDict != null && pageImageDict.Count > 0)
                {
                    foreach (int pageIndex in pageImageDict.Keys)
                    {
                        List<PageImageItem> imageItemList = pageImageDict[pageIndex];
                        image = imageItemList[0];
                        break;
                    }
                }

                if (image == null)
                {
                    return;
                }

                CPDFPage page = PdfViewControl.PDFToolManager.GetDocument().PageAtIndex(image.PageIndex);
                string savePath = System.IO.Path.Combine(folderDialog.SelectedPath, Guid.NewGuid() + ".jpg");
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + ".jpg");
                page.GetImgSelection().GetImgBitmap(image.ImageIndex, tempPath);

                Bitmap bitmap = new Bitmap(tempPath);
                bitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                Process.Start("explorer", "/select,\"" + savePath + "\"");
            }
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            PageImageItem image = null;
            Dictionary<int, List<PageImageItem>> pageImageDict = PdfViewControl.FocusPDFViewTool.GetSelectImageItems();
            if (pageImageDict != null && pageImageDict.Count > 0)
            {
                foreach (int pageIndex in pageImageDict.Keys)
                {
                    List<PageImageItem> imageItemList = pageImageDict[pageIndex];
                    image = imageItemList[0];
                    break;
                }
            }

            if (image == null)
            {
                return;
            }
            CPDFPage page = PdfViewControl.PDFToolManager.GetDocument().PageAtIndex(image.PageIndex);
            string tempPath = System.IO.Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
            page.GetImgSelection().GetImgBitmap(image.ImageIndex, tempPath);

            Bitmap bitmap = new Bitmap(tempPath);
            BitmapImage imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageData = new BitmapImage();
                imageData.BeginInit();
                imageData.StreamSource = ms;

                imageData.CacheOption = BitmapCacheOption.OnLoad;
                imageData.EndInit();
                imageData.Freeze();
                Clipboard.SetImage(imageData);
                bitmap.Dispose();
                File.Delete(tempPath);
            }
        }

        private void PDFToolManager_MouseMoveHandler(object sender, MouseEventObject e)
        {
        }

        private void PDFToolManager_MouseLeftButtonUpHandler(object sender, MouseEventObject e)
        {
            if (e.IsCreate)
            {
                OnAnnotEditHandler?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PDFToolManager_MouseLeftButtonDownHandler(object sender, MouseEventObject e)
        {
            BaseAnnot baseAnnot = PdfViewControl.GetCacheHitTestAnnot();
            if (baseAnnot != null)
            {
                AnnotData annotData = baseAnnot.GetAnnotData();
                AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                    PdfViewControl.GetCPDFViewer().GetDocument(), annotData.PageIndex, annotData.Annot);
                measurePropertyControl.SetPropertyForMeasureCreate(annotParam, annotData.Annot, PdfViewControl);
                SetMeasureInfoPanel(annotData.Annot, annotParam);
                currentAnnot = annotData.Annot;
            }

            panelState.RightPanel = PanelState.RightPanelState.PropertyPanel;
            // measurePropertyControl.SetPropertyForMeasureCreate(LineArgs, e);
            SetInfoPanelVisble(true, false);
        }

        private void SetMeasureInfoPanel(CPDFAnnotation annot,AnnotParam param = null)
        {
            if (annot == null)
            {
                return;
            }
            try
            {
                if (annot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_LINE)
                {
                    CPDFLineAnnotation lineAnnot = annot as CPDFLineAnnotation;
                    if (lineAnnot.IsMeasured() && lineAnnot.Points != null && lineAnnot.Points.Count() == 2)
                    {
                        InfoPanel.SetMeasureInfo(lineAnnot);
                        SetMeasureInfoType(CPDFMeasureType.CPDF_DISTANCE_MEASURE);
                    }
                }

                if (annot.Type == C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE)
                {
                    CPDFPolylineAnnotation polylineAnnot = annot as CPDFPolylineAnnotation;
                    if (polylineAnnot.IsMeasured() && polylineAnnot.Points != null && polylineAnnot.Points.Count() >= 2)
                    {
                        InfoPanel.SetMeasureInfo(polylineAnnot);
                        SetMeasureInfoType(CPDFMeasureType.CPDF_PERIMETER_MEASURE);
                    }
                }
                
                if(annot.Type== C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON)
                {
                    CPDFPolygonAnnotation Annot = annot as CPDFPolygonAnnotation;
                    CPDFAreaMeasure polygonMeasure = Annot.GetAreaMeasure();
                    CPDFMeasureInfo measureInfo = polygonMeasure.MeasureInfo;
                    CPDFCaptionType CaptionType = measureInfo.CaptionType;
                    bool IsArea = false;
                    bool IsLength = false;
                    if ((CaptionType& CPDFCaptionType.CPDF_CAPTION_AREA)== CPDFCaptionType.CPDF_CAPTION_AREA)
                    {
                        IsArea = true;
                    }
                    if ((CaptionType & CPDFCaptionType.CPDF_CAPTION_LENGTH) == CPDFCaptionType.CPDF_CAPTION_LENGTH)
                    {
                        IsLength = true;
                    }
                    SettingPanel.ChangedCheckBoxIsChecked(IsArea, IsLength);
                    InfoPanel.SetMeasureInfo(Annot);
                    SetMeasureInfoType(CPDFMeasureType.CPDF_AREA_MEASURE);
                }
            }
            catch (Exception e)
            {

            }
        }

        private double GetMeasureRatio(string baseUnit)
        {
            if (baseUnit == CPDFMeasure.CPDF_PT)
            {
                return 1 / 72;
            }
            if (baseUnit == CPDFMeasure.CPDF_IN)
            {
                return 1;
            }
            if (baseUnit == CPDFMeasure.CPDF_MM)
            {
                return 1 / 25.4;
            }
            if (baseUnit == CPDFMeasure.CPDF_CM)
            {
                return 1 / 2.54;
            }
            if (baseUnit == CPDFMeasure.CPDF_M)
            {
                return 1 / 0.0254;
            }
            if (baseUnit == CPDFMeasure.CPDFO_KM)
            {
                return 1 / 0.0254 / 1000;
            }

            if (baseUnit == CPDFMeasure.CPDF_FT)
            {
                return 12;
            }
            if (baseUnit == CPDFMeasure.CPDF_YD)
            {
                return 36;
            }
            if (baseUnit == CPDFMeasure.CPDF_MI)
            {
                return 63360;
            }
            return 0;
        }

        public void SetSettingsControl(CPDFDisplaySettingsControl cPDFDisplaySettingsControl)
        {
            displaySettingsControl = cPDFDisplaySettingsControl;
        }

        public void ClearAllToolState()
        {
            PDFMeasureTool.ClearAllToolState();
            InfoPanel.ClearMeasureInfo();
        }
        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            displaySettingsControl = null;
        }

        public void UnloadEvent()
        {
            PdfViewControl.MouseLeftButtonDownHandler -= PDFToolManager_MouseLeftButtonDownHandler;
            PdfViewControl.MouseLeftButtonUpHandler -= PDFToolManager_MouseLeftButtonUpHandler;
            PdfViewControl.MouseMoveHandler -= PDFToolManager_MouseMoveHandler;
            panelState.PropertyChanged -= PanelState_PropertyChanged;
        }

        private void PanelState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PanelState.IsLeftPanelExpand))
            {
                ExpandLeftPanel(panelState.IsLeftPanelExpand);
            }
            else if (e.PropertyName == nameof(PanelState.RightPanel))
            {
                if (panelState.RightPanel == PanelState.RightPanelState.PropertyPanel)
                {
                    ExpandRightPropertyPanel(measurePropertyControl, Visibility.Visible);
                }
                else if (panelState.RightPanel == PanelState.RightPanelState.ViewSettings)
                {
                    ExpandRightPropertyPanel(displaySettingsControl, Visibility.Visible);
                }
                else
                {
                    ExpandRightPropertyPanel(null, Visibility.Collapsed);
                }
            }
        }

        #endregion

        #region Expand and collapse Panel

        public void ExpandRightPropertyPanel(Visibility visible)
        {
            ExpandRightPropertyPanel(measurePropertyControl, visible);
        }

        public void ExpandNullRightPropertyPanel(Visibility visible)
        {
            ExpandRightPropertyPanel(null, visible);
        }

        public void ExpandViewSettings(Visibility visible)
        {
            SetViewSettings(displaySettingsControl, visible);
        }

        private void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible;
        }

        private void SetViewSettings(CPDFDisplaySettingsControl displaySettingsControl, Visibility visibility)
        {
            PropertyContainer.Child = displaySettingsControl;
            PropertyContainer.Visibility = visibility;
        }

        public void ExpandLeftPanel(bool isExpand)
        {
            BotaContainer.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            Splitter.Visibility = isExpand ? Visibility.Visible : Visibility.Collapsed;
            if (isExpand)
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(320);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(15);
            }
            else
            {
                BodyGrid.ColumnDefinitions[0].Width = new GridLength(0);
                BodyGrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MeasureInfoPanel_SettingClick(object sender, EventArgs e)
        {
            if (sender is MeasureInfoPanel)
            {
                switch ((sender as MeasureInfoPanel).MeasureType)
                {
                    case CPDFMeasureType.CPDF_DISTANCE_MEASURE:
                    case CPDFMeasureType.CPDF_PERIMETER_MEASURE:
                        SettingPanel.ShowAreaAndLength(Visibility.Collapsed);
                        break;
                    case CPDFMeasureType.CPDF_AREA_MEASURE:
                        SettingPanel.ShowAreaAndLength(Visibility.Visible);
                        break;
                    default:
                        break;
                }
            }
            SettingPanel.ReturnToInfoPanel = true;
            SetInfoPanelVisble(false, true);
            if(currentAnnot != null)
            {
                AnnotParam annotParam = ParamConverter.CPDFDataConverterToAnnotParam(
                    PdfViewControl.GetCPDFViewer().GetDocument(), currentAnnot.Page.PageIndex, currentAnnot);
                SettingPanel.BindMeasureSetting(GetMeasureInfoFromParam(annotParam));
            }
        }

        private CPDFMeasureInfo GetMeasureInfoFromParam(AnnotParam param)
        {
            if(param is LineMeasureParam lineParam)
            {
                return lineParam.measureInfo;
            }
            if (param is PolyLineMeasureParam polyLineParam)
            {
                return polyLineParam.measureInfo;
            }
            if (param is PolygonMeasureParam polygonParam)
            {
                return polygonParam.measureInfo;
            }
            return null;
        }

        private void SettingPanel_CancelEvent(object sender, EventArgs e)
        {
            SetInfoPanelVisble(SettingPanel.ReturnToInfoPanel, false);
        }

        private void SettingPanel_DoneEvent(object sender, EventArgs e)
        {
            SetInfoPanelVisble(SettingPanel.ReturnToInfoPanel, false);
            SettingPanel.SaveMeasureSetting(currentAnnot);
            InfoPanel.SetMeasureInfo(currentAnnot);
        }

        public void SetInfoPanelVisble(bool measureInfo, bool measureSetting)
        {
            InfoPanel.Visibility = measureInfo ? Visibility.Visible : Visibility.Collapsed;
            SettingPanel.Visibility = measureSetting ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetMeasureInfoType(CPDFMeasureType measureType)
        {
            InfoPanel?.SetMeasureType(measureType);
        }
        
        public void SetBOTAContainer(CPDFBOTABarControl botaControl)
        {
            this.BotaContainer.Child = botaControl;
        }

        public void SetMeasureScale(CPDFMeasureType measureType, string scale)
        {
            InfoPanel?.SetMeasureScale(measureType,scale);
        }
    }
}

