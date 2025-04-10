using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.Common.BaseControl
{
    public partial class WritableComboBox : UserControl
    {
        //判断鼠标是否悬停在此按钮上，通过MouseLeave-Enter赋值。
        public bool IsloseFocus = true;

        public bool IsCurrentPage
        {
            get { return (bool)GetValue(IsCurrentPageProperty); }
            set { SetValue(IsCurrentPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCurrentPageProperty =
            DependencyProperty.Register("IsCurrentPage", typeof(bool), typeof(WritableComboBox), new PropertyMetadata(false));

        public Visibility IsAllPageVisible
        {
            get { return (Visibility)GetValue(IsAllPageVisibleProperty); }
            set
            {
                SetValue(IsAllPageVisibleProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsAllPageVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAllPageVisibleProperty =
            DependencyProperty.Register("IsAllPageVisible", typeof(Visibility), typeof(WritableComboBox), new PropertyMetadata(Visibility.Visible, (d, e) =>
            {
                if ((Visibility)e.NewValue != Visibility.Visible)
                {
                    (d as WritableComboBox).SetIndexByVisiblity((Visibility)e.NewValue);
                }
            }));

        private void SetIndexByVisiblity(Visibility visible)
        {
            writableComboBox.SelectedIndex = 1;
        }

        public bool CurrentPage
        {
            get { return (bool)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(bool), typeof(WritableComboBox), new PropertyMetadata(false));

        public bool EvenPageIsEnabled
        {
            get { return (bool)GetValue(EvenPageIsEnabledProperty); }
            set { SetValue(EvenPageIsEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EvenPageIsEnabledProperty =
            DependencyProperty.Register("EvenPageIsEnabled", typeof(bool), typeof(WritableComboBox), new PropertyMetadata(true));

        public string SelectedIndex
        {
            get { return (string)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(string), typeof(WritableComboBox), new PropertyMetadata("0"));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =

            DependencyProperty.Register("Text", typeof(string), typeof(WritableComboBox), new PropertyMetadata(""));

        private List<int> pageIndexList = new List<int>();

        public List<int> PageIndexList
        {
            get { return (List<int>)GetValue(PageIndexListProperty); }
            set { SetValue(PageIndexListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageIndexList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageIndexListProperty =
            DependencyProperty.Register("PageIndexList", typeof(List<int>), typeof(WritableComboBox), new PropertyMetadata(new List<int>()));

        public int MaxPageRange
        {
            get { return (int)GetValue(MaxPageRangeProperty); }
            set { SetValue(MaxPageRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxPageRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxPageRangeProperty =
            DependencyProperty.Register("MaxPageRange", typeof(int), typeof(WritableComboBox), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnMaxPageRangeChanged)));

        private static void OnMaxPageRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int value = Convert.ToInt32(e.NewValue);
            if (value > 0)
            {
                (d as WritableComboBox).UpDataPagesInRange();
            }
        }

        private void UpDataPagesInRange()
        {
            if (writableComboBox.SelectedItem == null)
            {
                return;
            }
            if (writableComboBox.SelectedItem as ComboBoxItem == null)
            {
                return;
            }
            if ((writableComboBox.SelectedItem as ComboBoxItem).Tag != null)
            {
                switch ((writableComboBox.SelectedItem as ComboBoxItem).Tag.ToString())
                {
                    case "AllPage":
                        if (CommonHelper.GetPagesInRange(ref pageIndexList, "1-" + MaxPageRange, MaxPageRange, new char[] { ',' }, new char[] { '-' }))
                        {
                            PageIndexList = pageIndexList;
                            Text = "1-" + MaxPageRange;
                        }
                        break;

                    case "OddPage":
                        {
                            string pageRange = "";
                            for (int i = 1; i <= MaxPageRange; i++)
                            {
                                if (i % 2 != 0 || MaxPageRange == 1)
                                {
                                    if (string.IsNullOrEmpty(pageRange))
                                    {
                                        pageRange = i.ToString();
                                    }
                                    else
                                    {
                                        pageRange += "," + i;
                                    }
                                }
                            }
                            if (CommonHelper.GetPagesInRange(ref pageIndexList, pageRange, MaxPageRange, new char[] { ',' }, new char[] { '-' }))
                            {
                                PageIndexList = pageIndexList;
                                Text = pageRange;
                            }
                            break;
                        }
                    case "EvenPage":
                        {
                            string pageRange = "";
                            for (int i = 1; i <= MaxPageRange; i++)
                            {
                                if (i % 2 == 0 || MaxPageRange == 1)
                                {
                                    if (string.IsNullOrEmpty(pageRange))
                                    {
                                        pageRange = i.ToString();
                                    }
                                    else
                                    {
                                        pageRange += "," + i;
                                    }
                                }
                            }
                            if (CommonHelper.GetPagesInRange(ref pageIndexList, pageRange, MaxPageRange, new char[] { ',' }, new char[] { '-' }))
                            {
                                PageIndexList = pageIndexList;
                                Text = pageRange;
                            }
                            break;
                        }
                    case "CustomPage":
                        break;

                    default:
                        break;
                }
            }
        }

        public ItemCollection Items
        {
            get { return (ItemCollection)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =

    DependencyProperty.Register("Items", typeof(ItemCollection), typeof(WritableComboBox), new PropertyMetadata());

        /// <summary>
        /// 把子控件的事件传递出去，方便绑定
        /// </summary>
        public event RoutedEventHandler SelectionChanged;

        /// <summary>
        /// 把子控件的事件传递出去，方便绑定
        /// </summary>
        public event RoutedEventHandler TextChanged;

        public WritableComboBox()
        {
            InitializeComponent();

            this.Items = this.writableComboBox.Items;
        }

        private void writableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.writableComboBox.SelectedIndex == this.writableComboBox.Items.Count - 1)
            {
                if (this.writableComboBox.ActualWidth == 0) { this.writableTextBox.Width = 210; this.writableTextBox.Visibility = Visibility.Visible; return; }
                this.writableTextBox.Width = this.writableComboBox.ActualWidth - 28;
                Trace.WriteLine(this.writableComboBox.ActualWidth);
                this.writableTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                if (this.writableTextBox != null)
                {
                    ErrorBorder.Visibility = Visibility.Collapsed;
                    this.writableTextBox.Visibility = Visibility.Hidden;
                }
            }

            if (this.writableComboBox.Items.Count == 5)
            {
                if (this.writableComboBox.SelectedIndex == 1)
                { IsCurrentPage = true; }
                else
                {
                    IsCurrentPage = false;
                }
            }
            this.SelectedIndex = this.writableComboBox.SelectedIndex.ToString();

            if (writableComboBox.SelectedItem == null)
            {
                return;
            }

            if (writableComboBox.SelectedItem as ComboBoxItem == null)
            {
                return;
            }

            if ((writableComboBox.SelectedItem as ComboBoxItem).Tag != null)
            {
                PopTipPageRange.IsOpen = false;
                switch ((writableComboBox.SelectedItem as ComboBoxItem).Tag.ToString())
                {
                    case "AllPage":
                        if (CommonHelper.GetPagesInRange(ref pageIndexList, "1-" + MaxPageRange, MaxPageRange, new char[] { ',' }, new char[] { '-' }))
                        {
                            PageIndexList = pageIndexList;
                            Text = "1-" + MaxPageRange;
                        }
                        break;

                    case "OddPage":
                        {
                            string pageRange = "";
                            for (int i = 1; i <= MaxPageRange; i++)
                            {
                                if (i % 2 != 0 || MaxPageRange == 1)
                                {
                                    if (string.IsNullOrEmpty(pageRange))
                                    {
                                        pageRange = i.ToString();
                                    }
                                    else
                                    {
                                        pageRange += "," + i;
                                    }
                                }
                            }
                            if (CommonHelper.GetPagesInRange(ref pageIndexList, pageRange, MaxPageRange, new char[] { ',' }, new char[] { '-' }))
                            {
                                PageIndexList = pageIndexList;
                                Text = pageRange;
                            }
                            break;
                        }
                    case "EvenPage":
                        {
                            string pageRange = "";
                            for (int i = 1; i <= MaxPageRange; i++)
                            {
                                if (i % 2 == 0 || MaxPageRange == 1)
                                {
                                    if (string.IsNullOrEmpty(pageRange))
                                    {
                                        pageRange = i.ToString();
                                    }
                                    else
                                    {
                                        pageRange += "," + i;
                                    }
                                }
                            }
                            if (CommonHelper.GetPagesInRange(ref pageIndexList, pageRange, MaxPageRange, new char[] { ',' }, new char[] { '-' }))
                            {
                                PageIndexList = pageIndexList;
                                Text = pageRange;
                            }
                            break;
                        }
                    case "CustomPage":
                        writableTextBox.Text = string.Empty;
                        writableTextBox.Focus();
                        break;

                    default:
                        break;
                }
            }
            SelectionChanged?.Invoke(sender, e);
        }

        private void writableTextBox_TextChange(object sender, TextChangedEventArgs e)
        {
            if (this.writableComboBox.SelectedIndex == this.writableComboBox.Items.Count - 1)
            {
                Text = this.writableTextBox.Text;
            }
            else { Text = ""; }

            TextChanged?.Invoke(sender, e);
        }

        private void writableTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
            if (!IsloseFocus) { return; }
            if (CommonHelper.GetPagesInRange(ref pageIndexList, writableTextBox.Text, MaxPageRange, new char[] { ',' }, new char[] { '-' }))
            {
                PageIndexList = pageIndexList;
                Text = writableTextBox.Text;
               // PopTipPageRange.IsOpen = false;
            }
            else
            {
                //PopTipPageRange.IsOpen = true;
                //TxtError.Text = string.Format($"{LanguageHelper.CommonManager.GetString("PageEdit_SplitErrorTile")}{MaxPageRange}", $"{LanguageHelper.CommonManager.GetString("PageEdit_SplitErrorContent")}");
                // TxtError.Text = LanguageHelper.CommonManager.GetString("Main_PageRangedWarning");
                //MessageBox.Show($"{LanguageHelper.CommonManager.GetString("PageEdit_SplitErrorTile")}{MaxPageRange}", LanguageHelper.CommonManager.GetString("PageEdit_SplitErrorContent"), MessageBoxButton.OK, MessageBoxImage.Information);
                ErrorBorder.Visibility = Visibility.Visible;
                writableTextBox.Text = "";
            }
        }

        private void writableTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                writableComboBox.Focus();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //230621 重复调用 ，OnMaxPageRangeChanged 有调用此方法
            //注释 是为了  选中页面拆分，需要选自定义，并且显示选中的页码
            //UpDataPagesInRange();
            AllPageItem.Content = LanguageHelper.CommonManager.GetString("Option_AllPage");
            OddPageItem.Content = LanguageHelper.CommonManager.GetString("Option_OddPages");
            EvenPageItem.Content = LanguageHelper.CommonManager.GetString("Option_EvenPages");
            CustomPageItem.Content = LanguageHelper.CommonManager.GetString("Option_CustomPages");
            writableTextBox.Tag = LanguageHelper.DocEditorManager.GetString("Holder_Custom");
            writableTextBox.Width = this.Width - 25;
        }

        private void writableTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9,-]+").IsMatch(e.Text);
        }

        private void writableComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            IsloseFocus = true;
        }

        private void writableComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            IsloseFocus = false;
        }

        private void writableTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
        }

        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is WritableComboBox comboBox)
            {
                if (comboBox.IsEnabled)
                {
                    //还原透明度状态
                    this.Opacity = 1;
                    writableTextBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        writableTextBox.Text = Text;
                        writableTextBox.Focus();
                        TextBlock textBlock = CommonHelper.PageEditHelper.FindVisualChild<TextBlock>(writableTextBox);
                        if (textBlock != null)
                        {
                            // textBlock.Text = LanguageHelper.CommonManager.GetString("Main_PageRange_EG");
                        }
                    }));
                }
                else
                {
                    //置灰显示
                    this.Opacity = 0.6;
                    ErrorBorder.Visibility = Visibility.Collapsed;
                    if (writableComboBox.SelectedItem == null)
                    {
                        return;
                    }
                    if ((writableComboBox.SelectedItem as ComboBoxItem).Tag != null)
                    {
                        switch ((writableComboBox.SelectedItem as ComboBoxItem).Tag.ToString())
                        {
                            case "CustomPage":
                                writableTextBox.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    writableTextBox.Text = string.Empty;
                                    TextBlock textBlock = CommonHelper.PageEditHelper.FindVisualChild<TextBlock>(writableTextBox);
                                    if (textBlock != null)
                                    {
                                        textBlock.Text = null;
                                    }
                                }));
                                break;
                        }
                    }
                }
            }
        }
    }
}