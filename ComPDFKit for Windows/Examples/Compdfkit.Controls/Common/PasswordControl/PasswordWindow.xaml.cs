using ComPDFKit.PDFDocument;
using System;
using System.Windows;
using System.Windows.Input;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.Common
{
    public enum PasswordType : byte
    {
        UserPassword,
        OwnerPassword
    }

    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        private CPDFDocument pdfDoc;

        public PasswordType PasswordType = PasswordType.UserPassword;
        public delegate void DialogCloseEventHandler(object sender, PasswordEventArgs e);
        public event DialogCloseEventHandler DialogClosed;

        public string Password { get; private set; }

        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void PasswordDialog_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void InitDocument(CPDFDocument pdfDoc)
        {
            this.pdfDoc = pdfDoc;
            PasswordDialog.Canceled -= PasswordDialog_Canceled;
            PasswordDialog.Confirmed -= PasswordDialog_Confirmed;
            PasswordDialog.Closed -= PasswordDialog_Closed;
            PasswordDialog.Canceled += PasswordDialog_Canceled;
            PasswordDialog.Confirmed += PasswordDialog_Confirmed;
            PasswordDialog.Closed += PasswordDialog_Closed;
        }

        private void PasswordDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            PasswordDialog.Canceled -= PasswordDialog_Canceled;
            PasswordDialog.Confirmed -= PasswordDialog_Confirmed;
            PasswordDialog.Closed -= PasswordDialog_Closed;
        }

        private void PasswordDialog_Closed(object sender, EventArgs e)
        {
            PasswordEventArgs passwordEventArgs = new PasswordEventArgs(string.Empty);
            CloseWindow(passwordEventArgs);
        }

        private void PasswordDialog_Confirmed(object sender, string e)
        {
            string errorMessage = LanguageHelper.CommonManager.GetString("Tip_WrongPassword");
            if (pdfDoc != null)
            {
                if(PasswordType == PasswordType.UserPassword)
                {
                    pdfDoc.UnlockWithPassword(e);
                    if (pdfDoc.IsLocked == false)
                    {
                        PasswordEventArgs passwordEventArgs = new PasswordEventArgs(e);
                        Password = e;
                        CloseWindow(passwordEventArgs);
                    }
                    else
                    {
                        PasswordDialog.SetShowError(errorMessage, Visibility.Visible);
                    }
                }
                else
                {
                    if (pdfDoc.UnlockWithPassword(e))
                    {
                        if(pdfDoc.CheckOwnerPassword(e))
                        {
                            PasswordEventArgs passwordEventArgs = new PasswordEventArgs(e);
                            Password = e;
                            CloseWindow(passwordEventArgs);
                        }
                        else
                        {
                            PasswordDialog.SetShowError(errorMessage, Visibility.Visible);
                        }
                    }
                    else
                    {
                        PasswordDialog.SetShowError(errorMessage, Visibility.Visible);
                    }
                }
            }
        }

        private void PasswordDialog_Canceled(object sender, EventArgs e)
        {
            PasswordEventArgs passwordEventArgs = new PasswordEventArgs(string.Empty);
            CloseWindow(passwordEventArgs);
        }

        // The processing logic when the pop-up window is closed
        private void CloseWindow(PasswordEventArgs dialogResult)
        {
            // Trigger the close event and pass the return value
            DialogClosed?.Invoke(this, dialogResult);

            // Close the pop-up window
            Close();
        }

        private void PasswordWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }

    public class PasswordEventArgs : EventArgs
    {
        public string DialogResult { get; set; }

        public PasswordEventArgs(string dialogResult)
        {
            DialogResult = dialogResult;
        }
    }
}
