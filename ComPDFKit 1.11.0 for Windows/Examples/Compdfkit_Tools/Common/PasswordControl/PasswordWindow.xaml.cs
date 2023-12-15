using ComPDFKitViewer.PdfViewer;
using System;
using System.Windows;
using System.Windows.Input;
using Compdfkit_Tools.Helper;
using ComPDFKit.PDFDocument;

namespace Compdfkit_Tools.Common
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
        private CPDFDocument document;

        public PasswordType PasswordType = PasswordType.UserPassword;
        public delegate void DialogCloseEventHandler(object sender, PasswordEventArgs e);
        public event DialogCloseEventHandler DialogClosed;

        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void PasswordDialog_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordDialog.Canceled += PasswordDialog_Canceled;
            PasswordDialog.Confirmed += PasswordDialog_Confirmed;
            PasswordDialog.Closed += PasswordDialog_Closed;
        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            document = pdfViewer.Document;
        }
        
        public void InitWithDocument(CPDFDocument document)
        {
            this.document = document;
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
            if (document != null)
            {
                if(PasswordType == PasswordType.UserPassword)
                {
                    document.UnlockWithPassword(e);
                    if (document.IsLocked == false)
                    {
                        PasswordEventArgs passwordEventArgs = new PasswordEventArgs(e);
                        CloseWindow(passwordEventArgs);
                    }
                    else
                    {
                        PasswordDialog.SetShowError(errorMessage, Visibility.Visible);
                    }
                }
                else
                {
                    if (document.UnlockWithPassword(e))
                    {
                        if(document.CheckOwnerPassword(e))
                        {
                            PasswordEventArgs passwordEventArgs = new PasswordEventArgs(e);
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
