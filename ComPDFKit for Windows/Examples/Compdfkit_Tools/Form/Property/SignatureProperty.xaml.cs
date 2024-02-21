using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class SignatureProperty : UserControl
    {
        private WidgetSignArgs widgetArgs = null;
        private AnnotAttribEvent annotAttribEvent = null;

        bool IsLoadedData = false;

        public SignatureProperty()
        {
            InitializeComponent();
        }

        public void SetProperty(WidgetArgs Args, AnnotAttribEvent e)
        {
            widgetArgs = (WidgetSignArgs)Args;
            annotAttribEvent = e;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FormFieldCmb.SelectedIndex = (int)widgetArgs.FormField;
            IsLoadedData = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FormField, (sender as ComboBox).SelectedIndex);
                annotAttribEvent.UpdateAnnot();
            }
        }

    }
}
