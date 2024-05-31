using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFAnnotation.Form;
using System.Collections.Generic;

namespace ComPDFKit.Tool
{
	public class ComboBoxParam:WidgetParm
	{
		public  ComboBoxParam ()
        {
            CurrentType = C_ANNOTATION_TYPE.C_ANNOTATION_WIDGET;
            WidgetType = C_WIDGET_TYPE.WIDGET_COMBOBOX;
        }

		public bool IsItalic { get; set; }
		public bool IsBold { get; set; }
		public Dictionary<string,string> OptionItems { get; set; }=new Dictionary<string,string>();	
		public List<int> SelectItemsIndex { get; set; } = new List<int>();

		public override bool CopyTo(AnnotParam transfer)
		{
			ComboBoxParam comboboxTransfer = transfer as ComboBoxParam;
			if (comboboxTransfer == null)
			{
				return false;
			}
			if (!base.CopyTo(comboboxTransfer))
			{
				return false;
			}
			
			comboboxTransfer.IsItalic = IsItalic;
			comboboxTransfer.IsBold = IsBold;
			if(OptionItems != null)
			{
				Dictionary<string, string> optionDict=new Dictionary<string, string>();
				foreach (string dictKey in OptionItems.Keys)
				{
					optionDict[dictKey] = OptionItems[dictKey];
				}

				comboboxTransfer.OptionItems = optionDict;
			}

			if(SelectItemsIndex != null)
			{
				List<int> selectList=new List<int>();
				foreach (int index in SelectItemsIndex)
				{
					selectList.Add(index);
				}
				comboboxTransfer.SelectItemsIndex = selectList;
			}

			return true;
		}
	}
}
