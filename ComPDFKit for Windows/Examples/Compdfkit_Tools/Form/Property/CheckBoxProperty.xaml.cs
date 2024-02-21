using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CheckBoxProperty : UserControl
    {
        private WidgetCheckBoxArgs widgetArgs = null;
        private AnnotAttribEvent annotAttribEvent = null;

        bool IsLoadedData = false;
        public CheckBoxProperty()
        {
            InitializeComponent();
        }


        #region Loaded

        public void SetProperty(WidgetArgs Args, AnnotAttribEvent e)
        {
            widgetArgs = (WidgetCheckBoxArgs)Args;
            annotAttribEvent = e;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FieldNameText.Text = widgetArgs.FieldName;
            FormFieldCmb.SelectedIndex = (int)widgetArgs.FormField;
            BorderColorPickerControl.SetCheckedForColor(widgetArgs.LineColor);
            BackgroundColorPickerControl.SetCheckedForColor(widgetArgs.BgColor);
            CheckButtonStyleCmb.SelectedIndex = (int)widgetArgs.CheckStyle;
            chkSelected.IsChecked = widgetArgs.IsChecked;
            IsLoadedData = true;

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoadedData = false;
        }

        #endregion

        private void FieldNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FieldName, (sender as TextBox).Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void FormFieldCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FormField, (sender as ComboBox).SelectedIndex);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void BorderColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Color, ((SolidColorBrush)BorderColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void BackgroundColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.FillColor, ((SolidColorBrush)BackgroundColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void CheckButtonStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.CheckStyle, (sender as ComboBox).SelectedIndex);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void chkSelected_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsChecked, true);
                annotAttribEvent.UpdateAnnot();
            }
        }

        private void chkSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoadedData)
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.IsChecked, false);
                annotAttribEvent.UpdateAnnot();
            }
        }
    }
}
