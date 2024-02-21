using System;
using System.Windows;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;
using ComPDFKit.DigitalSign;

namespace Compdfkit_Tools.PDFControl
{
    public partial class VerifyDigitalSignatureControl : Window
    {
        private CPDFSignature signature;
        public event EventHandler<CPDFSignature> ViewCertificateEvent;
        
        public VerifyDigitalSignatureControl()
        {
            InitializeComponent();
            Title = LanguageHelper.SigManager.GetString("Title_Sig");
        }

        public void InitWithSignature(CPDFSignature signature)
        {
            DigitalSignatureInfoControl.InitWithSignature(signature);
            DigitalSignatureValiditySummaryControl.InitWithSignature(signature);
            this.signature = signature;
        }

        private void ViewCertificates_OnClick(object sender, RoutedEventArgs e)
        {
            ViewCertificateEvent?.Invoke(this, signature);
        }
    }
}