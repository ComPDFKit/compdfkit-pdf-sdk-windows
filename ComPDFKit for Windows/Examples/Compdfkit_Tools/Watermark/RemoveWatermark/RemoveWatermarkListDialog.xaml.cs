using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using Compdfkit_Tools.Helper;
using Path = System.IO.Path;

namespace Compdfkit_Tools.PDFControl { 
    /// <summary>
    /// Interaction logic for RemoveWatermarkListDialog.xaml
    /// </summary>
    public partial class RemoveWatermarkListDialog : Window
    {
        public RemoveWatermarkListDialog()
        {
            InitializeComponent();
            Title = LanguageHelper.SecurityManager.GetString("Title_RemoveWatermark");
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        { 
            Close();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            else
            {
                var path = dialog.SelectedPath;
                foreach (var fileInfo in RemoveWatermarkListControl.FileInfoDataList)
                {
                    fileInfo.Document.DeleteWatermarks();
                    string savePath = Path.Combine(path, Path.GetFileNameWithoutExtension(fileInfo.Document.FileName) + "_"
                        + LanguageHelper.SecurityManager.GetString("FileName_RemoveWatermark"));
                    fileInfo.Document.WriteToFilePath(savePath);
                }
                System.Diagnostics.Process.Start(path);
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var fileInfo in RemoveWatermarkListControl.FileInfoDataList)
            {
                fileInfo.Document.Release();
            }
            base.OnClosed(e);
        }
    }
}
