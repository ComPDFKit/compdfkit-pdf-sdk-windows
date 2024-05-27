using ComPDFKitViewer.Helper;
using System.Collections.Generic;

namespace ComPDFKit.Tool.UndoManger
{
	public class MultiAnnotHistory : IHistory
    {
        public List<AnnotHistory> Histories { get; set; } = new List<AnnotHistory>();
        public bool Redo()
        {
            if (Histories != null && Histories.Count > 0)
            {
                bool success=true;
                foreach (AnnotHistory history in Histories)
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
                foreach (AnnotHistory history in Histories)
                {
                    success &= history.Undo();
                }
                return success;
            }
            return false;
        }

        public void Check(IHistory checkItem, bool undo, bool redo, bool add)
        {
            List<AnnotHistory> checkAnnotList= new List<AnnotHistory>();
            if (checkItem is MultiAnnotHistory)
            {
                MultiAnnotHistory multiHistory = (MultiAnnotHistory)checkItem;
                foreach (AnnotHistory checkAnnot in multiHistory.Histories)
                {
                    if (checkAnnot == null)
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

            foreach (AnnotHistory checkAnnot in checkAnnotList)
            {
                if (add && checkAnnot.Action == HistoryAction.Remove)
                {
                    //remove
                    SubCurrentIndex(checkAnnot.GetPageIndex(),checkAnnot.GetAnnotIndex());
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

                if(undo && checkAnnot.Action == HistoryAction.Remove) 
                {
                    //add
                    UpdateCurrentIndex(checkAnnot.GetPageIndex(), checkAnnot.HistoryIndex, checkAnnot.GetAnnotIndex());
                }

                if(redo && checkAnnot.Action == HistoryAction.Add)
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
                    if (oldIndex > annotIndex || oldIndex<=-1)
                    {
                        annotHistory.SetAnnotIndex(oldIndex-1);
                    }

                    if (oldIndex == annotIndex)
                    {
                        annotHistory.SetAnnotIndex(-1);
                    }
                }
            }
        }

        internal void UpdateCurrentIndex(int pageIndex,int prevIndex, int annotIndex)
        {
            foreach (AnnotHistory annotHistory in Histories)
            {
                if (annotHistory.GetPageIndex() == pageIndex && annotHistory.HistoryIndex==prevIndex)
                {
                    annotHistory.SetAnnotIndex(annotIndex);
                }
            }
        }
    }
}
