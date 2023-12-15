using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Compdfkit_Tools.Helper;
using ComPDFKit.DigitalSign;

namespace Compdfkit_Tools.PDFControl
{
    public partial class DigitalSignatureValiditySummaaryControl : UserControl, INotifyPropertyChanged
    {
        private string _validitySummaryString;
        public string ValiditySummaryString
        {
            get => _validitySummaryString;
            set => UpdateProper(ref _validitySummaryString, value);
        }
        public DigitalSignatureValiditySummaaryControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void InitWithSignature(CPDFSignature signature)
        {
            bool isSignVerified = signature.SignerList.First().IsSignVerified;
            bool isCertTrusted = signature.SignerList.First().IsCertTrusted;
            bool isDocModified = signature.ModifyInfoList.Count > 0;
            bool isExpired = DateTime.Now >
            CommonHelper.GetDateTimeFromString(signature.SignerList.First().CertificateList.First().ValidityEnds);
            
            string validitySummaryString = "";
            if (isCertTrusted)
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SignerValid") + "\n\n";
            }
            else
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SignerInvalid") + "\n\n";
            }
            if(isDocModified || (!isSignVerified && !isCertTrusted))
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SigValid") + "\n\n";
            }
            else if (isSignVerified && isCertTrusted)
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SigInvalid") + "\n\n";
            }
            else if(isSignVerified && !isCertTrusted)
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SigUnknown") + "\n\n";
            }
            
            if(isExpired)
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SigExpired") + "\n\n";
            }

            if (!isDocModified)
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SigNoModified") + "\n";
            }
            else
            {
                validitySummaryString += LanguageHelper.SigManager.GetString("Text_SigModified") + "\n";
            }
            ValiditySummaryString = validitySummaryString;
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