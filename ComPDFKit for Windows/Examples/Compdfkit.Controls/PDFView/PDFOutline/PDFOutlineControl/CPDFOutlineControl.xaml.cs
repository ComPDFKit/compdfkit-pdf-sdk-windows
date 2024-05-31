using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using ComPDFKit.Tool;
using ComPDFKit.Viewer.Helper;
using ComPDFKit.Controls.PDFControlUI;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFOutlineControl : UserControl
    {
        PDFViewControl ViewControl;
        public ObservableCollection<CPDFOutlineNode> OutlineList
        {
            set; get;
        }
        public CPDFOutlineControl()
        {
            InitializeComponent();
            Loaded += CPDFOutlineControl_Loaded;
        }

        private void CPDFOutlineControl_Loaded(object sender, RoutedEventArgs e)
        {
            CPDFOutlineUI.OutlineSelectionChanged -= CPDFOutlineUI_OutlineSelectionChanged;
            CPDFOutlineUI.OutlineSelectionChanged += CPDFOutlineUI_OutlineSelectionChanged;
        }

        private void CPDFOutlineUI_OutlineSelectionChanged(object sender, object e)
        {
            try
            {
                TreeViewItem new_item = (TreeViewItem)e;
                CPDFOutline outline = (CPDFOutline)new_item.Tag;

                CPDFAction action = outline.GetAction();
                if (action != null && action.ActionType != C_ACTION_TYPE.ACTION_TYPE_UNKNOWN)
                {
                    ViewControl.PDFViewTool.ActionProcess(action);
                }
                else
                {
                    CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
                    CPDFDocument pdfDoc = pdfViewer.GetDocument();
                    CPDFDestination dest = outline.GetDestination(pdfDoc);
                    Size size = DataConversionForWPF.CSizeConversionForSize( pdfDoc.GetPageSize(Convert.ToInt32(dest.PageIndex) - 1));
                    if (dest.Position_X == -1)
                    {
                        dest.Position_X = 0;
                    }
                    if (dest.Position_Y == -1)
                    {
                        dest.Position_Y = (float)size.Height;
                    }
                    if (dest != null)
                    {
                        pdfViewer.GoToPage(dest.PageIndex, new Point(dest.Position_X, size.Height - dest.Position_Y));
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private int GetIndexFromParent(List<CPDFOutline> parentlist, CPDFOutline outline)
        {
            for (int i = 0; i < parentlist.Count; i++)
            {
                if (parentlist[i] == outline)
                {
                    return i;
                }
            }
            return -1;
        }

        public CPDFOutlineNode FindOutlineFromDto(ObservableCollection<CPDFOutlineNode> list, CPDFOutline outline)
        {
            foreach (CPDFOutlineNode item in list)
            {
                CPDFOutline parent = outline.GetParent();
                CPDFOutline PARENT = item.PDFOutline.GetParent();
                item.CurrentNodeName = outline.Title;
                int i = GetIndexFromParent(parent.ChildList, outline);
                int I = GetIndexFromParent(PARENT.ChildList, item.PDFOutline);

                if (item.PDFOutline.Title == outline.Title && i == I && outline.Level == item.PDFOutline.Level && ((parent.Title == PARENT.Title) || (parent.Title == null && PARENT.Title == null)))
                {
                    return item;
                }
                else if (item.ChildrenNodeList.Count > 0)
                {
                    CPDFOutlineNode retdto = FindOutlineFromDto(item.ChildrenNodeList, outline);
                    if (retdto != null)
                    {
                        return retdto;
                    }
                }

            }
            return null;
        }

        private CPDFOutlineNode ConvertCPDFToOutlineNode(CPDFOutline outline, CPDFOutlineNode parent)
        {
            CPDFOutlineNode node = new CPDFOutlineNode();
            if (outline != null)
            {
                node.PDFOutline = outline;
                if (parent != null)
                {
                    node.ParentNode = parent;
                }
                if (OutlineList != null && OutlineList.Count > 0)
                {
                    CPDFOutlineNode oldnode = FindOutlineFromDto(OutlineList, node.PDFOutline);
                }
                node.ChildrenNodeList = new ObservableCollection<CPDFOutlineNode>();
                if (outline.ChildList.Count > 0)
                {
                    foreach (CPDFOutline item in outline.ChildList)
                    {
                        node.ChildrenNodeList.Add(ConvertCPDFToOutlineNode(item, node));
                    }
                }
            }
            return node;
        }

        public ObservableCollection<CPDFOutlineNode> GetOutlineViewDataSource()
        {
            ObservableCollection<CPDFOutlineNode> data = new ObservableCollection<CPDFOutlineNode>();
            if (ViewControl == null && ViewControl.PDFViewTool == null)
            {
                return data;
            }

            CPDFViewer pdfViewer = ViewControl.PDFViewTool.GetCPDFViewer();
            CPDFDocument pdfDoc = pdfViewer?.GetDocument();
            if(pdfDoc!=null)
            {
                List<CPDFOutline> datasource = pdfDoc.GetOutlineList();
                foreach (CPDFOutline item in datasource)
                {
                    CPDFOutlineNode dto = ConvertCPDFToOutlineNode(item, null);
                    if (dto != null)
                    {
                        data.Add(dto);
                    }
                }
            }
           
            return data;
        }

        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            this.ViewControl = viewControl;
            if(ViewControl != null && viewControl.PDFViewTool!=null)
            {
                CPDFViewer pdfViewer = viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc=pdfViewer?.GetDocument();
                if(pdfDoc!=null)
                {
                    List<CPDFOutline> outlineList = pdfDoc.GetOutlineList();
                    CPDFOutlineUI.SetOutlineTree(outlineList);
                }
            }
        }
    }
}
