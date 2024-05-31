using System.Windows;
using System.Windows.Controls;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Helper;
using ComPDFKit.PDFDocument;

namespace PasswordBoxPlus.Helper
{
    public class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordHelper),
            new PropertyMetadata(new PropertyChangedCallback(OnPropertyChanged)));

        public static string GetPassword(DependencyObject d)
        {
            return (string)d.GetValue(PasswordProperty);
        }
        public static void SetPassword(DependencyObject d, string value)
        {
            d.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(string), typeof(PasswordHelper),
            new PropertyMetadata(new PropertyChangedCallback(OnAttachChanged)));

        public static string GetAttach(DependencyObject d)
        {
            return (string)d.GetValue(AttachProperty);
        }
        public static void SetAttach(DependencyObject d, string value)
        {
            d.SetValue(AttachProperty, value);
        }

        static bool isUpdating = false;
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            passwordBox.PasswordChanged -= passwordBox_PasswordChanged;
            if (!isUpdating)
                (d as PasswordBox).Password = e.NewValue.ToString();
            passwordBox.PasswordChanged += passwordBox_PasswordChanged;
        }

        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = d as PasswordBox;
            passwordBox.PasswordChanged += passwordBox_PasswordChanged;
        }

        private static void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            isUpdating = true;
            SetPassword(passwordBox, passwordBox.Password);
            isUpdating = false;
        }
        
        public static bool UnlockWithOwnerPassword(CPDFDocument document)
        {
            PasswordWindow window = new PasswordWindow();
            window.InitDocument(document);
            window.PasswordType = PasswordType.OwnerPassword;
            if (Application.Current.MainWindow != null) 
                window.Owner = Window.GetWindow(Application.Current.MainWindow);
            window.PasswordDialog.SetShowText(document.FileName + " " + LanguageHelper.CommonManager.GetString("Tip_Permission"));
            window.ShowDialog();
            return !IsOwnerLocked(document);
        }
        
        public static bool IsOwnerLocked(CPDFDocument document)
        {
            var info = document.GetPermissionsInfo();
            return !info.AllowsCopying || !info.AllowsPrinting || !info.AllowsCommenting ||
                   !info.AllowsHighQualityPrinting || !info.AllowsDocumentChanges || !info.AllowsDocumentAssembly;
        }
    }
}
