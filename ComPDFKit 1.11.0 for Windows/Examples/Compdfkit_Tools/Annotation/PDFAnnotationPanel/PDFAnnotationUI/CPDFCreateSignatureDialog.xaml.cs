using Compdfkit_Tools.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using ComPDFKitViewer.AnnotEvent;
using System.Windows.Ink;
using Compdfkit_Tools.Common;

namespace Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI
{
    public partial class CPDFCreateSignatureDialog : Window
    {
        SolidColorBrush solidColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
        public CPDFSignatureData cPDFSignatureData;
        private string SaveToPath;
        private string SignaturePath;
        private string DrawingSaveToPath;
        private double StrokeWidth = 3;
        private double StrokeHigh = 3;
        private bool IsPageLoaded = false;
        public CPDFCreateSignatureDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SignaturePath = System.IO.Path.Combine(Environment.CurrentDirectory, "ComPDFKit");
            SignaturePath = System.IO.Path.Combine(SignaturePath, "Signature");
            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(SignaturePath);
            IsPageLoaded = true;
            DrawinkCanvas.DefaultDrawingAttributes.Width = StrokeWidth;
            DrawinkCanvas.DefaultDrawingAttributes.Height = StrokeHigh;

            DrawinkCanvas.DefaultDrawingAttributes.Color = solidColorBrush.Color;
            InPutTextBox.Foreground = solidColorBrush;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            switch (CreateSignatureControl.SelectedIndex)
            {
                case 0:
                    SaveDrawSignature();
                    break;
                case 1:
                    SaveTextSignature();
                    break;
                case 2:
                    SaveImageSignature();
                    break;
                default:
                    break;
            }
            this.Close();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerControl.SetIsChecked(1);
            TextColorPickerControl.SetIsChecked(1);

            DrawinkCanvas.Strokes.Clear();
            InPutTextBox.Text = "";
            ImageImage.Source = null;
            AddImageBackground.Visibility = Visibility.Visible;
            SaveBtn.IsEnabled = false;

            DrawinkCanvas.DefaultDrawingAttributes.Color = solidColorBrush.Color;
            InPutTextBox.Foreground = solidColorBrush;

            if (CreateSignatureControl.SelectedIndex==1)
            {
                InPutTextBox.Focus();
            }
        }
        private void UpDataToStrokesObject()
        {
            foreach (var item in DrawinkCanvas.Strokes)
            {
                item.DrawingAttributes = DrawinkCanvas.DefaultDrawingAttributes.Clone();
            }
        }

        private void SaveToImage(string FilePath)
        {
            string path = SignaturePath;
            string name = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(FilePath));
                    double scale = Math.Min((double)600 / image.PixelWidth, (double)600 / image.PixelHeight);
                    scale = Math.Min(scale, 1);
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    var targetBitmap = new TransformedBitmap(image, new ScaleTransform(scale, scale));
                    encoder.Frames.Add(BitmapFrame.Create(targetBitmap));
                    path = System.IO.Path.Combine(path, name);
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                    if (!string.IsNullOrEmpty(SaveToPath))
                    {
                        DirectoryInfo CreatedFilePathFolder = new DirectoryInfo(SaveToPath);
                        if (CreatedFilePathFolder.Exists)
                        {
                            Directory.Delete(SaveToPath, true);
                        }
                    }
                    SaveToPath = path;

