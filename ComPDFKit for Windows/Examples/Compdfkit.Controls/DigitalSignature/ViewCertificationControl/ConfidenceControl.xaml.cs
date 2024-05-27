using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ComPDFKit.Controls.DigitalSignature.CPDFSignatureListControl;
using ComPDFKit.Controls.Properties;
using ComPDFKit.DigitalSign;

namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for ConfidenceControl.xaml
    /// </summary>
    public partial class ConfidenceControl : UserControl, INotifyPropertyChanged
    {
        public static ResourceDictionary ResourceDictionary { get; set; }
        private bool isTrusted;
        private CPDFSignatureCertificate cpdfCertificate;
        
        public event EventHandler TrustCertificateEvent;
        public bool IsTrusted
        {
            get => isTrusted;
            set => UpdateProper(ref isTrusted, value);
        }
        
        public ConfidenceControl()
        {
            InitializeComponent();
            DataContext = this;
            ResourceDictionary = this.Resources;
        }
        
        public void LoadConfidenceInfo(CPDFSignatureCertificate certificate)
        {
            cpdfCertificate = certificate;
            VerifyCertificate();
        }
        
        public void VerifyCertificate()
        {
            if (cpdfCertificate != null)
            {
                cpdfCertificate.CheckCertificateIsTrusted();
                IsTrusted = cpdfCertificate.IsTrusted;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void UpdateProper<T>(ref T properValue,
            T newValue,
            [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return;

            properValue = newValue;
            OnPropertyChanged(properName);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        private void TrustCertificateButton_OnClick(object sender, RoutedEventArgs e)
        {
            cpdfCertificate.AddToTrustedCertificates();
            VerifyCertificate();
            TrustCertificateEvent?.Invoke(this, null);
        }
    }
    
    public class ConfidenceStatusToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceDictionary resourceDictionary = ConfidenceControl.ResourceDictionary;
            if (value is bool isTrusted)
            {
                if (isTrusted)
                {
                    return resourceDictionary["ValidPath"];
                    
                }
                else
                {
                    return resourceDictionary["InvalidPath"];
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
