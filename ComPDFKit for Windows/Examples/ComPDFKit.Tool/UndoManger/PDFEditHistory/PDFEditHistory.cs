using ComPDFKit.PDFPage;
using ComPDFKitViewer.Helper;

namespace ComPDFKit.Tool.UndoManger
{
	public class PDFEditHistory : IHistory
	{
		public CPDFEditPage EditPage { get; set; }
		public int PageIndex {  get; set; }
		public bool Redo()
		{
			if(EditPage!=null && EditPage.IsValid())
			{
				return EditPage.Redo();
			}
			
			return false;
		}

		public bool Undo()
		{
			if(EditPage!=null && EditPage.IsValid())
			{
				return EditPage.Undo();
			}
			return false;
		}

		public void Check(IHistory changeItem, bool undo, bool redo, bool add)
        {

		}
	}
}
