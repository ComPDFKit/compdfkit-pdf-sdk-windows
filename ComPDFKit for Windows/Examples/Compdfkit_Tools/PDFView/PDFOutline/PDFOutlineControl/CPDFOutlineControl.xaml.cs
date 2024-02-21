using ComPDFKit.PDFDocument;
using ComPDFKit.PDFDocument.Action;
using Compdfkit_Tools.PDFControlUI;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFOutlineControl : UserControl
    {
        CPDFViewer pdfViewer;
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
                    pdfViewer.ProcessAction(action);
                }
                else
                {
                    CPDFDestination dest = outline.GetDestination(pdfViewer.Document);
                    if (dest != null)
                    {
                        pdfViewer.GoToPage(dest.PageIndex);
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
            List<CPDFOutline> datasource = pdfViewer.Document.GetOutlineList();
            foreach (CPDFOutline item in datasource)
            {

                CPDFOutlineNode dto = ConvertCPDFToOutlineNode(item, null);
                if (dto != null)
                {
                    data.Add(dto);
                }
            }
            return data;
        }

        public void InitWithPDFViewer(CPDFViewer pdfViewer)
        {
            this.pdfViewer = pdfViewer;
            List<CPDFOutline> outlineList = pdfViewer.Document.GetOutlineList();
            CPDFOutlineUI.SetOutlineTree(outlineList);
        }
    }
}
