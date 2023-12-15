using ComPDFKit.Import;
using Compdfkit_Tools.Data;
using Compdfkit_Tools.Properties;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI
{
    public partial class CPDFSignatureUI : UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        public ObservableCollection<CPDFSignatureData> SignatureList { get; set; }
        private WidgetSignArgs widgetSignArgs;
        private CPDFViewer pdfViewer;

        public CPDFSignatureUI()
        {
            InitializeComponent();
        }
        public void SetFormProperty(WidgetArgs Args, CPDFViewer cPDFViewer)
        {
            widgetSignArgs = (WidgetSignArgs)Args;
            pdfViewer = cPDFViewer;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SignatureList = new ObservableCollection<CPDFSignatureData>();
            Binding binding = new Binding();
            binding.Source = SignatureList;
            SignatureListBox.SetBinding(ListBox.ItemsSourceProperty, binding);

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
                    customStamp.Type = (SignatureType)items[i].Type;
                    customStamp.AnnotationType = CPDFAnnotationType.Signature;
                    SignatureList.Add(customStamp);
                }
            }
        }

        private void Signature_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (widgetSignArgs == null || pdfViewer == null)
            {
                PropertyChanged?.Invoke(this, (sender as ListBoxItem).DataContext as CPDFSignatureData);
            }
            else
            {
                FillForm(((sender as ListBoxItem).DataContext as CPDFSignatureData).SourcePath);
            }
        }

        private void FillForm(string ImagePath)
        {
            if (!string.IsNullOrEmpty(ImagePath))
            {
                using (FileStream fileData = File.OpenRead(ImagePath))
                {
                    BitmapFrame frame = null;
                    BitmapDecoder decoder = BitmapDecoder.Create(fileData, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    if (decoder != null && decoder.Frames.Count > 0)
                    {
                        frame = decoder.Frames[0];
                    }
                    if (frame != null)
                    {
                        byte[] ImageArray = new byte[frame.PixelWidth * frame.PixelHeight * 4];
                        int ImageWidth = frame.PixelWidth;
                        int ImageHeight = frame.PixelHeight;
                        frame.CopyPixels(ImageArray, frame.PixelWidth * 4, 0);
                        widgetSignArgs?.UpdateApWithImage(ImageArray, ImageWidth, ImageHeight, C_Scale_Type.fitCenter, 0);
                        pdfViewer?.ReloadVisibleAnnots();
                        pdfViewer.UndoManager.CanSave = true;
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
            widgetSignArgs = null;
            pdfViewer = null;
        }
    }
}
