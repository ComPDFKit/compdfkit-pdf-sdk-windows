using ComPDFKit.PDFPage;
using ComPDFKit.Tool;
using ComPDFKit.Controls.PDFControlUI;
using ComPDFKitViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFSearchControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private CPDFViewer pdfView;
        private int currentHighLightIndex { get; set; } = -1;

        private C_Search_Options searchOption = C_Search_Options.Search_Case_Insensitive;

        private PDFTextSearch textSearch;

        private string keyWord;

        private bool isClearResult = false;

        private SolidColorBrush highLightBrush = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xF7, 0x00));
         
        private int ResultCount = 0;

        private PDFViewControl ViewControl;
        private bool _isCaseSensitive = true;
        public bool IsCaseSensitive
        {
            set
            {
                _isCaseSensitive = value;
                if (_isCaseSensitive)
                {
                    searchOption &= ~C_Search_Options.Search_Case_Sensitive;
                }
                else
                {
                    searchOption |= C_Search_Options.Search_Case_Sensitive;
                }
                ViewControl.PDFViewTool?.StartFindText(keyWord, searchOption);
            }
            get
            {
                return _isCaseSensitive;
            }
        }

        public bool IsMatchWholeWord
        {
            set
            {
                if (value)
                {
                    searchOption |= C_Search_Options.Search_Match_Whole_Word;
                }
                else
                {
                    searchOption &= ~C_Search_Options.Search_Match_Whole_Word;
                }
                ViewControl?.PDFViewTool.StartFindText(keyWord, searchOption);
            }
        }

        public CPDFSearchControl()
        {
            InitializeComponent();
            Loaded += PDFSearch_Loaded;
            DataContext = this;
        }

        public void InitWithPDFViewer(PDFViewControl newViewControl)
        {
            ViewControl = newViewControl;
            if (ViewControl != null)
            {
                CPDFViewerTool viewTool = ViewControl.PDFViewTool;
                if (viewTool != null)
                {
                    CPDFViewer newPDFView = viewTool.GetCPDFViewer();
                    if (pdfView != newPDFView)
                    {
                        ClearSearchResult();
                        pdfView = newPDFView;
                    }
                }
            }
            else
            {
                ClearSearchResult();
            }

        }

        private void PDFSearch_Loaded(object sender, RoutedEventArgs e)
        {
            textSearch = new PDFTextSearch();

            SearchInput.SearchEvent -= SearchInput_SearchEvent;
            SearchInput.ClearEvent -= SearchInput_ClearEvent;
            textSearch.SearchCompletedHandler -= TextSearch_SearchCompletedHandler;
            SearchResult.SelectionChanged -= SearchResult_SelectionChanged;
            textSearch.SearchPercentHandler -= TextSearch_SearchPercentHandler;
            textSearch.SearchCancelHandler -= TextSearch_SearchCancelHandler;

            SearchInput.SearchEvent += SearchInput_SearchEvent;
            SearchInput.ClearEvent += SearchInput_ClearEvent;
            textSearch.SearchCompletedHandler += TextSearch_SearchCompletedHandler;
            SearchResult.SelectionChanged += SearchResult_SelectionChanged;
            textSearch.SearchPercentHandler += TextSearch_SearchPercentHandler;
            textSearch.SearchCancelHandler += TextSearch_SearchCancelHandler;

            SearchInput.MoveResultEvent -= SearchInput_MoveResultEvent;
            SearchInput.MoveResultEvent += SearchInput_MoveResultEvent;

            SearchTog.IsChecked = true;
        }

        private void SearchInput_MoveResultEvent(object sender, CPDFSearchInputUI.MoveDirection e)
        {
            if (keyWord != SearchInput.SearchKeyWord)
            {
                keyWord = SearchInput.SearchKeyWord;
                ViewControl?.PDFViewTool.StartFindText(keyWord, searchOption);
                SearchResult.ClearSearchResult();
            }
            if (e == CPDFSearchInputUI.MoveDirection.Previous)
            {
                ViewControl?.PDFViewTool.FindPrevious();
            }
            else
            {
                ViewControl?.PDFViewTool.FindNext();
            }
        }

        private void TextSearch_SearchPercentHandler(object sender, TextSearchResult e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!e.Items.ContainsKey(e.CurrentPage))
                {
                    return;
                }
                ProgressBar.ProgressValue = e.CurrentPage + 1;
                foreach (var item in e.Items[e.CurrentPage])
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var result = new BindSearchResult()
                    {
                        PageIndex = item.PageIndex,
                        TextContent = item.TextContent,
                        TextRect = item.TextRect,
                        SearchWord = keyWord,
                        HighLightColor = Color.FromArgb(0x99, 0xFF, 0xF7, 0x00),
                        PageRotate = item.PageRotate
                    };
                    SearchResult.AddSearchResult(result);
                    ResultNum.Text = SearchResult.ResultListControl.Items.Count.ToString();
                }

            });
        }

        private void SearchInput_ClearEvent(object sender, EventArgs e)
        {
            if (textSearch.CanDoSearch)
            {
                Dispatcher.Invoke(ClearSearchResult);
            }
            else
            {
                isClearResult = true;
                textSearch.CancleSearch();
            }
            ProgressBorder.Visibility = Visibility.Hidden;
        }

        private void TextSearch_SearchCancelHandler(object sender, TextSearchResult e)
        {
            if (isClearResult)
            {
                Dispatcher.Invoke(ClearSearchResult);
                isClearResult = false;
            }
        }

        private void SearchInput_FindPreviousEvent(object sender, EventArgs e)
        {
            if (currentHighLightIndex > 0)
            {
                currentHighLightIndex--;
                BindSearchResult result = SearchResult.GetItem(currentHighLightIndex);
                SearchResult.ClearSearchResult();
            }
        }

        private void SearchInput_FindNextEvent(object sender, EventArgs e)
        {
            currentHighLightIndex++;
            if (currentHighLightIndex >= 0)
            {
                BindSearchResult result = SearchResult.GetItem(currentHighLightIndex);
                SearchResult.ClearSearchResult();
            }
        }

        private void SearchResult_SelectionChanged(object sender, int e)
        {
            if (e < 0)
            {
                return;
            }
            currentHighLightIndex = e;
            BindSearchResult result = SearchResult.GetSelectItem();
            HighLightSelectResult(result);
            ResultNum.Text = "";
            ResultText.Text = LanguageHelper.BotaManager.GetString("Text_Result") + (e + 1) + "/" + SearchResult.ResultListControl.Items.Count;
        }

        private void TextSearch_SearchCompletedHandler(object sender, TextSearchResult e)
        {
            Dispatcher.Invoke(() =>
            {
                List<BindSearchResult> resultList = new List<BindSearchResult>();
                List<TextSearchItem> totalItems = new List<TextSearchItem>();
                foreach (int pageIndex in e.Items.Keys)
                {
                    List<TextSearchItem> textSearchItems = e.Items[pageIndex];
                    foreach (TextSearchItem item in textSearchItems)
                    {
                        resultList.Add(new BindSearchResult()
                        {
                            PageIndex = item.PageIndex,
                            TextContent = item.TextContent,
                            TextRect = item.TextRect,
                            SearchWord = keyWord,
                            HighLightColor = Color.FromArgb(0x99, 0xFF, 0xF7, 0x00),
                            PageRotate = item.PageRotate,
                            RefData=item
                        });
                        totalItems.Add(item);
                    }
                }
                SearchResult.SetSearchResult(resultList);
                ResultCount=resultList.Count;
                ViewControl.PDFViewTool?.SetPageSelectText(totalItems);
            });
        }

        private void SearchInput_SearchEvent(object sender, string e)
        {
            if (string.IsNullOrEmpty(e))
            {
                return;
            }

            if (pdfView == null || pdfView.GetDocument() == null)
            {
                return;
            }

            if (textSearch == null || !textSearch.CanDoSearch)
            {
                return;
            }

            if (SearchTog.IsChecked == true)
            {
                if (ViewControl.PDFViewTool.IsDocumentModified)
                {
                    if (pdfView.GetDocument().FilePath != string.Empty)
                    {
                        pdfView.GetDocument().WriteToLoadedPath();
                    }
                }
                keyWord = e;
                textSearch.TextSearchDocument = pdfView.GetDocument();
                SearchResult.ClearSearchResult();

                ResultNum.Text = "0";
                ResultText.Text = LanguageHelper.BotaManager.GetString("Tip_Result");
                ProgressBar.ProgressMaxValue = pdfView.GetDocument().PageCount;
                ProgressBorder.Visibility = Visibility.Visible;
                
                keyWord = e;
                textSearch.TextSearchDocument = pdfView.GetDocument();
                textSearch.SearchText(e, C_Search_Options.Search_Case_Insensitive, ViewControl.Password);
            }
            else if (ReplaceTog.IsChecked == true)
            {
                SearchInput_MoveResultEvent(null, CPDFSearchInputUI.MoveDirection.Next);
            }
        }

        private void HighLightSelectResult(BindSearchResult result)
        {
            if (result == null || result.RefData==null)
            {
                return;
            }

            List<TextSearchItem> selectList = new List<TextSearchItem>();
            TextSearchItem highlightItem=result.RefData as TextSearchItem;
            if(highlightItem != null)
            {
                highlightItem.CreatePaintBrush(highLightBrush.Color);
                selectList.Add(highlightItem);
                ViewControl.PDFToolManager?.HighLightSearchText(selectList);
            }
        }

        private void ClearSearchResult()
        {
            SearchResult?.ClearSearchResult();
            ResultNum.Text = string.Empty;
            ResultText.Text = string.Empty;
            SearchInput.SearchKeyWord = string.Empty;
        }

        private void SearchCancel_Click(object sender, RoutedEventArgs e)
        {
            textSearch.CancleSearch();
        }

        private void SearchTog_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewControl.PDFViewTool.IsDocumentModified)
            {
                if (pdfView.GetDocument().FilePath != string.Empty)
                {
                    pdfView.GetDocument().WriteToLoadedPath();
                    ViewControl.PDFViewTool.IsDocumentModified = false;
                } 
            } 
            ReplaceTog.IsChecked = false;
            SearchInput.InputGrid.RowDefinitions[1].Height = new GridLength(0);
            SearchInput.Height = 40;
            ReplaceBorder.Visibility = Visibility.Collapsed;
            SearchResult.Visibility = Visibility.Visible;
            ResultBorder.Visibility = Visibility.Visible; 
        }

        private void ReplaceTog_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewControl.PDFViewTool.IsDocumentModified)
            {
                pdfView.GetDocument().WriteToLoadedPath();
                ViewControl.PDFViewTool.IsDocumentModified = false;
            }
            SearchTog.IsChecked = false;
            SearchInput.InputGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            SearchInput.Height = 80;
            ReplaceBorder.Visibility = Visibility.Visible;
            SearchResult.Visibility = Visibility.Collapsed;
            ResultBorder.Visibility = Visibility.Collapsed;
            keyWord = string.Empty;
        }

        private void ReplaceCurrent_Click(object sender, RoutedEventArgs e)
        {
            ViewControl?.PDFViewTool.ReplaceText(SearchInput.ReplaceWord);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            keyWord = SearchInput.SearchKeyWord;
            ViewControl?.PDFViewTool.StartFindText(keyWord, searchOption);
            ViewControl?.PDFViewTool.ReplaceAllText(SearchInput.ReplaceWord); 
        }
    }
}
