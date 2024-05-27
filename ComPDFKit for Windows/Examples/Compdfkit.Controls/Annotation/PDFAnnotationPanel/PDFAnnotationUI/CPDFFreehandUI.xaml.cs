using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Tool.Help;
using ComPDFKit.Controls.Data;
using ComPDFKit.Controls.PDFControl;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComPDFKit.Tool.UndoManger;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFFreehandUI : UserControl
    {
        public event EventHandler<CPDFAnnotationData> PropertyChanged;
        public event EventHandler<bool> EraseClickHandler;
        public event EventHandler<double> EraseChangeHandler;

        private InkParam inkParam = null;
        private CPDFInkAnnotation cPDFAnnotation = null;
        private PDFViewControl pdfViewerControl = null;

        public CPDFFreehandUI()
        {
            InitializeComponent();
            ColorPickerControl.ColorChanged -= ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged -= CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged -= CPDFThicknessControl_ThicknessChanged;
            ColorPickerControl.ColorChanged += ColorPickerControl_ColorChanged;
            CPDFOpacityControl.OpacityChanged += CPDFOpacityControl_OpacityChanged;
            CPDFThicknessControl.ThicknessChanged += CPDFThicknessControl_ThicknessChanged;
            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());
            EraseThickness.ThicknessChanged -= EraseThickness_ThicknessChanged;
            EraseThickness.ThicknessChanged += EraseThickness_ThicknessChanged;
        }

        private void EraseThickness_ThicknessChanged(object sender, EventArgs e)
        {
            EraseChangeHandler?.Invoke(this, EraseThickness.Thickness);
            EraseCircle.Width = EraseThickness.Thickness * 6;
            EraseCircle.Height = EraseThickness.Thickness * 6;
        }

        private void CPDFOpacityControl_OpacityChanged(object sender, EventArgs e)
        {
            if (cPDFAnnotation == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());
            }
            else
            {
                double transparent = CPDFOpacityControl.OpacityValue / 100.0;
                if(transparent<=1)
                {
                    transparent = transparent * 255;
                }
                if (inkParam.Transparency != (byte)transparent && pdfViewerControl != null)
                {
                    InkAnnotHistory history = new InkAnnotHistory();
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                
                    cPDFAnnotation.SetTransparency((byte)transparent);
                    cPDFAnnotation.UpdateAp();
                    pdfViewerControl.UpdateAnnotFrame();

                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }

            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());
        }

        private void CPDFThicknessControl_ThicknessChanged(object sender, EventArgs e)
        {
            if (cPDFAnnotation == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());
            }
            else
            {
                if (Math.Abs(inkParam.Thickness - CPDFThicknessControl.Thickness) > 0.01 && pdfViewerControl != null)
                {
                    InkAnnotHistory history = new InkAnnotHistory();
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);

                    cPDFAnnotation.SetThickness(CPDFThicknessControl.Thickness);
                    cPDFAnnotation.UpdateAp();
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }

            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());
        }

        private void ColorPickerControl_ColorChanged(object sender, EventArgs e)
        {
            if (cPDFAnnotation == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());

            }
            else
            {
                byte[] color = new byte[3];
                color[0] = ((SolidColorBrush)ColorPickerControl.Brush).Color.R;
                color[1] = ((SolidColorBrush)ColorPickerControl.Brush).Color.G;
                color[2] = ((SolidColorBrush)ColorPickerControl.Brush).Color.B;
                
                if (!cPDFAnnotation.InkColor.SequenceEqual(color) && pdfViewerControl != null)
                {
                    InkAnnotHistory history = new InkAnnotHistory();
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetInkColor(color);
                    cPDFAnnotation.UpdateAp();
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }

            CPDFAnnotationPreviewerControl.DrawFreehandPreview(GetFreehandData());
        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cPDFAnnotation == null)
            {
                PropertyChanged?.Invoke(this, GetFreehandData());
            }
            else
            {
                if (inkParam.Content != NoteTextBox.Text && pdfViewerControl != null)
                {
                    InkAnnotHistory history = new InkAnnotHistory();
                    history.PDFDoc = pdfViewerControl.GetCPDFViewer().GetDocument();
                    history.Action = HistoryAction.Update;
                    history.PreviousParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    
                    cPDFAnnotation.SetContent(NoteTextBox.Text);
                    pdfViewerControl.UpdateAnnotFrame();
                    
                    history.CurrentParam = ParamConverter.CPDFDataConverterToAnnotParam(history.PDFDoc, cPDFAnnotation.Page.PageIndex, cPDFAnnotation);
                    pdfViewerControl.GetCPDFViewer().UndoManager.AddHistory(history);
                }
            }
        }

        public void SetPresentAnnotAttrib(AnnotParam annotParam, CPDFAnnotation annotation, PDFViewControl cPDFViewer)
        {
            inkParam = (InkParam)annotParam;
            cPDFAnnotation = (CPDFInkAnnotation)annotation;
            pdfViewerControl = cPDFViewer;
            if (inkParam==null)
            {
                return;
            }

            ColorPickerControl.Brush = new SolidColorBrush(ParamConverter.ConverterByteForColor(inkParam.InkColor));
            CPDFOpacityControl.OpacityValue = (int)(inkParam.Transparency / 255D * 100);
            CPDFThicknessControl.Thickness = Convert.ToInt16(inkParam.Thickness);
            ColorPickerControl.SetCheckedForColor(ParamConverter.ConverterByteForColor(inkParam.InkColor));
            NoteTextBox.Text = inkParam.Content;
        }

        public CPDFFreehandData GetFreehandData()
        {
            CPDFFreehandData pdfFreehandData = new CPDFFreehandData();
            pdfFreehandData.AnnotationType = CPDFAnnotationType.Freehand;
            pdfFreehandData.BorderColor = ((SolidColorBrush)ColorPickerControl.Brush).Color;
            pdfFreehandData.Opacity = CPDFOpacityControl.OpacityValue / 100.0;
            pdfFreehandData.Thickness = CPDFThicknessControl.Thickness;
            pdfFreehandData.Note = NoteTextBox.Text;
            return pdfFreehandData;
        }

        public void SetEraseCheck(bool isCheck)
        {
            if(isCheck)
            {
                FreehandBtn.IsChecked = false;
                EraseBtn.IsChecked = true;
                FreehandPanel.Visibility = Visibility.Collapsed;
                ErasePanel.Visibility = Visibility.Visible;
                CPDFAnnotationPreviewerControl.Visibility = Visibility.Collapsed;
                EraseCirclePanel.Visibility = Visibility.Visible;
            }
            else
            {
                FreehandBtn.IsChecked = true;
                EraseBtn.IsChecked = false;
                FreehandPanel.Visibility = Visibility.Visible;
                ErasePanel.Visibility = Visibility.Collapsed;
                CPDFAnnotationPreviewerControl.Visibility = Visibility.Visible;
                EraseCirclePanel.Visibility=Visibility.Collapsed;
            }
        }

        internal void ClearAnnotAttribEvent()
        {
            cPDFAnnotation = null;
        }

        internal int GetEraseThickness()
        {
            return EraseThickness.Thickness;
        }

        private void FreehandBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEraseCheck(false);
            EraseClickHandler?.Invoke(this, false);
        }

        private void EraseBtn_Click(object sender, RoutedEventArgs e)
        {
            SetEraseCheck(true);
            EraseClickHandler?.Invoke(this, true);
        }
    }
}
