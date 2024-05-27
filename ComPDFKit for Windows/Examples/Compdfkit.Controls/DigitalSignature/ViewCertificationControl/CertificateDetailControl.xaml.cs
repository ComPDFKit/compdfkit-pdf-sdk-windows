using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ComPDFKit.Controls.Helper;
using ComPDFKit.DigitalSign;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CertificateDetailControl : UserControl,INotifyPropertyChanged
    {
        private string version;
        public string Version
        {
            get => version;
            set => UpdateProper(ref version, value);
        }
        
        private string algorithm;
        public string Algorithm
        {
            get => algorithm;
            set => UpdateProper(ref algorithm, value);
        }
        
        private string subject;
        public string Subject
        {
            get => subject;
            set => UpdateProper(ref subject, value);
        }
        
        private string issuer;
        public string Issuer
        {
            get => issuer;
            set => UpdateProper(ref issuer, value);
        }
        
        private string serialNumber;
        public string SerialNumber
        {
            get => serialNumber;
            set => UpdateProper(ref serialNumber, value);
        }

        private string validityFrom;
        public string ValidityFrom
        {
            get => validityFrom;
            set => UpdateProper(ref validityFrom, value);
        }

        private string validityTo;
        public string ValidityTo
        {
            get => validityTo;
            set => UpdateProper(ref validityTo, value);
        }
        
        private string certificatePolicy;
        public string CertificatePolicy
        {
            get => certificatePolicy;
            set => UpdateProper(ref certificatePolicy, value);
        }
        
        private string crlDistributionPoint;
        public string CrlDistributionPoint
        {
            get => crlDistributionPoint;
            set => UpdateProper(ref crlDistributionPoint, value);
        }
        
        private string authorityInfoAccess;
        public string AuthorityInfoAccess
        {
            get => authorityInfoAccess;
            set => UpdateProper(ref authorityInfoAccess, value);
        }

        private string authorityKeyIdentifier;
        public string AuthorityKeyIdentifier
        {
            get => authorityKeyIdentifier;
            set => UpdateProper(ref authorityKeyIdentifier, value);
        }
        
        private string subjectKeyIdentifier;
        public string SubjectKeyIdentifier
        {
            get => subjectKeyIdentifier;
            set => UpdateProper(ref subjectKeyIdentifier, value);
        }
        
        private string basicConstraints;
        public string BasicConstraints
        {
            get => basicConstraints;
            set => UpdateProper(ref basicConstraints, value);
        }
        
        private string keyUsage;
        public string KeyUsage
        {
            get => keyUsage;
            set => UpdateProper(ref keyUsage, value);
        }
        
        private string publicKey;
        public string PublicKey
        {
            get => publicKey;
            set => UpdateProper(ref publicKey, value);
        }
        
        private string x509Data;
        public string X509Data
        {
            get => x509Data;
            set => UpdateProper(ref x509Data, value);
        }
        
        private string sha1Abstract;
        public string SHA1Digest
        {
            get => sha1Abstract;
            set => UpdateProper(ref sha1Abstract, value);
        }
        
        private string md5Digest;
        public string MD5Digest
        {
            get => md5Digest;
            set => UpdateProper(ref md5Digest, value);
        }

        public CertificateDetailControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public void LoadDetailInfo(CPDFSignatureCertificate certificate)
        {
            string certificatePolicyText = string.Empty;
            string crlDistributionPointText = string.Empty;
            string authorityInfoAccessText = string.Empty;
            string keyUsageText = string.Empty;
            var usageList = DictionaryValueConverter.GetUsage(certificate);

            foreach (var policy in certificate.CertificatePolicies)
            {
                certificatePolicyText += policy + "\n";
            }

            foreach (var access in certificate.AuthorityInfoAccess)
            {
                authorityInfoAccess += access + "\n";
            }

            foreach (var crl in certificate.CRLDistributionPoints)
            {
                crlDistributionPointText += crl + "\n";
            }
            
            for(int i = 0; i < usageList.Count; i++)
            {
                keyUsageText += usageList[i];
                keyUsageText += (i == usageList.Count - 1) ? "" : ", ";
            }
            
            Version = certificate.Version.ToString();
            Algorithm = certificate.SignatureAlgorithmType.ToString().Substring(26) + "(" + certificate.SignatureAlgorithmOID + ")";
            Subject = certificate.Subject;
            Issuer = certificate.Issuer;
            SerialNumber = certificate.SerialNumber;
            ValidityFrom = CommonHelper.GetExactDateFromString(certificate.ValidityStarts);
            ValidityTo = CommonHelper.GetExactDateFromString(certificate.ValidityEnds);
            CertificatePolicy = certificatePolicyText;
            CrlDistributionPoint = crlDistributionPointText;
            X509Data = certificate.X509Data;
            AuthorityInfoAccess = authorityInfoAccess;
            AuthorityKeyIdentifier = certificate.AuthorityKeyIdentifier;
            SubjectKeyIdentifier = certificate.SubjectKeyIdentifier;
            BasicConstraints = certificate.BasicConstraints;
            KeyUsage = keyUsageText;
            PublicKey = certificate.PublicKey;
            SHA1Digest = certificate.SHA1Digest;
            MD5Digest = certificate.MD5Digest;
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
