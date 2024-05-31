using System;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ComPDFKit.NativeMethod;

namespace ComPDFKit.Controls.PDFControl
{
    public partial class DeviceSerialControl : Window
    {
        public DeviceSerialControl()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Icon = new BitmapImage(new Uri("pack://application:,,,/ComPDFKit.Controls;component/ComPDFKit_Logo.ico"));
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				BoardText.Text = CPDFSDKVerifier.GetDeviceId();
			}
			catch (Exception)
			{
				
			}
		}

		private void CopyBtn_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (BoardText != null && BoardText.Text != string.Empty)
				{
					Clipboard.SetText(BoardText.Text);
					CopyTips.Visibility = Visibility.Visible;
					DispatcherTimer dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Tick -= this.StartTimer_Tick;
                    dispatcherTimer.Tick += this.StartTimer_Tick;
					dispatcherTimer.IsEnabled = false;
					dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
					dispatcherTimer.Start();
				}
			}
			catch (Exception)
			{
			}
		}

		private void StartTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				CopyTips.Visibility = Visibility.Collapsed;
				DispatcherTimer dispatcherTimer = sender as DispatcherTimer;
				if (dispatcherTimer != null)
				{
					dispatcherTimer.Stop();
				}
			}
			catch (Exception)
			{
			}
		}
    }
}