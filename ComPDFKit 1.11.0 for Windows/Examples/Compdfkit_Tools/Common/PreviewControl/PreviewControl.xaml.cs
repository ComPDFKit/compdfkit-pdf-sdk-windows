using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Compdfkit_Tools.PDFControl
{
    /// <summary>
    /// Interaction logic for PreviewControl.xaml
    /// </summary>
    public partial class PreviewControl : UserControl, INotifyPropertyChanged
    {
        private Point lastMousePosition;
        private double startVerticalOffset;
        private double startHorizontalOffset;

        private List<int> _pageIndexList = new List<int>();
        public List<int> PageRangeList
        {
            get => _pageIndexList;
            set
            {
                CurrentIndex = 1;
                _pageIndexList = value;
                PageCount = _pageIndexList.Count;
                PageRangeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _pageCount = 1;
        public int PageCount
        {
            get => _pageCount;
            set
            {
                UpdateProper(ref _pageCount, value);
            }
        }

        private int _currentIndex = 1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                else if (value > PageCount)
                {
                    value = PageCount;
                }
                 
                if (UpdateProper(ref _currentIndex, value))
                {
                    OnCurrentIndexChanged();
                }
            }
        }

        private void OnCurrentIndexChanged()
        {
            CurrentIndexChanged?.Invoke(this, EventArgs.Empty);
        }

        private double _scale = 0.3;
        public double Scale
        {
            get => _scale;
            set
            {
                UpdateProper(ref _scale, Math.Min((Math.Max(value, 0.1)), 1));
            }
        }

        private WriteableBitmap _imageSource;
        public WriteableBitmap ImageSource
        {
            get => _imageSource;
            set
            {
                UpdateProper(ref _imageSource, value);
            }
        }

        private CPDFDocument _document;
        public CPDFDocument Document
        {
            get { return _document; }
            set
            {
                _document = value;
            }
        }

        protected double aspectRatio;
        protected double thumbnailWidth;
        protected double thumbnailHeight;

        public event EventHandler CurrentIndexChanged;
        public event EventHandler PageRangeChanged;


        public PreviewControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void InitPreview(CPDFDocument document)
        {
            Document = document;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastMousePosition = e.GetPosition(ImageSv);
            startVerticalOffset = ImageSv.VerticalOffset;
            startHorizontalOffset = ImageSv.HorizontalOffset;
            Image.CaptureMouse();
            this.Cursor = Cursors.Hand;
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image.ReleaseMouseCapture();
            this.Cursor = Cursors.Arrow;
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (ImageSv.Visibility == Visibility.Visible && Image.IsMouseCaptured)
            {
                Point currentMousePosition = e.GetPosition(ImageSv);
                double deltaVerticalOffset = lastMousePosition.Y - currentMousePosition.Y;
                double deltaHorizontalOffset = lastMousePosition.X - currentMousePosition.X;

                double newVerticalOffset = startVerticalOffset + deltaVerticalOffset;
                double newHorizontalOffset = startHorizontalOffset + deltaHorizontalOffset;

                ImageSv.ScrollToVerticalOffset(newVerticalOffset);
                ImageSv.ScrollToHorizontalOffset(newHorizontalOffset);
            }
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

        private void CurrentIndexTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]$"))
                {
                    e.Handled = true;
                }
            }
        }

        private Thread renderThread = null;
        public virtual void BeginLoadImageThread(int pageIndex)
        {
            if (renderThread != null && renderThread.ThreadState == ThreadState.Running)
                return;
            renderThread = new Thread(new ParameterizedThreadStart(LoadImage));
            if(Document != null)
            {
                renderThread.Start(Document.PageAtIndex(pageIndex));
            }
        }

        protected readonly object queueLock = new object();
        protected void LoadImage(object pageObject)
        {
            CPDFPage pdfPage = (CPDFPage)pageObject;
            Size pageSize = pdfPage.PageSize;
            double ratio = CalculateThumbnailSize(pageSize) * 3;
            Rect pageRect = new Rect(0, 0, (int)(pageSize.Width * ratio), (int)(pageSize.Height * ratio));
            byte[] bmpData = new byte[(int)(pageRect.Width * pageRect.Height * 4)];
            lock (queueLock)
            {
                if(pdfPage.IsValid() == false)
                {
                    pdfPage = Document.PageAtIndex(pdfPage.PageIndex);
                }
                pdfPage.RenderPageBitmapWithMatrix((float)ratio, pageRect, 0xFFFFFFFF, bmpData, 0, true);
            }
            WriteableBitmap writeableBitmap = new WriteableBitmap((int)pageRect.Width, (int)pageRect.Height, 96, 96, PixelFormats.Bgra32, null);
            writeableBitmap.WritePixels(new Int32Rect(0, 0, (int)pageRect.Width, (int)pageRect.Height), bmpData, writeableBitmap.BackBufferStride, 0);
            writeableBitmap.Freeze();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            { 
                ImageSource = writeableBitmap;

            }));
        }

        private double CalculateThumbnailSize(Size size)
        {

            if (size.Height / size.Width > aspectRatio)
            {
                return (thumbnailWidth) / size.Width;
            }
            else
            {
                return (thumbnailHeight) / size.Height;

            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            if (Document == null)
            {
                return;
            }
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    aspectRatio = ImageGd.ActualHeight / ImageGd.ActualWidth;
                    thumbnailWidth = ImageGd.ActualWidth - 20;
                    thumbnailHeight = ImageGd.ActualHeight - 20;
                });
                CurrentIndexChanged += (s, e) =>
                {
                    BeginLoadImageThread(PageRangeList[CurrentIndex - 1] - 1);
                };
                PageRangeChanged += (s, e) =>
                {
                    BeginLoadImageThread(PageRangeList[CurrentIndex - 1] - 1);
                };
                AttachLoaded();
            }
            catch (Exception ex)
            {
            }
        }

        virtual public void AttachLoaded()
        {
            BeginLoadImageThread(0);
        }

        private void CurrentIndexTxt_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (e.Command == ApplicationCommands.Paste && Clipboard.ContainsText())
                {
                    string checkText = Clipboard.GetText();
                    if (int.TryParse(checkText, out int value))
                    {
                        e.CanExecute = true;
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void PageBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                if (button.Name == "PrePageBtn")
                {
                    CurrentIndex--;
                }
                else
                {
                    CurrentIndex++;
                }
            }
        }

        private void ScaleBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                if (button.Name == "ZoomInBtn")
                {
                    Scale -= 0.1;
                }
                else
                {
                    Scale += 0.1;
                }
            }
        }
    }
}
