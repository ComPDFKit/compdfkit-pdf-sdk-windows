using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace ComPDFKit.Tool
{
    /// <summary>
    /// Help class for calculating the alignment position of the rectangle
    /// </summary>
    public class AlignmentsHelp
    {
        /// <summary>
        /// Set the source rectangle to be left-aligned in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Origin rectangle
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// X Y direction distance required for alignment of the source rectangle
        /// </returns>
        public static Point SetAlignLeft(Rect src, Rect dst)
        {
            Point movePoint = new Point(dst.Left - src.Left, 0);
            return movePoint;
        }

        /// <summary>
        /// Set the source rectangle to be horizontally centered in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Origin rectangle
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// X Y direction distance required for alignment of the source rectangle
        /// </returns>
        public static Point SetAlignHorizonCenter (Rect src, Rect dst)
        {
            Point movePoint = new Point((dst.Left + dst.Right - src.Left - src.Right) / 2, 0);
            return movePoint;
        }

        /// <summary>
        /// Right-align the source rectangle in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Origin rectangle
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// X Y direction distance required for alignment of the source rectangle
        /// </returns>
        public static Point SetAlignRight(Rect src, Rect dst)
        {
            Point movePoint = new Point(dst.Right - src.Width - src.Left, 0);
            return movePoint;
        }

        /// <summary>
        /// Set the source rectangle to be top-aligned in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Origin rectangle
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// X Y direction distance required for alignment of the source rectangle
        /// </returns>
        public static Point SetAlignTop(Rect src, Rect dst)
        {
            Point movePoint = new Point(0, dst.Top - src.Top);
            return movePoint;
        }

        /// <summary>
        /// Set the source rectangle to be vertically centered in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Origin rectangle
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// X Y direction distance required for alignment of the source rectangle
        /// </returns>
        public static Point SetAlignVerticalCenter(Rect src, Rect dst)
        {
            Point movePoint = new Point(0, (dst.Bottom + dst.Top - src.Top - src.Bottom) / 2);
            return movePoint;
        }

        /// <summary>
        /// Set the source rectangle to be horizontally and vertically centered in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Origin rectangle
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// X Y direction distance required for alignment of the source rectangle
        /// </returns>
        public static Point SetAlignHorizonVerticalCenter(Rect src, Rect dst)
        {
            Point movePoint = new Point((dst.Left + dst.Right - src.Left - src.Right) / 2, (dst.Bottom + dst.Top - src.Top - src.Bottom) / 2);
            return movePoint;
        }

        /// <summary>
        /// Set the source rectangle to be bottom-aligned in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Origin rectangle
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// X Y direction distance required for alignment of the source rectangle
        /// </returns>
        public static Point SetAlignBottom(Rect src, Rect dst)
        {
            Point movePoint = new Point(0, dst.Bottom - src.Height - src.Top);
            return movePoint;
        }

        /// <summary>
        /// Set the source rectangle to be horizontally distributed and aligned in the target rectangle
        /// </summary>
        /// <param name="src">
        /// Array of source rectangles needed
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// Dictionary of XY direction distance required for alignment of each source rectangle
        /// </returns>
        public static Dictionary<Rect, Point> SetDistributeHorizontal(List<Rect> src, Rect dst)
        {
            Dictionary<Rect, Point> dictionary = new Dictionary<Rect, Point>();
            List<double> Leftlist = new List<double>();

            // Sort the data according to the leftmost position of each rectangle, not the array order
            foreach (Rect srcRect in src)
            {
                Leftlist.Add(srcRect.Left + srcRect.Width / 2);
            }
            double[] datalist = Leftlist.ToArray();
            Sort(datalist, 0, Leftlist.Count - 1);

            double startX = dst.Left;
            double endX = dst.Right;
            double interval = (endX - startX) / Leftlist.Count;
            for (int i = 0; i < datalist.Count(); i++)
            {
                int index = Leftlist.IndexOf(datalist[i]);
                Point movePoint = new Point(startX + i * interval - src[index].Left - src[index].Width / 2, 0);
                dictionary.Add(src[index], movePoint);
            }
            return dictionary;
        }

        /// <summary>
        /// Vertically distribute the source rectangles within the target rectangle (sorting based on the leftmost position of each rectangle, not the array order)
        /// </summary>
        /// <param name="src">
        /// Array of source rectangles needed
        /// </param>
        /// <param name="dst">
        /// Target rectangle
        /// </param>
        /// <returns>
        /// Dictionary of XY direction distance required for alignment of each source rectangle
        /// </returns>
        public static Dictionary<Rect, Point> SetDistributeVertical(List<Rect> src, Rect dst)
        {
            Dictionary<Rect, Point> dictionary = new Dictionary<Rect, Point>();
            List<double> Leftlist = new List<double>();

            // Sort the data according to the leftmost position of each rectangle, not the array order
            foreach (Rect srcRect in src)
            {
                Leftlist.Add(srcRect.Left + srcRect.Width / 2);
            }
            double[] datalist = Leftlist.ToArray();
            Sort(datalist, 0, Leftlist.Count - 1);

            double startY = dst.Top;
            double endY = dst.Bottom;
            double interval = (endY - startY) / Leftlist.Count;
            for (int i = 0; i < datalist.Count(); i++)
            {
                int index = Leftlist.IndexOf(datalist[i]);
                Point movePoint = new Point(0, startY + i * interval - src[index].Top - src[index].Height / 2);
                dictionary.Add(src[index], movePoint);
            }
            return dictionary;
        }

        #region Quick sort

        private static int SortUnit(double[] array, int low, int high)
        {
            double key = array[low];
            while (low < high)
            {
                // Search backward for values smaller than the key
                while (array[high] >= key && high > low)
                    --high;
                // Put the value smaller than key on the left
                array[low] = array[high];
                // Search forward for values larger than the key
                while (array[low] <= key && high > low)
                    ++low;
                // Put the value larger than key on the right
                array[high] = array[low];
            }
            // Left is smaller than key, right is larger than key.
            // Put the key in the current position of the cursor
            // At this point, 'low' equals 'high
            array[low] = key;
            return high;
        }

        private static void Sort(double[] array, int low, int high)
        {
            if (low >= high)
                return;
            // Finish a single unit sort
            int index = SortUnit(array, low, high);
            // Sort the left unit
            Sort(array, low, index - 1);
            // Sort the right unit
            Sort(array, index + 1, high);
        }

        #endregion
    }
}