                    AddImageBackground.Visibility = Visibility.Collapsed;
                    ImageImage.Source = targetBitmap;
                    SaveBtn.IsEnabled = true;
                }
                catch
                {

                }
            }
            else
            {
                SaveToPath = "";
            }
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "All Image Files(*.bmp;*.gif;*.jpeg;*.jpg;*.png;*.tiff)|*.bmp;*.gif;*.jpeg;*.jpg;*.png;*.tiff|(*.bmp)|*.bmp|" +
                "(*.gif)|*.gif|" +
                "(*.jpeg)|*.jpeg|" +
                "(*.jpg)|*.jpg|" +
                "(*.png)|*.png|" +
                "(*.tiff)|*.tiff";
            if (openFile.ShowDialog() == false)
            {
                return;
            }
            SaveToImage(openFile.FileName);
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsPageLoaded)
            {
                DrawinkCanvas.DefaultDrawingAttributes.Color = ((SolidColorBrush)ColorPickerControl.Brush).Color;
                UpDataToStrokesObject();
            }
        }

        private void TextColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsPageLoaded)
            {
                InPutTextBox.Foreground = TextColorPickerControl.Brush;

            }
        }
        private void SaveTextSignature()
        {
            if (string.IsNullOrEmpty(InPutTextBox.Text))
            {
                return;
            }
            System.Windows.Media.Brush fontcolor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#252629"));
            fontcolor = TextColorPickerControl.Brush;
            Bitmap bmp = TextToBitmap(InPutTextBox.Text, TextName.SelectionBoxItem.ToString(), 50, System.Drawing.Rectangle.Empty, fontcolor, System.Drawing.Color.Transparent);
            string guid = Guid.NewGuid().ToString();
            string path = System.IO.Path.Combine(SignaturePath, guid);
            bmp.Save(path, ImageFormat.Png);
            SaveToPath = path;

            cPDFSignatureData = new CPDFSignatureData();
            cPDFSignatureData.SourcePath = SaveToPath;
            cPDFSignatureData.AnnotationType = CPDFAnnotationType.Signature;
            cPDFSignatureData.Type = SignatureType.TextType;
        }

        private void SaveImageSignature()
        {
            cPDFSignatureData = new CPDFSignatureData();
            cPDFSignatureData.SourcePath = SaveToPath;
            cPDFSignatureData.AnnotationType = CPDFAnnotationType.Signature;
            cPDFSignatureData.Type = SignatureType.ImageType;
        }
        private void SaveDrawSignature()
        {
            var FreeHandpath = SignaturePath;
            string name = Guid.NewGuid().ToString();
            FreeHandpath = System.IO.Path.Combine(FreeHandpath, name);
            using (System.IO.FileStream stream = new System.IO.FileStream(FreeHandpath, System.IO.FileMode.Create))
            {
                DrawinkCanvas.Strokes.Save(stream);
            }

            StampAnnotArgs stampArgs = new StampAnnotArgs();

            List<List<System.Windows.Point>> RawPointList = new List<List<System.Windows.Point>>();
            for (int kk = 0; kk < DrawinkCanvas.Strokes.Count; kk++)
            {
                List<System.Windows.Point> p = new List<System.Windows.Point>();
                RawPointList.Add(p);
                for (int gg = 0; gg < DrawinkCanvas.Strokes[kk].StylusPoints.Count; gg++)
                {
                    var point = DrawinkCanvas.Strokes[kk].StylusPoints[gg].ToPoint();

                    if (point.X >= 0 && point.Y >= 0)
                        RawPointList[kk].Add(point);

                }
            }
            DrawingSaveToPath = FreeHandpath;

            double inkThickness;
            inkThickness = StrokeWidth;
            stampArgs.SetInkData(RawPointList, inkThickness, DrawinkCanvas.DefaultDrawingAttributes.Color);
            var writeStamp = stampArgs.GetStampDrawing();

            FreeHandpath = SignaturePath;
            string thumbnailName = Guid.NewGuid().ToString();
            FreeHandpath = System.IO.Path.Combine(FreeHandpath, thumbnailName);
            using (FileStream stream5 = new FileStream(FreeHandpath, FileMode.Create))
            {
                PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                encoder5.Frames.Add(BitmapFrame.Create(writeStamp));
                encoder5.Save(stream5);
            }
            SaveToPath = FreeHandpath;

            cPDFSignatureData = new CPDFSignatureData();
            cPDFSignatureData.AnnotationType = CPDFAnnotationType.Signature;
            cPDFSignatureData.SourcePath = SaveToPath;
            cPDFSignatureData.DrawingPath = DrawingSaveToPath;
            cPDFSignatureData.Type = SignatureType.Drawing;
            cPDFSignatureData.inkThickness = inkThickness;
            cPDFSignatureData.inkColor = DrawinkCanvas.DefaultDrawingAttributes.Color;
        }

        private Bitmap TextToBitmap(string text, string FontFamily, double size, System.Drawing.Rectangle rect, System.Windows.Media.Brush fontcolor, System.Drawing.Color backColor)
        {
            FormattedText formatText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new System.Windows.Media.FontFamily(FontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            size,
            fontcolor
            );

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext dc = drawingVisual.RenderOpen();
            dc.DrawText(formatText, new System.Windows.Point(2, 10));
            dc.Close();


            Rect x = drawingVisual.ContentBounds;
            Rect DrawRect = new Rect(0, 0, x.Width + (x.X / 2), x.Height + x.Y);

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)((int)DrawRect.Width + (x.X / 2)), (int)((int)DrawRect.Height + x.Y), 96F, 96F, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(drawingVisual);

            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            encoder.Save(stream);

            Bitmap bitmap = new Bitmap(stream);

            return bitmap;
        }

        private void TextName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TextName.SelectedIndex != -1 && IsPageLoaded)
            {
                InPutTextBox.FontFamily = new System.Windows.Media.FontFamily(TextName.SelectionBoxItem.ToString());
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string f in file)
            {
                string FileType = System.IO.Path.GetExtension(f).Trim().ToLower();
                string type = "*.bmp;*.gif;*.jpeg;*.jpg;*.png;*.tiff)|*.bmp;*.gif;*.jpeg;*.jpg;*.png;*.tiff";
                if (!string.IsNullOrEmpty(FileType))
                {
                    string imagetype = "*" + FileType;
                    string[] x = type.ToLower().Split(';');
                    List<string> list = x.ToList();
                    int imageindex = list.IndexOf(imagetype);
                    if (imageindex > 0)
                    {
                        SaveToImage(f);
                    }
                }
            }
        }

        private void CreateSignatureControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (IsPageLoaded && tabControl != null)
            {
                switch (tabControl.SelectedIndex)
                {
                    case 0:
                        if (DrawinkCanvas.Strokes.Count <= 0)
                        {
                            SaveBtn.IsEnabled = false;
                        }
                        else
                        {
                            SaveBtn.IsEnabled = true;
                        }
                        break;
                    case 1:
                        if (string.IsNullOrEmpty(InPutTextBox.Text))
                        {
                            SaveBtn.IsEnabled = false;
                        }
                        else
                        {
                            SaveBtn.IsEnabled = true;
                        }
                        break;
                    case 2:
                        if (ImageImage.Source == null)
                        {
                            SaveBtn.IsEnabled = false;
                        }
                        else
                        {
                            SaveBtn.IsEnabled = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void InPutTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsPageLoaded)
            {
                if (string.IsNullOrEmpty((sender as TextBox).Text))
                {
                    SaveBtn.IsEnabled = false;
                }
                else
                {
                    SaveBtn.IsEnabled = true;
                }
            }
        }

        private void ImageImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (IsPageLoaded)
            {
                if (ImageImage.Source == null)
                {
                    SaveBtn.IsEnabled = false;
                }
                else
                {
                    SaveBtn.IsEnabled = true;
                }
            }
        }

        private void DrawinkCanvas_Unloaded(object sender, RoutedEventArgs e)
        {

            DrawinkCanvas.Strokes.StrokesChanged -= Strokes_StrokesChanged;
        }

        private void DrawinkCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DrawinkCanvas.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (IsPageLoaded)
            {
                if (DrawinkCanvas.Strokes.Count <= 0)
                {
                    SaveBtn.IsEnabled = false;
                }
                else
                {
                    SaveBtn.IsEnabled = true;
                }
            }
        }

        private void StrokeWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsPageLoaded)
            {
                DrawinkCanvas.DefaultDrawingAttributes.Height = DrawinkCanvas.DefaultDrawingAttributes.Width = StrokeHigh = StrokeWidth = e.NewValue;
                UpDataToStrokesObject();
            }
        }

        private void ColorPickerControl_Loaded(object sender, RoutedEventArgs e)
        {
            ColorPickerControl.SetIsChecked(1);
        }

        private void TextColorPickerControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextColorPickerControl.SetIsChecked(1);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty((sender as TextBox).Text))
            {
                int num;
                int.TryParse((sender as TextBox).Text, out num);
                if (num > StrokeWidthSlider.Maximum)
                {
                    (sender as TextBox).Text = StrokeWidthSlider.Maximum.ToString();
                }

                if (num < StrokeWidthSlider.Minimum)
                {
                    (sender as TextBox).Text = StrokeWidthSlider.Minimum.ToString();
                }
            }
        }
    }
}
