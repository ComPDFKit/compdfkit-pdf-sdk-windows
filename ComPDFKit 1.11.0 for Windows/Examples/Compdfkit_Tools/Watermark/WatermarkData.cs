using ComPDFKit.PDFWatermark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControl
{

    public class WatermarkData
    {
        public event EventHandler ValueChanged;

        private C_Watermark_Type _type;
        public C_Watermark_Type Type
        {
            get => _type;
            set => UpdateProper(ref _type, value);
        }

        //Text--------------
        private string _text;
        public string Text
        {
            get => _text;
            set => UpdateProper(ref _text, value);
        }

        private string _fontName;
        public string FontName
        {
            get => _fontName;
            set => UpdateProper(ref _fontName, value);
        }

        private float _fontSize;
        public float FontSize
        {
            get => _fontSize;
            set => UpdateProper(ref _fontSize, value);
        }

        private byte[] _color;
        public byte[] Color
        {
            get => _color;
            set => UpdateProper(ref _color, value);
        }
        //----------------

        //Image---------
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set => UpdateProper(ref _imagePath, value);
        }

        private int _imageScale;
        public int ImageScale
        {
            get => _imageScale;
            set => UpdateProper(ref _imageScale, value);
        }
        //----------------

        private int _rotation;
        public int Rotation
        {
            get => _rotation;
            set => UpdateProper(ref _rotation, value);
        }

        private byte _opacity;
        public byte Opacity
        {
            get => _opacity;
            set => UpdateProper(ref _opacity, value);
        }

        private int _align;
        public int Align
        {
            get => _align;
            set => UpdateProper(ref _align, value);
        }

        private float _vertOffset;
        public float VertOffset
        {
            get => _vertOffset;
            set => UpdateProper(ref _vertOffset, value);
        }

        private float _horizOffset;
        public float HorizOffset
        {
            get => _horizOffset;
            set => UpdateProper(ref _horizOffset, value);
        }

        private bool _isFront;
        public bool IsFront
        {
            get => _isFront;
            set => UpdateProper(ref _isFront, value);
        }

        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get => _isFullScreen;
            set => UpdateProper(ref _isFullScreen, value);
        }

        private float _verticalSpacing;
        public float VerticalSpacing
        {
            get => _verticalSpacing;
            set => UpdateProper(ref _verticalSpacing, value);
        }

        private float _horizontalSpacing;
        public float HorizontalSpacing
        {
            get => _horizontalSpacing;
            set => UpdateProper(ref _horizontalSpacing, value);
        }


        public WatermarkData()
        {

        }

        protected void OnValueChanged(string propertyName = null)
        {
            ValueChanged?.Invoke(this, new EventArgs());
        }

        protected bool UpdateProper<T>(ref T properValue, T newValue, [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return false;

            properValue = newValue;
            OnValueChanged(properName);
            return true;
        }
    }

    public class TextWatermarkData : WatermarkData
    {

    }

    public class ImageWatermarkData : WatermarkData
    {

    }

}
