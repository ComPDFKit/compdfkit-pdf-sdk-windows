using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Compdfkit_Tools.PDFControl
{
    public partial class SetEncryptionControl : UserControl, INotifyPropertyChanged
    {
        private bool _isUserPasswordEnabled;
        public bool IsUserPasswordEnabled
        {
            get => _isUserPasswordEnabled;
            set
            {
                _isUserPasswordEnabled = value;
                OnPropertyChanged();
                UpdateValidation();
            }
        }
        private bool _isOwnerPasswordEnabled;

        public bool IsOwnerPasswordEnabled
        {
            get => _isOwnerPasswordEnabled;
            set
            {
                _isOwnerPasswordEnabled = value;
                OnPropertyChanged();
                UpdateValidation();
            }
        }
        
        private string _userPassword;
        public string UserPassword
        {
            get => _userPassword;
            set
            {
                _userPassword = value;
                OnPropertyChanged();
                UpdateValidation();
            }
        }
        
        private string _ownerPassword;
        public string OwnerPassword
        {
            get => _ownerPassword;
            set
            {
                _ownerPassword = value;
                OnPropertyChanged();
                UpdateValidation();
            }
        }
        
        private bool _isAllowPrint = true;
        public bool IsAllowPrint
        {
            get => _isAllowPrint;
            set
            {
                _isAllowPrint = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isAllowCopy = true;
        public bool IsAllowCopy
        {
            get => _isAllowCopy;
            set
            {
                _isAllowCopy = value;
                OnPropertyChanged();
            }
        }
        
        private int _cryptographicLevel;
        public int CryptographicLevel
        {
            get => _cryptographicLevel;
            set
            {
                _cryptographicLevel = value;
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IsEnsureProperty =
            DependencyProperty.Register(nameof(IsSettingValid), typeof(bool), typeof(SetEncryptionControl), new PropertyMetadata(false));

        public bool IsSettingValid
        {
            get => (bool)GetValue(IsEnsureProperty);
            private set => SetValue(IsEnsureProperty, value);
        }

        
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public SetEncryptionControl()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        private void UpdateValidation()
        {
            if (!IsOwnerPasswordEnabled && !IsUserPasswordEnabled)
            {
                IsSettingValid = false;
                return;
            }

            bool validUserOption = !IsUserPasswordEnabled || !string.IsNullOrEmpty(UserPassword);
            bool validOwnerOption = !IsOwnerPasswordEnabled || !string.IsNullOrEmpty(OwnerPassword);

            IsSettingValid = validUserOption && validOwnerOption;
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
    }
}