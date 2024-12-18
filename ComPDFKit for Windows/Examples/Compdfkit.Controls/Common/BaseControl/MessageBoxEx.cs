using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ComPDFKit.Controls.Helper;

namespace ComPDFKit.Controls.Common
{
    public class MessageBoxEx
    {
        private static string[] okstring = new string[] { LanguageHelper.CompressManager.GetString("Main_Ok")};
        private static string[] ok = okstring;

        private static string[] okcancelstring = new string[] { LanguageHelper.CompressManager.GetString("Main_Ok") , LanguageHelper.CompressManager.GetString("Main_Cancel")};
        private static string[] okcancel = okcancelstring;

        private static string[] abortretryignorestring = new string[] { LanguageHelper.CompressManager.GetString("Main_MenuHelp_About") , "Retry", "Ignore" };
        private static string[] abortretryignore = abortretryignorestring;

        private static string[] yesnocancelstring = new string[] { LanguageHelper.CompressManager.GetString("Main_Yes"), LanguageHelper.CompressManager.GetString("Main_No"), LanguageHelper.CompressManager.GetString("Main_Cancel") };
        private static string[] yesnocancel = yesnocancelstring;

        private static string[] yesnostring = new string[] { LanguageHelper.CompressManager.GetString("Main_Yes"), LanguageHelper.CompressManager.GetString("Main_No") };
        private static string[] yesno = yesnostring;

        private static string[] retrycancelstring = new string[] { "Retry", LanguageHelper.CompressManager.GetString("Main_Cancel") };
        private static string[] retrycancel = retrycancelstring;

        public static DialogResult Show(string text,string[] buttonTitles = null)
        {
            if(buttonTitles!=null&&buttonTitles.Length==1)
            {
                ok = buttonTitles;
            }
            else
            {
                ok = okstring;
            }

            myProc = new HookProc(OK);
            SetHook();
            DialogResult result = MessageBox.Show(text, Application.ProductName,MessageBoxButtons.OK);
            
            UnHook();

            return result;
        }

