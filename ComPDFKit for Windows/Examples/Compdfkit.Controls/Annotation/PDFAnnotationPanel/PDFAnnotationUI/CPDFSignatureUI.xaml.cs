using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControl;
using ComPDFKit.Controls.Properties;
using ComPDFKitViewer;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ComPDFKit.Controls.Helper;
using ComPDFKit.Tool.Help;

namespace ComPDFKit.Controls.Annotation.PDFAnnotationPanel.PDFAnnotationUI
{
    public partial class CPDFSignatureUI : UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        public ObservableCollection<CPDFSignatureData> SignatureList { get; set; }
        private SignatureParam signatureParam;
        private PDFViewControl viewControl;
        private CPDFSignatureWidget signatureWidget;

        public CPDFSignatureUI()
        {
            InitializeComponent();
        }

        public void SetFormProperty(AnnotParam param, PDFViewControl view, CPDFAnnotation sign)
        {
            signatureParam = (SignatureParam) param;
            viewControl = view;
            signatureWidget = (CPDFSignatureWidget)sign;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SignatureList = new ObservableCollection<CPDFSignatureData>();
            Binding binding = new Binding();
            binding.Source = SignatureList;
            SignatureListBox.SetBinding(ItemsControl.ItemsSourceProperty, binding);

            LoadSettings();
        }
        private void LoadSettings()
        {
            SignatureList items = Settings.Default.SignatureList;
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    CPDFSignatureData customStamp = new CPDFSignatureData();
                    customStamp.SourcePath = items[i].SourcePath;
                    customStamp.DrawingPath = items[i].DrawingPath;
                    customStamp.inkThickness = items[i].inkThickness;
                    customStamp.inkColor = items[i].inkColor;
                    customStamp.Type = items[i].Type;
                    customStamp.AnnotationType = CPDFAnnotationType.Signature;
                    SignatureList.Add(customStamp);
                }
            }
        }

        private void Signature_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (signatureParam == null || viewControl == null)
            {
                PropertyChanged?.Invoke(this, (sender as ListBoxItem).DataContext as CPDFSignatureData);
            }
            else
            {
                FillForm(((sender as ListBoxItem).DataContext as CPDFSignatureData).SourcePath);
                viewControl.PDFViewTool.IsDocumentModified = true;
            }
        }

        private void FillForm(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                byte[] imageData = null;
                int imageWidth = 0;
                int imageHeight = 0;
                PDFHelp.ImagePathToByte(imagePath, ref imageData, ref imageWidth, ref imageHeight);
                if (imageData != null && imageWidth > 0 && imageHeight > 0)
                {
                    if (signatureWidget != null && signatureWidget.IsValid())
                    {
                        signatureWidget.UpdateApWithImage(imageData, imageWidth, imageHeight, C_Scale_Type.fitCenter, 0);
                        if (viewControl != null && viewControl.PDFViewTool != null)
                        {
                            viewControl.UpdateAnnotFrame();
                        }
                    }
                }
            }
        }

        private void CreateSignature_Click(object sender, RoutedEventArgs e)
        {
            CPDFCreateSignatureDialog createStampDialog = new CPDFCreateSignatureDialog();
            createStampDialog.Owner = Window.GetWindow(this);
            createStampDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            createStampDialog.ShowDialog();
            UpDataSignature(createStampDialog.cPDFSignatureData);
        }

        private void UpDataSignature(CPDFSignatureData SignatureData)
        {
            if (SignatureData != null)
            {
                SignatureList.Add(SignatureData);

                SignatureList Signature = Settings.Default.SignatureList;
                if (Signature == null)
                {
                    Signature = Settings.Default.SignatureList = new SignatureList();
                }
                Signature.Add(SignatureData);
                Settings.Default.Save();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button ThisButton = sender as Button;
            if (ThisButton != null)
            {
                CPDFSignatureData data = ThisButton.DataContext as CPDFSignatureData;
                if (data != null)
                {
                    int index = SignatureList.IndexOf(data);
                    if (index != -1)
                    {
                        CPDFDeleteDialog cPDFDeleteDialog = new CPDFDeleteDialog(LanguageHelper.CommonManager.GetString("Caption_Warning"), 
                            LanguageHelper.CommonManager.GetString("Warn_Delete"));
                        cPDFDeleteDialog.Owner = Window.GetWindow(this);
                        cPDFDeleteDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        cPDFDeleteDialog.ShowDialog();
                        if (!cPDFDeleteDialog.IsDelete)
                        {
                            PropertyChanged?.Invoke(this, null);
                            return;
                        }

                        if (data.SourcePath != null)
                        {
                            try
                            {
                                if (File.Exists(data.SourcePath))
                                {
                                    File.Delete(data.SourcePath);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        SignatureList.RemoveAt(index);

                        SignatureList Signature = Settings.Default.SignatureList;
                        if (Signature != null)
                        {
                            Signature.RemoveAt(index);
                            Settings.Default.Save();
                        }

                        PropertyChanged?.Invoke(this, null);
                    }
                }
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            signatureParam = null;
            viewControl = null;
        }
    }
}
