using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ComPDFKit.Controls.PDFControlUI
{
    public partial class CPDFThumbnailUI : UserControl
    {
        /// <summary>
        /// Click to select the event in the thumbnail list
        /// </summary>
        public event EventHandler<int> SelectionChanged;

        /// <summary>
        /// Scroll state change event
        /// </summary>
        public event EventHandler<ScrollChangedEventArgs> ViewChanged;

        private bool lockGoToPage;

        /// <summary>
        /// A list of thumbnail results
        /// </summary>
        private List<ThumbnailItem> thumbResultList=new List<ThumbnailItem>();
        public CPDFThumbnailUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Select Result Change event
        /// </summary>
        private void ThumbListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!lockGoToPage)
            {
                SelectionChanged?.Invoke(this, ThumbListBox.SelectedIndex);
            }
        }

        /// <summary>
        /// Set the thumbnail list
        /// </summary>
        /// <param name="thumbList"></param>
        public void SetThumbResult(List<ThumbnailItem> thumbList)
        {
            thumbResultList?.Clear();
            ThumbListBox.ItemsSource = null;
            if (thumbList == null || thumbList.Count == 0)
            {
                return;
            }
            thumbResultList.AddRange(thumbList);
            ThumbListBox.ItemsSource = thumbResultList;
            ThumbListBox.UpdateLayout();
        }

        /// <summary>
        /// Content scrolling events
        /// </summary>
        private void ThumbListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ViewChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Determines whether the specified object is visible
        /// </summary>
        public bool IsItemVisible(ThumbnailItem checkItem)
        {
            if (ThumbListBox == null || thumbResultList == null || thumbResultList.Count == 0)
            {
                return false;
            }
            try
            {
                int index = thumbResultList.IndexOf(checkItem);
                if (index != -1)
                {
                    ListBoxItem itemContainer = (ListBoxItem)ThumbListBox.ItemContainerGenerator.ContainerFromIndex(index);
                    if (itemContainer==null || itemContainer.IsVisible == false || itemContainer.Visibility != Visibility.Visible)
                    {
                        return false;
                    }

                    GeneralTransform transform = itemContainer.TransformToAncestor(ThumbListBox);
                    Rect visualRect= transform.TransformBounds(new Rect(
                        0,
                        0,
                        itemContainer.ActualWidth,
                        itemContainer.ActualHeight
                        ));
                    Rect containerRect=new Rect(
                        0,
                        0,
                        ThumbListBox.ActualWidth,
                        ThumbListBox.ActualHeight);

                    containerRect.Intersect(visualRect);

                    if (containerRect.Width>1 && containerRect.Height>1)
                    {
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {

            }
           
            return false;
        }

        /// <summary>
        /// Select an object
        /// </summary>
        public void SelectItem(int checkIndex)
        {
            if(ThumbListBox!=null && thumbResultList != null && thumbResultList.Count>checkIndex && checkIndex>=0)
            {
                ThumbnailItem thumbItem= thumbResultList[checkIndex];
                if(IsItemVisible(thumbItem)==false)
                {
                    ThumbListBox.ScrollIntoView(thumbItem);
                }
                lockGoToPage = false;
                ThumbListBox.SelectedIndex = checkIndex;
            }
        }

        /// <summary>
        /// Select an object
        /// </summary>
        public void SelectItemWithoutGoTo(int checkIndex)
        {
            if (ThumbListBox != null && thumbResultList != null && thumbResultList.Count > checkIndex && checkIndex >= 0)
            {
                ThumbnailItem thumbItem = thumbResultList[checkIndex];
                if (IsItemVisible(thumbItem) == false)
                {
                    ThumbListBox.ScrollIntoView(thumbItem);
                }
                lockGoToPage = true;
                ThumbListBox.SelectedIndex = checkIndex;
                lockGoToPage = false;
            }
        }

    }


    /// <summary>
    /// Thumbnail object
    /// </summary>
    public class ThumbnailItem
    {
        /// <summary>
        /// Image width
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        ///Thumbnail width
        /// </summary>
        public int ThumbnailWidth { get; set; }

        /// <summary>
        /// Thumbnail height
        /// </summary>
        public int ThumbnailHeight { get; set; }

        /// <summary>
        /// Display page numbers
        /// </summary>
        public string ShowPageText
        {
            get
            {
                if (PageIndex >= 0)
                {
                    return (PageIndex + 1).ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Page index (starts with 0)
        /// </summary>
        public int PageIndex { get; set; } = -1;

        /// <summary>
        /// Thumbnails are like content
        /// </summary>
        public Image ImageData { get; set; } = new Image();
    }
}
