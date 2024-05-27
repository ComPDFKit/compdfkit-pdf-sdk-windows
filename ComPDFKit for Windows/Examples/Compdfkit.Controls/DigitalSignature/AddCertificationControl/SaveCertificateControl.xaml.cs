using ComPDFKit.DigitalSign;
using ComPDFKit.Controls.Helper;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security; 
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    /// <summary>
    /// Interaction logic for SaveCertificateControlControl.xaml
    /// </summary>
    public partial class SaveCertificateControl : UserControl, INotifyPropertyChanged
    {
        public CertificateInfo CertificateInfo;
        
        public event EventHandler CancelSaveEvent; 

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<CertificateAccess> FillSignatureEvent;

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set => UpdateProper(ref _filePath, value);
        }

        public SaveCertificateControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string filePath = CommonHelper.GetGeneratePathOrEmpty("PFX Files(*.pfx) | *.pfx|P12 Files(*.p12) | *.p12");
            if (filePath != string.Empty)
            {
                FilePath = filePath;
            } 
        }
         
        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(FilePath))
            {
                ErrorTipsText.Text = LanguageHelper.SigManager.GetString("Warn_NoFile");
                return;
            }

            if (string.IsNullOrEmpty(SetPasswordPbx.Password))
            {
                ErrorTipsText.Text = LanguageHelper.SigManager.GetString("Warn_NoPassword");
                return;
            }

            if (SetPasswordPbx.Password == ConfirmPasswordPbx.Password)
            {
                CertificateInfo.Password = SetPasswordPbx.Password;
            }
            else
            {
                ErrorTipsText.Text = LanguageHelper.SigManager.GetString("Warn_Password");
                return;
            }

            string certificateInfo = "/";
            certificateInfo += "C=" + CertificateInfo.Area;
            if (CertificateInfo.OrganizationUnit != string.Empty)
            {
                certificateInfo += "/OU=" + CertificateInfo.OrganizationUnit;
            }
            if (CertificateInfo.Organization != string.Empty)
            {
                certificateInfo += "/O=" + CertificateInfo.Organization;
            }
            if (CertificateInfo.OrganizationUnit != string.Empty)
            {
                certificateInfo += "/D=" + CertificateInfo.OrganizationUnit;
            }
            certificateInfo += "/CN=" + CertificateInfo.GrantorName;
            certificateInfo += "/emailAddress=" + CertificateInfo.Email;
            bool is_2048 = CertificateInfo.AlgorithmType == AlgorithmType.RSA2048bit;
            if(CPDFPKCS12CertHelper.GeneratePKCS12Cert(certificateInfo, CertificateInfo.Password, FilePath, CertificateInfo.PurposeType, is_2048))
            {
                CertificateAccess certificateAccess = new CertificateAccess()
                {
                    filePath = FilePath,
                    password = CertificateInfo.Password
                };
                FillSignatureEvent?.Invoke(sender, certificateAccess);
            } 
        }
        
        
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelSaveEvent?.Invoke(this, EventArgs.Empty);
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
         
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetPasswordPbx_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            TextBlock textBlock = PasswordTextBlock;

            if (passwordBox.Password.Length > 0)
            {
                textBlock.Visibility = Visibility.Hidden;
            }
            else
            {
                textBlock.Visibility = Visibility.Visible;
            }
        }

        private void ConfirmPasswordPbx_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;
            TextBlock textBlock = ConfirmPasswordTextBlock;

            if (passwordBox.Password.Length > 0)
            {
                textBlock.Visibility = Visibility.Hidden;
            }
            else
            {
                textBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
