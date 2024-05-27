using ComPDFKit.PDFPage;
using ComPDFKitViewer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComPDFKit.Tool.UndoManger.FindReplaceHistory
{
    public class FindReplaceHistory : IHistory
    {
        public CPDFEditPage EditPage { get; set; } 

        public int PageIndex { get; set; }

        void IHistory.Check(IHistory changeItem, bool undo, bool redo, bool add)
        {
            
        }

        bool IHistory.Redo()
        {
            if (EditPage != null && EditPage.IsValid())
            {
                return EditPage.Redo();
            }
            return false;
        }

        bool IHistory.Undo()
        {
            if (EditPage != null && EditPage.IsValid())
            { 
                return EditPage.Undo();
            }
            return false;
        }
    }
}
