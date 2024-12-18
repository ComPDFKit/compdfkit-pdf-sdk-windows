using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using ComPDFKitViewer;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using ComPDFKit.PDFAnnotation;
using System.Windows.Input;
using System.Reflection;

namespace ComPDFKit.Tool.Help
{
    public static class CommonHelper
    {
        private static Cursor _rotationCursor = null;
        public static Cursor RotationCursor
        {
            get
            {
                if (_rotationCursor == null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    Stream stream = assembly.GetManifestResourceStream("ComPDFKit.Tool.Resource.Cursor.Rotation.cur");
                    _rotationCursor = new Cursor(stream);
                }
                return _rotationCursor;
            }
        }

        /// <summary>
        /// Find the parent control of the target type of the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            try
            {
                while (obj != null)
                {
                    if (obj is T)
                        return obj as T;

                    obj = VisualTreeHelper.GetParent(obj);
                }
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Find the child control of the target type of the object
        /// </summary>
        /// <typeparam name="childItem">
        /// The type of the child control to find
        /// </typeparam>
        /// <param name="obj">
        /// The object to find
        /// </param>
        /// <returns>
        /// The child control of the target type of the object
        /// </returns>
        public static childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            try
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
            catch { return null; }
        }

        /// <summary>
        /// Find the child control of the target type of the object
        /// </summary>
        /// <typeparam name="childItem">
        /// The type of the child control to find
        /// </typeparam>
        /// <param name="obj">
        /// The object to find
        /// </param>
        /// <returns>
        /// The child control of the target type of the object
        /// </returns>
        public static List<childItem> FindVisualChildList<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            List<childItem> children = new List<childItem>();
            try
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is childItem)
                    {
                        children.Add((childItem)child);
                    }
                    else
                    {
                        childItem childOfChild = FindVisualChild<childItem>(child);
                        if (childOfChild != null)
                            children.Add(childOfChild);
                    }
                }
                return children;
            }
            catch { return children; }
        }

        public static PathGeometry GetPathIcon(string iconKey)
        {
            string pathIcon = "M18 3H2V15H5V18L10 15H18V3ZM5 6H11V7.5H5V6ZM5 9.5H15V11H5V9.5Z";
            try
            {
                TypeConverter typeCovert = TypeDescriptor.GetConverter(typeof(Geometry));
                if (CPDFViewer.StickyIconDict != null && CPDFViewer.StickyIconDict.ContainsKey(iconKey))
                {
                    pathIcon = CPDFViewer.StickyIconDict[iconKey];
                }

                return PathGeometry.CreateFromGeometry((Geometry)typeCovert.ConvertFrom(pathIcon));
            }
            catch (Exception ex)
            {

            }

            return new PathGeometry();
        }

        private static bool GetIconData(string iconName, Brush fillBrush, out string tempImagePath)
        {
            tempImagePath = string.Empty;

            try
            {
                if (CPDFViewer.StickyIconDict != null && CPDFViewer.StickyIconDict.ContainsKey(iconName))
                {
                    PathGeometry iconGeometry = GetPathIcon(iconName);
                    DrawingVisual iconVisual = new DrawingVisual();
                    DrawingContext iconContext = iconVisual.RenderOpen();
                    iconContext.DrawGeometry(fillBrush, null, iconGeometry);
                    iconContext.Close();
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(iconVisual);
                    string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

                    PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                    using (FileStream fs = File.Create(tempPath))
                    {
                        pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        pngEncoder.Save(fs);
                    }
                    tempImagePath = tempPath;
                    return true;
                }
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        public static void UpdateStickyAP(CPDFTextAnnotation textAnnotation)
        {
            if (textAnnotation == null || textAnnotation.IsValid() == false)
            {
                return;
            }
            try
            {
                string iconName = textAnnotation.GetIconName();
                byte opacity = textAnnotation.GetTransparency();
                SolidColorBrush fillBrush = new SolidColorBrush(Color.FromArgb(opacity, textAnnotation.Color[0], textAnnotation.Color[1], textAnnotation.Color[2]));

                if (GetIconData(iconName, fillBrush, out string apPath) && File.Exists(apPath))
                {
                    textAnnotation.UpdateApWithImage(apPath, string.Empty, textAnnotation.GetRotation());
                    File.Delete(apPath);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
