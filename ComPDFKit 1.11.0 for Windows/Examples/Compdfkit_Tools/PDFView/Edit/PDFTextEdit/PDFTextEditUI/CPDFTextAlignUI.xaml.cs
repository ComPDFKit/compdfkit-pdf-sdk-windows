using ComPDFKit.PDFPage.Edit;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Compdfkit_Tools.Edit
{
    public partial class CPDFTextAlignUI : UserControl
    {
        public event EventHandler<TextAlignType> TextAlignChanged;
        public CPDFTextAlignUI()
        {
            InitializeComponent();
        }

        public TextAlignType Alignment { get;private set; }

        public void SetFontAlign(TextAlignType newAlign)
        {
            ClearAlign();
            Alignment = newAlign;
            switch (newAlign)
            {
                case TextAlignType.AlignLeft:
                    AlignLeftBtn.IsChecked = true;
                    AlignLeftPath.Fill = Brushes.Blue;
                    break;
                case TextAlignType.AlignMiddle:
                    AlignCenterBtn.IsChecked = true; 
                    AlignCenterPath.Fill = Brushes.Blue;
                    break;
                case TextAlignType.AlignRight:
                    AlignRightBtn.IsChecked = true;
                    AlignRightPath.Fill = Brushes.Blue;
                    break;
                default:
                    break;
            }
        }

        public void ClearAlign()
        {
            Alignment = TextAlignType.AlignNone;
            AlignLeftPath.Fill = Brushes.Gray;
            AlignCenterPath.Fill= Brushes.Gray;
            AlignRightPath.Fill= Brushes.Gray;
            AlignLeftBtn.IsChecked = false;
            AlignCenterBtn.IsChecked = false;
            AlignRightBtn.IsChecked= false; 
        }

        private void TextAlignBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearAlign();
            ToggleButton clickBtn = sender as ToggleButton;
            if (clickBtn != null && clickBtn.Tag!=null)
            {
                clickBtn.IsChecked = true;
                TextAlignType newAlign = Alignment;

                switch(clickBtn.Tag.ToString())
                {
                    case "AlignLeft":
                        newAlign = TextAlignType.AlignLeft;
                        break;
                    case "AlignCenter":
                        newAlign = TextAlignType.AlignMiddle;
                        break;
                    case "AlignRight":
                        newAlign= TextAlignType.AlignRight;
                        break;
                    default:
                        break;
                }

                if(newAlign!=Alignment)
                {
                    SetFontAlign(newAlign);
                    TextAlignChanged?.Invoke(this, newAlign);
                }
            }
        }
    }
}
