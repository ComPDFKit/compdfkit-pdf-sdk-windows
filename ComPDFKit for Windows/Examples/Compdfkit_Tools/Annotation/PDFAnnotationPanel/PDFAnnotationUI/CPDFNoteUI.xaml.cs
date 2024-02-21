using Compdfkit_Tools.Common;
using Compdfkit_Tools.Data;
using ComPDFKitViewer.AnnotEvent;
using ComPDFKitViewer;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFNoteUI : UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        internal AnnotAttribEvent annotAttribEvent { get;private set; }

        public CPDFNoteUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
            CPDFAnnotationPreviewerControl.DrawNotePreview(GetNoteData());
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetNoteData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.Color, ((SolidColorBrush)ColorPickerControl.Brush).Color);
                annotAttribEvent.UpdateAnnot();
            }
            CPDFAnnotationPreviewerControl.DrawNotePreview(GetNoteData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (annotAttribEvent == null)
            {
                PropertyChanged?.Invoke(this, GetNoteData());
            }
            else
            {
                annotAttribEvent.UpdateAttrib(AnnotAttrib.NoteText, NoteTextBox.Text);
                annotAttribEvent.UpdateAnnot();
            }
        }

        public void SetPresentAnnotAttrib(AnnotAttribEvent annotAttribEvent)
        {
            this.annotAttribEvent = null;
            ColorPickerControl.Brush = new SolidColorBrush((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);
            NoteTextBox.Text = (string)annotAttribEvent.Attribs[AnnotAttrib.NoteText];
            this.annotAttribEvent = annotAttribEvent;

            if (annotAttribEvent.Attribs != null && annotAttribEvent.Attribs.ContainsKey(AnnotAttrib.Color))
            {
                ColorPickerControl.SetCheckedForColor((Color)annotAttribEvent.Attribs[AnnotAttrib.Color]);
            }
        }

        public CPDFNoteData GetNoteData()
        {
            CPDFNoteData pdfNoteData = new CPDFNoteData();
            pdfNoteData.AnnotationType = CPDFAnnotationType.Note;
            pdfNoteData.BorderColor = ((SolidColorBrush)ColorPickerControl.Brush).Color;
            pdfNoteData.Note = NoteTextBox.Text;
            return  pdfNoteData;
        }
    }
}
