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

namespace ComPDFKit.Controls.Measure
{
    public partial class MeasurePropertyControl : UserControl
    {
        private UIElement currentPanel = null;
        public MeasurePropertyControl()
        {
            InitializeComponent();
        }

        public void SetPropertyForMeasureCreate(AnnotParam param, CPDFAnnotation annot, PDFViewControl viewControl)
        {
            if (param == null)
            {
                ClearMeasurePanel();
                return;
            }

            switch (param.CurrentType)
            {
                case C_ANNOTATION_TYPE.C_ANNOTATION_LINE:
                    StraightnessProperty straightnessProperty = new StraightnessProperty();
                    if(param is LineMeasureParam lineMeasureParam)
                    {
                        straightnessProperty.SetAnnotParam(lineMeasureParam, annot, viewControl);
                    }
                    currentPanel = straightnessProperty;
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYLINE:
                    MultilineProperty multilineProperty = new MultilineProperty();
                    if (param is PolyLineMeasureParam polyLineMeasureParam)
                    {
                        multilineProperty.SetAnnotParam(polyLineMeasureParam, annot, viewControl);
                    }
                    currentPanel = multilineProperty;
                    break;
                case C_ANNOTATION_TYPE.C_ANNOTATION_POLYGON:
                    PolygonalProperty polygonProperty = new PolygonalProperty();
                    if (param is PolygonMeasureParam polygonMeasureParam)
                    {
                        polygonProperty.SetAnnotParam(polygonMeasureParam, annot, viewControl);
                    }
                    currentPanel = polygonProperty;
                    break;
                default:
                    break;
            }
            SetMeasurePanel(currentPanel);
        }

        private void SetMeasurePanel(UIElement newChild)
        {
            MeasurePropertyPanel.Child = newChild;
        }

        public void ClearMeasurePanel()
        {
            currentPanel = null;
            MeasurePropertyPanel.Child = null;
        }

        //public void SetPorpertyForMeasureModify(AnnotAttribEvent annotEvent)
        //{

        //}
    }
}
