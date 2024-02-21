using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Compdfkit_Tools.Helper
{
    public interface IHasFileHistory
    {
        string FilePath { get; set; }
    }

    [Serializable]
    public class PDFFileInfo : IHasFileHistory
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;    
        public string FileName { get; set; } = string.Empty;
        public string OpenDate { get; set; } = string.Empty;
    }

    public class FileHistoryHelper<T> : INotifyPropertyChanged where T: class, IHasFileHistory
    {

        private ObservableCollection<T> _history;

        public ObservableCollection<T> History
        {
            get => _history;
            set
            {
                _history = value;
                UpdateProper(ref _history, value);
            }
        }

        private static FileHistoryHelper<T> instance;
        public static FileHistoryHelper<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FileHistoryHelper<T>();
                }
                return instance;
            }
        }

        public int DefaultHistoryCount { get; private set; } = int.MaxValue;
        public string DefaultFilePath { get; private set; } = string.Empty;

        private FileHistoryHelper()
        {
            History = new ObservableCollection<T>();
            DefaultFilePath = "History.xml";
            DefaultHistoryCount = 10;
        }

        public void AddHistory(T item)
        {
            if (item == null)
            {
                return;
            }
            T existingItem = History.FirstOrDefault(i => i.FilePath == item.FilePath);

            if (existingItem != null)
            {
                History.Remove(existingItem);
            }
            History.Insert(0, item);
        
            if (History.Count > DefaultHistoryCount)
            {
                History.RemoveAt(History.Count - 1);
            }
        }

        public void SaveHistory(string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = DefaultFilePath;
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<T>));
                    serializer.Serialize(fs, History);
                }
            }
            catch (Exception ex)
            {
                ClearHistory();
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        public void LoadHistory(string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = DefaultFilePath;
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<T>));
                    History = (ObservableCollection<T>)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                ClearHistory();
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        public void ClearHistory()
        {
            History.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void UpdateProper<T>(ref T properValue,
                            T newValue,
                            [CallerMemberName] string properName = "")
        {
            properValue = newValue;
            OnPropertyChanged(properName);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
