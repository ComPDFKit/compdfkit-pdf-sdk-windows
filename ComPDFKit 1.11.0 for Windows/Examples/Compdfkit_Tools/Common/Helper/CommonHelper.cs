using ComPDFKit.PDFAnnotation;
using ComPDFKit.PDFDocument;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ComPDFKit.DigitalSign;
using ComPDFKit.PDFAnnotation.Form;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using System.Collections.ObjectModel;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using System.Drawing.Drawing2D;
using Matrix = System.Windows.Media.Matrix;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ComPDFKit.NativeMethod;

namespace Compdfkit_Tools.Helper
{
    public class SDKLicenseHelper
    {
        public string key = string.Empty;
        public string secret = string.Empty;
        public SDKLicenseHelper()
        {
            string sdkLicensePath = "license_key_windows.xml";
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(sdkLicensePath);

            XmlNode keyNode = xmlDocument.SelectSingleNode("/license/key");
            XmlNode secretNode = xmlDocument.SelectSingleNode("/license/secret");

            if (keyNode != null && secretNode != null)
            {
                key = keyNode.InnerText;
                secret = secretNode.InnerText;
            }
            else
            {
                Console.WriteLine("Key or secret element not found in the XML.");
            }
        }
    }

    public static class CommonHelper
    {
        public static bool IsImageCorrupted(string imagePath)
        {
            try
            {
                using (Bitmap bitmap = new Bitmap(imagePath))
                {
                    int width = bitmap.Width;
                    int height = bitmap.Height;
                }

                return false;
            }
            catch (Exception)
            {
                MessageBox.Show(LanguageHelper.CommonManager.GetString("Text_ImageCorrupted"), LanguageHelper.CommonManager.GetString("Button_OK"), MessageBoxButton.OK);
                return true;
            }
        }

        public static Bitmap ConvertTo32bppArgb(Bitmap source)
        {
            // Create a new Bitmap with 32bppArgb pixel format
            Bitmap newBitmap = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Create a Graphics object to draw the source image on the new Bitmap
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                // Set the interpolation mode and pixel offset mode for high-quality rendering
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                // Draw the source image on the new Bitmap
                g.DrawImage(source, new System.Drawing.Rectangle(0, 0, source.Width, source.Height));
            }
            return newBitmap;
        }

        public static byte[] ConvertBrushToByteArray(Brush brush)
        {
            if (brush is SolidColorBrush solidColorBrush)
            {
                Color color = solidColorBrush.Color;

                byte[] colorBytes = new byte[3];
                colorBytes[0] = color.R;
                colorBytes[1] = color.G;
                colorBytes[2] = color.B;

                return colorBytes;
            }
            else
            {
                throw new ArgumentException("The provided brush is not a SolidColorBrush.");
            }
        }



        public static int GetBitmapPointer(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            int bitmapPointer = hBitmap.ToInt32();

            return bitmapPointer;
        }

        public static string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        public static bool IsValidEmail(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }

