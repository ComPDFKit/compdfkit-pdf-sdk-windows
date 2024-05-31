using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Common;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControl;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFNoteUI : UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        internal StickyNoteParam stickyParam { get;private set; }
        private CPDFTextAnnotation textAnnot;
        private PDFViewControl viewControl;

        public CPDFNoteUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged -= ColorPickerControl_ColorChanged;
            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
            CPDFAnnotationPreviewerControl.DrawNotePreview(GetNoteData());
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (stickyParam == null)
            {
                PropertyChanged?.Invoke(this, GetNoteData());
            }
            else
            {
                SolidColorBrush colorBrush= ColorPickerControl.Brush as SolidColorBrush;
                if (textAnnot!=null && textAnnot.IsValid() && colorBrush!=null)
                {
                    byte[] color = new byte[3]
                    {
                        colorBrush.Color.R,
                        colorBrush.Color.G,
                        colorBrush.Color.B
                    };
                    
                    if (viewControl != null && !textAnnot.Color.SequenceEqual(color))
                    {
                        StickyNoteAnnotHistory history = new StickyNoteAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                        
                        textAnnot.SetColor(color);
                        textAnnot.UpdateAp();
                        viewControl.UpdateAnnotFrame();
                        
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    }
                }
            }
            CPDFAnnotationPreviewerControl.DrawNotePreview(GetNoteData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (stickyParam == null)
            {
                PropertyChanged?.Invoke(this, GetNoteData());
            }
            else
            {
                if (textAnnot != null && textAnnot.IsValid())
                {
                    if (viewControl != null && textAnnot.GetContent() != NoteTextBox.Text)
                    {
                        StickyNoteAnnotHistory history = new StickyNoteAnnotHistory();
                        history.PDFDoc = viewControl.GetCPDFViewer().GetDocument();
                        history.Action = HistoryAction.Update;
                        history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                        
                        textAnnot.SetContent(NoteTextBox.Text);
                        viewControl.UpdateAnnotFrame();
                        
                        history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, textAnnot.Page.PageIndex, textAnnot);
                        viewControl.GetCPDFViewer().UndoManager.AddHistory(history);
                    }
                }
            }
        }

        public void SetPresentAnnotAttrib(StickyNoteParam param,CPDFTextAnnotation annot,PDFViewControl view)
        {
            this.stickyParam = null;
            this.viewControl = null;
            this.textAnnot = null;

            if(param != null)
            {
                ColorPickerControl.Brush = new SolidColorBrush(Color.FromRgb(
                    param.StickyNoteColor[0],
                    param.StickyNoteColor[1],
                    param.StickyNoteColor[2]));
                NoteTextBox.Text = param.Content;
                ColorPickerControl.SetCheckedForColor(Color.FromRgb(
                    param.StickyNoteColor[0],
                    param.StickyNoteColor[1],
                    param.StickyNoteColor[2]));
            }
           
            this.stickyParam = param;
            this.textAnnot=annot;
            this.viewControl = view;
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
