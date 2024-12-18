using System;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Edit
{
    public partial class CPDFTextMarkupUI : UserControl
    {
        public event EventHandler<bool> TextUnderlineChanged;
        public event EventHandler<bool> TextStrikethroughChanged;

        public CPDFTextMarkupUI()
        {
            InitializeComponent();
        }

        private void AddUnderline_Click(object sender, RoutedEventArgs e)
        {
            TextUnderlineChanged?.Invoke(this, true);
        }

        private void RemoveUnderline_Click(object sender, RoutedEventArgs e)
        {
            TextUnderlineChanged?.Invoke(this, false);
        }

        private void AddStrikethrough_Click(object sender, RoutedEventArgs e)
        {
            TextStrikethroughChanged?.Invoke(this, true);
        }

        private void RemoveStrikethrough_Click(object sender, RoutedEventArgs e)
        {
            TextStrikethroughChanged?.Invoke(this, false);
        }
    }
}
