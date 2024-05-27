using ComPDFKitViewer.PdfViewer;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ComPDFKit.PDFPage;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Compdfkit_Tools.Edit
{
    public partial class PDFImageEditControl : UserControl
    {
        public CPDFViewer PDFView { get; private set; }
        public PDFEditEvent EditEvent { get; set; }

        public PDFImageEditControl()
        {
            InitializeComponent();
            Loaded += PDFImageEditControl_Loaded;
            Unloaded += PDFImageEditControl_Unloaded;
        }

        private void PDFImageEditControl_Unloaded(object sender, RoutedEventArgs e)
        {
            RotateUI.RotationChanged -= RotateUI_RotationChanged;
            FlipUI.FlipChanged -= FlipUI_FlipChanged;
        }

        public void InitWithPDFViewer(CPDFViewer newPDFView)
        {
            PDFView = newPDFView;
        }

        public void SetPDFImageEditData(PDFEditEvent newEvent)
        {
            EditEvent = null;
            if (newEvent != null && newEvent.EditType == CPDFEditType.EditImage)
            {
                SetImageTransparency(newEvent.Transparency);
            }
            EditEvent = newEvent;
            SetImageThumb();
        }

        private void PDFImageEditControl_Loaded(object sender, RoutedEventArgs e)
        {
            RotateUI.RotationChanged += RotateUI_RotationChanged;
            FlipUI.FlipChanged += FlipUI_FlipChanged;
        }

        private void FlipUI_FlipChanged(object sender, bool e)
        {
            if (EditEvent != null)
            {
                if(e)
                {
                    EditEvent.VerticalMirror = true;
                }
                else
                {
                    EditEvent.HorizontalMirror = true;
                }
              
                EditEvent.UpdatePDFEditByEventArgs();
                SetImageThumb();
            }
        }

        private void RotateUI_RotationChanged(object sender, double e)
        {
            if (EditEvent != null)
            {
                EditEvent.Rotate = (int)e;
                EditEvent.UpdatePDFEditByEventArgs();
                SetImageThumb();
            }
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
            if (EditEvent != null)
            {
                EditEvent.Transparency = (int)(ImasgeOpacitySlider.Value * 255);
                EditEvent.UpdatePDFEditByEventArgs();
                SetImageThumb();
            }
        }

        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            if(OpacityTextBox != null)
            {
                OpacityTextBox.Text = string.Format("{0}%", (int)(ImasgeOpacitySlider.Value * 100));
            }
          
            if (slider != null && slider.Tag != null && slider.Tag.ToString() == "false")
            {
                return;
            }

            if (EditEvent != null)
            {
                EditEvent.Transparency = (int)(ImasgeOpacitySlider.Value * 255);
                EditEvent.UpdatePDFEditByEventArgs();
                SetImageThumb();
            }
        }

        private void ImageReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (EditEvent != null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                if (openFileDialog.ShowDialog() == true)
                {
                    EditEvent.ReplaceImagePath = openFileDialog.FileName;
                    EditEvent.UpdatePDFEditByEventArgs();
                    EditEvent = null;
                }
            }
        }

        private void ImageExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PDFView != null)
            {
                Dictionary<int, List<Bitmap>> imageDict = PDFView.GetSelectedImages();
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
            if (EditEvent != null)
            {
                EditEvent.ClipImage = true;
                EditEvent.UpdatePDFEditByEventArgs();
            }
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

        public void SetImageThumb()
        {
            if (PDFView != null && EditEvent!=null)
            {
                try
                {
                    Dictionary<int, List<Bitmap>> imageDict = PDFView.GetSelectedImages();
                    foreach (int pageIndex in imageDict.Keys)
                    {
                        List<Bitmap> imageList = imageDict[pageIndex];
                        if (imageList.Count > 0)
                        {
                            Bitmap bitmapImage = imageList[0];
                            MemoryStream memoryStream = new MemoryStream();

                            bitmapImage.Save(memoryStream, bitmapImage.RawFormat);
                            BitmapImage imageShow = new BitmapImage();
                            imageShow.BeginInit();
                            imageShow.StreamSource = memoryStream;
                            imageShow.EndInit();
                            ImageThumbUI.Source = imageShow;
                            break;
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        public void SetImageTransparency(double transparency)
        {
            ImasgeOpacitySlider.Value = transparency / 255D;
            OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Ceiling(ImasgeOpacitySlider.Value * 100)));
        }
    }
}
