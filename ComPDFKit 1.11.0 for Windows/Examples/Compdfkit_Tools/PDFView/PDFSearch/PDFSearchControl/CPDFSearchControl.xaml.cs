using ComPDFKit.PDFPage;
using Compdfkit_Tools.PDFControlUI;
using ComPDFKitViewer.PdfViewer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.PDFControl
{
    public partial class CPDFSearchControl : UserControl
    {
        /// <summary>
        /// PDFViewer
        /// </summary>
        private CPDFViewer pdfView;
        private int currentHighLightIndex { get; set; } = -1;

        private PDFTextSearch textSearch;

        private string keyWord;

        private SolidColorBrush highLightBrush = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xF7, 0x00));

        private int ResultCount = 0;
        public CPDFSearchControl()
        {
            InitializeComponent();
            Loaded += PDFSearch_Loaded;
        }

        public void InitWithPDFViewer(CPDFViewer newPDFView)
        {
            if(pdfView!=newPDFView)
            {
                ClearSearchResult();
                pdfView = newPDFView;
            }
        }

        private void PDFSearch_Loaded(object sender, RoutedEventArgs e)
        {
            textSearch = new PDFTextSearch();
            SearchInput.SearchEvent += SearchInput_SearchEvent;
            SearchInput.ClearEvent += SearchInput_ClearEvent;
            textSearch.SearchCompletedHandler += TextSearch_SearchCompletedHandler;
            SearchResult.SelectionChanged += SearchResult_SelectionChanged;
        }

        private void SearchInput_ClearEvent(object sender, EventArgs e)
        {
            ClearSearchResult();
        }

        private void SearchInput_FindPreviousEvent(object sender, EventArgs e)
        {
            if (currentHighLightIndex > 0)
            {
                currentHighLightIndex--;
                BindSearchResult result = SearchResult.GetItem(currentHighLightIndex);
                HighLightSelectResult(result);
            }
        }

        private void SearchInput_FindNextEvent(object sender, EventArgs e)
        {
            currentHighLightIndex++;
            if (currentHighLightIndex >= 0)
            {
                BindSearchResult result = SearchResult.GetItem(currentHighLightIndex);
                HighLightSelectResult(result);
            }
        }

        private void SearchResult_SelectionChanged(object sender, int e)
        {
            currentHighLightIndex = e;
            BindSearchResult result = SearchResult.GetSelectItem();
            HighLightSelectResult(result);
            ResultText.Text = LanguageHelper.BotaManager.GetString("Text_Result")+ (e+1) + "/" + ResultCount;
        }

        private void TextSearch_SearchCompletedHandler(object sender, TextSearchResult e)
        {
            Dispatcher.Invoke(() =>
            {
                List<BindSearchResult> resultList = new List<BindSearchResult>();

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
                            PageRotate = item.PageRotate
                        });
                    }
                }
                SearchResult.SetSearchResult(resultList);
                ResultCount=resultList.Count;

                if (resultList==null ||  resultList.Count==0)
                {
                    ResultText.Text= string.Empty;
                }
                else
                {
                    ResultText.Text = ResultCount + " " + LanguageHelper.BotaManager.GetString("Tip_Result");
                }
            });
        }

        private void SearchInput_SearchEvent(object sender, string e)
        {
            if (string.IsNullOrEmpty(e))
            {
                return;
            }

            if (pdfView == null || pdfView.Document == null)
            {
                return;
            }

            if (textSearch != null && textSearch.CanDoSearch)
            {
                keyWord = e;
                textSearch.TextSearchDocument = pdfView.Document;
                textSearch.SearchText(e, C_Search_Options.Search_Case_Insensitive);
            }
        }

        private void HighLightSelectResult(BindSearchResult result)
        {
            if (result == null)
            {
                return;
            }

            List<TextSearchItem> selectList = new List<TextSearchItem>();
            selectList.Add(new TextSearchItem()
            {
                PageIndex = result.PageIndex,
                TextRect = result.TextRect,
                TextContent = result.TextContent,
                PageRotate = result.PageRotate,

            });
            pdfView.SetPageSelectText(selectList, highLightBrush);
        }

        private void ClearSearchResult()
        {
            SearchResult?.SetSearchResult(null);
            ResultText.Text = string.Empty;
            SearchInput.SearchKeyWord=string.Empty;
        }
    }
}
