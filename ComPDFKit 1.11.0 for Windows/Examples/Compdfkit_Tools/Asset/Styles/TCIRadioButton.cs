using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.Asset.Styles
{
    public class TCIRadioButton : RadioButton
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TCIRadioButton), new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(TCIRadioButton), new PropertyMetadata(string.Empty));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty TextWidthProperty =
    DependencyProperty.Register("TextWidth", typeof(double), typeof(TCIRadioButton), new PropertyMetadata());

        public double TextWidth
        {
            get => (double)GetValue(TextWidthProperty);
            set => SetValue(TextWidthProperty, value);
        }

        static TCIRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TCIRadioButton), new FrameworkPropertyMetadata(typeof(TCIRadioButton)));
        }


    }
}
