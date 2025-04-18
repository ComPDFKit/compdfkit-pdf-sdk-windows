﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ComPDFKit.Controls.Common.BaseControl
{
    #region EnumPlacement

    public enum EnumPlacement
    {
        /// <summary>
        /// 左上
        /// </summary>
        LeftTop,

        /// <summary>
        /// 左中
        /// </summary>
        LeftCenter,

        /// <summary>
        /// 左下
        /// </summary>
        LeftBottom,

        /// <summary>
        /// 右上
        /// </summary>
        RightTop,

        /// <summary>
        /// 右中
        /// </summary>
        RightCenter,

        /// <summary>
        /// 右下
        /// </summary>
        RightBottom,

        /// <summary>
        /// 上左
        /// </summary>
        TopLeft,

        /// <summary>
        /// 上中
        /// </summary>
        TopCenter,

        /// <summary>
        /// 上右
        /// </summary>
        TopRight,

        /// <summary>
        /// 下左
        /// </summary>
        BottomLeft,

        /// <summary>
        /// 下中
        /// </summary>
        BottomCenter,

        /// <summary>
        /// 下右
        /// </summary>
        BottomRight,
    }

    #endregion EnumPlacement

    public class DoubleUtil
    {
        public static double DpiScaleX
        {
            get
            {
                int dx = 0;
                int dy = 0;
                GetDPI(out dx, out dy);
                if (dx != 96)
                {
                    return (double)dx / 96.0;
                }
                return 1.0;
            }
        }

        public static double DpiScaleY
        {
            get
            {
                int dx = 0;
                int dy = 0;
                GetDPI(out dx, out dy);
                if (dy != 96)
                {
                    return (double)dy / 96.0;
                }
                return 1.0;
            }
        }

        public static void GetDPI(out int dpix, out int dpiy)
        {
            dpix = 0;
            dpiy = 0;
            using (System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_DesktopMonitor"))
            {
                using (System.Management.ManagementObjectCollection moc = mc.GetInstances())
                {
                    foreach (System.Management.ManagementObject each in moc)
                    {
                        dpix = int.Parse((each.Properties["PixelsPerXLogicalInch"].Value.ToString()));
                        dpiy = int.Parse((each.Properties["PixelsPerYLogicalInch"].Value.ToString()));
                    }
                }
            }
        }

        public static bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            double num = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * 2.2204460492503131E-16;
            double num2 = value1 - value2;
            return -num < num2 && num > num2;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            [FieldOffset(0)]
            internal double DoubleValue;

            [FieldOffset(0)]
            internal ulong UintValue;
        }

        public static bool IsNaN(double value)
        {
            DoubleUtil.NanUnion nanUnion = default(DoubleUtil.NanUnion);
            nanUnion.DoubleValue = value;
            ulong num = nanUnion.UintValue & 18442240474082181120uL;
            ulong num2 = nanUnion.UintValue & 4503599627370495uL;
            return (num == 9218868437227405312uL || num == 18442240474082181120uL) && num2 != 0uL;
        }
    }

    public class UIElementEx
    {
        public static double RoundLayoutValue(double value, double dpiScale)
        {
            double num;
            if (!DoubleUtil.AreClose(dpiScale, 1.0))
            {
                num = Math.Round(value * dpiScale) / dpiScale;
                if (DoubleUtil.IsNaN(num) || double.IsInfinity(num) || DoubleUtil.AreClose(num, 1.7976931348623157E+308))
                {
                    num = value;
                }
            }
            else
            {
                num = Math.Round(value);
            }
            return num;
        }
    }

    /// <summary>
    /// 带三角形的气泡边框
    /// </summary>
    public class AngleBorder : Decorator
    {
        #region 依赖属性

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register("Placement", typeof(EnumPlacement), typeof(AngleBorder),
            new FrameworkPropertyMetadata(EnumPlacement.RightCenter, FrameworkPropertyMetadataOptions.AffectsRender, OnDirectionPropertyChangedCallback));

        public EnumPlacement Placement
        {
            get { return (EnumPlacement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        public static readonly DependencyProperty TailWidthProperty =
            DependencyProperty.Register("TailWidth", typeof(double), typeof(AngleBorder), new PropertyMetadata(10d));

        /// <summary>
        /// 尾巴的宽度，默认值为7
        /// </summary>
        public double TailWidth
        {
            get { return (double)GetValue(TailWidthProperty); }
            set { SetValue(TailWidthProperty, value); }
        }

        public static readonly DependencyProperty TailHeightProperty =
            DependencyProperty.Register("TailHeight", typeof(double), typeof(AngleBorder), new PropertyMetadata(10d));

        /// <summary>
        /// 尾巴的高度，默认值为10
        /// </summary>
        public double TailHeight
        {
            get { return (double)GetValue(TailHeightProperty); }
            set { SetValue(TailHeightProperty, value); }
        }

        public static readonly DependencyProperty TailVerticalOffsetProperty =
            DependencyProperty.Register("TailVerticalOffset", typeof(double), typeof(AngleBorder), new PropertyMetadata(13d));

        /// <summary>
        /// 尾巴距离顶部的距离，默认值为10
        /// </summary>
        public double TailVerticalOffset
        {
            get { return (double)GetValue(TailVerticalOffsetProperty); }
            set { SetValue(TailVerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty TailHorizontalOffsetProperty =
            DependencyProperty.Register("TailHorizontalOffset", typeof(double), typeof(AngleBorder),
                new PropertyMetadata(12d));

        /// <summary>
        /// 尾巴距离顶部的距离，默认值为10
        /// </summary>
        public double TailHorizontalOffset
        {
            get { return (double)GetValue(TailHorizontalOffsetProperty); }
            set { SetValue(TailHorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(AngleBorder)
                , new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 255, 255))));

        /// <summary>
        /// 背景色，默认值为#FFFFFF，白色
        /// </summary>
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(AngleBorder)
                , new PropertyMetadata(new Thickness(10, 5, 10, 5)));

        /// <summary>
        /// 内边距
        /// </summary>
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(AngleBorder)
                , new PropertyMetadata(default(Brush)));

        /// <summary>
        /// 边框颜色
        /// </summary>
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(AngleBorder), new PropertyMetadata(new Thickness(1d)));

        /// <summary>
        /// 边框大小
        /// </summary>
        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(System.Windows.CornerRadius)
                , typeof(AngleBorder), new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// 边框大小
        /// </summary>
        public System.Windows.CornerRadius CornerRadius
        {
            get { return (System.Windows.CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        #endregion 依赖属性

        #region 方法重写

        /// <summary>
        /// 该方法用于测量整个控件的大小
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns>控件的大小</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Thickness padding = this.Padding;

            Size result = new Size();
            if (Child != null)
            {
                //测量子控件的大小
                Child.Measure(constraint);

                //三角形在左边与右边的，整个容器的宽度则为：里面子控件的宽度 + 设置的padding + 三角形的宽度
                //三角形在上面与下面的，整个容器的高度则为：里面子控件的高度 + 设置的padding + 三角形的高度
                switch (Placement)
                {
                    case EnumPlacement.LeftTop:
                    case EnumPlacement.LeftBottom:
                    case EnumPlacement.LeftCenter:
                    case EnumPlacement.RightTop:
                    case EnumPlacement.RightBottom:
                    case EnumPlacement.RightCenter:
                        result.Width = Child.DesiredSize.Width + padding.Left + padding.Right + this.TailWidth;
                        result.Height = Child.DesiredSize.Height + padding.Top + padding.Bottom;
                        break;

                    case EnumPlacement.TopLeft:
                    case EnumPlacement.TopCenter:
                    case EnumPlacement.TopRight:
                    case EnumPlacement.BottomLeft:
                    case EnumPlacement.BottomCenter:
                    case EnumPlacement.BottomRight:
                        result.Width = Child.DesiredSize.Width + padding.Left + padding.Right;
                        result.Height = Child.DesiredSize.Height + padding.Top + padding.Bottom + this.TailHeight;
                        break;

                    default:
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// 设置子控件的大小与位置
        /// </summary>
        /// <param name="arrangeSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Thickness padding = this.Padding;
            if (Child != null)
            {
                switch (Placement)
                {
                    case EnumPlacement.LeftTop:
                    case EnumPlacement.LeftBottom:
                    case EnumPlacement.LeftCenter:
                        this.TailWidth = 6;
                        Child.Arrange(new Rect(new Point(padding.Left + this.TailWidth, padding.Top), Child.DesiredSize));
                        //ArrangeChildLeft();
                        break;

                    case EnumPlacement.RightTop:
                    case EnumPlacement.RightBottom:
                    case EnumPlacement.RightCenter:
                        ArrangeChildRight(padding);
                        break;

                    case EnumPlacement.TopLeft:
                    case EnumPlacement.TopRight:
                    case EnumPlacement.TopCenter:
                        Child.Arrange(new Rect(new Point(padding.Left, this.TailHeight + padding.Top), Child.DesiredSize));
                        break;

                    case EnumPlacement.BottomLeft:
                    case EnumPlacement.BottomRight:
                    case EnumPlacement.BottomCenter:
                        Child.Arrange(new Rect(new Point(padding.Left, padding.Top), Child.DesiredSize));
                        break;

                    default:
                        break;
                }
            }
            return arrangeSize;
        }

        private void ArrangeChildRight(Thickness padding)
        {
            double x = padding.Left;
            double y = padding.Top;

            if (!Double.IsNaN(this.Height) && this.Height != 0)
            {
                y = (this.Height - (Child.DesiredSize.Height)) / 2;
            }

            Child.Arrange(new Rect(new Point(x, y), Child.DesiredSize));
        }

        /// <summary>
        /// 绘制控件
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Child != null)
            {
                Geometry cg = null;
                Brush brush = null;
                Pen pen = new Pen();

                pen.Brush = this.BorderBrush;
                pen.Thickness = UIElementEx.RoundLayoutValue(BorderThickness.Left, DoubleUtil.DpiScaleX);

                switch (Placement)
                {
                    case EnumPlacement.LeftTop:
                    case EnumPlacement.LeftBottom:
                    case EnumPlacement.LeftCenter:
                        //生成小尾巴在左侧的图形和底色
                        cg = CreateGeometryTailAtLeft();
                        brush = CreateFillBrush();
                        break;

                    case EnumPlacement.RightTop:
                    case EnumPlacement.RightCenter:
                    case EnumPlacement.RightBottom:
                        //生成小尾巴在右侧的图形和底色
                        cg = CreateGeometryTailAtRight();
                        brush = CreateFillBrush();
                        break;

                    case EnumPlacement.TopLeft:
                    case EnumPlacement.TopCenter:
                    case EnumPlacement.TopRight:
                        //生成小尾巴在右侧的图形和底色
                        cg = CreateGeometryTailAtTop();
                        brush = CreateFillBrush();
                        break;

                    case EnumPlacement.BottomLeft:
                    case EnumPlacement.BottomCenter:
                    case EnumPlacement.BottomRight:
                        //生成小尾巴在右侧的图形和底色
                        cg = CreateGeometryTailAtBottom();
                        brush = CreateFillBrush();
                        break;

                    default:
                        break;
                }

                GuidelineSet guideLines = new GuidelineSet();
                drawingContext.PushGuidelineSet(guideLines);
                drawingContext.DrawGeometry(brush, pen, cg);
            }
        }

        #endregion 方法重写

        #region 私有方法

        private Geometry CreateGeometryTailAtRight()
        {
            CombinedGeometry result = new CombinedGeometry();

            this.TailHeight = 12;
            this.TailWidth = 6;

            switch (this.Placement)
            {
                case EnumPlacement.RightTop:
                    //不做任何处理
                    break;

                case EnumPlacement.RightBottom:
                    this.TailVerticalOffset = this.ActualHeight - this.TailHeight - this.TailVerticalOffset;
                    break;

                case EnumPlacement.RightCenter:
                    this.TailVerticalOffset = (this.ActualHeight - this.TailHeight) / 2;
                    break;
            }

            #region 绘制三角形

            Point arcPoint1 = new Point(this.ActualWidth - TailWidth, TailVerticalOffset);
            Point arcPoint2 = new Point(this.ActualWidth, TailVerticalOffset + TailHeight / 2);
            Point arcPoint3 = new Point(this.ActualWidth - TailWidth, TailVerticalOffset + TailHeight);

            LineSegment as1_2 = new LineSegment(arcPoint2, false);
            LineSegment as2_3 = new LineSegment(arcPoint3, false);

            PathFigure pf1 = new PathFigure();
            pf1.IsClosed = false;
            pf1.StartPoint = arcPoint1;
            pf1.Segments.Add(as1_2);
            pf1.Segments.Add(as2_3);

            PathGeometry pg1 = new PathGeometry();
            pg1.Figures.Add(pf1);

            #endregion 绘制三角形

            #region 绘制矩形边框

            RectangleGeometry rg2 = new RectangleGeometry(new Rect(0, 0, this.ActualWidth - TailWidth, this.ActualHeight)
                , CornerRadius.TopLeft, CornerRadius.BottomRight, new TranslateTransform(0.5, 0.5));

            #endregion 绘制矩形边框

            #region 合并两个图形

            result.Geometry1 = pg1;
            result.Geometry2 = rg2;
            result.GeometryCombineMode = GeometryCombineMode.Union;

            #endregion 合并两个图形

            return result;
        }

        private Geometry CreateGeometryTailAtLeft()
        {
            CombinedGeometry result = new CombinedGeometry();

            this.TailHeight = 12;
            this.TailWidth = 6;

            switch (this.Placement)
            {
                case EnumPlacement.LeftTop:
                    //不做任何处理
                    break;

                case EnumPlacement.LeftBottom:
                    this.TailVerticalOffset = this.ActualHeight - this.TailHeight - this.TailVerticalOffset;
                    break;

                case EnumPlacement.LeftCenter:
                    this.TailVerticalOffset = (this.ActualHeight - this.TailHeight) / 2;
                    break;
            }

            #region 绘制三角形

            Point arcPoint1 = new Point(TailWidth, TailVerticalOffset);
            Point arcPoint2 = new Point(0, TailVerticalOffset + TailHeight / 2);
            Point arcPoint3 = new Point(TailWidth, TailVerticalOffset + TailHeight);

            LineSegment as1_2 = new LineSegment(arcPoint2, false);
            LineSegment as2_3 = new LineSegment(arcPoint3, false);

            PathFigure pf = new PathFigure();
            pf.IsClosed = false;
            pf.StartPoint = arcPoint1;
            pf.Segments.Add(as1_2);
            pf.Segments.Add(as2_3);

            PathGeometry g1 = new PathGeometry();
            g1.Figures.Add(pf);

            #endregion 绘制三角形

            #region 绘制矩形边框

            RectangleGeometry g2 = new RectangleGeometry(new Rect(TailWidth, 0, this.ActualWidth - this.TailWidth, this.ActualHeight)
                , CornerRadius.TopLeft, CornerRadius.BottomRight);

            #endregion 绘制矩形边框

            #region 合并两个图形

            result.Geometry1 = g1;
            result.Geometry2 = g2;
            result.GeometryCombineMode = GeometryCombineMode.Union;

            #endregion 合并两个图形

            return result;
        }

        private Geometry CreateGeometryTailAtTop()
        {
            CombinedGeometry result = new CombinedGeometry();

            switch (this.Placement)
            {
                case EnumPlacement.TopLeft:
                    break;

                case EnumPlacement.TopCenter:
                    this.TailHorizontalOffset = (this.ActualWidth - this.TailWidth) / 2;
                    break;

                case EnumPlacement.TopRight:
                    this.TailHorizontalOffset = this.ActualWidth - this.TailWidth - this.TailHorizontalOffset;
                    break;
            }

            #region 绘制三角形

            Point anglePoint1 = new Point(this.TailHorizontalOffset, this.TailHeight);
            Point anglePoint2 = new Point(this.TailHorizontalOffset + (this.TailWidth / 2), 0);
            Point anglePoint3 = new Point(this.TailHorizontalOffset + this.TailWidth, this.TailHeight);

            LineSegment as1_2 = new LineSegment(anglePoint2, true);
            LineSegment as2_3 = new LineSegment(anglePoint3, true);

            PathFigure pf = new PathFigure();
            pf.IsClosed = false;
            pf.StartPoint = anglePoint1;
            pf.Segments.Add(as1_2);
            pf.Segments.Add(as2_3);

            PathGeometry g1 = new PathGeometry();
            g1.Figures.Add(pf);

            #endregion 绘制三角形

            #region 绘制矩形边框

            RectangleGeometry g2 = new RectangleGeometry(new Rect(0, this.TailHeight, this.ActualWidth, this.ActualHeight - this.TailHeight)
                , CornerRadius.TopLeft, CornerRadius.BottomRight);

            #endregion 绘制矩形边框

            #region 合并

            result.Geometry1 = g1;
            result.Geometry2 = g2;
            result.GeometryCombineMode = GeometryCombineMode.Union;

            #endregion 合并

            return result;
        }

        private Geometry CreateGeometryTailAtBottom()
        {
            CombinedGeometry result = new CombinedGeometry();

            switch (this.Placement)
            {
                case EnumPlacement.BottomLeft:
                    break;

                case EnumPlacement.BottomCenter:
                    this.TailHorizontalOffset = (this.ActualWidth - this.TailWidth) / 2;
                    break;

                case EnumPlacement.BottomRight:
                    this.TailHorizontalOffset = this.ActualWidth - this.TailWidth - this.TailHorizontalOffset;
                    break;
            }

            #region 绘制三角形

            Point anglePoint1 = new Point(this.TailHorizontalOffset, this.ActualHeight - this.TailHeight);
            Point anglePoint2 = new Point(this.TailHorizontalOffset + this.TailWidth / 2, this.ActualHeight);
            Point anglePoint3 = new Point(this.TailHorizontalOffset + this.TailWidth, this.ActualHeight - this.TailHeight);

            LineSegment as1_2 = new LineSegment(anglePoint2, true);
            LineSegment as2_3 = new LineSegment(anglePoint3, true);

            PathFigure pf = new PathFigure();
            pf.IsClosed = false;
            pf.StartPoint = anglePoint1;
            pf.Segments.Add(as1_2);
            pf.Segments.Add(as2_3);

            PathGeometry g1 = new PathGeometry();
            g1.Figures.Add(pf);

            #endregion 绘制三角形

            #region 绘制矩形边框

            RectangleGeometry g2 = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight - this.TailHeight)
                , CornerRadius.TopLeft, CornerRadius.BottomRight);

            #endregion 绘制矩形边框

            #region 合并

            result.Geometry1 = g1;
            result.Geometry2 = g2;
            result.GeometryCombineMode = GeometryCombineMode.Union;

            #endregion 合并

            return result;
        }

        private Brush CreateFillBrush()
        {
            Brush result = null;

            GradientStopCollection gsc = new GradientStopCollection();
            gsc.Add(new GradientStop(((SolidColorBrush)this.Background).Color, 0));
            LinearGradientBrush backGroundBrush = new LinearGradientBrush(gsc, new Point(0, 0), new Point(0, 1));
            result = backGroundBrush;

            return result;
        }

        /// <summary>
        /// 根据三角形方向设置消息框的水平位置，偏左还是偏右
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        public static void OnDirectionPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AngleBorder angleBorder = d as AngleBorder;
            if (angleBorder != null)
            {
                switch ((EnumPlacement)e.NewValue)
                {
                    case EnumPlacement.LeftTop:
                    case EnumPlacement.LeftBottom:
                    case EnumPlacement.LeftCenter:
                    case EnumPlacement.RightTop:
                    case EnumPlacement.RightBottom:
                    case EnumPlacement.RightCenter:
                        angleBorder.TailWidth = 6;
                        angleBorder.TailHeight = 12;
                        break;

                    case EnumPlacement.TopLeft:
                    case EnumPlacement.TopCenter:
                    case EnumPlacement.TopRight:
                    case EnumPlacement.BottomLeft:
                    case EnumPlacement.BottomCenter:
                    case EnumPlacement.BottomRight:
                        angleBorder.TailWidth = 12;
                        angleBorder.TailHeight = 6;
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion 私有方法
    }

    public class PopTip : Popup
    {
        #region private fields

        private bool mIsLoaded = false;
        private AngleBorder angleBorder;

        #endregion private fields

        #region DependencyProperty

        #region PlacementEx

        public EnumPlacement PlacementEx
        {
            get { return (EnumPlacement)GetValue(PlacementExProperty); }
            set { SetValue(PlacementExProperty, value); }
        }

        public static readonly DependencyProperty PlacementExProperty =
            DependencyProperty.Register("PlacementEx", typeof(EnumPlacement), typeof(PopTip)
                , new PropertyMetadata(EnumPlacement.TopLeft, PlacementExChangedCallback));

        private static void PlacementExChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopTip poptip = d as PopTip;
            if (poptip != null)
            {
                EnumPlacement placement = (EnumPlacement)e.NewValue;
                switch (placement)
                {
                    case EnumPlacement.LeftTop:
                        break;

                    case EnumPlacement.LeftBottom:
                        break;

                    case EnumPlacement.LeftCenter:
                        break;

                    case EnumPlacement.RightTop:
                        break;

                    case EnumPlacement.RightBottom:
                        break;

                    case EnumPlacement.RightCenter:
                        break;

                    case EnumPlacement.TopLeft:
                        break;

                    case EnumPlacement.TopCenter:
                        poptip.Placement = PlacementMode.Top;
                        break;

                    case EnumPlacement.TopRight:
                        break;

                    case EnumPlacement.BottomLeft:
                        break;

                    case EnumPlacement.BottomCenter:
                        break;

                    case EnumPlacement.BottomRight:
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion PlacementEx

        #region Background

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(PopTip), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(109, 129, 154))));

        #endregion Background

        #region BorderThickness

        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(PopTip), new PropertyMetadata(new Thickness(1)));

        #endregion BorderThickness

        #region BorderBrush

        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(PopTip), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(204, 206, 219))));

        #endregion BorderBrush

        #region CornerRadius

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(PopTip), new PropertyMetadata(new CornerRadius(5)));

        #endregion CornerRadius

        #endregion DependencyProperty

        #region Override

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.AllowsTransparency = true;
            //this.StaysOpen = false;

            UIElement element = this.Child;
            this.Child = null;

            Grid root = new Grid()
            {
                Margin = new Thickness(10),
            };

            angleBorder = new AngleBorder()
            {
                Background = this.Background,
                CornerRadius = this.CornerRadius,
                BorderThickness = this.BorderThickness,
                BorderBrush = this.BorderBrush,
            };
            switch (this.PlacementEx)
            {
                case EnumPlacement.LeftTop:
                    angleBorder.Placement = EnumPlacement.RightTop;
                    break;

                case EnumPlacement.LeftBottom:
                    angleBorder.Placement = EnumPlacement.RightBottom;
                    break;

                case EnumPlacement.LeftCenter:
                    angleBorder.Placement = EnumPlacement.RightCenter;
                    break;

                case EnumPlacement.RightTop:
                    angleBorder.Placement = EnumPlacement.LeftTop;
                    break;

                case EnumPlacement.RightBottom:
                    angleBorder.Placement = EnumPlacement.LeftBottom;
                    break;

                case EnumPlacement.RightCenter:
                    angleBorder.Placement = EnumPlacement.LeftCenter;
                    break;

                case EnumPlacement.TopLeft:
                    angleBorder.Placement = EnumPlacement.BottomLeft;
                    break;

                case EnumPlacement.TopCenter:
                    angleBorder.Placement = EnumPlacement.BottomCenter;
                    break;

                case EnumPlacement.TopRight:
                    angleBorder.Placement = EnumPlacement.BottomRight;
                    break;

                case EnumPlacement.BottomLeft:
                    angleBorder.Placement = EnumPlacement.TopLeft;
                    break;

                case EnumPlacement.BottomCenter:
                    angleBorder.Placement = EnumPlacement.TopCenter;
                    break;

                case EnumPlacement.BottomRight:
                    angleBorder.Placement = EnumPlacement.TopRight;
                    break;

                default:
                    break;
            }

            //在原有控件基础上，最外层套一个AngleBorder
            angleBorder.Child = element;

            root.Children.Add(angleBorder);

            this.Child = root;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (this.mIsLoaded)
            {
                return;
            }

            FrameworkElement targetElement = this.PlacementTarget as FrameworkElement;
            FrameworkElement child = this.Child as FrameworkElement;

            if (targetElement == null || child == null) return;

            switch (this.PlacementEx)
            {
                case EnumPlacement.LeftTop:
                    this.Placement = PlacementMode.Left;
                    break;

                case EnumPlacement.LeftBottom:
                    this.Placement = PlacementMode.Left;
                    this.VerticalOffset = targetElement.ActualHeight - child.ActualHeight;
                    break;

                case EnumPlacement.LeftCenter:
                    this.Placement = PlacementMode.Left;
                    this.VerticalOffset = this.GetOffset(targetElement.ActualHeight, child.ActualHeight);
                    break;

                case EnumPlacement.RightTop:
                    this.Placement = PlacementMode.Right;
                    break;

                case EnumPlacement.RightBottom:
                    this.Placement = PlacementMode.Right;
                    this.VerticalOffset = targetElement.ActualHeight - child.ActualHeight;
                    break;

                case EnumPlacement.RightCenter:
                    this.Placement = PlacementMode.Right;
                    this.VerticalOffset = this.GetOffset(targetElement.ActualHeight, child.ActualHeight);
                    break;

                case EnumPlacement.TopLeft:
                    this.Placement = PlacementMode.Top;
                    break;

                case EnumPlacement.TopCenter:
                    this.Placement = PlacementMode.Top;
                    this.HorizontalOffset = this.GetOffset(targetElement.ActualWidth, child.ActualWidth);
                    break;

                case EnumPlacement.TopRight:
                    this.Placement = PlacementMode.Top;
                    this.HorizontalOffset = targetElement.ActualWidth - child.ActualWidth;
                    break;

                case EnumPlacement.BottomLeft:
                    this.Placement = PlacementMode.Bottom;
                    break;

                case EnumPlacement.BottomCenter:
                    this.Placement = PlacementMode.Bottom;
                    this.HorizontalOffset = this.GetOffset(targetElement.ActualWidth, child.ActualWidth);
                    break;

                case EnumPlacement.BottomRight:
                    this.Placement = PlacementMode.Bottom;
                    this.HorizontalOffset = targetElement.ActualWidth - child.ActualWidth;
                    break;
            }
            this.mIsLoaded = true;
        }

        #endregion Override

        #region private function

        private double GetOffset(double targetSize, double poptipSize)
        {
            if (double.IsNaN(targetSize) || double.IsNaN(poptipSize))
            {
                return 0;
            }
            return (targetSize / 2.0) - (poptipSize / 2.0);
        }

        #endregion private function
    }
}