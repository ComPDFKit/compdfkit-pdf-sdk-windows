using ComPDFKit.Import;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKitViewer.Helper;
using System.Collections.Generic;
using System.Linq;

namespace ComPDFKit.Tool.UndoManger
{
	public class AnnotHistory : IHistory
    {
        internal CPDFAnnotation Annot { get; set; }
        public CPDFDocument PDFDoc { get; set; }

        public HistoryAction Action { get; set; }

        internal int HistoryIndex { get; set; } = 0;

        public AnnotParam CurrentParam { get; set; }
        public AnnotParam PreviousParam { get; set; }

        public virtual int GetPageIndex()
        {
            return -1;
        }

        public virtual int GetAnnotIndex()
        {
            return -1;
        }

        public virtual void SetAnnotIndex(int newIndex)
        {

        }

        public AnnotHistory() 
        { 

        }

        public bool Redo()
        {
            bool result=false;
            switch(Action)
            {
                case HistoryAction.Add:
                    result = Add();
                    break;
                case HistoryAction.Remove:
                    result = Remove();
                    break;
                case HistoryAction.Update:
                    result = Update(false);
                    break;
                default:
                    break;
            }
            return result;
        }

        public bool Undo()
        {
            bool result = false;
            switch (Action)
            {
                case HistoryAction.Add:
                    result = Remove();
                    break;
                case HistoryAction.Remove:
                    result = Add();
                    break;
                case HistoryAction.Update:
                    result = Update(true);
                    break;
                default:
                    break;
            }
            return result;
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
                foreach (IHistory checkHistory in groupHistory.Histories)
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

            foreach (AnnotHistory checkAnnot in checkAnnotList)
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
            if (GetPageIndex() == pageIndex)
            {
                int oldIndex = GetAnnotIndex();
                if (oldIndex == annotIndex && oldIndex >= 0)
                {
                    SetAnnotIndex(-1);
                    HistoryIndex = -1;
                }
                else
                {
                    if (oldIndex > annotIndex || oldIndex <= -1)
                    {
                        SetAnnotIndex(oldIndex - 1);
                        HistoryIndex = oldIndex - 1;
                    }
                }
            }
        }

        internal void UpdateCurrentIndex(int pageIndex, int prevIndex, int annotIndex)
        {
            if (GetPageIndex() == pageIndex && HistoryIndex == prevIndex)
            {
                SetAnnotIndex(annotIndex);
                HistoryIndex = annotIndex;
            }
        }

        internal virtual bool Add()
        {
            return false;
        }

        internal virtual bool Remove()
        {
            return false;
        }

        internal virtual bool Update(bool isUndo)
        {
            return false;
        }

        internal bool MakeAnnotValid(AnnotParam currentParam)
        {
            if (currentParam == null || PDFDoc == null || !PDFDoc.IsValid())
            {
                return false;
            }
           
            CPDFPage pdfPage = PDFDoc.PageAtIndex(currentParam.PageIndex);
            if (pdfPage != null)
            {
                List<CPDFAnnotation> annotList = pdfPage.GetAnnotations();
                if (annotList != null && annotList.Count > currentParam.AnnotIndex)
                {
                    Annot = annotList[currentParam.AnnotIndex];
                    if (Annot != null && Annot.IsValid())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool CheckArrayEqual<T>(T[] compareA, T[] compareB)
        {
            if (compareA == compareB)
            {
                return true;
            }

            if(compareA!=null && compareB!=null)
            {
                if(compareA.Length != compareB.Length) 
                { 
                    return false; 
                }

                for (int i = 0; i < compareA.Length; i++)
                {
                    if (!compareA[i].Equals(compareB[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }
    }
}
