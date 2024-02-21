using ComPDFKit.DigitalSign;
using Compdfkit_Tools.Helper;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class AddExistedCertificationControl : UserControl,INotifyPropertyChanged
    {
        public event EventHandler SaveEvent;
        public event EventHandler CancelEvent;
        public event EventHandler<CertificateAccess> FillSignatureEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _canContinue;
        public bool CanContinue
        {
            get => _canContinue;
            set => UpdateProper(ref _canContinue, value);
        }

        public AddExistedCertificationControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CancelBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CancelEvent?.Invoke(this, EventArgs.Empty);
        }

        private void DoneBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!CPDFPKCS12CertHelper.CheckPKCS12Password(FileNameTxt.Text, PasswordBoxTxt.Password))
            {
                ErrorTipsText.Text = "Invalid Password.";
                return;
            }

            FillSignatureEvent?.Invoke(sender, new CertificateAccess { filePath = FileNameTxt.Text, password = PasswordBoxTxt.Password });

        }

        private void SelectFileBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty("PFX Files(*.pfx) | *.pfx|P12 Files(*.p12) | *.p12");
            if (filePath != string.Empty)
            {
                FileNameTxt.Text = filePath;
            }
        }

        private void PasswordBoxTxt_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = (PasswordBox)sender;

            if (passwordBox.Password.Length > 0)
            {
                PasswordTextBlock.Visibility = Visibility.Hidden;
            }
            else
            {
                PasswordTextBlock.Visibility = Visibility.Visible;
            }
            CanContinue = FileNameTxt.Text.Length > 0 && PasswordBoxTxt.Password.Length > 0;
        }
        
        private void FileNameTxt_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CanContinue = FileNameTxt.Text.Length > 0 && PasswordBoxTxt.Password.Length > 0;
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
    }
}