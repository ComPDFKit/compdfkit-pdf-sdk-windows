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
    /// <summary>
    /// Interaction logic for SummaryControl.xaml
    /// </summary>
    public partial class SummaryControl : UserControl,INotifyPropertyChanged
    {
        private string award;
        public string Award
        {
            get => award;
            set => UpdateProper(ref award, value);
        }
        
        private string grantor;
        public string Grantor
        {
            get => grantor;
            set => UpdateProper(ref grantor, value);
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
        
        private string intendedUsage;
        public string IntendedUsage
        {
            get => intendedUsage;
            set => UpdateProper(ref intendedUsage, value);
        }

        public SummaryControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public void LoadSummaryInfo(CPDFSignatureCertificate certificate)
        {
            string awardText;
            string grantorText = DictionaryValueConverter.GetGrantorFromDictionary(certificate.SubjectDict);
            string email = DictionaryValueConverter.GetEmailFormDictionary(certificate.SubjectDict);
            var usageList = DictionaryValueConverter.GetUsage(certificate);
            string keyUsageText = "";
            
            if(email != null)
            {
                awardText = grantorText + " <" + email + ">";
            }
            else
            {
                awardText = grantorText;
            }
            
            for(int i = 0; i < usageList.Count; i++)
            {
                keyUsageText += usageList[i];
                keyUsageText += (i == usageList.Count - 1) ? "" : ", ";
            }
            Award = awardText;
            Grantor = grantorText;
            ValidityFrom = CommonHelper.GetExactDateFromString(certificate.ValidityStarts);
            ValidityTo = CommonHelper.GetExactDateFromString(certificate.ValidityEnds);
            IntendedUsage = keyUsageText;
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
