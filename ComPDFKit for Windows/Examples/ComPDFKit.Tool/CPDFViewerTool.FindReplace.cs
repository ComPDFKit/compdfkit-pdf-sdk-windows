using ComPDFKit.PDFPage;
using ComPDFKit.PDFPage.Edit;
using ComPDFKit.Tool.Help;
using ComPDFKit.Tool.UndoManger.FindReplaceHistory;
using ComPDFKit.Viewer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ComPDFKit.Tool
{
    public partial class CPDFViewerTool
    {
        // find text
        private string findText;

        // find options
        private C_Search_Options options;

        // current selection index in editTextFindSelectionList
        private int currentSelectionIndex = -1;

        // current selection
        private CPDFEditTextFindSelection currentTextFindSelection;

        // EditTextFindSelection list in single page
        private List<CPDFEditTextFindSelection> editTextFindSelectionList;

        // Whether to allow circular search to avoid infinite loops when there are no matches in the entire document
        private bool canLoop = true;

        // When replacement occurs, set this variable to true
        private bool isReplaced = false;

        // Set this variable to true when there is a page jump during the search process
        private bool isOtherPage = false;

        // current selection
        private CPDFEditTextFindSelection editTextFindSelection;


        private CPDFEditPage findSelectionEditPage;

        private int nextPageIndex = 0;

        private int tempPageIndex = -1;

        private int currentPageIndex
        {
            get
            {
                if (findSelectionEditPage != null)
                {
                    return findSelectionEditPage.GetPageIndex();
                }
                else
                {
                    return -1;
                }
            }
        }

        public bool CheckPageVisiable()
        {
            return (currentPageIndex >= PDFViewer.CurrentRenderFrame.Renders.First().Key &&
                currentPageIndex <= PDFViewer.CurrentRenderFrame.Renders.Last().Key);
        }

        public void StartFindText(string findText, C_Search_Options options)
        {
            tempPageIndex = PDFViewer.CurrentRenderFrame.PageIndex;

            // Check if the current mode is content editing
            if (currentModel != ToolType.ContentEdit)
            {
                SetToolType(ToolType.ContentEdit);
            }

            // Check if the findText is empty
            if (string.IsNullOrEmpty(findText))
            {
                return;
            }

            if (this.findText != findText)
            {

            }

            this.findText = findText;
            this.options = options;
            canLoop = true;
            FindText();
        }

        internal void FindText([CallerMemberName] string caller = "")
        {
            // Start searching from the currently displayed page
            CPDFPage pdfPage = GetCPDFViewer().GetDocument().PageAtIndex(nextPageIndex);
            findSelectionEditPage = pdfPage.GetEditPage();
            findSelectionEditPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage | CPDFEditType.EditPath);

            // If the passed value is null, it will cause the program to freeze
            if (string.IsNullOrEmpty(findText))
            {
                return;
            }

            findSelectionEditPage.FindText(findText, options);

            // Get all search content of the current page for subsequent search and highlighting
            editTextFindSelectionList = findSelectionEditPage.GetTextFindSelectionList();

            if (caller == "FindPrevious")
            {
                currentSelectionIndex = editTextFindSelectionList.Count;
            }
            else
            {
                currentSelectionIndex = -1;
            }
            isReplaced = false;
        }

        public bool FindPrevious()
        { 
            if (!CheckPageVisiable())
            {
                FindText();
            }

            int tempSelectionIndex = currentSelectionIndex - 1;

            if (tempSelectionIndex >= 0)
            {
                currentSelectionIndex = tempSelectionIndex;
            }
            // The result of the previous option does not exist, try to find the result of another page
            else
            {
                // When there is no cross-page search, you need to preset the cross-page status
                if (!isOtherPage && editTextFindSelectionList?.Count != 0)
                {
                    nextPageIndex = currentPageIndex;
                    isOtherPage = true;
                }

                if (currentPageIndex != 0)
                {
                    tempPageIndex = currentPageIndex - 1;
                }
                else
                {
                    if (canLoop)
                    {
                        tempPageIndex = PDFViewer.GetDocument().PageCount - 1;
                        canLoop = false;
                    }
                    else
                    {
                        tempPageIndex -= 1;
                    }
                }

                // Search upwards
                if (tempPageIndex >= 0)
                {
                    nextPageIndex = tempPageIndex;
                    FindText();
                    currentSelectionIndex = editTextFindSelectionList.Count;
                    return FindPrevious();
                }

                // Failed to find, set the search status to the initial state
                else
                {
                    isOtherPage = false;
                    canLoop = true;
                    return false;
                }
            }

            editTextFindSelection = editTextFindSelectionList[currentSelectionIndex];
            GoToFoundDestination(editTextFindSelection);
            isOtherPage = false;
            canLoop = true;
            return true;
        }

        public bool FindNext()
        {
            if (currentPageIndex == -1)
            {
                return false;
            }

            if (!CheckPageVisiable())
            {
                FindText();
            }

            int tempSelectionIndex = 0;
            if (isReplaced)
            {
                tempSelectionIndex = currentSelectionIndex;
                isReplaced = false;
            }
            else
            {
                tempSelectionIndex = currentSelectionIndex + 1;
            }

            if (tempSelectionIndex < editTextFindSelectionList?.Count)
            {
                currentSelectionIndex = tempSelectionIndex;
            }
            else
            {
                // Search across pages downwards, only allow looping when reaching the bottom for the first time
                if (currentPageIndex != PDFViewer.GetDocument().PageCount - 1)
                {
                    tempPageIndex = currentPageIndex + 1;
                }
                else
                {
                    if (canLoop)
                    {
                        tempPageIndex = 0;
                        canLoop = false;
                    }
                    else
                    {
                        tempPageIndex += 1;
                    }
                }

                if (tempPageIndex < PDFViewer.GetDocument().PageCount)
                {
                    nextPageIndex = tempPageIndex;
                    FindText();
                    currentSelectionIndex = -1;
                    return FindNext();
                }
                // Failed to find, set the search status to the initial state
                else
                {
                    canLoop = true;
                    return false;
                }
            }

            editTextFindSelection = editTextFindSelectionList[currentSelectionIndex];

            GoToFoundDestination(editTextFindSelection);

            canLoop = true;
            return true;
        }

        private void GoToFoundDestination(CPDFEditTextFindSelection editTextFindSelection, bool isReplacing = false)
        {
            float Position_X = editTextFindSelection.RectList[0].left;
            float Position_Y = editTextFindSelection.RectList[0].top;
            editTextFindSelection.GetTextArea(isReplacing).GetLastSelectChars();

            SelectedEditAreaForIndex(editTextFindSelection.GetPageIndex(), editTextFindSelection.GetTextAreaIndex());
            PDFViewer.GoToPage(editTextFindSelection.GetPageIndex(), new System.Windows.Point(Position_X, Position_Y));
        }

        public bool ReplaceText(string text)
        {
            if (findSelectionEditPage != null)
            {
                // Automatically search down when continuously triggered for replacement
                if (isReplaced)
                {
                    FindNext();
                }

                // An overflow error occurred: Just translated and started searching, but no content was selected
                if (currentSelectionIndex < 0 || currentSelectionIndex > editTextFindSelectionList.Count)
                {
                    return false;
                }

                // Current search age has no replaceable content error
                if (editTextFindSelectionList.Count == 0)
                {
                    return false;
                }

                // Get the current search result
                CPDFEditTextFindSelection editTextFindSelection = editTextFindSelectionList[currentSelectionIndex];
                var area = editTextFindSelection.GetTextArea();
                var rect = area.SelectLineRects[0];
                CPDFEditTextArea editTextArea = findSelectionEditPage.ReplaceText(editTextFindSelection, text);
                editTextFindSelection.GetTextArea(true).GetLastSelectChars();
                if (editTextArea != null)
                {
                    findSelectionEditPage.EndEdit();
                    findSelectionEditPage.FindText(findText, options);
                    editTextFindSelectionList = findSelectionEditPage.GetTextFindSelectionList();
                    {
                        FindReplaceHistory findReplaceHistory = new FindReplaceHistory();
                        findReplaceHistory.EditPage = findSelectionEditPage;
                        findReplaceHistory.PageIndex = findSelectionEditPage.GetPageIndex();
                        PDFViewer.UndoManager.AddHistory(findReplaceHistory);
                    }
                }

                isReplaced = true;

                GoToFoundDestination(editTextFindSelection, true);
                return true;
            }

            return false;
        }

        public int ReplaceAllText(string replaceText)
        {
            int changeCount = 0;
            for (int pageIndex = 0; pageIndex < PDFViewer.GetDocument().PageCount; pageIndex++)
            {
                CPDFPage page = PDFViewer.GetDocument().PageAtIndex(pageIndex);
                CPDFEditPage editPage = page.GetEditPage();
                editPage.BeginEdit(CPDFEditType.EditText | CPDFEditType.EditImage | CPDFEditType.EditPath);
                editPage.FindText(findText, options);
                List<CPDFEditTextFindSelection> editTextFindSelectionList = editPage.GetTextFindSelectionList();

                for (int i = editTextFindSelectionList.Count - 1; i >= 0; i--)
                {
                    if (editTextFindSelectionList.Count == 0)
                    {
                        continue;
                    }
                    changeCount += editTextFindSelectionList.Count;
                    editPage.FindText(findText, options);

                    editPage.ReplaceText(editTextFindSelectionList[i], replaceText);
                }
            }

            ResetFindPlaceStatus();

            SelectedEditAreaForIndex(-1, -1);
            PDFViewer.UpdateRenderFrame();
            StartFindText(findText, options);
            return 0;
        }

        private void ResetFindPlaceStatus()
        {
            currentSelectionIndex = -1;
            tempPageIndex = -1;

            isOtherPage = false;
            isReplaced = false;
            canLoop = true; 
        }
    }
}
