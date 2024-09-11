using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Printing;
using System.Drawing.Printing;
using System.Windows;
using ComPDFKit.PDFDocument;

namespace ComPDFKit.Controls
{ 
    public enum DuplexStage
    {
        None,
        FrontSide,
        BackSide
    }

    public enum DuplexPrintMod
    {
        None,
        FlipLongEdge,
        FlipShortEdge
    }

    public enum PrintMod
    {
        Size,
        Poster,
        Multiple,
        Booklet
    }

    public enum DisplayPageNumber
    {
        Two,
        Four,
        Six,
        Nine,
        Sixteen,
        Customized
    }

    public enum PageOrder
    {
        Horizontal,
        HorizontalReverse,
        Vertical,
        VerticalReverse,
    }

    public enum SizeType
    {
        Adaptive,
        Actural,
        Customized
    }

    public enum BookletBinding
    {
        Left,
        Right,
        LeftTall,
        RightTall
    }

    public enum BookletSubset
    {
        BothSides,
        FrontSideOnly,
        BackSideOnly
    }

    public class PrinterModel
    {
    }

    public class PrintSettingsInfo
    {
        public CPDFDocument Document;
        public bool IsGrayscale { get; set; } = false;
        public bool IsReverseOrder { get; set; } = false;
        public bool NeedRerendering { get; set; } = true;
        public bool IsPaperSizeChanged { get; set; } = false;
        public bool NeedReversePage { get; set; } = false;
        public bool IsPrintAnnot { get; set; } = true;
        public bool IsPrintForm { get; set; } = true;
        public bool IsBorderless { get; set; } = false;

        public List<int> TargetPaperList = new List<int>();

        public PageMargins PageMargins { get; set; } = new PageMargins() { Top = 0, Bottom = 0, Left = 0, Right = 0 };

        public int Copies { get; set; } = 1;

        public string PrinterName { get; set; } = string.Empty;

        public DuplexPrintMod DuplexPrintMod { get; set; } = DuplexPrintMod.None;
        public PageOrientation PrintOrientation { get; set; } = PageOrientation.Portrait;
        public Rect PageBound { get; set; } = new Rect();
        public List<int> PageRangeList = new List<int>();
        public PrintDocument PrintDocument { get; set; } = new PrintDocument();
        public Thickness Margins { get; set; } = new Thickness() { Bottom = 0, Left = 0, Right = 0, Top = 0 };
        public PaperSize PaperSize { get; set; } = null;
        public double ActualHeight { get => PaperSize.Height - Margins.Bottom - Margins.Top; }
        public double ActualWidth { get => PaperSize.Width - Margins.Left - Margins.Right; }
        public PrintMode PrintMode { get; set; } = null;
    }

    public abstract class PrintMode { }

    public class SizeModeInfo : PrintMode
    {
        public SizeType SizeType { get; set; } = SizeType.Adaptive;
        public int Scale { get; set; } = 100;
    }

    public class PosterModeInfo : PrintMode
    {
        public bool HasCutmarks { get; set; } = false;
        public bool HasLabel { get; set; } = false;
        public double OverLap { get; set; } = 0;
        public int TileRatio { get; set; } = 100;
        public string Label { get; set; } = string.Empty;
    }

    public class MultipleModeInfo : PrintMode
    {
        public PageOrder PageOrder { get; set; } = PageOrder.Horizontal;
        public SheetPair Sheet { get; set; } = new SheetPair(2, 1);
        public bool IsAutoRotate { get; set; } = false;
        public bool IsPrintBorder { get; set; } = false;
        public class SheetPair : IEquatable<SheetPair>
        {
            public int HorizontalPageNumber { get; set; }
            public int VerticalPageNumber { get; set; }
            public int TotalPageNumber
            {
                get => HorizontalPageNumber * VerticalPageNumber;
            }

            public SheetPair(int horizontalPageNumber, int verticalPageNumber)
            {
                HorizontalPageNumber = horizontalPageNumber;
                VerticalPageNumber = verticalPageNumber;
            }

            public bool Equals(SheetPair other)
            {
                return HorizontalPageNumber == other.HorizontalPageNumber && VerticalPageNumber == other.VerticalPageNumber;
            }
        }
    }

    public class BookletModeInfo : PrintMode
    {
        public BookletBinding BookletBinding { get; set; } = BookletBinding.Left;
        public BookletSubset Subset { get; set; } = BookletSubset.BothSides;
        public int BeginPageIndex { get; set; }
        public int EndPageIndex { get; set; }
        public bool IsAutoRotate { get; set; } = false;
    }
    public struct PageMargins
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
    }
}
