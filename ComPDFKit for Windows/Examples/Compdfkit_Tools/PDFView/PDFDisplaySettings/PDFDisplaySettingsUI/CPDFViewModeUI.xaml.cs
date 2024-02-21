using ComPDFKitViewer;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFViewModeUI : UserControl
    {
        public event EventHandler<string> SetContinuousEvent;
        public event EventHandler<string> SetViewModeEvent;

        public event EventHandler<SplitMode> SplitModeChanged;

        public event EventHandler<ViewMode> ViewModeChanged;

        public event EventHandler<bool> CropModeChanged;


        private SolidColorBrush ActivePathBrush = new SolidColorBrush(Color.FromRgb(0x14,0x60,0xF3));

        private SolidColorBrush NormalPathBrush = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));
        public CPDFViewModeUI()
        {
            InitializeComponent(); 
        }

        private void ViewModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var viewModeRadioButton = sender as RadioButton;
            SetViewModeEvent?.Invoke(sender, viewModeRadioButton.Tag as string);
        }

        private void ContinuousRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var continuousRadioButton = sender as RadioButton;
            SetContinuousEvent?.Invoke(sender,(continuousRadioButton.Tag as string));
        }

        private void HorizonBtn_Click(object sender, RoutedEventArgs e)
        {
            VerticalBtn.IsChecked = false;
            HorizonPath.Fill = HorizonBtn.IsChecked == true ? ActivePathBrush : NormalPathBrush;
            VerticalPath.Fill = NormalPathBrush;
            SplitModeChanged?.Invoke(this, HorizonBtn.IsChecked == true ? SplitMode.Horizontal:SplitMode.None);
        }

        private void VerticalBtn_Click(object sender, RoutedEventArgs e)
        {
            HorizonBtn.IsChecked = false;
            HorizonPath.Fill = NormalPathBrush;
            VerticalPath.Fill = VerticalBtn.IsChecked == true ? ActivePathBrush : NormalPathBrush;
            SplitModeChanged?.Invoke(this, VerticalBtn.IsChecked == true ? SplitMode.Vertical : SplitMode.None);
        }

        private void ClearViewState()
        {
            List<ToggleButton> viewBtnList = new List<ToggleButton>()
            {
                SingleViewBtn,
                DoubleViewBtn,
                BookViewBtn,
            };

            List<Path> viewPathList = new List<Path>()
            {
                BookViewPath,
                DoubleViewPath,
                SingleViewPath
            };

            foreach (ToggleButton item in viewBtnList)
            {
                item.IsChecked = false;
            }

            foreach(Path path in viewPathList)
            {
                path.Fill = NormalPathBrush;
            }
        }

        private void BookViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearViewState();
            BookViewBtn.IsChecked = true;
            BookViewPath.Fill = ActivePathBrush;
            ViewModeChanged?.Invoke(this, ContinuePageBtn.IsChecked==true? ViewMode.BookContinuous:ViewMode.Book);
        }

        private void DoubleViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearViewState();
            DoubleViewBtn.IsChecked = true;
            DoubleViewPath.Fill = ActivePathBrush;
            ViewModeChanged?.Invoke(this, ContinuePageBtn.IsChecked == true ? ViewMode.DoubleContinuous:ViewMode.Double);
        }

        private void SingleViewBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearViewState();
            SingleViewBtn.IsChecked = true;
            SingleViewPath.Fill = ActivePathBrush;
            ViewModeChanged?.Invoke(this, ContinuePageBtn.IsChecked == true ? ViewMode.SingleContinuous:ViewMode.Single);
        }

        private void CropModeBtn_Click(object sender, RoutedEventArgs e)
        {
            CropModeChanged?.Invoke(this, CropModeBtn.IsChecked == true);
        }

        private void ContinuePageBtn_Click(object sender, RoutedEventArgs e)
        {
            if(SingleViewBtn.IsChecked==true)
            {
                ViewModeChanged?.Invoke(this, ContinuePageBtn.IsChecked == true ? ViewMode.SingleContinuous : ViewMode.Single);
                return;
            }

            if (DoubleViewBtn.IsChecked == true)
            {
                ViewModeChanged?.Invoke(this, ContinuePageBtn.IsChecked == true ? ViewMode.DoubleContinuous : ViewMode.Double);
                return;
            }

            if(BookViewBtn.IsChecked == true)
            {
                ViewModeChanged?.Invoke(this, ContinuePageBtn.IsChecked == true ? ViewMode.BookContinuous : ViewMode.Book);
            }
        }

        public void SetSplitModeUI(SplitMode mode)
        {
            switch(mode)
            {
                case SplitMode.None:
                    HorizonBtn.IsChecked = false;
                    VerticalBtn.IsChecked = false;
                    HorizonPath.Fill = NormalPathBrush;
                    VerticalPath.Fill = NormalPathBrush;
                    break;
                case SplitMode.Vertical:
                    VerticalBtn.IsChecked = true;
                    HorizonBtn.IsChecked = false;
                    HorizonPath.Fill = NormalPathBrush;
                    VerticalPath.Fill = ActivePathBrush;
                    break;
                case SplitMode.Horizontal:
                    VerticalBtn.IsChecked = false;
                    HorizonBtn.IsChecked = true;
                    HorizonPath.Fill = ActivePathBrush;
                    VerticalPath.Fill = NormalPathBrush;
                    break;
                default:
                    break;
            }
        }

        public void SetViewModeUI(ViewMode mode)
        {
            ClearViewState();

            switch (mode)
            {
                case ViewMode.Book:
                    BookViewBtn.IsChecked = true;
                    BookViewPath.Fill = ActivePathBrush;
                    ContinuePageBtn.IsChecked = false;
                    break;
                case ViewMode.BookContinuous:
                    BookViewBtn.IsChecked = true;
                    BookViewPath.Fill = ActivePathBrush;
                    ContinuePageBtn.IsChecked = true;
                    break;
                case ViewMode.Single:
                    SingleViewBtn.IsChecked = true;
                    SingleViewPath.Fill = ActivePathBrush;
                    ContinuePageBtn.IsChecked = false;
                    break;
                case ViewMode.SingleContinuous:
                    SingleViewBtn.IsChecked = true;
                    SingleViewPath.Fill = ActivePathBrush;
                    ContinuePageBtn.IsChecked = true;
                    break;
                case ViewMode.Double:
                    DoubleViewBtn.IsChecked = true;
                    DoubleViewPath.Fill = ActivePathBrush;
                    ContinuePageBtn.IsChecked = false;
                    break;
                case ViewMode.DoubleContinuous:
                    DoubleViewBtn.IsChecked = true;
                    DoubleViewPath.Fill = ActivePathBrush;
                    ContinuePageBtn.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        public void SetCropUI(bool isAutoCrop)
        {
            CropModeBtn.IsChecked=isAutoCrop;
        }



        public void SetSplitContainerVisibility(Visibility visibility)
        {
            SplitContainer.Visibility = visibility;
            if (visibility == Visibility.Visible)
            {
                DisplayContainer.Margin = new Thickness(0,30,0,0);
            }
            else
            {
                DisplayContainer.Margin = new Thickness(0);
            } 
        }

        public void SetCropContainerVisibility(Visibility visibility)
        {
            CropContainer.Visibility = visibility;
            if (visibility == Visibility.Visible)
            {
                DisplayContainer.Margin = new Thickness(0, 30, 0, 0);
            }
            else
            {
                DisplayContainer.Margin = new Thickness(0);
            }
        }
    }
}