        public static string GetExactDateFromString(string dateString)
        {
            DateTime dateTime = GetDateTimeFromString(dateString);
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static DateTime GetDateTimeFromString(string dateString)
        {
            int start = 0;
            for (int i = 0; i < dateString.Length; i++)
            {
                if (char.IsNumber(dateString[i]))
                {
                    start = i;
                    break;
                }
            }
            string date = dateString.Substring(start, 14);
            string year = date.Substring(0, 4);
            string month = date.Substring(4, 2);
            string day = date.Substring(6, 2);
            string hour = date.Substring(8, 2);
            string minute = date.Substring(10, 2);
            string second = date.Substring(12, 2);
            return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), int.Parse(second));
        }

        /// <summary>
        /// Returns the file size based on the specified file path, with the smallest unit being bytes (B).
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>
        /// The calculated file size, with units in bytes (B), kilobytes (KB), megabytes (MB), or gigabytes (GB).
        /// </returns>
        public static string GetFileSize(string filePath)
        {
            try
            {
                long fileSize = new FileInfo(filePath).Length;
                string[] sizes = { "B", "KB", "MB", "GB" };
                int order = 0;

                while (fileSize >= 1024 && order < sizes.Length - 1)
                {
                    fileSize /= 1024;
                    order++;
                }

                return $"{fileSize} {sizes[order]}";
            }
            catch
            {
                return "0B";
            }
        }

        public static string GetExistedPathOrEmpty(string filter = "PDF files (*.pdf)|*.pdf")
        {
            string selectedFilePath = string.Empty;
            OpenFileDialog openFileDialog;
            try
            {
                openFileDialog = new OpenFileDialog
                {
                    Filter = filter
                };
            }
            catch
            {
                return string.Empty;
            };


            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
            }
            return selectedFilePath;
        }

        public static string GetGeneratePathOrEmpty(string filter, string defaultFileName = "")
        {
            string selectedFilePath = string.Empty;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultFileName
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                selectedFilePath = saveFileDialog.FileName;
            }
            return selectedFilePath;
        }

        public static string GetPageParmFromList(List<int> pagesList)
        {
            string pageParam = "";
            if (pagesList.Count != 0)
            {
                pagesList.Sort();

                for (int i = 0; i < pagesList.Count; i++)
                {
                    if (i == 0)
                    {
                        pageParam += pagesList[0].ToString();
                    }
                    else
                    {
                        if (pagesList[i] == pagesList[i - 1] + 1)
                        {
                            if (i >= 2)
                            {
                                if (pagesList[i - 1] != pagesList[i - 2] + 1)
                                    pageParam += "-";
                            }
                            else
                                pageParam += "-";

                            if (i == pagesList.Count - 1)
                            {
                                pageParam += pagesList[i].ToString();
                            }
                        }
                        else
                        {
                            if (i >= 2)
                            {
                                if (pagesList[i - 1] == pagesList[i - 2] + 1)
                                    pageParam += pagesList[i - 1].ToString();
                            }
                            pageParam += "," + pagesList[i].ToString();
                        }
                    }
                }
            }
            return pageParam;
        }

        public static List<int> GetDefaultPageList(CPDFDocument document)
        {
            List<int> pageRangeList = new List<int>();
            for (int i = 0; i < document.PageCount; i++)
            {
                pageRangeList.Add(i + 1);
            }
            return pageRangeList;
        }

        public static bool GetPagesInRange(ref List<int> pageList, string pageRange, int count, char[] enumerationSeparator, char[] rangeSeparator, bool inittag = false)
        {
            if (pageRange == null || pageList == null)
            {
                return false;
            }

            pageList.Clear();
            int starttag = inittag ? 0 : 1;

            string[] rangeSplit = pageRange.Split(enumerationSeparator);

            foreach (string range in rangeSplit)
            {
                if (range.Contains("-"))
                {
                    string[] limits = range.Split(rangeSeparator);
                    if (limits.Length == 2 && int.TryParse(limits[0], out int start) && int.TryParse(limits[1], out int end))
                    {
                        if (start < starttag || end > count || start > end)
                        {
                            return false;
                        }

                        for (int i = start; i <= end; i++)
                        {
                            if (pageList.Contains(i - 1))
                            {
                                return false;
                            }

                            pageList.Add(i - 1);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (int.TryParse(range, out int pageNr))
                {
                    if (pageNr < starttag || pageNr > count)
                    {
                        return false;
                    }

                    if (pageList.Contains(pageNr - 1))
                    {
                        return false;
                    }

                    pageList.Add(pageNr - 1);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        internal static byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        internal static class PageEditHelper
        {
            public static T FindVisualParent<T>(DependencyObject obj) where T : class
            {
                while (obj != null)
                {
                    if (obj is T)
                        return obj as T;

                    obj = VisualTreeHelper.GetParent(obj);
                }
                return null;
            }

            public static childItem FindVisualChild<childItem>(DependencyObject obj)
                 where childItem : DependencyObject
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is childItem)
                        return (childItem)child;
                    else
                    {
                        childItem childOfChild = FindVisualChild<childItem>(child);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
                return null;
            }
        }

        public static class ViewportHelper
        {
            public static CPDFDocument CopyDoc;

            public static bool IsInViewport(ScrollViewer sv, Control item)
            {
                if (item == null) return false;
                ItemsControl itemsControl = null;
                if (item is ListBoxItem)
                    itemsControl = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
                else
                    throw new NotSupportedException(item.GetType().Name);
                ScrollContentPresenter scrollContentPresenter = (ScrollContentPresenter)sv.Template.FindName("PART_ScrollContentPresenter", sv);
                MethodInfo isInViewportMethod = sv.GetType().GetMethod("IsInViewport", BindingFlags.NonPublic | BindingFlags.Instance);
                return (bool)isInViewportMethod.Invoke(sv, new object[] { scrollContentPresenter, item });
            }

            public static T FindVisualParent<T>(DependencyObject obj) where T : class
            {
                while (obj != null)
                {
                    if (obj is T)
                        return obj as T;

                    obj = VisualTreeHelper.GetParent(obj);
                }
                return null;
            }

            public static childItem FindVisualChild<childItem>(DependencyObject obj)
                   where childItem : DependencyObject
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is childItem)
                        return (childItem)child;
                    else
                    {
                        childItem childOfChild = FindVisualChild<childItem>(child);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
                return null;
            }
        }

        public class ArrowHelper
        {
            public bool HasStartArrow
            {
                get
                {
                    if (StartSharp != C_LINE_TYPE.LINETYPE_UNKNOWN && StartSharp != C_LINE_TYPE.LINETYPE_NONE)
                    {
                        return true;
                    }
                    return false;
                }
            }

            public bool IsStartClosed
            {
                get
                {
                    if (StartSharp == C_LINE_TYPE.LINETYPE_CLOSEDARROW || StartSharp == C_LINE_TYPE.LINETYPE_RCLOSEDARROW || StartSharp == C_LINE_TYPE.LINETYPE_DIAMOND)
                    {
                        return true;
                    }
                    return false;
                }
            }

            public bool HasEndArrow
            {
                get
                {
                    if (EndSharp != C_LINE_TYPE.LINETYPE_UNKNOWN && EndSharp != C_LINE_TYPE.LINETYPE_NONE)
                    {
                        return true;
                    }
                    return false;
                }
            }

            public bool IsEndClosed
            {
                get
                {
                    if (EndSharp == C_LINE_TYPE.LINETYPE_CLOSEDARROW || EndSharp == C_LINE_TYPE.LINETYPE_RCLOSEDARROW || EndSharp == C_LINE_TYPE.LINETYPE_DIAMOND)
                    {
                        return true;
                    }
                    return false;
                }
            }

            public uint ArrowAngle { get; set; }
            public uint ArrowLength { get; set; }
            public Point? LineStart { get; set; }
            public Point? LineEnd { get; set; }
            public PathGeometry Body { get; set; }
            public C_LINE_TYPE StartSharp { get; set; }
            public C_LINE_TYPE EndSharp { get; set; }
            public ArrowHelper()
            {
                Body = new PathGeometry();
                ArrowLength = 12;
                ArrowAngle = 60;
            }
            protected PathFigure CreateLineBody()
            {
                if (LineStart != null && LineEnd != null)
                {
                    PathFigure lineFigure = new PathFigure();
                    lineFigure.StartPoint = (Point)LineStart;
                    LineSegment linePath = new LineSegment();
                    linePath.Point = (Point)LineEnd;
                    lineFigure.Segments.Add(linePath);
                    return lineFigure;
                }
                return null;
            }
            protected PathFigure CreateStartArrow()
            {
                switch (StartSharp)
                {
                    case C_LINE_TYPE.LINETYPE_NONE:
                    case C_LINE_TYPE.LINETYPE_UNKNOWN:
                        break;
                    case C_LINE_TYPE.LINETYPE_ARROW:
                    case C_LINE_TYPE.LINETYPE_CLOSEDARROW:
                        return CreateStartOpenArrow();
                    case C_LINE_TYPE.LINETYPE_ROPENARROW:
                    case C_LINE_TYPE.LINETYPE_RCLOSEDARROW:
                        return CreateStartReverseArrow();
                    case C_LINE_TYPE.LINETYPE_BUTT:
                        return CreateStartButtArrow();
                    case C_LINE_TYPE.LINETYPE_DIAMOND:
                        return CreateStartDiamondArrow();
                    case C_LINE_TYPE.LINETYPE_CIRCLE:
                        return CreateStartRoundArrow();
                    case C_LINE_TYPE.LINETYPE_SQUARE:
                        return CreateStartSquareArrow();
                    case C_LINE_TYPE.LINETYPE_SLASH:
                        return CreateStartSlashArrow();
                    default:
                        break;
                }
                return null;
            }
            protected virtual PathFigure CreateEndArrow()
            {
                switch (EndSharp)
                {
                    case C_LINE_TYPE.LINETYPE_NONE:
                    case C_LINE_TYPE.LINETYPE_UNKNOWN:
                        break;
                    case C_LINE_TYPE.LINETYPE_ARROW:
                    case C_LINE_TYPE.LINETYPE_CLOSEDARROW:
                        return CreateEndOpenArrow();
                    case C_LINE_TYPE.LINETYPE_ROPENARROW:
                    case C_LINE_TYPE.LINETYPE_RCLOSEDARROW:
                        return CreateEndReverseArrow();
                    case C_LINE_TYPE.LINETYPE_BUTT:
                        return CreateEndButtArrow();
                    case C_LINE_TYPE.LINETYPE_DIAMOND:
                        return CreateEndDiamondArrow();
                    case C_LINE_TYPE.LINETYPE_CIRCLE:
                        return CreateEndRoundArrow();
                    case C_LINE_TYPE.LINETYPE_SQUARE:
                        return CreateEndSquareArrow();
                    case C_LINE_TYPE.LINETYPE_SLASH:
                        return CreateEndSlashArrow();
                    default:
                        break;
                }
                return null;
            }

            public PathGeometry BuildArrowBody()
            {
                Body.Figures.Clear();
                PathFigure lineBody = CreateLineBody();
                if (lineBody != null)
                {
                    Body.Figures.Add(lineBody);
                    PathFigure arrowFigure = CreateStartArrow();
                    if (arrowFigure != null)
                    {
                        Body.Figures.Add(arrowFigure);
                    }
                    arrowFigure = CreateEndArrow();
                    if (arrowFigure != null)
                    {
                        Body.Figures.Add(arrowFigure);
                    }
                }
                return Body;
            }

            private PathFigure CreateStartOpenArrow()
            {
                if (ArrowLength == 0 || !HasStartArrow || LineStart == null || LineEnd == null || ArrowAngle == 0)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment arrowSegment = new PolyLineSegment();
                Vector lineVector = (Point)LineEnd - (Point)LineStart;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(ArrowAngle / 2);
                arrowFigure.StartPoint = (Point)LineStart + (lineVector * rotateMatrix);
                arrowSegment.Points.Add((Point)LineStart);
                rotateMatrix.Rotate(-ArrowAngle);
                arrowSegment.Points.Add((Point)LineStart + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(arrowSegment);
                arrowFigure.IsClosed = IsStartClosed;
                arrowFigure.IsFilled = IsStartClosed;
                return arrowFigure;
            }

            private PathFigure CreateEndOpenArrow()
            {
                if (ArrowLength == 0 || !HasEndArrow || LineStart == null || LineEnd == null || ArrowAngle == 0)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment arrowSegment = new PolyLineSegment();
                Vector lineVector = (Point)LineStart - (Point)LineEnd;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(ArrowAngle / 2);
                arrowFigure.StartPoint = (Point)LineEnd + (lineVector * rotateMatrix);
                arrowSegment.Points.Add((Point)LineEnd);
                rotateMatrix.Rotate(-ArrowAngle);
                arrowSegment.Points.Add((Point)LineEnd + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(arrowSegment);
                arrowFigure.IsClosed = IsEndClosed;
                arrowFigure.IsFilled = IsEndClosed;
                return arrowFigure;
            }

            private PathFigure CreateStartReverseArrow()
            {
                if (ArrowLength == 0 || !HasStartArrow || LineStart == null || LineEnd == null || ArrowAngle == 0)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment arrowSegment = new PolyLineSegment();
                Vector lineVector = (Point)LineStart - (Point)LineEnd;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(ArrowAngle / 2);
                arrowFigure.StartPoint = (Point)LineStart + (lineVector * rotateMatrix);
                arrowSegment.Points.Add((Point)LineStart);
                rotateMatrix.Rotate(-ArrowAngle);
                arrowSegment.Points.Add((Point)LineStart + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(arrowSegment);
                arrowFigure.IsClosed = IsStartClosed;
                arrowFigure.IsFilled = IsStartClosed;
                return arrowFigure;
            }

            private PathFigure CreateEndReverseArrow()
            {
                if (ArrowLength == 0 || !HasEndArrow || LineStart == null || LineEnd == null || ArrowAngle == 0)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment arrowSegment = new PolyLineSegment();
                Vector lineVector = (Point)LineEnd - (Point)LineStart;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(ArrowAngle / 2);
                arrowFigure.StartPoint = (Point)LineEnd + (lineVector * rotateMatrix);
                arrowSegment.Points.Add((Point)LineEnd);
                rotateMatrix.Rotate(-ArrowAngle);
                arrowSegment.Points.Add((Point)LineEnd + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(arrowSegment);
                arrowFigure.IsClosed = IsEndClosed;
                arrowFigure.IsFilled = IsEndClosed;
                return arrowFigure;
            }

            private PathFigure CreateStartButtArrow()
            {
                if (ArrowLength == 0 || !HasStartArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                LineSegment buttSegment = new LineSegment();
                Vector lineVector = (Point)LineStart - (Point)LineEnd;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(90);
                arrowFigure.StartPoint = (Point)LineStart + (lineVector * rotateMatrix);
                rotateMatrix.Rotate(-180);
                buttSegment.Point = ((Point)LineStart + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(buttSegment);
                return arrowFigure;
            }

            private PathFigure CreateEndButtArrow()
            {
                if (ArrowLength == 0 || !HasEndArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                LineSegment buttSegment = new LineSegment();
                Vector lineVector = (Point)LineEnd - (Point)LineStart;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(90);
                arrowFigure.StartPoint = (Point)LineEnd + (lineVector * rotateMatrix);
                rotateMatrix.Rotate(-180);
                buttSegment.Point = ((Point)LineEnd + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(buttSegment);

                return arrowFigure;
            }

            private PathFigure CreateStartDiamondArrow()
            {
                if (ArrowLength == 0 || !HasStartArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment arrowSegment = new PolyLineSegment();
                Vector lineVector = (Point)LineStart - (Point)LineEnd;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(45);

                Point cornerTop = (Point)LineStart + (lineVector * rotateMatrix);

                Vector turnVector = cornerTop - (Point)LineStart;
                turnVector.Normalize();
                turnVector *= ArrowLength;
                Matrix turnMatrix = new Matrix();
                turnMatrix.Rotate(-90);
                Point awayPoint = cornerTop + (turnVector * turnMatrix);

                rotateMatrix = new Matrix();
                rotateMatrix.Rotate(-45);
                Point cornerDown = (Point)LineStart + (lineVector * rotateMatrix);

                arrowFigure.StartPoint = (Point)LineStart;
                arrowSegment.Points.Add(cornerTop);
                arrowSegment.Points.Add(awayPoint);
                arrowSegment.Points.Add(cornerDown);
                arrowSegment.Points.Add((Point)LineStart);

                arrowFigure.Segments.Add(arrowSegment);
                arrowFigure.IsClosed = IsStartClosed;
                arrowFigure.IsFilled = IsStartClosed;
                return arrowFigure;
            }

            private PathFigure CreateEndDiamondArrow()
            {

                if (ArrowLength == 0 || !HasEndArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment arrowSegment = new PolyLineSegment();
                Vector lineVector = (Point)LineEnd - (Point)LineStart;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(45);

                Point cornerTop = (Point)LineEnd + (lineVector * rotateMatrix);

                Vector turnVector = cornerTop - (Point)LineEnd;
                turnVector.Normalize();
                turnVector *= ArrowLength;
                Matrix turnMatrix = new Matrix();
                turnMatrix.Rotate(-90);
                Point awayPoint = cornerTop + (turnVector * turnMatrix);

                rotateMatrix = new Matrix();
                rotateMatrix.Rotate(-45);
                Point cornerDown = (Point)LineEnd + (lineVector * rotateMatrix);

                arrowFigure.StartPoint = (Point)LineEnd;
                arrowSegment.Points.Add(cornerTop);
                arrowSegment.Points.Add(awayPoint);
                arrowSegment.Points.Add(cornerDown);
                arrowSegment.Points.Add((Point)LineEnd);

                arrowFigure.Segments.Add(arrowSegment);
                arrowFigure.IsClosed = IsEndClosed;
                arrowFigure.IsFilled = IsEndClosed;
                return arrowFigure;
            }

            private PathFigure CreateStartRoundArrow()
            {
                if (ArrowLength == 0 || !HasStartArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                Vector lineVector = (Point)LineEnd - (Point)LineStart;
                lineVector.Normalize();
                lineVector *= ArrowLength;

                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(180);
                arrowFigure.StartPoint = (Point)LineStart + (lineVector * rotateMatrix);
                ArcSegment circleSegment = new ArcSegment();
                circleSegment.Point = (Point)LineStart;
                circleSegment.Size = new Size(ArrowLength / 2, ArrowLength / 2);
                arrowFigure.Segments.Add(circleSegment);
                circleSegment = new ArcSegment();
                circleSegment.Point = (Point)arrowFigure.StartPoint;
                circleSegment.Size = new Size(ArrowLength / 2, ArrowLength / 2);

                arrowFigure.Segments.Add(circleSegment);

                return arrowFigure;
            }

            private PathFigure CreateEndRoundArrow()
            {
                if (ArrowLength == 0 || !HasEndArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                Vector lineVector = (Point)LineStart - (Point)LineEnd;
                lineVector.Normalize();
                lineVector *= ArrowLength;

                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(180);
                arrowFigure.StartPoint = (Point)LineEnd + (lineVector * rotateMatrix);
                ArcSegment circleSegment = new ArcSegment();
                circleSegment.Point = (Point)LineEnd;
                circleSegment.Size = new Size(ArrowLength / 2, ArrowLength / 2);
                arrowFigure.Segments.Add(circleSegment);
                circleSegment = new ArcSegment();
                circleSegment.Point = (Point)arrowFigure.StartPoint;
                circleSegment.Size = new Size(ArrowLength / 2, ArrowLength / 2);

                arrowFigure.Segments.Add(circleSegment);

                return arrowFigure;
            }

            private PathFigure CreateStartSquareArrow()
            {
                if (ArrowLength == 0 || !HasStartArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment squreSegment = new PolyLineSegment();

                Vector lineVector = (Point)LineEnd - (Point)LineStart;
                lineVector.Normalize();
                lineVector *= (ArrowLength / 2);
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(90);
                arrowFigure.StartPoint = (Point)LineStart + (lineVector * rotateMatrix);
                rotateMatrix.Rotate(-180);
                Point pointCorner = (Point)LineStart + (lineVector * rotateMatrix);
                squreSegment.Points.Add(pointCorner);

                Vector moveVector = arrowFigure.StartPoint - pointCorner;
                moveVector.Normalize();
                moveVector *= (ArrowLength);
                rotateMatrix = new Matrix();
                rotateMatrix.Rotate(90);

                squreSegment.Points.Add(pointCorner + (moveVector * rotateMatrix));
                squreSegment.Points.Add(arrowFigure.StartPoint + (moveVector * rotateMatrix));
                squreSegment.Points.Add(arrowFigure.StartPoint);
                squreSegment.Points.Add((Point)LineStart);
                arrowFigure.Segments.Add(squreSegment);

                return arrowFigure;
            }

            private PathFigure CreateEndSquareArrow()
            {

                if (ArrowLength == 0 || !HasEndArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                PolyLineSegment squreSegment = new PolyLineSegment();

                Vector lineVector = (Point)LineStart - (Point)LineEnd;
                lineVector.Normalize();
                lineVector *= (ArrowLength / 2);
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(90);
                arrowFigure.StartPoint = (Point)LineEnd + (lineVector * rotateMatrix);
                rotateMatrix.Rotate(-180);
                Point pointCorner = (Point)LineEnd + (lineVector * rotateMatrix);
                squreSegment.Points.Add(pointCorner);

                Vector moveVector = arrowFigure.StartPoint - pointCorner;
                moveVector.Normalize();
                moveVector *= (ArrowLength);
                rotateMatrix = new Matrix();
                rotateMatrix.Rotate(90);

                squreSegment.Points.Add(pointCorner + (moveVector * rotateMatrix));
                squreSegment.Points.Add(arrowFigure.StartPoint + (moveVector * rotateMatrix));
                squreSegment.Points.Add(arrowFigure.StartPoint);
                squreSegment.Points.Add((Point)LineEnd);
                arrowFigure.Segments.Add(squreSegment);

                return arrowFigure;
            }

            private PathFigure CreateStartSlashArrow()
            {
                if (ArrowLength == 0 || !HasStartArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                LineSegment buttSegment = new LineSegment();
                Vector lineVector = (Point)LineStart - (Point)LineEnd;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(45);
                arrowFigure.StartPoint = (Point)LineStart + (lineVector * rotateMatrix);
                rotateMatrix.Rotate(-180);
                buttSegment.Point = ((Point)LineStart + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(buttSegment);
                return arrowFigure;
            }

            private PathFigure CreateEndSlashArrow()
            {
                if (ArrowLength == 0 || !HasEndArrow || LineStart == null || LineEnd == null)
                {
                    return null;
                }
                PathFigure arrowFigure = new PathFigure();
                LineSegment buttSegment = new LineSegment();
                Vector lineVector = (Point)LineEnd - (Point)LineStart;
                lineVector.Normalize();
                lineVector *= ArrowLength;
                Matrix rotateMatrix = new Matrix();
                rotateMatrix.Rotate(45);
                arrowFigure.StartPoint = (Point)LineEnd + (lineVector * rotateMatrix);
                rotateMatrix.Rotate(-180);
                buttSegment.Point = ((Point)LineEnd + (lineVector * rotateMatrix));
                arrowFigure.Segments.Add(buttSegment);

                return arrowFigure;
            }
        }
    }

    public class PanelState
    {
        private static PanelState instance;

        public enum RightPanelState
        {
            None,
            PropertyPanel,
            ViewSettings
        }

        private bool _isLeftPanelExpand;
        public bool IsLeftPanelExpand
        {
            get { return _isLeftPanelExpand; }
            set
            {
                if (_isLeftPanelExpand != value)
                {
                    _isLeftPanelExpand = value;
                    OnPropertyChanged();
                }
            }
        }

        private RightPanelState _rightPanel;
        public RightPanelState RightPanel
        {
            get { return _rightPanel; }
            set
            {
                if (_rightPanel != value)
                {
                    _rightPanel = value;
                    OnPropertyChanged();
                }
            }
        }

        private PanelState() { }

        public static PanelState GetInstance()
        {
            if (instance == null)
            {
                instance = new PanelState();
            }
            return instance;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SaveHelper
    {
        private static SaveHelper instance;
        private bool _canSave;
        public bool CanSave
        {
            get { return _canSave; }
            set
            {
                if (_canSave != value)
                {
                    _canSave = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CommandHelper
    {
        public static void CopyImage_Click(Dictionary<int, List<Bitmap>> imageDict)
        {
            try
            {
                if (imageDict != null && imageDict.Count > 0)
                {
                    foreach (int pageIndex in imageDict.Keys)
                    {
                        List<Bitmap> imageList = imageDict[pageIndex];
                        foreach (Bitmap image in imageList)
                        {
                            MemoryStream ms = new MemoryStream();
                            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            BitmapImage imageData = new BitmapImage();
                            imageData.BeginInit();
                            imageData.StreamSource = ms;
                            imageData.CacheOption = BitmapCacheOption.OnLoad;
                            imageData.EndInit();
                            imageData.Freeze();
                            Clipboard.SetImage(imageData);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void ExtraImage_Click(Dictionary<int, List<Bitmap>> imageDict)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string choosePath = folderDialog.SelectedPath;
                string openPath = choosePath;
                try
                {
                    if (imageDict != null && imageDict.Count > 0)
                    {
                        foreach (int pageIndex in imageDict.Keys)
                        {
                            List<Bitmap> imageList = imageDict[pageIndex];
                            foreach (Bitmap image in imageList)
                            {
                                string savePath = Path.Combine(choosePath, Guid.NewGuid() + ".jpg");
                                image.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                openPath = savePath;
                            }
                        }
                    }
                    Process.Start("explorer", "/select,\"" + openPath + "\"");
                }
                catch (Exception ex)
                {

                }
            }
        }

        public static double CheckZoomLevel(double[] zoomLevelList, double zoom, bool IsGrowth)
        {
            double standardZoom = 100;
            if (zoom <= 0.01)
            {
                return 0.01;
            }
            if (zoom >= 10)
            {
                return 10;
            }

            zoom *= 100;
            for (int i = 0; i < zoomLevelList.Length - 1; i++)
            {
                if (zoom > zoomLevelList[i] && zoom <= zoomLevelList[i + 1] && IsGrowth)
                {
                    standardZoom = zoomLevelList[i + 1];
                    break;
                }
                if (zoom >= zoomLevelList[i] && zoom < zoomLevelList[i + 1] && !IsGrowth)
                {
                    standardZoom = zoomLevelList[i];
                    break;
                }
            }
            return standardZoom / 100;
        }
    }

    public class SignatureHelper
    {
        public static List<CPDFSignature> SignatureList;

        public static void InitEffectiveSignatureList(CPDFDocument document)
        {
            SignatureList = document.GetSignatureList();
            for (int index = SignatureList.Count - 1; index >= 0; index--)
            {
                if (SignatureList[index].SignerList.Count <= 0)
                {
                    SignatureList.RemoveAt(index);
                }
            }
        }

        public static void VerifySignatureList(CPDFDocument document)
        {
            foreach (var sig in SignatureList)
            {
                sig.VerifySignatureWithDocument(document);
            }

        }
    }
}
