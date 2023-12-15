using Compdfkit_Tools.Helper;
using ComPDFKitViewer.PdfViewer;
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

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFTitleBarControl : UserControl
    {
        public static readonly DependencyProperty CanSaveProperty =
            DependencyProperty.RegisterAttached("CanSave", typeof(bool), typeof(CPDFTitleBarControl), new PropertyMetadata(false));
        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set { SetValue(CanSaveProperty, value); }
        }


        public event EventHandler OpenFileEvent;
        public event EventHandler SaveFileEvent;
        public event EventHandler SaveAsFileEvent;

        public CPDFTitleBarControl()
        {
            InitializeComponent();
        }

        private void OpenFileItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileEvent?.Invoke(sender, RoutedEventArgs.Empty);
        }

        private void SaveFileItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileEvent?.Invoke(sender, RoutedEventArgs.Empty);
        }

        private void SaveAsItem_Click(object sender, RoutedEventArgs e)
        {
            SaveAsFileEvent?.Invoke(sender, RoutedEventArgs.Empty); 
        }

        private void AboutUsItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.compdf.com/company/about");
        }

        private void ContactUs_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.compdf.com/contact-sales");
        }
        
        private void TechnicalSupport_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.compdf.com/support");
        }

        private void PrivacyAgreement_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.compdf.com/privacy-policy");
        }

        private void ServiceTerms_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.compdf.com/terms-of-service");
        }

        private void DeviceSerial_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            DeviceSerialControl deviceSerialControl = new DeviceSerialControl()
            {
                Owner = parentWindow
            };
            deviceSerialControl.ShowDialog();
        }
    }
}
