using Compdfkit_Tools.PDFControl;

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
using System.Windows.Navigation;
using System.IO;
using System.ComponentModel;
using System.Resources;
using System.Runtime.CompilerServices;
using Dragablz;
using Compdfkit_Tools.Helper;
using System.Windows.Controls.Primitives;
using Compdfkit_Tools.Common;
using Compdfkit_Tools.Data;
using ComPDFKit.PDFDocument;
using System.Reflection;
using System.Net.Http;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Property
        private PDFViewControl passwordViewer;
        private PDFViewControl pdfViewControl = new PDFViewControl();
        private string[] oldAndNewFilePath;
        public string AppInfo
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name + " " + string.Join(".", Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3)); }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = this; 
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordUI.Closed -= PasswordUI_Closed;
            PasswordUI.Canceled -= PasswordUI_Canceled;
            PasswordUI.Confirmed -= PasswordUI_Confirmed;
            PasswordUI.Confirmed += PasswordUI_Confirmed;
            PasswordUI.Canceled += PasswordUI_Canceled;
            PasswordUI.Closed += PasswordUI_Closed;
            HomePageControl.OpenFileEvent -= OpenFileEventHandler;
            HomePageControl.OpenFileEvent += OpenFileEventHandler;
            TabControl.SelectionChanged -= TabControl_SelectionChanged;
            TabControl.SelectionChanged += TabControl_SelectionChanged;
            FirstLoadFile();
            CPDFAnnotationData.Author = Properties.Settings.Default.DocumentAuthor;
        }

        private void FirstLoadFile()
        {
            if (Properties.Settings.Default.IsLoadLastFileNeeded)
            {
                if (LoadLastOpenedDocuments())
                {
                    TabControl.SelectedIndex = Properties.Settings.Default.LastSelectedFileIndex;
                    if (TabControl.SelectedIndex == -1)
                    {
                        HomePageButton.IsToggled = true;
                    }
                    Properties.Settings.Default.IsLoadLastFileNeeded = false;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    HomePageButton.IsToggled = true;
                }
            }
            HomePageButton.IsToggled = true;
        }

        private bool LoadLastOpenedDocuments()
        {
            if (Properties.Settings.Default.LastOpenedFiles != null && Properties.Settings.Default.LastOpenedFiles.Count > 0)
            {
                foreach (var filePath in Properties.Settings.Default.LastOpenedFiles)
                {
                    TabControlLoadDocument(filePath);
                }
                return true;
            }
            return false;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabablz = sender as Dragablz.TabablzControl;
            if (tabablz.SelectedIndex != -1)
            {
                HomePageButton.IsToggled = false;
            }
        }

        private void OpenFileEventHandler(object sender, OpenFileEventArgs args)
        {
            if (args.OperationType == FileOperationType.OpenFileDirectly)
            {
                TabControlLoadDocument(args.FilePath, args.FeatureName);
            }
            else if (args.OperationType == FileOperationType.CreateNewFile)
            {
                bool confirmCreate = false;
                BlankPageSetting blankPageSetting = new BlankPageSetting();
                CreateBlankPageSettingDialog createBlankPageSettingDialog = new CreateBlankPageSettingDialog()
                {
                    Owner = this
                };
                createBlankPageSettingDialog.CreateBlankPage += (o, setting) =>
                {
                    blankPageSetting = setting;
                    confirmCreate = true;
                };
                createBlankPageSettingDialog.ShowDialog();
                if (!confirmCreate)
                {
                    return;
                }

                TabItemExt tabItem = new TabItemExt();
                MainPage viewPage = new MainPage();
                CPDFDocument document = CPDFDocument.CreateDocument();
                document.SetInfo(new CPDFInfo
                {
                    Author = Properties.Settings.Default.DocumentAuthor,
                    Creator = Assembly.GetExecutingAssembly().GetName().Name,
                    CreationDate = DateTime.Now.ToString(),
                    Subject = "Document",
                    Producer = Assembly.GetExecutingAssembly().GetName().Name,
                    Keywords = "Document",
                    Version = string.Join(".", Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3))
                });
                document.InsertPage(0, blankPageSetting.Size.Width * 7.2 / 2.54, blankPageSetting.Size.Height * 7.2 / 2.54, "");
                if (blankPageSetting.Orientation == Orientation.Horizontal)
                {
                    document.RotatePage(0, 1);
                }
                viewPage.CheckExistBeforeOpenFileEvent -= ViewPage_CheckExistBeforeOpenFileEvent;
                viewPage.FileChangeEvent -= ViewPage_FileChangeEvent;
                viewPage.AfterSaveAsFileEvent -= ViewPage_AfterSaveAsFileEvent;
                viewPage.CheckExistBeforeOpenFileEvent += ViewPage_CheckExistBeforeOpenFileEvent;
                viewPage.FileChangeEvent += ViewPage_FileChangeEvent;
                viewPage.AfterSaveAsFileEvent += ViewPage_AfterSaveAsFileEvent;

                viewPage.InitWithDocument(document);
                tabItem.Content = viewPage;
                tabItem.IsSelected = true;
                tabItem.FileName = "Blank Page.pdf";
                tabItem.Tag = "Blank Page.pdf";

                TabControl.Items.Add(tabItem);

                viewPage.CanSave = true;
                viewPage.pdfViewer.PDFView.UndoManager.CanSave = true;
            }
            else
            {
                string filePath = CommonHelper.GetExistedPathOrEmpty();
                if (filePath != string.Empty)
                {
                    TabControlLoadDocument(filePath);
                }
            }
        }

        private void ViewPage_AfterSaveAsFileEvent(object sender, string e)
        {
            if (sender is MainPage mainPage)
            {
                var tabItem = (from object t in TabControl.Items select t as TabItemExt).FirstOrDefault(item => Equals(item.Content, mainPage));
                if (tabItem != null)
                {
                    tabItem.FileName = Path.GetFileName(e);
                    tabItem.Tag = e;
                }
                mainPage.pdfViewer.PDFView.CloseDocument();
                mainPage.pdfViewer.PDFView.InitDocument(e);
                mainPage.pdfViewer.PDFView.Load();

                App.OpenedFilePathList.Add(e);
            }
        }

        private void TabControlLoadDocument(string filePath, string featureName = "")
        {
            if (App.OpenedFilePathList.Contains(filePath))
            {
                for (int i = 0; i < App.Current.Windows.Count; i++)
                {
                    MainWindow win = App.Current.Windows[i] as MainWindow;
                    if (win != null)
                    {
                        var items = win.TabControl.Items;

                        foreach (TabItemExt item in items)
                        {
                            if (item.Tag.ToString().ToLower() == filePath.ToLower())
                            {
                                win.Activate();
                                item.IsSelected = true;
                                (item.Content as MainPage).SetFeatureMode(featureName);
                                return;
                            }
                        }
                    }
                }
            }

            TabItemExt tabItem = new TabItemExt();
            MainPage viewPage = new MainPage();

            viewPage.CheckExistBeforeOpenFileEvent -= ViewPage_CheckExistBeforeOpenFileEvent;
            viewPage.FileChangeEvent -= ViewPage_FileChangeEvent;
            viewPage.AfterSaveAsFileEvent -= ViewPage_AfterSaveAsFileEvent;
            viewPage.CheckExistBeforeOpenFileEvent += ViewPage_CheckExistBeforeOpenFileEvent;
            viewPage.FileChangeEvent += ViewPage_FileChangeEvent;
            viewPage.AfterSaveAsFileEvent += ViewPage_AfterSaveAsFileEvent;

            passwordViewer = new PDFViewControl();
            passwordViewer.PDFView.InitDocument(filePath);
            if (passwordViewer.PDFView.Document == null)
            {
                MessageBox.Show("Open File Failed");
                return;
            }

            if (passwordViewer.PDFView.Document.IsLocked)
            {
                PasswordUI.SetShowText(System.IO.Path.GetFileName(filePath) + " " + LanguageHelper.CommonManager.GetString("Tip_Encrypted"));
                PasswordUI.ClearPassword();
                PopupBorder.Visibility = Visibility.Visible;
                PasswordUI.Visibility = Visibility.Visible;
            }
            else
            {
                viewPage.InitWithFilePath(filePath);
                tabItem.Content = viewPage;
                tabItem.IsSelected = true;
                tabItem.FileName = Path.GetFileName(filePath);
                tabItem.Tag = filePath;

                App.OpenedFilePathList.Add(filePath);
                TabControl.Items.Add(tabItem);
                viewPage.Loaded += (sender, e) =>
                {
                    viewPage.SetFeatureMode(featureName);
                 };
            }
        }

        private void PasswordUI_Closed(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }

        private void PasswordUI_Canceled(object sender, EventArgs e)
        {
            PopupBorder.Visibility = Visibility.Collapsed;
            PasswordUI.Visibility = Visibility.Collapsed;
        }

        private void PasswordUI_Confirmed(object sender, string e)
        {
            if (passwordViewer != null && passwordViewer.PDFView != null && passwordViewer.PDFView.Document != null)
            {
                passwordViewer.PDFView.Document.UnlockWithPassword(e);
                if (passwordViewer.PDFView.Document.IsLocked == false)
                {
                    PasswordUI.SetShowError("", Visibility.Collapsed);
                    PasswordUI.ClearPassword();
                    PasswordUI.Visibility = Visibility.Collapsed;
                    PopupBorder.Visibility = Visibility.Collapsed;
                    pdfViewControl = passwordViewer;

                    string filePath = passwordViewer.PDFView.Document.FilePath;
                    TabItemExt tabItem = new TabItemExt();
                    MainPage viewPage = new MainPage();

                    tabItem.Content = viewPage;
                    tabItem.IsSelected = true;
                    tabItem.FileName = Path.GetFileName(filePath);
                    tabItem.Tag = filePath;

                    viewPage.SetPDFViewer(pdfViewControl);
                    App.OpenedFilePathList.Add(filePath);

                    TabControl.Items.Add(tabItem);
                }
                else
                {
                    PasswordUI.SetShowError("error", Visibility.Visible);
                }
            }

        }


        private void ViewPage_FileChangeEvent(object sender, EventArgs e)
        {
            for (int i = 0; i < App.Current.Windows.Count; i++)
            {
                MainWindow win = App.Current.Windows[i] as MainWindow;
                if (win != null)
                {
                    var items = win.TabControl.Items;

                    foreach (TabItemExt item in items)
                    {
                        if (item.Tag.ToString().ToLower() == oldAndNewFilePath[1].ToLower())
                        {
                            item.IsSelected = true;
                            item.FileName = Path.GetFileName(oldAndNewFilePath[0]);
                            item.IsSelected = true;
                            item.Tag = oldAndNewFilePath[0];
                        }
                    }
                }
            }

            for (int i = 0; i < App.OpenedFilePathList.Count; i++)
            {
                if (oldAndNewFilePath[1].ToLower() == App.OpenedFilePathList[i].ToLower())
                {
                    App.OpenedFilePathList[i] = oldAndNewFilePath[0];
                    break;
                }
            }
        }

        private bool ViewPage_CheckExistBeforeOpenFileEvent(string[] arg)
        {
            if (App.OpenedFilePathList.Contains(arg[0]))
            {
                for (int i = 0; i < App.Current.Windows.Count; i++)
                {
                    MainWindow win = App.Current.Windows[i] as MainWindow;
                    if (win != null)
                    {
                        var items = win.TabControl.Items;

                        foreach (TabItemExt item in items)
                        {
                            if (item.Tag.ToString().ToLower() == arg[0].ToLower())
                            {
                                win.Activate();
                                item.IsSelected = true;
                                return true;
                            }
                        }
                    }
                }
            }
            oldAndNewFilePath = arg;
            return false;
        }

        public class TabItemExt : TabItem, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private string _fileName;
            public string FileName
            {
                get
                {
                    return _fileName;
                }
                set
                {
                    if (_fileName != value)
                    {
                        _fileName = value;
                        OnPropertyChanged();
                    }
                }
            }

            public void OnPropertyChanged([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private DragablzItem FindParentDragablzItem(DependencyObject element)
        {
            while (element != null && !(element is DragablzItem))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            return element as DragablzItem;
        }

        private TabControl FindParentTabControl(DependencyObject element)
        {
            while (element != null && !(element is TabControl))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            return element as TabControl;
        }

        private void CloseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dragablzItem = FindParentDragablzItem(button);
            var tabControl = FindParentTabControl(dragablzItem);
            MainPage mainPage = (dragablzItem.Content as TabItemExt).Content as MainPage;
            if (mainPage == null)
            {
                return;
            }
            if (mainPage.CanSave)
            {
                string fileName = (dragablzItem.Content as TabItemExt).FileName;
                var message = fileName + " " + LanguageHelper.CommonManager.GetString("Warn_NotSave");
                var result = MessageBox.Show(message, LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    mainPage.SaveFile();
                    App.OpenedFilePathList.Remove((dragablzItem.Content as TabItemExt).Tag.ToString());
                    tabControl.Items.Remove(dragablzItem.Content);

                }
                else if (result == MessageBoxResult.No)
                {
                    App.OpenedFilePathList.Remove((dragablzItem.Content as TabItemExt).Tag.ToString());
                    tabControl.Items.Remove(dragablzItem.Content);
                }
                else
                {

                }
            }
            else
            {
                App.OpenedFilePathList.Remove((dragablzItem.Content as TabItemExt).Tag.ToString());
                tabControl.Items.Remove(dragablzItem.Content);
            }

            if (tabControl.Items.Count == 0)
            {
                HomePageButton.IsToggled = true;
            }
        }

        private void DefaultAddButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = CommonHelper.GetExistedPathOrEmpty();
            if (filePath != string.Empty)
            {
                TabControlLoadDocument(filePath);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            int count = TabControl.Items.Count;
            while (count > 0)
            {
                TabItemExt item = TabControl.Items[0] as TabItemExt;

                MainPage mainPage = item.Content as MainPage;
                if (mainPage == null)
                {
                    count--;
                    continue;
                }
                if (mainPage.CanSave)
                {
                    string fileName = item.FileName;
                    var message = fileName + " " + LanguageHelper.CommonManager.GetString("Warn_NotSave");
                    var result = MessageBox.Show(message, LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        mainPage.SaveFile();
                        App.OpenedFilePathList.Remove(item.Tag.ToString());
                        TabControl.Items.Remove(item);
                        count--;
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        App.OpenedFilePathList.Remove(item.Tag.ToString());
                        TabControl.Items.Remove(item);
                        count--;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    App.OpenedFilePathList.Remove(item.Tag.ToString());
                    TabControl.Items.Remove(item);
                    count--;
                }
            }

            if (count == 0)
            {
                SystemCommands.CloseWindow(this);
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog settingsDialog = new SettingsDialog();
            settingsDialog.LanguageChanged -= SettingsDialog_LanguageChanged;
            settingsDialog.LanguageChanged += SettingsDialog_LanguageChanged;
            settingsDialog.Owner = this;
            settingsDialog.ShowDialog();
        }

        private void SettingsDialog_LanguageChanged(object sender, string e)
        {
            foreach (var tab in TabControl.Items)
            {
                var item = tab as TabItemExt;
                if (item?.Content is MainPage mainPage)
                {
                    if (mainPage.CanSave)
                    {
                        string fileName = item.FileName;
                        var message = fileName + " " + LanguageHelper.CommonManager.GetString("Warn_NotSave");
                        var result = MessageBox.Show(message, LanguageHelper.CommonManager.GetString("Caption_Warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            mainPage.SaveFile();
                        }
                        else
                        {
                            break;
                        }
                    }
                    mainPage.pdfViewer.PDFView.CloseDocument();
                }
            }

            if (Properties.Settings.Default.LastOpenedFiles == null)
            {
                Properties.Settings.Default.LastOpenedFiles = new System.Collections.Specialized.StringCollection();
            }
            Properties.Settings.Default.LastOpenedFiles.Clear();
            foreach (var filePath in App.OpenedFilePathList)
            {
                Properties.Settings.Default.LastOpenedFiles.Add(filePath);
            }
            Properties.Settings.Default.LastSelectedFileIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.IsLoadLastFileNeeded = true;
            Properties.Settings.Default.Save();
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            this.Close();
        }


        private void HomePageButton_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is HomePageButton homePageButton && homePageButton.IsToggled)
            {
                TabControl.SelectedIndex = -1;
            }
            else
            {
                TabControl.SelectedIndex = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool UpdateProper<T>(ref T properValue,
    T newValue,
    [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return false;

            properValue = newValue;
            OnPropertyChanged(properName);
            return true;
        }

        private bool HomePageButton_QueryLock(object sender, HomePageButton.QueryLockEventArgs e)
        {
            return TabControl.Items.Count <= 0;
        }
    }
}
