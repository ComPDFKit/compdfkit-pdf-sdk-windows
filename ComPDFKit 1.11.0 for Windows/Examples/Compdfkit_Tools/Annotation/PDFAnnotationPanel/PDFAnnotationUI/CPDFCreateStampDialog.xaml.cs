using ComPDFKit.PDFAnnotation;
using Compdfkit_Tools.Data;
using ComPDFKitViewer.AnnotEvent;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI
{
    public partial class CPDFCreateStampDialog : Window
    {
        public CPDFStampData cPDFStampData;
        private string SaveToPath;
        private string StampTextDate;
        private static int Dpi;
        private string CustomStampPath;
        private int StampWidth;
        private int StampHeight;
        private static float ScaleDpi { get { return (96F / Dpi); } }
        private C_TEXTSTAMP_SHAPE Shape;
        private C_TEXTSTAMP_COLOR Color;
        bool PageLoaded = false;

        public ObservableCollection<string> ShapeBoxList { get; set; }
        public CPDFCreateStampDialog()
        {
            InitializeComponent();
        }

        public void SetCreateHeaderIndex(int index)
        {
            CreateHeader.SelectedIndex = index;
            if (index == 0)
            {
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PageLoaded)
            {
                UpTextPreview();
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            StampType Type = StampType.UNKNOWN_STAMP;
            switch (CreateHeader.SelectedIndex)
            {
                case 0:
                    Type = StampType.TEXT_STAMP;
                    break;
                case 1:
                    Type = StampType.IMAGE_STAMP;
                    break;
                default:
                    break;
            }
            CretaeStampData(Type);
            this.Close();
        }

        private void SaveToImage(string FilePath)
        {
            string path = CustomStampPath;
            string name = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(CustomStampPath))
            {
                try
                {
                    BitmapImage image = new BitmapImage(new Uri(FilePath));
                    double scale = Math.Min((double)600 / image.PixelWidth, (double)600 / image.PixelHeight);
                    scale = Math.Min(scale, 1);
                    string ext = Path.GetExtension(FilePath);
                    if (ext.ToUpper() == ".PNG")
                    {
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
                        ImageImage.Source = targetBitmap;
                        StampWidth = targetBitmap.PixelWidth;
                        StampHeight = targetBitmap.PixelHeight;
                        AddImagebackground.Visibility = Visibility.Collapsed;
                        SaveBtn.IsEnabled = true;
                    }
                    else
                    {
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        TransformedBitmap targetBitmap = new TransformedBitmap(image, new ScaleTransform(scale, scale));
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
                        ImageImage.Source = targetBitmap;
                        StampWidth = targetBitmap.PixelWidth;
                        StampHeight = targetBitmap.PixelHeight;
                        AddImagebackground.Visibility = Visibility.Collapsed;
                        SaveBtn.IsEnabled = true;
                    }
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

        private void UpTextPreview()
        {
            if (!PageLoaded)
            {
                return;
            }
            string date = "";
            string dateType = "";
            if ((bool)Date.IsChecked)
            {
                dateType = "yyyy-MM-dd";
            }
            if ((bool)Time.IsChecked)
            {
                dateType = dateType + " HH:mm:ss";
            }
            if (!String.IsNullOrEmpty(dateType))
            {
                date = DateTime.Now.ToString(dateType);
            }

            var bytes = CPDFStampAnnotation.GetTempTextStampImage(StampText.Text, date,
            Shape, Color, out int stampWidth, out int stampHeight, out int width, out int height);
            if (bytes.Length > 0)
            {
                PixelFormat fmt = PixelFormats.Bgra32;
                BitmapSource bps = BitmapSource.Create(width, height, 96, 96, fmt, null, bytes, (width * fmt.BitsPerPixel + 7) / 8);
                TextImage.Source = bps;
                StampTextDate = date;
                StampWidth = stampWidth;
                StampHeight = stampHeight;
                //Type = StampType.TEXT_STAMP;
            }
            else
            {
                TextImage.Source = null;
            }
        }

        private void ShapeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageLoaded)
            {
                switch (ShapeBox.SelectedIndex)
                {
                    case 0:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_WHITE;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_NONE;
                        break;
                    case 1:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_GREEN;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_RECT;
                        break;
                    case 2:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_BLUE;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_RECT;
                        break;
                    case 3:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_RED;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_RECT;
                        break;
                    case 4:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_GREEN;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_LEFT_TRIANGLE;
                        break;
                    case 5:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_BLUE;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_LEFT_TRIANGLE;
                        break;
                    case 6:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_RED;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_LEFT_TRIANGLE;
                        break;
                    case 7:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_GREEN;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_RIGHT_TRIANGLE;
                        break;
                    case 8:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_BLUE;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_RIGHT_TRIANGLE;
                        break;
                    case 9:
                        Color = C_TEXTSTAMP_COLOR.TEXTSTAMP_RED;
                        Shape = C_TEXTSTAMP_SHAPE.TEXTSTAMP_RIGHT_TRIANGLE;
                        break;
                    default:
                        break;
                }
                UpTextPreview();
            }
        }

        private void CretaeStampData(StampType Type)
        {
            switch (Type)
            {
                case StampType.UNKNOWN_STAMP:
                    break;
                case StampType.IMAGE_STAMP:
                    CreateCPDFStampData();
                    cPDFStampData.Type = Type;
                    break;
                case StampType.TEXT_STAMP:
                    SaveImageToPath();
                    CreateCPDFStampData();
                    cPDFStampData.Type = Type;
                    break;
                default:
                    break;
            }
        }

        private void SaveImageToPath()
        {
            string path = CustomStampPath;
            string name = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(path) && TextImage.Source != null)
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)TextImage.Source));
                path = System.IO.Path.Combine(path, name);
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(stream);
                }
                SaveToPath = path;
            }
            else
            {
                SaveToPath = "";
            }
        }

        private void CreateCPDFStampData()
        {
            CPDFStampData stamp = new CPDFStampData();
            stamp.Opacity = 1;
            stamp.SourcePath = SaveToPath;
            stamp.StampText = StampText.Text;
            stamp.MaxWidth = (int)(double)((StampWidth / 72D * Dpi) * ScaleDpi);
            stamp.MaxHeight = (int)(double)((StampHeight / 72D * Dpi) * ScaleDpi);
            stamp.StampTextDate = StampTextDate;
            stamp.TextColor = (TextStampColor)(int)Color;
            stamp.TextSharp = (TextStampSharp)(int)Shape;
            stamp.IsCheckedDate = (bool)Date.IsChecked;
            stamp.IsCheckedTime = (bool)Time.IsChecked;
            stamp.AnnotationType = CPDFAnnotationType.Stamp;
            cPDFStampData = stamp;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShapeBoxList = new ObservableCollection<string>
            {
                Helper.LanguageHelper.PropertyPanelManager.GetString("Style_General"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Rec_Green"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Rec_Blue"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Rec_Red"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Left_Green"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Left_Blue"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Left_Red"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Right_Green"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Right_Blue"),
                Helper.LanguageHelper.PropertyPanelManager.GetString("Right_Red")
            };
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var dpiProperty = typeof(SystemParameters).GetProperty("Dpi", flags);
            Dpi = (int)dpiProperty.GetValue(null, null);

            CustomStampPath = System.IO.Path.Combine(Environment.CurrentDirectory, "ComPDFKit");
            CustomStampPath = System.IO.Path.Combine(CustomStampPath, "CustomStamp");
            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(CustomStampPath);

            Binding ShapeBoxbinding = new Binding();
            ShapeBoxbinding.Source = this;
            ShapeBoxbinding.Path = new System.Windows.PropertyPath("ShapeBoxList");
            ShapeBox.SetBinding(ListBox.ItemsSourceProperty, ShapeBoxbinding);

            PageLoaded = true;

            UpTextPreview();
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


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ShapeBox.SelectedIndex = 0;
            cPDFStampData = null;
            ImageImage.Source = null;
            TextImage.Source = null;
            Date.IsChecked = false;
            Time.IsChecked = false;
            StampText.Text = "Stamp Text";
            AddImagebackground.Visibility = Visibility.Visible;
            if (CreateHeader.SelectedIndex == 0)
            {
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
            }
            UpTextPreview();
        }

        private void Date_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StampText.Text))
            {
                SaveBtn.IsEnabled = false;
            }
            else
            {
                UpTextPreview();
                SaveBtn.IsEnabled = true;
            }

        }

        private void Date_Unchecked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StampText.Text))
            {
                SaveBtn.IsEnabled = false;
            }
            else
            {
                UpTextPreview();
                SaveBtn.IsEnabled = true;
            }
        }

        private void Time_Checked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StampText.Text))
            {
                SaveBtn.IsEnabled = false;
            }
            else
            {
                UpTextPreview();
                SaveBtn.IsEnabled = true;
            }
        }

        private void Time_Unchecked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StampText.Text))
            {
                SaveBtn.IsEnabled = false;
            }
            else
            {
                UpTextPreview();
                SaveBtn.IsEnabled = true;
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

        private void ImageImage_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (PageLoaded)
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

        private void CreateHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (PageLoaded && tabControl != null)
            {
                switch (tabControl.SelectedIndex)
                {
                    case 0:
                        if (string.IsNullOrEmpty(StampText.Text))
                        {
                            SaveBtn.IsEnabled = false;
                        }
                        else
                        {
                            SaveBtn.IsEnabled = true;
                        }
                        break;
                    case 1:
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
    }
}
