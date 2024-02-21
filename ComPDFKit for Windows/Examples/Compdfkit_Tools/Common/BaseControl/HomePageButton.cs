using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.Common
{
    public class HomePageButton : Button
    {
        public static readonly DependencyProperty IsToggledProperty =
    DependencyProperty.Register("IsToggled", typeof(bool), typeof(HomePageButton), new PropertyMetadata(false));

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }

        public static readonly RoutedEvent ToggledEvent = 
            EventManager.RegisterRoutedEvent("Toggled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HomePageButton));
        public event RoutedEventHandler Toggled
        {
            add { AddHandler(ToggledEvent, value); }
            remove { RemoveHandler(ToggledEvent, value); }
        }

        public HomePageButton()
        {
            Click += HomePageButton_Click; ;
        }

        public class QueryLockEventArgs : EventArgs
        {
            public bool CanLock { get; set; }
        }

        public delegate bool QueryLockHandler(object sender, QueryLockEventArgs e);

        public event QueryLockHandler QueryLock;

        private void HomePageButton_Click(object sender, RoutedEventArgs e)
        {
            if (QueryLock?.Invoke(this, new QueryLockEventArgs()) == false)
            {
                IsToggled = !IsToggled;
                RaiseToggledEvent();
            }
        }

        private void RaiseToggledEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ToggledEvent);
            RaiseEvent(newEventArgs);
        }
    }
}
