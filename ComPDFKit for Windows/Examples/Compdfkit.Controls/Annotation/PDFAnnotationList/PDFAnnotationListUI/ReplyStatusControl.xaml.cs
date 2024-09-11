using ComPDFKit.PDFAnnotation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ComPDFKit.Controls.PDFControlUI
{
    /// <summary>
    /// Interaction logic for ReplyStatusControl.xaml
    /// </summary>
    public partial class ReplyStatusControl : UserControl
    {
        public List<PathMenuItem> PathItems;

        public event EventHandler<CPDFAnnotationState> ReplyStatusChanged;

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(nameof(Status), typeof(CPDFAnnotationState), typeof(ReplyStatusControl),
                new PropertyMetadata(CPDFAnnotationState.C_ANNOTATION_NONE, OnStatusChanged));

        public CPDFAnnotationState Status
        {
            get => (CPDFAnnotationState)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ReplyStatusControl;
            var newState = (CPDFAnnotationState)e.NewValue;
            control.SetButtonIcon(newState);
        }

        public ReplyStatusControl()
        {
            InitializeComponent();
            InitPathItems();
        }

        private void InitPathItems()
        {
            PathItems = new List<PathMenuItem>
            {
                new PathMenuItem
                {
                    Header = "Accepted",
                    IconPath = new Path
                    {
                        Data = Geometry.Parse("M6.58308 8.44989L9.4497 1.99999C10.0199 1.99999 10.5668 2.22651 10.97 2.6297C11.3732 3.0329 11.5997 3.57975 11.5997 4.14996V7.01658H15.6559C15.8637 7.01422 16.0695 7.05707 16.259 7.14215C16.4486 7.22722 16.6174 7.3525 16.7537 7.50929C16.8901 7.66608 16.9907 7.85063 17.0486 8.05017C17.1066 8.24971 17.1204 8.45946 17.0892 8.66488L16.1003 15.1148C16.0484 15.4565 15.8748 15.7681 15.6114 15.992C15.348 16.2158 15.0126 16.337 14.6669 16.3331H6.58308M6.58308 8.44989V16.3331M6.58308 8.44989H4.66961C4.26402 8.44271 3.8699 8.58471 3.56209 8.84893C3.25427 9.11315 3.05419 9.48119 2.99981 9.88319V14.8998C3.05419 15.3018 3.25427 15.6698 3.56209 15.934C3.8699 16.1983 4.26402 16.3403 4.66961 16.3331H6.58308"),
                    },
                    Tag = CPDFAnnotationState.C_ANNOTATION_ACCEPTED
                },
                new PathMenuItem
                {
                    Header = "Rejected",
                    IconPath = new Path
                    {
                        Data = Geometry.Parse("M11.5224 8.88346L8.65577 15.3333C8.08556 15.3333 7.53871 15.1068 7.13551 14.7036C6.73232 14.3004 6.5058 13.7536 6.5058 13.1834V10.3168H2.44954C2.24178 10.3191 2.03598 10.2763 1.84642 10.1912C1.65686 10.1061 1.48807 9.98085 1.35173 9.82406C1.21539 9.66727 1.11477 9.48271 1.05684 9.28317C0.998906 9.08363 0.985051 8.87389 1.01623 8.66846L2.00521 2.21857C2.05704 1.87679 2.23065 1.56526 2.49404 1.34138C2.75743 1.1175 3.09286 0.996351 3.43852 1.00026H11.5224M11.5224 8.88346V1.00026M11.5224 8.88346H13.4359C13.8415 8.89063 14.2356 8.74863 14.5434 8.48441C14.8512 8.22019 15.0513 7.85215 15.1057 7.45015V2.43357C15.0513 2.03157 14.8512 1.66352 14.5434 1.39931C14.2356 1.13509 13.8415 0.993086 13.4359 1.00026H11.5224"),
                    },
                    Tag = CPDFAnnotationState.C_ANNOTATION_REJECTED
                },
                new PathMenuItem
                {
                    Header = "Cancelled",
                    IconPath = new Path
                    {
                        Data = Geometry.Parse("M2 2 H16 V16 H2 V2 M4 4 L14 14 M14 4 L4 14"),
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Fill = Brushes.Transparent
                    },
                    Tag = CPDFAnnotationState.C_ANNOTATION_CANCELLED
                },
                new PathMenuItem
                {
                    Header = "Completed",
                    IconPath = new Path
                    {
                        Data = Geometry.Parse("M2 2 H16 V16 H2 V2 M5 9 L8 12 L14 6"),
                        Stroke = Brushes.Black, 
                        StrokeThickness = 1, 
                        Fill = Brushes.Transparent 
                    },
                    Tag = CPDFAnnotationState.C_ANNOTATION_COMPLETED
                },

                new PathMenuItem
                {
                    Header = "None",
                    IconPath = new Path
                    {
                        Data = Geometry.Parse("M2 2 H16 V16 H2 V2 M5 9 H13"),
                        Stroke = Brushes.Black,  
                        StrokeThickness = 1, 
                        Fill = Brushes.Transparent 
                    },
                    Tag = CPDFAnnotationState.C_ANNOTATION_NONE
                }, 
            };

            Style style = FindResource("MenuItemStyle") as Style;

            foreach (var item in PathItems)
            {
                item.IconPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43474D"));
                item.IconPath.StrokeThickness = 1.5;
                item.IconPath.StrokeDashCap = PenLineCap.Round;
                item.IconPath.StrokeDashArray = new DoubleCollection { 2, 0 };
                item.IconPath.HorizontalAlignment = HorizontalAlignment.Center;
                item.IconPath.VerticalAlignment = VerticalAlignment.Center;
                item.Style = style;
                item.Click += MenuItem_Click;
                IconMenu.Items.Add(item);
            }
        }

        public void SetButtonIcon(CPDFAnnotationState status)
        {
            var pathItem = PathItems.Find(x => (CPDFAnnotationState)x.Tag == status);
            if (pathItem == null) return;
            ButtonIcon.Children.Clear();
            ButtonIcon.Children.Add(pathItem.IconPath.Clone());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem is PathMenuItem pathItem)
                {
                    ButtonIcon.Children.Clear();
                    ButtonIcon.Children.Add(pathItem.IconPath.Clone());
                    ReplyStatusChanged?.Invoke(this, (CPDFAnnotationState)pathItem.Tag);
                }
            }
        }
    }

    public class PathMenuItem : MenuItem
    {
        public static readonly DependencyProperty IconPathProperty =
            DependencyProperty.Register(nameof(IconPath), typeof(Path), typeof(PathMenuItem));

        public Path IconPath
        {
            get
            {
                var a = (Path)GetValue(IconPathProperty);
                return a;
            }
            set { SetValue(IconPathProperty, value); }
        }
    }

    public static class PathExtensions
    {
        public static Path Clone(this Path original)
        {
            return new Path
            {
                Data = original.Data?.Clone(),
                Fill = original.Fill?.Clone(),
                Stroke = original.Stroke?.Clone(),
                StrokeThickness = original.StrokeThickness,
                StrokeDashArray = original.StrokeDashArray?.Clone(),
                StrokeDashCap = original.StrokeDashCap,
                HorizontalAlignment = original.HorizontalAlignment,
                VerticalAlignment = original.VerticalAlignment
            };
        }
    }
}
