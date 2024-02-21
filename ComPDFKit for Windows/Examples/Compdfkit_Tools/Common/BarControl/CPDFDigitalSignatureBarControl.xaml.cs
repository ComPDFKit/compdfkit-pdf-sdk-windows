using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Compdfkit_Tools.Helper;

namespace Compdfkit_Tools.Common
{
    /// <summary>
    /// Interaction logic for CPDFDigitalSignatureBarControl.xaml
    /// </summary>
    public partial class CPDFDigitalSignatureBarControl : UserControl
    {
        private string addSig = LanguageHelper.ToolBarManager.GetString("Button_NewSig");
        private string verifySig = LanguageHelper.ToolBarManager.GetString("Button_VerifySig");
        private bool isFirstLoad = true;
        private int counter = 0;

        public event EventHandler<DigitalSignatureAction> DigitalSignatureActionChanged;

        public enum DigitalSignatureAction
        {
            AddSignatureField,
            Signing,
            VerifySignature
        }

        #region data
        Dictionary<string, string> ButtonDict;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Create Default UI


        private void ClearToolState(UIElement sender)
        {
            foreach (UIElement child in DigitalSignBarGrid.Children)
            {
                if (child is ToggleButton toggle && ((child as ToggleButton) != sender))
                {
                    toggle.IsChecked = false;
                }
            }
        }
        
        public void ClearAllToolState()
        {
            foreach (UIElement child in DigitalSignBarGrid.Children)
            {
                if (child is ToggleButton toggle)
                {
                    toggle.IsChecked = false;
                }
            }
        }

        public CPDFDigitalSignatureBarControl()
        {
            ButtonDict = new Dictionary<string, string>
            {
                {addSig,"M12.0663 0.731427C11.8457 0.510843 11.4792 0.543891 11.3017 0.800376L8.50221 4.84406C8.47541 4.88277 8.43581 4.91078 8.39039 4.92317L4.70212 5.92906C4.62329 5.95056 4.56548 6.01698 4.55485 6.09799C4.49741 6.53554 4.28835 7.98652 3.86452 9.50018C3.50509 10.7839 2.41361 13.2636 1.80714 14.6016C1.43791 14.4321 0.986655 14.4994 0.682542 14.8035C0.292017 15.194 0.292017 15.8272 0.682542 16.2177C1.07307 16.6082 1.70623 16.6082 2.09676 16.2177C2.40731 15.9072 2.4709 15.4432 2.28754 15.0698C3.47427 14.5032 5.62247 13.5334 7.40006 13.0357C9.40855 12.4733 10.4209 12.3578 10.7798 12.3344C10.8716 12.3284 10.9526 12.2662 10.9768 12.1775L11.9771 8.50985C11.9895 8.46442 12.0175 8.42483 12.0562 8.39803L16.0999 5.59856C16.3563 5.42099 16.3894 5.05449 16.1688 4.83391L12.0663 0.731427ZM7.84417 7.82298C7.79817 7.77698 7.72119 7.78646 7.68771 7.84224L5.83809 10.925C5.78453 11.0142 5.88602 11.1157 5.97529 11.0622L9.058 9.21253C9.11378 9.17905 9.12326 9.10207 9.07726 9.05607L7.84417 7.82298ZM15.527 15.5935C15.4522 15.5983 15.3766 15.6007 15.3006 15.6007C15.2254 15.6007 15.1507 15.5983 15.0767 15.5937L13.5309 18.2711C13.4525 18.4069 13.2553 18.4037 13.1813 18.2653L12.5771 17.1345C12.5412 17.0673 12.4702 17.0263 12.3941 17.0288L11.1126 17.0709C10.9558 17.0761 10.8544 16.9069 10.9329 16.7711L12.4536 14.1371C12.0425 13.5634 11.8006 12.8603 11.8006 12.1007C11.8006 10.1677 13.3676 8.60071 15.3006 8.60071C17.2336 8.60071 18.8006 10.1677 18.8006 12.1007C18.8006 12.8594 18.5592 13.5616 18.149 14.1349L19.671 16.7711C19.7494 16.9069 19.648 17.0761 19.4912 17.0709L18.2098 17.0288C18.1336 17.0263 18.0627 17.0673 18.0268 17.1345L17.4225 18.2653C17.3486 18.4037 17.1514 18.4069 17.0729 18.2711L15.527 15.5935Z"},
                {verifySig, "M12.0663 0.846661C11.8457 0.626078 11.4792 0.659126 11.3017 0.91561L8.50221 4.95929C8.47541 4.998 8.43581 5.02601 8.39039 5.0384L4.70212 6.0443C4.62329 6.06579 4.56548 6.13221 4.55485 6.21322C4.49741 6.65078 4.28835 8.10176 3.86452 9.61542C3.50509 10.8991 2.41361 13.3788 1.80714 14.7169C1.43791 14.5473 0.986655 14.6146 0.682542 14.9187C0.292017 15.3093 0.292017 15.9424 0.682542 16.333C1.07307 16.7235 1.70623 16.7235 2.09676 16.333C2.40731 16.0224 2.4709 15.5584 2.28754 15.185C3.47427 14.6185 5.62247 13.6487 7.40006 13.151C9.40855 12.5886 10.4209 12.473 10.7798 12.4496C10.8716 12.4436 10.9526 12.3815 10.9768 12.2927L11.9771 8.62508C11.9895 8.57966 12.0175 8.54006 12.0562 8.51327L16.0999 5.71379C16.3563 5.53623 16.3894 5.16973 16.1688 4.94914L12.0663 0.846661ZM7.84417 7.93821C7.79817 7.89221 7.72119 7.90169 7.68771 7.95748L5.83809 11.0402C5.78453 11.1295 5.88602 11.2309 5.97529 11.1774L9.058 9.32776C9.11378 9.29429 9.12326 9.2173 9.07726 9.1713L7.84417 7.93821ZM11.8814 12.374C11.8814 12.1507 12.0293 11.9545 12.244 11.8932L15.3033 11.0191C15.3931 10.9935 15.4883 10.9935 15.578 11.0191L18.6374 11.8932C18.852 11.9545 19 12.1507 19 12.374V15.9132C19 16.0119 18.9713 16.1077 18.9146 16.1886C18.5137 16.7603 16.7809 19.1154 15.4407 19.1154C14.1004 19.1154 12.3677 16.7603 11.9667 16.1886C11.91 16.1077 11.8814 16.0119 11.8814 15.9132V12.374ZM14.4114 15.9256L14.4114 15.9256L15.1305 16.6447L17.4549 14.3203L16.7358 13.6012L15.1305 15.2065L14.1993 14.2754L13.4802 14.9945L14.4114 15.9256Z"},
            };
            InitializeComponent();
        }

