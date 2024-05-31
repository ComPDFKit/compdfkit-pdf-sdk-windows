using ComPDFKit.PDFDocument;
using ComPDFKit.PDFWatermark;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static ComPDFKit.Controls.PDFControl.PageRangeDialog;
using MessageBox = System.Windows.MessageBox;


namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for WatermarkDialog.xaml
    /// </summary>
    public partial class WatermarkDialog : Window, INotifyPropertyChanged
    {
        private bool _canChangeRange = false;
        public bool CanChangeRange
        {
            get => _canChangeRange;
            set => UpdateProper(ref _canChangeRange, value);
        }

        private C_Watermark_Type _watermarkType = C_Watermark_Type.WATERMARK_TYPE_TEXT;
        public C_Watermark_Type WatermarkType
        {
            get => _watermarkType;
            set
            {
                if (UpdateProper(ref _watermarkType, value))
                {
                    watermarkData.Type = _watermarkType;
                }
            }
        }

        private string _watermarkText = "Watermark";
        public string WatermarkText
        {
            get => _watermarkText;
            set
            {
                if (UpdateProper(ref _watermarkText, value))
                {
                    watermarkData.Text = _watermarkText;
                }
            }
        }

        private byte[] _brush = new byte[] { 255, 0, 0 };
        public byte[] Brush
        {
            get => _brush;
            set
            {
                if (UpdateProper(ref _brush, value))
                {
                    watermarkData.Color = _brush;
                }
            }
        }
        private int _selectedTag = 4;
        public int SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (UpdateProper(ref _selectedTag, value))
                {
                    watermarkData.Align = _selectedTag;
                }
            }
        }

        private string _imagePath = "";
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (CommonHelper.IsImageCorrupted(value))
                {
                    return;
                }
                if (UpdateProper(ref _imagePath, value))
                { 
                    watermarkData.ImagePath = _imagePath;
                }
            }
        }

        private string _imageScale = "100";
        public string ImageScale
        {
            get => _imageScale;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                if (int.Parse(value) > ScaleNumericControl.Maximum)
                {
                    value = ScaleNumericControl.Maximum.ToString();
                }
                if (int.Parse(value) < ScaleNumericControl.Minimum)
                {
                    value = ScaleNumericControl.Minimum.ToString();
                }
                if (UpdateProper(ref _imageScale, value))
                {
                    watermarkData.ImageScale = int.Parse(_imageScale);
                }
            }
        }

        private WatermarkData watermarkData = new WatermarkData();

        private string _fontName = "Helvetica";
        public string FontName
        {
            get => _fontName;
            set
            {
                if (UpdateProper(ref _fontName, value))
                {
                    watermarkData.FontName = _fontName;
                }
            }
        }

        private string _textSize = "48";
        public string TextSize
        {
            get => _textSize;
            set
            {
                if (UpdateProper(ref _textSize, value))
                {
                    watermarkData.FontSize = float.Parse(_textSize);
                }
            }
        }

        private bool _isFront = true;
        public bool IsFront
        {
            get => _isFront;
            set
            {
                if (UpdateProper(ref _isFront, value))
                {
                    watermarkData.IsFront = _isFront;
                }
            }
        }

        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (UpdateProper(ref _isFullScreen, value))
                {
                    watermarkData.IsFullScreen = _isFullScreen;
                }
            }
        }

        private int _rotation;
        public int Rotation
        {
            get => _rotation;
            set
            {
                if (UpdateProper(ref _rotation, value))
                {
                    watermarkData.Rotation = _rotation;
                }
            }
        }

        private byte _opacityValue = 255;
        public byte OpacityValue
        {
            get => _opacityValue;
            set
            {
                if (UpdateProper(ref _opacityValue, value))
                {
                    watermarkData.Opacity = _opacityValue;
                }
            }
        }

        private float _verticalSpacing;
        public float VerticalSpacing
        {
            get => _verticalSpacing;
            set
            {
                if (UpdateProper(ref _verticalSpacing, value))
                {
                    watermarkData.VerticalSpacing = _verticalSpacing;
                }
            }
        }

        private float _horizontalSpacing;
        public float HorizontalSpacing
        {
            get => _horizontalSpacing;
            set
            {
                if (UpdateProper(ref _horizontalSpacing, value))
                {
                    watermarkData.HorizontalSpacing = _horizontalSpacing;
                }
            }
        }

        private float _vertOffset;
        public float VertOffset
        {
            get => _vertOffset;
            set
            {
                if (UpdateProper(ref _vertOffset, value))
                {
                    watermarkData.VertOffset = _vertOffset;
                }
            }
        }

        private float _horizOffset;
        public float HorizOffset
        {
            get => _horizOffset;
            set
            {
                if (UpdateProper<float>(ref _horizOffset, value))
                {
                    watermarkData.HorizOffset = _horizOffset;
                }
            }
        }

        private string _postScriptName;
        public string PostScriptName
        {
            get => _postScriptName;
            set
            {
                if (UpdateProper<string>(ref _postScriptName, value))
                {
                    watermarkData.FontName = _postScriptName;
                }
            }
        }

        public delegate void WindowClosedEventHandler(object sender, WatermarkData watermarkData); 
        public event WindowClosedEventHandler WindowClosed;
        private WeakReference weakReference;

        public WatermarkDialog()
        {
            DataContext = this;
            InitializeComponent();
            Title = LanguageHelper.SecurityManager.GetString("Title_AddWatermark");
        }

        public bool InitWithFileInfo(FileInfoWithRange fileInfo)
        {
            if (fileInfo == null)
            {
                return false;
            }
            InitPreviewControl(fileInfo);
            return true;
        }

        private void InitPreviewControl(FileInfoWithRange fileInfo)
        {
            PreviewControl.Document = fileInfo.Document;
            PreviewControl.MarkedPageList = fileInfo.PageRangeList;
            PreviewControl.PageRangeList = CommonHelper.GetDefaultPageList(PreviewControl.Document);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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



        private void FontSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as ComboBox;
            if (item != null)
            {
                TextSize = (item.SelectedItem as ComboBoxItem).Tag.ToString();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            ColorPickerControl.SetIsChecked(1);
            InitWatermarkData(ref watermarkData);
            PreviewControl.WatermarkData = watermarkData;
            watermarkData.ValueChanged += 
                (s, e) => PreviewControl.WatermarkData = watermarkData;
            weakReference = new WeakReference(this);

            FontFamilyCmb.ItemsSource = CPDFFont.GetFontNameDictionary().Keys;
            // 如果存在helvetica,arial,times,则默认选择
            if (FontFamilyCmb.Items.Contains("Helvetica"))
            {
                FontFamilyCmb.SelectedItem = "Helvetica";
            }
            else if (FontFamilyCmb.Items.Contains("Arial"))
            {
                FontFamilyCmb.SelectedItem = "Arial";
            }
            else if (FontFamilyCmb.Items.Contains("Times"))
            {
                FontFamilyCmb.SelectedItem = "Times";
            }
            else
            {
                FontFamilyCmb.SelectedIndex = 0;
            }
        }

        private void InitWatermarkData(ref WatermarkData watermarkData)
        {
            watermarkData.Type = WatermarkType;
            watermarkData.Text = WatermarkText;
            watermarkData.FontName = FontName;
            watermarkData.FontSize = float.Parse(TextSize);
            watermarkData.Color = Brush;
            watermarkData.ImagePath = ImagePath;
            watermarkData.ImageScale = int.Parse(ImageScale);
            watermarkData.Rotation = Rotation;
            watermarkData.Opacity = OpacityValue;
            watermarkData.Align = SelectedTag;
            watermarkData.VertOffset = VertOffset;
            watermarkData.IsFront = IsFront;
            watermarkData.IsFullScreen = IsFullScreen;
            watermarkData.VerticalSpacing = VerticalSpacing;
            watermarkData.HorizontalSpacing = HorizontalSpacing;

        }

        private void ImagePathBtn_Click(object sender, RoutedEventArgs e)
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty("image (*.jpg;*.png;*.bmp;*.jpeg;*.gif;*.tiff;)|*.jpg;*.png;*.bmp;*.jpeg;*.gif;*.tiff;");
            if (!string.IsNullOrEmpty(filePath))
            {
                ImagePath = filePath;
            }
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            var opacityControl = sender as CPDFOpacityControl;
            if (opacityControl != null)
            {
                OpacityValue = (byte)(opacityControl.OpacityValue / 100.0 * 255.0);
            }
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            var colorPicker = sender as ColorPickerControl;
            if (colorPicker != null)
            {
                Brush = CommonHelper.ConvertBrushToByteArray(colorPicker.Brush);
            }
        }

        private void TypeRdo_Checked(object sender, RoutedEventArgs e)
        {
            var rdo = sender as RadioButton;
            if (rdo != null)
            {
                if (rdo.Name == "TextRdo")
                {
                    WatermarkType = C_Watermark_Type.WATERMARK_TYPE_TEXT;
                }
                else if (rdo.Name == "ImageRdo")
                {
                    WatermarkType = C_Watermark_Type.WATERMARK_TYPE_IMG;
                }
                else
                {
                    watermarkData.Type = C_Watermark_Type.WATERMARK_TYPE_UNKNOWN;
                }
            }
        }

        private void CPDFRotationControl_RotationChanged(object sender, EventArgs e)
        {
            var rotationControl = sender as CPDFRotationControl;
            if (rotationControl != null)
            {
                Rotation = int.Parse(rotationControl.RotationText);
            }
        }

        private void CPDFTileControl_HorizontalSpacingChanged(object sender, EventArgs e)
        {
            var tileControl = sender as CPDFTileControl;
            if (tileControl != null)
            {
                HorizontalSpacing = float.Parse(tileControl.HorizontalSpacingValue);
            }
        }

        private void CPDFTileControl_VerticalSpacingChanged(object sender, EventArgs e)
        {
            var tileControl = sender as CPDFTileControl;
            if (tileControl != null)
            {
                VerticalSpacing = float.Parse(tileControl.VerticalSpacingValue);
            }
        }

        private void CPDFTileControl_FullScreenChanged(object sender, EventArgs e)
        {
            var tileControl = sender as CPDFTileControl;
            if (tileControl != null)
            {
                IsFullScreen = tileControl.IsFullScreen;
            }
        }

        private void CPDFLocationControl_HorizOffsetChanged(object sender, EventArgs e)
        {
            var locationControl = sender as CPDFLocationControl;
            if (locationControl != null)
            {
 
                HorizOffset = (float)(float.Parse(locationControl.HorizOffsetValue) / 25.4 * 72.0);
            }
        }

        private void CPDFLocationControl_VertOffsetChanged(object sender, EventArgs e)
        {
            var locationControl = sender as CPDFLocationControl;
            if (locationControl != null)
            {
                VertOffset = (float)(float.Parse(locationControl.VertOffsetValue) / 25.4 *72);
            }
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if(watermarkData.Type == C_Watermark_Type.WATERMARK_TYPE_TEXT && string.IsNullOrEmpty(watermarkData.Text))
            {
                MessageBox.Show(LanguageHelper.SecurityManager.GetString("Warn_EmptyWatermarkText"), "Warning", MessageBoxButton.OK);
                return;
            }
            if (watermarkData.Type == C_Watermark_Type.WATERMARK_TYPE_IMG && string.IsNullOrEmpty(watermarkData.ImagePath))
            {
                MessageBox.Show(LanguageHelper.SecurityManager.GetString("Warn_EmptyWatermarkText"), "Warning", MessageBoxButton.OK);
                return;
            }
            this.Close();
            WindowClosed?.Invoke(weakReference.Target,  watermarkData);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            PreviewControl.DeleteDisplayWatermark();
            base.OnClosed(e);
        }       

        private void LocationRdo_Checked(object sender, RoutedEventArgs e)
        {
            var locationChk = sender as RadioButton;
            if (locationChk != null)
            {
                if (locationChk.Name == "FrontRdo")
                {
                    IsFront = true;
                }
                else
                {
                    IsFront = false;
                }
            }
        }

        private void FontFamilyCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontStyleCmb.ItemsSource = CPDFFont.GetFontNameDictionary()[FontFamilyCmb.SelectedItem.ToString()];
            FontStyleCmb.SelectedIndex = 0;
            string postScriptName = string.Empty;
            CPDFFont.GetPostScriptName(FontFamilyCmb.SelectedItem.ToString(), FontStyleCmb?.SelectedItem?.ToString(), ref postScriptName);
            PostScriptName = postScriptName;
        }

        private void FontStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string postScriptName = string.Empty;
            CPDFFont.GetPostScriptName(FontFamilyCmb.SelectedValue.ToString(), FontStyleCmb.SelectedValue?.ToString(), ref postScriptName);
            PostScriptName = postScriptName;

        }
    }
}
