using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFDocument;
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
using System.Windows.Shapes;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControl
{
    /// <summary>
    /// Interaction logic for FillDigitalSignatureDialog.xaml
    /// </summary>
    public partial class FillDigitalSignatureDialog : Window
    {
        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                FillDigitalSignatureControl.SignaturePath = value;
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                FillDigitalSignatureControl.Password = value;
            }
        }

        private CPDFSignatureWidget _signatureWidget;
        public CPDFSignatureWidget SignatureWidget
        {
            get => this._signatureWidget;
            set
            {
                this._signatureWidget = value;
                FillDigitalSignatureControl.signatureWidget = value;
            }
        }

        private CPDFDocument _document;
        public CPDFDocument Document
        {
            get => _document;
            set
            {
                _document = value;
                FillDigitalSignatureControl.Document = value;
            }
        }

        public event EventHandler<string> AfterFillSignature;

        public FillDigitalSignatureDialog()
        {
            InitializeComponent();
            FillDigitalSignatureControl.AfterFillSignature -= ReloadAfterFillSignature;
            FillDigitalSignatureControl.AfterFillSignature += ReloadAfterFillSignature;
            Title = LanguageHelper.SigManager.GetString("Title_SignAp");
        }

        private void ReloadAfterFillSignature(object sender, string e)
        {
            AfterFillSignature?.Invoke(sender, e);
        }
    }
}
