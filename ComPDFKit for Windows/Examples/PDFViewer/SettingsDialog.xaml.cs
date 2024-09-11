using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ComPDFKit.Controls.Helper;

namespace PDFViewer
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public string AppVersion
        {
            get { return string.Join(".", Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(3)); }
        }

        public event EventHandler<string> LanguageChanged;
        public SettingsDialog()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            Title = App.MainResourceManager.GetString("Title_Settings");
            DataContext = this;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Divisor = GetDivisor();
            Properties.Settings.Default.Save(); 
            ComPDFKit.Controls.Data.CPDFAnnotationData.Author = Properties.Settings.Default.AnnotationAuthor;
        }

        private void EventSetter_ClickHandler(object sender, RoutedEventArgs e)
        {
            var webLocation = (sender as Button)?.Tag.ToString();
            if (!string.IsNullOrEmpty(webLocation))
            {
                Process.Start(webLocation);
            }
        }
        
        private int GetDivisor()
        {
            if (!DivisorTxb.IsValueValid)
            {
                return 10;
            }
            return int.TryParse(DivisorTxb.Text, out int divisor) ? divisor : 10;
        }

        private void SettingsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            HighlightFormTog.IsChecked = Properties.Settings.Default.IsHighlightFormArea;
            HighlightLinkTog.IsChecked = Properties.Settings.Default.IsHighlightLinkArea;
            FontSubsettingTog.IsChecked = Properties.Settings.Default.FontSubsetting;
            AuthorTxb.Text = Properties.Settings.Default.DocumentAuthor;
            AnnotatorTxb.Text = Properties.Settings.Default.AnnotationAuthor;
            SelectCurrentLanguage();
            DivisorTxb.Text = Properties.Settings.Default.Divisor.ToString();
        }
        
        private void SelectCurrentLanguage()
        {
            foreach (ComboBoxItem item in LanguageCmb.Items)
            {
                if (item.Tag.ToString() == App.CurrentCulture)
                {
                    item.IsSelected = true;
                }
            }
        }

        private void AuthorTxb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.DocumentAuthor = AuthorTxb.Text;
        }

        private void HighlightLinkTog_Click(object sender, RoutedEventArgs e)
        {
            if (HighlightLinkTog.IsChecked != null)
                Properties.Settings.Default.IsHighlightLinkArea = HighlightLinkTog.IsChecked.Value;
        }

        private void HighlightFormTog_Click(object sender, RoutedEventArgs e)
        {
            if (HighlightFormTog.IsChecked != null)
                Properties.Settings.Default.IsHighlightFormArea = HighlightFormTog.IsChecked.Value;
        }

        private void LanguageCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string language = ((ComboBoxItem)LanguageCmb.SelectedItem).Tag.ToString();
            if (language.Equals(App.CurrentCulture))
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show(App.MainResourceManager.GetString("Tip_Restart"),
                App.MainResourceManager.GetString("Tip_RestartTitle"), 
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.Cultrue = language;
                Properties.Settings.Default.Save();
                LanguageChanged?.Invoke(this, language);
            }
            else
            {
                SelectCurrentLanguage();
            }
        }

        private void AnnotatorTxb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.AnnotationAuthor = AnnotatorTxb.Text;
        }

        private void FontSubsettingTog_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FontSubsetting = FontSubsettingTog.IsChecked.Value;
        }
    }
}
