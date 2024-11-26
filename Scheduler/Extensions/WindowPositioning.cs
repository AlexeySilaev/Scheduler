using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Scheduler.Extensions
{
    internal static class WindowPositioning
    {
        private static NativeMethods.MONITORINFO _primaryMonitor;

        public static void SetCentered(this Window window)
        {
            Size winSize = GetElementPixelSize(window);
            window.Left = _primaryMonitor.rcWork.left + (_primaryMonitor.rcWork.Size.Width - winSize.Width) / 2;
            window.Top = _primaryMonitor.rcWork.top + (_primaryMonitor.rcWork.Size.Height - winSize.Height) / 2;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        public static void SetRightBottom(this Window window)
        {
            Size winSize = GetElementPixelSize(window);
            window.Left = _primaryMonitor.rcWork.left + _primaryMonitor.rcWork.Size.Width - winSize.Width;
            window.Top = _primaryMonitor.rcWork.top + _primaryMonitor.rcWork.Size.Height - winSize.Height;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        public static void DefinePrimaryMonitor()
        {
            var window = Application.Current.MainWindow;
            var hwnd = new WindowInteropHelper(window).EnsureHandle();
            var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            NativeMethods.MONITORINFO info = new NativeMethods.MONITORINFO();
            NativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);
            _primaryMonitor = info;
        }

        public static Size GetElementPixelSize(FrameworkElement element)
        {
            Matrix matrix;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            return new Size
            {
                Width = (int)(matrix.M11 * element.Width),
                Height = (int)(matrix.M22 * element.Height)
            };
        }

        static class NativeMethods
        {
            public const Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;

            [DllImport("user32.dll")]
            public static extern IntPtr MonitorFromWindow(IntPtr handle, Int32 flags);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern bool GetMonitorInfo(HandleRef hmonitor, MONITORINFO info);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
            public class MONITORINFO
            {
                internal int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                internal RECT rcMonitor = new RECT();
                internal RECT rcWork = new RECT();
                internal int dwFlags = 0;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;

                public RECT(int left, int top, int right, int bottom)
                {
                    this.left = left;
                    this.top = top;
                    this.right = right;
                    this.bottom = bottom;
                }

                public Size Size => new Size(right - left, bottom - top);
            }
        }
    }
}
