using Compdfkit_Tools.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using RadioButton = System.Windows.Controls.RadioButton;

namespace Compdfkit_Tools.PDFControl
{
    /// <summary>
    /// Interaction logic for PageRangeDialog.xaml
    /// </summary>
    public partial class PageRangeDialog : Window, INotifyPropertyChanged
    {
        private bool canContinue = true;

        private int pageCount = 0;

        private List<int> defaultPageList = new List<int>(); 
         
        private List<int> _pageIndexList = new List<int>();
        public List<int> PageIndexList
        {
            get => _pageIndexList;
            set
            {
                _pageIndexList = value;
                PreviewControl.PageRangeList = _pageIndexList;
            }
        }

        private bool _isEvenEnable = false;
        public bool IsEvenEnable
        {
            get => _isEvenEnable;
            set
            {
                UpdateProper(ref _isEvenEnable, value);
            }
        }

        private string _pageRange;
        public string PageRange
        {
            get => _pageRange;
            set
            {
                if (fileInfo != null & UpdateProper(ref _pageRange, value))
                {
                    List<int> list = new List<int>();
                    canContinue = CommonHelper.GetPagesInRange(ref list, PageRange, fileInfo.Document.PageCount, new char[] { ',' }, new char[] { '-' });
                    if (canContinue)
                    {
                        List<int> newList = list.Select(item => item + 1).ToList();
                        PageIndexList = newList;
                    }
                    else
                    {
                        PageIndexList = fileInfo.PageRangeList;
                    }
                }
            }
        }
         
        private FileInfoWithRange fileInfo;

        private WeakReference weakReference;
        public delegate void WindowClosedEventHandler(object sender, List<int> result);
        public event WindowClosedEventHandler WindowClosed;

        public PageRangeDialog()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        public void InitWithFileInfo(FileInfoWithRange fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!canContinue)
            {
                MessageBox.Show("Please enter the right page range", 
                    LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            WindowClosed?.Invoke(weakReference.Target, PageIndexList);
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowClosed?.Invoke(weakReference.Target, null);
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            fileInfo.Document.ReleasePages();
            base.OnClosed(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            weakReference = new WeakReference(this);
            PreviewControl.InitPreview(fileInfo.Document);
            defaultPageList = CommonHelper.GetDefaultPageList(fileInfo.Document);
            PageIndexList = defaultPageList;
            pageCount = fileInfo.Document.PageCount;
            if (pageCount <= 1)
            {
                IsEvenEnable = false;
            }
            else
            {
                IsEvenEnable = true;
            }
        }

        private void RangeRdo_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null && fileInfo != null)
            {
                canContinue = true;
                switch (radioButton.Tag)
                {
                    case "All":
                        PageIndexList = defaultPageList;
                        break;
                    case "Odd":
                        PageIndexList = defaultPageList.Where(value => value % 2 != 0).ToList();
                        break;
                    case "Even":
                        if (defaultPageList.Count > 1)
                        {
                            PageIndexList = defaultPageList.Where(value => value % 2 == 0).ToList();
                        }
                        break;
                    case "Custom":
                        List<int> list = new List<int>();
                        canContinue = CommonHelper.GetPagesInRange(ref list, PageRange, fileInfo.Document.PageCount, new char[] { ',' }, new char[] { '-' }, false);
                        if (canContinue)
                        {
                            // Increment each element in the list by 1
                            for (int i = 0; i < list.Count; i++)
                            {
                                list[i]++;
                            }
                            PageIndexList = list;
                        }
                        else
                        {
                            PageIndexList = defaultPageList;
                        }
                        break;
                    default:
                        break;
                }
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
    }
}
