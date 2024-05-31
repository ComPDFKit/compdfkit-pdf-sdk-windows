using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ComPDFKit.Controls.PDFControl
{ 
    public partial class AddCertificationControl : UserControl, INotifyPropertyChanged
    {
        public Dictionary<bool, CreateCertificationMode> getCreateCertificationMode = new Dictionary<bool, CreateCertificationMode>()
        {
            {true, CreateCertificationMode.AddExistedCertification},
            {false, CreateCertificationMode.AddCustomCertification}
        };

        private bool _addExistedCertification = true;
        public bool AddExistedCertification
        {
            get => _addExistedCertification;
            set => UpdateProper(ref _addExistedCertification, value);
        }

        private bool _addCustomCertification;
        public bool AddCustomCertification
        {
            get => _addCustomCertification;
            set => UpdateProper(ref _addCustomCertification, value);
        }

        public event EventHandler<CreateCertificationMode> ContinueEvent;
        public event EventHandler CancelEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        public AddCertificationControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            ContinueEvent?.Invoke(this, getCreateCertificationMode[AddExistedCertification]);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelEvent?.Invoke(this, EventArgs.Empty);
        }

        protected void UpdateProper<T>(ref T properValue,
                       T newValue,
                       [CallerMemberName] string properName = "")
        {
            if (object.Equals(properValue, newValue))
                return;

            properValue = newValue;
            OnPropertyChanged(properName);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}