using System;
using System.Windows;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.DigitalSign;

namespace ComPDFKit.Controls.PDFControl
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