using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using System.Collections.Generic;

namespace ComPDFKit.Tool
{
	public class ListBoxParam:WidgetParm
	{
		public  ListBoxParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            WidgetType = C_WIDGET_TYPE.WIDGET_LISTBOX;
        }

		public bool IsItalic { get; set; }
		public bool IsBold { get; set; }
		public Dictionary<string, string> OptionItems { get; set; }=new Dictionary<string, string>();	
		public List<int> SelectItemsIndex { get; set; }=new List<int>();	

		public override bool CopyTo(AnnotParam transfer)
		{
			ListBoxParam listboxTransfer = transfer as ListBoxParam;
			if (listboxTransfer == null)
			{
				return false;
			}
			if (!base.CopyTo(listboxTransfer))
			{
				return false;
			}

			listboxTransfer.IsItalic = IsItalic;
			listboxTransfer.IsBold = IsBold;
			if (OptionItems != null)
			{
				Dictionary<string, string> optionDict = new Dictionary<string, string>();
				foreach (string dictKey in OptionItems.Keys)
				{
					optionDict[dictKey] = OptionItems[dictKey];
				}

				listboxTransfer.OptionItems = optionDict;
			}

			if (SelectItemsIndex != null)
			{
				List<int> selectList = new List<int>();
				foreach (int index in SelectItemsIndex)
				{
					selectList.Add(index);
				}
				listboxTransfer.SelectItemsIndex = selectList;
			}

			return true;
		}
	}
}
