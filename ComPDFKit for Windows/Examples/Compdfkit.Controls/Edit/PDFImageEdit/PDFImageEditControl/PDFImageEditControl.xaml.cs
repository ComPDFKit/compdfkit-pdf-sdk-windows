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
using System.Windows.Media.Imaging;
using ComPDFKit.Tool.DrawTool;
using ComPDFKitViewer.Helper;
using ComPDFKit.Tool.Help;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ComPDFKit.Controls.Edit
{
    public partial class PDFImageEditControl : UserControl, INotifyPropertyChanged
    {
        #region property
        public CPDFViewerTool ToolView { get; set; }
        public List<ImageEditParam> EditEvents { get; set; } = new List<ImageEditParam>();

        private Visibility _onlySingleVisible = Visibility.Collapsed;
        public Visibility OnlySingleVisible
        {
            get => _onlySingleVisible;
            set => UpdateProper(ref  _onlySingleVisible, value);
        }

        //public List<PDFEditEvent> EditMultiEvents { get; set; }
        #endregion 

        public PDFImageEditControl()
        {
            DataContext = this;
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
                GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
                if (imageAreas.Count == 0 || pdfPage == null || editPage == null)
                    return;

                SelectedRect selectedRect = ToolView.GetSelectedRectForEditAreaObject(imageAreas[0]);
                if (selectedRect == null)
                    return;

                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageAreas[0].GetFrame());
                double currentZoom = ToolView.GetCPDFViewer().CurrentRenderFrame.ZoomFactor;
                Rect rect = selectedRect.GetRect();
                Rect maxRect = selectedRect.GetMaxRect();
                Rect pdfRect = new Rect((rect.X - maxRect.X) / currentZoom, (rect.Y - maxRect.Y) / currentZoom, rect.Width / currentZoom, rect.Height / currentZoom);
                pdfRect = DpiHelper.StandardRectToPDFRect(pdfRect);
                CRect newCRect = new CRect((float)pdfRect.Left, (float)pdfRect.Bottom, (float)pdfRect.Right, (float)pdfRect.Top);
                if(imageAreas[0].CutWithRect(newCRect))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;

                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, imageAreas[0]);
                 }

                editPage.EndEdit();
                SetImageThumb();
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
            if (EditEvents.Count == 1)
            {
                try
                {
                    GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);

                    string path = Path.GetTempPath();
                    string uuid = Guid.NewGuid().ToString("N");
                    string imagePath = Path.Combine(path, uuid + ".tmp");
                    imageAreas.FirstOrDefault().ExtractImage(imagePath);

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

        public void SetPDFImageEditData(List<ImageEditParam> newEvents)
        {

            EditEvents = newEvents.Where(newEvent => newEvent.EditIndex >= 0 && newEvent.EditType == CPDFEditType.EditImage).ToList();

            if (EditEvents.Count > 0)
            {
                SetImageTransparency(EditEvents.FirstOrDefault().Transparency);
            }

            if (RotationTxb != null && EditEvents.Count > 0)
            {
                GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
                RotationTxb.Text = imageAreas?.FirstOrDefault()?.GetRotation().ToString();
            }

            if (EditEvents.Count == 1)
            {
                OnlySingleVisible = Visibility.Visible;
                SetImageThumb();
            }
            else
            {
                OnlySingleVisible = Visibility.Collapsed;
            }
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
            GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if(ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageAreas[0].GetFrame());
                bool result;
                if (e)
                {
                    result = imageAreas[0].VerticalMirror();
                }
                else
                {
                    result = imageAreas[0].HorizontalMirror();
                }

                if (result)
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, imageAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditImageArea imageArea in imageAreas)
                {
                    bool result;
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
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (imageAreas.Count == 1)
                SetImageThumb();

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
            GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if(ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageAreas[0].GetFrame());
                if (imageAreas[0].Rotate((int)e))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, imageAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditImageArea imageArea in imageAreas)
                {
                    if (imageArea.Rotate((int)e))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            RotationTxb.Text = imageAreas.FirstOrDefault().GetRotation().ToString();
            if (imageAreas.Count == 1)
                SetImageThumb();

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

            GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageAreas[0].GetFrame());
                if (imageAreas[0].SetImageTransparency((byte)(ImasgeOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, imageAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditImageArea imageArea in imageAreas)
                {
                    if (imageArea.SetImageTransparency((byte)(ImasgeOpacitySlider.Value * 255)))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (imageAreas.Count == 1)
                SetImageThumb();
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

            GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageAreas[0].GetFrame());
                if (imageAreas[0].SetImageTransparency((byte)(ImasgeOpacitySlider.Value * 255)))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, imageAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditImageArea imageArea in imageAreas)
                {
                    if (imageArea.SetImageTransparency((byte)(ImasgeOpacitySlider.Value * 255)))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if (imageAreas.Count == 1)
                SetImageThumb();
        }

        private void ImageReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            //if(EditMultiEvents!=null && EditMultiEvents.Count>1)
            //{
            //    return;
            //}

            if (EditEvents.Count > 0)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                if (openFileDialog.ShowDialog() == true)
                {
                    GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
                    if (imageAreas.Count == 0 || pdfPage == null || editPage == null)
                        return;

                    int imageWidth = 0;
                    int imageHeight = 0;
                    byte[] imageData = null;
                    PDFHelp.ImagePathToByte(openFileDialog.FileName, ref imageData, ref imageWidth, ref imageHeight);
                    if (imageData != null && imageWidth > 0 && imageHeight > 0)
                    {
                        Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageAreas[0].GetFrame());
                        CRect imageRect = imageAreas[0].GetClipRect();
                        if (imageAreas[0].ReplaceImageArea(imageRect, imageData, imageWidth, imageHeight))
                        {
                            PDFEditHistory editHistory = new PDFEditHistory();
                            editHistory.EditPage = editPage;
                            editHistory.PageIndex = pdfPage.PageIndex;
                            ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                            ToolView.UpdateRender(oldRect, imageAreas[0]);
                        }

                        editPage.EndEdit();
                        SetImageThumb();
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

        private void GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage)
        {
            imageAreas = new List<CPDFEditImageArea>();
            editPage = null;
            pdfPage = null;
            if (ToolView == null || EditEvents.Count == 0)
            {
                return;
            }
            try
            {
                foreach (var EditEvent in EditEvents)
                {
                    CPDFViewer pdfViewer = ToolView.GetCPDFViewer();
                    CPDFDocument pdfDoc = pdfViewer.GetDocument();
                    pdfPage = pdfDoc.PageAtIndex(EditEvent.PageIndex);
                    editPage = pdfPage.GetEditPage();
                    List<CPDFEditArea> editAreas = editPage.GetEditAreaList();
                    if (editAreas != null && editAreas.Count > EditEvent.EditIndex)
                    {
                        imageAreas.Add(editAreas[EditEvent.EditIndex] as CPDFEditImageArea);
                    }
                } 
            }
            catch (Exception ex)
            {

            }
        }

        private void SetAbsRotation(double absRotation)
        {
            GetImageArea(out List<CPDFEditImageArea> imageAreas, out CPDFPage pdfPage, out CPDFEditPage editPage);
            if (imageAreas.Count == 0 || pdfPage == null || editPage == null)
                return;

            if (ToolView.CurrentEditAreaObject() != null)
            {
                Rect oldRect = DataConversionForWPF.CRectConversionForRect(imageAreas[0].GetFrame());
                int rotation = (int)absRotation - imageAreas[0].GetRotation();
                if (imageAreas[0].Rotate(rotation))
                {
                    PDFEditHistory editHistory = new PDFEditHistory();
                    editHistory.EditPage = editPage;
                    editHistory.PageIndex = pdfPage.PageIndex;
                    ToolView.GetCPDFViewer().UndoManager.AddHistory(editHistory);
                    ToolView.UpdateRender(oldRect, imageAreas[0]);
                }
            }
            else
            {
                GroupHistory groupHistory = new GroupHistory();
                foreach (CPDFEditImageArea imageArea in imageAreas)
                {
                    int rotation = (int)absRotation - imageArea.GetRotation();
                    if (imageArea.Rotate(rotation))
                    {
                        PDFEditHistory editHistory = new PDFEditHistory();
                        editHistory.EditPage = editPage;
                        editHistory.PageIndex = pdfPage.PageIndex;
                        groupHistory.Histories.Add(editHistory);
                    }
                }

                ToolView.GetCPDFViewer()?.UndoManager.AddHistory(groupHistory);
                ToolView.GetCPDFViewer()?.UpdateRenderFrame();
            }

            editPage.EndEdit();
            if(imageAreas.Count == 1)
                SetImageThumb();
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool UpdateProper<T>(ref T properValue,
                                           T newValue,
                                           [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return false;

            properValue = newValue;
            OnPropertyChanged(properName);
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
