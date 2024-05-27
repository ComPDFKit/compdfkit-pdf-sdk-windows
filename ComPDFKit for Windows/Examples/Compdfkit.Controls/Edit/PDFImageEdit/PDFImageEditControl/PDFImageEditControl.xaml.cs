using ComPDFKit.Import;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool;
using ComPDFKit.Tool.UndoManger;
using ComPDFKit.Viewer.Helper;
using ComPDFKitViewer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Helper;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Controls.Edit
{
    public partial class PDFImageEditControl : UserControl
    {
        #region property
        public CPDFViewerTool ToolView { get; set; }
        public ImageEditParam EditEvent { get; set; }

        //public List<PDFEditEvent> EditMultiEvents { get; set; }
        #endregion 

        public PDFImageEditControl()
        {
            InitializeComponent();
            Loaded += PDFImageEditControl_Loaded;
            Unloaded += PDFImageEditControl_Unloaded;
        }

        #region Load unload custom control

        private void PDFImageEditControl_Loaded(object sender, RoutedEventArgs e)
        {
            RotateUI.RotationChanged -= RotateUI_RotationChanged;
            FlipUI.FlipChanged -= FlipUI_FlipChanged;
            RotateUI.RotationChanged += RotateUI_RotationChanged;
            FlipUI.FlipChanged += FlipUI_FlipChanged;
            ToolView.SelectedDataChanged -= ToolView_SelectedDataChanged;
            ToolView.SelectedDataChanged += ToolView_SelectedDataChanged;
        }

        private void PDFImageEditControl_Unloaded(object sender, RoutedEventArgs e)
        {
            RotateUI.RotationChanged -= RotateUI_RotationChanged;
            FlipUI.FlipChanged -= FlipUI_FlipChanged;
        } 

        #endregion

        #region Property changed
        private void ToolView_SelectedDataChanged(object sender, SelectedAnnotData e)
        {
            if (ToolView.GetIsCropMode())
            {
                GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
                SelectedRect selectedRect = ToolView.GetSelectedRectForEditAreaObject(imageArea);
                if (selectedRect != null)
                {
                    Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
                    double currentZoom = ToolView.GetCPDFViewer().CurrentRenderFrame.ZoomFactor;
                    Rect rect = selectedRect.GetRect();
                    Rect maxRect = selectedRect.GetMaxRect();

                    Rect pdfRect = new Rect((rect.X - maxRect.X) / currentZoom, (rect.Y - maxRect.Y) / currentZoom, rect.Width / currentZoom, rect.Height / currentZoom);
                    pdfRect = DpiHelper.StandardRectToPDFRect(pdfRect);
                    CRect newCRect = new CRect((float)pdfRect.Left, (float)pdfRect.Bottom, (float)pdfRect.Right, (float)pdfRect.Top);
                    imageArea.CutWithRect(newCRect);

                    SetImageThumb();
                    ToolView.UpdateRender(oldRect, imageArea);
                    editPage.EndEdit();
                }
            }
        }


        #endregion

        #region Init PDFViewer
        public void InitWithPDFViewer(CPDFViewerTool newPDFView)
        {
            ToolView = newPDFView;
        }
        #endregion

        #region public method

        public void SetRotationText(float rotation)
        {
            RotationTxb.Text = rotation.ToString(CultureInfo.CurrentCulture);
        }

        #endregion

        #region Image Edit

        public void SetImageThumb()
        {
            if (EditEvent != null)
            {
                try
                {
                    GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);

                    string path = Path.GetTempPath();
                    string uuid = Guid.NewGuid().ToString("N");
                    string imagePath = Path.Combine(path, uuid + ".tmp");
                    imageArea.ExtractImage(imagePath);

                    Bitmap bitmapImage = new Bitmap(imagePath);
                    MemoryStream memoryStream = new MemoryStream();

                    bitmapImage.Save(memoryStream, bitmapImage.RawFormat);
                    BitmapImage imageShow = new BitmapImage();
                    imageShow.BeginInit();
                    imageShow.StreamSource = memoryStream;
                    imageShow.EndInit();
                    ImageThumbUI.Source = imageShow;
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void SetImageTransparency(double transparency)
        {
            ImasgeOpacitySlider.Value = transparency / 255D;
            OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Round(ImasgeOpacitySlider.Value * 100)));
        }

        public void SetPDFImageEditData(ImageEditParam newEvent)
        {
            if (newEvent.EditIndex < 0)
            {
                EditEvent = null;
            }
            else
            {
                EditEvent = newEvent;
            }
            if (newEvent != null && newEvent.EditType == CPDFEditType.EditImage)
            {
                SetImageTransparency(newEvent.Transparency);
            }

            if (RotationTxb != null && newEvent != null && newEvent.EditType == CPDFEditType.EditImage)
            {
                GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
                RotationTxb.Text = imageArea?.GetRotation().ToString();
                //RotationTxb.Text = newEvent.Rotate.ToString(CultureInfo.CurrentCulture);
            }

            EditEvent = newEvent;
            SetImageThumb();
        }
        #endregion

        //public void SetPDFImageMultiEditData(List<PDFEditEvent> editEvents)
        //{
        //    EditEvent = null;
        //    EditMultiEvents = null;

        //    if (ImageThumbBorder != null)
        //    {
        //        ImageThumbBorder.Visibility = Visibility.Collapsed;
        //    }

        //    if(ImageReplaceBtn != null)
        //    {
        //        ImageReplaceBtn.Visibility = Visibility.Collapsed;
        //    }

        //    if(ImageClipBtn!=null)
        //    {
        //        ImageClipBtn.Visibility = Visibility.Collapsed;
        //    }

        //    if (ImageThumbUI != null)
        //    {
        //        ImageThumbUI.Source = null;
        //    }


        //    if (editEvents != null && editEvents.Count > 0)
        //    {
        //        PDFEditEvent editEvent = editEvents[0];
        //        if (editEvent != null && editEvent.EditType == CPDFEditType.EditImage)
        //        {
        //            SetImageTransparency(editEvent.Transparency);
        //        }
        //    }

        //    EditMultiEvents = editEvents;
        //}

        #region Property changed

        private void FlipUI_FlipChanged(object sender, bool e)
        {
            GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
                bool result = false;
                if (e)
                {
                    result = imageArea.VerticalMirror();
                }
                else
                {
                    result = imageArea.HorizontalMirror();
                }
                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, imageArea);
                    editPage.EndEdit();
                }

                SetImageThumb();
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        if (e)
            //        {
            //            editEvent.VerticalMirror = true;
            //        }
            //        else
            //        {
            //            editEvent.HorizontalMirror = true;
            //        }
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void RotateUI_RotationChanged(object sender, double e)
        {
            GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
                if (imageArea.Rotate((int)e))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    //SetRotationText(EditEvent.CurrentRotated);
                    SetImageThumb();
                    ToolView.UpdateRender(oldRect, imageArea);
                    editPage.EndEdit();
                    RotationTxb.Text = imageArea.GetRotation().ToString();
                }
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.Rotate = (int)e;
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "false";
            }
        }

        private void SliderOpacity_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                slider.Tag = "true";
            }
            GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
                if (imageArea.SetImageTransparency((byte)(ImasgeOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    SetImageThumb();
                    ToolView.UpdateRender(oldRect, imageArea);
                    editPage.EndEdit();
                }
            }

            //if (EditMultiEvents != null)
            //{
            //    foreach (PDFEditEvent editEvent in EditMultiEvents)
            //    {
            //        editEvent.Transparency = (int)(ImageOpacitySlider.Value * 255);
            //    }
            //    PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            //}
        }

        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if (OpacityTextBox != null)
            {
                OpacityTextBox.Text = string.Format("{0}%", (int)(ImasgeOpacitySlider.Value * 100));
            }

            if (slider != null && slider.Tag != null && slider.Tag.ToString() == "false")
            {
                return;
            }
            GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageArea != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
                if (imageArea.SetImageTransparency((byte)(ImasgeOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    SetImageThumb();
                    ToolView.UpdateRender(oldRect, imageArea);
                    editPage.EndEdit();
                }
            }
        }

        private void ImageReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            //if(EditMultiEvents!=null && EditMultiEvents.Count>1)
            //{
            //    return;
            //}

            if (EditEvent != null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                if (openFileDialog.ShowDialog() == true)
                {
                    GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
                    if (imageArea != null)
                    {
                        int imageWidth = 0;
                        int imageHeight = 0;
                        byte[] imageData = null;
                        PDFHelp.ImagePathToByte(openFileDialog.FileName, ref imageData, ref imageWidth, ref imageHeight);

                        if (imageData != null && imageWidth > 0 && imageHeight > 0)
                        {
                            Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
                            CRect imageRect = imageArea.GetClipRect();
                            if (imageArea.ReplaceImageArea(imageRect, imageData, imageWidth, imageHeight))
                            {
                                PDFEditHistory editHistory = new PDFEditHistory();
                                editHistory.EditPage = editPage;
                                if (pdfPage != null)
                                {
                                    editHistory.PageIndex = pdfPage.PageIndex;
                                }

                                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                                SetImageThumb();
                                ToolView.UpdateRender(oldRect, imageArea);
                                editPage.EndEdit();
                            }
                        }
                    }
                }
            }
        }

        private void ImageExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ToolView != null)
            {
                // Dictionary<int, List<Bitmap>> imageDict = PDFView.GetSelectedImages();
                Dictionary<int, List<Bitmap>> imageDict = new Dictionary<int, List<Bitmap>>();
                if (imageDict != null && imageDict.Count > 0)
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                    if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string choosePath = folderBrowser.SelectedPath;
                        string openPath = choosePath;
                        try
                        {
                            foreach (int pageIndex in imageDict.Keys)
                            {
                                List<Bitmap> imageList = imageDict[pageIndex];
                                foreach (Bitmap image in imageList)
                                {
                                    string savePath = System.IO.Path.Combine(choosePath, Guid.NewGuid() + ".jpg");
                                    image.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    openPath = savePath;
                                }
                            }
                            Process.Start("explorer", "/select,\"" + openPath + "\"");
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        private void ImageClipBtn_Click(object sender, RoutedEventArgs e)
        {
            ToolView.SetCropMode(!ToolView.GetIsCropMode());
        }

        private void OpacityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectItem = OpacityComboBox.SelectedItem as ComboBoxItem;
            if (selectItem != null && selectItem.Content != null)
            {
                if (double.TryParse(selectItem.Content.ToString().TrimEnd('%'), out double newOpacity))
                {
                    OpacityTextBox.Text = selectItem.Content.ToString();
                    ImasgeOpacitySlider.Value = newOpacity / 100.0;
                }
            }
        }

        private void GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage)
        {
            imageArea = null;
            editPage = null;
            pdfPage = null;
            if (ToolView == null || EditEvent == null)
            {
                return;
            }

            try
            {
                CPDFViewer pdfViewer = ToolView.GetCPDFViewer();
                CPDFDocument pdfDoc = pdfViewer.GetDocument();
                pdfPage = pdfDoc.PageAtIndex(EditEvent.PageIndex);
                editPage = pdfPage.GetEditPage();
                List<CPDFEditArea> editAreas = editPage.GetEditAreaList();
                if (editAreas != null && editAreas.Count > EditEvent.EditIndex)
                {
                    imageArea = editAreas[EditEvent.EditIndex] as CPDFEditImageArea;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SetAbsRotation(double absRotation)
        {
            GetImageArea(out CPDFEditImageArea imageArea, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageArea != null)
            {
                int rotation = (int)absRotation - imageArea.GetRotation();
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageArea.GetFrame());
                if (imageArea.Rotate(rotation))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    if (pdfPage != null)
                    {
                        editHistory.PageIndex = pdfPage.PageIndex;
                    }

                    ToolView.GetCPDFViewer()?.UndoManager.AddHistory(editHistory);
                    SetImageThumb();
                    ToolView.UpdateRender(oldRect, imageArea);
                    editPage.EndEdit();
                }
            }
        }

        private void RotationTxb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(RotationTxb.Text, out double rotation))
            {
                return;
            }
            SetAbsRotation(rotation);
        }

        private void RotationTxb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RotationTxb_LostFocus(null, null);
            }
        }
        #endregion 
    }
}
