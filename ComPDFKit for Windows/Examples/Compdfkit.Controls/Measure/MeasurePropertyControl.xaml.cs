using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using ComPDFKit.Controls.Measure.Property;
using ComPDFKit.Controls.PDFControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ComPDFKitViewer;

namespace ComPDFKit.Controls.Measure
{
    public partial class MeasurePropertyControl : UserControl
    {
        StraightnessProperty straightnessProperty = new StraightnessProperty();
        MultilineProperty multilineProperty = new MultilineProperty();
        PolygonalProperty polygonProperty = new PolygonalProperty();
        AnnotParam currentParam = null;

        public event EventHandler<LineMeasureParam> LineMeasureParamChanged;
        public event EventHandler<PolyLineMeasureParam> PolyLineMeasureParamChanged;
        public event EventHandler<PolygonMeasureParam> PolygonMeasureParamChanged;

        public C_ANNOTATION_TYPE CurrentCreateType
        {
            get;
            set;
        } = C_ANNOTATION_TYPE.C_ANNOTATION_NONE;

        private UIElement currentPanel = null;
        private PDFViewControl pdfViewerControl;

        public MeasurePropertyControl()
        {
            InitializeComponent();
            straightnessProperty.LineMeasureParamChanged -= StraightnessProperty_LineMeasureParamChanged;
            multilineProperty.PolyLineMeasureParamChanged -= MultilineProperty_PolyLineMeasureParamChanged;
            polygonProperty.PolygonMeasureParamChanged -= PolygonProperty_PolygonMeasureParamChanged;

            multilineProperty.PolyLineMeasureParamChanged += MultilineProperty_PolyLineMeasureParamChanged;
            straightnessProperty.LineMeasureParamChanged += StraightnessProperty_LineMeasureParamChanged;
            polygonProperty.PolygonMeasureParamChanged += PolygonProperty_PolygonMeasureParamChanged; 
        }

        private void PolygonProperty_PolygonMeasureParamChanged(object sender, PolygonMeasureParam e)
        {
            PolygonMeasureParamChanged?.Invoke(this, e);
        }

        private void MultilineProperty_PolyLineMeasureParamChanged(object sender, PolyLineMeasureParam e)
        {
             PolyLineMeasureParamChanged?.Invoke(this, e);
        }

        public void SetPropertyForMeasureCreate(AnnotParam param, CPDFAnnotation annot, PDFViewControl viewControl)
        {
            if (param == null)
            {
                ClearMeasurePanel();
                return;
            }
            
            if(annot == null)
            {
                CurrentCreateType = param.CurrentType;
                currentParam = param;
            }

            switch (param.CurrentType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    {
                        if (param is LineMeasureParam lineMeasureParam)
                        {
                            straightnessProperty.SetAnnotParam(lineMeasureParam, annot, viewControl);
                            SetMeasurePanel(straightnessProperty);
                            return;
                        }
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    if (param is PolyLineMeasureParam polyLineMeasureParam)
                    {
                        multilineProperty.SetAnnotParam(polyLineMeasureParam, annot, viewControl);
                        SetMeasurePanel(multilineProperty);
                        return;
                    }
                    break;

                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    if (param is PolygonMeasureParam polygonMeasureParam && (annot ==null || (annot as CPDFPolygonAnnotation).IsMeasured()))
                    {
                        polygonProperty.SetAnnotParam(polygonMeasureParam, annot, viewControl);
                        SetMeasurePanel(polygonProperty);
                        return;
                    }
                    break;

                default:
                    break;
            }

            ClearMeasurePanel();
        }

        private void StraightnessProperty_LineMeasureParamChanged(object sender, LineMeasureParam e)
        {
            LineMeasureParamChanged?.Invoke(this, e);
        }

        private void SetMeasurePanel(UIElement newChild)
        {
            currentPanel = newChild;
            MeasurePropertyPanel.Child = newChild;
        }

        public void ClearMeasurePanel()
        {
            currentPanel = null;
            MeasurePropertyPanel.Child = null;
        }

        internal void InitWithPDFViewer(PDFViewControl pdfViewControl)
        {
            if (this.pdfViewerControl != null)
            {
                UnLoadPDFViewHandler();
            }
            this.pdfViewerControl = pdfViewControl;
            LoadPDFViewHandler();
        }

        private void LoadPDFViewHandler()
        {
            if (pdfViewerControl != null)
            {
                pdfViewerControl.MouseLeftButtonDown -= PdfViewerControl_MouseLeftButtonDown;
                pdfViewerControl.MouseLeftButtonDown += PdfViewerControl_MouseLeftButtonDown; 
            }
        }

        private void PdfViewerControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetAnnotEventData();
        }

        private void SetAnnotEventData()
        {
            if (pdfViewerControl.GetCacheHitTestAnnot() == null)
            {
                if (pdfViewerControl != null && (pdfViewerControl.PDFToolManager.GetToolType() == ToolType.CreateAnnot))
                {
                    switch (CurrentCreateType)
                    {
                        case  C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                            straightnessProperty.Annotation = null;
                            straightnessProperty.SetAnnotParam(currentParam as LineMeasureParam, null, pdfViewerControl);
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                            multilineProperty.Annotation = null;
                            multilineProperty.SetAnnotParam(currentParam as PolyLineMeasureParam, null, pdfViewerControl);
                            break;
                        case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                            polygonProperty.Annotation = null;
                            polygonProperty.SetAnnotParam(currentParam as PolygonMeasureParam, null, pdfViewerControl);
                            break;
                    }
                    ShowCreateAnnotPanel();
                }
                else
                {
                    ClearMeasurePanel();
                }
            }
        }

        private void ShowCreateAnnotPanel()
        {
            switch (CurrentCreateType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_NONE:
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    currentPanel = straightnessProperty;
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    currentPanel = polygonProperty;
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    currentPanel = multilineProperty;
                    break;
            }
            SetMeasurePanel(currentPanel); 
        }

        private void UnLoadPDFViewHandler()
        {

        }

        //public void SetPorpertyForMeasureModify(AnnotAttribEvent annotEvent)
        //{

        //}
    }
}
