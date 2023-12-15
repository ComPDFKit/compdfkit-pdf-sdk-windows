using System.Linq;
using System.Windows;
using Compdfkit_Tools.Helper;
using Compdfkit_Tools.PDFControl;

namespace Compdfkit_Tools.PDFControl
{
    public partial class WatermarkOperationTypeDialog : Window
    {
        public WatermarkOperationTypeDialog()
        {
            InitializeComponent();
            Title = LanguageHelper.SecurityManager.GetString("Title_OperationType");
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (AddWatermarkRdo.IsChecked == true)
            {
                WatermarkListDialog watermarkListDialog = new WatermarkListDialog();
                OpenNewDialog(watermarkListDialog);

            }
            else if (RemoveWatermarkRdo.IsChecked == true)
            {
                RemoveWatermarkListDialog removeWatermarkListDialog = new RemoveWatermarkListDialog();
                OpenNewDialog(removeWatermarkListDialog);
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