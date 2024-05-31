using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace ComPDFKit.Controls.Common
{
    public partial class CPDFThicknessUI : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler ThicknessChanged;

        private int _thickness = 1;
        public int Thickness
        {
            get
            {
                return _thickness;
            }
            set
            {
                if (_thickness != value)
                {
                    _thickness = value;
                    DropDownNumberBoxControl.SelectValueItem(_thickness);
                    OnPropertyChanged(nameof(Thickness));
                    OnThicknessChanged();
                }
            }
        }

        public CPDFThicknessUI()
        {
            InitializeComponent();
            this.DataContext = this;
            InitPresetNumberArray();
        }

        public void InitPresetNumberArray()
        {
            List<int> list = new List<int>();
            for(int i = 1; i <= 10;i++)
            {
                list.Add(i);
            }
            DropDownNumberBoxControl.InitPresetNumberArray(list);
        }

        private void OnThicknessChanged()
        {
            ThicknessChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
