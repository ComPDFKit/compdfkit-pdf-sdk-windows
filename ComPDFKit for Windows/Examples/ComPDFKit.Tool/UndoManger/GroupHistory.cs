using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComPDFKit.Tool.UndoManger
{
    public class GroupHistory : IHistory
    {
        public List<IHistory> Histories { get; set; } = new List<IHistory>();
        public bool Redo()
        {
            if (Histories != null && Histories.Count > 0)
            {
                bool success = true;

                List<AnnotHistory> checkAnnotList = new List<AnnotHistory>();
                List<IHistory> OtherList = new List<IHistory>();

                foreach (IHistory history in Histories)
                {
                    if (history is AnnotHistory)
                    {
                        checkAnnotList.Add(history as AnnotHistory);
                        continue;
                    }

                    OtherList.Add(history);
                }

                List<AnnotHistory> loopList = new List<AnnotHistory>();
                if (checkAnnotList != null && checkAnnotList.Count > 0)
                {
                    List<int> pageList = checkAnnotList.AsEnumerable().Select(x => x.GetPageIndex()).Distinct().ToList();
                    pageList.Sort();
                    foreach (int pageIndex in pageList)
                    {
                        List<AnnotHistory> groupList = checkAnnotList.AsEnumerable().Where(x => x.GetPageIndex() == pageIndex).OrderByDescending(x => x.GetAnnotIndex()).ToList();
                        if (groupList != null && groupList.Count > 0)
                        {
                            loopList.AddRange(groupList);
                        }
                    }
                }

                foreach (AnnotHistory history in loopList)
                {
                    success &= history.Redo();
                }

                foreach (IHistory history in OtherList)
                {
                    success &= history.Redo();
                }
                return success;
            }
            return false;
        }

        public bool Undo()
        {
            if (Histories != null && Histories.Count > 0)
            {
                bool success = true;

                List<AnnotHistory> checkAnnotList = new List<AnnotHistory>();
                List<IHistory> OtherList = new List<IHistory>();

                foreach (IHistory history in Histories)
                {
                    if (history is AnnotHistory)
                    {
                        checkAnnotList.Add(history as AnnotHistory);
                        continue;
                    }

                    OtherList.Add(history);
                }

                List<AnnotHistory> loopList = new List<AnnotHistory>();
                if (checkAnnotList != null && checkAnnotList.Count > 0)
                {
                    List<int> pageList = checkAnnotList.AsEnumerable().Select(x => x.GetPageIndex()).Distinct().ToList();
                    pageList.Sort();
                    foreach (int pageIndex in pageList)
                    {
                        List<AnnotHistory> groupList = checkAnnotList.AsEnumerable().Where(x => x.GetPageIndex() == pageIndex).OrderByDescending(x => x.GetAnnotIndex()).ToList();
                        if (groupList != null && groupList.Count > 0)
                        {
                            loopList.AddRange(groupList);
                        }
                    }
                }

                foreach (AnnotHistory history in loopList)
                {
                    success &= history.Undo();
                }

                foreach (IHistory history in OtherList)
                {
                    success &= history.Undo();
                }
                return success;
            }
            return false;
        }

        public void Check(IHistory checkItem, bool undo, bool redo, bool add)
        {
            List<AnnotHistory> checkAnnotList = new List<AnnotHistory>();
            if (checkItem is MultiAnnotHistory)
            {
                MultiAnnotHistory multiHistory = (MultiAnnotHistory)checkItem;
                foreach (AnnotHistory checkAnnot in multiHistory.Histories)
                {
                    if (checkAnnot == null || checkAnnotList.Contains(checkAnnot))
                    {
                        continue;
                    }
                    checkAnnotList.Add(checkAnnot);
                }
            }

            if (checkItem is GroupHistory)
            {
                GroupHistory groupHistory = (GroupHistory)checkItem;
                foreach(IHistory checkHistory in groupHistory.Histories)
                {
                    AnnotHistory checkAnnot = checkHistory as AnnotHistory;
                    if (checkAnnot == null || checkAnnotList.Contains(checkAnnot))
                    {
                        continue;
                    }
                    checkAnnotList.Add(checkAnnot);
                }
            }

            if (checkItem is AnnotHistory)
            {
                checkAnnotList.Add((AnnotHistory)checkItem);
            }

            List<AnnotHistory> loopList = new List<AnnotHistory>();
            if (checkAnnotList != null && checkAnnotList.Count > 0)
            {
                List<int> pageList = checkAnnotList.AsEnumerable().Select(x => x.GetPageIndex()).Distinct().ToList();
                pageList.Sort();
                foreach (int pageIndex in pageList)
                {
                    List<AnnotHistory> groupList = checkAnnotList.AsEnumerable().Where(x => x.GetPageIndex() == pageIndex).OrderByDescending(x => x.GetAnnotIndex()).ToList();
                    if (groupList != null && groupList.Count > 0)
                    {
                        loopList.AddRange(groupList);
                    }
                }
            }

            foreach (AnnotHistory checkAnnot in loopList)
            {
                if (add && checkAnnot.Action == HistoryAction.Remove)
                {
                    //remove
                    SubCurrentIndex(checkAnnot.GetPageIndex(), checkAnnot.GetAnnotIndex());
                }

                if (undo && checkAnnot.Action == HistoryAction.Add)
                {
                    //remove
                    SubCurrentIndex(checkAnnot.GetPageIndex(), checkAnnot.GetAnnotIndex());
                }

                if (redo && checkAnnot.Action == HistoryAction.Remove)
                {
                    //remove
                    SubCurrentIndex(checkAnnot.GetPageIndex(), checkAnnot.GetAnnotIndex());
                }

                if (undo && checkAnnot.Action == HistoryAction.Remove)
                {
                    //add
                    UpdateCurrentIndex(checkAnnot.GetPageIndex(), checkAnnot.HistoryIndex, checkAnnot.GetAnnotIndex());
                }

                if (redo && checkAnnot.Action == HistoryAction.Add)
                {
                    //add
                    UpdateCurrentIndex(checkAnnot.GetPageIndex(), checkAnnot.HistoryIndex, checkAnnot.GetAnnotIndex());
                }
            }
        }

        internal void SubCurrentIndex(int pageIndex, int annotIndex)
        {
            foreach (AnnotHistory annotHistory in Histories)
            {
                if (annotHistory.GetPageIndex() == pageIndex)
                {
                    int oldIndex = annotHistory.GetAnnotIndex();
                    if (oldIndex == annotIndex && oldIndex >= 0)
                    {
                        annotHistory.SetAnnotIndex(-1);
                        annotHistory.HistoryIndex = -1;
                    }
                    else
                    {
                        if (oldIndex > annotIndex || oldIndex <= -1)
                        {
                            annotHistory.SetAnnotIndex(oldIndex - 1);
                            annotHistory.HistoryIndex = oldIndex - 1;
                        }
                    }
                }
            }
        }

        internal void UpdateCurrentIndex(int pageIndex, int prevIndex, int annotIndex)
        {
            foreach (AnnotHistory annotHistory in Histories)
            {
                if (annotHistory.GetPageIndex() == pageIndex && annotHistory.HistoryIndex == prevIndex)
                {
                    annotHistory.SetAnnotIndex(annotIndex);
                    annotHistory.HistoryIndex = annotIndex;
                }
            }
        }
    }
}
