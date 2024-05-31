using ComPDFKit.Controls.Data;
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
using System.Windows.Ink;
using ComPDFKit.Controls.Common;
using ComPDFKitViewer.Helper;
using ComPDFKit.PDFDocument;

namespace ComPDFKit.Controls.Annotation.PDFAnnotationPanel.PDFAnnotationUI
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

        private string postScriptName;

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

            FontNameCmb.Items.Clear();
            FontNameCmb.ItemsSource = CPDFFont.GetFontNameDictionary().Keys;
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

            if (CreateSignatureControl.SelectedIndex == 1)
            {
                InPutTextBox.Focus();
            }
        }
        private void UpDateToStrokesObject()
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
                UpDateToStrokesObject();
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
            Bitmap bmp = TextToBitmap(InPutTextBox.Text, postScriptName, 50, System.Drawing.Rectangle.Empty, fontcolor, System.Drawing.Color.Transparent);
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
            WriteableBitmap writeStamp = RawPointListToBitmap(RawPointList, inkThickness, DrawinkCanvas.DefaultDrawingAttributes.Color);
            if (writeStamp == null)
            {
                return;
            }

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

        private WriteableBitmap RawPointListToBitmap(List<List<System.Windows.Point>> RawPointList, double InkThickness, System.Windows.Media.Color InkColor)
        {
            if (RawPointList != null && RawPointList.Count > 0)
            {
                GeometryGroup PaintGeomtry = new GeometryGroup();
                int minLeft = -1;
                int minTop = -1;
                int maxLeft = -1;
                int maxTop = -1;

                foreach (List<System.Windows.Point> Item in RawPointList)
                {
                    for (int i = 0; i < Item.Count; i++)
                    {
                        System.Windows.Point paintPoint = Item[i];

                        if (minLeft == -1)
                        {
                            minLeft = (int)paintPoint.X;
                            maxLeft = (int)paintPoint.X;
                            minTop = (int)paintPoint.Y;
                            maxTop = (int)paintPoint.Y;
                        }
                        else
                        {
                            minLeft = (int)Math.Min(minLeft, paintPoint.X);
                            maxLeft = (int)Math.Max(maxLeft, paintPoint.X);
                            minTop = (int)Math.Min(minTop, paintPoint.Y);
                            maxTop = (int)Math.Max(maxTop, paintPoint.Y);
                        }
                    }
                }
                if (minLeft >= 0 && maxLeft > minLeft && minTop >= 0 && maxTop > minTop)
                {
                    List<List<System.Windows.Point>> points = new List<List<System.Windows.Point>>();

                    foreach (List<System.Windows.Point> Item in RawPointList)
                    {
                        PathGeometry PaintPath = new PathGeometry();
                        PathFigureCollection Figures = new PathFigureCollection();
                        PathFigure AddFigure = new PathFigure();
                        Figures.Add(AddFigure);
                        PaintPath.Figures = Figures;
                        PaintGeomtry.Children.Add(PaintPath);

                        List<System.Windows.Point> changeList = new List<System.Windows.Point>();
                        for (int i = 0; i < Item.Count; i++)
                        {
                            System.Windows.Point paintPoint = DpiHelper.CurrentPointToStandardPoint(new System.Windows.Point(Item[i].X - minLeft, Item[i].Y - minTop));
                            changeList.Add(paintPoint);

                            if (i == 0)
                            {
                                AddFigure.StartPoint = paintPoint;
                            }
                            else
                            {
                                LineSegment AddSegment = new LineSegment();
                                AddSegment.Point = paintPoint;
                                AddFigure.Segments.Add(AddSegment);
                            }
                        }
                        if (changeList.Count > 0)
                        {
                            points.Add(changeList);
                        }
                    }
                    int drawWidth = (int)DpiHelper.CurrentNumToStandardNum(maxLeft - minLeft);
                    int drawHeight = (int)DpiHelper.CurrentNumToStandardNum(maxTop - minTop);
                    RawPointList = points;

                    DrawingVisual copyVisual = new DrawingVisual();
                    DrawingContext copyContext = copyVisual.RenderOpen();
                    System.Windows.Media.Pen drawPen = new System.Windows.Media.Pen(new SolidColorBrush(InkColor), InkThickness);
                    copyContext?.DrawGeometry(null, drawPen, PaintGeomtry);
                    copyContext.Close();
                    RenderTargetBitmap targetBitmap = new RenderTargetBitmap(drawWidth, drawHeight, 96, 96, PixelFormats.Pbgra32);
                    targetBitmap.Render(copyVisual);
                    byte[] ImageArray = new byte[targetBitmap.PixelWidth * targetBitmap.PixelHeight * 4];
                    targetBitmap.CopyPixels(new Int32Rect(0, 0, (int)targetBitmap.PixelWidth, (int)targetBitmap.PixelHeight), ImageArray, targetBitmap.PixelWidth * 4, 0);

                    WriteableBitmap writeBitmap = new WriteableBitmap(targetBitmap.PixelWidth, targetBitmap.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
                    writeBitmap.WritePixels(new Int32Rect(0, 0, targetBitmap.PixelWidth, targetBitmap.PixelHeight), ImageArray, targetBitmap.PixelWidth * 4, 0);
                    return writeBitmap;
                }
            }
            return null;
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
            DrawinkCanvas.Strokes.StrokesChanged -= Strokes_StrokesChanged;
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
                UpDateToStrokesObject();
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

        private void FontNameCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //StyleNameCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontNameCmb.SelectedValue.ToString()];  
            StyleNameCmb.SelectedIndex = 0;
            CPDFFont.GetPostScriptName(FontNameCmb.SelectedValue.ToString(), StyleNameCmb.SelectedValue.ToString(),ref postScriptName);
            InPutTextBox.FontFamily = new System.Windows.Media.FontFamily(postScriptName);
        }

        private void StyleNameCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(StyleNameCmb.SelectedValue?.ToString()))
            {
                InPutTextBox.FontStyle = GetFontStyle(StyleNameCmb.SelectedValue.ToString());
                InPutTextBox.FontWeight = GetFontWeight(StyleNameCmb.SelectedValue.ToString());
            } 
        }

        private FontWeight GetFontWeight(string toString)
        {
            switch (toString)
            {
                case "Regular":
                    return FontWeights.Normal;
                case "Oblique":
                    return FontWeights.Normal;
                case "Bold":
                    return FontWeights.Bold;
                case "BoldOblique":
                    return FontWeights.Bold;
                default:
                    return FontWeights.Normal;
            }
        }

        private System.Windows.FontStyle GetFontStyle(string style)
        {
            switch (style)
            {
                case "Regular":
                    return FontStyles.Normal;
                case "Oblique":
                    return FontStyles.Italic;
                case "Bold":
                    return FontStyles.Normal;
                case "BoldOblique":
                    return FontStyles.Oblique;
                default:
                    return FontStyles.Normal;
            }
        }
    }
}
