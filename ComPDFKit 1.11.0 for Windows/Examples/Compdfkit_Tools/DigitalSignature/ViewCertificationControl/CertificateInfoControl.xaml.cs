using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
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
using ComPDFKit.DigitalSign;

namespace Compdfkit_Tools.PDFControl {
    public partial class CertificateInfoControl : UserControl
    {
        public event EventHandler TrustCertificateEvent;
        public CertificateInfoControl()
        {
            InitializeComponent();
        }

        public void LoadCertificateInfo(CPDFSignatureCertificate certificate)
        {
            SummaryControl.LoadSummaryInfo(certificate);
            CertificateDetailControl.LoadDetailInfo(certificate);
            ConfidenceControl.LoadConfidenceInfo(certificate);

            ConfidenceControl.TrustCertificateEvent += (sender, args) =>
            {
                TrustCertificateEvent?.Invoke(sender, null);
            };
        }
    }
}
