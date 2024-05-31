using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Common
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : UserControl
    {
        public event EventHandler Closed;

        public event EventHandler Canceled;

        public event EventHandler<string> Confirmed;
        public PasswordDialog()
        {
            InitializeComponent();
        }

        public void SetShowText(string newText)
        {
            FileEncryptText.Text = newText;
        }

        private void PasswordDialogClose_Click(object sender, RoutedEventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void PasswordDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        private void PasswordDialogConfirm_Click(object sender, RoutedEventArgs e)
        {
            Confirmed?.Invoke(this, PasswordBoxText.Password);
        }

        public void SetShowError(string errorText,Visibility visible)
        {
            ErrorTipsText.Text = errorText;
            ErrorTipsText.Visibility = visible;
        }

        public void ClearPassword()
        {
            PasswordBoxText.Password = string.Empty;
            SetShowError(string.Empty, Visibility.Collapsed);
        }
    }
}
