using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;


namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFSearchResultUI : UserControl
    {
        public event EventHandler<int> SelectionChanged;

        private ObservableCollection<TextBindData> searchResults;

        public CPDFSearchResultUI()
        {
            InitializeComponent();
            searchResults = new ObservableCollection<TextBindData>();
            ICollectionView groupView = CollectionViewSource.GetDefaultView(searchResults);
            groupView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TextBindData.ShowPageIndex)));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InvokeSelectionChangedEvent(ResultListControl.SelectedIndex);
        }
        public void SetSearchResult(List<BindSearchResult> results)
        {
            searchResults.Clear();
            if (results == null || results.Count == 0)
            {
                ResultListControl.ItemsSource = null;
                NoResultText.Visibility = Visibility.Visible;
                return;
            }

            foreach (BindSearchResult item in results)
            {
                searchResults.Add(new TextBindData()
                {
                    BindProperty = item
                });
            }
            ResultListControl.ItemsSource = searchResults;
            ResultListControl.Visibility = Visibility.Visible;
            NoResultText.Visibility = Visibility.Collapsed;
        }

        public void ClearSearchResult()
        {
            searchResults.Clear();
            ResultListControl.ItemsSource = null;
            ResultListControl.Visibility = Visibility.Collapsed;
            NoResultText.Visibility = Visibility.Visible;
        }
        
        public void AddSearchResult(BindSearchResult result)
        {
            if (result == null)
            {
                return;
            }
            searchResults.Add(new TextBindData()
            {
                BindProperty = result
            });
            ResultListControl.ItemsSource = searchResults;
            ResultListControl.Visibility = Visibility.Visible;
            NoResultText.Visibility = Visibility.Collapsed;
        }

        public BindSearchResult GetSelectItem()
        {
            TextBindData bindData = ResultListControl.SelectedItem as TextBindData;
            if (bindData != null)
            {
                return bindData.BindProperty;
            }

            return null;
        }

        public BindSearchResult GetItem(int index)
        {
            if (index < 0)
            {
                return null;
            }
            if (ResultListControl.HasItems && ResultListControl.Items.Count > index)
            {
                TextBindData bindData = ResultListControl.Items[index] as TextBindData;
                if (bindData != null)
                {
                    return bindData.BindProperty;
                }
            }

            return null;
        }

        /// <summary>
        /// Clear selected results.
        /// </summary>
        public void ClearSelection()
        {
            int oldSelectionIndex = ResultListControl.SelectedIndex;
            ResultListControl.UnselectAll();
            if (oldSelectionIndex != ResultListControl.SelectedIndex)
            {
                InvokeSelectionChangedEvent(ResultListControl.SelectedIndex);
            }
        }

        /// <summary>
        /// Set selected results.
        /// </summary>
        /// <param name="selectIndex">The selected index.</param>
        public void SelectItem(int selectIndex)
        {
            if (ResultListControl.SelectedIndex != selectIndex)
            {
                ResultListControl.SelectedIndex = selectIndex;
            }
        }

        internal void InvokeSelectionChangedEvent(int selectionIndex)
        {
            SelectionChanged?.Invoke(this, selectionIndex);
        }
    }

    public class BindSearchResult
    {
        /// <summary>
        /// Page index.
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// The search text results.
        /// </summary>
        public string TextContent { get; set; }
        /// <summary>
        /// Search highlight color.
        /// </summary>
        public Color HighLightColor { get; set; }
        /// <summary>
        /// Search keywords.
        /// </summary>
        public string SearchWord { get; set; }
        /// <summary>
        /// Search result range area.
        /// </summary>
        public Rect TextRect { get; set; }
        /// <summary>
        /// The page rotation angle.
        /// </summary>
        public int PageRotate { get; set; }
        public object RefData {  get; set; }
    }

    internal class TextBindData
    {
        public int ShowPageIndex { get { return BindProperty.PageIndex + 1; } set { BindProperty.PageIndex = value; } }
        public BindSearchResult BindProperty { get; set; }
        public TextBindData()
        {
            BindProperty = new BindSearchResult();
        }
    }

    internal class SearchResultBindHelper : DependencyObject
    {
        public static FlowDocument GetDocumentBind(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(DocumentBindProperty);
        }
        public static void SetDocumentBind(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(DocumentBindProperty, value);
        }

        public static readonly DependencyProperty DocumentBindProperty =
            DependencyProperty.RegisterAttached("DocumentBind",
                typeof(BindSearchResult),
                typeof(SearchResultBindHelper),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = false,
                    PropertyChangedCallback = (obj, e) =>
                    {
                        RichTextBox richTextBox = obj as RichTextBox;
                        BindSearchResult bindItem = e.NewValue as BindSearchResult;
                        if (richTextBox != null && bindItem != null)
                        {
                            richTextBox.Document = GetFlowDocument(bindItem.TextContent.Trim(), bindItem.SearchWord, bindItem.HighLightColor);
                        }
                    }
                });

        /// <summary>
        /// Get document stream data
        /// </summary>
        /// <param name="content">Search text results</param>
        /// <param name="keyword">Search for keywords</param>
        /// <param name="textColor">Highlight text color</param>
        /// <returns>Document flow data</returns>
        public static FlowDocument GetFlowDocument(string content, string keyword, Color textColor)
        {
            FlowDocument Document = new FlowDocument();
            Paragraph textPara = new Paragraph();
            Document.Blocks.Add(textPara);
            List<int> indexList = new List<int>();
            content = Regex.Replace(content, "[\r\n]", " ");
            if (keyword.Length > 0)
            {
                for (int i = 0; i < content.Length && i >= 0;)
                {
                    i = content.IndexOf(keyword, i, StringComparison.OrdinalIgnoreCase);
                    if (i == -1)
                    {
                        break;
                    }
                    if (indexList.Contains(i) == false)
                    {
                        indexList.Add(i);
                    }
                    i += keyword.Length;
                }
            }
            List<string> splitList = new List<string>();
            int lastIndex = -1;
            foreach (int index in indexList)
            {
                string prevStr = string.Empty;
                if (lastIndex == -1)
                {
                    prevStr = content.Substring(0, index);
                }
                else
                {
                    prevStr = content.Substring(lastIndex + keyword.Length, index - lastIndex - 1);
                }
                if (prevStr != string.Empty)
                {
                    splitList.Add(prevStr);
                }

                splitList.Add(content.Substring(index, keyword.Length));
                lastIndex = index;
            }
            if (indexList.Count > 0)
            {
                lastIndex = indexList[indexList.Count - 1];
                if (content.Length > lastIndex + keyword.Length)
                {
                    splitList.Add(content.Substring(lastIndex + keyword.Length));
                }
            }
            TextBlock addBlock = new TextBlock();
            foreach (string textappend in splitList)
            {
                Run textRun = new Run(textappend);
                if (textappend.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    textRun.Background = new SolidColorBrush(textColor);
                }
                addBlock.Inlines.Add(textRun);
            }
            addBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            textPara.Inlines.Add(addBlock);

            return Document;
        }

    }
}
