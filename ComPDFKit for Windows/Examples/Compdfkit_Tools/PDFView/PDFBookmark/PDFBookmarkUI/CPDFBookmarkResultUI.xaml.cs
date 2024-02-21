using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControlUI
{ 
    public partial class CPDFBookmarkResultUI : UserControl
    {
        public event EventHandler<int> SelectionChanged;

        public event EventHandler<BookmarkChangeData> BookmarkDelete;

        public event EventHandler<BookmarkChangeData> BookmarkEdit;

        public event EventHandler<int> BookmarkClicked;

        private ObservableCollection<BookmarkBindData> bookmarkResults;

        public CPDFBookmarkResultUI()
        {
            InitializeComponent();
            bookmarkResults = new ObservableCollection<BookmarkBindData>();
            ICollectionView groupView = CollectionViewSource.GetDefaultView(bookmarkResults);
            groupView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(BookmarkBindData.ShowPageIndex)));
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid sourceGrid=sender as Grid;
            if(sourceGrid!=null && sourceGrid.Children.Count==2)
            {
                Border sourceBorder = sourceGrid.Children[1] as Border;
                if(sourceBorder!=null)
                {
                    sourceBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid sourceGrid = sender as Grid;
            if (sourceGrid != null && sourceGrid.Children.Count == 2)
            {
                Border sourceBorder = sourceGrid.Children[1] as Border;
                if (sourceBorder != null)
                {
                    sourceBorder.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void EditBorder_Click(object sender, RoutedEventArgs e)
        {
            Border sourceBtn =sender as Border;
            if(sourceBtn != null)
            {
                DependencyObject findElement = null;
                if (FindParent<Grid>(sourceBtn, out findElement) && findElement != null)
                {
                    Grid sourceGrid = findElement as Grid;
                    BookmarkBindData bindData = sourceGrid.Tag as BookmarkBindData;
                    if (bindData != null)
                    {
                        BookmarkEdit?.Invoke(this, new BookmarkChangeData()
                        {
                            PageIndex = bindData.BindProperty.PageIndex,
                            BookmarkTitle = bindData.BindProperty.BookmarkTitle,
                            BindData= bindData
                        });
                    }
                }
            }
            e.Handled = true;
        }

        private void DelBorder_Click(object sender, RoutedEventArgs e)
        {
            Border sourceBtn = sender as Border;
            if (sourceBtn != null)
            {
                DependencyObject findElement = null;
                if (FindParent<Grid>(sourceBtn, out findElement) && findElement != null)
                {
                    Grid sourceGrid = findElement as Grid;
                    BookmarkBindData bindData = sourceGrid.Tag as BookmarkBindData;
                    if (bindData != null)
                    {
                        BookmarkDelete?.Invoke(this, new BookmarkChangeData()
                        {
                            PageIndex = bindData.BindProperty.PageIndex,
                            BookmarkTitle = bindData.BindProperty.BookmarkTitle
                        });
                        bookmarkResults?.Remove(bindData);

                        if(bookmarkResults.Count==0)
                        {
                            NoResultText.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private bool FindParent<T>(DependencyObject checkElement,out DependencyObject parent)
        {
            parent = null;

            try
            {
                if (checkElement != null)
                {
                   DependencyObject parentElement=  VisualTreeHelper.GetParent(checkElement);
                    if(parentElement!=null && parentElement is T)
                    {
                        parent =parentElement;
                        return true;
                    }
                   return FindParent<T>(parentElement,out parent);
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public void SetBookmarkResult(List<BindBookmarkResult> results)
        {
            bookmarkResults?.Clear();
            if (results == null || results.Count == 0)
            {
                ResultListControl.ItemsSource = null;
                NoResultText.Visibility= Visibility.Visible;
                return;
            }

            foreach (BindBookmarkResult item in results)
            {
                bookmarkResults.Add(new BookmarkBindData()
                {
                    BindProperty = item
                });
            }
            ResultListControl.ItemsSource = bookmarkResults;
            NoResultText.Visibility = Visibility.Collapsed;
        }

        public BindBookmarkResult GetSelectItem()
        {
            BookmarkBindData bindData = ResultListControl.SelectedItem as BookmarkBindData;
            if (bindData != null)
            {
                return bindData.BindProperty;
            }

            return null;
        }

        public BindBookmarkResult GetItem(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (ResultListControl.HasItems && ResultListControl.Items.Count > index)
            {
                BookmarkBindData bindData = ResultListControl.Items[index] as BookmarkBindData;
                if (bindData != null)
                {
                    return bindData.BindProperty;
                }
            }

            return null;
        }

        public void ClearSelection()
        {
            int oldSelectionIndex = ResultListControl.SelectedIndex;
            ResultListControl.UnselectAll();
            if (oldSelectionIndex != ResultListControl.SelectedIndex)
            {
                SelectionChanged?.Invoke(this,ResultListControl.SelectedIndex);
            }
        }

        public void SelectItem(int selectIndex)
        {
            if (ResultListControl.SelectedIndex != selectIndex)
            {
                ResultListControl.SelectedIndex = selectIndex;
            }
        }

        private void ResultListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, ResultListControl.SelectedIndex);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid sourceGrid = sender as Grid;
            if (sourceGrid != null)
            {
                BookmarkBindData bindData = sourceGrid.Tag as BookmarkBindData;
                if (bindData != null)
                {
                    BookmarkClicked?.Invoke(this, bindData.BindProperty.PageIndex);
                }
            }
        }

        private void ResultListControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResultListControl?.UnselectAll();
        }
    }

    public class BookmarkChangeData
    {
        public int PageIndex { get; set; }
        public string BookmarkTitle { get; set; }
        public string NewTitle { get;set; }

        public object BindData { get; set; }
    }

    public class BindBookmarkResult:INotifyPropertyChanged
    {
        private int _pageIndex;
        public int PageIndex 
        {
            get
            {
                return _pageIndex;
            }
            set
            {
                if(_pageIndex != value)
                {
                    _pageIndex = value;
                    OnPropertyChanged(nameof(PageIndex));
                }
            }
        }

        private string _bookmarkTitle;
        public string BookmarkTitle 
        {
            get
            {
                return _bookmarkTitle;
            }
            set
            {
                if(_bookmarkTitle != value)
                {
                    _bookmarkTitle = value;
                    OnPropertyChanged(nameof(BookmarkTitle));
                }
               
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class BookmarkBindData
    {
        public int ShowPageIndex { get { return BindProperty.PageIndex + 1; } set { BindProperty.PageIndex = value; } }
        public BindBookmarkResult BindProperty { get; set; }
        public BookmarkBindData()
        {
            BindProperty = new BindBookmarkResult();
        }
    }
}
