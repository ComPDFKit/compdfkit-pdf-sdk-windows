using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControl
{

    public class BlankPageSetting
    {
        public Size Size { get; set; } = new Size(210, 297);
        public Orientation Orientation { get; set; } = Orientation.Vertical;
    }

    public partial class CreateBlankPageSettingDialog : Window
    {
        public event EventHandler<BlankPageSetting> CreateBlankPage;

        public CreateBlankPageSettingDialog()
        {
            InitializeComponent();
            Title = LanguageHelper.CommonManager.GetString("Title_NewFile");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var blankPageSetting = new BlankPageSetting
            {
                Size = GetSettingSize(),
                Orientation = GetSettingOrientation()
            };
            CreateBlankPage?.Invoke(this, blankPageSetting);
            Close();
        }

        private Orientation GetSettingOrientation()
        {
            return HorizontalRdo.IsChecked == true ? Orientation.Horizontal : Orientation.Vertical;
        }

        private Size GetSettingSize()
        {
            if (A3Rdo.IsChecked == true)
                return new Size(297, 420);
            if (A4Rdo.IsChecked == true)
                return new Size(210, 297);
            if (A5Rdo.IsChecked == true)
                return new Size(148, 219);

            return new Size(210, 297);

        }
    }
}