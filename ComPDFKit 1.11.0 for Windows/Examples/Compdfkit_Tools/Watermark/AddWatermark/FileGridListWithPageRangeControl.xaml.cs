using ComPDFKit.PDFDocument;
using Compdfkit_Tools.Common;
using Compdfkit_Tools.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Compdfkit_Tools.PDFControl.FileGridListControl;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using UserControl = System.Windows.Controls.UserControl;

namespace Compdfkit_Tools.PDFControl
{ 
    public class FileInfoWithRange : INotifyPropertyChanged
    {
        public CPDFDocument Document;
        public string Name { get; set; }
        public string Path { get; set; }
        public string Size { get; set; }
         
        private string _pageRange;
        public string PageRange { get => _pageRange; set => UpdateProper(ref _pageRange, value); }

        private List<int> _pageRangeList = new List<int>();
        public List<int> PageRangeList
        {
            get => _pageRangeList;
            set
            {
                _pageRangeList = value;
                PageRange = CommonHelper.GetPageParmFromList(PageRangeList);
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

    public partial class FileGridListWithPageRangeControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsEnsureProperty =
    DependencyProperty.Register(nameof(IsEnsure), typeof(bool), typeof(FileGridListWithPageRangeControl), new PropertyMetadata(false));


        public bool IsEnsure
        {
            get => (bool)GetValue(IsEnsureProperty);
            private set
            {
                SetValue(IsEnsureProperty, value);
            }
        }


        private int _fileNumText;
        public int FileNumText
        {
            get => _fileNumText;
            set
            {
                if (UpdateProper(ref _fileNumText, value))
                    IsEnsure = _fileNumText > 0;
            }
        }
         
        public FileInfoWithRange FileForDisplay
        {
            get
            {
                if (FileDataGrid.Items.Count > 0)
                {
                    if (FileDataGrid.SelectedItems.Count > 0)
                    {
                        return FileDataGrid.SelectedItems[0] as FileInfoWithRange;
                    }
                    else
                    {
                        return FileDataGrid.Items[0] as FileInfoWithRange;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        private ObservableCollection<FileInfoWithRange> _fileInfoDataList;
        public ObservableCollection<FileInfoWithRange> FileInfoDataList
        {
            get { return _fileInfoDataList; }
            set
            {
                _fileInfoDataList = value;
                _fileInfoDataList.CollectionChanged += (sender, args) =>
                {
                    FileNumText = _fileInfoDataList.Count;
                };
            }
        }

        public FileGridListWithPageRangeControl()
        {
            this.DataContext = this;
            InitializeComponent();
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

        private void AddFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    CPDFDocument document = CPDFDocument.InitWithFilePath(filePath);
                    if(document == null)
                    {
                        continue;
                    }

                    if (FileInfoDataList.Any(item => item.Path == document.FilePath))
                    {
                        continue;
                    }

                    if (document.IsLocked)
                    {
                        PasswordWindow passwordWindow = new PasswordWindow();
                        passwordWindow.InitWithDocument(document);
                        passwordWindow.Owner = Window.GetWindow(this);
                        passwordWindow.PasswordDialog.SetShowText(filePath + " is encrypted.");
                        passwordWindow.ShowDialog();
                        if (document.IsLocked)
                        {
                            document.Release();
                            continue;
                        } 
                    }

                    List<int> pageRangeList = new List<int>();
                    for (int i = 0; i < document.PageCount; i++)
                    {
                        pageRangeList.Add(i + 1);
                    }

                    FileInfoDataList.Add(new FileInfoWithRange
                    {
                        Document = document,
                        Name = document.FileName,
                        Size = CommonHelper.GetFileSize(filePath),
                        Path = document.FilePath,
                        PageRangeList = pageRangeList
                    });
                }
            }
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FileDataGrid.SelectedItems.Count == 0)
            {
                foreach (var item in FileInfoDataList)
                {
                    item.Document.Release();
                }
                FileInfoDataList.Clear();
                return;
            }
            var selectedItems = FileDataGrid.SelectedItems;
            var selectedItemsList = selectedItems.Cast<FileInfoWithRange>().ToList();

            foreach (var item in selectedItemsList)
            {
                FileInfoDataList.Remove(item);
            }
        }

        private void PageRangeBtn_Click(object sender, RoutedEventArgs e)
        {
            PageRangeDialog pageRangeDialog = new PageRangeDialog();
            if (FileDataGrid.SelectedItems[0] is FileInfoWithRange fileInfo)
            {
                pageRangeDialog.InitWithFileInfo(fileInfo);
            }

            pageRangeDialog.Owner = Window.GetWindow(this);
            pageRangeDialog.WindowClosed += PageRangeDialog_WindowClosed;

            pageRangeDialog.ShowDialog();
        }

        private void PageRangeDialog_WindowClosed(object sender, List<int> result)
        {
            if (result != null)
            {
              (FileDataGrid.SelectedItems[0] as FileInfoWithRange).PageRangeList = result;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FileInfoDataList = new ObservableCollection<FileInfoWithRange>();
            FileDataGrid.ItemsSource = FileInfoDataList;
        }

        private void FileDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
            {
                RemoveBtn.Content = dataGrid.SelectedItems.Count > 0 ? LanguageHelper.SecurityManager.GetString("Button_RemoveSelected") : LanguageHelper.SecurityManager.GetString("Button_RemoveAll");
                PageRangeBtn.Visibility = dataGrid.SelectedItems.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void FileDataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FileDataGrid.UnselectAll();
        }
    }
}
