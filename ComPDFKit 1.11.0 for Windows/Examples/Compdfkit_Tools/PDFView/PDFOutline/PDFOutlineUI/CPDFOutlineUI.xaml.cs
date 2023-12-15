using ComPDFKit.PDFDocument;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControlUI
{
    public partial class CPDFOutlineNode
    {
        public CPDFOutlineNode ParentNode = null;

        public CPDFOutline PDFOutline = null;

        public string CurrentNodeName = string.Empty;

        public ObservableCollection<CPDFOutlineNode> ChildrenNodeList
        {
            get;
            set;
        }

        public bool IsExpanded = false;

        public int PageIndex = 0;

        public double PositionX;

        public double PositionY;
    }

    public partial class CPDFOutlineUI : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CPDFOutlineNode> OutlineList { get; set; } = new ObservableCollection<CPDFOutlineNode>();

        public event EventHandler<object> OutlineSelectionChanged;

        public CPDFOutlineUI()
        {
            InitializeComponent();
        }

        private void BuildOutlineTree(List<CPDFOutline> outlineList, ItemCollection nodes)
        {
            foreach (var outline in outlineList)
            {
                TreeViewItem new_node = new TreeViewItem();
                new_node.Header = outline.Title;
                ToolTipService.SetToolTip(new_node, new_node.Header);
                nodes.Add(new_node);
                new_node.Tag = outline;

                List<CPDFOutline> childList = outline.ChildList;
                if (childList != null && childList.Count > 0)
                {
                    BuildOutlineTree(childList, new_node.Items);
                }
            }
        }

        public void SetOutlineTree(List<CPDFOutline> outlineList)
        {
            this.OutlineList.Clear();
            if (!OutlineTree.HasItems)
            {
                if (outlineList != null && outlineList.Count > 0)
                {
                    OutlineTree.BeginInit();
                    BuildOutlineTree(outlineList, OutlineTree.Items);
                    OutlineTree.EndInit();
                }
            }

            if(outlineList==null || outlineList.Count==0)
            {
                NoResultText.Visibility = Visibility.Visible;
            }
            else
            {
                NoResultText.Visibility= Visibility.Collapsed;
            }
        }

        private void OutlineTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            OutlineSelectionChanged?.Invoke(this, e.NewValue);
        }
    }
}
