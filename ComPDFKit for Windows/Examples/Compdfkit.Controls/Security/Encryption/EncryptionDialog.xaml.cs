using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Helper;
using ComPDFKit.PDFDocument;
using MessageBox = System.Windows.MessageBox;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class EncryptionDialog : Window, INotifyPropertyChanged
    {
        private int _currentFileNum;
        public int CurrentFileNum
        {
            get => _currentFileNum;
            set => UpdateProper(ref _currentFileNum, value);
        }

        private int _fileNum;
        public int FileNum
        {
            get => _fileNum;
            set => UpdateProper(ref _fileNum, value);
        }

        private bool isEnsure;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool CanEncrypt
        {
            get { return FileListControl.FileInfoDataList.Count > 0 && SetEncryptionControl.IsSettingValid; }
        }

        private List<string> failedList = new List<string>();
        public EncryptionDialog()
        {
            InitializeComponent();
            DataContext = this;
            CurrentFileNum = 0;
            Title = LanguageHelper.SecurityManager.GetString("Title_Security");
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonEncrypt_Click(object sender, RoutedEventArgs e)
        {
            if (SetEncryptionControl.IsUserPasswordEnabled && SetEncryptionControl.IsOwnerPasswordEnabled)
            {
                if(SetEncryptionControl.UserPassword == SetEncryptionControl.OwnerPassword)
                {
                    MessageBox.Show(LanguageHelper.SecurityManager.GetString("Warn_SamePassword"),
                        LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var savePath = dialog.SelectedPath;
            if (string.IsNullOrEmpty(savePath)) return;

            CurrentFileNum = 0;
            var permissionsInfo = new CPDFPermissionsInfo();
            string userPassword = string.Empty;
            string ownerPassword = string.Empty;

            if (SetEncryptionControl.IsOwnerPasswordEnabled && !string.IsNullOrEmpty(SetEncryptionControl.OwnerPassword))
            {
                permissionsInfo.AllowsPrinting = SetEncryptionControl.IsAllowPrint;
                permissionsInfo.AllowsCopying = SetEncryptionControl.IsAllowCopy;
                ownerPassword = SetEncryptionControl.OwnerPassword;
            }
            if (SetEncryptionControl.IsUserPasswordEnabled && !string.IsNullOrEmpty(SetEncryptionControl.UserPassword))
            {
                userPassword = SetEncryptionControl.UserPassword;
            }
            foreach (var fileInfoData in FileListControl.FileInfoDataList)
            {
                try
                {
                    var fullSavePath = savePath + Path.DirectorySeparatorChar +
                                       Path.GetFileNameWithoutExtension(fileInfoData.FileName) + "_Encrypted.pdf";
                    fileInfoData.Document.Encrypt(userPassword, ownerPassword, permissionsInfo,
                        (CPDFDocumentEncryptionLevel)SetEncryptionControl.CryptographicLevel);
                    if (!fileInfoData.Document.WriteToFilePath(fullSavePath))
                    {
                        failedList.Add(fileInfoData.Location);
                    }
                    fileInfoData.Document.Release();
                }
                catch (Exception exception)
                {
                    failedList.Add(fileInfoData.Location);
                }
            }
            if (failedList.Count > 0)
            {
                System.Windows.MessageBox.Show("Failed to encrypt the following files:\n" + string.Join("\n", failedList));
            }
            if (failedList.Count < FileListControl.FileInfoDataList.Count)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + savePath + Path.DirectorySeparatorChar +
                    Path.GetFileNameWithoutExtension(FileListControl.FileInfoDataList[0].FileName) + "_Encrypted.pdf");
            }
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var fileInfo in FileListControl.FileInfoDataList)
            {
                fileInfo.Document.Release();
            }
            base.OnClosed(e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void UpdateProper<T>(ref T properValue,
            T newValue,
            [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return;

            properValue = newValue;
            OnPropertyChanged(properName);
        }
    }
}