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
                foreach (IHistory history in Histories)
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
                foreach (IHistory history in Histories)
                {
                    success &= history.Undo();
                }
                return success;
            }
            return false;
        }

        public void Check(IHistory checkItem, bool undo, bool redo, bool add)
        {
            
        }
    }
}
