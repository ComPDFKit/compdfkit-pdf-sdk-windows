using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.PDFPage;
using ComPDFKit.Tool;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using UserControl = System.Windows.Controls.UserControl;
using System.Collections.Generic;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Comparison;
using ComPDFKit.Tool.DrawTool;

namespace ComPDFKit.Controls.PDFView
{
    public partial class ComparisonControl : UserControl, INotifyPropertyChanged
    {
        public PDFViewControl PdfViewControl;
        public CPDFAnnotationControl PDFAnnotationControl = new CPDFAnnotationControl();
        private CPDFDisplaySettingsControl displaySettingsControl = null;
        private PanelState panelState = PanelState.GetInstance();
        private double[] zoomLevelList = { 1f, 8f, 12f, 25, 33f, 50, 66f, 75, 100, 125, 150, 200, 300, 400, 600, 800, 1000 };
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<bool> OnCanSaveChanged;
        public event EventHandler<UIElement> OnCompareStatusChanged;
        

        private bool CanSave
        {
            get
            {
                if (PdfViewControl != null && PdfViewControl.PDFViewTool.GetCPDFViewer() != null)
                {
                    if (PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanRedo ||
                        PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.CanUndo)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public ComparisonControl()
        {
            InitializeComponent();
            panelState.PropertyChanged -= PanelState_PropertyChanged;
            panelState.PropertyChanged += PanelState_PropertyChanged;
        }

        public void SetPropertyContainer(UIElement uiElement)
        {
            PropertyContainer.Child = uiElement;
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
                    ExpandRightPropertyPanel(PDFAnnotationControl, Visibility.Visible);
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



        public void ExpandRightPropertyPanel(UIElement propertytPanel, Visibility visible)
        {
            PropertyContainer.Width = 260;
            PropertyContainer.Child = propertytPanel;
            PropertyContainer.Visibility = visible;
        }

        #region Init PDFViewer

        private void InitialControl()
        {
            PDFGrid.Child = PdfViewControl;
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged -= UndoManager_PropertyChanged;
            PdfViewControl.PDFViewTool.GetCPDFViewer().UndoManager.PropertyChanged += UndoManager_PropertyChanged;
        }

        private void PdfViewControl_MouseRightButtonDownHandler(object sender, ComPDFKit.Tool.MouseEventObject e)
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
                    break;
                case MouseHitTestType.Text:
                    CreateSelectTextContextMenu(sender, ref ContextMenu);
                    break;
                case MouseHitTestType.ImageSelect:
                    CreateSelectImageContextMenu(sender, ref ContextMenu);
                    break;
                default:
                    PdfViewControl.CreateViewerMenu(sender, ref ContextMenu);
                    break;
            }
            PdfViewControl.SetRightMenu(ContextMenu);
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

        private void CreateSelectTextContextMenu(object sender, ref ContextMenu menu)
        {
            menu.Items.Add(new MenuItem() { Header = LanguageHelper.CommonManager.GetString("Menu_Copy"), Command = ApplicationCommands.Copy, CommandTarget = (UIElement)sender });
        }

        public void InitWithPDFViewer(PDFViewControl pdfViewer)
        {
            PdfViewControl = pdfViewer;
            PDFGrid.Child = PdfViewControl;
            FloatPageTool.InitWithPDFViewer(pdfViewer);
            InitialControl();
            DataContext = this;
            if (PdfViewControl != null && PdfViewControl.PDFViewTool.GetCPDFViewer() != null)
            {
                PdfViewControl.MouseRightButtonDownHandler -= PdfViewControl_MouseRightButtonDownHandler;
                PdfViewControl.MouseRightButtonDownHandler += PdfViewControl_MouseRightButtonDownHandler;
                
                ComparisonBarControl.ComparisonActionChanged -= ComparisonBarControl_ComparisonActionChanged;
                ComparisonBarControl.ComparisonActionChanged += ComparisonBarControl_ComparisonActionChanged;
            }
        }

        private void ComparisonBarControl_ComparisonActionChanged(object sender, CPDFComparisonBarControl.ComparisonAction e)
        {
            ComparisonSettingDialog dialog = new ComparisonSettingDialog(PdfViewControl);
            if (e == CPDFComparisonBarControl.ComparisonAction.ContentComparison)
            {
                dialog.SetCompareType(CompareType.ContentCompare);
            }
            else
            {
                dialog.SetCompareType(CompareType.OverwriteCompare);
            }
            dialog.Owner = Window.GetWindow((DependencyObject)sender);
            dialog.OnCompareStatusChanged += (o, element) => OnCompareStatusChanged?.Invoke(this, element);
            dialog.OpenOldFile(PdfViewControl.GetCPDFViewer().GetDocument());
            dialog.ShowDialog();
        }


        public void SetBOTAContainer(CPDFBOTABarControl botaControl)
        {
            this.BotaContainer.Child = botaControl;
        }

        public void SetDisplaySettingsControl(CPDFDisplaySettingsControl displaySettingsControl)
        {
            this.displaySettingsControl = displaySettingsControl;
        }

        #endregion

        public void ClearViewerControl()
        {
            PDFGrid.Child = null;
            BotaContainer.Child = null;
            PropertyContainer.Child = null;
            SignatureStatusBorder.Child = null;
            displaySettingsControl = null;
        }

        #region PropertyChanged

        /// <summary>
        /// Undo Redo Event Noitfy
        /// </summary>
        private void UndoManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName == "CanSave")
            {
                OnCanSaveChanged?.Invoke(this, CanSave);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Context Menu

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
                string savePath = Path.Combine(folderDialog.SelectedPath, Guid.NewGuid() + ".jpg");
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
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
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
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
        #endregion

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PdfViewControl.MouseRightButtonDownHandler -= PdfViewControl_MouseRightButtonDownHandler;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}