        /// <summary>
        /// When the "Add Signatrue Field" button is pressed, 
        /// it triggers an event: entering the state of creating a signature field. 
        /// Clicking again will return to the state where signing is possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (isFirstLoad)
            {
                foreach (KeyValuePair<string, string> data in ButtonDict)
                {
                    string Path = data.Value;
                    string name = data.Key;

                    Geometry annotationGeometry = Geometry.Parse(Path);
                    Path path = new Path
                    {
                        Width = 20,
                        Height = 20,
                        Data = annotationGeometry,
                        Fill = new SolidColorBrush(Color.FromRgb(0x43, 0x47, 0x4D))
                    };
                       CreateButtonForPath(path, name);
                }
                isFirstLoad = false;
            }
        } 

        private void CreateButtonForPath(Path path, String name)
        {
            StackPanel stackPanel = new StackPanel();
            TextBlock textBlock = new TextBlock();
            if (path != null)
            {
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                stackPanel.Children.Add(path);
            }
            if (!string.IsNullOrEmpty(name))
            {
                textBlock.Text = name;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Margin = new Thickness(8, 0, 0, 0);
                textBlock.FontSize = 12;
                stackPanel.Children.Add(textBlock);
            }

            Style style = (Style)FindResource("RoundMarginToggleButtonStyle");
            ToggleButton button = new ToggleButton();
            button.BorderThickness = new Thickness(0);
            button.Padding = new Thickness(10, 5, 10, 5);
            button.Tag = name;
            button.ToolTip = name;
            button.Style = style;
            button.Content = stackPanel;
            if (name == addSig)
            {
                button.Click += AddDigitalSignatureBtn_Click;
            }
            else if (name == verifySig)
            {
                button.Click += VerifySignatureBtn_Click;
            }
            DigitalSignBarGrid.ColumnDefinitions.Add(new ColumnDefinition());
            DigitalSignBarGrid.Width += 180;
            Grid.SetColumn(button, counter++);
            DigitalSignBarGrid.Children.Add(button);
        }

        private void AddDigitalSignatureBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearToolState(sender as ToggleButton);
            if ((bool)(sender as ToggleButton).IsChecked)
            {
                DigitalSignatureActionChanged?.Invoke(sender, DigitalSignatureAction.AddSignatureField);
            }
            else
            {
                DigitalSignatureActionChanged?.Invoke(sender, DigitalSignatureAction.Signing);
            }
        }

        private void VerifySignatureBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)(sender as ToggleButton).IsChecked)
            {
                DigitalSignatureActionChanged?.Invoke(sender, DigitalSignatureAction.VerifySignature);
            }
        }

        #endregion
    }
}
