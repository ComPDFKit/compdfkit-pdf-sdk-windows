using Compdfkit_Tools.Measure.Property;
using Compdfkit_Tools.PDFControl;
using ComPDFKitViewer.AnnotEvent;
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

namespace Compdfkit_Tools.Measure
{
    /// <summary>
    /// MeasurePropertyControl.xaml 的交互逻辑
    /// </summary>
    public partial class MeasurePropertyControl : UserControl
    {
        private UIElement currentPanel = null;
        public MeasurePropertyControl()
        {
            InitializeComponent();
        }

        public void SetPropertyForMeasureCreate(AnnotHandlerEventArgs Args, AnnotAttribEvent attribEvent)
        {
            if (Args == null)
            {
                ClearMeasurePanel();
                return;
            }

            switch (Args.EventType)
            {
                case AnnotArgsType.LineMeasure:
                    StraightnessProperty straightnessProperty = new StraightnessProperty();
                    if (attribEvent != null)
                    {
                        straightnessProperty.SetAnnotEventData(attribEvent);
                    }
                    else
                    {
                        straightnessProperty.SetAnnotArgsData((LineMeasureArgs)Args);
                    }
                    currentPanel = straightnessProperty;
                    break;
                case AnnotArgsType.PolyLineMeasure:
                    MultilineProperty multilineProperty = new MultilineProperty();
                    if (attribEvent != null)
                    {
                        multilineProperty.SetAnnotEventData(attribEvent);
                    }
                    else
                    {
                        multilineProperty.SetAnnotArgsData((PolyLineMeasureArgs)Args);
                    }
                    currentPanel = multilineProperty;
                    break;
                case AnnotArgsType.PolygonMeasure:
                    PolygonalProperty polygonalProperty = new PolygonalProperty();
                    if (attribEvent != null)
                    {
                        polygonalProperty.SetAnnotEventData(attribEvent);
                    }
                    else
                    {
                        polygonalProperty.SetAnnotArgsData((PolygonMeasureArgs)Args);
                    }
                    currentPanel = polygonalProperty;
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

        public void SetPorpertyForMeasureModify(AnnotAttribEvent annotEvent)
        {

        }
    }
}
