using ComPDFKit.Import;
using ComPDFKit.NativeMethod;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;
using System.Windows;

namespace ComPDFKit.Tool
{
    /// <summary>
    /// This class is about search result object.
    /// </summary>
    public class TextSearchItem
    {
        /// <summary>
        /// Page index
        /// </summary>
        public int PageIndex;
        /// <summary>
        /// The bounds of the selection on the page(PDF 72DPI)
        /// </summary>
        public Rect TextRect;
        /// <summary>
        /// The text contains in the selection
        /// </summary>
        public string TextContent;
        /// <summary>
        /// Page rotation angle
        /// </summary>
        public int PageRotate;

        public void CreatePaintBrush(Color color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.PaintBrush = new SolidColorBrush(color);
            });
        }

        public SolidColorBrush PaintBrush { get; private set; } = Brushes.Transparent;


        public void CreateBorderBrush(Color color)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.BorderBrush = new SolidColorBrush(color);
            });
        }
        public SolidColorBrush BorderBrush { get; private set; } = Brushes.Transparent;


        public int BorderThickness;
        public Thickness Padding;
    }

    /// <summary>
    /// Result of text search 
    /// </summary>
    public class TextSearchResult
    {
        /// <summary>
        /// Start page index
        /// </summary>
        public int StartPage;
        /// <summary>
        /// End page index
        /// </summary>
        public int EndPage;
        /// <summary>
        /// Percentage of search process
        /// </summary>
        public double Percent;
        /// <summary>
        /// Current page index
        /// </summary>
        public int CurrentPage;
        /// <summary>
        /// The number of search result
        /// </summary>
        public int TotalCount;
        /// <summary>
        /// The maximum value of search result
        /// </summary>
        public int PageMaxCount;
        /// <summary>
        /// Details of current search result
        /// </summary>
        public Dictionary<int, List<TextSearchItem>> Items = new Dictionary<int, List<TextSearchItem>>();
    }

    /// <summary>
    /// This class is about text search.
    /// </summary>
    public class PDFTextSearch
    {
        /// <summary>
        /// A notification that a find operation finished working on a page of a document.
        /// </summary>
        public event EventHandler<TextSearchResult> SearchPercentHandler;

        /// <summary>
        /// A notification that a find operation cancels.
        /// </summary>
        public event EventHandler<TextSearchResult> SearchCancelHandler;

        /// <summary>
        /// A notification that a find operation finished of a document.
        /// </summary>
        public event EventHandler<TextSearchResult> SearchCompletedHandler;

        /// <summary>
        /// Associates a CPDFDocument.
        /// </summary>
        public CPDFDocument TextSearchDocument;

        private CPDFDocument mSearchDocument;
        private bool isCancel;
        private string searchKeywords;
        private string password;
        private C_Search_Options searchOption;
        private int startPage;
        private int endPage;

        /// <summary>
        /// Whether to allow to search.
        /// </summary>
        public bool CanDoSearch { get; private set; } = true;

        /// <summary>
        /// Constructor function.
        /// </summary>
        public PDFTextSearch()
        {

        }

        private void DoWork()
        {
            endPage = endPage == -1 ? TextSearchDocument.PageCount - 1 : Math.Min(TextSearchDocument.PageCount - 1, endPage);
            TextSearchResult searchResult = new TextSearchResult();
            searchResult.StartPage = startPage;
            searchResult.EndPage = endPage;
            double searchPercent = 100;

            mSearchDocument = CPDFDocument.InitWithFilePath(TextSearchDocument.FilePath);
            if (mSearchDocument.IsLocked && !string.IsNullOrEmpty(password))
            {
                mSearchDocument.UnlockWithPassword(password);
            }

            if (mSearchDocument != null && !mSearchDocument.IsLocked)
            {
                int pageMaxCount = 0;
                int recordCount = 0;
                searchPercent = 0;
                for (int i = startPage; i <= endPage; i++)
                {
                    CPDFTextSearcher mPDFTextSearcher = new CPDFTextSearcher();
                    CPDFPage pageCore = mSearchDocument.PageAtIndex(i);
                    if(pageCore == null)
                    {
                        continue;
                    }   

                    CPDFTextPage textPage = pageCore.GetTextPage();
                    int startIndex = 0;
                    List<TextSearchItem> textSearchItems = new List<TextSearchItem>();
                    if (mPDFTextSearcher.FindStart(textPage, searchKeywords, searchOption, startIndex))
                    {
                        CRect textRect = new CRect();
                        string textContent = "";
                        while (mPDFTextSearcher.FindNext(pageCore, textPage, ref textRect, ref textContent, ref startIndex))
                        {
                            if (textContent == "")
                            {
                                textContent = searchKeywords;
                            }

                            textSearchItems.Add(new TextSearchItem()
                            {
                                PageIndex = i,
                                TextRect = new Rect(textRect.left, textRect.top, textRect.width(), textRect.height()),
                                TextContent = textContent,
                                PageRotate = pageCore.Rotation
                            });

                            var matchResult = Regex.Matches(textContent, searchKeywords, RegexOptions.IgnoreCase);
                            if (matchResult != null)
                            {
                                recordCount += matchResult.Count;
                            }
                        }
                    }

                    mPDFTextSearcher.FindClose();
                    if (textSearchItems.Count > 0)
                    {
                        searchResult.Items.Add(i, textSearchItems);
                    }

                    pageMaxCount = Math.Max(pageMaxCount, textSearchItems.Count);
                    searchResult.TotalCount = recordCount;
                    searchResult.PageMaxCount = pageMaxCount;
                    if (SearchPercentHandler != null)
                    {
                        searchPercent = (int)((i + 1 - startPage) * 100 / (endPage + 1 - startPage));
                        searchResult.Percent = searchPercent;
                        searchResult.CurrentPage = i;
                        SearchPercentHandler.Invoke(this, searchResult);
                    }

                    mSearchDocument.ReleasePages(i);
                    if (isCancel)
                    {
                        break;
                    }
                }

                searchPercent = 100;
                mSearchDocument.Release();
            }

            if (SearchCompletedHandler != null && !isCancel)
            {
                searchResult.Percent = searchPercent;
                SearchCompletedHandler.Invoke(this, searchResult);
            }

            if (SearchCancelHandler != null && isCancel)
            {
                SearchCancelHandler.Invoke(this, searchResult);
            }

            CanDoSearch = true;
            isCancel = false;
        }

        /// <summary>
        /// Cancles a search.
        /// </summary>

        public void CancleSearch()
        {
            isCancel = true;
        }

        /// <summary>
        /// Searches the specified string in the document.
        /// </summary>
        /// <param name="search">Search the specified string</param>
        /// <param name="option">Search options</param>
        /// <param name="pwd">Document password</param>
        /// <param name="startPage">Start page index</param>
        /// <param name="endPage">End page index</param>
        /// <returns>Returns true on success, false on failure</returns>
        public bool SearchText(string search, C_Search_Options option, string pwd = "", int startPage = 0, int endPage = -1)
        {
            if (CPDFSDKVerifier.TextSearch == false)
            {
                Trace.WriteLine("Your license does not support this feature, please upgrade your license privilege.");
                return false;
            }

            if (CanDoSearch)
            {
                searchKeywords = search;
                password = pwd;
                searchOption = option;
                this.startPage = startPage;
                this.endPage = endPage;
                isCancel = false;
                CanDoSearch = false;
                Thread taskThread = new Thread(DoWork);
                taskThread.Start();
                return true;
            }
            return false;
        }
    }
}
