using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Common
{
    public partial class CPDFOpacityUI : UserControl, INotifyPropertyChanged
    {


        private int _opacityValue = 100;
        public int OpacityValue
        {
            get
            {
                return _opacityValue;
            }

            set
            {
                _opacityValue = value;
                DropDownNumberBoxControl?.SelectValueItem(OpacityValue);
                OnPropertyChanged(nameof(OpacityValue));
                OnOpacityChanged();
            }
        }

        public event EventHandler OpacityChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public CPDFOpacityUI()
        {
            InitializeComponent();
            this.DataContext = this;
            InitPresetNumberArray();
        }

        public void InitPresetNumberArray()
        {
            List<int> list = new List<int>();
            list.Add(25);
            list.Add(50);
            list.Add(75);
            list.Add(100);
            DropDownNumberBoxControl.InitPresetNumberArray(list);
        }

        private void OnOpacityChanged()
        {
            OpacityChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
    }
}
