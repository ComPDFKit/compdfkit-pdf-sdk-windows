using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using Compdfkit_Tools.Helper;
using ComPDFKit.DigitalSign;

namespace Compdfkit_Tools.PDFControl
{
    /// <summary>
    /// Interaction logic for ViewCertificationControl.xaml
    /// </summary>
    public partial class ViewCertificateDialog : Window
    {
        private List<CPDFSignatureCertificate> certificateList;
        public ViewCertificateDialog()
        {
            InitializeComponent();
            Title = LanguageHelper.SigManager.GetString("Title_Cert");
        }

        public void InitCertificateList(CPDFSignature signature)
        {
            CertificateListView.ItemsSource = null;
            certificateList = signature.SignerList.First().CertificateList;
            CertificateListView.ItemsSource = certificateList;
            CertificateListView.SelectedIndex = certificateList.Count - 1;
        }

        private void CertificateListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = CertificateListView.SelectedIndex;
            if(index >= 0)
            {
                CertificateInfoControl.LoadCertificateInfo(certificateList[index]);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    
    public class DictionaryValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Dictionary<string, string> dictionary)
            {
                return GetGrantorFromDictionary(dictionary);
            }
            return "Unknown Signer";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        
        public static string GetGrantorFromDictionary(Dictionary<string, string> dictionary)
        {
            string grantor = string.Empty;
            dictionary.TryGetValue("CN", out grantor);
            if (string.IsNullOrEmpty(grantor))
            {
                dictionary.TryGetValue("OU", out grantor);
            }
            if (string.IsNullOrEmpty(grantor))
            {
                grantor = "Unknown Signer";
            }
            return grantor;
        }

        public static string GetDNFromDictionary(Dictionary<string, string> dictionary)
        {
            List<string> dnParts = new List<string>();

            if (dictionary.TryGetValue("CN", out string cn))
            {
                if (!string.IsNullOrEmpty(cn))
                {
                    dnParts.Add("CN=" + cn);
                }
            }

            if (dictionary.TryGetValue("O", out string o))
            {
                if (!string.IsNullOrEmpty(o))
                {
                    dnParts.Add("O=" + o);
                }
            }

            if (dictionary.TryGetValue("OU", out string ou))
            {
                dnParts.Add("OU=" + ou);
            }

            if (dictionary.TryGetValue("C", out string c))
            {
                dnParts.Add("C=" + c);
            }

            if (dictionary.TryGetValue("ST", out string st))
            {
                dnParts.Add("ST=" + st);
            }
             
            string DN = string.Join(", ", dnParts);

            return DN;
        }


        public static string GetEmailFormDictionary(Dictionary<string, string> dictionary)
        {
            string email = string.Empty;
            dictionary.TryGetValue("emailAddress", out email);
            return email;
        }

        public static List<string> GetUsage(CPDFSignatureCertificate certificate)
        {
            int usage = certificate.KeyUsage;
            List<string> usageList = new List<string>();
            if ((usage & 1 << 0) != 0)
            {
                usageList.Add("Encipher Only");
            }
            if ((usage & 1 << 1) != 0)
            {
                usageList.Add("CRL Signature");
            }
            if ((usage & 1 << 2) != 0)
            {
                usageList.Add("Certificate Signature");
            }
            if ((usage & 1 << 3) != 0)
            {
                usageList.Add(LanguageHelper.SigManager.GetString("Usage_Key"));
            }
            if ((usage & 1 << 4) != 0)
            {
                usageList.Add("Data Encipherment");
            }
            if ((usage & 1 << 5) != 0)
            {
                usageList.Add(LanguageHelper.SigManager.GetString("Usage_Keys"));
            }
            if ((usage & 1 << 6) != 0)
            {
                usageList.Add(LanguageHelper.SigManager.GetString("Usage_NonRepudiation"));
            }
            if ((usage & 1 << 7) != 0)
            {
                usageList.Add(LanguageHelper.SigManager.GetString("Usage_DigitalSignature"));
            }
            if ((usage & 1 << 15) != 0)
            {
                usageList.Add("Decipher Only");
            }

            return usageList;
        }
    }
}
