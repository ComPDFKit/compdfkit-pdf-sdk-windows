using ComPDFKit.DigitalSign;
using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.Controls.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pen = System.Windows.Media.Pen;
using PixelFormat = System.Windows.Media.PixelFormat;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for CPDFSignControl.xaml
    /// </summary>
    public partial class FillDigitalSignatureControl : UserControl
    {

        private readonly string logoPath = "Logo_opa40.png";
        private string imagePath = string.Empty;
        private string Text = string.Empty;
        private Dictionary<string, Border> TabDict { get; set; }

        private CPDFSignatureConfig tempSignatureConfig = new CPDFSignatureConfig();

        private CPDFSignatureCertificate signatureCertificate;

        public CPDFDocument Document;

        private string signatureName = string.Empty;

        private string location = string.Empty;

        private string reason = string.Empty;

        private float[] textColor = new float[] { 0, 0, 0 };

        private string _signaturePath = string.Empty;
        public string SignaturePath
        {
            get => _signaturePath;
            set
            {
                _signaturePath = value;
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                signatureCertificate = CPDFPKCS12CertHelper.GetCertificateWithPKCS12Path(SignaturePath, Password);
                signatureName = DictionaryValueConverter.GetGrantorFromDictionary(signatureCertificate.SubjectDict);
                tempSignatureConfig.Text = signatureName;
                InitTempSignature();
            }
        }

        private void InitTempSignature()
        {
            NameChk.IsChecked = true;
            DateChk.IsChecked = true;
            LogoChk.IsChecked = true;
            TabChk.IsChecked = true;
            SetProperty();
            CreateTempSignature();
            KeyboardInPutTextBox.Text = signatureName;
        }

        public CPDFSignatureWidget signatureWidget { get; set; }

        public event EventHandler<string> AfterFillSignature;

        public FillDigitalSignatureControl()
        {
            InitializeComponent();
            TabDict = new Dictionary<string, Border>
            {
                ["Keyboard"] = KeyboardBorder,
                ["Trackpad"] = TrackpadBorder,
                ["Image"] = ImageBorder,
                ["None"] = NoneBorder
            };
            SetCheckedTab("Keyboard");
            ReasonCmb.SelectedIndex = 0;
        }

        private void CreateTempSignature()
        {
            CPDFDocument tempDocument = CPDFDocument.CreateDocument();
            tempDocument.InsertPage(0, 200, 200, string.Empty);
            CPDFPage page = tempDocument.PageAtIndex(0);
            CPDFSignatureWidget signatureWidget = page.CreateWidget(C_WIDGET_TYPE.WIDGET_SIGNATUREFIELDS) as CPDFSignatureWidget;
            signatureWidget.SetRect(new CRect(0, 100, 300, 0));
            tempSignatureConfig.IsDrawLogo = (bool)LogoChk.IsChecked;
            if (tempSignatureConfig.IsDrawLogo)
            {
                if(!string.IsNullOrEmpty(logoPath))
                {
                    byte[] imageData = null;
                    int imageWidth = 0;
                    int imageHeight = 0;

                    ComPDFKit.Tool.Help.PDFHelp.ImagePathToByte(logoPath, ref imageData, ref imageWidth, ref imageHeight);
                    if (imageData != null && imageWidth > 0 && imageHeight > 0)
                    {
                        if (signatureWidget.IsValid())
                        {
                            tempSignatureConfig.LogoData = imageData;
                            tempSignatureConfig.LogoWidth = imageWidth;
                            tempSignatureConfig.LogoHeight = imageHeight;
                        }
                    }
                }
            }

            tempSignatureConfig.Content = Text;
            tempSignatureConfig.TextColor = textColor;
            tempSignatureConfig.ContentColor = new float[] { 0, 0, 0 };
            signatureWidget.UpdataApWithSignature(tempSignatureConfig);

            byte[] signatureBitmapBytes = GetTempSignatureImage(signatureWidget, out int width, out int height);

            signatureWidget.ReleaseAnnot();

            if (signatureBitmapBytes.Length > 0)
            {
                PixelFormat fmt = PixelFormats.Bgra32;
                BitmapSource bps = BitmapSource.Create(width, height, 96, 96, fmt, null, signatureBitmapBytes, (width * fmt.BitsPerPixel + 7) / 8);
                imageControl.Source = bps;
            }
            else
            {
                imageControl.Source = null;
            }
        }

        public static byte[] GetTempSignatureImage(CPDFSignatureWidget signatureWidget, out int width, out int height)
        {
            CRect rect = signatureWidget.GetRect();

            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var dpiProperty = typeof(SystemParameters).GetProperty("Dpi", flags);
            int dpi = (int)dpiProperty.GetValue(null, null);

            width = (int)(rect.width() * dpi / 72D * 2);
            height = (int)(rect.height() * dpi / 72D * 2);

            byte[] imageData = new byte[width * height * 4];
            signatureWidget.RenderAnnot(width, height, imageData, CPDFAppearanceType.Normal);

            return imageData;
        }

        private void TextAlignBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton checkBtn = sender as ToggleButton;
            if (checkBtn == null)
            {
                return;
            }
            checkBtn.IsChecked = true;
            if (checkBtn != TextAlignLeftBtn)
            {
                tempSignatureConfig.IsContentAlignLeft = true;
                TextAlignLeftBtn.IsChecked = false;
            }
            if (checkBtn != TextAlignRightBtn)
            {
                tempSignatureConfig.IsContentAlignLeft = false;
                TextAlignRightBtn.IsChecked = false;
            }
            CreateTempSignature();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border clickBorder = sender as Border;
            if (clickBorder == null || clickBorder.Tag == null)
            {
                return;
            }
            SetCheckedTab(clickBorder.Tag.ToString());
            ImagePickPanel.Visibility = Visibility.Hidden;

            if (clickBorder == NoneBorder)
            {
                tempSignatureConfig.IsDrawOnlyContent = true;
            }
            else
            {
                tempSignatureConfig.IsDrawOnlyContent = false;
                if (clickBorder == KeyboardBorder)
                {
                    tempSignatureConfig.Text = signatureName;
                    //tempSignatureConfig.ImageBitmap = null;
                    KeyboardPopup.Visibility = Visibility.Visible;
                }
                else
                {
                    tempSignatureConfig.Text = string.Empty;
                    if (clickBorder == TrackpadBorder)
                    {
                        CanvaDrawPopup.Visibility = Visibility.Visible;
                    }
                    else if (clickBorder == ImageBorder)
                    {
                        ImagePickPanel.Visibility = Visibility.Visible;
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            byte[] imageData = null;
                            int imageWidth = 0;
                            int imageHeight = 0;

                            ComPDFKit.Tool.Help.PDFHelp.ImagePathToByte(imagePath, ref imageData, ref imageWidth, ref imageHeight);
                            if (imageData != null && imageWidth > 0 && imageHeight > 0)
                            {
                                if (signatureWidget.IsValid())
                                {
                                    tempSignatureConfig.ImageData = imageData;
                                    tempSignatureConfig.ImageWidth = imageWidth;
                                    tempSignatureConfig.ImageHeight = imageHeight;
                                }
                            }
                        }
                    }
                }
            }
            SetProperty();
            CreateTempSignature();
        }

        private void SetCheckedTab(string tab)
        {
            if (TabDict != null && TabDict.ContainsKey(tab))
            {
                foreach (string key in TabDict.Keys)
                {
                    Border checkBorder = TabDict[key];
                    if (checkBorder == null)
                    {
                        continue;
                    }

                    checkBorder.BorderThickness = new Thickness(0);
                    if (key == tab)
                    {
                        checkBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                    }
                }
            }
        }

        private void CanvasPopupClose_Click(object sender, RoutedEventArgs e)
        {
            CanvaDrawPopup.Visibility = Visibility.Collapsed;
        }

        private void CanvasClearBtn_Click(object sender, RoutedEventArgs e)
        {
            DrawInkCanvas.Strokes.Clear();

        }

        private void CanvasPopupConfirm_Click(object sender, RoutedEventArgs e)
        {
            int height = 0;
            int width = 0;
            tempSignatureConfig.ImageData = GetDrawInk(ref height,ref width);
            tempSignatureConfig.ImageWidth = width;
            tempSignatureConfig.ImageHeight = height;
            CanvaDrawPopup.Visibility = Visibility.Collapsed;
            SetProperty();
            CreateTempSignature();
        }

        public byte[] GetDrawInk(ref int height, ref int width)
        {
            if (DrawInkCanvas != null && DrawInkCanvas.Strokes != null && DrawInkCanvas.Strokes.Count > 0)
            {
                Rect bound = DrawInkCanvas.Strokes.GetBounds();
                DrawingVisual drawVisual = new DrawingVisual();
                DrawingContext drawContext = drawVisual.RenderOpen();

                foreach (Stroke drawStroke in DrawInkCanvas.Strokes)
                {
                    Pen drawPen = new Pen(new SolidColorBrush(drawStroke.DrawingAttributes.Color), drawStroke.DrawingAttributes.Width);
                    PathGeometry drawPath = new PathGeometry();
                    PathFigureCollection Figures = new PathFigureCollection();
                    PathFigure AddFigure = new PathFigure();
                    Figures.Add(AddFigure);
                    drawPath.Figures = Figures;

                    if (drawStroke.StylusPoints.Count > 1)
                    {
                        StylusPoint startPoint = drawStroke.StylusPoints[0];
                        AddFigure.StartPoint = new Point(startPoint.X - bound.X, startPoint.Y - bound.Y);
                        for (int i = 1; i < drawStroke.StylusPoints.Count; i++)
                        {
                            StylusPoint drawPoint = drawStroke.StylusPoints[i];
                            Point offsetPoint = new Point(drawPoint.X - bound.X, drawPoint.Y - bound.Y);
                            LineSegment drawSegment = new LineSegment();
                            drawSegment.Point = offsetPoint;
                            AddFigure.Segments.Add(drawSegment);
                        }
                    }

                    if (AddFigure.Segments.Count > 0)
                    {
                        drawContext.DrawGeometry(null, drawPen, drawPath);
                    }
                }
                drawContext.Close();

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bound.Width, (int)bound.Height, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawVisual);
                BitmapFrame newFrame = BitmapFrame.Create(renderBitmap);
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(newFrame);
                using (MemoryStream newStream = new MemoryStream())
                {
                    pngEncoder.Save(newStream);
                    Bitmap bitmap = new Bitmap(newStream);
                    height = bitmap.Height;
                    width = bitmap.Width;
                    return GetBitmapData(bitmap);
                }
            }
            else
            {
                return null;
            }
        }

        private byte[] GetBitmapData(Bitmap bitmap)
        {
            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;

            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            return rgbValues;
        }
        
        private void SetProperty()
        {
            Text = string.Empty;
            if ((bool)NameChk.IsChecked)
            {
                if ((bool)TabChk.IsChecked)
                {
                    Text += "Name: ";
                }
                Text += DictionaryValueConverter.GetGrantorFromDictionary(signatureCertificate.SubjectDict) + "\n";
            }

            if ((bool)DateChk.IsChecked)
            {
                if ((bool)TabChk.IsChecked)
                {
                    Text += "Date: ";
                }
                DateTime currentDateTime = DateTime.Now;

                string customFormat = "yyyy.MM.dd HH:mm:ss";

                string formattedDateTime = currentDateTime.ToString(customFormat);
                Text += formattedDateTime + "\n";
            }

            if ((bool)LogoChk.IsChecked)
            {
                tempSignatureConfig.IsDrawLogo = true;
            }
            else
            {
                tempSignatureConfig.IsDrawLogo = false;
            }

            if ((bool)ReasonChk.IsChecked)
            {
                if ((bool)TabChk.IsChecked)
                {
                    Text += "Reason: ";
                }
                Text += (ReasonCmb.SelectedItem as ComboBoxItem).Content.ToString() + "\n";
            }

            if ((bool)DistinguishableNameChk.IsChecked)
            {
                if ((bool)TabChk.IsChecked)
                {
                    Text += "DN: ";
                }
                var keyOrder = new List<string> { "CN",  "O", "OU", "emailAddress", "L", "ST", "C" };

                var keyMapping = new Dictionary<string, string>
                {
                    { "CN", "cn" },
                    { "OU", "ou" },
                    { "O", "o" },
                    { "L", "l" },
                    { "ST", "st" },
                    { "C", "c" },
                    { "emailAddress", "email" }
                };

                var stringBuilder = new StringBuilder();

                foreach (var originalKey in keyOrder)
                {
                    if (keyMapping.TryGetValue(originalKey, out string newKey) &&
                        signatureCertificate.SubjectDict.TryGetValue(originalKey, out string value) && !string.IsNullOrEmpty(value))
                    {
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append(",");
                        }
                        stringBuilder.Append(newKey + "=" + value);
                    }
                }

                 Text += stringBuilder.ToString()+"\n";
            }

            if ((bool)ComPDFKitVersionChk.IsChecked)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                if ((bool)TabChk.IsChecked)
                {
                    Text += "ComPDFKit Version: ";
                }
                Text += version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString() + "\n";

            }

            if ((bool)PositionChk.IsChecked)
            {
                if ((bool)TabChk.IsChecked)
                {
                    Text += "Position: ";
                }
                Text += PositionTbx.Text + "\n";
            }
        }

        private void ReasonCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkItem = sender as CheckBox;
            if (checkItem == null)
            {
                return;
            }

            ReasonPanel.Visibility = checkItem.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BrowseTxt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            string pngPath = CommonHelper.GetExistedPathOrEmpty(filter);
            if (!string.IsNullOrEmpty(pngPath))
            {
                try
                {
                    byte[] imageData = null;
                    int imageWidth = 0;
                    int imageHeight = 0;
                    ComPDFKit.Tool.Help.PDFHelp.ImagePathToByte(pngPath, ref imageData, ref imageWidth, ref imageHeight);
                    if (imageData != null && imageWidth > 0 && imageHeight > 0)
                    {
                        tempSignatureConfig.ImageData = imageData;
                        tempSignatureConfig.ImageWidth = imageWidth;
                        tempSignatureConfig.ImageHeight = imageHeight;
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("The image is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                SetProperty();
                CreateTempSignature();
            }
        }

        private void ClearTxt_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            imagePath = string.Empty;
            //tempSignatureConfig.ImageBitmap = null;
            SetProperty();
            CreateTempSignature();
        }

        private void NameChk_Click(object sender, RoutedEventArgs e)
        {
            SetProperty();
            CreateTempSignature();
        }

        private void DateChk_Click(object sender, RoutedEventArgs e)
        {
            SetProperty();
            CreateTempSignature();
        }

        private void LogoChk_Click(object sender, RoutedEventArgs e)
        {
            SetProperty();
            CreateTempSignature();
        }

        private void ReasonChk_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)ReasonChk.IsChecked)
            {
                Reasonstp.Visibility = Visibility.Collapsed;
            }
            else
            {
                Reasonstp.Visibility = Visibility.Visible;
            }
            SetProperty();
            CreateTempSignature();
        }

        private void DistinguishableNameChk_Click(object sender, RoutedEventArgs e)
        {
            SetProperty();
            CreateTempSignature();
        }

        private void PositionChk_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)PositionChk.IsChecked)
            {
                PositionStp.Visibility = Visibility.Collapsed;
            }
            else
            {
                PositionStp.Visibility = Visibility.Visible;
            }
            SetProperty();
            CreateTempSignature();
        }

        private void TabChk_Click(object sender, RoutedEventArgs e)
        {
            SetProperty();
            CreateTempSignature();
        }

        private void ReasonCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetProperty();
            CreateTempSignature();
        }

        private void ComPDFKitVersionChk_Click(object sender, RoutedEventArgs e)
        {
            SetProperty();
            CreateTempSignature();
        }

        private void PositionTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(bool)PositionChk.IsChecked)
            {
                PositionStp.Visibility = Visibility.Collapsed;
            }
            else
            {
                PositionStp.Visibility = Visibility.Visible;
            }
            SetProperty();
            CreateTempSignature();
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            string filePath = CommonHelper.GetGeneratePathOrEmpty("PDF files (*.pdf)|*.pdf", Document.FileName + "_Signed.pdf");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (filePath.ToLower() == Document.FilePath.ToLower())
            {
                MessageBox.Show("Do not use the new file name that is the same as the current file name.");
                return;
            }

            if ((bool)ReasonChk.IsChecked)
            {
                reason = (ReasonCmb?.SelectedItem as ComboBoxItem)?.Content?.ToString();
            }
            else
            {
                reason = string.Empty;
            }

            if ((bool)PositionChk.IsChecked)
            {
                location = PositionTbx.Text;
            }
            else
            {
                location = string.Empty;
            }

            signatureWidget.UpdataApWithSignature(tempSignatureConfig);

            if (Document.WriteSignatureToFilePath(signatureWidget, filePath, SignaturePath, Password, location, reason, CPDFSignaturePermissions.CPDFSignaturePermissionsNone))
            {
                signatureCertificate.AddToTrustedCertificates();
                AfterFillSignature?.Invoke(sender, filePath);
            }

            CloseWindow(sender);
        }


        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow(sender);
        }

        private void CloseWindow(object sender)
        {
            Window parentWindow = Window.GetWindow(sender as DependencyObject);
            parentWindow?.Close();
        }

        private void TextColorPickerControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextColorPickerControl.SetIsChecked(0);
            TextColorPickerControl.Brush = new SolidColorBrush(Colors.Black);
        }

        private void TextColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            KeyboardInPutTextBox.Foreground = TextColorPickerControl.Brush;
        }

        private void KeyboardCancel_Click(object sender, RoutedEventArgs e)
        {
            KeyboardPopup.Visibility = Visibility.Collapsed;
        }

        private void KeyboardClear_Click(object sender, RoutedEventArgs e)
        {
            KeyboardInPutTextBox.Text = string.Empty;
        }

        private void KeyboardSave_Click(object sender, RoutedEventArgs e)
        {
            signatureName = KeyboardInPutTextBox.Text;
            tempSignatureConfig.Text = signatureName;
            SolidColorBrush solidColorBrush = TextColorPickerControl.Brush as SolidColorBrush;
            float red = solidColorBrush.Color.R;
            float green = solidColorBrush.Color.G;
            float blue = solidColorBrush.Color.B;
            textColor = new[] { red / 255, green / 255, blue / 255 };

            KeyboardPopup.Visibility = Visibility.Collapsed;
            SetProperty();
            CreateTempSignature();
        }

        private void KeyboardPopupClose_Click(object sender, RoutedEventArgs e)
        {
            KeyboardPopup.Visibility = Visibility.Collapsed;
        }
    }
}
