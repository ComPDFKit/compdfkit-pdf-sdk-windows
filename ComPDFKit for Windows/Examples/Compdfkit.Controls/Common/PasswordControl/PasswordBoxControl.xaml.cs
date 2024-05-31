using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComPDFKit.Controls.Common
{
    /// <summary>
    /// Interaction logic for PasswordControl.xaml
    /// </summary>
    public partial class PasswordBoxControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty WatermarkProperty =
    DependencyProperty.Register("Watermark", typeof(string), typeof(PasswordBoxControl), new PropertyMetadata(null));
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty =
DependencyProperty.Register("Password", typeof(string), typeof(PasswordBoxControl), new PropertyMetadata(string.Empty));
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        #region - 用于绑定ViewModel部分 -

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(PasswordBoxControl), new PropertyMetadata(default(ICommand)));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(PasswordBoxControl), new PropertyMetadata(default(object)));

        public IInputElement CommandTarget { get; set; }

        #endregion

        public static readonly RoutedEvent PasswordChangedEvent =
        EventManager.RegisterRoutedEvent("PasswordChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PasswordBoxControl));

        public event RoutedEventHandler PasswordChanged
        {
            add => AddHandler(PasswordChangedEvent, value);
            remove => RemoveHandler(PasswordChangedEvent, value);
        }

        public PasswordBoxControl()
        {
            InitializeComponent();
            Grid.DataContext = this;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Password == string.Empty)
            {
                DisplayPasswordChk.IsChecked = false;
            }
        }
    }
}
