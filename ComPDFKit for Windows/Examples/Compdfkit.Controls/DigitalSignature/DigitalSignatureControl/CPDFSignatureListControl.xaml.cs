using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.DigitalSign;
using ComPDFKit.Import;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace ComPDFKit.Controls.DigitalSignature.CPDFSignatureListControl
{
    public class SignatureData
    {
        public SignatureStatus Status { get; set; }
        public string Signer { get; set; }
    }
    
    public partial class CPDFSignatureListControl : UserControl
    {
        private PDFViewControl pdfViewer;
        private List<SignatureData> signatureDataList;
        private List<CPDFSignature> signatureList;
        private ContextMenu popContextMenu;
        
        public event EventHandler<CPDFSignature> ViewCertificateEvent;
        public event EventHandler<CPDFSignature> ViewSignatureEvent;
        public event EventHandler DeleteSignatureEvent;
        public CPDFSignatureListControl()
        {
            InitializeComponent();
            DataContext = this;
            popContextMenu = new ContextMenu();
            MenuItem viewSignatureDetailsMenu = new MenuItem();
            viewSignatureDetailsMenu.Header = LanguageHelper.BotaManager.GetString("Menu_SigDetail");
            viewSignatureDetailsMenu.Click -= ViewSignatureDetailsMenu_Click;
            viewSignatureDetailsMenu.Click += ViewSignatureDetailsMenu_Click;
            popContextMenu.Items.Add(viewSignatureDetailsMenu);
            
            MenuItem viewCertificateDetailsMenu = new MenuItem();
            viewCertificateDetailsMenu.Header = LanguageHelper.BotaManager.GetString("Menu_CertDetail");
            viewCertificateDetailsMenu.Click -= ViewCertificateDetailsMenu_Click;
            viewCertificateDetailsMenu.Click += ViewCertificateDetailsMenu_Click;
            popContextMenu.Items.Add(viewCertificateDetailsMenu);
            
            MenuItem deleteMenu = new MenuItem();
            deleteMenu.Header = LanguageHelper.BotaManager.GetString("Menu_Delete");
            deleteMenu.Click -= DeleteMenu_Click;
            deleteMenu.Click += DeleteMenu_Click;
            popContextMenu.Items.Add(deleteMenu);
        }
        
        private void ViewSignatureDetailsMenu_Click(object sender, RoutedEventArgs e)
        {
            int index = SignatureList.SelectedIndex;
            if (index >= 0 && index < signatureList.Count)
            {
                ViewSignatureEvent?.Invoke(this, signatureList[index]);
            }
        }
        
        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = LanguageHelper.BotaManager.GetString("Text_SureDelete");
            MessageBoxButton button = MessageBoxButton.OKCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageBoxText, LanguageHelper.CommonManager.GetString("Caption_Warning"), button, icon, MessageBoxResult.Cancel);
            if(result != MessageBoxResult.OK)
            {
                return;
            }
            int index = SignatureList.SelectedIndex;
            if (index >= 0 && index < signatureList.Count)
            {
                var widget = signatureList[index].GetSignatureWidget(pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument());
                pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument().RemoveSignature(signatureList[index], true);
                widget.ResetForm();
                widget.SetIsLocked(false);
                pdfViewer.PDFViewTool.GetCPDFViewer().UpdateAnnotFrame();
                DeleteSignatureEvent?.Invoke(this, null);
            }
        }
        
        private void ViewCertificateDetailsMenu_Click(object sender, RoutedEventArgs e)
        {
            int index = SignatureList.SelectedIndex;
            if (index >= 0 && index < signatureList.Count)
            {
                ViewCertificateEvent?.Invoke(this, signatureList[index]);
            }
        }

        public void InitWithPDFViewer(PDFViewControl newPDFView)
        {
            pdfViewer = newPDFView;
        }
        
        public void LoadSignatureList()
        {
            if (pdfViewer != null && pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument() != null)
            {
                signatureList = SignatureHelper.SignatureList;
                if (signatureList != null)
                {
                    SignatureList.ItemsSource = null;
                    signatureDataList = new List<SignatureData>();
                    foreach (CPDFSignature signature in signatureList)
                    {
                        var item = new SignatureData();
                        CPDFSigner signer = signature.SignerList.First();
                        bool isSignVerified = signer.IsSignVerified;
                        bool isCertTrusted = signer.IsCertTrusted;
                        bool notModified = signature.ModifyInfoList.Count == 0;
                        if (isSignVerified && isCertTrusted && notModified)
                        {
                            item.Status = SignatureStatus.Valid;
                        }
                        else if (isSignVerified && !isCertTrusted && notModified)
                        {
                            item.Status = SignatureStatus.Unknown;
                        }
                        else
                        {
                            item.Status = SignatureStatus.Invalid;
                        }
                        
                        item.Signer = signature.Name + "'s signature";
                        signatureDataList.Add(item);
                    }

                    if (signatureList.Count > 0)
                    {
                        SignatureList.ContextMenu = popContextMenu;
                    }
                    SignatureList.ItemsSource = signatureDataList;
                    SignatureList.Visibility = Visibility.Visible;
                }
                else
                {
                    SignatureList.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SignatureList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (SignatureList != null && SignatureList.SelectedIndex == -1)
                {
                    foreach (var item in popContextMenu.Items)
                    {

                        (item as MenuItem).IsEnabled = false;
                    }
                }
                else
                {
                    foreach (var item in popContextMenu.Items)
                    {

                        (item as MenuItem).IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SignatureList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SignatureList.SelectedIndex = -1;
        }
        
        private void SignatureList_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int index = SignatureList.SelectedIndex;
            if (index >= 0 && index < signatureList.Count)
            {
                CRect rect = signatureList[index].GetPageBound(pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument());
                Point point = new Point(rect.left, rect.top);
                pdfViewer.PDFViewTool.GetCPDFViewer().GoToPage(signatureList[index].GetPageIndex(pdfViewer.PDFViewTool.GetCPDFViewer().GetDocument()),point);
            }
        }
    }
    
    public class SignatureStatusToPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SignatureStatus signatureStatus && parameter is CPDFSignatureListControl cpdfSignatureListControl)
            {
                switch (signatureStatus)
                {
                    case SignatureStatus.Valid:
                        return cpdfSignatureListControl.FindResource("ValidSignaturePath");
                    case SignatureStatus.Invalid:
                        return cpdfSignatureListControl.FindResource("InvalidSignaturePath");
                    case SignatureStatus.Unknown:
                        return cpdfSignatureListControl.FindResource("UnknownSignaturePath");
                    default:
                        return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}