        public static DialogResult Show(string text,string title,string[] buttonTitles = null)
        {
            if (buttonTitles != null && buttonTitles.Length == 1)
            {
                ok = buttonTitles;
            }
            else
            {
                ok = okstring;
            }

            myProc = new HookProc(OK);
            SetHook();
            DialogResult result = MessageBox.Show(text,title);
            UnHook();

            return result;
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, string[] buttonTitles=null)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    if (buttonTitles != null && buttonTitles.Length == 1)
                    {
                        ok = buttonTitles;
                    }
                    else
                    {
                        ok = okstring;
                    }
                    myProc = new HookProc(OK);
                    break;
                case MessageBoxButtons.OKCancel:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        okcancel = buttonTitles;
                    }
                    else
                    {
                        okcancel = okcancelstring;
                    }
                    myProc = new HookProc(OKCancel);
                break;
                case MessageBoxButtons.AbortRetryIgnore:
                    if (buttonTitles != null && buttonTitles.Length == 3)
                    {
                        abortretryignore = buttonTitles;
                    }
                    else
                    {
                        abortretryignore = abortretryignorestring;
                    }
                    myProc = new HookProc(AbortRetryIgnore);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    if (buttonTitles != null && buttonTitles.Length == 3)
                    {
                        yesnocancel = buttonTitles;
                    }
                    else
                    {
                        yesnocancel = yesnocancelstring;
                    }
                    myProc = new HookProc(YesNoCancel);
                    break;
                case MessageBoxButtons.YesNo:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        yesno = buttonTitles;
                    }
                    else
                    {
                        yesno = yesnostring;
                    }
                    myProc = new HookProc(YesNo);
                    break;
                case MessageBoxButtons.RetryCancel:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        retrycancel = buttonTitles;
                    }
                    else
                    {
                        retrycancel = retrycancelstring;
                    }
                    myProc = new HookProc(RetryCancel);
                    break;
                default:
                    break;
            }

            SetHook();
            DialogResult result = MessageBox.Show(text, string.IsNullOrEmpty(caption)?Application.ProductName:caption, buttons);
            UnHook();
            return result;
        }

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons,MessageBoxIcon icons,string[] buttonTitles = null)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    if (buttonTitles != null && buttonTitles.Length == 1)
                    {
                        ok = buttonTitles;
                    }
                    else
                    {
                        ok = okstring;
                    }
                    myProc = new HookProc(OK);
                    break;
                case MessageBoxButtons.OKCancel:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        okcancel = buttonTitles;
                    }
                    else
                    {
                        okcancel = okcancelstring;
                    }
                    myProc = new HookProc(OKCancel);
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    if (buttonTitles != null && buttonTitles.Length == 3)
                    {
                        abortretryignore = buttonTitles;
                    }
                    else
                    {
                        abortretryignore = abortretryignorestring;
                    }
                    myProc = new HookProc(AbortRetryIgnore);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    if (buttonTitles != null && buttonTitles.Length == 3)
                    {
                        yesnocancel = buttonTitles;
                    }
                    else
                    {
                        yesnocancel = yesnocancelstring;
                    }
                    myProc = new HookProc(YesNoCancel);
                    break;
                case MessageBoxButtons.YesNo:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        yesno = buttonTitles;
                    }
                    else
                    {
                        yesno = yesnostring;
                    }
                    myProc = new HookProc(YesNo);
                    break;
                case MessageBoxButtons.RetryCancel:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        retrycancel = buttonTitles;
                    }
                    else
                    {
                        retrycancel = retrycancelstring;
                    }
                    myProc = new HookProc(RetryCancel);
                    break;
                default:
                    break;
            }
            SetHook();
            DialogResult result = MessageBox.Show(text, string.IsNullOrEmpty(caption) ? Application.ProductName : caption, buttons, icons);
            UnHook();
            return result;
        }
        
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, string[] buttonTitles=null)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    if (buttonTitles != null && buttonTitles.Length == 1)
                    {
                        ok = buttonTitles;
                    }
                    else
                    {
                        ok = okstring;
                    }
                    myProc = new HookProc(OK);
                    break;
                case MessageBoxButtons.OKCancel:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        okcancel = buttonTitles;
                    }
                    else
                    {
                        okcancel = okcancelstring;
                    }
                    myProc = new HookProc(OKCancel);
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    if (buttonTitles != null && buttonTitles.Length == 3)
                    {
                        abortretryignore = buttonTitles;
                    }
                    else
                    {
                        abortretryignore = abortretryignorestring;
                    }
                    myProc = new HookProc(AbortRetryIgnore);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    if (buttonTitles != null && buttonTitles.Length == 3)
                    {
                        yesnocancel = buttonTitles;
                    }
                    else
                    {
                        yesnocancel = yesnocancelstring;
                    }
                    myProc = new HookProc(YesNoCancel);
                    break;
                case MessageBoxButtons.YesNo:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        yesno = buttonTitles;
                    }
                    else
                    {
                        yesno = yesnostring;
                    }
                    myProc = new HookProc(YesNo);
                    break;
                case MessageBoxButtons.RetryCancel:
                    if (buttonTitles != null && buttonTitles.Length == 2)
                    {
                        retrycancel = buttonTitles;
                    }
                    else
                    {
                        retrycancel = retrycancelstring;
                    }
                    myProc = new HookProc(RetryCancel);
                    break;
                default:
                    break;
            }

            DialogResult result = MessageBox.Show(text, string.IsNullOrEmpty(caption) ? Application.ProductName : caption, buttons, icon, defaultButton);

            return result;
        }

        public enum HookType
        {
            Keyboard = 2,
            CBT = 5,
            Mouse = 7, 
        };

        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();
        [DllImport("user32.dll")]
        static extern int GetDlgItem(IntPtr hDlg, int nIDDlgItem);
        [DllImport("user32", EntryPoint = "SetDlgItemText")]
        static extern int SetDlgItemTextA(IntPtr hDlg, int nIDDlgItem, string lpString);

        [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);
        [DllImport("user32.dll")]
        static extern void UnhookWindowsHookEx(IntPtr handle);
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, [MarshalAs(UnmanagedType.FunctionPtr)] HookProc lpfn, IntPtr hInstance, int threadID);
        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr handle, int code, IntPtr wparam, IntPtr lparam);


        static IntPtr _nextHookPtr;
        ////must be global, or it will be Collected by GC, then no callback func can be used for the Hook
        static HookProc myProc = new HookProc(MyHookProc);

        private delegate IntPtr HookProc(int code, IntPtr wparam, IntPtr lparam);

        private static IntPtr OK(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                hChildWnd = wparam;

                var index = (IntPtr)GetDlgItem(hChildWnd,1);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index, ok[0]);
                }
            }
            else
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);

            return IntPtr.Zero;
        }

        private static IntPtr OKCancel(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;
            bool result = false;
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                hChildWnd = wparam;

                IntPtr index = (IntPtr)GetDlgItem(hChildWnd, 1);
                if (index != IntPtr.Zero)
                {
                   result = SetWindowText(index, okcancel[0]);
                }
                index = (IntPtr)GetDlgItem(hChildWnd, 2);
                if (index != IntPtr.Zero)
                {
                    result = SetWindowText(index, okcancel[1]);
                }
            }
            else
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);

            return IntPtr.Zero;
        }

        private static IntPtr RetryCancel(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                hChildWnd = wparam;

                var index = (IntPtr)GetDlgItem(hChildWnd,4);
                if (index!=IntPtr.Zero)
                {
                    SetWindowText(index, retrycancel[0]);
                }
                index = (IntPtr)GetDlgItem(hChildWnd, 2);
                if (GetDlgItem(hChildWnd, 2) != 0)
                {
                    SetWindowText(index, retrycancel[1]);
                }
            }
            else
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);
            return IntPtr.Zero;
        }

        private static IntPtr YesNo(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                hChildWnd = wparam;

                var index = (IntPtr)GetDlgItem(hChildWnd, 6);
                if (index!=IntPtr.Zero)
                {
                    SetWindowText(index, yesno[0]);
                }
                index = (IntPtr)GetDlgItem(hChildWnd, 7);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index, yesno[1]);
                }
            }
            else
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);
            return IntPtr.Zero;
        }
        private static IntPtr YesNoCancel(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                hChildWnd = wparam;

                var index = (IntPtr)GetDlgItem(hChildWnd, 6);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index,yesnocancel[0]);
                }

                index = (IntPtr)GetDlgItem(hChildWnd, 7);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index, yesnocancel[1]);
                }

                index = (IntPtr)GetDlgItem(hChildWnd, 2);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index, yesnocancel[2]);
                }
            }
            else
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);
            return IntPtr.Zero;
        }

        private static IntPtr AbortRetryIgnore(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                hChildWnd = wparam;

                var index = (IntPtr)GetDlgItem(hChildWnd, 3);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index, abortretryignore[0]);
                }

                index = (IntPtr)GetDlgItem(hChildWnd, 4);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index, abortretryignore[1]);
                }

                index = (IntPtr)GetDlgItem(hChildWnd, 5);
                if (index != IntPtr.Zero)
                {
                    SetWindowText(index, abortretryignore[2]);
                }
            }
            else
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);
            return IntPtr.Zero;
        }

        private static IntPtr MyHookProc(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;// msgbox is "child"
            // notification that a window is about to be activated
            // window handle is wParam
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                // set window handles of messagebox
                hChildWnd = wparam;
                //to get the text of yes button

                for(int i=0;i<21;i++)
                {
                    if (GetDlgItem(hChildWnd, i) != 0)
                        SetDlgItemTextA(hChildWnd,i,string.Format("Item {0}",i));
                }
            }
            else
            {
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);// otherwise, continue with any possible chained hooks
            }

            return IntPtr.Zero;
        }

        public static void SetHook()
        {
            try
            {
                if (_nextHookPtr != IntPtr.Zero)//Hooked already
                {
                    return;
                }
                _nextHookPtr = SetWindowsHookEx((int)HookType.CBT, myProc, IntPtr.Zero, GetCurrentThreadId());
            }
            catch { }
        }

        public static  void UnHook()
        {
            if (_nextHookPtr != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_nextHookPtr);
                _nextHookPtr = IntPtr.Zero;
            }
        }
    }
}
