using Compdfkit_Tools.Data;
using Compdfkit_Tools.Properties;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI
{
    public partial class CPDFStampUI : UserControl
    {
        #region StandardStamp

        List<string> Path = new List<string>
        {
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Approved.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/NotApproved.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Completed.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Final.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Draft.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Confidential.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/ForPublicRelease.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/NotForPublicRelease.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/ForComment.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Void.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/PreliminaryResults.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/InformationOnly.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Accepted.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Rejected.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/Witness.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/InitialHere.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/SignHere.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/revised.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/PrivateMark1.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/PrivateMark2.png",
            "pack://application:,,,/Compdfkit_Tools;component/Asset/Resource/Annotation/StampIcons/PrivateMark3.png",
        };
        List<string> StampText = new List<string>
        {
            "Approved","NotApproved","Completed","Final","Draft","Confidential","ForPublicRelease","NotForPublicRelease",
            "ForComment","Void","PreliminaryResults","InformationOnly","Accepted","Rejected","Witness","InitialHere","SignHere",
            "revised","PrivateMark#1","PrivateMark#2","PrivateMark#3"
        };
        List<int> MaxWidth = new List<int>
        {
            218,292,234,130,150,280,386,461,282,121,405,366,30,30,133,133,133,173,30,30,30
        };
        List<int> MaxHeight = new List<int>
        {
           66,66,66,66,66,66,66,66,66,66,66,66,30,30,39,39,39,66,30,30,30
        };

        #endregion
        public ObservableCollection<CPDFStampData> StandardStampList { get; set; }
        public ObservableCollection<CPDFStampData> CustomStampList { get; set; }

        public event EventHandler<CPDFAnnotationData> PropertyChanged;

        public CPDFStampUI()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new CPDFStampData());
        } 

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            CPDFCreateStampDialog createStampDialog = new CPDFCreateStampDialog();
            createStampDialog.SetCreateHeaderIndex(0);
            createStampDialog.Owner = Window.GetWindow(this);
            createStampDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            createStampDialog.ShowDialog();
            UpDataCustomStamp(createStampDialog.cPDFStampData);
        }

        private void Image_Click(object sender, RoutedEventArgs e)
        {
            CPDFCreateStampDialog createStampDialog = new CPDFCreateStampDialog();
            createStampDialog.SetCreateHeaderIndex(1);
            createStampDialog.Owner = Window.GetWindow(this);
            createStampDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            createStampDialog.ShowDialog();
            UpDataCustomStamp(createStampDialog.cPDFStampData);
        }

        private void Standard_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PropertyChanged?.Invoke(this, (sender as ListBoxItem).DataContext as CPDFStampData);
        }

        private void Customize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PropertyChanged?.Invoke(this, (sender as ListViewItem).DataContext as CPDFStampData);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StandardStampList = new ObservableCollection<CPDFStampData>();
            CustomStampList = new ObservableCollection<CPDFStampData>();
            InitStandardStamp();
            LoadSettings();

            Binding Standardbinding = new Binding();
            Standardbinding.Source = this;
            Standardbinding.Path = new System.Windows.PropertyPath("StandardStampList");
            StandardListBox.SetBinding(ListBox.ItemsSourceProperty, Standardbinding);

            Binding Custombinding = new Binding();
            Custombinding.Source = this;
            Custombinding.Path = new System.Windows.PropertyPath("CustomStampList");
            CustomListBox.SetBinding(ListBox.ItemsSourceProperty, Custombinding);

            ICollectionView groupView = CollectionViewSource.GetDefaultView(CustomStampList);
            groupView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CPDFStampData.TypeText)));
        }

        public void InitStandardStamp()
        {
            for (int i = 0; i < Path.Count; i++)
            {
                CPDFStampData standardStamp = new CPDFStampData();
                standardStamp.Opacity = 1;
                standardStamp.SourcePath = Path[i];
                standardStamp.StampText = StampText[i];
                standardStamp.MaxWidth = MaxWidth[i];
                standardStamp.MaxHeight = MaxHeight[i];
                standardStamp.Type = StampType.STANDARD_STAMP;
                standardStamp.AnnotationType = CPDFAnnotationType.Stamp;
                StandardStampList.Add(standardStamp);
            }
        }

        /// <summary>
        /// Loading CacheStamp
        /// </summary>
        public void LoadSettings()
        {
            CustomStampList stamps = Settings.Default.CustomStampList;
            CustomStampList.Clear();
            if (stamps != null)
            {
                for (int i = 0; i < stamps.Count; i++)
                {
                    CPDFStampData customStamp = new CPDFStampData();
                    customStamp.Opacity = 1;
                    customStamp.StampText = stamps[i].StampText;
                    customStamp.StampTextDate = stamps[i].StampTextDate;
                    customStamp.MaxWidth = stamps[i].MaxWidth;
                    customStamp.MaxHeight = stamps[i].MaxHeight;
                    customStamp.SourcePath = stamps[i].SourcePath;
                    customStamp.Type = stamps[i].Type;
                    customStamp.TextSharp = stamps[i].TextSharp;
                    customStamp.TextColor = stamps[i].TextColor;
                    customStamp.IsCheckedTime = stamps[i].IsCheckedTime;
                    customStamp.IsCheckedDate = stamps[i].IsCheckedDate;
                    customStamp.AnnotationType = CPDFAnnotationType.Stamp;
                    CustomStampList.Add(customStamp);
                }
            }
        }

        public void UpDataCustomStamp(CPDFStampData oldstamp)
        {
            if (oldstamp != null)
            {
                CustomStampList.Add(oldstamp);
                CustomStampList stamps = Settings.Default.CustomStampList;
                if (stamps == null)
                {
                    stamps = Settings.Default.CustomStampList = new CustomStampList();
                }
                stamps.Add(oldstamp);
                Settings.Default.Save();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button ThisButton = sender as Button;
            if (ThisButton != null)
            {
                CPDFStampData stampData = ThisButton.DataContext as CPDFStampData;
                if (stampData != null)
                {
                    int index = CustomStampList.IndexOf(stampData);
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
                        if (stampData.SourcePath != null)
                        {
                            try
                            {
                                if (File.Exists(stampData.SourcePath))
                                {
                                    File.Delete(stampData.SourcePath);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        CustomStampList.RemoveAt(index);

                        CustomStampList stamps = Settings.Default.CustomStampList;
                        if (stamps != null)
                        {
                            stamps.RemoveAt(index);
                            Settings.Default.Save();
                        }
                        PropertyChanged?.Invoke(this, null);
                    }
                }
            }
        }
    }
}
