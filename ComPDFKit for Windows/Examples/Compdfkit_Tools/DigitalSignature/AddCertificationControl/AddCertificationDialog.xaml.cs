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

namespace Compdfkit_Tools.PDFControl
{
    /// <summary>
    /// Interaction logic for AddCertificationDialog.xaml
    /// </summary>
    public partial class AddCertificationDialog : Window
    {
        private AddCertificationControl addCertificationControl = null;
        private AddCustomCertificationControl addCustomCertificationControl = null;
        private AddExistedCertificationControl addExistedCertificationControl = null;
        private SaveCertificateControl saveCertificateControl = null;

        private string addCertificationControlTitle = Helper.LanguageHelper.SigManager.GetString("Title_AddDigitalSign");
        private string addExistedCertificationControlTitle = Helper.LanguageHelper.SigManager.GetString("Title_AddID");
        private string addCustomCertificationControlTitle = Helper.LanguageHelper.SigManager.GetString("Title_SigInfo");
        private string saveCertificateTitle = Helper.LanguageHelper.SigManager.GetString("Title_Save");

        public event EventHandler<CertificateAccess> FillSignatureEvent;


        public AddCertificationDialog()
        {
            InitializeComponent();
            addCertificationControl = new AddCertificationControl();
            BodyBd.Child = addCertificationControl;
            Title = addCertificationControlTitle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            addCertificationControl.ContinueEvent -= ContinueEvent;
            addCertificationControl.ContinueEvent += ContinueEvent;
            addCertificationControl.CancelEvent -= CloseEvent;
            addCertificationControl.CancelEvent += CloseEvent;
        }

        private void ContinueEvent(object sender, CreateCertificationMode e)
        {
            if(e == CreateCertificationMode.AddExistedCertification)
            {
                addExistedCertificationControl = new AddExistedCertificationControl();
                BodyBd.Child = addExistedCertificationControl;
                Title = addExistedCertificationControlTitle;
                this.Height = addExistedCertificationControl.Height + 35;
                this.Width = addExistedCertificationControl.Width + 20;
                 
                addExistedCertificationControl.CancelEvent += CancelEvent;
                addExistedCertificationControl.FillSignatureEvent -= FillSignature;
                addExistedCertificationControl.FillSignatureEvent += FillSignature;


            }
            else if(e == CreateCertificationMode.AddCustomCertification)
            {
                addCustomCertificationControl = new AddCustomCertificationControl();
                BodyBd.Child = addCustomCertificationControl;
                Title = addCustomCertificationControlTitle;
                this.Height = addCustomCertificationControl.Height + 35;
                this.Width = addCustomCertificationControl.Width + 20;

                addCustomCertificationControl.ContinueEvent -= ContinueEvent;
                addCustomCertificationControl.ContinueEvent += ContinueEvent;

                addCustomCertificationControl.CancelEvent -= CancelEvent;
                addCustomCertificationControl.CancelEvent += CancelEvent;
            }
            else if (e == CreateCertificationMode.SaveCertificate)
            {
                saveCertificateControl = new SaveCertificateControl();
                saveCertificateControl.CertificateInfo = addCustomCertificationControl.CertificateInfo;
                BodyBd.Child = saveCertificateControl;
                Title = saveCertificateTitle;
                
                this.Height = saveCertificateControl.Height + 20;
                this.Width = saveCertificateControl.Width + 20;
                saveCertificateControl.FillSignatureEvent -= FillSignature;
                saveCertificateControl.FillSignatureEvent += FillSignature;
                saveCertificateControl.CancelSaveEvent -= CancelSaveEvent;
                saveCertificateControl.CancelSaveEvent += CancelSaveEvent;
            }
        }

        private void FillSignature(object sender, CertificateAccess e)
        {
            Close();
            FillSignatureEvent?.Invoke(sender, e);
        }

        private void CancelEvent(object sender, EventArgs e)
        {
            addCertificationControl = new AddCertificationControl();
            BodyBd.Child = addCertificationControl;
            Title = addCertificationControlTitle;
            this.Height = addCertificationControl.Height + 35;
            this.Width = addCertificationControl.Width + 20;
            addCertificationControl.ContinueEvent -= ContinueEvent;
            addCertificationControl.ContinueEvent += ContinueEvent;
            addCertificationControl.CancelEvent -= CloseEvent;
            addCertificationControl.CancelEvent += CloseEvent;
        }
        
        private void CancelSaveEvent(object sender, EventArgs e)
        {
            BodyBd.Child = addCustomCertificationControl;
            Title = addCustomCertificationControlTitle;
            this.Height = addCustomCertificationControl.Height + 35;
            this.Width = addCustomCertificationControl.Width + 20;
            addCustomCertificationControl.ContinueEvent -= ContinueEvent;
            addCustomCertificationControl.ContinueEvent += ContinueEvent;
            addCustomCertificationControl.CancelEvent -= CancelEvent;
            addCustomCertificationControl.CancelEvent += CancelEvent;
        }

        private void CloseEvent(object sender, EventArgs e)
        {
            Close();
        }
    }
}
