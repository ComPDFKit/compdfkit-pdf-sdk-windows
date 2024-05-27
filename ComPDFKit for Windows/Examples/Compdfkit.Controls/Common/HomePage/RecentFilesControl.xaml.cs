using ComPDFKit.Controls.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for RecentFilesControl.xaml
    /// </summary>
    public partial class RecentFilesControl : UserControl
    {
        /// <summary>
        /// Open file event. Parameter is file path.
        /// </summary>
        public event EventHandler<OpenFileEventArgs> OpenFileEvent;
        public RecentFilesControl()
        {
            this.DataContext = FileHistoryHelper<PDFFileInfo>.Instance;
            InitializeComponent();
        }

        private void ListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem listViewItem)
            {
                if (listViewItem.Content is PDFFileInfo fileInfo)
                {
                    OpenFileEvent?.Invoke(this, new OpenFileEventArgs(FileOperationType.OpenFileDirectly, fileInfo.FilePath));
                }
            }
        }

    }
}
