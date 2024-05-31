using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ComPDFKit.Tool.Help
{
    internal class ImportWin32
    {
        private const string ImeDll = "imm32.dll";
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct CompositionForm
        {
            public int dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }
        [DllImport(ImeDll)]
        internal static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport(ImeDll)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        [DllImport(ImeDll)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref CompositionForm form);
        [DllImport(ImeDll)]
        internal static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);
    }
}
