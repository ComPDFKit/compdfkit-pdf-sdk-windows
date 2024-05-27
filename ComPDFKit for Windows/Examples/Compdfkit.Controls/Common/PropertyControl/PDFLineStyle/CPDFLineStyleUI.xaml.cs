using ComPDFKit.Controls.Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DashStyle = System.Windows.Media.DashStyle;

namespace ComPDFKit.Controls.Common
{
    public partial class CPDFLineStyleUI : UserControl, INotifyPropertyChanged
    {
        private CPDFDashData _dashStyle = new CPDFDashData();
        public CPDFDashData DashStyle
        {
            get => _dashStyle;
            set => _dashStyle = value;
        }

        private int _dashSpacing = 1;
        public int DashSpacing
        {
            get => _dashSpacing;
            set
            {
                _dashSpacing = value;
                DashStyle.DashSpacing = _dashSpacing;
                OnDashSpacingChanged();
            }
        }

        private bool _isSolid = true;
        public bool IsSolid { 
            get => _isSolid;
            set {
                _isSolid = value;
                DashStyle.IsSolid = value;
            }
        }

        public event EventHandler LineStyleChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public CPDFLineStyleUI()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton.Tag.ToString() == "Solid")
            {
                IsSolid = true;
            }
            else
            {
                IsSolid = false;
                DashStyle.DashSpacing = DashSpacing;
            }
            LineStyleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void OnDashSpacingChanged()
        {
            LineStyleChanged?.Invoke(this, EventArgs.Empty);
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
