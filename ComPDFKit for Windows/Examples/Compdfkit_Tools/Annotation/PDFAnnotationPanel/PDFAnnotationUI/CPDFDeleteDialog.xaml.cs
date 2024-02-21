using System.Windows;

namespace Compdfkit_Tools.Annotation.PDFAnnotationPanel.PDFAnnotationUI
{
    public partial class CPDFDeleteDialog : Window
    {
        private string titleContent = "";
        private string contentContent = "";
        public bool IsDelete = false;
        public CPDFDeleteDialog()
        {
            InitializeComponent();
        }

        public CPDFDeleteDialog(string title, string content) : this()
        {
            titleContent = title;
            contentContent = content;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsDelete = false;
            this.Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            IsDelete = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title.Text = titleContent;
            Content.Text = contentContent;
        }
    }
}
