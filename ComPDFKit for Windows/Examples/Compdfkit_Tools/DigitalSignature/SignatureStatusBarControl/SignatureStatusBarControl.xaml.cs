using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Compdfkit_Tools.Helper;
using ComPDFKit.DigitalSign;

namespace Compdfkit_Tools.PDFControl
{
    public enum SignatureStatus
    {
        None,
        Valid,
        Invalid,
        MultiSig,
        Unknown,
    }
    public partial class SignatureStatusBarControl : UserControl, INotifyPropertyChanged
    {
        private string _messageString;
        public string MessageString
        {
            get => _messageString;
            set => UpdateProper(ref _messageString, value);
        }
        private string validString = LanguageHelper.SigManager.GetString("Text_SignerValid");
        private string invalidString = LanguageHelper.SigManager.GetString("Text_SignerInvalid");
        private string multiSigString = LanguageHelper.SigManager.GetString("Text_MultiSig");
        private string unknownString = LanguageHelper.SigManager.GetString("Text_SigUnknown");

        public event EventHandler OnViewSignatureButtonClicked;
        
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

        public SignatureStatusBarControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        private void SetStatus(SignatureStatus status)
        {
            ValidBorder.Visibility = Visibility.Collapsed;
            InvalidBorder.Visibility = Visibility.Collapsed;
            UnknownBorder.Visibility = Visibility.Collapsed;

            switch (status)
            {
                case SignatureStatus.None:
                    MessageString = "";
                    Visibility = Visibility.Collapsed;
                    break;
                case SignatureStatus.Valid:
                    ValidBorder.Visibility = Visibility.Visible;
                    Visibility = Visibility.Visible;
                    MessageString = validString;
                    break;
                case SignatureStatus.Invalid:
                    InvalidBorder.Visibility = Visibility.Visible;
                    Visibility = Visibility.Visible;
                    MessageString = invalidString;
                    break;
                case SignatureStatus.MultiSig:
                    InvalidBorder.Visibility = Visibility.Visible;
                    Visibility = Visibility.Visible;
                    MessageString = multiSigString;
                    break;
                case SignatureStatus.Unknown:
                    UnknownBorder.Visibility = Visibility.Visible;
                    Visibility = Visibility.Visible;
                    MessageString = unknownString;
                    break;
            }
        }

        public void SetStatus(List<CPDFSignature> signatureList)
        {
            SignatureStatus status;
            if (signatureList.Count == 1)
            {
                bool isTrusted = true;
                bool isVerified = true;
                bool notModified = true;
                foreach (var signature in signatureList)
                {
                    CPDFSigner signer = signature.SignerList.First();
                    if(signer.IsSignVerified == false)
                        isVerified = false;
                    if (signer.IsCertTrusted == false)
                        isTrusted = false;
                    if (signature.ModifyInfoList.Count > 0)
                        notModified = false;
                }
                
                
                if (isTrusted && isVerified && notModified)
                {
                    status = SignatureStatus.Valid;
                }
                else if (!isTrusted && isVerified && notModified)
                {
                    status = SignatureStatus.Unknown;
                }
                else
                {
                    status = SignatureStatus.Invalid;
                }
            }
            else if (signatureList.Count > 1)
            {
                status = SignatureStatus.MultiSig;
            }
            else
            {
                status = SignatureStatus.None;
            }
            Status = status;
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


        private void ButtonViewSignature_OnClick(object sender, RoutedEventArgs e)
        {
            OnViewSignatureButtonClicked?.Invoke(this,null);
        }
    }
}
