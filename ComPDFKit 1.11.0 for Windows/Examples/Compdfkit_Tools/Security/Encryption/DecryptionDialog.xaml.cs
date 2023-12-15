using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using Compdfkit_Tools.Common;
using Compdfkit_Tools.Helper;
using ComPDFKit.PDFDocument;

namespace Compdfkit_Tools.PDFControl
{
    public partial class DecryptionDialog : Window, INotifyPropertyChanged
    {
        private List<string> failedList = new List<string>();
        
        public event PropertyChangedEventHandler PropertyChanged;
        public DecryptionDialog()
        {
            InitializeComponent();
            DataContext = this;
            Title = LanguageHelper.SecurityManager.GetString("Title_RemoveSecurity");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            var savePath = dialog.SelectedPath;
            if (string.IsNullOrEmpty(savePath)) return;
            
            foreach (var fileInfoData in FileListControl.FileInfoDataList)
            {
                try
                {
                    var fullSavePath = savePath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileInfoData.FileName) + "_Encrypted.pdf";
                    if(!fileInfoData.Document.Decrypt(fullSavePath))
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
                var message = "Failed to remove password for the following files:\n" + string.Join("\n", failedList);
                System.Windows.MessageBox.Show(message, "Failed to remove password", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if(failedList.Count < FileListControl.FileInfoDataList.Count)
            {
                System.Diagnostics.Process.Start(savePath);
            }
            this.Close();
        }

        

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}