using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComPDFKit.Controls.PDFControlUI
{
    /// <summary>
    /// Interaction logic for AnnotationReplyListControl.xaml
    /// </summary>
    public partial class AnnotationReplyListControl : UserControl
    {
        public ObservableCollection<CPDFAnnotationListUI.ReplyData> ReplyListSource
        {
            get => (ObservableCollection<CPDFAnnotationListUI.ReplyData>)GetValue(ReplyListProperty);
            set => SetValue(ReplyListProperty, value);
        }

        /// <summary>
        /// The source of the reply list.
        /// </summary>
        public static readonly DependencyProperty ReplyListProperty = DependencyProperty.Register(
            nameof(ReplyListSource),
            typeof(ObservableCollection<CPDFAnnotationListUI.ReplyData>),
            typeof(AnnotationReplyListControl),
            new PropertyMetadata(null, OnReplyListSourceChanged));

        private static void OnReplyListSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newReplyListSource = e.NewValue as ObservableCollection<CPDFAnnotationListUI.ReplyData>;
            if (d is AnnotationReplyListControl control)
            {
                control.ReplyList.ItemsSource = newReplyListSource;
            }
        }

        public bool IsShowInput
        {
            get => (bool)GetValue(IsShowInputProperty);
            set => SetValue(IsShowInputProperty, value);
        }

        /// <summary>
        /// The visibility of the reply input.
        /// </summary>
        public static readonly DependencyProperty IsShowInputProperty = DependencyProperty.Register(
            nameof(IsShowInput),
            typeof(bool),
            typeof(AnnotationReplyListControl),
            new PropertyMetadata(false, OnIsShowInputChanged));

        public static event EventHandler ReplyListChanged;
        public static event EventHandler<CPDFAnnotationListUI.ReplyData> ReplyDeleted;


        private static void OnIsShowInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnnotationReplyListControl control)
            {
                control.SetReplyInputVisibility((bool)e.NewValue);
            }
        }

        public bool IsShowList
        {
            get => (bool)GetValue(IsShowListProperty);
            set => SetValue(IsShowListProperty, value);
        }

        /// <summary>
        /// The visibility of the reply list.
        /// </summary>
        public static readonly DependencyProperty IsShowListProperty = DependencyProperty.Register(
            nameof(IsShowList),
            typeof(bool),
            typeof(AnnotationReplyListControl),
            new PropertyMetadata(false, OnIsShowListChanged));

        public void SetReplyInputVisibility(bool isVisible)
        {
            if (isVisible)
            {
                ReplyGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
                ReplyGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                ReplyGrid.RowDefinitions[0].Height = new GridLength(0);
                ReplyGrid.RowDefinitions[1].Height = new GridLength(0);
            }
        }

        private static void OnIsShowListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnnotationReplyListControl control)
            {
                control.SetReplyListVisibility((bool)e.NewValue);
            }
        }

        public void SetReplyListVisibility(bool isVisible)
        {
            ReplyGrid.RowDefinitions[2].Height = isVisible ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
        }

        public static void InvokeReplyListChanged()
        {
            ReplyListChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void InvokeReplyDeleted(CPDFAnnotationListUI.ReplyData replyData)
        {
            ReplyDeleted?.Invoke(null, replyData);
        }

        public AnnotationReplyListControl()
        {
            InitializeComponent();
        }


        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is AnnotationBindData annotBindData)
                {
                    if (!string.IsNullOrEmpty(InputTxb.Text))
                    {
                        var replyAnnot = annotBindData.BindProperty.Annotation.CreateReplyAnnotation();
                        replyAnnot.SetContent(InputTxb.Text);
                        replyAnnot.SetAuthor(Data.CPDFAnnotationData.Author);
                        annotBindData.BindProperty.ReplyList.Add(new CPDFAnnotationListUI.ReplyData
                        {
                            ReplyAnnotation = replyAnnot
                        });
                        InputTxb.Text = string.Empty;
                        InvokeReplyListChanged();
                    }
                }
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            IsShowInput = false;
        }

        private void ContentBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Visibility = Visibility.Collapsed;
                if (textBox.DataContext is CPDFAnnotationListUI.ReplyData replyData)
                {
                    replyData.ReplyAnnotation.SetContent(textBox.Text);
                    InvokeReplyListChanged();
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReplyDeleted -= AnnotationReplyListControl_ReplyDeleted;
            ReplyDeleted += AnnotationReplyListControl_ReplyDeleted;
        }

        private void AnnotationReplyListControl_ReplyDeleted(object sender, CPDFAnnotationListUI.ReplyData e)
        {
            ReplyListSource.Remove(e);
        }
    }

    public class ShowContentBoxCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is TextBox textBox)
            {
                textBox.Visibility = Visibility.Visible;
                textBox.Focus();
                textBox.SelectAll();
            }
        }
    }

    public class DeleteReplyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is CPDFAnnotationListUI.ReplyData replyData)
            {
                replyData.ReplyAnnotation.RemoveAnnot();
                AnnotationReplyListControl.InvokeReplyDeleted(replyData);
                AnnotationReplyListControl.InvokeReplyListChanged();
            }
        }
    }

}
