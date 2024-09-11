using ComPDFKit.PDFPage;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Tool.UndoManger
{
    public class PDFEditHistory : IHistory
    {
        public CPDFEditPage EditPage { get; set; }
        public int PageIndex { get; set; }
        public bool Redo()
        {
            if (EditPage != null && EditPage.IsValid())
            {
                bool result = EditPage.Redo();
                if (result)
                {
                    EditPage.EndEdit();
                }
                return result;

            }

            return false;
        }

        public bool Undo()
        {
            if (EditPage != null && EditPage.IsValid())
            {
                bool result = EditPage.Undo();
                if (result)
                {
                    EditPage.EndEdit();
                }
                return result;
            }
            return false;
        }

        public void Check(IHistory changeItem, bool undo, bool redo, bool add)
        {

        }
    }
}
