using ComPDFKit.DigitalSign;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.Controls.Helper;
using Nager.Country;
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
    /// Interaction logic for AddCustomCertificationControl.xaml
    /// </summary>
    public partial class AddCustomCertificationControl : UserControl
    {
        public event EventHandler SaveEvent;
        public event EventHandler CancelEvent;
        public CertificateInfo CertificateInfo = new CertificateInfo();

        private string _grantorName = string.Empty;
        public string GrantorName
        {
            get => _grantorName;
            set
            {
                _grantorName = value;
                CertificateInfo.GrantorName = value;
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                CertificateInfo.Email = value;
            }
        }

        private string _organization = string.Empty;
        public string Organization
        {
            get => _organization;
            set
            {
                _organization = value;
                CertificateInfo.Organization = value;
            }
        }

        private string _organizationalUnit = string.Empty;
        public string OrganizationalUnit
        {
            get => _organizationalUnit;
            set
            {
                _organizationalUnit = value;
                CertificateInfo.OrganizationUnit = value;
            }
        }

        private readonly CountryProvider countryProvider = new CountryProvider();
        private readonly List<string> countryNames = new List<string>();
        
        public event EventHandler<CreateCertificationMode> ContinueEvent;

        public AddCustomCertificationControl()
        {
            InitializeComponent();
            this.DataContext = this;
            FillComboBox();
        }

        private void FillComboBox()
        {
            FillComboBoxWithCountries();
            FillComboBoxWithPropose();
        }

        private void FillComboBoxWithPropose()
        {
            PurposeCmb.Items.Clear();
            PurposeCmb.Items.Add(LanguageHelper.SigManager.GetString("Option_Sign"));
            PurposeCmb.Items.Add(LanguageHelper.SigManager.GetString("Option_Encrypt"));
            PurposeCmb.Items.Add(LanguageHelper.SigManager.GetString("Option_SignAndEncrypt"));
        }

        private void FillComboBoxWithCountries()
        {
            var countries = countryProvider.GetCountries();
            foreach (var country in countries)
            {
                var formattedName = $"{country.Alpha2Code} - {country.CommonName}";
                countryNames.Add(formattedName);
            }

            AreaCmb.ItemsSource = countryNames;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelEvent?.Invoke(this, EventArgs.Empty);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {

            if (CertificateInfo.GrantorName == string.Empty)
            {
                ErrorTipsText.Text = "Please input Grantor Name";
                return;
            }
            if(CertificateInfo.Email == string.Empty)
            {
                ErrorTipsText.Text = "Please input Email";
                return;
            }
            if (!CommonHelper.IsValidEmail(CertificateInfo.Email))
            {
                ErrorTipsText.Text = "Email format is not correct";
                return;
            }
            ContinueEvent?.Invoke(this, CreateCertificationMode.SaveCertificate);
        }

        private void AreaCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AreaCmb.SelectedItem != null)
            {
                string selectedText = AreaCmb.SelectedItem.ToString();
                string[] parts = selectedText.Split('-');
                CertificateInfo.Area = parts[0].Trim(); // Extract the Alpha2Code 
            }
        }

        private void PurposeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PurposeCmb.SelectedItem != null)
            {
                CertificateInfo.PurposeType = (CPDFCertUsage)PurposeCmb.SelectedIndex + 1;
            }
        }
    }
}
