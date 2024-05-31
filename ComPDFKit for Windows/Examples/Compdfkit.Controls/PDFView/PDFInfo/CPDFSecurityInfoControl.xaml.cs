using ComPDFKit.PDFDocument;
using ComPDFKitViewer;
using System.Windows.Controls;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class CPDFSecurityInfoControl : UserControl
    {
        private string T_Allowed = LanguageHelper.DocInfoManager.GetString("Allow_True");
        private string T_NotAllowed = LanguageHelper.DocInfoManager.GetString("Allow_False");

        public PDFViewControl ViewControl;
        public void InitWithPDFViewer(PDFViewControl viewControl)
        {
            this.ViewControl = viewControl;
            if(viewControl!=null && viewControl.PDFViewTool != null )
            {
                CPDFViewer pdfViewer=viewControl.PDFViewTool.GetCPDFViewer();
                CPDFDocument pdfDoc=pdfViewer?.GetDocument();
                if(pdfDoc!=null)
                {
                    InitializeSecurityInfo(pdfDoc);
                }
            }
        }


        public CPDFSecurityInfoControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pass in a boolean value and return the corresponding text.
        /// </summary>
        /// <param name="isTrue"></param>
        /// <returns></returns>
        private string GetStringFromBool(bool isTrue)
        {
            if (isTrue)
            {
                return T_Allowed;
            }
            else
            {
                return T_NotAllowed;
            }
        }

        private void InitializeSecurityInfo(CPDFDocument pdfDocument)
        {
            CPDFPermissionsInfo Permissions = pdfDocument.GetPermissionsInfo();
            AllowsPrintingTextBlock.Text = GetStringFromBool(Permissions.AllowsPrinting);
            AllowsCopyingTextBlock.Text = GetStringFromBool(Permissions.AllowsCopying);
            AllowsDocumentChangesTextBlock.Text = GetStringFromBool(Permissions.AllowsDocumentChanges);
            AllowsDocumentAssemblyTextBlock.Text = GetStringFromBool(Permissions.AllowsDocumentAssembly);
            AllowsCommentingTextBlock.Text = GetStringFromBool(Permissions.AllowsCommenting);
            AllowsFormFieldEntryTextBlock.Text = GetStringFromBool(Permissions.AllowsFormFieldEntry);
        }
    }
}
