using ComPDFKit.DigitalSign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compdfkit_Tools.PDFControl
{
    public enum CreateCertificationMode
    {
        AddExistedCertification,
        AddCustomCertification,
        SaveCertificate
    }

    public enum AlgorithmType
    {
        RSA1024bit,
        RSA2048bit
    }

    public class CertificateInfo
    {
        public string GrantorName = string.Empty; 
        public string Organization = string.Empty;
        public string OrganizationUnit = string.Empty;
        public string Email = string.Empty;
        public string Area = string.Empty;
        public string Password = string.Empty;
        public AlgorithmType AlgorithmType;
        public CPDFCertUsage PurposeType;
    }
    
    public class CertificateAccess
    {
        public string filePath;
        public string password; 
    }
}
