using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace ComPDFKit.Tool.Help
{
    public static class CommonHelper
    {
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
            List <childItem> children = new List <childItem>();
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


    }
}
