using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Compdfkit_Tools.Common;
using Compdfkit_Tools.Helper;
using ComPDFKit.PDFDocument;
using Microsoft.Win32;
using PasswordBoxPlus.Helper;

namespace Compdfkit_Tools.PDFControl
{
    public partial class FileGridListControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsEnsureProperty =
            DependencyProperty.Register(nameof(IsEnsure), typeof(bool), typeof(FileGridListControl), new PropertyMetadata(false));
        public bool IsEnsure
        {
            get => (bool)GetValue(IsEnsureProperty);
            private set
            {
                SetValue(IsEnsureProperty, value);
            }
        }

        public class FileInfoData
        {
            public string FileName { get; set; }
            public string Size { get; set; }
            public string Location { get; set; }

            public CPDFDocument Document { get; set; }
        };

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

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<FileInfoData> _fileInfoDataList;

        public ObservableCollection<FileInfoData> FileInfoDataList
        {
            get
            {
                return _fileInfoDataList;
            }
            set
            {
                _fileInfoDataList = value;
                _fileInfoDataList.CollectionChanged += (sender, args) =>
                {
                    FileNumText = _fileInfoDataList.Count;
                };
            }
        }

        public FileGridListControl()
        {
            InitializeComponent();
            FileInfoDataList = new ObservableCollection<FileInfoData>();
            DataContext = this;
            FileDataGrid.ItemsSource = FileInfoDataList;
            FileInfoDataList.Clear();
        }

        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = @"PDF Files (*.pdf)|*.pdf";
            dialog.ShowDialog();

            foreach (var fileName in dialog.FileNames)
            {
                var file = new FileInfo(fileName);
                var fileInfoData = new FileInfoData()
                {
                    FileName = file.Name,
                    Size = CommonHelper.GetFileSize(fileName),
                    Location = file.FullName
                };

                if (FileInfoDataList.Any(item => item.Location == fileInfoData.Location))
                {
                    continue;
                }

                var document = CPDFDocument.InitWithFilePath(fileInfoData.Location);
                if (document == null)
                {
                    continue;
                }
                
                if (PasswordHelper.IsOwnerLocked(document))
                {
                    if(!PasswordHelper.UnlockWithOwnerPassword(document))
                    {
                        document.Release();
                        continue;
                    }
                }
                
                fileInfoData.Document = document;
                FileInfoDataList.Add(fileInfoData);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (FileDataGrid.SelectedItems.Count == 0)
            {
                foreach (var item in FileInfoDataList)
                {
                    item.Document.Release();
                }
                FileInfoDataList.Clear();
                OnPropertyChanged("FileNum");
                return;
            }
            var selectedItems = FileDataGrid.SelectedItems;
            var selectedItemsList = selectedItems.Cast<FileInfoData>().ToList();

            foreach (var item in selectedItemsList)
            {
                item.Document.Release();
                FileInfoDataList.Remove(item);
            }
            OnPropertyChanged("FileNum");
        }

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

        private void FileDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnRemove.Content = FileDataGrid.SelectedItems.Count > 0 ? LanguageHelper.SecurityManager.GetString("Button_RemoveSelected") : LanguageHelper.SecurityManager.GetString("Button_RemoveAll");
        }

        private void FileDataGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FileDataGrid.UnselectAll();
        }
    }
}