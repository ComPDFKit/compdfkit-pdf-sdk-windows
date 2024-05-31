using ComPDFKit.PDFDocument;
using ComPDFKitViewer;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFCreateInfoControl : UserControl
    {
        public PDFViewControl ViewControl;
        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            this.ViewControl = viewControl;
            if(viewControl!=null && viewControl.PDFViewTool != null)
            {
                CPDFViewer pdfViewer=viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc=pdfViewer.GetDocument();
                if(pdfDoc!=null)
                {
                    InitializeCreateInfo(pdfDoc);
                }    
            }
        }


        public CPDFCreateInfoControl()
        {
            InitializeComponent();
        }

        private void InitializeCreateInfo(CPDFDocument cpdfDocument)
        {
            VersionTextBlock.Text = cpdfDocument.GetInfo().Version;
            PageCountTextBlock.Text = cpdfDocument.PageCount.ToString();
            CreatorTextBlock.Text = cpdfDocument.GetInfo().Creator;
            CreationDateTextBlock.Text = ConverPDFTime(cpdfDocument.GetInfo().CreationDate);
            ModificationDateTextBlock.Text = ConverPDFTime(cpdfDocument.GetInfo().ModificationDate);
        }

        private string ConverPDFTime(string timeText)
        {
            try
            {
                if (Regex.IsMatch(timeText, "(?<=D\\:)[0-9]+(?=[\\+\\-])"))
                {
                    string dateStr = Regex.Match(timeText, "(?<=D\\:)[0-9]+(?=[\\+\\-])").Value;
                    timeText = dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6, 2) + " " + dateStr.Substring(8, 2) + ":" +
                        dateStr.Substring(10, 2) + ":" + dateStr.Substring(12, 2);
                }
                else if (Regex.IsMatch(timeText, "(?<=D\\:)[0-9]+"))
                {
                    string dateStr = Regex.Match(timeText, "(?<=D\\:)[0-9]+").Value;
                    if (dateStr.Length > 0)
                    {
                        timeText = dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6, 2) + " " + dateStr.Substring(8, 2) + ":" +
                        dateStr.Substring(10, 2) + ":" + dateStr.Substring(12, 2);
                    }
                }
            }
            catch(Exception ex)
            {

            }
           

            return timeText;
        }
    }
}
