using ComPDFKit.PDFAnnotation.Form;
using ComPDFKit.PDFAnnotation;
using ComPDFKitViewer;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControl
{
    public partial class RadioButtonProperty : UserControl
    {
        private WidgetRadioButtonArgs widgetArgs = null;
        private AnnotAttribEvent annotAttribEvent = null; 
        private CPDFViewer pdfViewer;

        bool IsLoadedData = false;
        public RadioButtonProperty()
        {
            InitializeComponent();
        }

        #region Loaded
        public void SetProperty(WidgetArgs Args, AnnotAttribEvent e, CPDFViewer cPDFViewer)
        {
            widgetArgs = (WidgetRadioButtonArgs)Args;
            annotAttribEvent = e;
            pdfViewer = cPDFViewer;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FieldNameText.Text = widgetArgs.FieldName;
            FormFieldCmb.SelectedIndex = (int)widgetArgs.FormField;
            BorderColorPickerControl.SetCheckedForColor(widgetArgs.LineColor);
            BackgroundColorPickerControl.SetCheckedForColor(widgetArgs.BgColor);
            RadioButtonStyleCmb.SelectedIndex = (int)widgetArgs.CheckStyle;
            chkSelected.IsChecked = widgetArgs.IsChecked;

            if (IsShowWarning())
            {
                WarningPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                WarningPanel.Visibility = Visibility.Visible;
            }

            IsLoadedData = true;
        }

        private bool IsShowWarning()
        {
            int count = 0;
            var page = pdfViewer.Document.PageAtIndex(pdfViewer.CurrentIndex);
            List<CPDFAnnotation> annotList = page.GetAnnotations();
            if (annotList != null && annotList.Count > 0)
            {
                count = annotList.Where(x => x.Type == C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET)
                    .Cast<CPDFWidget>().Where(x => x.WidgeType == C_WIDGET_TYPE.WIDGET_RADIOBUTTON).Count();
            }

            return count>1;
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

        private void RadioButtonStyleCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
