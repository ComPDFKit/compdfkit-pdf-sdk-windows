using System.Linq;
using System.Windows;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;

namespace Compdfkit_Tools.PDFControl
{
    public partial class SecurityOperationTypeDialog : Window
    {
        public SecurityOperationTypeDialog()
        {
            InitializeComponent();
            Title = LanguageHelper.SecurityManager.GetString("Title_OperationType");
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (RdoAddPassword.IsChecked == true)
            {
                EncryptionDialog dialog = new EncryptionDialog();
                OpenNewDialog(dialog);
            }
            else if (RdoRemovePassword.IsChecked == true)
            {
                DecryptionDialog dialog = new DecryptionDialog();
                OpenNewDialog(dialog);
            }
            else
            {
                MessageBox.Show("Please select an operation type.");
            }
        }

        private void OpenNewDialog(Window newDialog)
        {
            newDialog.Owner = this.Owner;
            this.Close();
            newDialog.ShowDialog();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}