using ComPDFKit.PDFAnnotation;
using ComPDFKit.Tool;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFAnnoationListUI : UserControl
    {
        public class BindAnnotationResult : INotifyPropertyChanged
        {
            public int PageIndex { get; set; }

            public int AnnotIndex { get => annotationData.AnnotIndex; }

            public string CreateDate
            {
                get
                {
                    if (Regex.IsMatch(annotationData.CreateTime, "(?<=D\\:)[0-9]+(?=[\\+\\-])"))
                    {
                        string dateStr = Regex.Match(annotationData.CreateTime, "(?<=D\\:)[0-9]+(?=[\\+\\-])").Value;
                        return (dateStr.Substring(0, 4) + "-" + dateStr.Substring(4, 2) + "-" + dateStr.Substring(6, 2) + ", " + dateStr.Substring(8, 2) + ":" +
                            dateStr.Substring(10, 2));
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
            }

            public string Note
            {
                get => annotationData.Content;
            }

            public C_ANNOTATION_TYPE CurrentAnnotationType
            {
                get => annotationData.CurrentType;
            }

            public AnnotParam annotationData { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal class AnnotationBindData
        {
            public BindAnnotationResult BindProperty { get; set; }
            public AnnotationBindData()
            {
                BindProperty = new BindAnnotationResult();
            }
            public int ShowPageIndex { get { return BindProperty.PageIndex + 1; } set { BindProperty.PageIndex = value; } }
        }

        private ObservableCollection<AnnotationBindData> annotationList = new ObservableCollection<AnnotationBindData>();

        public event EventHandler<object> AnnotationSelectionChanged;

        public event EventHandler<Dictionary<int, List<int>>> DeleteItemHandler;

        private ContextMenu popContextMenu;
        private bool enableSelectEvent = true;

        public CPDFAnnoationListUI()
        {
            InitializeComponent();
            ICollectionView groupView = CollectionViewSource.GetDefaultView(annotationList);
            groupView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(AnnotationBindData.ShowPageIndex)));
            popContextMenu = new ContextMenu();
            MenuItem deleteMenu = new MenuItem();
            deleteMenu.Header = LanguageHelper.BotaManager.GetString("Menu_Delete");
            deleteMenu.Click -= DeleteMenu_Click;
            deleteMenu.Click += DeleteMenu_Click;
            popContextMenu.Items.Add(deleteMenu);
            MenuItem deleteAllMenu = new MenuItem();
            deleteAllMenu.Header = LanguageHelper.BotaManager.GetString("Menu_DeleteAll");
            deleteAllMenu.Click -= DeleteAllMenu_Click;
            deleteAllMenu.Click += DeleteAllMenu_Click;
            popContextMenu.Items.Add(deleteAllMenu);
        }

        private void DeleteAllMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<int, List<int>> delDict = new Dictionary<int, List<int>>();

                foreach (AnnotationBindData bindData in annotationList)
                {
                    if (delDict.ContainsKey(bindData.BindProperty.PageIndex) == false)
                    {
                        delDict[bindData.BindProperty.PageIndex] = new List<int>();
                    }
                    delDict[bindData.BindProperty.PageIndex].Add(bindData.BindProperty.AnnotIndex);
                }

                if (delDict.Count > 0)
                {
                    DeleteItemHandler?.Invoke(this, delDict);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AnnotationList != null && AnnotationList.SelectedIndex >= 0)
                {
                    AnnotationBindData bindData = annotationList[AnnotationList.SelectedIndex];

                    Dictionary<int, List<int>> delDict = new Dictionary<int, List<int>>();
                    delDict[bindData.BindProperty.PageIndex] = new List<int>()
                    {
                        bindData.BindProperty.AnnotIndex
                    };
                    DeleteItemHandler?.Invoke(this, delDict);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SetAnnotationList(List<BindAnnotationResult> results)
        {
            annotationList.Clear();
            AnnotationList.ContextMenu = null;
            if (results == null || results.Count == 0)
            {
                AnnotationList.ItemsSource = null;
                NoContentText.Visibility = Visibility.Visible;
                return;
            }

            foreach (BindAnnotationResult item in results)
            {
                annotationList.Add(new AnnotationBindData()
                {
                    BindProperty = item
                });
            }
            AnnotationList.ItemsSource = annotationList;
            if (annotationList.Count > 0)
            {
                AnnotationList.ContextMenu = popContextMenu;
            }
            AnnotationList.Visibility = Visibility.Visible;
            NoContentText.Visibility = Visibility.Collapsed;
        }

        private void AnnotationListControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (enableSelectEvent)
            {
                try
                {
                    AnnotationSelectionChanged?.Invoke(this, (e.AddedItems[0] as AnnotationBindData).BindProperty);
                }
                catch { }
            }
        }
         
        public void CancelSelected()
        {
            AnnotationList.SelectedIndex = -1;
        }

        public void SelectAnnotationChanged(int annotationIndex = -1)
        {

            AnnotationList.SelectedIndex = annotationIndex;
        }

        public void SelectAnnotationChanged(int pageIIndex, int annotIndex)
        {
            if (annotationList != null && annotationList.Count > 0)
            {
                for (int i = 0; i < annotationList.Count; i++)
                {
                    AnnotationBindData data = annotationList[i];
                    if (data.BindProperty.PageIndex == pageIIndex && data.BindProperty.AnnotIndex == annotIndex)
                    {
                        enableSelectEvent = false;
                        AnnotationList.SelectedIndex = i;
                        enableSelectEvent = true;
                        break;
                    }
                }
            }
        }

        private void AnnotationList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                MenuItem checkMenu = popContextMenu.Items[0] as MenuItem;
                if (checkMenu != null)
                {
                    checkMenu.IsEnabled = true;
                }
                if (AnnotationList != null && AnnotationList.SelectedIndex == -1)
                {
                    checkMenu.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void AnnotationList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CancelSelected();
        }
    }
}
