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
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace Compdfkit_Tools.Edit
{
    public partial class PDFImageEditControl : UserControl
    {
        public CPDFViewer PDFView { get; private set; }
        public PDFEditEvent EditEvent { get; set; }
        public List<PDFEditEvent> EditMultiEvents {  get; set; }

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
        
        public void SetRotationText(float rotation)
        {
            RotationTxb.Text = rotation.ToString(CultureInfo.CurrentCulture);
        }

        public void SetPDFImageEditData(PDFEditEvent newEvent)
        {
            EditEvent = null;
            EditMultiEvents = null;
            if(ImageThumbBorder!= null)
            {
                ImageThumbBorder.Visibility = Visibility.Visible;
            }

            if (ImageReplaceBtn != null)
            {
                ImageReplaceBtn.Visibility = Visibility.Visible;
            }

            if (ImageClipBtn != null)
            {
                ImageClipBtn.Visibility = Visibility.Visible;
            }

            if (newEvent != null && newEvent.EditType == CPDFEditType.EditImage)
            {
                SetImageTransparency(newEvent.Transparency);
            }
            
            if(RotationTxb!=null && newEvent!=null && newEvent.EditType == CPDFEditType.EditImage)
            {
                RotationTxb.Text = newEvent.CurrentRotated.ToString(CultureInfo.CurrentCulture);
            }
            
            EditEvent = newEvent;
            SetImageThumb();
        }

        public void SetPDFImageMultiEditData(List<PDFEditEvent> editEvents)
        {
            EditEvent = null;
            EditMultiEvents = null;

            if (ImageThumbBorder != null)
            {
                ImageThumbBorder.Visibility = Visibility.Collapsed;
            }

            if(ImageReplaceBtn != null)
            {
                ImageReplaceBtn.Visibility = Visibility.Collapsed;
            }

            if(ImageClipBtn!=null)
            {
                ImageClipBtn.Visibility = Visibility.Collapsed;
            }

            if (ImageThumbUI != null)   
            {
                ImageThumbUI.Source = null;
            }
            
            if(RotationTxb!=null)
            {
                RotationTxb.Text = "";
            }

           
            if (editEvents != null && editEvents.Count > 0)
            {
                PDFEditEvent editEvent = editEvents[0];
                if (editEvent != null && editEvent.EditType == CPDFEditType.EditImage)
                {
                    SetImageTransparency(editEvent.Transparency);
                }
            }

            EditMultiEvents = editEvents;
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

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    if (e)
                    {
                        editEvent.VerticalMirror = true;
                    }
                    else
                    {
                        editEvent.HorizontalMirror = true;
                    }
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void RotateUI_RotationChanged(object sender, double e)
        {
            if (EditEvent != null)
            {
                EditEvent.Rotate = (int)e;
                EditEvent.UpdatePDFEditByEventArgs();
                SetRotationText(EditEvent.CurrentRotated);
                SetImageThumb();
            }

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.Rotate = (int)e;
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
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

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.Transparency = (int)(ImasgeOpacitySlider.Value * 255);
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
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

            if (EditMultiEvents != null)
            {
                foreach (PDFEditEvent editEvent in EditMultiEvents)
                {
                    editEvent.Transparency = (int)(ImasgeOpacitySlider.Value * 255);
                }
                PDFEditEvent.UpdatePDFEditList(EditMultiEvents);
            }
        }

        private void ImageReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            if(EditMultiEvents!=null && EditMultiEvents.Count>1)
            {
                return;
            }

            if (EditEvent != null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp;";
                if (openFileDialog.ShowDialog() == true)
                {
                    EditEvent.ReplaceImagePath = openFileDialog.FileName;
                    EditEvent.UpdatePDFEditByEventArgs();
                    // EditEvent = null;
                    SetImageThumb();
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
            if(EditMultiEvents!=null && EditMultiEvents.Count>1)
            {
                return;
            }

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
            OpacityTextBox.Text = string.Format("{0}%", (int)(Math.Round(ImasgeOpacitySlider.Value * 100)));
        }
        
        private void SetAbsRotation(double absRotation, PDFEditEvent editEvent)
        {
            absRotation -= editEvent.CurrentRotated;
            if (absRotation != 0)
            {
                editEvent.Rotate = (int)absRotation;
                editEvent.UpdatePDFEditByEventArgs();
                SetImageThumb();
            }
        }
        
        private void SetAbsRotation(double absRotation, List<PDFEditEvent> editEvents)
        {
            foreach (var editEvent in editEvents)
            {
                SetAbsRotation(absRotation, editEvent);
            }
        }
        
        private void RotationTxb_LostFocus(object sender, RoutedEventArgs e)
        {
            if(!double.TryParse(RotationTxb.Text, out double rotation))
            {
                return;
            }
            if(EditEvent!=null)
            {
                SetAbsRotation(rotation, EditEvent);
            }
            else if(EditMultiEvents != null && EditMultiEvents.Count>0)
            {
                SetAbsRotation(rotation, EditMultiEvents);
            }
        }

        private void RotationTxb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RotationTxb_LostFocus(null, null);
            }
        }
    }
}
