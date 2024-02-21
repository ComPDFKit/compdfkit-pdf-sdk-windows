using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using ComPDFKit.DigitalSign;

namespace Compdfkit_Tools.PDFControl
{
    public partial class DigitalSignatureInfoControl : UserControl, INotifyPropertyChanged
    {
        private string _signerInfo;
        public string SignerInfo
        {
            get => _signerInfo;
            set => UpdateProper(ref _signerInfo, value);
        }
        
        private string _timeInfo;
        public string TimeInfo
        {
            get => _timeInfo;
            set => UpdateProper(ref _timeInfo, value);
        }
        
        private SignatureStatus _status;
        public SignatureStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                SetStatus(_status);
            }
        }
        public DigitalSignatureInfoControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public void InitWithSignature(CPDFSignature signature)
        {
            string validity;
            string signerName;
            string email;
            string time;
            
            if(signature == null)
                return;
            SignatureStatus status;
            CPDFSigner signer = signature.SignerList.First();
            bool isTrusted = signer.IsCertTrusted;
            bool isVerified = signer.IsSignVerified;
            bool notModified = signature.ModifyInfoList.Count == 0;
            if (isTrusted && isVerified && notModified)
            {
                status = SignatureStatus.Valid;
                validity = LanguageHelper.SigManager.GetString("Text_Valid");
            }
            else if (!isTrusted && isVerified && notModified)
            {
                status = SignatureStatus.Unknown;
                validity = LanguageHelper.SigManager.GetString("Text_Unknown");
            }
            else
            {
                status = SignatureStatus.Invalid;
                validity = LanguageHelper.SigManager.GetString("Text_Invalid");
            }
            Status = status;
            string signedBy = LanguageHelper.SigManager.GetString("Text_Signer");
            signerName = signature.Name;
            email = DictionaryValueConverter.GetEmailFormDictionary(signature.SignerList.First().CertificateList.Last().SubjectDict);
            time = signature.Date;
            SignerInfo = validity + LanguageHelper.SigManager.GetString("Text_Signer") + "\"" + signerName;
            if (!string.IsNullOrEmpty(email))
            {
                SignerInfo += "<" + email + ">" + "\"";
            }
            else
            {
                SignerInfo += "\"";
            }

            TimeInfo = LanguageHelper.SigManager.GetString("Text_SignTime") + CommonHelper.GetExactDateFromString(time);
        }
        
        private void SetStatus(SignatureStatus status)
        {
            ValidBorder.Visibility = Visibility.Collapsed;
            InvalidBorder.Visibility = Visibility.Collapsed;
            UnknownBorder.Visibility = Visibility.Collapsed;

            switch (status)
            {
                case SignatureStatus.None:
                    break;
                case SignatureStatus.Valid:
                    ValidBorder.Visibility = Visibility.Visible;
                    break;
                case SignatureStatus.Invalid:
                    InvalidBorder.Visibility = Visibility.Visible;
                    break;
                case SignatureStatus.Unknown:
                    UnknownBorder.Visibility = Visibility.Visible;
                    break;
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
    }
}